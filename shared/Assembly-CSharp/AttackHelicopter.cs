using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class AttackHelicopter : PlayerHelicopter
{
	public enum PreferredRocketType
	{
		HV,
		Incendiary
	}

	public class GunnerInputState
	{
		public bool fire1;

		public bool fire2;

		public bool reload;

		public Ray eyeRay;

		public Vector3 eyePos;

		public void Reset()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			fire1 = false;
			fire2 = false;
			reload = false;
			eyeRay = default;
		}
	}

	[Header("Attack Helicopter")]
	public Transform gunnerEyePos;

	[SerializeField]
	private Transform turbofanBone;

	[SerializeField]
	private GameObjectRef turretStoragePrefab;

	[SerializeField]
	private GameObjectRef rocketStoragePrefab;

	[SerializeField]
	private GameObjectRef gunCamUIPrefab;

	[SerializeField]
	private GameObjectRef gunCamUIDialogPrefab;

	[SerializeField]
	private GameObject gunCamUIParent;

	[SerializeField]
	private ParticleSystemContainer fxLightDamage;

	[SerializeField]
	private ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	private ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	private SoundDefinition damagedLightLoop;

	[SerializeField]
	private SoundDefinition damagedHeavyLoop;

	[SerializeField]
	private GameObject damageSoundTarget;

	public HealthThresholdToggle healthThresholdToggle;

	[SerializeField]
	private MeshRenderer monitorStaticRenderer;

	[SerializeField]
	private Material monitorStatic;

	[SerializeField]
	private Material monitorStaticSafeZone;

	[Header("Heli Pilot Flares")]
	[SerializeField]
	public GameObjectRef flareFireFX;

	[SerializeField]
	public GameObjectRef pilotFlare;

	[SerializeField]
	public Transform leftFlareLaunchPos;

	[SerializeField]
	public Transform rightFlareLaunchPos;

	[SerializeField]
	public float flareLaunchVel = 10f;

	[Header("Heli Pilot Lights")]
	[SerializeField]
	private Renderer rocketLightOff;

	[SerializeField]
	private Renderer rocketLightRed;

	[SerializeField]
	private Renderer rocketLightGreen;

	[SerializeField]
	private Renderer flareLightOff;

	[SerializeField]
	private Renderer flareLightRed;

	[SerializeField]
	private Renderer flareLightGreen;

	[Header("Heli Turret")]
	public Vector2 turretPitchClamp = new Vector2(-15f, 70f);

	public Vector2 turretYawClamp = new Vector2(-90f, 90f);

	public const Flags IN_GUNNER_VIEW_FLAG = Flags.Reserved9;

	public const Flags IN_SAFE_ZONE_FLAG = Flags.Reserved10;

	protected static int headingGaugeIndex = Animator.StringToHash("headingFraction");

	protected static int altGaugeIndex = Animator.StringToHash("altFraction");

	protected int altShakeIndex = -1;

	public EntityRef<AttackHelicopterTurret> turretInstance;

	public EntityRef<AttackHelicopterRockets> rocketsInstance;

	public GunnerInputState gunnerInputState = new GunnerInputState();

	public TimeSince timeSinceLastGunnerInput;

	public TimeSince timeSinceFailedWeaponFireRPC;

	public TimeSince timeSinceFailedFlareRPC;

	private int __sync_DetachedPanelsSync;

	[Sync(Autosave = true)]
	public int DetachedPanelsSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_DetachedPanelsSync;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_DetachedPanelsSync, value))
			{
				__sync_DetachedPanelsSync = value;
				byte nameID = __GetWeaverID("DetachedPanelsSync");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool HasSafeZoneFlag => HasFlag(Flags.Reserved10);

	public bool GunnerIsInGunnerView => HasFlag(Flags.Reserved9);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AttackHelicopter.OnRpcMessage"))
		{
			if (rpc == 3309981499u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CloseGunnerView"));
				}
				using (TimeWarning.New("RPC_CloseGunnerView"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3309981499u, "RPC_CloseGunnerView", this, player, 3f))
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
							RPC_CloseGunnerView(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_CloseGunnerView");
					}
				}
				return true;
			}
			if (rpc == 1427416040 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenGunnerView"));
				}
				using (TimeWarning.New("RPC_OpenGunnerView"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1427416040u, "RPC_OpenGunnerView", this, player, 3f))
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
							RPC_OpenGunnerView(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_OpenGunnerView");
					}
				}
				return true;
			}
			if (rpc == 4185921214u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenStorage"));
				}
				using (TimeWarning.New("RPC_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4185921214u, "RPC_OpenStorage", this, player, 3f))
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
							RPC_OpenStorage(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_OpenStorage");
					}
				}
				return true;
			}
			if (rpc == 148009183 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenTurret"));
				}
				using (TimeWarning.New("RPC_OpenTurret"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(148009183u, "RPC_OpenTurret", this, player, 3f))
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
							RPC_OpenTurret(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_OpenTurret");
					}
				}
				return true;
			}
			if (rpc == 46796481 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetRocketAmmoType"));
				}
				using (TimeWarning.New("RPC_SetRocketAmmoType"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(46796481u, "RPC_SetRocketAmmoType", this, player, 3f))
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
							RPC_SetRocketAmmoType(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_SetRocketAmmoType");
					}
				}
				return true;
			}
			if (rpc == 3589595843u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_TriggerRocketReload"));
				}
				using (TimeWarning.New("RPC_TriggerRocketReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3589595843u, "RPC_TriggerRocketReload", this, player, 3f))
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
							RPC_TriggerRocketReload(msg7);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_TriggerRocketReload");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!isServer)
		{
			return;
		}
		VehicleEngineController<PlayerHelicopter>.EngineState engineState = engineController.EngineStateFrom(old);
		if (engineController.CurEngineState == engineState)
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, engineController.IsStartingOrOn);
		}
		AttackHelicopterTurret turret = GetTurret();
		if (Object.op_Implicit((Object)(object)turret) && !engineController.IsStartingOrOn)
		{
			HeldEntity attachedHeldEntity = turret.GetAttachedHeldEntity();
			if (Object.op_Implicit((Object)(object)attachedHeldEntity) && attachedHeldEntity is ITurretNotify turretNotify)
			{
				turretNotify.OnAddedRemovedToTurret(added: false);
			}
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == turretStoragePrefab.GetEntity().prefabID)
		{
			AttackHelicopterTurret attackHelicopterTurret = (AttackHelicopterTurret)child;
			turretInstance.Set(attackHelicopterTurret);
			attackHelicopterTurret.owner = this;
		}
		if (child.prefabID == rocketStoragePrefab.GetEntity().prefabID)
		{
			AttackHelicopterRockets attackHelicopterRockets = (AttackHelicopterRockets)child;
			rocketsInstance.Set(attackHelicopterRockets);
			attackHelicopterRockets.owner = this;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.attackHeli != null)
		{
			turretInstance.uid = info.msg.attackHeli.turretID;
			rocketsInstance.uid = info.msg.attackHeli.rocketsID;
		}
	}

	public AttackHelicopterTurret GetTurret()
	{
		AttackHelicopterTurret attackHelicopterTurret = turretInstance.Get(isServer);
		if (attackHelicopterTurret.IsValid())
		{
			return attackHelicopterTurret;
		}
		return null;
	}

	public AttackHelicopterRockets GetRockets()
	{
		AttackHelicopterRockets attackHelicopterRockets = rocketsInstance.Get(isServer);
		if (attackHelicopterRockets.IsValid())
		{
			return attackHelicopterRockets;
		}
		return null;
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (!IsOn())
		{
			return;
		}
		bool flag = inputState.IsDown(BUTTON.FIRE_PRIMARY);
		bool flag2 = inputState.WasJustPressed(BUTTON.FIRE_SECONDARY);
		if (flag)
		{
			AttackHelicopterRockets rockets = GetRockets();
			if (rockets.TryFireRocket(player))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				WeaponFireFailed(rockets.GetRocketAmount(), player);
			}
		}
		if (flag2 && !GetRockets().TryFireFlare())
		{
			FlareFireFailed(player);
		}
	}

	public override void PassengerInput(InputState inputState, BasePlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.PassengerInput(inputState, player);
		timeSinceLastGunnerInput = TimeSince.op_Implicit(0f);
		gunnerInputState.fire1 = inputState.IsDown(BUTTON.FIRE_PRIMARY);
		gunnerInputState.fire2 = inputState.IsDown(BUTTON.FIRE_SECONDARY);
		gunnerInputState.reload = inputState.IsDown(BUTTON.RELOAD);
		gunnerInputState.eyeRay.direction = Quaternion.Euler(inputState.current.aimAngles) * Vector3.forward;
		gunnerInputState.eyeRay.origin = player.eyes.position + gunnerInputState.eyeRay.direction * 0.5f;
		if (IsOn() && GunnerIsInGunnerView)
		{
			AttackHelicopterTurret turret = GetTurret();
			if (turret.InputTick(gunnerInputState))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_PRIMARY))
			{
				turret.UpdateAmmoAmounts();
				WeaponFireFailed(turret.InventoryAmmo, player);
			}
			AttackHelicopterRockets rockets = GetRockets();
			if (rockets.InputTick(gunnerInputState, player))
			{
				MarkAllMountedPlayersAsHostile();
			}
			else if (inputState.WasJustPressed(BUTTON.FIRE_SECONDARY))
			{
				WeaponFireFailed(rockets.GetRocketAmount(), player);
			}
		}
		else
		{
			if (!IsOn() || GunnerIsInGunnerView)
			{
				return;
			}
			AttackHelicopterTurret turret2 = GetTurret();
			if (Object.op_Implicit((Object)(object)turret2))
			{
				HeldEntity attachedHeldEntity = turret2.GetAttachedHeldEntity();
				if (Object.op_Implicit((Object)(object)attachedHeldEntity) && attachedHeldEntity is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: false);
				}
			}
		}
	}

	public void WeaponFireFailed(int ammo, BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!(TimeSince.op_Implicit(timeSinceFailedWeaponFireRPC) <= 1f) && ammo <= 0)
		{
			ClientRPC(RpcTarget.Player("WeaponFireFailed", player));
			timeSinceFailedWeaponFireRPC = TimeSince.op_Implicit(0f);
		}
	}

	public void FlareFireFailed(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!(TimeSince.op_Implicit(timeSinceFailedFlareRPC) <= 1f))
		{
			ClientRPC(RpcTarget.Player("FlareFireFailed", player));
			timeSinceFailedFlareRPC = TimeSince.op_Implicit(0f);
		}
	}

	public override void VehicleFixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AttackHelicopter.VehicleFixedUpdate"))
		{
			base.VehicleFixedUpdate();
			if (TimeSince.op_Implicit(timeSinceLastGunnerInput) > 0.5f)
			{
				gunnerInputState.Reset();
			}
		}
	}

	public override bool EnterTrigger(TriggerBase trigger)
	{
		bool result = base.EnterTrigger(trigger);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved10, InSafeZone());
		return result;
	}

	public override void LeaveTrigger(TriggerBase trigger)
	{
		base.LeaveTrigger(trigger);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved10, InSafeZone());
	}

	public override void PrePlayerDismount(BasePlayer player, BaseMountable seat)
	{
		base.PrePlayerDismount(player, seat);
		if (HasFlag(Flags.Reserved9) && IsPassenger(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved9, b: false);
				return;
			}
		}
		if (!IsPassenger(player))
		{
			return;
		}
		AttackHelicopterTurret turret = GetTurret();
		if (Object.op_Implicit((Object)(object)turret))
		{
			HeldEntity attachedHeldEntity = turret.GetAttachedHeldEntity();
			if (Object.op_Implicit((Object)(object)attachedHeldEntity) && attachedHeldEntity is ITurretNotify turretNotify)
			{
				turretNotify.OnAddedRemovedToTurret(added: false);
			}
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			if (turretInstance.IsValid(isServer))
			{
				turretInstance.Get(isServer).DropItems();
			}
			if (rocketsInstance.IsValid(isServer))
			{
				rocketsInstance.Get(isServer).DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.attackHeli = Pool.Get<AttackHeli>();
		info.msg.attackHeli.turretID = turretInstance.uid;
		info.msg.attackHeli.rocketsID = rocketsInstance.uid;
	}

	public void MarkAllMountedPlayersAsHostile()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if ((Object)(object)mountPoint.mountable != (Object)null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((Object)(object)mounted != (Object)null)
				{
					mounted.MarkHostileFor();
				}
			}
		}
	}

	public override bool AdminFixUp(int tier)
	{
		if (!base.AdminFixUp(tier))
		{
			return false;
		}
		AttackHelicopterTurret turret = GetTurret();
		if ((Object)(object)turret != (Object)null)
		{
			ItemDefinition itemToCreate;
			ItemDefinition itemDefinition;
			switch (tier)
			{
			case 1:
				turret.inventory.Clear();
				itemToCreate = ItemManager.FindItemDefinition("hmlmg");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			case 2:
				turret.inventory.Clear();
				itemToCreate = ItemManager.FindItemDefinition("rifle.ak");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			default:
				turret.inventory.Clear();
				itemToCreate = ItemManager.FindItemDefinition("lmg.m249");
				itemDefinition = ItemManager.FindItemDefinition("ammo.rifle");
				break;
			}
			turret.inventory.AddItem(itemToCreate, 1, 0uL);
			turret.UpdateAmmoAmounts();
			int inventoryAmmo = turret.InventoryAmmo;
			int num = itemDefinition.stackable * (turret.inventory.capacity - 1);
			turret.forceAcceptAmmo = true;
			if (inventoryAmmo < num)
			{
				int num2 = num - inventoryAmmo;
				while (num2 > 0)
				{
					int num3 = Mathf.Min(num2, itemDefinition.stackable);
					turret.inventory.AddItem(itemDefinition, itemDefinition.stackable, 0uL);
					num2 -= num3;
				}
			}
			turret.forceAcceptAmmo = false;
		}
		AttackHelicopterRockets rockets = GetRockets();
		if ((Object)(object)rockets != (Object)null)
		{
			ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition("flare");
			ItemDefinition itemToCreate2 = ItemManager.FindItemDefinition("ammo.rocket.hv");
			ItemDefinition itemDefinition3 = ItemManager.FindItemDefinition("ammo.rocket.fire");
			int amount = itemDefinition2.stackable * 6;
			int amount2 = itemDefinition3.stackable * 6;
			switch (tier)
			{
			case 1:
				rockets.inventory.Clear();
				rockets.inventory.AddItem(itemDefinition2, amount, 0uL, ItemContainer.LimitStack.All);
				break;
			case 2:
				rockets.inventory.Clear();
				rockets.inventory.AddItem(itemDefinition2, amount, 0uL, ItemContainer.LimitStack.All);
				rockets.inventory.AddItem(itemToCreate2, amount2, 0uL, ItemContainer.LimitStack.All);
				rockets.inventory.AddItem(itemDefinition3, amount2, 0uL, ItemContainer.LimitStack.All);
				break;
			default:
				rockets.inventory.Clear();
				rockets.inventory.AddItem(itemDefinition2, amount, 0uL, ItemContainer.LimitStack.All);
				rockets.inventory.AddItem(itemToCreate2, amount2, 0uL, ItemContainer.LimitStack.All);
				rockets.inventory.AddItem(itemDefinition3, amount2, 0uL, ItemContainer.LimitStack.All);
				break;
			}
		}
		return true;
	}

	public void LaunchFlare()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(flareFireFX.resourcePath, this, StringPool.Get("FlareLaunchPos"), Vector3.zero, Vector3.zero);
		GameManager.server.CreatePrefab(pilotFlare.resourcePath, leftFlareLaunchPos.position, Quaternion.identity).GetComponent<HeliPilotFlare>().Init(-((Component)this).transform.right * flareLaunchVel);
		GameManager.server.CreatePrefab(pilotFlare.resourcePath, rightFlareLaunchPos.position, Quaternion.identity).GetComponent<HeliPilotFlare>().Init(((Component)this).transform.right * flareLaunchVel);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenTurret(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.isMounted || (IsSafe() && (Object)(object)player != (Object)(object)creatorEntity))
		{
			return;
		}
		StorageContainer turret = GetTurret();
		if (!((Object)(object)turret == (Object)null))
		{
			BasePlayer driver = GetDriver();
			if (!((Object)(object)driver != (Object)null) || !((Object)(object)driver != (Object)(object)player))
			{
				turret.PlayerOpenLoot(player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.isMounted || (IsSafe() && (Object)(object)player != (Object)(object)creatorEntity))
		{
			return;
		}
		StorageContainer rockets = GetRockets();
		if (!((Object)(object)rockets == (Object)null))
		{
			BasePlayer driver = GetDriver();
			if (!((Object)(object)driver != (Object)null) || !((Object)(object)driver != (Object)(object)player))
			{
				rockets.PlayerOpenLoot(player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenGunnerView(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || !IsOn() || !IsPassenger(player) || InSafeZone())
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, b: true);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_CloseGunnerView(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!IsPassenger(player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_SetRocketAmmoType(RPCMessage msg)
	{
		if (!((Object)(object)GetDriver() != (Object)(object)msg.player))
		{
			PreferredRocketType ammoType = (PreferredRocketType)msg.read.Int32();
			GetRockets().SetAmmoType(ammoType);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_TriggerRocketReload(RPCMessage msg)
	{
		if (!((Object)(object)GetDriver() != (Object)(object)msg.player))
		{
			GetRockets().StartReload();
		}
	}

	public override void FilterServerProjectileAmmo(List<Item> ammoList)
	{
		base.FilterServerProjectileAmmo(ammoList);
		AttackHelicopterRockets rockets = GetRockets();
		for (int i = 0; i < ammoList.Count; i++)
		{
			Item item = ammoList[i];
			if (((Object)(object)item.info == (Object)(object)rockets.hvRocketDef && rockets.preferredRocketType != PreferredRocketType.HV) || ((Object)(object)item.info == (Object)(object)rockets.incendiaryRocketDef && rockets.preferredRocketType != PreferredRocketType.Incendiary))
			{
				ammoList.RemoveAt(i);
				i--;
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		if ((Object)(object)healthThresholdToggle != (Object)null)
		{
			healthThresholdToggle.NoteDamage(info, ((Component)this).transform);
		}
		base.Hurt(info);
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		if ((Object)(object)healthThresholdToggle != (Object)null)
		{
			if (newvalue > oldvalue)
			{
				healthThresholdToggle.NoteHealing((newvalue - oldvalue) / MaxHealth());
			}
			DetachedPanelsSync = healthThresholdToggle.UpdateDetachedMask(DetachedPanelsSync, healthFraction);
			healthThresholdToggle.ApplyMask(DetachedPanelsSync);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if ((Object)(object)healthThresholdToggle != (Object)null)
		{
			DetachedPanelsSync = healthThresholdToggle.UpdateDetachedMask(DetachedPanelsSync, healthFraction);
			healthThresholdToggle.ApplyMask(DetachedPanelsSync);
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: DetachedPanelsSync for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_DetachedPanelsSync);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_DetachedPanelsSync;
				int _sync_DetachedPanelsSync = reader.Int32();
				__sync_DetachedPanelsSync = _sync_DetachedPanelsSync;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "DetachedPanelsSync")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_DetachedPanelsSync = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public AttackHelicopter()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
	}
}
