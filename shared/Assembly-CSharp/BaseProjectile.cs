using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseProjectile : AttackEntity
{
	[Serializable]
	public class Magazine
	{
		[Serializable]
		public struct Definition
		{
			[Tooltip("Set to 0 to not use inbuilt mag")]
			public int builtInSize;

			[Tooltip("If using inbuilt mag, will accept these types of ammo")]
			[InspectorFlags]
			public AmmoTypes ammoTypes;
		}

		public Definition definition;

		public int capacity;

		public int contents;

		[ItemSelector]
		public ItemDefinition ammoType;

		public bool allowPlayerReloading = true;

		public bool allowAmmoSwitching = true;

		public void ServerInit()
		{
			if (definition.builtInSize > 0)
			{
				capacity = definition.builtInSize;
			}
		}

		public Magazine Save()
		{
			Magazine val = Pool.Get<Magazine>();
			if ((Object)(object)ammoType == (Object)null)
			{
				val.capacity = capacity;
				val.contents = 0;
				val.ammoType = 0;
			}
			else
			{
				val.capacity = capacity;
				val.contents = contents;
				val.ammoType = ammoType.itemid;
			}
			return val;
		}

		public void Load(Magazine mag)
		{
			contents = mag.contents;
			capacity = mag.capacity;
			ammoType = ItemManager.FindItemDefinition(mag.ammoType);
		}

		public bool CanReload(IAmmoContainer ammoSource)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if (contents >= capacity)
			{
				return false;
			}
			return ammoSource.HasAmmo(definition.ammoTypes);
		}
	}

	public enum WeaponCrosshairSettings
	{
		Standard,
		StandardWithAiming,
		StandardWithReloading,
		StandardWithAimingAndReloading,
		Always
	}

	public static class BaseProjectileFlags
	{
		public const Flags BurstToggle = Flags.Reserved6;
	}

	[Header("NPC Info")]
	public float NoiseRadius = 100f;

	[Header("Projectile")]
	public float damageScale = 1f;

	public float distanceScale = 1f;

	public float projectileVelocityScale = 1f;

	public bool automatic;

	public bool usableByTurret = true;

	[Tooltip("Final damage is scaled by this amount before being applied to a target when this weapon is mounted to a turret")]
	public float turretDamageScale = 0.35f;

	public bool largeTurretWeapon;

	public float turretReloadDurationOverride = -1f;

	[Tooltip("How far away this attack effect can be heard")]
	[Header("Effects")]
	public float maxAttackEffectDistance = 400f;

	public GameObjectRef attackFX;

	public GameObjectRef silencedAttack;

	public GameObjectRef muzzleBrakeAttack;

	public SoundDefinition fireModeSound;

	public Transform MuzzlePoint;

	[Header("Reloading")]
	public float reloadTime = 1f;

	public bool canUnloadAmmo = true;

	public Magazine primaryMagazine;

	public bool fractionalReload;

	public float reloadStartDuration;

	public float reloadFractionDuration;

	public float reloadEndDuration;

	public float alternateDryFireRate;

	public bool sendReloadSignalFromServer;

	[Header("Recoil")]
	public float aimSway = 3f;

	public float aimSwaySpeed = 1f;

	public RecoilProperties recoil;

	[Header("Aim Cone")]
	public AnimationCurve aimconeCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
	{
		new Keyframe(0f, 1f),
		new Keyframe(1f, 1f)
	});

	public float aimCone;

	public float hipAimCone = 1.8f;

	public float aimconePenaltyPerShot;

	public float aimConePenaltyMax;

	public float aimconePenaltyRecoverTime = 0.1f;

	public float aimconePenaltyRecoverDelay = 0.1f;

	public float stancePenaltyScale = 1f;

	[Header("Iconsights")]
	public bool hasADS = true;

	public bool noAimingWhileCycling;

	public bool manualCycle;

	public WeaponCrosshairSettings CrosshairSettings;

	[NonSerialized]
	protected bool needsCycle;

	[NonSerialized]
	protected bool isCycling;

	[NonSerialized]
	public bool aiming;

	[Header("Burst Information")]
	public bool isBurstWeapon;

	public bool canChangeFireModes = true;

	public bool defaultOn = true;

	public float internalBurstRecoilScale = 0.8f;

	public float internalBurstFireRateScale = 0.8f;

	public float internalBurstAimConeScale = 0.8f;

	public float resetDuration = 0.3f;

	public int numShotsFired;

	public const float maxDistance = 300f;

	[NonSerialized]
	private EncryptedValue<float> nextReloadTime = float.NegativeInfinity;

	[NonSerialized]
	private EncryptedValue<float> startReloadTime = float.NegativeInfinity;

	private float lastReloadTime = -10f;

	private bool modsChangedInitialized;

	private float stancePenalty;

	private float aimconePenalty;

	private uint cachedModHash;

	private float sightAimConeScale = 1f;

	private float sightAimConeOffset;

	private float hipAimConeScale = 1f;

	private float hipAimConeOffset;

	protected bool reloadStarted;

	protected bool reloadFinished;

	private int fractionalInsertCounter;

	private static readonly Effect reusableInstance = new Effect();

	public RecoilProperties recoilProperties
	{
		get
		{
			if (!((Object)(object)recoil == (Object)null))
			{
				return recoil.GetRecoil();
			}
			return null;
		}
	}

	public bool isSemiAuto => !automatic;

	public override Transform MuzzleTransform => MuzzlePoint;

	public override bool IsUsableByTurret => usableByTurret;

	protected virtual bool CanRefundAmmo => true;

	protected virtual ItemDefinition PrimaryMagazineAmmo => primaryMagazine.ammoType;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseProjectile.OnRpcMessage"))
		{
			if (rpc == 3168282921u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CLProject"));
				}
				using (TimeWarning.New("CLProject"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3168282921u, "CLProject", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3168282921u, "CLProject", this, player))
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
							CLProject(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in CLProject");
					}
				}
				return true;
			}
			if (rpc == 1720368164 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Reload"));
				}
				using (TimeWarning.New("Reload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1720368164u, "Reload", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Reload(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Reload");
					}
				}
				return true;
			}
			if (rpc == 240404208 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerFractionalReloadInsert"));
				}
				using (TimeWarning.New("ServerFractionalReloadInsert"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(240404208u, "ServerFractionalReloadInsert", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ServerFractionalReloadInsert(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in ServerFractionalReloadInsert");
					}
				}
				return true;
			}
			if (rpc == 555589155 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartReload"));
				}
				using (TimeWarning.New("StartReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(555589155u, "StartReload", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							StartReload(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in StartReload");
					}
				}
				return true;
			}
			if (rpc == 1918419884 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SwitchAmmoTo"));
				}
				using (TimeWarning.New("SwitchAmmoTo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1918419884u, "SwitchAmmoTo", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SwitchAmmoTo(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in SwitchAmmoTo");
					}
				}
				return true;
			}
			if (rpc == 3327286961u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ToggleFireMode"));
				}
				using (TimeWarning.New("ToggleFireMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3327286961u, "ToggleFireMode", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3327286961u, "ToggleFireMode", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ToggleFireMode(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in ToggleFireMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected bool TryReload(IAmmoContainer ammoSource, int desiredAmount, bool canRefundAmmo = true)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		PooledList<Item> val = Pool.Get<PooledList<Item>>();
		try
		{
			ammoSource.FindItemsByItemID((List<Item>)(object)val, primaryMagazine.ammoType.itemid);
			if (((List<Item>)(object)val).Count == 0 && !primaryMagazine.allowAmmoSwitching)
			{
				return false;
			}
			if (((List<Item>)(object)val).Count == 0)
			{
				Item item = ammoSource.FindAmmo(primaryMagazine.definition.ammoTypes);
				if (item == null)
				{
					return false;
				}
				ammoSource.FindItemsByItemID((List<Item>)(object)val, item.info.itemid);
				if (((List<Item>)(object)val).Count == 0)
				{
					return false;
				}
				if (primaryMagazine.contents > 0)
				{
					if (canRefundAmmo)
					{
						ammoSource.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
					}
					SetAmmoCount(0);
				}
				primaryMagazine.ammoType = ((List<Item>)(object)val)[0].info;
			}
			int num = desiredAmount;
			if (num == -1)
			{
				num = primaryMagazine.capacity - primaryMagazine.contents;
			}
			for (int i = 0; i < ((List<Item>)(object)val).Count; i++)
			{
				Item item2 = ((List<Item>)(object)val)[i];
				_ = item2.amount;
				int num2 = Mathf.Min(num, item2.amount);
				item2.UseItem(num2);
				ModifyAmmoCount(num2);
				num -= num2;
				if (num <= 0)
				{
					break;
				}
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void SwitchAmmoTypesIfNeeded(IAmmoContainer ammoSource)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Item item = ammoSource.FindItemByItemID(primaryMagazine.ammoType.itemid);
		if (item != null)
		{
			return;
		}
		Item item2 = ammoSource.FindAmmo(primaryMagazine.definition.ammoTypes);
		if (item2 == null)
		{
			return;
		}
		item = ammoSource.FindItemByItemID(item2.info.itemid);
		if (item != null)
		{
			if (primaryMagazine.contents > 0)
			{
				ammoSource.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			primaryMagazine.ammoType = item.info;
		}
	}

	public static void StripAmmoToType(ref List<Item> ammos, ItemDefinition onlyAllowed)
	{
		if (!((Object)(object)onlyAllowed != (Object)null))
		{
			return;
		}
		for (int num = ammos.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)ammos[num].info != (Object)(object)onlyAllowed)
			{
				ammos.RemoveAt(num);
			}
		}
	}

	public void SetAmmoCount(int newCount)
	{
		primaryMagazine.contents = newCount;
		GetItem()?.MarkDirty();
	}

	public void ModifyAmmoCount(int amount)
	{
		SetAmmoCount(primaryMagazine.contents + amount);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return player.GetInheritedProjectileVelocity(direction);
	}

	public virtual float GetDamageScale(bool getMax = false)
	{
		return damageScale;
	}

	public virtual float GetDistanceScale(bool getMax = false)
	{
		return distanceScale;
	}

	public virtual float GetProjectileVelocityScale(bool getMax = false)
	{
		return projectileVelocityScale;
	}

	public virtual float GetOverrideProjectileThickness(Projectile projectile)
	{
		if ((Object)(object)projectile == (Object)null)
		{
			return 0f;
		}
		return projectile.thickness;
	}

	protected void StartReloadCooldown(float cooldown)
	{
		nextReloadTime = CalculateCooldownTime(nextReloadTime, cooldown, catchup: false, unscaledTime: true);
		startReloadTime = (float)nextReloadTime - cooldown;
	}

	protected void ResetReloadCooldown()
	{
		nextReloadTime = float.NegativeInfinity;
	}

	protected bool HasReloadCooldown()
	{
		return Time.unscaledTime < (float)nextReloadTime;
	}

	protected float GetReloadCooldown()
	{
		return Mathf.Max((float)nextReloadTime - Time.unscaledTime, 0f);
	}

	protected float GetReloadIdle()
	{
		return Mathf.Max(Time.unscaledTime - (float)nextReloadTime, 0f);
	}

	private void OnDrawGizmos()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient && (Object)(object)MuzzlePoint != (Object)null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(MuzzlePoint.position, MuzzlePoint.position + MuzzlePoint.forward * 10f);
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (Object.op_Implicit((Object)(object)ownerPlayer))
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(MuzzlePoint.position, MuzzlePoint.position + ownerPlayer.eyes.rotation * Vector3.forward * 10f);
			}
		}
	}

	public virtual RecoilProperties GetRecoil()
	{
		return recoilProperties;
	}

	public override float AmmoFraction()
	{
		return (float)primaryMagazine.contents / (float)primaryMagazine.capacity;
	}

	public virtual void DidAttackServerside()
	{
	}

	public override bool ServerIsReloading()
	{
		return Time.time < lastReloadTime + reloadTime;
	}

	public override bool CanReload()
	{
		return primaryMagazine.contents < primaryMagazine.capacity;
	}

	public override void TopUpAmmo()
	{
		SetAmmoCount(primaryMagazine.capacity);
	}

	public void SkipReload()
	{
		lastReloadTime = Time.time - reloadTime;
		ResetReloadCooldown();
		StartAttackCooldown(0f);
		StartAttackCooldownRaw(0f);
	}

	public override void ServerReload()
	{
		if (ServerIsReloading())
		{
			return;
		}
		lastReloadTime = Time.time;
		StartAttackCooldown(reloadTime);
		if (sendReloadSignalFromServer)
		{
			SignalBroadcast(Signal.Reload);
		}
		else
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				ownerPlayer.SignalBroadcast(Signal.Reload);
			}
		}
		SetAmmoCount(primaryMagazine.capacity);
	}

	public override bool ServerTryReload(IAmmoContainer ammoSource)
	{
		if (ServerIsReloading())
		{
			return false;
		}
		if (TryReloadMagazine(ammoSource, -1, shouldUpdateShieldState: false))
		{
			if (sendReloadSignalFromServer)
			{
				SignalBroadcast(Signal.Reload);
			}
			else
			{
				BasePlayer ownerPlayer = GetOwnerPlayer();
				if ((Object)(object)ownerPlayer != (Object)null)
				{
					ownerPlayer.SignalBroadcast(Signal.Reload);
				}
			}
			lastReloadTime = Time.time;
			StartAttackCooldown(reloadTime);
			return true;
		}
		return false;
	}

	public override Vector3 ModifyAIAim(Vector3 eulerInput, float swayModifier = 1f)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time * (aimSwaySpeed * 1f + aiAimSwayOffset);
		float num2 = Mathf.Sin(Time.time * 2f);
		float num3 = ((num2 < 0f) ? (1f - Mathf.Clamp(Mathf.Abs(num2) / 1f, 0f, 1f)) : 1f);
		float num4 = (false ? 0.6f : 1f);
		float num5 = (aimSway * 1f + aiAimSwayOffset) * num4 * num3 * swayModifier;
		eulerInput.y += (Mathf.PerlinNoise(num, num) - 0.5f) * num5 * Time.deltaTime;
		eulerInput.x += (Mathf.PerlinNoise(num + 0.1f, num + 0.2f) - 0.5f) * num5 * Time.deltaTime;
		return eulerInput;
	}

	public float GetAIAimcone()
	{
		NPCPlayer nPCPlayer = GetOwnerPlayer() as NPCPlayer;
		if (Object.op_Implicit((Object)(object)nPCPlayer))
		{
			return nPCPlayer.GetAimConeScale() * aiAimCone;
		}
		return aiAimCone;
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		bool flag = (Object)(object)ownerPlayer != (Object)null;
		if (primaryMagazine.contents <= 0)
		{
			SignalBroadcast(Signal.DryFire);
			StartAttackCooldownRaw(1f);
			return;
		}
		ModifyAmmoCount(-1);
		if (primaryMagazine.contents < 0)
		{
			SetAmmoCount(0);
		}
		bool flag2 = flag && ownerPlayer.IsNpc;
		BaseEntity owner;
		BaseNPC2 castedUnityObject;
		bool flag3 = TryGetOwner(out owner) && BaseNetworkableEx.Is<BaseNPC2>((Object)(object)owner, out castedUnityObject);
		bool flag4 = flag2 || flag3;
		if (flag2 && (ownerPlayer.isMounted || (Object)(object)ownerPlayer.GetParentEntity() != (Object)null))
		{
			NPCPlayer nPCPlayer = ownerPlayer as NPCPlayer;
			if ((Object)(object)nPCPlayer != (Object)null)
			{
				nPCPlayer.SetAimDirection(nPCPlayer.GetAimDirection());
			}
		}
		StartAttackCooldownRaw(repeatDelay);
		Vector3 val = (flag ? ownerPlayer.eyes.position : ((Component)MuzzlePoint).transform.position);
		Vector3 val2 = ((Component)MuzzlePoint).transform.forward;
		if (parameters.originOverride.HasValue)
		{
			Matrix4x4 value = parameters.originOverride.Value;
			val = ((Matrix4x4)(ref value)).GetPosition();
			value = parameters.originOverride.Value;
			val2 = ((Matrix4x4)(ref value)).MultiplyVector(Vector3.forward);
		}
		ItemModProjectile ammoInfo = ((Component)primaryMagazine.ammoType).GetComponent<ItemModProjectile>();
		SignalBroadcast(Signal.Attack, string.Empty, null, GetAttackEffect(), maxAttackEffectDistance);
		Projectile component = ammoInfo.projectileObject.Get().GetComponent<Projectile>();
		float num = ammoInfo.projectileVelocity * parameters.speedModifier;
		bool flag5 = GetParentEntity() is BasePlayer;
		BaseEntity baseEntity = null;
		if (flag && useOwnerForward)
		{
			val2 = ownerPlayer.eyes.BodyForward();
		}
		Ray val4 = default(Ray);
		for (int i = 0; i < ammoInfo.numProjectiles; i++)
		{
			Vector3 val3 = (flag2 ? AimConeUtil.GetModifiedAimConeDirection(ammoInfo.projectileSpread + GetAimCone() + GetAIAimcone(), val2) : ((!flag3) ? AimConeUtil.GetModifiedAimConeDirection(ammoInfo.projectileSpread + GetAimCone(), val2) : val2));
			float radius = (parameters.useBulletThickness ? GetOverrideProjectileThickness(component) : 0f);
			List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
			((Ray)(ref val4))._002Ector(val, val3);
			GamePhysics.TraceAll(val4, radius, list, 300f, 1220225793, (QueryTriggerInteraction)1, ownerPlayer);
			float distanceOverride = 0f;
			for (int j = 0; j < list.Count && parameters.damageModifier != 0f; j++)
			{
				RaycastHit hit = list[j];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if (flag5)
				{
					if ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)this || entity.EqualNetID((BaseNetworkable)this)))
					{
						continue;
					}
				}
				else if ((Object)(object)entity != (Object)null && this.HasEntityInParents(entity))
				{
					continue;
				}
				if (entity is BasePlayer basePlayer && basePlayer.TryGetActiveShield(out var foundShield) && foundShield.RaycastAgainstColliders(val4, 300f))
				{
					Vector3 val5 = ((Component)this).transform.InverseTransformPoint(((Component)foundShield).transform.position);
					Vector3 val6 = ((Component)this).transform.InverseTransformPoint(basePlayer.CenterPoint());
					if (((Vector3)(ref val5)).sqrMagnitude < ((Vector3)(ref val6)).sqrMagnitude)
					{
						continue;
					}
				}
				if ((Object)(object)entity != (Object)null && entity.isClient)
				{
					continue;
				}
				ColliderInfo component2 = ((Component)((RaycastHit)(ref hit)).collider).GetComponent<ColliderInfo>();
				if ((Object)(object)component2 != (Object)null && !component2.HasFlag(ColliderInfo.Flags.Shootable))
				{
					continue;
				}
				BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
				if (((Object)(object)entity != (Object)null && entity.IsNpc && flag4 && (Object)(object)baseCombatEntity != (Object)null && baseCombatEntity.GetFaction() != BaseCombatEntity.Faction.Horror && !(entity is BasePet)) || !((Object)(object)entity != (Object)null) || (!((Object)(object)baseEntity == (Object)null) && !((Object)(object)entity == (Object)(object)baseEntity) && !entity.EqualNetID((BaseNetworkable)baseEntity)) || !entity.IsVisible(val, ((RaycastHit)(ref hit)).point, 300f))
				{
					continue;
				}
				HitInfo info = Pool.Get<HitInfo>();
				AssignInitiator(info);
				info.Weapon = this;
				info.WeaponPrefab = base.gameManager.FindPrefab(base.PrefabName).GetComponent<AttackEntity>();
				info.IsPredicting = false;
				info.DoHitEffects = component.doHitEffects;
				info.DidHit = true;
				info.ProjectileVelocity = val3 * 300f;
				info.PointStart = MuzzlePoint.position;
				info.PointEnd = ((RaycastHit)(ref hit)).point;
				info.HitPositionWorld = ((RaycastHit)(ref hit)).point;
				info.HitNormalWorld = ((RaycastHit)(ref hit)).normal;
				info.HitEntity = entity;
				info.UseProtection = true;
				info.UseProtectionForNPCs = parameters.useProtectionForNPCs;
				distanceOverride = ((RaycastHit)(ref hit)).distance;
				component.CalculateDamage(info, GetProjectileModifier(), 1f);
				info.damageTypes.ScaleAll(GetDamageScale() * parameters.damageModifier * (flag4 ? npcDamageScale : turretDamageScale));
				float num2 = ((num > 0f) ? (((RaycastHit)(ref hit)).distance / num) : 0f);
				if (num2 > 0.2f)
				{
					Invoke(delegate
					{
						ProcessHit(info, ammoInfo);
					}, num2);
				}
				else
				{
					ProcessHit(info, ammoInfo);
				}
				if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			Pool.FreeUnmanaged<RaycastHit>(ref list);
			Vector3 val7 = ((flag && ownerPlayer.isMounted) ? (val3 * 6f) : Vector3.zero);
			CreateProjectileEffectClientside(ammoInfo.GetOverrideProjectile(this).resourcePath, val + val7, val3 * num, Random.Range(1, 100), null, IsSilenced(), forceClientsideEffects: true, null, distanceOverride);
		}
		static void ProcessHit(HitInfo hitInfo, ItemModProjectile itemModProjectile)
		{
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			if (!hitInfo.Weapon.IsValid() || !hitInfo.HitEntity.IsValid())
			{
				Pool.Free<HitInfo>(ref hitInfo);
			}
			else
			{
				hitInfo.HitEntity.OnAttacked(hitInfo);
				itemModProjectile.ServerProjectileHit(hitInfo);
				itemModProjectile.ServerProjectileHitEntity(hitInfo);
				Shield shield = hitInfo.HitEntity as Shield;
				if (hitInfo.HitEntity is BasePlayer || hitInfo.HitEntity is BaseNpc || (Object)(object)shield != (Object)null)
				{
					hitInfo.HitPositionLocal = ((Component)hitInfo.HitEntity).transform.InverseTransformPoint(hitInfo.HitPositionWorld);
					hitInfo.HitNormalLocal = ((Component)hitInfo.HitEntity).transform.InverseTransformDirection(hitInfo.HitNormalWorld);
					hitInfo.HitMaterial = StringPool.Get(((Object)(object)shield != (Object)null) ? shield.GetHitMaterialString() : "Flesh");
					Effect.server.ImpactEffect(hitInfo);
				}
				Pool.Free<HitInfo>(ref hitInfo);
			}
		}
	}

	private void AssignInitiator(HitInfo info)
	{
		info.Initiator = GetOwnerPlayer();
		if ((Object)(object)info.Initiator == (Object)null)
		{
			info.Initiator = GetParentEntity();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		primaryMagazine.ServerInit();
		Invoke(DelayedModSetup, 0.1f);
	}

	public void DelayedModSetup()
	{
		if (!modsChangedInitialized)
		{
			Item item = GetCachedItem();
			if (item != null && item.contents != null)
			{
				ItemContainer contents = item.contents;
				contents.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(contents.onItemAddedRemoved, new Action<Item, bool>(ModsChanged));
				modsChangedInitialized = true;
			}
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			Item item = GetCachedItem();
			if (item != null && item.contents != null)
			{
				ItemContainer contents = item.contents;
				contents.onItemAddedRemoved = (Action<Item, bool>)Delegate.Remove(contents.onItemAddedRemoved, new Action<Item, bool>(ModsChanged));
				modsChangedInitialized = false;
			}
		}
		base.DestroyShared();
	}

	public void ModsChanged(Item item, bool added)
	{
		Invoke(DelayedModsChanged, 0.1f);
	}

	public void ForceModsChanged()
	{
		Invoke(DelayedModSetup, 0f);
		Invoke(DelayedModsChanged, 0.2f);
	}

	public void DelayedModsChanged()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnWeaponModChange", this, GetOwnerPlayer()) != null)
		{
			return;
		}
		int num = Mathf.CeilToInt(ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectMagCap, ProjectileWeaponMod.SelectScalar) * (float)primaryMagazine.definition.builtInSize);
		if (num == primaryMagazine.capacity)
		{
			return;
		}
		if (primaryMagazine.contents > 0 && primaryMagazine.contents > num)
		{
			_ = primaryMagazine.ammoType;
			int contents = primaryMagazine.contents;
			BasePlayer ownerPlayer = GetOwnerPlayer();
			ItemContainer itemContainer = null;
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				itemContainer = ownerPlayer.inventory.containerMain;
			}
			else if (GetCachedItem() != null)
			{
				itemContainer = GetCachedItem().parent;
			}
			SetAmmoCount(0);
			if (itemContainer != null)
			{
				Item item = ItemManager.Create(primaryMagazine.ammoType, contents, 0uL, isServerSide: true, 0uL);
				if (!item.MoveToContainer(itemContainer))
				{
					Vector3 vPos = ((Component)this).transform.position;
					if ((Object)(object)itemContainer.entityOwner != (Object)null)
					{
						vPos = ((Component)itemContainer.entityOwner).transform.position + Vector3.up * 0.25f;
					}
					item.Drop(vPos, Vector3.up * 5f);
				}
			}
		}
		primaryMagazine.capacity = num;
		SendNetworkUpdate();
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		if (item != null && command == "unload_ammo" && !HasReloadCooldown())
		{
			UnloadAmmo(item, player);
		}
	}

	public void UnloadAmmo(Item item, BasePlayer player)
	{
		BaseProjectile component = ((Component)item.GetHeldEntity()).GetComponent<BaseProjectile>();
		if (!component.canUnloadAmmo || Interface.CallHook("OnAmmoUnload", component, item, player) != null || !Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		int num = component.primaryMagazine.contents;
		if (num <= 0)
		{
			return;
		}
		component.SetAmmoCount(0);
		item.MarkDirty();
		SendNetworkUpdateImmediate();
		int stackable = component.primaryMagazine.ammoType.stackable;
		if (num > stackable)
		{
			int num2 = Mathf.FloorToInt((float)(num / component.primaryMagazine.ammoType.stackable));
			num %= stackable;
			for (int i = 0; i < num2; i++)
			{
				Item item2 = ItemManager.Create(component.primaryMagazine.ammoType, stackable, 0uL, isServerSide: true, 0uL);
				player.GiveItem(item2);
			}
		}
		if (num > 0)
		{
			Item item3 = ItemManager.Create(component.primaryMagazine.ammoType, num, 0uL, isServerSide: true, 0uL);
			player.GiveItem(item3);
		}
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		if (!((Object)(object)crafter == (Object)null) && item != null)
		{
			UnloadAmmo(item, crafter);
		}
	}

	public override void ReturnedFromCancelledCraft(Item item, BasePlayer crafter)
	{
		if (!((Object)(object)crafter == (Object)null) && item != null)
		{
			BaseProjectile component = ((Component)item.GetHeldEntity()).GetComponent<BaseProjectile>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetAmmoCount(0);
			}
		}
	}

	public override void SetLightsOn(bool isOn)
	{
		base.SetLightsOn(isOn);
		UpdateAttachmentsState();
	}

	protected override bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return signal == Signal.Attack;
	}

	public void UpdateAttachmentsState()
	{
		_ = flags;
		bool b = ShouldLightsBeOn();
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
			if ((Object)(object)projectileWeaponMod != (Object)null && projectileWeaponMod.isLight)
			{
				using FlagsUpdateScope flagsUpdateScope = projectileWeaponMod.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.On, b);
			}
		}
	}

	private bool ShouldLightsBeOn()
	{
		if (LightsOn())
		{
			if (!IsDeployed())
			{
				return parentEntity.Get(base.isServer) is AutoTurret;
			}
			return true;
		}
		return false;
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildRemoved(child);
		if (child is ProjectileWeaponMod { isLight: not false })
		{
			using (FlagsUpdateScope flagsUpdateScope = child.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			SetLightsOn(isOn: false);
		}
	}

	public bool CanAiAttack()
	{
		return true;
	}

	public virtual float GetAimCone()
	{
		uint num = 0u;
		foreach (BaseEntity child in children)
		{
			num += (uint)(int)child.net.ID.Value;
			num += (uint)child.flags;
		}
		uint num2 = CRC.Compute32(0u, num);
		if (num2 != cachedModHash)
		{
			sightAimConeScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectSightAimCone, ProjectileWeaponMod.SelectScalar);
			sightAimConeOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectSightAimCone, ProjectileWeaponMod.SelectOffset);
			hipAimConeScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectHipAimCone, ProjectileWeaponMod.SelectScalar);
			hipAimConeOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectHipAimCone, ProjectileWeaponMod.SelectOffset);
			cachedModHash = num2;
		}
		float num3 = aimCone;
		num3 *= (UsingInternalBurstMode() ? internalBurstAimConeScale : 1f);
		if ((Object)(object)recoilProperties != (Object)null && recoilProperties.overrideAimconeWithCurve && primaryMagazine.capacity > 0)
		{
			num3 += recoilProperties.aimconeCurve.Evaluate((float)numShotsFired / (float)primaryMagazine.capacity % 1f) * recoilProperties.aimconeCurveScale;
			aimconePenalty = 0f;
		}
		if (aiming || base.isServer)
		{
			return (num3 + aimconePenalty + stancePenalty * stancePenaltyScale) * sightAimConeScale + sightAimConeOffset;
		}
		return (num3 + aimconePenalty + stancePenalty * stancePenaltyScale) * sightAimConeScale + sightAimConeOffset + hipAimCone * hipAimConeScale + hipAimConeOffset;
	}

	public float ScaleRepeatDelay(float delay)
	{
		float num = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectRepeatDelay, ProjectileWeaponMod.SelectScalar);
		float num2 = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectRepeatDelay, ProjectileWeaponMod.SelectOffset);
		float num3 = (UsingInternalBurstMode() ? internalBurstFireRateScale : 1f);
		return delay * num * num3 + num2;
	}

	public Projectile.Modifier GetProjectileModifier()
	{
		return new Projectile.Modifier
		{
			damageOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectDamage, ProjectileWeaponMod.SelectOffset),
			damageScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectDamage, ProjectileWeaponMod.SelectScalar) * GetDamageScale(),
			distanceOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectDistance, ProjectileWeaponMod.SelectOffset),
			distanceScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectDistance, ProjectileWeaponMod.SelectScalar) * GetDistanceScale()
		};
	}

	public bool IsBurstModeOnly()
	{
		if (isBurstWeapon)
		{
			return !canChangeFireModes;
		}
		return false;
	}

	public bool UsingBurstMode()
	{
		if (IsBurstDisabled())
		{
			return false;
		}
		return IsBurstEligable();
	}

	public bool UsingInternalBurstMode()
	{
		if (IsBurstDisabled())
		{
			return false;
		}
		return isBurstWeapon;
	}

	public bool IsBurstEligable()
	{
		if (isBurstWeapon)
		{
			return true;
		}
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if ((Object)(object)projectileWeaponMod != (Object)null && projectileWeaponMod.burstCount > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float TimeBetweenBursts()
	{
		return repeatDelay * 2f;
	}

	public int GetBurstModeCount()
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if ((Object)(object)projectileWeaponMod != (Object)null && projectileWeaponMod.burstCount > 0)
				{
					return projectileWeaponMod.burstCount;
				}
			}
		}
		return 3;
	}

	public virtual bool CanAttack()
	{
		if (ProjectileWeaponMod.HasBrokenWeaponMod(this))
		{
			return false;
		}
		return true;
	}

	public virtual float GetTurretReloadDuration()
	{
		if (turretReloadDurationOverride == -1f)
		{
			return GetReloadDuration() * 0.5f;
		}
		return turretReloadDurationOverride;
	}

	public virtual float GetReloadDuration()
	{
		if (fractionalReload)
		{
			int num = Mathf.Min(primaryMagazine.capacity - primaryMagazine.contents, GetAvailableAmmo());
			return reloadStartDuration + reloadEndDuration + reloadFractionDuration * (float)num;
		}
		return reloadTime;
	}

	public int GetAvailableAmmo()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null)
		{
			return primaryMagazine.contents;
		}
		List<Item> list = Pool.Get<List<Item>>();
		ownerPlayer.inventory.FindAmmo(list, primaryMagazine.definition.ammoTypes);
		int num = 0;
		if (list.Count != 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Item item = list[i];
				if ((Object)(object)item.info == (Object)(object)primaryMagazine.ammoType)
				{
					num += item.amount;
				}
			}
		}
		Pool.Free<Item>(ref list, false);
		return num;
	}

	public bool IsBurstDisabled()
	{
		return HasFlag(Flags.Reserved6) == defaultOn;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(2uL)]
	private void ToggleFireMode(RPCMessage msg)
	{
		if (canChangeFireModes && IsBurstEligable())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.Reserved6, !HasFlag(Flags.Reserved6));
			}
			Facepunch.Rust.Analytics.Azure.OnBurstModeToggled(msg.player, this, HasFlag(Flags.Reserved6));
		}
	}

	public virtual bool TryReloadMagazine(IAmmoContainer ammoSource, int desiredAmount = -1, bool shouldUpdateShieldState = true)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnMagazineReload", this, ammoSource, GetOwnerPlayer());
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!TryReload(ammoSource, desiredAmount))
		{
			return false;
		}
		SendNetworkUpdateImmediate();
		ItemManager.DoRemoves();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			ownerPlayer.inventory.ServerUpdate(0f);
		}
		if (shouldUpdateShieldState)
		{
			if (!fractionalReload)
			{
				UpdateShieldState(bHeld: true);
			}
			else if (primaryMagazine.contents == primaryMagazine.capacity || !ammoSource.HasAmmo(primaryMagazine.definition.ammoTypes))
			{
				UpdateShieldState(bHeld: true);
			}
		}
		return true;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void SwitchAmmoTo(RPCMessage msg)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetOwnerPlayer(out var ownerPlayer))
		{
			return;
		}
		int num = msg.read.Int32();
		if (num == primaryMagazine.ammoType.itemid)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(num);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return;
		}
		ItemModProjectile component = ((Component)itemDefinition).GetComponent<ItemModProjectile>();
		if (Object.op_Implicit((Object)(object)component) && component.IsAmmo(primaryMagazine.definition.ammoTypes) && Interface.CallHook("OnAmmoSwitch", this, ownerPlayer, itemDefinition) == null)
		{
			if (primaryMagazine.contents > 0)
			{
				ownerPlayer.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			primaryMagazine.ammoType = itemDefinition;
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			ownerPlayer.inventory.ServerUpdate(0f);
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		reloadStarted = false;
		reloadFinished = false;
		fractionalInsertCounter = 0;
		UpdateAttachmentsState();
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void StartReload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
		}
		else if (Interface.CallHook("OnWeaponReload", this, player) == null)
		{
			reloadFinished = false;
			reloadStarted = true;
			fractionalInsertCounter = 0;
			if (CanRefundAmmo)
			{
				SwitchAmmoTypesIfNeeded(player.inventory);
			}
			OnReloadStarted();
			StartReloadCooldown(GetReloadDuration());
		}
	}

	protected virtual void OnReloadStarted()
	{
		UpdateShieldState(bHeld: false);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void ServerFractionalReloadInsert(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!fractionalReload)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload not allowed (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_type");
			return;
		}
		if (!reloadStarted)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload request skipped (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_skip");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (GetReloadIdle() > 3f)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, $"T+{GetReloadIdle()}s ({base.ShortPrefabName})");
			player.stats.combat.LogInvalid(player, this, "reload_time");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (Time.unscaledTime < (float)startReloadTime + reloadStartDuration)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload too early (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_fraction_too_early");
			reloadStarted = false;
			reloadFinished = false;
		}
		if (Time.unscaledTime < (float)startReloadTime + reloadStartDuration + (float)fractionalInsertCounter * reloadFractionDuration)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload rate too high (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_fraction_rate");
			reloadStarted = false;
			reloadFinished = false;
		}
		else
		{
			fractionalInsertCounter++;
			if (primaryMagazine.contents < primaryMagazine.capacity)
			{
				TryReloadMagazine(player.inventory, 1);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Reload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!reloadStarted)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Request skipped (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_skip");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!fractionalReload)
		{
			if (GetReloadCooldown() > 1f)
			{
				AntiHack.Log(player, AntiHackType.ReloadHack, $"T-{GetReloadCooldown()}s ({base.ShortPrefabName})");
				player.stats.combat.LogInvalid(player, this, "reload_time");
				reloadStarted = false;
				reloadFinished = false;
				return;
			}
			if (GetReloadIdle() > 1.5f)
			{
				AntiHack.Log(player, AntiHackType.ReloadHack, $"T+{GetReloadIdle()}s ({base.ShortPrefabName})");
				player.stats.combat.LogInvalid(player, this, "reload_time");
				reloadStarted = false;
				reloadFinished = false;
				return;
			}
		}
		if (fractionalReload)
		{
			ResetReloadCooldown();
			UpdateShieldState(bHeld: true);
		}
		reloadStarted = false;
		reloadFinished = true;
		if (!fractionalReload)
		{
			TryReloadMagazine(player.inventory);
		}
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	[RPC_Server.FromOwner]
	private void CLProject(RPCMessage msg)
	{
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		if (reloadFinished && HasReloadCooldown())
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Reloading (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_cooldown");
			return;
		}
		reloadStarted = false;
		reloadFinished = false;
		if (primaryMagazine.contents <= 0 && !base.UsingInfiniteAmmoCheat)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Project magazine empty (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "magazine_empty_project");
			return;
		}
		ItemDefinition primaryMagazineAmmo = PrimaryMagazineAmmo;
		ProjectileShoot val = msg.read.Proto<ProjectileShoot>((ProjectileShoot)null);
		try
		{
			if (primaryMagazineAmmo.itemid != val.ammoType)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Ammo mismatch (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "ammo_mismatch");
				return;
			}
			if (!base.UsingInfiniteAmmoCheat)
			{
				ModifyAmmoCount(-1);
			}
			ItemModProjectile component = ((Component)primaryMagazineAmmo).GetComponent<ItemModProjectile>();
			if ((Object)(object)component == (Object)null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "mod_missing");
				return;
			}
			if (val.projectiles.Count > component.numProjectiles)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Count mismatch (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "count_mismatch");
				return;
			}
			Interface.CallHook("OnWeaponFired", this, msg.player, component, val);
			if (player.InGesture)
			{
				return;
			}
			SignalBroadcast(Signal.Attack, string.Empty, msg.connection, GetAttackEffect(), maxAttackEffectDistance);
			player.CleanupExpiredProjectiles();
			Guid projectileGroupId = Guid.NewGuid();
			foreach (Projectile projectile in val.projectiles)
			{
				if (player.HasFiredProjectile(projectile.projectileID))
				{
					AntiHack.Log(player, AntiHackType.ProjectileHack, $"Duplicate ID ({projectile.projectileID})");
					player.stats.combat.LogInvalid(player, this, "duplicate_id");
					continue;
				}
				Vector3 positionOffset = Vector3.zero;
				if (ConVar.AntiHack.projectile_positionoffset && (player.isMounted || player.HasParent()))
				{
					if (!ValidateEyePos(player, projectile.startPos, checkLineOfSight: false))
					{
						continue;
					}
					Vector3 position = player.eyes.position;
					positionOffset = position - projectile.startPos;
					projectile.startPos = position;
				}
				else if (!ValidateEyePos(player, projectile.startPos))
				{
					continue;
				}
				player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, this, primaryMagazineAmmo, projectileGroupId, positionOffset);
				if (!player.limitNetworking)
				{
					CreateProjectileEffectClientside(component.GetOverrideProjectile(this).resourcePath, projectile.startPos, projectile.startVel, projectile.seed, msg.connection, IsSilenced());
				}
			}
			player.MakeNoise(((Component)player).transform.position, BaseCombatEntity.ActionVolume.Loud);
			SingletonComponent<NpcNoiseManager>.Instance.OnWeaponShot(player, this);
			player.stats.Add(component.category + "_fired", val.projectiles.Count, (Stats)5);
			player.LifeStoryShotFired(this);
			StartAttackCooldown(ScaleRepeatDelay(repeatDelay) + animationDelay);
			player.MarkHostileFor();
			UpdateItemCondition();
			DidAttackServerside();
			BaseMountable mounted = player.GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				mounted.OnWeaponFired(this);
			}
			EACServer.LogPlayerUseWeapon(player, this);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void CreateProjectileEffectClientside(string prefabName, Vector3 pos, Vector3 velocity, int seed, Connection sourceConnection, bool silenced = false, bool forceClientsideEffects = false, List<Connection> targets = null, float distanceOverride = 0f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnClientProjectileEffectCreate", sourceConnection, this, prefabName) == null)
		{
			Effect effect = reusableInstance;
			effect.InitWithSourceEntity(Effect.Type.Projectile, this, pos, velocity, sourceConnection);
			((EffectData)effect).scale = (silenced ? 0f : 1f);
			if (forceClientsideEffects)
			{
				((EffectData)effect).scale = 2f;
			}
			effect.pooledString = prefabName;
			((EffectData)effect).number = seed;
			effect.targets = targets;
			((EffectData)effect).distanceOverride = distanceOverride;
			EffectNetwork.Send(effect);
		}
	}

	public void UpdateItemCondition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			return;
		}
		float barrelConditionLoss = ((Component)primaryMagazine.ammoType).GetComponent<ItemModProjectile>().barrelConditionLoss;
		float num = 0.25f;
		bool usingInfiniteAmmoCheat = base.UsingInfiniteAmmoCheat;
		if (!usingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(num + barrelConditionLoss);
		}
		if (ownerItem.contents == null || ownerItem.contents.itemList == null)
		{
			return;
		}
		for (int num2 = ownerItem.contents.itemList.Count - 1; num2 >= 0; num2--)
		{
			Item item = ownerItem.contents.itemList[num2];
			if (item != null && !usingInfiniteAmmoCheat)
			{
				float num3 = 1f;
				ProjectileWeaponMod projectileWeaponMod = item.GetHeldEntity() as ProjectileWeaponMod;
				if ((Object)(object)projectileWeaponMod != (Object)null)
				{
					num3 = projectileWeaponMod.ConditionLossMultiplier;
				}
				item.LoseCondition((num + barrelConditionLoss) * num3);
			}
		}
	}

	public bool IsSilenced()
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if ((Object)(object)projectileWeaponMod != (Object)null && projectileWeaponMod.isSilencer && !projectileWeaponMod.IsBroken())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AllowsPingUsage()
	{
		using (TimeWarning.New("AllowsPingUsage"))
		{
			if (children != null)
			{
				foreach (BaseEntity child in children)
				{
					ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
					if ((Object)(object)projectileWeaponMod != (Object)null && projectileWeaponMod.allowPings && !projectileWeaponMod.IsBroken())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public string GetAttackEffectAdditive()
	{
		string result = "";
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (!((Object)(object)projectileWeaponMod == (Object)null) && projectileWeaponMod.additiveEffect.isValid)
				{
					result = projectileWeaponMod.additiveEffect.resourcePath;
					break;
				}
			}
		}
		return result;
	}

	protected string GetAttackEffect()
	{
		string resourcePath = attackFX.resourcePath;
		if ((Object)(object)primaryMagazine.ammoType != (Object)null)
		{
			ItemModProjectile component = ((Component)primaryMagazine.ammoType).GetComponent<ItemModProjectile>();
			if (component.attackEffectOverride.isValid)
			{
				resourcePath = component.attackEffectOverride.resourcePath;
			}
		}
		if (children != null)
		{
			EffectSilencerSelect effectSilencerSelect = default(EffectSilencerSelect);
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if ((Object)(object)projectileWeaponMod == (Object)null)
				{
					continue;
				}
				if (projectileWeaponMod.isSilencer)
				{
					resourcePath = projectileWeaponMod.defaultSilencerEffect.resourcePath;
					if (silencedAttack.isValid)
					{
						resourcePath = silencedAttack.resourcePath;
						GameObject val = silencedAttack.Get();
						if ((Object)(object)val != (Object)null && val.TryGetComponent<EffectSilencerSelect>(ref effectSilencerSelect) && effectSilencerSelect.GetEffectForSilencerType(projectileWeaponMod.silencerType, out var result))
						{
							resourcePath = result.resourcePath;
						}
					}
					break;
				}
				if (projectileWeaponMod.isMuzzleBrake)
				{
					if (muzzleBrakeAttack.isValid)
					{
						resourcePath = muzzleBrakeAttack.resourcePath;
					}
					break;
				}
			}
		}
		return resourcePath;
	}

	public BaseEntity FindWeaponModEntity(Item weaponModItem)
	{
		if (weaponModItem == null)
		{
			return null;
		}
		ItemModEntity component = ((Component)weaponModItem.info).GetComponent<ItemModEntity>();
		object obj;
		if (component == null)
		{
			obj = null;
		}
		else
		{
			GameObjectRef entityPrefab = component.entityPrefab;
			if (entityPrefab == null)
			{
				obj = null;
			}
			else
			{
				GameObject obj2 = entityPrefab.Get();
				obj = ((obj2 != null) ? GameObjectEx.ToBaseEntity(obj2) : null);
			}
		}
		BaseEntity baseEntity = (BaseEntity)obj;
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		foreach (BaseEntity child in children)
		{
			if (!((Object)(object)child == (Object)null) && child.prefabID == baseEntity.prefabID)
			{
				return child;
			}
		}
		return null;
	}

	public override bool CanUseNetworkCache(Connection sendingTo)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null || ownerPlayer.net == null)
		{
			return true;
		}
		if (ownerPlayer.IsBeingSpectated)
		{
			return false;
		}
		Connection connection = ownerPlayer.net.connection;
		if (sendingTo == null || connection == null)
		{
			return true;
		}
		return sendingTo != connection;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Pool.Get<BaseProjectile>();
		if (info.forDisk || info.SendingTo(GetOwnerConnection()) || ForceSendMagazine(info))
		{
			info.msg.baseProjectile.primaryMagazine = primaryMagazine.Save();
		}
	}

	public virtual bool ForceSendMagazine(SaveInfo saveInfo)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (Object.op_Implicit((Object)(object)ownerPlayer) && ownerPlayer.IsBeingSpectated)
		{
			ReadOnlySpan<BasePlayer> spectators = ownerPlayer.GetSpectators();
			for (int i = 0; i < spectators.Length; i++)
			{
				if (spectators[i].net.connection == saveInfo.forConnection)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			primaryMagazine.Load(info.msg.baseProjectile.primaryMagazine);
		}
	}
}
