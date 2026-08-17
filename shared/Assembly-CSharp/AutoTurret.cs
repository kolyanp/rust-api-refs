using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class AutoTurret : ContainerIOEntity, IRemoteControllable, IHostileWarningEntity, IAdminUpdatableIdentifier
{
	public class UpdateAutoTurretScanQueue : PersistentObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (((PersistentObjectWorkQueue<AutoTurret>)this).ShouldAdd(entity))
			{
				entity.TargetScan();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class UpdateAutoTurretAmmoQueue : ObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (((ObjectWorkQueue<AutoTurret>)this).ShouldAdd(entity))
			{
				entity.UpdateTotalAmmo();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class UpdateAutoTurretTick : ObjectWorkQueue<AutoTurret>
	{
		protected override void RunJob(AutoTurret entity)
		{
			if (((ObjectWorkQueue<AutoTurret>)this).ShouldAdd(entity))
			{
				entity.RunScheduledTick();
			}
		}

		protected override bool ShouldAdd(AutoTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	private enum YawPitchMode
	{
		Separate,
		Merged
	}

	public struct AutoTurretPreserveInfo
	{
		public List<ulong> authedPlayers;

		public bool isPeacekeeper;

		public string rcIdentifier;
	}

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public float bulletSpeed;

	public AmbienceEmitter ambienceEmitter;

	public bool playAmbientSounds;

	public GameObject assignDialog;

	public LaserBeam laserBeam;

	public static int GlobalPowerCounter = 1;

	[NonSerialized]
	public int PowerOrder;

	public HashSet<AutoTurret> nearbyTurrets;

	private HashSet<AutoTurret> interferringTurrets;

	[ServerVar(Help = "How many milliseconds to spend on target scanning per frame")]
	public static float scan_budget_ms = 0.5f;

	[ServerVar(Help = "How many milliseconds to spend on ammo updating per frame")]
	public static float ammo_update_ms = 0.1f;

	[ServerVar(Help = "How many milliseconds to spend on a tick per frame")]
	public static float tick_update_ms = 1f;

	public static UpdateAutoTurretScanQueue updateAutoTurretScanQueue = new UpdateAutoTurretScanQueue();

	public static UpdateAutoTurretAmmoQueue updateAutoTurretAmmoQueue = new UpdateAutoTurretAmmoQueue();

	public static UpdateAutoTurretTick updateTurretTick = new UpdateAutoTurretTick();

	[Header("RC")]
	public float rcTurnSensitivity;

	public Transform RCEyes;

	public GameObjectRef IDPanelPrefab;

	public RemoteControllableControls rcControls;

	public string rcIdentifier;

	public TargetTrigger targetTrigger;

	public TriggerBase interferenceTrigger;

	public float maxInterference;

	public float attachedWeaponZOffsetScale;

	public Transform socketTransform;

	public const float ServerTickRateSeconds = 0.2f;

	public bool authDirty;

	public double nextVisCheck;

	public double lastTargetSeenTime;

	private Vector3 lastTargetAimOffset;

	private double lastDamageEventTime;

	private double lastScanTime;

	public bool targetVisible;

	public bool booting;

	public Vector3 targetAimDir;

	private int currentBurstShotsFired;

	public const float bulletDamage = 15f;

	public RealTimeSinceEx timeSinceLastServerTick;

	public static HashSet<AutoTurret> interferenceUpdateList = new HashSet<AutoTurret>();

	private const float SlowProjectileSpeedMultplier = 2f;

	private const float SlowProjectileSpeedThreshold = 100f;

	protected Transform cachedTransf;

	private YawPitchMode rotateMode;

	private Matrix4x4 toYawFromRoot;

	private Matrix4x4 toPitchFromRootOrYaw;

	private Matrix4x4 toRCEyesFromPitch;

	private Quaternion gunAimInitialYawRot;

	private Quaternion gunAimInitialPitchOrTotalRot;

	private Quaternion gunAimYawRotLS;

	private Quaternion gunAimPitchOrTotalRotLS;

	private Quaternion gunAimTotalRotWS;

	private Action _actionSetOnline;

	private Action _actionServerThink;

	private Action _actionServerDo;

	private Action _actionSendAimDir;

	private Action _actionUpdateAttachedWeapon;

	public Vector3 lastSentAimDir;

	public static float[] visibilityOffsets = new float[3] { 0f, 0.15f, -0.15f };

	public int peekIndex;

	[NonSerialized]
	public int numConsecutiveMisses;

	private double nextIdleAimTime;

	[NonSerialized]
	public int totalAmmo;

	public double nextAmmoCheckTime;

	public bool totalAmmoDirty;

	public float currentAmmoGravity;

	public float currentAmmoVelocity;

	public HeldEntity AttachedWeapon;

	private bool shouldUpdateOnOutOfAmmo;

	private float lastTickTime;

	public BaseCombatEntity target;

	public Transform eyePos;

	public Transform muzzlePos;

	public Vector3 aimDir;

	public Transform gun_yaw;

	public Transform gun_pitch;

	public float sightRange;

	public SoundDefinition turnLoopDef;

	public SoundDefinition movementChangeDef;

	public SoundDefinition ambientLoopDef;

	public SoundDefinition focusCameraDef;

	public float focusSoundFreqMin;

	public float focusSoundFreqMax;

	public GameObjectRef peacekeeperToggleSound;

	public GameObjectRef onlineSound;

	public GameObjectRef offlineSound;

	public GameObjectRef targetAcquiredEffect;

	public GameObjectRef targetLostEffect;

	public GameObjectRef reloadEffect;

	public float aimCone;

	public const Flags Flag_Peacekeeper = Flags.Reserved1;

	public const Flags Flag_Equipped = Flags.Reserved3;

	public const Flags Flag_MaxAuths = Flags.Reserved4;

	public const Flags Flag_ShowAlphaCover = Flags.Reserved5;

	[NonSerialized]
	public HashSet<ulong> authorizedPlayers;

	[NonSerialized]
	public int consumptionAmount;

	public bool CanPing => false;

	private int interferenceCount => interferringTurrets.Count;

	public virtual bool RequiresMouse => true;

	public float MaxRange => 10000f;

	public RemoteControllableControls RequiredControls => rcControls;

	public int ViewerCount { get; set; }

	public CameraViewerId? ControllingViewerId { get; set; }

	public bool IsBeingControlled
	{
		get
		{
			if (ViewerCount > 0)
			{
				return ControllingViewerId.HasValue;
			}
			return false;
		}
	}

	public double nextShotTime { get; private set; }

	public double lastShotTime { get; private set; }

	private Action actionSetOnline => SetOnline;

	private Action actionServerThink => ServerThink;

	private Action actionServerDo => ServerDo;

	private Action actionSendAimDir => SendAimDir;

	private Action actionUpdateAttachedWeapon => UpdateAttachedWeapon;

	protected override bool PreventDuplicatesInQueue
	{
		public get
		{
			return Sentry.debugPreventDuplicates;
		}
	}

	public bool IsServer => base.isServer;

	public bool IsClient => base.isClient;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AutoTurret.OnRpcMessage"))
		{
			if (rpc == 1092560690 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AddSelfAuthorize"));
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 3057055788u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AssignToFriend"));
				}
				using (TimeWarning.New("AssignToFriend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3057055788u, "AssignToFriend", this, player, 3f))
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
							AssignToFriend(msg2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in AssignToFriend");
					}
				}
				return true;
			}
			if (rpc == 253307592 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ClearList"));
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ClearList(rpc3);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 1500257773 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - FlipAim"));
				}
				using (TimeWarning.New("FlipAim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1500257773u, "FlipAim", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							FlipAim(rpc4);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in FlipAim");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RemoveSelfAuthorize"));
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RemoveSelfAuthorize(rpc5);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 2025588587 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AdminUpdateIdentifier"));
				}
				using (TimeWarning.New("Server_AdminUpdateIdentifier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2025588587u, "Server_AdminUpdateIdentifier", this, player, 3f))
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
							Server_AdminUpdateIdentifier(msg3);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in Server_AdminUpdateIdentifier");
					}
				}
				return true;
			}
			if (rpc == 1770263114 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_AttackAll"));
				}
				using (TimeWarning.New("SERVER_AttackAll"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1770263114u, "SERVER_AttackAll", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_AttackAll(rpc6);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in SERVER_AttackAll");
					}
				}
				return true;
			}
			if (rpc == 3265538831u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_Peacekeeper"));
				}
				using (TimeWarning.New("SERVER_Peacekeeper"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3265538831u, "SERVER_Peacekeeper", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_Peacekeeper(rpc7);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in SERVER_Peacekeeper");
					}
				}
				return true;
			}
			if (rpc == 1677685895 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestOpenRCPanel"));
				}
				using (TimeWarning.New("SERVER_RequestOpenRCPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
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
							SERVER_RequestOpenRCPanel(msg4);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in SERVER_RequestOpenRCPanel");
					}
				}
				return true;
			}
			if (rpc == 1053317251 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetID"));
				}
				using (TimeWarning.New("Server_SetID"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1053317251u, "Server_SetID", this, player, 3f))
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
							Server_SetID(msg5);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in Server_SetID");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool PeacekeeperMode()
	{
		return HasFlag(Flags.Reserved1);
	}

	protected virtual bool ShouldApplyInterference()
	{
		return true;
	}

	private void ServerInit_Interference()
	{
		if (!Application.isLoadingSave)
		{
			AddNearbyTurrets();
		}
	}

	private void ServerDestroy_Interference()
	{
		RemoveFromNeabyTurrets();
	}

	private void PostServerLoad_Interference()
	{
		AddNearbyTurrets();
	}

	private void OnTurretPowerChanged(bool online)
	{
		if (ShouldApplyInterference())
		{
			if (online)
			{
				PowerOrder = GlobalPowerCounter++;
				RecalculateInterference();
			}
			else
			{
				OnTurretDisabled();
				SetInterferenceEnabled(state: false);
			}
		}
	}

	private void Save_Interference(AutoTurret data)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		data.powerOrder = PowerOrder;
		data.interferenceList = Pool.Get<List<NetworkableId>>();
		foreach (AutoTurret interferringTurret in interferringTurrets)
		{
			if (interferringTurret.IsValid())
			{
				data.interferenceList.Add(interferringTurret.net.ID);
			}
		}
	}

	private void Load_Interference(AutoTurret data)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		PowerOrder = data.powerOrder;
		if (PowerOrder > GlobalPowerCounter)
		{
			GlobalPowerCounter = PowerOrder;
		}
		interferringTurrets.Clear();
		if (data.interferenceList == null)
		{
			return;
		}
		foreach (NetworkableId interference in data.interferenceList)
		{
			AutoTurret autoTurret = BaseNetworkable.serverEntities.Find(interference) as AutoTurret;
			if ((Object)(object)autoTurret != (Object)null && (Object)(object)autoTurret != (Object)(object)this && !autoTurret.IsDestroyed)
			{
				interferringTurrets.Add(autoTurret);
			}
		}
	}

	private void AddNearbyTurrets()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		nearbyTurrets.Clear();
		List<AutoTurret> list = Pool.Get<List<AutoTurret>>();
		if (Interface.CallHook("OnNearbyTurretsScan", this, list) == null)
		{
			Vis.Entities(((Component)this).transform.position, Sentry.interferenceradius, list, 256, (QueryTriggerInteraction)1);
		}
		foreach (AutoTurret item in list)
		{
			if (item.IsServer && !((Object)(object)item == (Object)(object)this) && !item.IsDestroyed)
			{
				item.nearbyTurrets.Add(this);
				nearbyTurrets.Add(item);
			}
		}
		Pool.FreeUnmanaged<AutoTurret>(ref list);
	}

	private void RemoveFromNeabyTurrets()
	{
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			nearbyTurret.nearbyTurrets.Remove(this);
		}
		nearbyTurrets.Clear();
	}

	private bool ShouldTurretOverload(out int estimatedInterference)
	{
		estimatedInterference = 0;
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			if (nearbyTurret.IsOn() && !nearbyTurret.HasInterference())
			{
				if (nearbyTurret.interferringTurrets.Count + 1 == Sentry.maxinterference)
				{
					return true;
				}
				estimatedInterference++;
				if (estimatedInterference >= Sentry.maxinterference)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool RecalculateInterference()
	{
		object obj = Interface.CallHook("OnInterferenceUpdate", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		bool flag = HasInterference();
		if (ShouldTurretOverload(out var _))
		{
			SetInterferenceEnabled(state: true);
			return flag != HasInterference();
		}
		SetInterferenceEnabled(state: false);
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			if (nearbyTurret.IsOn() && !nearbyTurret.HasInterference())
			{
				nearbyTurret.interferringTurrets.Add(this);
				interferringTurrets.Add(nearbyTurret);
			}
		}
		return flag != HasInterference();
	}

	private void OnTurretDisabled()
	{
		if (HasInterference())
		{
			return;
		}
		interferringTurrets.Clear();
		List<AutoTurret> list = Pool.Get<List<AutoTurret>>();
		foreach (AutoTurret nearbyTurret in nearbyTurrets)
		{
			nearbyTurret.interferringTurrets.Remove(this);
			if (nearbyTurret.HasInterference() && nearbyTurret.interferenceCount < Sentry.maxinterference)
			{
				list.Add(nearbyTurret);
			}
		}
		SortTurretsByInterferenceLevel(list);
		foreach (AutoTurret item in list)
		{
			item.SetInterferenceEnabled(state: false);
			item.RecalculateInterference();
		}
		Pool.FreeUnmanaged<AutoTurret>(ref list);
	}

	private void ClearInterference()
	{
		foreach (AutoTurret interferringTurret in interferringTurrets)
		{
			interferringTurret.interferringTurrets.Remove(this);
		}
		interferringTurrets.Clear();
	}

	private void SortTurretsByPowerOnTime(List<AutoTurret> turrets)
	{
		turrets.Sort((AutoTurret a, AutoTurret b) => a.PowerOrder.CompareTo(b.PowerOrder));
	}

	private void SortTurretsByInterferenceLevel(List<AutoTurret> turrets)
	{
		List<AutoTurret> list = Pool.Get<List<AutoTurret>>();
		list.AddRange(turrets.OrderBy((AutoTurret x) => x.nearbyTurrets.Count((AutoTurret y) => y.IsOn() && !y.HasInterference())));
		turrets.Clear();
		turrets.AddRange(list);
		Pool.FreeUnmanaged<AutoTurret>(ref list);
	}

	private void SetInterferenceEnabled(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, state);
	}

	public bool HasInterference()
	{
		if (ShouldApplyInterference())
		{
			return IsOnFire();
		}
		return false;
	}

	public Transform GetEyes()
	{
		return RCEyes;
	}

	public float GetFovScale()
	{
		return 1f;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public virtual bool CanControl(ulong playerID)
	{
		object obj = Interface.CallHook("OnEntityControl", this, playerID);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (booting)
		{
			return false;
		}
		if (IsPowered())
		{
			return !PeacekeeperMode();
		}
		return false;
	}

	public bool InitializeControl(CameraViewerId viewerID)
	{
		ViewerCount++;
		if (!ControllingViewerId.HasValue)
		{
			ControllingViewerId = viewerID;
			SetTarget(null);
			SendAimDirImmediate();
			return true;
		}
		return false;
	}

	public void StopControl(CameraViewerId viewerID)
	{
		ViewerCount--;
		if (ControllingViewerId == viewerID)
		{
			ControllingViewerId = null;
		}
	}

	public Matrix4x4 GetEyesMatrix()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return GetCenterMuzzle() * toRCEyesFromPitch;
	}

	public void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		CameraViewerId? controllingViewerId = ControllingViewerId;
		if (viewerID != controllingViewerId)
		{
			return;
		}
		UpdateManualAim(inputState);
		double timeAsDouble = Time.timeAsDouble;
		if (timeAsDouble < nextShotTime)
		{
			return;
		}
		if (inputState.WasJustPressed(BUTTON.RELOAD))
		{
			Reload();
		}
		else
		{
			if (EnsureReloaded())
			{
				return;
			}
			bool num = inputState.IsDown(BUTTON.FIRE_PRIMARY);
			bool flag = TryGetAttachedWeapon(out var baseProjectile);
			bool flag2 = flag && IsUsingBurstFireWeapon(baseProjectile);
			int num2 = (flag2 ? baseProjectile.GetBurstModeCount() : 0);
			bool flag3 = flag2 && IsBursting(baseProjectile);
			if (num | flag3)
			{
				if (flag)
				{
					if (baseProjectile is ITurretNotify turretNotify)
					{
						turretNotify.WarmupTick(wantsShoot: true);
					}
					float damageModifier = 1f;
					float speedModifier = 1f;
					ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
					if (Object.op_Implicit((Object)(object)ammoType))
					{
						ItemModProjectile component = ((Component)ammoType).GetComponent<ItemModProjectile>();
						if (Object.op_Implicit((Object)(object)component) && component.projectileVelocity < 100f)
						{
							speedModifier = 2f;
						}
					}
					if (baseProjectile.primaryMagazine.contents > 0)
					{
						if (!flag2 || currentBurstShotsFired < num2)
						{
							FireAttachedGun(Vector3.zero, aimCone, null, damageModifier, speedModifier);
						}
						float num3;
						if (flag2)
						{
							currentBurstShotsFired++;
							if (currentBurstShotsFired < num2 && baseProjectile.primaryMagazine.contents > 0)
							{
								num3 = Mathf.Max(baseProjectile.ScaleRepeatDelay(baseProjectile.repeatDelay), baseProjectile.NextAttackTime - (float)timeAsDouble);
							}
							else
							{
								ResetBurstFireState();
								num3 = Mathf.Max(baseProjectile.TimeBetweenBursts(), Player.clientTickInterval.Get() * 2f);
							}
						}
						else
						{
							ResetBurstFireState();
							num3 = (baseProjectile.isSemiAuto ? (baseProjectile.repeatDelay * 1.5f) : baseProjectile.repeatDelay);
							num3 = baseProjectile.ScaleRepeatDelay(num3);
						}
						nextShotTime = timeAsDouble + (double)num3;
						if ((float)nextShotTime < baseProjectile.NextAttackTime && !Mathf.Approximately((float)nextShotTime, baseProjectile.NextAttackTime))
						{
							Debug.LogWarning((object)string.Format("Turret {0} next shot scheduled in {1}s will be skipped due to it being less than attached {2} attack cooldown ({3}s).", new object[4]
							{
								((Object)this).name,
								num3,
								((Object)baseProjectile).name,
								baseProjectile.NextAttackTime - (float)timeAsDouble
							}), (Object)(object)this);
						}
					}
					else
					{
						ResetBurstFireState();
						nextShotTime = timeAsDouble + 5.0;
					}
				}
				else if (HasGenericFireable())
				{
					AttachedWeapon.ServerUse();
					nextShotTime = timeAsDouble + 0.11500000208616257;
				}
				else
				{
					nextShotTime = timeAsDouble + 1.0;
				}
			}
			else if (Object.op_Implicit((Object)(object)baseProjectile) && baseProjectile is ITurretNotify turretNotify2)
			{
				turretNotify2.WarmupTick(wantsShoot: false);
			}
		}
	}

	public bool UpdateManualAim(InputState inputState)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		float num = (0f - inputState.current.mouseDelta.y) * rcTurnSensitivity;
		float num2 = inputState.current.mouseDelta.x * rcTurnSensitivity;
		Quaternion val = Quaternion.LookRotation(aimDir, ((Component)this).transform.up);
		Vector3 val2 = ((Quaternion)(ref val)).eulerAngles + new Vector3(num, num2, 0f);
		if (val2.x >= 0f && val2.x <= 135f)
		{
			val2.x = Mathf.Clamp(val2.x, 0f, 45f);
		}
		if (val2.x >= 225f && val2.x <= 360f)
		{
			val2.x = Mathf.Clamp(val2.x, 285f, 360f);
		}
		Vector3 val3 = Quaternion.Euler(val2) * Vector3.forward;
		bool result = !Mathf.Approximately(aimDir.x, val3.x) || !Mathf.Approximately(aimDir.y, val3.y) || !Mathf.Approximately(aimDir.z, val3.z);
		aimDir = val3;
		return result;
	}

	public override void InitShared()
	{
		base.InitShared();
		RCSetup();
	}

	public override void DestroyShared()
	{
		RCShutdown();
		base.DestroyShared();
	}

	public void RCSetup()
	{
		if (base.isServer)
		{
			RemoteControlEntity.InstallControllable(this);
		}
	}

	public void RCShutdown()
	{
		if (base.isServer)
		{
			RemoteControlEntity.RemoveControllable(this);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void Server_SetID(RPCMessage msg)
	{
		string oldID = msg.read.String();
		string newID = msg.read.String();
		SetID(msg.player, oldID, newID);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_AdminUpdateIdentifier(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && (msg.player.IsAdmin || msg.player.IsDeveloper))
		{
			string oldID = msg.read.String();
			string newID = msg.read.String();
			SetID(msg.player, oldID, newID, bypassChecks: true);
		}
	}

	public void SetID(BasePlayer player, string oldID, string newID, bool bypassChecks = false)
	{
		if ((CanChangeID(player) || bypassChecks) && (string.IsNullOrEmpty(oldID) || ComputerStation.IsValidIdentifier(oldID)) && ComputerStation.IsValidIdentifier(newID) && oldID == GetIdentifier() && Interface.CallHook("OnTurretIdentifierSet", this, player, newID) == null)
		{
			Debug.Log((object)"SetID success!");
			UpdateIdentifier(newID);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void SERVER_RequestOpenRCPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanChangeID(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenRCPanel", player), GetIdentifier());
		}
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		_ = rcIdentifier;
		if (base.isServer)
		{
			if (!RemoteControlEntity.IDInUse(newID))
			{
				rcIdentifier = newID;
			}
			SendNetworkUpdate();
		}
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public virtual bool CanChangeID(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return CanChangeSettings(player);
		}
		return false;
	}

	public override int ConsumptionAmount()
	{
		return consumptionAmount;
	}

	public void SetOnline()
	{
		SetIsOnline(online: true);
	}

	public void SetIsOnline(bool online)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (Object.op_Implicit((Object)(object)attachedWeapon) && attachedWeapon is ITurretNotify turretNotify)
		{
			turretNotify.OnAddedRemovedToTurret(online);
		}
		if (online != IsOn() && Interface.CallHook("OnTurretToggle", this) == null)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, online);
			}
			OnTurretPowerChanged(online);
			booting = false;
			BaseProjectile attachedWeapon2 = GetAttachedWeapon();
			if (Object.op_Implicit((Object)(object)attachedWeapon2))
			{
				attachedWeapon2.SetLightsOn(online);
			}
			SendNetworkUpdate();
			if (IsOffline())
			{
				ResetBurstFireState();
				SetTarget(null);
				isLootable = true;
			}
			else
			{
				isLootable = false;
				authDirty = true;
			}
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int result = Mathf.Min(1, GetCurrentEnergy());
		switch (outputSlot)
		{
		case 0:
			if (!HasTarget())
			{
				return 0;
			}
			return result;
		case 1:
			if (totalAmmo > 50)
			{
				return 0;
			}
			return result;
		case 2:
			if (totalAmmo != 0)
			{
				return 0;
			}
			return result;
		default:
			return 0;
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (IsPowered() && !IsOn())
		{
			InitiateStartup();
		}
		else if ((!IsPowered() && IsOn()) || booting)
		{
			InitiateShutdown();
		}
	}

	public void InitiateShutdown()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((!IsOffline() || booting) && Interface.CallHook("OnTurretShutdown", this) == null)
		{
			CancelInvoke(actionSetOnline);
			booting = false;
			Effect.server.Run(offlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			SetIsOnline(online: false);
		}
	}

	public void InitiateStartup()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnline() && !booting && Interface.CallHook("OnTurretStartup", this) == null)
		{
			Effect.server.Run(onlineSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			Invoke(actionSetOnline, 2f);
			booting = true;
		}
	}

	public void SetPeacekeepermode(bool isOn)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (PeacekeeperMode() != isOn)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, isOn);
			}
			Effect.server.Run(peacekeeperToggleSound.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
	}

	public static bool IsValidWeapon(ItemDefinition itemDef)
	{
		ItemModEntity component = ((Component)itemDef).GetComponent<ItemModEntity>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if ((Object)(object)component2 == (Object)null)
		{
			return false;
		}
		if (!component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}

	public bool IsValidWeapon(Item item)
	{
		if (item.isBroken)
		{
			return false;
		}
		return IsValidWeapon(item.info);
	}

	public bool CanAcceptItem(Item item, int targetSlot)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Item slot = base.inventory.GetSlot(0);
		if (IsValidWeapon(item) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			ItemModProjectile component = ((Component)item.info).GetComponent<ItemModProjectile>();
			BaseProjectile attachedWeapon = GetAttachedWeapon();
			if (slot == null || (Object)(object)attachedWeapon == (Object)null || (Object)(object)component == (Object)null)
			{
				return false;
			}
			if ((attachedWeapon.primaryMagazine.definition.ammoTypes & component.ammoType) == 0)
			{
				return false;
			}
			if (targetSlot == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved4);
	}

	public void UpdateMaxAuthCapacity()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (authorizedPlayers.Count >= 200)
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		bool b = (Object)(object)activeGameMode != (Object)null && activeGameMode.limitTeamAuths && authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize();
		flagsUpdateScope.Set(Flags.Reserved4, b);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void FlipAim(RPCMessage rpc)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOnline() && IsAuthed(rpc.player) && !booting && Interface.CallHook("OnTurretRotate", this, rpc.player) == null)
		{
			((Component)this).transform.rotation = Quaternion.LookRotation(-((Component)this).transform.forward, ((Component)this).transform.up);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		AddSelfAuthorize(rpc.player);
	}

	public void AddSelfAuthorize(BasePlayer player)
	{
		if (!IsOnline() && player.CanBuild() && !AtMaxAuthCapacity() && Interface.CallHook("OnTurretAuthorize", this, player) == null)
		{
			authorizedPlayers.Add(player.userID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, player, authorizedPlayers, "added", player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (!booting && !IsOnline() && IsAuthed(rpc.player) && Interface.CallHook("OnTurretDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.Remove(rpc.player.userID);
			authDirty = true;
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "removed", rpc.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ClearList(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && !booting && !IsOnline() && player.CanBuild() && Interface.CallHook("OnTurretClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			authDirty = true;
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc.player, authorizedPlayers, "clear", rpc.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AssignToFriend(RPCMessage msg)
	{
		if (!AtMaxAuthCapacity() && !((Object)(object)msg.player == (Object)null) && msg.player.CanInteract() && CanChangeSettings(msg.player))
		{
			ulong num = msg.read.UInt64();
			if (num != 0L && !IsAuthed(num) && Interface.CallHook("OnTurretAssign", this, num, msg.player) == null)
			{
				Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, msg.player, authorizedPlayers, "added", num);
				authorizedPlayers.Add(num);
				UpdateMaxAuthCapacity();
				SendNetworkUpdate();
				Interface.CallHook("OnTurretAssigned", this, num, msg.player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SERVER_Peacekeeper(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player) && Interface.CallHook("OnTurretModeToggle", this, rpc.player) == null)
		{
			SetPeacekeepermode(isOn: true);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SERVER_AttackAll(RPCMessage rpc)
	{
		if (IsAuthed(rpc.player) && Interface.CallHook("OnTurretModeToggle", this, rpc.player) == null)
		{
			SetPeacekeepermode(isOn: false);
		}
	}

	public virtual float TargetScanRate()
	{
		return 1f;
	}

	public override void ServerInit()
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		if ((Object)(object)targetTrigger != (Object)null)
		{
			TargetTrigger obj = targetTrigger;
			obj.OnEntityEnterTrigger = (Action<BaseNetworkable>)Delegate.Combine(obj.OnEntityEnterTrigger, new Action<BaseNetworkable>(OnEntityEnterTrigger));
			((Component)targetTrigger).GetComponent<SphereCollider>().radius = sightRange;
		}
		InvokeRepeating(actionServerThink, Random.Range(0f, 1f), 0.2f);
		InvokeRandomized(actionServerDo, Random.Range(0f, 1f), 0.03f, 0.05f);
		InvokeRandomized(actionSendAimDir, Random.Range(0f, 1f), 0.2f, 0.05f);
		((PersistentObjectWorkQueue<AutoTurret>)updateAutoTurretScanQueue).Add(this);
		if (ShouldApplyInterference())
		{
			ServerInit_Interference();
		}
		cachedTransf = ((Component)this).transform;
		rotateMode = ((gun_pitch.localPosition.x == 0f && gun_pitch.localPosition.z == 0f) ? YawPitchMode.Merged : YawPitchMode.Separate);
		if (rotateMode == YawPitchMode.Merged)
		{
			toPitchFromRootOrYaw = cachedTransf.worldToLocalMatrix * gun_pitch.localToWorldMatrix;
			gunAimInitialPitchOrTotalRot = ((Matrix4x4)(ref toPitchFromRootOrYaw)).rotation;
		}
		else
		{
			toYawFromRoot = ((Component)this).transform.root.worldToLocalMatrix * gun_yaw.localToWorldMatrix;
			gunAimInitialYawRot = ((Matrix4x4)(ref toYawFromRoot)).rotation;
			toPitchFromRootOrYaw = gun_yaw.worldToLocalMatrix * gun_pitch.localToWorldMatrix;
			gunAimInitialPitchOrTotalRot = ((Matrix4x4)(ref toPitchFromRootOrYaw)).rotation;
			gunAimTotalRotWS = gun_pitch.rotation;
		}
		if (Object.op_Implicit((Object)(object)RCEyes))
		{
			toRCEyesFromPitch = gun_pitch.worldToLocalMatrix * RCEyes.localToWorldMatrix;
		}
		if (ShouldApplyInterference())
		{
			ServerInit_Interference();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<AutoTurret>)updateAutoTurretScanQueue).Remove(this);
		if (ShouldApplyInterference())
		{
			ServerDestroy_Interference();
		}
	}

	public void OnEntityEnterTrigger(BaseNetworkable entity)
	{
		if (entity is BasePlayer player && !IsAuthed(player))
		{
			authDirty = true;
		}
	}

	public void SendAimDir()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((net == null || net.group == null || BaseNetworkable.HasConnections(net.group, ((Component)this).transform.position)) && (HasTarget() || Vector3.Angle(lastSentAimDir, aimDir) > 1f))
		{
			SendAimDirImmediate();
		}
	}

	public void SendAimDirImmediate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		lastSentAimDir = aimDir;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_ReceiveAimDir"), aimDir);
	}

	public void SetTarget(BaseCombatEntity targ)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnTurretTarget", this, targ) != null)
		{
			return;
		}
		if ((Object)(object)targ != (Object)(object)target || targ.IsRealNull() != target.IsRealNull())
		{
			Effect.server.Run(((Object)(object)targ == (Object)null) ? targetLostEffect.resourcePath : targetAcquiredEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			if (outputs != null && outputs.Length != 0 && (Object)(object)outputs[0].connectedTo.Get() != (Object)null)
			{
				MarkDirtyForceUpdateOutputs();
			}
			nextShotTime += 0.10000000149011612;
			authDirty = true;
		}
		target = targ;
		if (target.IsRealNull())
		{
			targetVisible = false;
			nextVisCheck = 0.0;
		}
		else
		{
			OnTargetSeen(target, Time.realtimeSinceStartupAsDouble);
		}
	}

	private void OnTargetSeen(BaseCombatEntity seenTarget, double realtimeSinceStartupAsDouble)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		lastTargetSeenTime = realtimeSinceStartupAsDouble;
		lastTargetAimOffset = AimOffset(seenTarget);
	}

	public virtual bool CheckPeekers()
	{
		return true;
	}

	public bool ObjectVisible(BaseCombatEntity obj)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		object obj2 = Interface.CallHook("CanBeTargeted", obj, this);
		if (obj2 is bool)
		{
			return (bool)obj2;
		}
		Vector3 position = ((Component)eyePos).transform.position;
		if (GamePhysics.CheckSphere(position, 0.1f, 136314880, (QueryTriggerInteraction)0))
		{
			return false;
		}
		Vector3 val = AimOffset(obj);
		float num = Vector3.Distance(val, position);
		Vector3 val2 = val - position;
		Vector3 val3 = Vector3.Cross(((Vector3)(ref val2)).normalized, Vector3.up);
		if (num > sightRange)
		{
			return false;
		}
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		int num2 = ((!(obj is BasePlayer)) ? 1 : 3);
		for (int i = 0; i < num2; i++)
		{
			val2 = val + val3 * visibilityOffsets[i] - position;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			list.Clear();
			GamePhysics.TraceAll(new Ray(position, normalized), 0f, list, num * 1.1f, 1218652417, (QueryTriggerInteraction)0);
			for (int j = 0; j < list.Count; j++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(list[j]);
				if ((!((Object)(object)entity != (Object)null) || !entity.isClient) && (!((Object)(object)entity != (Object)null) || !((Object)(object)entity.ToPlayer() != (Object)null) || entity.EqualNetID((BaseNetworkable)obj)) && (!((Object)(object)entity != (Object)null) || !entity.EqualNetID((BaseNetworkable)this)))
				{
					if ((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)obj || entity.EqualNetID((BaseNetworkable)obj)))
					{
						Pool.FreeUnmanaged<RaycastHit>(ref list);
						peekIndex = i;
						return true;
					}
					if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
					{
						break;
					}
				}
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return false;
	}

	public virtual void FireAttachedGun(Vector3 targetPos, float aimCone, BaseCombatEntity target = null, float damageModifier = 1f, float speedModifier = 1f)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetAttachedWeapon(out var baseProjectile) && !IsOffline() && (!(baseProjectile is ITurretNotify turretNotify) || turretNotify.CanShoot()))
		{
			Matrix4x4 val = GetCenterMuzzle();
			if (IsBeingControlled)
			{
				val *= toRCEyesFromPitch;
			}
			baseProjectile.ServerUse(new HeldEntityServerUseParams(damageModifier, speedModifier, val, useBulletThickness: false));
		}
	}

	public virtual void FireGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		if (IsOffline())
		{
			return;
		}
		if ((Object)(object)muzzleToUse == (Object)null)
		{
			muzzleToUse = muzzlePos;
		}
		Matrix4x4 centerMuzzle = GetCenterMuzzle();
		Vector3 val = ((Matrix4x4)(ref centerMuzzle)).MultiplyVector(Vector3.forward);
		Vector3 val2 = ((Matrix4x4)(ref centerMuzzle)).GetPosition() - val * 0.25f;
		Vector3 val3 = val;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, val3);
		targetPos = val2 + modifiedAimConeDirection * 300f;
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(val2, modifiedAimConeDirection), 0f, list, 300f, 1220225809, (QueryTriggerInteraction)0);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			RaycastHit hit = list[i];
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (((Object)(object)entity != (Object)null && ((Object)(object)entity == (Object)(object)this || entity.EqualNetID((BaseNetworkable)this))) || (PeacekeeperMode() && (Object)(object)target != (Object)null && (Object)(object)entity != (Object)null && (Object)(object)((Component)entity).GetComponent<BasePlayer>() != (Object)null && !entity.EqualNetID((BaseNetworkable)target)))
			{
				continue;
			}
			BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
			if ((Object)(object)baseCombatEntity != (Object)null)
			{
				ApplyDamage(baseCombatEntity, ((RaycastHit)(ref hit)).point, modifiedAimConeDirection);
				if (baseCombatEntity.EqualNetID((BaseNetworkable)target))
				{
					flag = true;
				}
			}
			if (!((Object)(object)entity != (Object)null) || entity.ShouldBlockProjectiles())
			{
				targetPos = ((RaycastHit)(ref hit)).point;
				Vector3 val4 = targetPos - val2;
				val3 = ((Vector3)(ref val4)).normalized;
				break;
			}
		}
		int num = 2;
		if (!flag)
		{
			numConsecutiveMisses++;
		}
		else
		{
			numConsecutiveMisses = 0;
		}
		if ((Object)(object)target != (Object)null && targetVisible && numConsecutiveMisses > num)
		{
			ApplyDamage(target, ((Component)target).transform.position - val3 * 0.25f, val3);
			numConsecutiveMisses = 0;
		}
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_FireGun"), StringPool.Get(((Object)((Component)muzzleToUse).gameObject).name), targetPos);
		Pool.FreeUnmanaged<RaycastHit>(ref list);
	}

	public void ApplyDamage(BaseCombatEntity entity, Vector3 point, Vector3 normal)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		float num = 15f * Random.Range(0.9f, 1.1f);
		if (entity is BasePlayer && (Object)(object)entity != (Object)(object)target)
		{
			num *= 0.5f;
		}
		if (PeacekeeperMode() && (Object)(object)entity == (Object)(object)target)
		{
			target.MarkHostileFor(300f);
		}
		HitInfo hitInfo = Pool.Get<HitInfo>();
		hitInfo.Initiator = this;
		hitInfo.HitEntity = entity;
		hitInfo.damageTypes.Add(DamageType.Bullet, num);
		hitInfo.HitPositionWorld = point;
		entity.OnAttacked(hitInfo);
		if (entity is BasePlayer || entity is BaseNpc)
		{
			hitInfo.HitNormalWorld = -normal;
			hitInfo.HitMaterial = StringPool.Get("Flesh");
			Effect.server.ImpactEffect(hitInfo);
		}
		Pool.Free<HitInfo>(ref hitInfo);
	}

	public void IdleTick(float dt)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if (Object.op_Implicit((Object)(object)attachedWeapon) && attachedWeapon is ITurretNotify turretNotify)
		{
			turretNotify.WarmupTick(wantsShoot: false);
		}
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		if (realtimeSinceStartupAsDouble > nextIdleAimTime)
		{
			nextIdleAimTime = realtimeSinceStartupAsDouble + (double)Random.Range(4f, 5f);
			Quaternion val = Quaternion.LookRotation(((Component)this).transform.forward, Vector3.up);
			val *= Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.up);
			targetAimDir = val * Vector3.forward;
		}
		if (!HasTarget())
		{
			aimDir = Mathx.Lerp(aimDir, targetAimDir, 2f, dt);
		}
	}

	public virtual bool HasClipAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((Object)(object)attachedWeapon == (Object)null)
		{
			return false;
		}
		return attachedWeapon.primaryMagazine.contents > 0;
	}

	public virtual bool HasReserveAmmo()
	{
		return totalAmmo > 0;
	}

	public int GetTotalAmmo()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((Object)(object)attachedWeapon == (Object)null)
		{
			return num;
		}
		List<Item> ammos = Pool.Get<List<Item>>();
		base.inventory.FindAmmo(ammos, attachedWeapon.primaryMagazine.definition.ammoTypes);
		if (!attachedWeapon.primaryMagazine.allowAmmoSwitching)
		{
			BaseProjectile.StripAmmoToType(ref ammos, attachedWeapon.primaryMagazine.ammoType);
		}
		for (int i = 0; i < ammos.Count; i++)
		{
			num += ammos[i].amount;
		}
		Pool.Free<Item>(ref ammos, false);
		return num;
	}

	public AmmoTypes GetValidAmmoTypes()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((Object)(object)attachedWeapon == (Object)null)
		{
			return (AmmoTypes)2;
		}
		return attachedWeapon.primaryMagazine.definition.ammoTypes;
	}

	public ItemDefinition GetDesiredAmmo()
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((Object)(object)attachedWeapon == (Object)null)
		{
			return null;
		}
		return attachedWeapon.primaryMagazine.ammoType;
	}

	public void Reload()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetAttachedWeapon(out var baseProjectile))
		{
			return;
		}
		ResetBurstFireState();
		_ = baseProjectile.primaryMagazine.ammoType;
		float turretReloadDuration = baseProjectile.GetTurretReloadDuration();
		nextShotTime = math.max(nextShotTime, Time.timeAsDouble + (double)Mathf.Min(turretReloadDuration, 2f));
		AmmoTypes ammoTypes = baseProjectile.primaryMagazine.definition.ammoTypes;
		if (baseProjectile.primaryMagazine.contents > 0)
		{
			bool flag = false;
			if (base.inventory.capacity > base.inventory.itemList.Count)
			{
				flag = true;
			}
			else
			{
				int num = 0;
				foreach (Item item in base.inventory.itemList)
				{
					if ((Object)(object)item.info == (Object)(object)baseProjectile.primaryMagazine.ammoType)
					{
						num += item.MaxStackable() - item.amount;
					}
				}
				flag = num >= baseProjectile.primaryMagazine.contents;
			}
			if (!flag)
			{
				return;
			}
			base.inventory.AddItem(baseProjectile.primaryMagazine.ammoType, baseProjectile.primaryMagazine.contents, 0uL);
			baseProjectile.SetAmmoCount(0);
		}
		List<Item> ammos = Pool.Get<List<Item>>();
		base.inventory.FindAmmo(ammos, ammoTypes);
		if (!baseProjectile.primaryMagazine.allowAmmoSwitching)
		{
			BaseProjectile.StripAmmoToType(ref ammos, baseProjectile.primaryMagazine.ammoType);
		}
		if (ammos.Count > 0)
		{
			Effect.server.Run(reloadEffect.resourcePath, this, StringPool.Get("WeaponAttachmentPoint"), Vector3.zero, Vector3.zero);
			totalAmmoDirty = true;
			baseProjectile.primaryMagazine.ammoType = ammos[0].info;
			int num2 = 0;
			while (baseProjectile.primaryMagazine.contents < baseProjectile.primaryMagazine.capacity && num2 < ammos.Count)
			{
				if ((Object)(object)ammos[num2].info == (Object)(object)baseProjectile.primaryMagazine.ammoType)
				{
					int num3 = baseProjectile.primaryMagazine.capacity - baseProjectile.primaryMagazine.contents;
					num3 = Mathf.Min(ammos[num2].amount, num3);
					ammos[num2].UseItem(num3);
					baseProjectile.ModifyAmmoCount(num3);
				}
				num2++;
			}
		}
		ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
		if (Object.op_Implicit((Object)(object)ammoType))
		{
			ItemModProjectile component = ((Component)ammoType).GetComponent<ItemModProjectile>();
			GameObject val = component.GetOverrideProjectile(baseProjectile).Get();
			if (Object.op_Implicit((Object)(object)val))
			{
				if (Object.op_Implicit((Object)(object)val.GetComponent<Projectile>()))
				{
					currentAmmoGravity = 0f;
					currentAmmoVelocity = component.GetMaxVelocity();
				}
				else
				{
					ServerProjectile component2 = val.GetComponent<ServerProjectile>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						currentAmmoGravity = component2.gravityModifier;
						currentAmmoVelocity = component2.speed;
					}
				}
			}
		}
		Pool.Free<Item>(ref ammos, false);
		baseProjectile.SendNetworkUpdate();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		totalAmmoDirty = true;
		Reload();
		if (ShouldApplyInterference())
		{
			PostServerLoad_Interference();
		}
	}

	public void UpdateTotalAmmo()
	{
		int num = totalAmmo;
		totalAmmo = GetTotalAmmo();
		if (num != totalAmmo && ((Object)(object)outputs[1].connectedTo.Get() != (Object)null || (Object)(object)outputs[2].connectedTo.Get() != (Object)null))
		{
			MarkDirtyForceUpdateOutputs();
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (Object.op_Implicit((Object)(object)((Component)item.info).GetComponent<ItemModEntity>()))
		{
			if (IsInvoking(actionUpdateAttachedWeapon))
			{
				UpdateAttachedWeapon();
			}
			Invoke(actionUpdateAttachedWeapon, 0.5f);
		}
	}

	public bool EnsureReloaded(bool onlyReloadIfEmpty = true)
	{
		bool flag = HasReserveAmmo();
		if (onlyReloadIfEmpty)
		{
			if (flag && !HasClipAmmo())
			{
				Reload();
				return true;
			}
		}
		else if (flag)
		{
			Reload();
			return true;
		}
		return false;
	}

	public BaseProjectile GetAttachedWeapon()
	{
		return AttachedWeapon as BaseProjectile;
	}

	public bool TryGetAttachedWeapon(out BaseProjectile baseProjectile)
	{
		baseProjectile = GetAttachedWeapon();
		return (Object)(object)baseProjectile != (Object)null;
	}

	public virtual bool HasFallbackWeapon()
	{
		return false;
	}

	public bool HasGenericFireable()
	{
		if ((Object)(object)AttachedWeapon != (Object)null)
		{
			return AttachedWeapon.IsInstrument();
		}
		return false;
	}

	public void UpdateAttachedWeapon()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		ResetBurstFireState();
		HeldEntity heldEntity = TryAddWeaponToTurret(base.inventory.GetSlot(0), socketTransform, this, attachedWeaponZOffsetScale);
		bool flag = (Object)(object)heldEntity != (Object)null;
		flagsUpdateScope.Set(Flags.Reserved3, flag);
		if (flag)
		{
			AttachedWeapon = heldEntity;
			totalAmmoDirty = true;
			Reload();
			UpdateTotalAmmo();
			if (IsOffline())
			{
				heldEntity.SetLightsOn(isOn: false);
			}
		}
		else
		{
			BaseProjectile attachedWeapon = GetAttachedWeapon();
			if ((Object)(object)attachedWeapon != (Object)null)
			{
				attachedWeapon.SetGenericVisible(wantsVis: false);
				attachedWeapon.SetLightsOn(isOn: false);
				if (attachedWeapon is ITurretNotify turretNotify)
				{
					turretNotify.OnAddedRemovedToTurret(added: false);
				}
			}
			AttachedWeapon = null;
		}
		bool b = false;
		if (flag)
		{
			BaseProjectile component = ((Component)heldEntity).GetComponent<BaseProjectile>();
			b = (Object)(object)component != (Object)null && component.largeTurretWeapon;
		}
		flagsUpdateScope.Set(Flags.Reserved5, b);
	}

	public static HeldEntity TryAddWeaponToTurret(Item weaponItem, Transform parent, BaseEntity entityParent, float zOffsetScale)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		HeldEntity heldEntity = null;
		if (weaponItem != null && (weaponItem.info.category == ItemCategory.Weapon || weaponItem.info.category == ItemCategory.Fun))
		{
			BaseEntity heldEntity2 = weaponItem.GetHeldEntity();
			if ((Object)(object)heldEntity2 != (Object)null)
			{
				HeldEntity component = ((Component)heldEntity2).GetComponent<HeldEntity>();
				if ((Object)(object)component != (Object)null && component.IsUsableByTurret)
				{
					heldEntity = component;
				}
			}
		}
		if ((Object)(object)heldEntity == (Object)null)
		{
			return null;
		}
		Transform transform = ((Component)heldEntity).transform;
		Transform muzzleTransform = heldEntity.MuzzleTransform;
		heldEntity.SetParent(null);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		Quaternion val = transform.rotation * Quaternion.Inverse(muzzleTransform.rotation);
		heldEntity.limitNetworking = false;
		using (FlagsUpdateScope flagsUpdateScope = heldEntity.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		heldEntity.SetParent(entityParent, StringPool.Get(((Object)parent).name));
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.rotation *= val;
		Vector3 val2 = parent.InverseTransformPoint(muzzleTransform.position);
		transform.localPosition = Vector3.left * val2.x;
		float num = Vector3.Distance(muzzleTransform.position, transform.position);
		transform.localPosition += Vector3.forward * num * zOffsetScale;
		heldEntity.SetGenericVisible(wantsVis: true);
		heldEntity.SetLightsOn(isOn: true);
		if (heldEntity is ITurretNotify turretNotify)
		{
			turretNotify.OnAddedRemovedToTurret(added: true);
		}
		return heldEntity;
	}

	public override void OnDied(HitInfo info)
	{
		BaseProjectile attachedWeapon = GetAttachedWeapon();
		if ((Object)(object)attachedWeapon != (Object)null)
		{
			attachedWeapon.SetGenericVisible(wantsVis: false);
			attachedWeapon.SetLightsOn(isOn: false);
			if (attachedWeapon is ITurretNotify turretNotify)
			{
				turretNotify.OnAddedRemovedToTurret(added: false);
			}
		}
		AttachedWeapon = null;
		base.OnDied(info);
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (!IsAuthed(baseEntity))
		{
			return false;
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		UpdateTotalAmmo();
		EnsureReloaded(onlyReloadIfEmpty: false);
		UpdateTotalAmmo();
		nextShotTime = Time.timeAsDouble;
	}

	public virtual float GetMaxAngleForEngagement()
	{
		return 1f;
	}

	private void ResetBurstFireState()
	{
		currentBurstShotsFired = 0;
	}

	private bool IsBursting(BaseProjectile attached)
	{
		if (attached.primaryMagazine.contents <= 0 || !IsUsingBurstFireWeapon(attached))
		{
			return false;
		}
		return currentBurstShotsFired != 0;
	}

	private bool IsUsingBurstFireWeapon()
	{
		if (!TryGetAttachedWeapon(out var baseProjectile))
		{
			return false;
		}
		return IsUsingBurstFireWeapon(baseProjectile);
	}

	private bool IsUsingBurstFireWeapon(BaseProjectile attached)
	{
		return attached.IsBurstModeOnly();
	}

	public void TargetTick()
	{
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AutoTurret.ServerTick.TickCycle.TargetTick"))
		{
			double timeAsDouble = Time.timeAsDouble;
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			if (realtimeSinceStartupAsDouble >= nextVisCheck)
			{
				nextVisCheck = realtimeSinceStartupAsDouble + (double)Random.Range(0.2f, 0.3f);
				targetVisible = ObjectVisible(target);
				if (targetVisible)
				{
					OnTargetSeen(target, realtimeSinceStartupAsDouble);
				}
			}
			bool flag = TryGetAttachedWeapon(out var baseProjectile);
			bool flag2 = flag && IsBursting(baseProjectile);
			EnsureReloaded();
			if (!(timeAsDouble >= nextShotTime) || !(targetVisible | flag2) || (!flag2 && !(Mathf.Abs(AngleToTarget(target, currentAmmoGravity != 0f)) < GetMaxAngleForEngagement())))
			{
				return;
			}
			if (flag)
			{
				if (baseProjectile is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: true);
				}
				float damageModifier = 1f;
				float speedModifier = 1f;
				ItemDefinition ammoType = baseProjectile.primaryMagazine.ammoType;
				if (Object.op_Implicit((Object)(object)ammoType))
				{
					ItemModProjectile component = ((Component)ammoType).GetComponent<ItemModProjectile>();
					if (Object.op_Implicit((Object)(object)component) && component.projectileVelocity < 100f)
					{
						speedModifier = 2f;
					}
				}
				if (baseProjectile.primaryMagazine.contents > 0)
				{
					if (!target.IsRealNull() && target.GetParentEntity() is TrainCar trainCar)
					{
						Vector3 worldVelocity = trainCar.GetWorldVelocity();
						float magnitude = ((Vector3)(ref worldVelocity)).magnitude;
						float num = Mathf.Pow(1f - TrainCar.TrainTurretInaccuratePerVelocity, magnitude);
						if (Random.Range(0f, 1f) > num)
						{
							damageModifier = 0f;
						}
					}
					if (!target.IsRealNull())
					{
						lastTargetAimOffset = AimOffset(target);
					}
					FireAttachedGun(lastTargetAimOffset, aimCone, PeacekeeperMode() ? target : null, damageModifier, speedModifier);
					float num2 = 0f;
					if (IsUsingBurstFireWeapon(baseProjectile))
					{
						int burstModeCount = baseProjectile.GetBurstModeCount();
						currentBurstShotsFired++;
						if (currentBurstShotsFired < burstModeCount && baseProjectile.primaryMagazine.contents > 0)
						{
							num2 = Mathf.Max(baseProjectile.ScaleRepeatDelay(baseProjectile.repeatDelay), baseProjectile.NextAttackTime - (float)timeAsDouble);
						}
						else
						{
							ResetBurstFireState();
							num2 = Mathf.Max(baseProjectile.TimeBetweenBursts(), 0.4f);
						}
					}
					else
					{
						ResetBurstFireState();
						num2 = (baseProjectile.isSemiAuto ? (baseProjectile.repeatDelay * 1.5f) : baseProjectile.repeatDelay);
						num2 = baseProjectile.ScaleRepeatDelay(num2);
					}
					nextShotTime = timeAsDouble + (double)num2;
					if ((float)nextShotTime < baseProjectile.NextAttackTime && !Mathf.Approximately((float)nextShotTime, baseProjectile.NextAttackTime))
					{
						Debug.LogWarning((object)string.Format("Turret {0} next shot scheduled in {1}s will be skipped due to it being less than attached {2} attack cooldown ({3}s).", new object[4]
						{
							((Object)this).name,
							num2,
							((Object)baseProjectile).name,
							baseProjectile.NextAttackTime - (float)timeAsDouble
						}), (Object)(object)this);
					}
					shouldUpdateOnOutOfAmmo = true;
					lastShotTime = timeAsDouble;
				}
				else
				{
					ResetBurstFireState();
					nextShotTime = timeAsDouble + 5.0;
					if (shouldUpdateOnOutOfAmmo)
					{
						shouldUpdateOnOutOfAmmo = false;
						baseProjectile.SendNetworkUpdate();
					}
				}
			}
			else if (HasFallbackWeapon())
			{
				FireGun(AimOffset(target), aimCone, null, target);
				nextShotTime = timeAsDouble + 0.11500000208616257;
				lastShotTime = timeAsDouble;
			}
			else if (HasGenericFireable())
			{
				AttachedWeapon.ServerUse();
				lastShotTime = timeAsDouble;
				nextShotTime = timeAsDouble + 0.11500000208616257;
			}
			else
			{
				nextShotTime = timeAsDouble + 1.0;
			}
		}
	}

	public bool HasTarget()
	{
		if ((Object)(object)target != (Object)null)
		{
			return target.IsAlive();
		}
		return false;
	}

	public void OfflineTick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		aimDir = Vector3.up;
	}

	public virtual bool IsEntityHostile(BaseCombatEntity ent)
	{
		if (ent is ScarecrowNPC)
		{
			return true;
		}
		if (ent is BasePet basePet && (Object)(object)basePet.Brain.OwningPlayer != (Object)null)
		{
			if (!basePet.Brain.OwningPlayer.IsHostile())
			{
				return ent.IsHostile();
			}
			return true;
		}
		return ent.IsHostile();
	}

	public bool ShouldTarget(BaseCombatEntity targ)
	{
		if (targ is AutoTurret)
		{
			return false;
		}
		if (targ is RidableHorse)
		{
			return false;
		}
		if (targ is BasePet basePet && (Object)(object)basePet.Brain.OwningPlayer != (Object)null && IsAuthed(basePet.Brain.OwningPlayer))
		{
			return false;
		}
		if (targ is Drone drone)
		{
			if (!drone.IsBeingControlled)
			{
				return false;
			}
			if (IsAuthed(drone.OwnerID))
			{
				return false;
			}
			if (!drone.IsHostile())
			{
				return false;
			}
		}
		return true;
	}

	public void ScheduleForTargetScan()
	{
		((PersistentObjectWorkQueue<AutoTurret>)updateAutoTurretScanQueue).Add(this);
	}

	public void TargetScan()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		if (!target.IsRealNull())
		{
			double num = realtimeSinceStartupAsDouble - lastTargetSeenTime;
			double num2 = realtimeSinceStartupAsDouble - lastDamageEventTime;
			if ((Object)(object)target == (Object)null || target.IsDead() || (num > 3.0 && num2 > 3.0) || Vector3.Distance(((Component)this).transform.position, ((Component)target).transform.position) > sightRange || (PeacekeeperMode() && !IsEntityHostile(target)))
			{
				SetTarget(null);
			}
		}
		if (HasInterference())
		{
			if (HasTarget())
			{
				SetTarget(null);
			}
		}
		else
		{
			if (HasTarget() || IsOffline() || IsBeingControlled || aimDir == Vector3.up)
			{
				return;
			}
			bool flag = (Object)(object)targetTrigger != (Object)null && targetTrigger.entityContents != null && !CollectionEx.IsEmpty(targetTrigger.entityContents) && realtimeSinceStartupAsDouble - lastScanTime >= (double)Sentry.scantimer;
			if (!authDirty && !flag)
			{
				return;
			}
			authDirty = false;
			lastScanTime = realtimeSinceStartupAsDouble;
			if ((Object)(object)targetTrigger != (Object)null && targetTrigger.entityContents != null)
			{
				foreach (BaseEntity entityContent in targetTrigger.entityContents)
				{
					BaseCombatEntity baseCombatEntity = entityContent as BaseCombatEntity;
					if ((Object)(object)baseCombatEntity == (Object)null)
					{
						continue;
					}
					if (!Sentry.targetall)
					{
						BasePlayer basePlayer = baseCombatEntity as BasePlayer;
						if ((Object)(object)basePlayer != (Object)null && (IsAuthed(basePlayer) || Ignore(basePlayer)))
						{
							continue;
						}
					}
					if ((!PeacekeeperMode() || IsEntityHostile(baseCombatEntity)) && baseCombatEntity.IsAlive() && ShouldTarget(baseCombatEntity) && InFiringArc(baseCombatEntity) && ObjectVisible(baseCombatEntity))
					{
						SetTarget(baseCombatEntity);
						if (target != null)
						{
							break;
						}
					}
				}
			}
			if (PeacekeeperMode() && (Object)(object)target == (Object)null)
			{
				nextShotTime = Time.timeAsDouble + 1.0;
			}
		}
	}

	public virtual bool Ignore(BasePlayer player)
	{
		return false;
	}

	public void ServerDo()
	{
		if (!base.isClient && !base.IsDestroyed)
		{
			float dt = (float)(double)timeSinceLastServerTick;
			timeSinceLastServerTick = 0.0;
			UpdateFacingToTarget(dt);
		}
	}

	public void ServerThink()
	{
		if (!base.isClient && !base.IsDestroyed)
		{
			((ObjectWorkQueue<AutoTurret>)updateTurretTick).Add(this);
			if (totalAmmoDirty && Time.timeAsDouble > nextAmmoCheckTime)
			{
				((ObjectWorkQueue<AutoTurret>)updateAutoTurretAmmoQueue).Add(this);
				totalAmmoDirty = false;
				nextAmmoCheckTime = Time.timeAsDouble + 0.5;
			}
		}
	}

	public override void OnNetworkGroupEnter(Group group)
	{
		base.OnNetworkGroupEnter(group);
	}

	public void RunScheduledTick()
	{
		if (lastTickTime == 0f)
		{
			lastTickTime = Time.time;
		}
		if (!IsOnline())
		{
			OfflineTick();
		}
		else if (!IsBeingControlled)
		{
			if (HasTarget())
			{
				TargetTick();
			}
			else
			{
				float dt = Time.time - lastTickTime;
				IdleTick(dt);
			}
		}
		lastTickTime = Time.time;
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (((IsOnline() && !HasTarget()) || !targetVisible) && !((Object)(object)(info.Initiator as AutoTurret) != (Object)null) && !((Object)(object)(info.Initiator as SamSite) != (Object)null) && !((Object)(object)(info.Initiator as GunTrap) != (Object)null))
		{
			BasePlayer basePlayer = info.Initiator as BasePlayer;
			if (!Object.op_Implicit((Object)(object)basePlayer) || !IsAuthed(basePlayer))
			{
				SetTarget(info.Initiator as BaseCombatEntity);
				lastDamageEventTime = Time.realtimeSinceStartupAsDouble;
			}
		}
	}

	public void UpdateFacingToTarget(float dt)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)null && targetVisible && !IsBeingControlled)
		{
			using (TimeWarning.New("AutoTurret.ServerTick.UpdateFacing"))
			{
				Vector3 val = AimOffset(target);
				Vector3 position = eyePos.position;
				Vector3 val3;
				if (peekIndex != 0)
				{
					Vector3 val2 = position;
					Vector3.Distance(val, val2);
					val3 = val - val2;
					Vector3 val4 = Vector3.Cross(((Vector3)(ref val3)).normalized, Vector3.up);
					val += val4 * visibilityOffsets[peekIndex];
				}
				val3 = val - position;
				Vector3 val5 = ((Vector3)(ref val3)).normalized;
				if (currentAmmoGravity != 0f)
				{
					float num = 0.2f;
					if (target is BasePlayer)
					{
						float num2 = Mathf.Clamp01(target.WaterFactor()) * 1.8f;
						if (num2 > num)
						{
							num = num2;
						}
					}
					val = ((Component)target).transform.position + Vector3.up * num;
					float angle = GetAngle(position, val, currentAmmoVelocity, currentAmmoGravity);
					Vector3 val6 = Vector3Ex.XZ3D(val) - Vector3Ex.XZ3D(position);
					val6 = ((Vector3)(ref val6)).normalized;
					val5 = Quaternion.LookRotation(val6) * Quaternion.Euler(angle, 0f, 0f) * Vector3.forward;
				}
				aimDir = val5;
			}
		}
		UpdateAiming(dt);
	}

	public float GetAngle(Vector3 launchPosition, Vector3 targetPosition, float launchVelocity, float gravityScale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		float num = Physics.gravity.y * gravityScale;
		float num2 = Vector3.Distance(Vector3Ex.XZ3D(launchPosition), Vector3Ex.XZ3D(targetPosition));
		float num3 = launchPosition.y - targetPosition.y;
		float num4 = Mathf.Pow(launchVelocity, 2f);
		float num5 = Mathf.Pow(launchVelocity, 4f);
		float num6 = Mathf.Atan((num4 + Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		float num7 = Mathf.Atan((num4 - Mathf.Sqrt(num5 - num * (num * Mathf.Pow(num2, 2f) + 2f * num3 * num4))) / (num * num2)) * 57.29578f;
		if (float.IsNaN(num6) && float.IsNaN(num7))
		{
			return -45f;
		}
		if (float.IsNaN(num6))
		{
			return num7;
		}
		if (!(num6 > num7))
		{
			return num7;
		}
		return num6;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AddSelfAuthorize(deployedBy);
	}

	public override ItemContainerId GetIdealContainer(BasePlayer player, Item item, ItemMoveModifier modifier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return default(ItemContainerId);
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		bool num = item.info.category == ItemCategory.Weapon;
		bool flag = item.info.category == ItemCategory.Ammunition;
		if (num)
		{
			return 0;
		}
		if (flag)
		{
			for (int i = 1; i < base.inventory.capacity; i++)
			{
				if (!base.inventory.SlotTaken(item, i))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		if (!IsAuthed(player))
		{
			SprayCan.LastReskinError = SprayCan.NotAuthorized;
			return false;
		}
		return base.CanBeRedirectSwapped(player);
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref AutoTurretPreserveInfo autoTurretPreserve = ref preserveInfo.autoTurretPreserve;
		autoTurretPreserve.authedPlayers = Pool.Get<List<ulong>>();
		autoTurretPreserve.authedPlayers.AddRange(authorizedPlayers);
		autoTurretPreserve.isPeacekeeper = HasFlag(Flags.Reserved1);
		autoTurretPreserve.rcIdentifier = GetIdentifier();
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ref IItemContainerEntity.ContainerPreserveInfo containerPreserve = ref preserveInfo.containerPreserve;
		IItemContainerEntity.ContainerSet key = new IItemContainerEntity.ContainerSet
		{
			ContainerIndex = -1,
			PrefabId = 0u
		};
		List<Item> list = containerPreserve.storageDict[key];
		foreach (Item item in list)
		{
			if (IsValidWeapon(item))
			{
				item.MoveToContainer(base.inventory);
				AttachedWeapon = item.GetHeldEntity() as HeldEntity;
				break;
			}
		}
		foreach (Item item2 in list)
		{
			if (item2.info.category == ItemCategory.Ammunition)
			{
				item2.MoveToContainer(base.inventory);
			}
		}
		containerPreserve.storageDict[key].Clear();
		base.Reskin_Restore(ref preserveInfo);
		ref AutoTurretPreserveInfo autoTurretPreserve = ref preserveInfo.autoTurretPreserve;
		foreach (ulong authedPlayer in autoTurretPreserve.authedPlayers)
		{
			authorizedPlayers.Add(authedPlayer);
		}
		if (HasFlag(Flags.Reserved1) != autoTurretPreserve.isPeacekeeper)
		{
			SetFlagLocal(Flags.Reserved1, autoTurretPreserve.isPeacekeeper);
		}
		if (!string.IsNullOrEmpty(autoTurretPreserve.rcIdentifier) && GetIdentifier() != autoTurretPreserve.rcIdentifier)
		{
			UpdateIdentifier(autoTurretPreserve.rcIdentifier);
		}
		Pool.FreeUnmanaged<ulong>(ref autoTurretPreserve.authedPlayers);
	}

	public bool IsOnline()
	{
		return IsOn();
	}

	public bool IsOffline()
	{
		return !IsOnline();
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public virtual Matrix4x4 GetCenterMuzzle()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			Matrix4x4 localToWorldMatrix = cachedTransf.localToWorldMatrix;
			if (rotateMode == YawPitchMode.Separate)
			{
				return localToWorldMatrix * toYawFromRoot * Matrix4x4.Rotate(gunAimYawRotLS) * toPitchFromRootOrYaw * Matrix4x4.Rotate(gunAimPitchOrTotalRotLS);
			}
			return localToWorldMatrix * toPitchFromRootOrYaw * Matrix4x4.Rotate(gunAimPitchOrTotalRotLS);
		}
		return gun_pitch.localToWorldMatrix;
	}

	public float AngleToTarget(Vector3 targetPos, bool use2D = false)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		use2D = true;
		Matrix4x4 centerMuzzle = GetCenterMuzzle();
		Vector3 position = ((Matrix4x4)(ref centerMuzzle)).GetPosition();
		Vector3 zero = Vector3.zero;
		Vector3 val;
		if (use2D)
		{
			zero = Vector3Ex.Direction2D(targetPos, position);
		}
		else
		{
			val = targetPos - position;
			zero = ((Vector3)(ref val)).normalized;
		}
		Vector3 val2 = ((Matrix4x4)(ref centerMuzzle)).MultiplyVector(Vector3.forward);
		Vector3 val3;
		if (!use2D)
		{
			val3 = val2;
		}
		else
		{
			val = Vector3Ex.XZ3D(val2);
			val3 = ((Vector3)(ref val)).normalized;
		}
		return Vector3.Angle(val3, zero);
	}

	public float AngleToTarget(BaseCombatEntity potentialtarget, bool use2D = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Vector3 targetPos = AimOffset(potentialtarget);
		return AngleToTarget(targetPos, use2D);
	}

	public virtual bool InFiringArc(BaseCombatEntity potentialtarget)
	{
		return Mathf.Abs(AngleToTarget(potentialtarget)) <= 90f;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (IsAuthed(player))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (IsOnline())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemIsOnline, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.autoturret = Pool.Get<AutoTurret>();
		info.msg.autoturret.aimDir = aimDir;
		if (info.forDisk || IsAuthed(info.forConnection.userid))
		{
			info.msg.autoturret.users = Pool.Get<List<PlayerNameID>>();
			foreach (ulong authorizedPlayer in authorizedPlayers)
			{
				PlayerNameID val = Pool.Get<PlayerNameID>();
				val.userid = authorizedPlayer;
				info.msg.autoturret.users.Add(val);
			}
		}
		if (info.forDisk && ShouldApplyInterference())
		{
			Save_Interference(info.msg.autoturret);
		}
		if (info.forDisk)
		{
			info.msg.rcEntity = Pool.Get<RCEntity>();
			info.msg.rcEntity.identifier = GetIdentifier();
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.autoturret != null)
		{
			authorizedPlayers.Clear();
			if (info.msg.autoturret.users != null)
			{
				foreach (PlayerNameID user in info.msg.autoturret.users)
				{
					authorizedPlayers.Add(user.userid);
				}
			}
			info.msg.autoturret.users = null;
			aimDir = info.msg.autoturret.aimDir;
			if (base.isServer && ShouldApplyInterference())
			{
				Load_Interference(info.msg.autoturret);
			}
		}
		if (info.msg.rcEntity != null)
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public Vector3 AimOffset(BaseCombatEntity aimat)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = aimat as BasePlayer;
		if (!ObjectEx.IsUnityNull(basePlayer))
		{
			if (basePlayer.IsSleeping())
			{
				return ((Component)basePlayer).transform.position + Vector3.up * 0.1f;
			}
			if (basePlayer.IsWounded())
			{
				return ((Component)basePlayer).transform.position + Vector3.up * 0.25f;
			}
			if (basePlayer.TryGetActiveShield(out var foundShield) && foundShield.IsBlocking())
			{
				return ((Component)foundShield).transform.position;
			}
			if (!ObjectEx.IsUnityNull(basePlayer.eyes))
			{
				return basePlayer.eyes.position;
			}
			return basePlayer.GetCenter();
		}
		if (!ObjectEx.IsUnityNull(aimat))
		{
			return aimat.CenterPoint();
		}
		return Vector3.zero;
	}

	public float GetAimSpeed()
	{
		if (HasTarget())
		{
			return 5f;
		}
		return 1f;
	}

	public void UpdateAiming(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (aimDir == Vector3.zero)
		{
			return;
		}
		float num = 5f;
		if (base.isServer && !IsBeingControlled)
		{
			num = ((!HasTarget()) ? 15f : 35f);
		}
		Quaternion val = Quaternion.LookRotation(aimDir);
		if (!base.isServer)
		{
			return;
		}
		Quaternion rotation = cachedTransf.rotation;
		if (rotateMode == YawPitchMode.Merged)
		{
			Quaternion val2 = Quaternion.Inverse(rotation * gunAimInitialPitchOrTotalRot) * val;
			if (gunAimPitchOrTotalRotLS != val2)
			{
				gunAimPitchOrTotalRotLS = Mathx.Lerp(gunAimPitchOrTotalRotLS, val2, num, dt);
			}
		}
		else if (gunAimTotalRotWS != val)
		{
			gunAimTotalRotWS = Mathx.Lerp(gunAimTotalRotWS, val, num, dt);
			Quaternion val3 = Quaternion.Inverse(rotation * gunAimInitialYawRot) * gunAimTotalRotWS;
			Vector3 eulerAngles = ((Quaternion)(ref val3)).eulerAngles;
			gunAimYawRotLS = Quaternion.Euler(0f, eulerAngles.y, 0f);
			gunAimPitchOrTotalRotLS = Quaternion.Euler(eulerAngles.x, 0f, 0f);
		}
	}

	public bool IsAuthed(ulong id)
	{
		return authorizedPlayers.Contains(id);
	}

	public bool IsAuthed(BasePlayer player)
	{
		return IsAuthed(player.userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public virtual bool CanChangeSettings(BasePlayer player)
	{
		if (IsAuthed(player) && player.CanBuild())
		{
			return IsOffline();
		}
		return false;
	}

	bool IHostileWarningEntity.WarningEnabled(BaseEntity forEntity)
	{
		if (!IsPowered())
		{
			return false;
		}
		if (!PeacekeeperMode())
		{
			return false;
		}
		BasePlayer basePlayer = forEntity as BasePlayer;
		if ((Object)(object)basePlayer == (Object)null)
		{
			return false;
		}
		if (IsAuthed(basePlayer))
		{
			return false;
		}
		return true;
	}

	float IHostileWarningEntity.WarningRange()
	{
		return sightRange * 2f;
	}

	public AutoTurret()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		consumptionAmount = 10;
		bulletSpeed = 200f;
		playAmbientSounds = true;
		nearbyTurrets = new HashSet<AutoTurret>();
		interferringTurrets = new HashSet<AutoTurret>();
		rcTurnSensitivity = 4f;
		rcIdentifier = "";
		maxInterference = -1f;
		attachedWeaponZOffsetScale = -0.5f;
		lastTargetAimOffset = Vector3.zero;
		targetVisible = true;
		targetAimDir = Vector3.forward;
		lastSentAimDir = Vector3.forward;
		totalAmmoDirty = true;
		sightRange = 30f;
		focusSoundFreqMin = 2.5f;
		focusSoundFreqMax = 7f;
		authorizedPlayers = new HashSet<ulong>();
		base._002Ector();
	}
}
