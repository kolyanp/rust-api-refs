using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using Rust.Safety;
using UnityEngine;

public class PTBoatTurret : BaseMountedUseable
{
	[Header("Mounted Weapon")]
	[SerializeField]
	private ItemDefinition[] weapons;

	[SerializeField]
	private Transform weaponSocket;

	[SerializeField]
	private int turretAnimationType;

	[SerializeField]
	private GameObjectRef screenshakeEffect;

	public bool useLegIK;

	[Header("AI")]
	[SerializeField]
	private float aiMaxAttackDistance = 75f;

	[SerializeField]
	private float aiAccuracy = 1f;

	[ServerVar(Help = "Seconds an NPC gunner keeps firing in each burst")]
	public static float ai_burst_length = 3f;

	[ServerVar(Help = "Seconds an NPC gunner waits between bursts")]
	public static float ai_time_between_bursts = 3f;

	[ServerVar(Help = "Minimum seconds between NPC gunner shots")]
	public static float ai_fire_rate = 0.125f;

	public const Flags Flag_Empty = Flags.Reserved5;

	public static readonly Phrase NeedAmmoPhrase = new Phrase("mountedweapon.reload.tip", "You need regular 5.56 ammo in your inventory to reload.");

	public static readonly Phrase AmmoFullPhrase = new Phrase("mountedweapon.reload.full.tip", "Can't reload. Ammo is already full!");

	private const float AiTickRate = 0.05f;

	private const float AiTurnSpeed = 105f;

	private const float AiIdleTurnSpeed = 40f;

	private const float AiMuzzleClearance = 1.5f;

	private BasePlayer aiExtraTarget;

	private BasePlayer aiLastSeenTarget;

	private Vector3 aiLastSeenPosition;

	private float aiBurstStartTime = float.NegativeInfinity;

	private float aiLastFireTime = float.NegativeInfinity;

	private Vector3 aiIdleLookDir;

	private float aiNextIdleLookTime;

	private int savedAmmoPerGun = -1;

	public ItemDefinition WeaponDef => weapons[0];

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (isServer && info.fromDisk && info.msg.mountedWeapon != null)
		{
			savedAmmoPerGun = info.msg.mountedWeapon.ammoStackSize;
		}
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInitPostNetworkGroupAssign();
		ItemDefinition[] array = weapons;
		for (int i = 0; i < array.Length; i++)
		{
			HeldEntity heldEntity = AutoTurret.TryAddWeaponToTurret(ItemManager.Create(array[i], 1, 0uL, isServerSide: true, 0uL), weaponSocket, this, 0f);
			if (!((Object)(object)heldEntity == (Object)null))
			{
				((Component)heldEntity).transform.localPosition = Vector3.zero;
				heldEntity.useOwnerForward = false;
			}
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerServerInput(inputState, player);
		Vector3 aimAngles = inputState.current.aimAngles;
		if (CanAim() && !Vector3Ex.IsNaNOrInfinity(aimAngles))
		{
			aimDir = Quaternion.Euler(aimAngles) * Vector3.forward;
			RotateGun(1000f);
		}
	}

	public override bool CanFireMountedWeapon(BasePlayer player)
	{
		if (base.CanFireMountedWeapon(player) && !player.InSafeZone())
		{
			return !AnyGunReloading();
		}
		return false;
	}

	private bool AnyGunReloading()
	{
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is BaseProjectile { IsReloadStarted: not false })
			{
				return true;
			}
		}
		return false;
	}

	public override void OnWeaponFired(BaseProjectile weapon)
	{
		base.OnWeaponFired(weapon);
		UpdateEmptyFlag();
	}

	public override void OnWeaponReloaded(BaseProjectile weapon)
	{
		base.OnWeaponReloaded(weapon);
		UpdateEmptyFlag();
	}

	private void FireGuns()
	{
		foreach (BaseEntity child in children)
		{
			if (child is BaseProjectile baseProjectile && !baseProjectile.HasAttackCooldown())
			{
				baseProjectile.ServerUse();
			}
		}
		UpdateEmptyFlag();
	}

	private void UpdateEmptyFlag()
	{
		bool flag = true;
		foreach (BaseEntity child in children)
		{
			if (child is BaseProjectile baseProjectile && baseProjectile.primaryMagazine.contents > 0)
			{
				flag = false;
			}
		}
		if (HasFlag(Flags.Reserved5) == flag)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, flag);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BaseEntity child in children)
		{
			if (child is BaseProjectile baseProjectile)
			{
				num++;
				num2 += baseProjectile.primaryMagazine.contents;
			}
		}
		if (num != 0)
		{
			info.msg.mountedWeapon = Pool.Get<MountedWeapon>();
			info.msg.mountedWeapon.ammoStackSize = num2 / num;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (savedAmmoPerGun < 0)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			if (child is BaseProjectile baseProjectile)
			{
				baseProjectile.primaryMagazine.contents = Mathf.Min(savedAmmoPerGun, baseProjectile.primaryMagazine.capacity);
				baseProjectile.SendNetworkUpdate();
			}
		}
		savedAmmoPerGun = -1;
		UpdateEmptyFlag();
	}

	public override void LightToggle(BasePlayer player)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, !IsOn());
	}

	public void SetAiExtraTarget(BasePlayer target)
	{
		aiExtraTarget = target;
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		SetGunner(GetMounted());
		if (GetMounted() is HumanNPC)
		{
			InvokeRepeating(AiTick, 0f, 0.05f);
		}
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		CancelInvoke(AiTick);
		SetGunner(null);
	}

	private void SetGunner(BasePlayer gunner)
	{
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is BaseProjectile baseProjectile)
			{
				baseProjectile.forcedOwner = gunner;
				baseProjectile.OnGunnerChanged();
				baseProjectile.SendNetworkUpdate();
			}
		}
	}

	private void AiTick()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.move || !(GetMounted() is HumanNPC humanNPC) || humanNPC.InSafeZone())
		{
			return;
		}
		BasePlayer aiTarget = GetAiTarget(humanNPC, out var canShoot);
		if ((Object)(object)aiTarget == (Object)null)
		{
			LookAround();
			return;
		}
		if (canShoot)
		{
			aiLastSeenTarget = aiTarget;
			aiLastSeenPosition = aiTarget.eyes.position;
		}
		bool flag = !canShoot && (Object)(object)aiTarget == (Object)(object)aiExtraTarget && (Object)(object)aiTarget == (Object)(object)aiLastSeenTarget;
		Vector3 val = (flag ? aiLastSeenPosition : aiTarget.eyes.position) - muzzle.position;
		if (val.sqrMagnitude > aiMaxAttackDistance * aiMaxAttackDistance)
		{
			return;
		}
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aiAccuracy, val.normalized);
		aimDir = Vector3.RotateTowards(aimDir, modifiedAimConeDirection, MathF.PI * 7f / 240f, 0f);
		if (Time.time > aiBurstStartTime + ai_burst_length + ai_time_between_bursts && (canShoot | flag))
		{
			aiBurstStartTime = Time.time;
		}
		if (Time.time < aiBurstStartTime + ai_burst_length && Time.time > aiLastFireTime + ai_fire_rate && Vector3.Dot(muzzle.forward, val.normalized) >= 0.9f)
		{
			aiLastFireTime = Time.time;
			FireGuns();
		}
		foreach (BaseEntity child in children)
		{
			if (child is BaseProjectile baseProjectile && baseProjectile.primaryMagazine.contents <= 0)
			{
				baseProjectile.ServerReload();
			}
		}
	}

	private void LookAround()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time > aiNextIdleLookTime)
		{
			aiNextIdleLookTime = Time.time + Random.Range(3f, 8f);
			aiIdleLookDir = ((Component)this).transform.rotation * Quaternion.Euler(Random.Range(-10f, 0f), Random.Range(yawClamp.x, yawClamp.y), 0f) * Vector3.forward;
		}
		aimDir = Vector3.RotateTowards(aimDir, aiIdleLookDir, MathF.PI / 90f, 0f);
	}

	private BasePlayer GetAiTarget(HumanNPC npc, out bool canShoot)
	{
		BasePlayer basePlayer = (((Object)(object)npc.Brain != (Object)null && (Object)(object)npc.Brain.Navigator != (Object)null) ? (npc.Brain.Navigator.FacingDirectionEntity as BasePlayer) : null);
		canShoot = true;
		if (CanAiShootAt(aiExtraTarget))
		{
			return aiExtraTarget;
		}
		if (CanAiShootAt(basePlayer))
		{
			return basePlayer;
		}
		canShoot = false;
		if (aiExtraTarget.IsValidAttackTarget())
		{
			return aiExtraTarget;
		}
		if (!basePlayer.IsValidAttackTarget())
		{
			return null;
		}
		return basePlayer;
	}

	public bool CanAiShootAt(BasePlayer target)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!target.IsValidAttackTarget())
		{
			return false;
		}
		Vector3 position = target.eyes.position;
		if (CanFireMountedWeaponAt(position - muzzle.position))
		{
			return GamePhysics.LineOfSight(muzzle.position, position, 1218519297, 1.5f, 0f, GetRootParentEntity());
		}
		return false;
	}

	static PTBoatTurret()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
	}
}
