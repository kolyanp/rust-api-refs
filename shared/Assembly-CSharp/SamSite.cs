using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SamSite : ContainerIOEntity
{
	public interface ISamSiteTarget
	{
		static List<ISamSiteTarget> serverList;

		SamTargetType SAMTargetType { get; }

		bool isClient { get; }

		bool IsValidSAMTarget(bool staticRespawn);

		Vector3 CenterPoint();

		Vector3 GetWorldVelocity();

		bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity);

		static ISamSiteTarget()
		{
			serverList = new List<ISamSiteTarget>();
		}
	}

	public class SamTargetType
	{
		public readonly float scanRadius;

		public readonly float speedMultiplier;

		public readonly float timeBetweenBursts;

		public SamTargetType(float scanRadius, float speedMultiplier, float timeBetweenBursts)
		{
			this.scanRadius = scanRadius;
			this.speedMultiplier = speedMultiplier;
			this.timeBetweenBursts = timeBetweenBursts;
		}
	}

	[Header("SAM Site")]
	public Animator pitchAnimator;

	public GameObject yaw;

	public GameObject pitch;

	public GameObject gear;

	public Transform eyePoint;

	public float gearEpislonDegrees;

	public float turnSpeed;

	public float clientLerpSpeed;

	public Vector3 currentAimDir;

	public Vector3 targetAimDir;

	public float vehicleScanRadius;

	public float missileScanRadius;

	public GameObjectRef projectileTest;

	public GameObjectRef muzzleFlashTest;

	public bool staticRespawn;

	public ItemDefinition ammoType;

	public Transform[] tubes;

	[ServerVar(Help = "how long until static sam sites auto repair")]
	public static float staticrepairseconds = 1200f;

	[ServerVar(Help = "Delay before SAM sites that haven't shot a target will auto-reload")]
	public static float autoreloaddelay = 45f;

	public SoundDefinition yawMovementLoopDef;

	public float yawGainLerp;

	public float yawGainMovementSpeedMult;

	public SoundDefinition pitchMovementLoopDef;

	public float pitchGainLerp;

	public float pitchGainMovementSpeedMult;

	public int lowAmmoThreshold;

	public Flags Flag_TargetMode;

	public Flags Flag_ManuallySetMode;

	public static SamTargetType targetTypeUnknown;

	public static SamTargetType targetTypeVehicle;

	public static SamTargetType targetTypeMissile;

	public ISamSiteTarget currentTarget;

	public SamTargetType mostRecentTargetType;

	public Item ammoItem;

	public float lockOnTime;

	public float lastTargetVisibleTime;

	public int lastAmmoCount;

	public int currentTubeIndex;

	public int firedCount;

	private float reloadVolleyFinishTime;

	private TimeSince sinceLastRocketFired;

	private Action WeaponTickCB;

	private int input1Amount;

	public override bool ValidateMeleeColliderAntihack => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SamSite.OnRpcMessage"))
		{
			if (rpc == 3160662357u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ToggleDefenderMode"));
				}
				using (TimeWarning.New("ToggleDefenderMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3160662357u, "ToggleDefenderMode", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3160662357u, "ToggleDefenderMode", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ToggleDefenderMode(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ToggleDefenderMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool IsPowered()
	{
		if (!staticRespawn)
		{
			return HasFlag(Flags.Reserved8);
		}
		return true;
	}

	public override int ConsumptionAmount()
	{
		return 25;
	}

	public bool IsInDefenderMode()
	{
		return HasFlag(Flag_TargetMode);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public void SetTarget(ISamSiteTarget target)
	{
		bool num = currentTarget != target;
		currentTarget = target;
		if (!ObjectEx.IsUnityNull(target))
		{
			mostRecentTargetType = target.SAMTargetType;
		}
		if (num)
		{
			MarkIODirty();
		}
	}

	public void MarkIODirty()
	{
		if (!staticRespawn)
		{
			lastPassthroughEnergy = -1;
			MarkDirtyForceUpdateOutputs();
		}
	}

	public void ClearTarget()
	{
		SetTarget(null);
	}

	public override void ServerInit()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		targetTypeUnknown = new SamTargetType(vehicleScanRadius, 1f, 5f);
		targetTypeVehicle = new SamTargetType(vehicleScanRadius, 1f, 5f);
		targetTypeMissile = new SamTargetType(missileScanRadius, 2.25f, 3.5f);
		mostRecentTargetType = targetTypeUnknown;
		ClearTarget();
		InvokeRandomized(TargetScan, 1f, 3f, 0.2f);
		currentAimDir = ((Component)this).transform.forward;
		if (base.inventory != null && !staticRespawn)
		{
			base.inventory.onItemAddedRemoved = OnItemAddedRemoved;
		}
	}

	public void OnItemAddedRemoved(Item arg1, bool arg2)
	{
		EnsureAmmoLoaded();
		if (IsPowered())
		{
			MarkIODirty();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.samSite = Pool.Get<SAMSite>();
		info.msg.samSite.aimDir = GetAimDir();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (staticRespawn && HasFlag(Flags.Reserved1))
		{
			Invoke(SelfHeal, staticrepairseconds);
		}
	}

	public void SelfHeal()
	{
		lifestate = LifeState.Alive;
		base.health = startHealth;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void Die(HitInfo info = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (staticRespawn)
		{
			ClearTarget();
			Quaternion val = Quaternion.LookRotation(currentAimDir, Vector3.up);
			val = Quaternion.Euler(0f, ((Quaternion)(ref val)).eulerAngles.y, 0f);
			currentAimDir = val * Vector3.forward;
			Invoke(SelfHeal, staticrepairseconds);
			lifestate = LifeState.Dead;
			base.health = 0f;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
			return;
		}
		base.Die(info);
	}

	public void FixedUpdate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = currentAimDir;
		if (!ObjectEx.IsUnityNull(currentTarget) && IsPowered())
		{
			float num = projectileTest.Get().GetComponent<ServerProjectile>().speed * currentTarget.SAMTargetType.speedMultiplier;
			Vector3 val2 = currentTarget.CenterPoint();
			float num2 = Vector3.Distance(val2, ((Component)eyePoint).transform.position);
			float num3 = num2 / num;
			Vector3 val3 = val2 + currentTarget.GetWorldVelocity() * num3;
			num3 = Vector3.Distance(val3, ((Component)eyePoint).transform.position) / num;
			val3 = val2 + currentTarget.GetWorldVelocity() * num3;
			Vector3 val4 = currentTarget.GetWorldVelocity();
			if (((Vector3)(ref val4)).magnitude > 0.1f)
			{
				float num4 = Mathf.Sin(Time.time * 3f) * (1f + num3 * 0.5f);
				Vector3 val5 = val3;
				val4 = currentTarget.GetWorldVelocity();
				val3 = val5 + ((Vector3)(ref val4)).normalized * num4;
			}
			val4 = val3 - ((Component)eyePoint).transform.position;
			currentAimDir = ((Vector3)(ref val4)).normalized;
			if (num2 > currentTarget.SAMTargetType.scanRadius)
			{
				ClearTarget();
			}
		}
		Quaternion val6 = Quaternion.LookRotation(currentAimDir, ((Component)this).transform.up);
		Vector3 eulerAngles = ((Quaternion)(ref val6)).eulerAngles;
		eulerAngles = BaseMountable.ConvertVector(eulerAngles);
		float num5 = Mathf.InverseLerp(0f, 90f, 0f - eulerAngles.x);
		float num6 = Mathf.Lerp(15f, -75f, num5);
		Quaternion localRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
		yaw.transform.localRotation = localRotation;
		Quaternion localRotation2 = pitch.transform.localRotation;
		float x = ((Quaternion)(ref localRotation2)).eulerAngles.x;
		localRotation2 = pitch.transform.localRotation;
		Quaternion localRotation3 = Quaternion.Euler(x, ((Quaternion)(ref localRotation2)).eulerAngles.y, num6);
		pitch.transform.localRotation = localRotation3;
		if (currentAimDir != val)
		{
			SendNetworkUpdate();
		}
	}

	public Vector3 GetAimDir()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return currentAimDir;
	}

	public bool HasValidTarget()
	{
		return !ObjectEx.IsUnityNull(currentTarget);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasAmmo())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	private void AddTargetSet(List<ISamSiteTarget> allTargets, float scanRadius)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		foreach (ISamSiteTarget server in ISamSiteTarget.serverList)
		{
			if (!(server is MLRSRocket) && Vector3.Distance(server.CenterPoint(), ((Component)eyePoint).transform.position) < scanRadius)
			{
				allTargets.Add(server);
			}
		}
	}

	private void AddMLRSRockets(List<ISamSiteTarget> allTargets, float scanRadius)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (MLRSRocket.serverList.Count == 0)
		{
			return;
		}
		foreach (MLRSRocket server in MLRSRocket.serverList)
		{
			if (Vector3.Distance(((Component)server).transform.position, ((Component)this).transform.position) < scanRadius)
			{
				allTargets.Add(server);
			}
		}
	}

	public void TargetScan()
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPowered())
		{
			lastTargetVisibleTime = 0f;
			return;
		}
		if (Time.time > lastTargetVisibleTime + 3f)
		{
			ClearTarget();
		}
		if (!staticRespawn)
		{
			int num = ((ammoItem != null && ammoItem.parent == base.inventory) ? ammoItem.amount : 0);
			bool flag = lastAmmoCount < lowAmmoThreshold;
			bool flag2 = num < lowAmmoThreshold;
			if (num != lastAmmoCount && flag != flag2)
			{
				MarkIODirty();
			}
			lastAmmoCount = num;
		}
		if (HasValidTarget() || IsDead())
		{
			return;
		}
		List<ISamSiteTarget> list = Pool.Get<List<ISamSiteTarget>>();
		if (Interface.CallHook("OnSamSiteTargetScan", this, list) == null)
		{
			if (!IsInDefenderMode())
			{
				AddTargetSet(list, targetTypeVehicle.scanRadius);
			}
			AddMLRSRockets(list, targetTypeMissile.scanRadius);
		}
		ISamSiteTarget samSiteTarget = null;
		foreach (ISamSiteTarget item in list)
		{
			if (!item.isClient && !(item.CenterPoint().y < ((Component)eyePoint).transform.position.y) && item.IsVisible(((Component)eyePoint).transform.position, item.SAMTargetType.scanRadius * 2f) && item.IsValidSAMTarget(staticRespawn) && Interface.CallHook("OnSamSiteTarget", this, item) == null)
			{
				samSiteTarget = item;
				break;
			}
		}
		if (!ObjectEx.IsUnityNull(samSiteTarget) && currentTarget != samSiteTarget)
		{
			lockOnTime = Time.time + 0.5f;
		}
		SetTarget(samSiteTarget);
		if (!ObjectEx.IsUnityNull(currentTarget))
		{
			lastTargetVisibleTime = Time.time;
		}
		Pool.FreeUnmanaged<ISamSiteTarget>(ref list);
		if (WeaponTickCB == null)
		{
			WeaponTickCB = WeaponTick;
		}
		if (ObjectEx.IsUnityNull(currentTarget))
		{
			CancelInvoke(WeaponTickCB);
		}
		else
		{
			InvokeRandomized(WeaponTickCB, 0f, 0.5f, 0.2f);
		}
	}

	public virtual bool HasAmmo()
	{
		if (!staticRespawn)
		{
			if (ammoItem != null && ammoItem.amount > 0)
			{
				return ammoItem.parent == base.inventory;
			}
			return false;
		}
		return true;
	}

	public void LoadAmmo()
	{
		if (staticRespawn)
		{
			return;
		}
		for (int i = 0; i < base.inventory.itemList.Count; i++)
		{
			Item item = base.inventory.itemList[i];
			if (item != null && item.info.itemid == ammoType.itemid && item.amount > 0)
			{
				ammoItem = item;
				return;
			}
		}
		ammoItem = null;
	}

	public void EnsureAmmoLoaded()
	{
		if (!HasAmmo())
		{
			LoadAmmo();
		}
	}

	public bool IsLoadingAmmo()
	{
		return IsInvoking(LoadAmmo);
	}

	private void ReloadVolley()
	{
		if (mostRecentTargetType == null)
		{
			mostRecentTargetType = targetTypeUnknown;
		}
		float timeBetweenBursts = mostRecentTargetType.timeBetweenBursts;
		reloadVolleyFinishTime = Time.time + timeBetweenBursts;
		firedCount = 0;
	}

	public void WeaponTick()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (IsDead() || Time.time < reloadVolleyFinishTime)
		{
			return;
		}
		if ((firedCount > 0 && TimeSince.op_Implicit(sinceLastRocketFired) > autoreloaddelay) || firedCount >= 6)
		{
			ReloadVolley();
		}
		else
		{
			if (Time.time < lockOnTime)
			{
				return;
			}
			if (!IsPowered())
			{
				firedCount = 0;
				return;
			}
			EnsureAmmoLoaded();
			if (Interface.CallHook("CanSamSiteShoot", this) == null && HasAmmo())
			{
				bool num = ammoItem != null && ammoItem.amount == lowAmmoThreshold;
				if (!staticRespawn && ammoItem != null)
				{
					ammoItem.UseItem();
				}
				firedCount++;
				sinceLastRocketFired = TimeSince.op_Implicit(0f);
				float speedMultiplier = 1f;
				if (!ObjectEx.IsUnityNull(currentTarget))
				{
					speedMultiplier = currentTarget.SAMTargetType.speedMultiplier;
				}
				FireProjectile(tubes[currentTubeIndex].position, currentAimDir, speedMultiplier);
				Effect.server.Run(muzzleFlashTest.resourcePath, this, StringPool.Get("Tube " + (currentTubeIndex + 1)), Vector3.zero, Vector3.up);
				currentTubeIndex++;
				if (currentTubeIndex >= tubes.Length)
				{
					currentTubeIndex = 0;
				}
				if (num)
				{
					MarkIODirty();
				}
			}
		}
	}

	public void FireProjectile(Vector3 origin, Vector3 direction, float speedMultiplier)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(projectileTest.resourcePath, origin, Quaternion.LookRotation(direction, Vector3.up));
		if (!((Object)(object)baseEntity == (Object)null))
		{
			baseEntity.creatorEntity = this;
			ServerProjectile component = ((Component)baseEntity).GetComponent<ServerProjectile>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.InitializeVelocity(GetInheritedProjectileVelocity(direction) + direction * component.speed * speedMultiplier);
			}
			baseEntity.Spawn();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int result = Mathf.Min(1, GetCurrentEnergy());
		switch (outputSlot)
		{
		case 0:
			if (ObjectEx.IsUnityNull(currentTarget))
			{
				return 0;
			}
			return result;
		case 1:
			if (ammoItem == null || ammoItem.amount >= lowAmmoThreshold || ammoItem.parent != base.inventory)
			{
				return 0;
			}
			return result;
		case 2:
			if (HasAmmo())
			{
				return 0;
			}
			return result;
		default:
			return GetCurrentEnergy();
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void ToggleDefenderMode(RPCMessage msg)
	{
		if (staticRespawn)
		{
			return;
		}
		BasePlayer player = msg.player;
		if ((Object)(object)player == (Object)null || !player.CanBuild())
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag == IsInDefenderMode() || Interface.CallHook("OnSamSiteModeToggle", this, player, flag) != null)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flag_ManuallySetMode, flag);
		flagsUpdateScope.Set(Flag_TargetMode, flag);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		switch (inputSlot)
		{
		case 0:
			base.UpdateFromInput(inputAmount, inputSlot);
			break;
		case 1:
			if (input1Amount != inputAmount)
			{
				bool flag = HasFlag(Flag_ManuallySetMode);
				using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flag_TargetMode, (inputAmount == 0) ? flag : (!flag));
			}
			input1Amount = inputAmount;
			break;
		}
	}

	public SamSite()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		gearEpislonDegrees = 20f;
		turnSpeed = 1f;
		clientLerpSpeed = 1f;
		currentAimDir = Vector3.forward;
		targetAimDir = Vector3.forward;
		vehicleScanRadius = 350f;
		missileScanRadius = 500f;
		yawGainLerp = 8f;
		yawGainMovementSpeedMult = 0.1f;
		pitchGainLerp = 10f;
		pitchGainMovementSpeedMult = 0.5f;
		lowAmmoThreshold = 5;
		Flag_TargetMode = Flags.Reserved9;
		Flag_ManuallySetMode = Flags.Reserved10;
		base._002Ector();
	}
}
