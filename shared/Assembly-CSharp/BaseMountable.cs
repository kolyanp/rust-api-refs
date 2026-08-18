using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[ResetStaticFields]
public class BaseMountable : BaseCombatEntity
{
	public enum ClippingCheckLocation
	{
		HeadOnly,
		WholeBody
	}

	public enum DismountConvarType
	{
		Misc,
		Boating,
		Flying,
		GroundVehicle,
		Horse
	}

	public enum MountStatType
	{
		None,
		Boating,
		Flying,
		Driving
	}

	public enum MountGestureType
	{
		None,
		UpperBody
	}

	public enum MountSyncType
	{
		RepositionPerFrame,
		Parent,
		ParentAIOnly
	}

	public const float MountCheckRadius = 0.25f;

	public static Phrase dismountPhrase;

	[Header("Base Mountable")]
	public MountSyncType mountSyncType;

	[Header("View")]
	public Transform eyePositionOverride;

	public Transform eyeCenterOverride;

	public bool overrideEyesRotation;

	public Vector2 pitchClamp;

	public Vector2 yawClamp;

	public bool canWieldItems;

	public bool relativeViewAngles;

	public bool disableLegsWhenMounted;

	public bool disableBreastCensorshipWhenMounted;

	[Header("Mounting")]
	public bool AllowForceMountWhenRestrained;

	[Tooltip("Allow players to mount other mountables/ladders from this vehicle")]
	public bool mountChaining;

	public Transform mountAnchor;

	public float mountLOSVertOffset;

	[Tooltip("The speed of the posde animation for this mountable.")]
	[Header("Mount Pose")]
	[Range(0f, 1f)]
	public float mountedAnimationSpeed;

	public PlayerModel.MountPoses mountPose;

	[Tooltip("Toggles the vehicleAimYaw parameter update, only used by specific mountable like the rowboat and steering wheel.")]
	public bool animateVehicleAim360;

	[Space]
	public float maxMountDistance;

	public Transform[] dismountPositions;

	public bool checkPlayerLosOnMount;

	public bool disableMeshCullingForPlayers;

	public bool allowHeadLook;

	public bool ignoreVehicleParent;

	public bool legacyDismount;

	public ItemModWearable wearWhileMounted;

	public bool modifiesPlayerCollider;

	public BasePlayer.CapsuleColliderInfo customPlayerCollider;

	public float clippingCheckRadius;

	public bool clippingAndVisChecks;

	public ClippingCheckLocation clippingChecksLocation;

	public SoundDefinition mountSoundDef;

	public SoundDefinition swapSoundDef;

	public SoundDefinition dismountSoundDef;

	public bool allowFootstepEffects;

	public DismountConvarType dismountHoldType;

	[NonSerialized]
	private EntityRef _mountedRef;

	public MountStatType mountTimeStatType;

	public MountGestureType allowedGestures;

	public bool canDrinkWhileMounted;

	public bool allowSleeperMounting;

	public bool shouldShowHudHealth;

	[Help("Set this to true if the mountable is enclosed so it doesn't move inside cars and such")]
	public bool animateClothInLocalSpace;

	[SerializeField]
	private bool protectsFromAnimals;

	[Header("Camera")]
	public BasePlayer.CameraMode MountedCameraMode;

	[Header("Rigidbody (Optional)")]
	public Rigidbody rigidBody;

	public bool wantsBoundaryRepelCheck;

	[FormerlySerializedAs("needsVehicleTick")]
	public bool isMobile;

	public float SideLeanAmount;

	public const float playerHeight = 1.8f;

	public const float playerRadius = 0.5f;

	public BasePlayer _mounted;

	public static ListHashSet<BaseMountable> AllMountables;

	public static ListHashSet<BaseMountable> Mounted;

	[ServerVar(Help = "Toggles the usage of mountable MountedPlayerSync optimisations (only used by boat scientists currently)")]
	public static bool canPauseMountedPlayerSync;

	protected bool syncsMountedPlayers;

	public const float MOUNTABLE_TICK_RATE = 0.05f;

	public bool ProtectsFromAnimals
	{
		get
		{
			if ((Object)(object)((Component)this).transform.parent != (Object)null && BaseNetworkableEx.Is<BaseMountable>((Object)(object)((Component)((Component)this).transform.parent).GetComponent<BaseMountable>(), out BaseMountable castedUnityObject))
			{
				return castedUnityObject.ProtectsFromAnimals;
			}
			return protectsFromAnimals;
		}
	}

	public override float PositionTickRate
	{
		protected get
		{
			return 0.05f;
		}
	}

	public virtual bool IsSummerDlcVehicle => false;

	protected virtual bool BypassClothingMountBlocks => false;

	public virtual bool BlocksDoors => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMountable.OnRpcMessage"))
		{
			if (rpc == 1735799362 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_WantsDismount"));
				}
				using (TimeWarning.New("RPC_WantsDismount"))
				{
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
							RPC_WantsDismount(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_WantsDismount");
					}
				}
				return true;
			}
			if (rpc == 4014300952u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_WantsMount"));
				}
				using (TimeWarning.New("RPC_WantsMount"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4014300952u, "RPC_WantsMount", this, player, 3f))
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
							RPC_WantsMount(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_WantsMount");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool CanHoldItems()
	{
		return canWieldItems;
	}

	public virtual BasePlayer.CameraMode GetMountedCameraMode()
	{
		return MountedCameraMode;
	}

	public virtual bool DirectlyMountable()
	{
		return true;
	}

	public virtual Transform GetEyeOverride()
	{
		if ((Object)(object)eyePositionOverride != (Object)null)
		{
			return eyePositionOverride;
		}
		return ((Component)this).transform;
	}

	public virtual bool ModifiesThirdPersonCamera()
	{
		return false;
	}

	public virtual Vector2 GetPitchClamp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pitchClamp;
	}

	public virtual Vector2 GetYawClamp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return yawClamp;
	}

	public virtual bool AnyMounted()
	{
		return IsBusy();
	}

	public bool IsMounted()
	{
		return AnyMounted();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (base.isServer && !info.forDisk && Object.op_Implicit((Object)(object)_mounted) && _mounted.IsValid())
		{
			info.msg.baseMountable = Pool.Get<BaseMountable>();
			info.msg.baseMountable.mounted = _mounted.net.ID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public virtual BasePlayer GetMounted()
	{
		if (base.isServer)
		{
			return _mounted;
		}
		return null;
	}

	public virtual Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player.GetMounted() != (Object)(object)this)
		{
			return Vector3.zero;
		}
		return GetEyeOverride().position;
	}

	public virtual Quaternion EyeRotationForPlayer(BasePlayer player)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player.GetMounted() != (Object)(object)this || !overrideEyesRotation)
		{
			return Quaternion.identity;
		}
		return GetEyeOverride().rotation;
	}

	public virtual Vector3 EyeCenterForPlayer(BasePlayer player, Quaternion lookRot)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player.GetMounted() != (Object)(object)this)
		{
			return Vector3.zero;
		}
		return ((Component)eyeCenterOverride).transform.position;
	}

	public virtual float WaterFactorForPlayer(BasePlayer player, out WaterLevel.WaterInfo info)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		OBB val = player.WorldSpaceBounds();
		Bounds val2 = ((OBB)(ref val)).ToBounds();
		if (((Bounds)(ref val2)).size == Vector3.zero)
		{
			((Bounds)(ref val2)).size = new Vector3(0.1f, 0.1f, 0.1f);
		}
		info = WaterLevel.GetWaterInfo(val2, waves: true, volumes: true, this);
		return WaterLevel.Factor(in info, val2);
	}

	public override float AntiHackVelocity()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			return baseEntity.AntiHackVelocity();
		}
		return base.AntiHackVelocity();
	}

	public virtual bool PlayerIsMounted(BasePlayer player)
	{
		if (player.IsValid())
		{
			return (Object)(object)player.GetMounted() == (Object)(object)this;
		}
		return false;
	}

	public virtual BaseVehicle VehicleParent()
	{
		if (ignoreVehicleParent)
		{
			return null;
		}
		return GetParentEntity() as BaseVehicle;
	}

	public virtual bool HasValidDismountPosition(BasePlayer player)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		BaseVehicle baseVehicle = VehicleParent();
		if ((Object)(object)baseVehicle != (Object)null && !baseVehicle.childMountableHandleDismountPoints)
		{
			return baseVehicle.HasValidDismountPosition(player);
		}
		Transform[] array = dismountPositions;
		foreach (Transform val in array)
		{
			if (!((Object)(object)val == (Object)null) && ValidDismountPosition(player, ((Component)val).transform.position))
			{
				return true;
			}
		}
		return false;
	}

	protected virtual bool IgnoreChildEntitiesForDismountClipChecks()
	{
		return false;
	}

	protected virtual bool DismountCheckSkipVehicles()
	{
		return true;
	}

	public virtual bool ValidDismountPosition(BasePlayer player, Vector3 disPos)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		bool debugDismounts = Debugging.DebugDismounts;
		Vector3 dismountCheckStart = GetDismountCheckStart(player);
		if (debugDismounts)
		{
			Debug.Log((object)$"ValidDismountPosition debug: Checking dismount point {disPos} from {dismountCheckStart}.");
		}
		Vector3 start = disPos + new Vector3(0f, 0.5f, 0f);
		Vector3 end = disPos + new Vector3(0f, 1.3f, 0f);
		Collider col = null;
		if (!GamePhysics.CheckCapsule(base.isServer ? GamePhysics.Realm.Server : GamePhysics.Realm.Client, start, end, 0.5f, 1537286401, (QueryTriggerInteraction)0))
		{
			Vector3 position = disPos + ((Component)this).transform.up * 0.5f;
			if (IsVisibleAndCanSee(position))
			{
				Vector3 newPos = disPos + BasePlayer.NoClipOffset();
				if (debugDismounts)
				{
					Debug.Log((object)$"ValidDismountPosition debug: Dismount point {disPos} is visible.");
				}
				if (legacyDismount || !AntiHack.TestNoClipping(player, dismountCheckStart, newPos, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin_dismount), ConVar.AntiHack.noclip_backtracking, out col, overlapVehicleLayer: false, this, forceCast: false, IgnoreChildEntitiesForDismountClipChecks(), DismountCheckSkipVehicles()))
				{
					if (debugDismounts)
					{
						Debug.Log((object)$"<color=green>ValidDismountPosition debug: Dismount point {disPos} is valid</color>.");
					}
					return true;
				}
				if (debugDismounts)
				{
					Debug.Log((object)$"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid due to antihack</color>.");
				}
			}
			else if (debugDismounts)
			{
				Debug.Log((object)$"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid due to IsVisibleAndCanSee</color>.");
			}
		}
		if (debugDismounts && debugDismounts)
		{
			Debug.Log((object)$"<color=red>ValidDismountPosition debug: Dismount point {disPos} is invalid</color>", (Object)(object)col);
		}
		return false;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!AnyMounted())
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public void EnableMountedPlayerSync()
	{
		syncsMountedPlayers = true;
	}

	public void DisableMountedPlayerSync()
	{
		syncsMountedPlayers = false;
	}

	public virtual void MounteeTookDamage(BasePlayer mountee, HitInfo info)
	{
	}

	public virtual void LightToggle(BasePlayer player)
	{
	}

	public virtual void OnWeaponFired(BaseProjectile weapon)
	{
	}

	public virtual bool CanSwapToThis(BasePlayer player)
	{
		object obj = Interface.CallHook("CanSwapToSeat", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override void OnDied(HitInfo info)
	{
		DismountAllPlayers();
		base.OnDied(info);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_WantsMount(RPCMessage msg)
	{
		WantsMount(msg.player);
	}

	public void WantsMount(BasePlayer player)
	{
		if (!player.IsValid() || !player.CanInteract() || Interface.CallHook("OnPlayerWantsMount", player, this) != null)
		{
			return;
		}
		if (!DirectlyMountable())
		{
			BaseVehicle baseVehicle = VehicleParent();
			if ((Object)(object)baseVehicle != (Object)null && baseVehicle.IsVehicleMountPoint(this))
			{
				baseVehicle.WantsMount(player);
				return;
			}
		}
		AttemptMount(player);
	}

	public virtual void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_mounted != (Object)null || IsDead() || !player.CanMountMountablesNow() || IsTransferring() || IsSeatClipping(this) || ClothingBlocksMounting(player))
		{
			return;
		}
		if (doMountChecks)
		{
			if (checkPlayerLosOnMount)
			{
				Vector3 position = player.eyes.position;
				Vector3 val = mountAnchor.position + ((Component)this).transform.up * mountLOSVertOffset;
				Vector3 val2 = val - position;
				Ray ray = default(Ray);
				((Ray)(ref ray))._002Ector(position, ((Vector3)(ref val2)).normalized);
				PooledList<RaycastHit> val3 = Pool.Get<PooledList<RaycastHit>>();
				try
				{
					GamePhysics.TraceAllUnordered(ray, 0.25f, (List<RaycastHit>)(object)val3, Vector3.Distance(position, val), 1218519297, (QueryTriggerInteraction)0);
					foreach (RaycastHit item in (List<RaycastHit>)(object)val3)
					{
						BaseEntity entity = RaycastHitEx.GetEntity(item);
						if (!((Object)(object)entity == (Object)null) && !((Object)(object)entity == (Object)(object)this) && !((Object)(object)entity == (Object)(object)VehicleParent()))
						{
							return;
						}
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			if (!HasValidDismountPosition(player))
			{
				return;
			}
			if ((checkPlayerLosOnMount || ConVar.AntiHack.check_mount_distance >= 2) && ConVar.AntiHack.check_mount_distance >= 1)
			{
				float distanceFromMountAnchor = GetDistanceFromMountAnchor(player);
				float num = maxMountDistance;
				if (GetParentEntity() is BaseMountable baseMountable)
				{
					num = Mathf.Max(num, baseMountable.maxMountDistance);
				}
				if (distanceFromMountAnchor > num)
				{
					Debug.Log((object)$"Player {((Object)player).name} is too far from mount anchor: {distanceFromMountAnchor} > {num}");
					return;
				}
			}
		}
		MountPlayer(player);
	}

	public virtual bool AttemptDismount(BasePlayer player)
	{
		if ((Object)(object)player != (Object)(object)_mounted)
		{
			return false;
		}
		if (IsTransferring())
		{
			return false;
		}
		if (!AllowPlayerInstigatedDismount(player))
		{
			return false;
		}
		if ((Object)(object)VehicleParent() != (Object)null && !VehicleParent().AllowPlayerInstigatedDismount(player))
		{
			return false;
		}
		DismountPlayer(player);
		return true;
	}

	public virtual bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		return true;
	}

	[RPC_Server]
	public void RPC_WantsDismount(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!HasValidDismountPosition(player))
		{
			Interface.CallHook("OnPlayerDismountFailed", player, this);
		}
		else if (Interface.CallHook("OnPlayerWantsDismount", player, this) == null && (!((Object)(object)player != (Object)null) || !player.IsRestrained))
		{
			AttemptDismount(player);
		}
	}

	public bool ShouldRepositionPerFrame()
	{
		return mountSyncType != MountSyncType.Parent && (mountSyncType != MountSyncType.ParentAIOnly || !(_mounted is HumanNPC));
	}

	public void MountPlayer(BasePlayer player)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_mounted != (Object)null || (Object)(object)mountAnchor == (Object)null || Interface.CallHook("CanMountEntity", player, this) != null)
		{
			return;
		}
		player.EnsureDismounted();
		_mounted = player;
		Transform val = mountAnchor;
		player.SetMounted(this);
		if (!ShouldRepositionPerFrame())
		{
			if ((Object)(object)player.GetParentEntity() != (Object)(object)this)
			{
				player.SetParent(this, worldPositionStays: true, sendImmediate: true);
			}
			((Component)player).transform.localPosition = Vector3.zero;
			((Component)player).transform.localRotation = Quaternion.identity;
			((Component)player).transform.hasChanged = true;
		}
		else
		{
			player.MovePosition(val.position);
			((Component)player).transform.rotation = val.rotation;
			player.ServerRotation = val.rotation;
		}
		Quaternion rotation = val.rotation;
		player.OverrideViewAngles(((Quaternion)(ref rotation)).eulerAngles);
		_mounted.eyes.NetworkUpdate(val.rotation);
		player.SendNetworkUpdateImmediate();
		Facepunch.Rust.Analytics.Azure.OnMountEntity(player, this, VehicleParent());
		OnPlayerMounted();
		Interface.CallHook("OnEntityMounted", this, player);
		if (allowedGestures == MountGestureType.None && player.InGesture)
		{
			player.Server_CancelGesture();
		}
		else if (allowedGestures == MountGestureType.UpperBody && player.InGesture && player.CurrentGestureIsFullBody)
		{
			player.Server_CancelGesture();
		}
		if (this.IsValid() && player.IsValid())
		{
			player.ProcessMissionEvent(BaseMission.MissionEventType.MOUNT_ENTITY, net.ID, 1f);
		}
		SendNetworkUpdate();
	}

	public virtual void OnPlayerMounted()
	{
		if ((Object)(object)_mounted != (Object)null)
		{
			Mounted.TryAdd(this);
		}
		UpdateMountFlags();
	}

	public virtual void OnPlayerDismounted(BasePlayer player)
	{
		Mounted.Remove(this);
		UpdateMountFlags();
	}

	public virtual void UpdateMountFlags()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, (Object)(object)_mounted != (Object)null);
		}
		BaseVehicle baseVehicle = VehicleParent();
		if ((Object)(object)baseVehicle != (Object)null)
		{
			baseVehicle.UpdateMountFlags();
		}
	}

	public virtual void DismountAllPlayers()
	{
		if (Object.op_Implicit((Object)(object)_mounted))
		{
			DismountPlayer(_mounted);
		}
	}

	public void DismountPlayer(BasePlayer player, bool lite = false)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_mounted == (Object)null || (Object)(object)_mounted != (Object)(object)player || Interface.CallHook("CanDismountEntity", player, this) != null)
		{
			return;
		}
		if (!ShouldRepositionPerFrame())
		{
			_mounted.SetParent(null, worldPositionStays: true, sendImmediate: true);
		}
		BaseVehicle baseVehicle = VehicleParent();
		if (lite)
		{
			if ((Object)(object)baseVehicle != (Object)null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			_mounted.DismountObject();
			_mounted = null;
			if ((Object)(object)baseVehicle != (Object)null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
			Interface.CallHook("OnEntityDismounted", this, player);
			return;
		}
		if (!GetDismountPosition(player, out var res) || Distance(res) > 10f)
		{
			if ((Object)(object)baseVehicle != (Object)null)
			{
				baseVehicle.PrePlayerDismount(player, this);
			}
			res = ((Component)player).transform.position;
			_mounted.DismountObject();
			_mounted.MovePosition(res);
			((Component)_mounted).transform.rotation = Quaternion.identity;
			_mounted.ClientRPC(RpcTarget.Player("ForcePositionTo", _mounted), res);
			BasePlayer mounted = _mounted;
			_mounted = null;
			Debug.LogWarning((object)("Killing player due to invalid dismount point :" + player.displayName + " / " + player.userID.Get() + " on obj : " + ((Object)((Component)this).gameObject).name));
			mounted.Hurt(1000f, DamageType.Suicide, mounted, useProtection: false);
			if ((Object)(object)baseVehicle != (Object)null)
			{
				baseVehicle.PlayerDismounted(player, this);
			}
			OnPlayerDismounted(player);
			return;
		}
		if ((Object)(object)baseVehicle != (Object)null)
		{
			baseVehicle.PrePlayerDismount(player, this);
		}
		if (AntiHack.TestNoClipping(_mounted, res, res, BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _, overlapVehicleLayer: true))
		{
			_mounted.PauseVehicleNoClipDetection(5f);
		}
		_mounted.DismountObject();
		((Component)_mounted).transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
		_mounted.OverrideViewAngles(Vector3.zero);
		_mounted.MovePosition(res);
		_mounted.ForceUpdateTriggers();
		_mounted.SendNetworkUpdateImmediate();
		_mounted.SendModelState(force: true);
		_mounted = null;
		if ((Object)(object)baseVehicle != (Object)null)
		{
			baseVehicle.PlayerDismounted(player, this);
		}
		if (player.net != null)
		{
			if (Object.op_Implicit((Object)(object)player.GetParentEntity()))
			{
				BaseEntity baseEntity = player.GetParentEntity();
				player.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", player), ((Component)baseEntity).transform.InverseTransformPoint(res), baseEntity.net.ID);
			}
			else
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionTo", player), res);
				player.ClientRPC(RpcTarget.NetworkGroup("ForceResetRotation", player));
			}
		}
		Facepunch.Rust.Analytics.Azure.OnDismountEntity(player, this, baseVehicle);
		Interface.CallHook("OnEntityDismounted", this, player);
		OnPlayerDismounted(player);
		SendNetworkUpdate();
	}

	public virtual bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		BaseVehicle baseVehicle = VehicleParent();
		if ((Object)(object)baseVehicle != (Object)null && !baseVehicle.childMountableHandleDismountPoints && baseVehicle.IsVehicleMountPoint(this))
		{
			return baseVehicle.GetDismountPosition(player, out res);
		}
		int num = 0;
		Transform[] array = dismountPositions;
		foreach (Transform val in array)
		{
			if (!((Object)(object)val == (Object)null))
			{
				if (ValidDismountPosition(player, ((Component)val).transform.position))
				{
					res = ((Component)val).transform.position;
					return true;
				}
				num++;
			}
		}
		if (!silent)
		{
			Debug.LogWarning((object)("Failed to find dismount position for player :" + player.displayName + " / " + player.userID.Get() + " on obj : " + ((Object)((Component)this).gameObject).name));
		}
		res = ((Component)player).transform.position;
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (isMobile && !(this is BaseVehicleMountPoint { RequiresVehicleFixedUpdateOnSeat: false }))
		{
			AllMountables.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		DismountAllPlayers();
		AllMountables.Remove(this);
		base.DoServerDestroy();
	}

	public static void FixedUpdateCycle()
	{
		for (int num = AllMountables.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = AllMountables[num];
			if ((Object)(object)baseMountable == (Object)null)
			{
				AllMountables.RemoveAt(num);
			}
			else if (baseMountable.isSpawned)
			{
				baseMountable.VehicleFixedUpdate();
			}
		}
		for (int num2 = AllMountables.Count - 1; num2 >= 0; num2--)
		{
			BaseMountable baseMountable2 = AllMountables[num2];
			if ((Object)(object)baseMountable2 == (Object)null)
			{
				AllMountables.RemoveAt(num2);
			}
			else if (baseMountable2.isSpawned)
			{
				baseMountable2.PostVehicleFixedUpdate();
			}
		}
	}

	public static void PlayerSyncCycle()
	{
		for (int num = Mounted.Count - 1; num >= 0; num--)
		{
			BaseMountable baseMountable = Mounted[num];
			if ((Object)(object)baseMountable == (Object)null || (Object)(object)baseMountable.GetMounted() == (Object)null)
			{
				Mounted.RemoveAt(num);
			}
			else if (baseMountable.isSpawned)
			{
				baseMountable.MountedPlayerSync();
			}
		}
	}

	public virtual void VehicleFixedUpdate()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseMountable.VehicleFixedUpdate"))
		{
			if (!wantsBoundaryRepelCheck || !((Object)(object)rigidBody != (Object)null) || rigidBody.IsSleeping() || rigidBody.isKinematic)
			{
				return;
			}
			float world_boundary_force_start_distance = vehicle.world_boundary_force_start_distance;
			float world_boundary_force_offset = vehicle.world_boundary_force_offset;
			bool num = (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null;
			bool flag = num && DeepSeaManager.IsInsideDeepSea(((Component)this).transform.position);
			Vector3 center = (flag ? Vector3Ex.WithY(((Bounds)(ref DeepSeaManager.DeepSeaBounds)).center, 0f) : Vector3.zero);
			float num2 = (flag ? float.MaxValue : Mathf.Max(0f, ValidBounds.TestDist(this, ((Component)this).transform.position) - world_boundary_force_offset));
			if (num)
			{
				DeepSeaPortal.PortalModeEnum portalMode = ((!flag) ? DeepSeaPortal.PortalModeEnum.Entrance : DeepSeaPortal.PortalModeEnum.Exit);
				if (DeepSeaManager.IsInsideAnyPortal(((Component)this).transform.position, portalMode, out var deepSeaPortal))
				{
					OBB val = deepSeaPortal.WorldSpaceBounds();
					Transform transform = ((Component)deepSeaPortal).transform;
					float num3 = Vector3.Dot(((Component)this).transform.position - transform.position, transform.forward);
					float num4 = val.extents.z - num3;
					if (num4 < vehicle.deepseaportal_boundary_force_start_distance)
					{
						bool num5 = deepSeaPortal.PortalDirection == DeepSeaManager.GetEntrancePortalDirection();
						bool flag2;
						if (flag)
						{
							(flag2, _) = DeepSeaManager.CanTeleportToMainIsland(this);
						}
						else
						{
							(flag2, _) = DeepSeaManager.CanTeleportToDeepSea(this);
						}
						if (!num5 || !flag2)
						{
							num2 = (flag ? num4 : Mathf.Min(num2, num4));
						}
					}
				}
			}
			if (num2 < world_boundary_force_start_distance)
			{
				ApplyRepelForce(num2, world_boundary_force_start_distance, center);
			}
		}
	}

	private void ApplyRepelForce(float distToWorldEdge, float forceStartDistance, Vector3 center)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (distToWorldEdge > forceStartDistance)
		{
			return;
		}
		Vector3 val = ((Component)this).transform.position - center;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.Dot(rigidBody.linearVelocity, normalized);
		if (num > 0f)
		{
			float num2 = 1f - distToWorldEdge / forceStartDistance;
			Rigidbody obj = rigidBody;
			obj.linearVelocity -= normalized * num * (num2 * num2);
			if (distToWorldEdge < forceStartDistance * 0.25f)
			{
				float num3 = 1f - distToWorldEdge / (forceStartDistance * 0.25f);
				rigidBody.AddForce(-normalized * 20f * num3, (ForceMode)5);
			}
		}
	}

	public virtual void PostVehicleFixedUpdate()
	{
	}

	public virtual void MountedPlayerSync()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if ((syncsMountedPlayers || !canPauseMountedPlayerSync) && ShouldRepositionPerFrame())
		{
			((Component)_mounted).transform.rotation = ((Component)mountAnchor).transform.rotation;
			_mounted.ServerRotation = ((Component)mountAnchor).transform.rotation;
			_mounted.MovePosition(((Component)mountAnchor).transform.position);
		}
	}

	public virtual void PlayerServerInput(InputState inputState, BasePlayer player)
	{
	}

	public virtual float GetComfort()
	{
		return 0f;
	}

	public virtual void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
	}

	public bool TryFireProjectile(StorageContainer ammoStorage, AmmoTypes ammoType, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		projectile = null;
		if ((Object)(object)ammoStorage == (Object)null)
		{
			return false;
		}
		ItemContainer inventory = ammoStorage.inventory;
		if (inventory == null)
		{
			return false;
		}
		return TryFireProjectile(inventory, ammoType, firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile);
	}

	public virtual void FilterServerProjectileAmmo(List<Item> ammoList)
	{
	}

	public bool TryFireProjectile(ItemContainer ammoContainer, AmmoTypes ammoType, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		projectile = null;
		if (ammoContainer == null)
		{
			return false;
		}
		bool result = false;
		List<Item> list = Pool.Get<List<Item>>();
		ammoContainer.FindAmmo(list, ammoType);
		FilterServerProjectileAmmo(list);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].amount <= 0)
			{
				list.RemoveAt(num);
			}
		}
		if (list.Count > 0)
		{
			Item ammoItem = list[list.Count - 1];
			result = FireProjectile(ammoItem, firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile);
		}
		Pool.Free<Item>(ref list, false);
		return result;
	}

	public bool FireProjectile(Item ammoItem, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		ItemModProjectile component = ((Component)ammoItem.info).GetComponent<ItemModProjectile>();
		if (FireProjectile(component.GetOverrideProjectile(this), firingPos, firingDir, shooter, launchOffset, minSpeed, out projectile))
		{
			ammoItem.UseItem();
			return true;
		}
		return false;
	}

	public bool FireProjectile(GameObjectRef projectilePrefab, Vector3 firingPos, Vector3 firingDir, BasePlayer shooter, float launchOffset, float minSpeed, out ServerProjectile projectile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(firingPos, firingDir, ref val, launchOffset, 1237003025))
		{
			launchOffset = ((RaycastHit)(ref val)).distance - 0.1f;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(projectilePrefab.resourcePath, firingPos + firingDir * launchOffset);
		projectile = ((Component)baseEntity).GetComponent<ServerProjectile>();
		Vector3 val2 = projectile.initialVelocity + firingDir * projectile.speed;
		if (minSpeed > 0f)
		{
			float num = Vector3.Dot(val2, firingDir) - minSpeed;
			if (num < 0f)
			{
				val2 += firingDir * (0f - num);
			}
		}
		projectile.InitializeVelocity(val2);
		if (shooter.IsValid())
		{
			baseEntity.creatorEntity = shooter;
			baseEntity.OwnerID = shooter.userID;
		}
		baseEntity.Spawn();
		Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(shooter, baseEntity, this);
		return true;
	}

	public override void DisableTransferProtection()
	{
		base.DisableTransferProtection();
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted != (Object)null && mounted.IsTransferProtected())
		{
			mounted.DisableTransferProtection();
		}
	}

	protected virtual int GetClipCheckMask()
	{
		return 1210122497;
	}

	public virtual bool IsSeatClipping(BaseMountable mountable)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!clippingAndVisChecks)
		{
			return false;
		}
		if ((Object)(object)mountable == (Object)null)
		{
			return false;
		}
		int clipCheckMask = GetClipCheckMask();
		Vector3 position = ((Component)mountable.eyePositionOverride).transform.position;
		Vector3 position2 = ((Component)mountable).transform.position;
		Vector3 val = position - position2;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = clippingCheckRadius;
		if (mountable.modifiesPlayerCollider)
		{
			num = Mathf.Min(num, mountable.customPlayerCollider.radius);
		}
		Vector3 startPos = position - normalized * (num - 0.2f);
		return IsSeatClipping(mountable, startPos, num, clipCheckMask, position2, normalized);
	}

	private void DebugSeatClipping(BaseMountable mountable, bool clipped, Vector3 startPos, Vector3 endPos, float radius, int mask, bool headOnly)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Debugging.DebugClippingChecks && clipped)
		{
			Collider[] array = (headOnly ? Physics.OverlapSphere(startPos, radius, mask, (QueryTriggerInteraction)1) : Physics.OverlapCapsule(startPos, endPos, radius, mask, (QueryTriggerInteraction)1));
			foreach (Collider val in array)
			{
				Transform root = ((Component)val).transform.root;
				Debug.Log((object)("[Mount Debug] Clipping blocked by " + ((Object)val).name + " (" + ((Object)root).name + ") on " + mountable.ShortPrefabName));
			}
		}
	}

	public virtual Vector3 GetMountRagdollVelocity(BasePlayer player)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.zero;
	}

	protected virtual bool IsSeatClipping(BaseMountable mountable, Vector3 startPos, float radius, int mask, Vector3 seatPos, Vector3 direction)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		bool flag;
		if (clippingChecksLocation == ClippingCheckLocation.HeadOnly)
		{
			flag = GamePhysics.CheckSphere(GamePhysics.Realm.Server, startPos, radius, mask, (QueryTriggerInteraction)1);
			if (Debugging.DebugClippingChecks)
			{
				DebugSeatClipping(mountable, flag, startPos, startPos, radius, mask, headOnly: true);
			}
			return flag;
		}
		Vector3 val = seatPos + direction * (radius + 0.05f);
		flag = GamePhysics.CheckCapsule(GamePhysics.Realm.Server, startPos, val, radius, mask, (QueryTriggerInteraction)1);
		if (Debugging.DebugClippingChecks)
		{
			DebugSeatClipping(mountable, flag, startPos, val, radius, mask, headOnly: false);
		}
		return flag;
	}

	public virtual bool IsInstrument()
	{
		return false;
	}

	public virtual Vector3 GetDismountCheckStart(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = GetMountedPosition() + BasePlayer.NoClipOffset();
		Vector3 val2 = (((Object)(object)mountAnchor == (Object)null) ? ((Component)this).transform.forward : ((Component)mountAnchor).transform.forward);
		Vector3 val3 = (((Object)(object)mountAnchor == (Object)null) ? ((Component)this).transform.up : ((Component)mountAnchor).transform.up);
		if (mountPose == PlayerModel.MountPoses.Chair)
		{
			val += -val2 * 0.32f;
			val += val3 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			val += -val2 * 0.26f;
			val += val3 * 0.25f;
		}
		else if (mountPose == PlayerModel.MountPoses.SitGeneric)
		{
			val += -val2 * 0.26f;
		}
		return val;
	}

	public virtual Vector3 GetMountedPosition()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mountAnchor == (Object)null)
		{
			return ((Component)this).transform.position;
		}
		return ((Component)mountAnchor).transform.position;
	}

	public virtual float GetSpeed()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!isMobile)
		{
			return 0f;
		}
		return Vector3.Dot(GetLocalVelocity(), ((Component)this).transform.forward);
	}

	public bool CanPlayerSeeMountPoint(Ray ray, BasePlayer player, float maxDistance)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if ((Object)(object)mountAnchor == (Object)null)
		{
			return false;
		}
		RaycastHit hit = default(RaycastHit);
		if (Physics.SphereCast(ray, 0.25f, ref hit, maxDistance, 1218652417))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if ((Object)(object)entity != (Object)null)
			{
				if ((Object)(object)entity == (Object)(object)this || EqualNetID((BaseNetworkable)entity))
				{
					return true;
				}
				if (entity is BasePlayer basePlayer)
				{
					BaseMountable mounted = basePlayer.GetMounted();
					if ((Object)(object)mounted == (Object)(object)this)
					{
						return true;
					}
					if ((Object)(object)mounted != (Object)null && (Object)(object)mounted.VehicleParent() == (Object)(object)this)
					{
						return true;
					}
				}
				BaseEntity baseEntity = entity.GetParentEntity();
				if (RaycastHitEx.IsOnLayer(hit, (Layer)13) && ((Object)(object)baseEntity == (Object)(object)this || EqualNetID((BaseNetworkable)baseEntity)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetDistanceFromMountAnchor(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Distance(((Component)player).transform.position, mountAnchor.position);
	}

	public bool NearMountPoint(BasePlayer player)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if ((Object)(object)mountAnchor == (Object)null)
		{
			return false;
		}
		if (GetDistanceFromMountAnchor(player) > maxMountDistance)
		{
			return false;
		}
		return CanPlayerSeeMountPoint(player.eyes.HeadRay(), player, 2f);
	}

	public bool ClothingBlocksMounting(BasePlayer player)
	{
		if (BypassClothingMountBlocks)
		{
			return false;
		}
		foreach (Item item in player.inventory.containerWear.itemList)
		{
			if ((Object)(object)item.info.ItemModWearable != (Object)null && item.info.ItemModWearable.preventsMounting)
			{
				return true;
			}
		}
		return false;
	}

	public static Vector3 ConvertVector(Vector3 vec)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			if (((Vector3)(ref vec))[i] > 180f)
			{
				ref Vector3 reference = ref vec;
				int num = i;
				((Vector3)(ref reference))[num] = ((Vector3)(ref reference))[num] - 360f;
			}
			else if (((Vector3)(ref vec))[i] < -180f)
			{
				ref Vector3 reference = ref vec;
				int num = i;
				((Vector3)(ref reference))[num] = ((Vector3)(ref reference))[num] + 360f;
			}
		}
		return vec;
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		if (AnyMounted())
		{
			BasePlayer mounted = GetMounted();
			SprayCan.LastReskinError = SprayCan.PlayerIsMounted;
			SprayCan.LastReskinErrorArgString = NameHelper.GetPlayerNameStreamSafe(player, mounted);
			return false;
		}
		return base.CanBeRedirectSwapped(player);
	}

	public BaseMountable()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		pitchClamp = new Vector2(-80f, 50f);
		yawClamp = new Vector2(-80f, 80f);
		canWieldItems = true;
		relativeViewAngles = true;
		mountChaining = true;
		mountLOSVertOffset = 0.5f;
		maxMountDistance = 1.5f;
		clippingCheckRadius = 0.4f;
		canDrinkWhileMounted = true;
		animateClothInLocalSpace = true;
		protectsFromAnimals = true;
		SideLeanAmount = 0.2f;
		syncsMountedPlayers = true;
		base._002Ector();
	}

	static BaseMountable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		dismountPhrase = new Phrase("dismount", "Dismount");
		AllMountables = new ListHashSet<BaseMountable>();
		Mounted = new ListHashSet<BaseMountable>();
		canPauseMountedPlayerSync = false;
	}
}
