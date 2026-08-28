using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Safety;
using UnityEngine;
using UnityEngine.Assertions;

public class MountedWeapon : StorageContainer
{
	private struct LoadDataCache
	{
		public int ammoId;

		public int ammoCount;
	}

	[ServerVar]
	public static int antihack_level;

	[ServerVar]
	public static float antihack_max_snap_degrees;

	[ServerVar]
	public static float antihack_max_degrees_per_second_yaw;

	[ServerVar]
	public static float antihack_max_degrees_per_second_pitch;

	[ReplicatedVar]
	public static bool ENABLE_CLIENT_AUTHORITY;

	[ReplicatedVar]
	public static bool DEBUG;

	private static readonly int Up;

	[Header("Mounted Weapon")]
	[SerializeField]
	private Transform _eyes;

	[SerializeField]
	private bool _usingSights;

	[SerializeField]
	private bool _flipPitch;

	[SerializeField]
	private bool _invertForward;

	[SerializeField]
	private bool _clientAuthority;

	[SerializeField]
	[ItemSelector]
	private ItemDefinition _ammoItem;

	[SerializeField]
	[Header("Mounted Weapon - Weapon")]
	private ItemDefinition _weapon;

	[SerializeField]
	private Transform _attachPoint;

	[SerializeField]
	private Transform _yawPivot;

	[SerializeField]
	private Transform _pitchPivot;

	[SerializeField]
	private GameObjectRef _screenshakeEffect;

	[SerializeField]
	private GameObjectRef _dryFireEffect;

	[ItemSelector]
	public ItemDefinition AmmoDef;

	[SerializeField]
	[Header("Mounted Weapon - Second Weapon")]
	private ItemDefinition _weapon2;

	[SerializeField]
	private Transform _attachPoint2;

	[Header("Mounted Weapon - Player General Animation")]
	[SerializeField]
	private int _turretAnimationType;

	[SerializeField]
	private Transform _leftHandIdleIKPosition;

	[SerializeField]
	private Transform _rightHandIdleIKPosition;

	[SerializeField]
	private AnimationCurve _reloadIKBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool _walkAroundTurret;

	[SerializeField]
	private float _minWalkAroundDistance = 1.11f;

	[SerializeField]
	private float _walkAroundDistance = 1.11f;

	[SerializeField]
	private float _reloadWalkAroundDistance = 1.11f;

	[SerializeField]
	private AnimationCurve _walkAroundDistanceCurve;

	[SerializeField]
	private AnimationCurve _reloadWalkAroundBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool _forceSeatPositionUpdates;

	[Header("Mounted Weapon - Player Camera Animation")]
	[SerializeField]
	private Transform _cameraAnimation;

	[SerializeField]
	private Animator _cameraAnimationController;

	[SerializeField]
	private float _fovMultiplier = 1f;

	[SerializeField]
	[Header("Mounted Weapon - Viewmodel")]
	private bool _useViewmodel;

	[SerializeField]
	private ViewModel _viewmodel;

	[Header("Mounted Weapon - Aim Movement Sounds")]
	[SerializeField]
	private SoundDefinition aimMovementSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementYawSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementPitchSoundDef;

	[SerializeField]
	private AnimationCurve aimMovementGainCurve;

	[SerializeField]
	private float aimMovementSpeedDecayRate = 200f;

	[SerializeField]
	private float aimMovementSpeedMax = 100f;

	public const Flags Flag_WeaponAttached = Flags.Reserved15;

	public const Flags Flag_Lights = Flags.Reserved5;

	private static readonly Phrase _ammoPhrase;

	private static readonly Phrase _ammoFullPhrase;

	private Vector3 _defaultEyePosition;

	private Quaternion _defaultEyeRotation;

	private EntityRef<HeldEntity> _attachedEntity;

	private EntityRef<HeldEntity> _attachedEntity2;

	private float _reloadTime;

	private MountedWeaponSeat _seat;

	private BasePlayer _mountedPlayer;

	private Vector3 _seatRelativePosition;

	private float _targetWorldYaw;

	private float _targetWorldPitch;

	private float _worldPitch;

	private float _worldYaw;

	private float _startTime;

	private float _reloadServerTimer;

	private float _lastAimRpcTime;

	private float _lastAimYaw;

	private float _lastAimPitch;

	private int[] _reloadStartMag = new int[2];

	private int[] _reloadTaken = new int[2];

	private LoadDataCache? _loadDataCache;

	private ServersideMountedWeaponSnapshot __sync_Snapshot;

	private NetworkableId __sync_GunId;

	private NetworkableId __sync_Gun2Id;

	private bool __sync_IsReloading;

	private bool __sync_IsEmpty;

	private GameObject _worldModel => ((Component)((Component)this).GetComponentInChildren<BaseProjectile>(true)).gameObject;

	public ItemDefinition WeaponDef => _weapon;

	[Sync(RequireChange = false, Pack = false)]
	private ServersideMountedWeaponSnapshot Snapshot
	{
		[CompilerGenerated]
		get
		{
			return __sync_Snapshot;
		}
		[CompilerGenerated]
		set
		{
			__sync_Snapshot = value;
			byte nameID = __GetWeaverID("Snapshot");
			SV_SyncVarSend(nameID);
		}
	}

	[Sync(Autosave = true)]
	private NetworkableId GunId
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_GunId;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<NetworkableId>(__sync_GunId, value))
			{
				__sync_GunId = value;
				byte nameID = __GetWeaverID("GunId");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private NetworkableId Gun2Id
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_Gun2Id;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<NetworkableId>(__sync_Gun2Id, value))
			{
				__sync_Gun2Id = value;
				byte nameID = __GetWeaverID("Gun2Id");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Pack = false)]
	public bool IsReloading
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsReloading;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsReloading, value))
			{
				__sync_IsReloading = value;
				byte nameID = __GetWeaverID("IsReloading");
				SV_SyncVarSend(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public bool IsEmpty
	{
		[CompilerGenerated]
		get
		{
			return __sync_IsEmpty;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_IsEmpty, value))
			{
				__sync_IsEmpty = value;
				byte nameID = __GetWeaverID("IsEmpty");
				QueueSyncVar(nameID);
			}
		}
	}

	private bool HasSecondWeapon
	{
		get
		{
			if ((Object)(object)_attachPoint2 != (Object)null)
			{
				return (Object)(object)_weapon2 != (Object)null;
			}
			return false;
		}
	}

	public Transform PitchPivot => _pitchPivot;

	public bool PilotedByAi
	{
		get
		{
			if ((Object)(object)_seat != (Object)null)
			{
				return _seat.GetMounted() is HumanNPC;
			}
			return false;
		}
	}

	private bool HasServerAuthority
	{
		get
		{
			if (_clientAuthority)
			{
				if ((Object)(object)_seat != (Object)null)
				{
					return _seat.GetMounted() is HumanNPC;
				}
				return false;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MountedWeapon.OnRpcMessage"))
		{
			if (rpc == 2998965234u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_ReceiveClientAim"));
				}
				using (TimeWarning.New("SV_ReceiveClientAim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2998965234u, "SV_ReceiveClientAim", this, player, 100uL))
						{
							return true;
						}
						long position = msg.read.Position;
						ServersideMountedWeaponSnapshot val = msg.read.Proto<ServersideMountedWeaponSnapshot>((ServersideMountedWeaponSnapshot)null);
						try
						{
							if (!RPC_Server.InputValidation.Test(val.time))
							{
								return true;
							}
							if (!RPC_Server.InputValidation.Test(val.pitch))
							{
								return true;
							}
							if (!RPC_Server.InputValidation.Test(val.yaw))
							{
								return true;
							}
							msg.read.Position = position;
							if (!RPC_Server.MaxDistance.Test(2998965234u, "SV_ReceiveClientAim", this, player, 3f))
							{
								return true;
							}
						}
						finally
						{
							((IDisposable)val)?.Dispose();
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
							SV_ReceiveClientAim(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_ReceiveClientAim");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public BaseEntity GetOwnerEntity()
	{
		return this;
	}

	public MountedWeaponSeat GetSeat()
	{
		return _seat;
	}

	public void AssignSeat(MountedWeaponSeat seat)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_seat = seat;
		_seatRelativePosition = ((Component)this).transform.InverseTransformPoint(((Component)seat).transform.position);
	}

	public Quaternion EyeRotationForPlayer(BasePlayer player, Quaternion baseEyeRot)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (IsReloading && (Object)(object)_cameraAnimation != (Object)null)
		{
			return baseEyeRot * _cameraAnimation.localRotation;
		}
		return baseEyeRot;
	}

	public Transform GetCustomEyes()
	{
		return _eyes;
	}

	private void ShowDebug()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG && base.isServer)
		{
			Vector3 val = ((Component)this).transform.position + Vector3.up * 3.5f;
			UnityEngine.DDraw.BroadcastText(val, "SERVER\n" + $"WorldYaw:   {_worldYaw:F1}\n" + $"WorldPitch: {_worldPitch:F1}\n" + $"TargetYaw:  {_targetWorldYaw:F1}\n" + $"TargetPitch:{_targetWorldPitch:F1}", Color.yellow, 0f);
			WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var turretPitch);
			UnityEngine.DDraw.BroadcastText(val + Vector3.up * 1.2f, $"Server Local\nYaw: {turretYaw:F1}\nPitch: {turretPitch:F1}", Color.white, 0f);
		}
	}

	public Vector3 EyePositionForPlayer(BasePlayer player, Quaternion lookRot, Vector3 baseEyePos)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (IsReloading && (Object)(object)_cameraAnimation != (Object)null)
		{
			return baseEyePos + _cameraAnimation.localPosition;
		}
		return baseEyePos;
	}

	private void Tick()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		_clientAuthority = ENABLE_CLIENT_AUTHORITY;
		ShowDebug();
		if (!Object.op_Implicit((Object)(object)_seat) || !_seat.AnyMounted())
		{
			return;
		}
		if (IsReloading)
		{
			LerpToZero();
		}
		Vector3 gunForward = GetGunForward();
		((Component)_seat).transform.forward = gunForward;
		if (_walkAroundTurret || _forceSeatPositionUpdates)
		{
			using (TimeWarning.New("MountedWeapon.Update.SeatPosition"))
			{
				_seatRelativePosition = ((Component)this).transform.InverseTransformPoint(((Component)_seat).transform.position);
				float num = _walkAroundDistance;
				Vector2 pitchClamp = _seat.GetPitchClamp();
				float num2 = 0f;
				if (base.isServer)
				{
					num2 = _worldPitch;
				}
				if (true)
				{
					float num3 = Mathf.InverseLerp(pitchClamp.x, pitchClamp.y, num2);
					float num4 = _walkAroundDistanceCurve.Evaluate(num3);
					num4 = Mathf.Clamp01(num4);
					num = Mathf.Lerp(_minWalkAroundDistance, _walkAroundDistance, num4);
				}
				Vector3 position = ((Component)this).transform.position + -GetGunForward() * num;
				position.y = ((Component)this).transform.TransformPoint(_seatRelativePosition).y;
				((Component)_seat).transform.position = position;
			}
		}
		float num5 = 65f;
		if (_usingSights)
		{
			num5 = 110f;
		}
		if (!base.isServer)
		{
			return;
		}
		using (TimeWarning.New("MountedWeapon.Tick.Server"))
		{
			if (!_clientAuthority || ((Object)(object)_seat != (Object)null && _seat.GetMounted() is HumanNPC))
			{
				_worldYaw = Mathf.LerpAngle(_worldYaw, _targetWorldYaw, Time.deltaTime * num5);
				_worldPitch = Mathf.LerpAngle(_worldPitch, _targetWorldPitch, Time.deltaTime * num5);
				WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var turretPitch);
				Vector2 yawClamp = GetSeat().GetYawClamp();
				Vector2 pitchClamp2 = GetSeat().GetPitchClamp();
				turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
				turretPitch = Mathf.Clamp(turretPitch, pitchClamp2.x, pitchClamp2.y);
				_yawPivot.localRotation = Quaternion.Euler(0f, turretYaw, 0f);
				_pitchPivot.localRotation = Quaternion.Euler(turretPitch, 0f, 0f);
			}
		}
	}

	private void Update()
	{
		Tick();
	}

	public override void Load(LoadInfo info)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		_attachedEntity.uid = GunId;
		_attachedEntity2.uid = Gun2Id;
		CalculateReloadTime();
		if (!info.fromDisk)
		{
			return;
		}
		if (base.isServer)
		{
			UpdateClient(force: true);
			if (info.msg.mountedWeapon != null)
			{
				_loadDataCache = new LoadDataCache
				{
					ammoId = info.msg.mountedWeapon.ammoItemID,
					ammoCount = info.msg.mountedWeapon.ammoStackSize
				};
				SetupTurretsWithLoadData();
			}
		}
		GetZeroInWorldAngles(out var worldYaw, out var worldPitch);
		SetTargetAngles(worldYaw, worldPitch, set: true);
	}

	private void SetTargetAngles(float worldYaw, float worldPitch, bool set = false)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GetOwnerEntity() == (Object)null || (Object)(object)GetSeat() == (Object)null)
		{
			return;
		}
		_targetWorldYaw = worldYaw;
		_targetWorldPitch = worldPitch;
		if (set)
		{
			if (base.isServer)
			{
				_worldYaw = worldYaw;
				_worldPitch = worldPitch;
			}
			_targetWorldYaw = worldYaw;
			_targetWorldPitch = worldPitch;
			WorldAngleToTurretAngle(_targetWorldYaw, _targetWorldPitch, out var turretYaw, out var turretPitch);
			Vector2 yawClamp = GetSeat().GetYawClamp();
			Vector2 pitchClamp = GetSeat().GetPitchClamp();
			turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
			turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
			_yawPivot.localRotation = Quaternion.Euler(0f, turretYaw, 0f);
			_pitchPivot.localRotation = Quaternion.Euler(turretPitch, 0f, 0f);
		}
		Transform transform = ((Component)this).transform;
		Quaternion rotation = ((Component)this).transform.rotation;
		transform.rotation = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
	}

	private void GetZeroInWorldAngles(out float worldYaw, out float worldPitch)
	{
		TurretAngleToWorldAngle(0f, 0f, out worldYaw, out worldPitch);
	}

	private Vector3 GetGunForward()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PitchPivot == (Object)null)
		{
			return Vector3.forward;
		}
		Vector3 val = PitchPivot.forward;
		if (_invertForward)
		{
			val = -val;
		}
		return val;
	}

	private void CalculateReloadTime()
	{
		_reloadTime = 0f;
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		if ((Object)(object)baseProjectile != (Object)null)
		{
			_reloadTime = baseProjectile.reloadTime;
		}
		BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
		if ((Object)(object)baseProjectile2 != (Object)null)
		{
			_reloadTime = Mathf.Max(_reloadTime, baseProjectile2.reloadTime);
		}
	}

	private static float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle < -180f)
		{
			angle += 360f;
		}
		return angle;
	}

	private HeldEntity GetWeaponEntity()
	{
		HeldEntity heldEntity = _attachedEntity.Get(base.isServer);
		if (heldEntity.IsValid())
		{
			return heldEntity;
		}
		return null;
	}

	private HeldEntity GetWeaponEntity2()
	{
		HeldEntity heldEntity = _attachedEntity2.Get(base.isServer);
		if (heldEntity.IsValid())
		{
			return heldEntity;
		}
		return null;
	}

	private void TurretAngleToWorldAngle(float turretYaw, float turretPitch, out float worldYaw, out float worldPitch)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float num = (_flipPitch ? (-1f) : 1f);
		Vector3 val = Quaternion.Euler((0f - turretPitch) * num, turretYaw, 0f) * Vector3.forward;
		Vector3 val2 = ((Component)this).transform.TransformDirection(val);
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		worldYaw = Mathf.Atan2(normalized.x, normalized.z) * 57.29578f;
		worldPitch = (0f - Mathf.Asin(normalized.y)) * 57.29578f;
	}

	private void WorldAngleToTurretAngle(float worldYaw, float worldPitch, out float turretYaw, out float turretPitch)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Quaternion.Euler(new Vector3(worldPitch, worldYaw, 0f)) * Vector3.forward;
		Transform transform = ((Component)this).transform;
		Quaternion val2 = ((Component)this).transform.rotation;
		transform.rotation = Quaternion.Euler(0f, ((Quaternion)(ref val2)).eulerAngles.y, 0f);
		Vector3 val3 = ((Component)this).transform.InverseTransformDirection(val);
		val2 = Quaternion.LookRotation(((Vector3)(ref val3)).normalized, ((Component)this).transform.up);
		Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
		turretYaw = eulerAngles.y;
		turretPitch = (0f - eulerAngles.x) * (_flipPitch ? (-1f) : 1f);
		turretYaw = NormalizeAngle(turretYaw);
		turretPitch = NormalizeAngle(turretPitch);
	}

	private void LerpToZero()
	{
		float num = 25f;
		WorldAngleToTurretAngle(_worldYaw, _worldPitch, out var turretYaw, out var _);
		TurretAngleToWorldAngle(turretYaw, 0f, out var worldYaw, out var worldPitch);
		if (base.isServer)
		{
			_worldYaw = Mathf.MoveTowardsAngle(_worldYaw, worldYaw, Time.deltaTime * num);
			_worldPitch = Mathf.MoveTowardsAngle(_worldPitch, worldPitch, Time.deltaTime * num);
			SetTargetAngles(_worldYaw, _worldPitch);
			UpdateClient(force: true);
		}
	}

	private void HandleAiming(InputState inputState, BasePlayer player, bool asClient, float currentPitch, float currentYaw)
	{
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		float num = 1.5f;
		bool flag = false;
		if (base.isServer)
		{
			flag = HasServerAuthority && !asClient;
		}
		if ((Object)(object)_seat == (Object)null || !_seat.AnyMounted() || IsReloading || (Object)(object)player == (Object)null || (Object)(object)player.eyes == (Object)null || Snapshot == null)
		{
			return;
		}
		bool flag2 = false;
		if (!flag)
		{
			return;
		}
		if (_usingSights)
		{
			float num2 = (0f - inputState.current.mouseDelta.y) * num;
			float num3 = inputState.current.mouseDelta.x * num;
			WorldAngleToTurretAngle(currentYaw, currentPitch, out var turretYaw, out var turretPitch);
			turretYaw += num3;
			turretPitch -= num2;
			Vector2 yawClamp = _seat.GetYawClamp();
			Vector2 pitchClamp = _seat.GetPitchClamp();
			turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
			turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
			TurretAngleToWorldAngle(turretYaw, turretPitch, out var worldYaw, out var worldPitch);
			SetTargetAngles(worldYaw, worldPitch);
			float num4 = 0.15f;
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw, currentYaw)) > num4 || Mathf.Abs(Mathf.DeltaAngle(worldPitch, currentPitch)) > num4)
			{
				flag2 = true;
			}
		}
		else
		{
			Quaternion val = Quaternion.LookRotation(Ballistics.GetBulletHitPoint(new Ray(player.eyes.position + player.eyes.HeadForward() * 0.5f, player.eyes.HeadForward()), (BaseEntity)this) - ((Component)this).transform.position);
			Quaternion.Euler(currentPitch, currentYaw, 0f);
			Vector3 eulerAngles = ((Quaternion)(ref val)).eulerAngles;
			float y = eulerAngles.y;
			float x = eulerAngles.x;
			WorldAngleToTurretAngle(y, x, out var turretYaw2, out var turretPitch2);
			Vector2 yawClamp2 = _seat.GetYawClamp();
			Vector2 pitchClamp2 = _seat.GetPitchClamp();
			turretYaw2 = Mathf.Clamp(turretYaw2, yawClamp2.x, yawClamp2.y);
			turretPitch2 = Mathf.Clamp(turretPitch2, pitchClamp2.x, pitchClamp2.y);
			TurretAngleToWorldAngle(turretYaw2, turretPitch2, out var worldYaw2, out var worldPitch2);
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw2, currentYaw)) > 0.05f || Mathf.Abs(Mathf.DeltaAngle(worldPitch2, currentPitch)) > 0.05f)
			{
				SetTargetAngles(worldYaw2, worldPitch2);
			}
			float num5 = 1f;
			if (Mathf.Abs(Mathf.DeltaAngle(worldYaw2, currentYaw)) > num5 || Mathf.Abs(Mathf.DeltaAngle(worldPitch2, currentPitch)) > num5)
			{
				flag2 = true;
			}
		}
		if (flag2 && !base.isClient)
		{
			UpdateClient();
		}
	}

	public void OnPlayerMounted()
	{
		UpdateClient(force: true);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		Invoke(delegate
		{
			UpdateAttachedWeapon(_weapon, _attachPoint);
			if (HasSecondWeapon)
			{
				UpdateAttachedWeapon(_weapon, _attachPoint2, second: true);
			}
		}, 0.5f);
		_startTime = Time.realtimeSinceStartup;
		_reloadServerTimer = 0f;
		GetZeroInWorldAngles(out var worldYaw, out var worldPitch);
		_worldYaw = worldYaw;
		_worldPitch = worldPitch;
		SetTargetAngles(_worldYaw, _worldPitch);
		Invoke(delegate
		{
			UpdateClient(force: true);
		}, 1f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		_lastAimRpcTime = Time.realtimeSinceStartup;
		_lastAimYaw = _worldYaw;
		_lastAimPitch = _worldPitch;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.mountedWeapon = Pool.Get<MountedWeapon>();
			BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
			if ((Object)(object)baseProjectile != (Object)null)
			{
				info.msg.mountedWeapon.ammoItemID = baseProjectile.primaryMagazine.ammoType.itemid;
				info.msg.mountedWeapon.ammoStackSize = baseProjectile.primaryMagazine.contents;
			}
		}
	}

	private void RefundAmmo(BasePlayer player, int amount)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (amount > 0 && !((Object)(object)player == (Object)null))
		{
			Item item = ItemManager.Create(AmmoDef, amount, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(player.inventory.containerMain))
			{
				item.Drop(((Component)player).transform.position + Vector3.up, Vector3.down);
			}
		}
	}

	public void OnPlayerDismounted(BasePlayer player)
	{
		HeldEntity weaponEntity = GetWeaponEntity();
		HeldEntity weaponEntity2 = GetWeaponEntity2();
		bool isReloading = IsReloading;
		if ((Object)(object)weaponEntity != (Object)null)
		{
			weaponEntity.forcedOwner = null;
			if (isReloading)
			{
				IsReloading = false;
				_reloadServerTimer = 0f;
				CancelInvoke(ProcessServerReloadTimer);
				if (weaponEntity is BaseProjectile baseProjectile)
				{
					RefundAmmo(player, _reloadTaken[0]);
					baseProjectile.primaryMagazine.contents = _reloadStartMag[0];
					IsEmpty = _reloadStartMag[0] == 0;
					baseProjectile.SkipReload();
				}
			}
		}
		if ((Object)(object)weaponEntity2 != (Object)null)
		{
			weaponEntity2.forcedOwner = null;
			if (isReloading && weaponEntity2 is BaseProjectile baseProjectile2)
			{
				RefundAmmo(player, _reloadTaken[1]);
				baseProjectile2.primaryMagazine.contents = _reloadStartMag[1];
				IsEmpty = _reloadStartMag[1] == 0;
				baseProjectile2.SkipReload();
			}
		}
		_reloadTaken[0] = (_reloadTaken[1] = 0);
		_reloadStartMag[0] = (_reloadStartMag[1] = 0);
	}

	private void SetupTurretsWithLoadData()
	{
		if (!_loadDataCache.HasValue)
		{
			return;
		}
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(_loadDataCache.Value.ammoId);
		if (!((Object)(object)itemDefinition == (Object)null))
		{
			if ((Object)(object)baseProjectile != (Object)null)
			{
				baseProjectile.primaryMagazine.ammoType = itemDefinition;
				baseProjectile.primaryMagazine.contents = _loadDataCache.Value.ammoCount;
			}
			BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
			if ((Object)(object)baseProjectile2 != (Object)null)
			{
				baseProjectile2.primaryMagazine.ammoType = itemDefinition;
				baseProjectile2.primaryMagazine.contents = _loadDataCache.Value.ammoCount;
			}
		}
	}

	public void LightToggle(BasePlayer basePlayer)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, !HasFlag(Flags.Reserved5));
	}

	public override void AdminKill()
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			baseEntity.AdminKill();
		}
		base.AdminKill();
	}

	public override BasePlayer ToPlayer()
	{
		BaseEntity ownerEntity = GetOwnerEntity();
		if ((Object)(object)ownerEntity != (Object)null && ownerEntity is BaseVehicleSeat baseVehicleSeat)
		{
			return baseVehicleSeat.GetMounted();
		}
		return null;
	}

	public bool Fire(bool isAi = false)
	{
		if (IsReloading)
		{
			return false;
		}
		BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
		BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
		int num = (int)(0u | (TryFireWeapon(baseProjectile) ? 1u : 0u)) | (TryFireWeapon(baseProjectile2) ? 1 : 0);
		if ((Object)(object)baseProjectile != (Object)null)
		{
			IsEmpty = baseProjectile.AmmoFraction() <= 0f && ((Object)(object)baseProjectile2 == (Object)null || baseProjectile2.AmmoFraction() <= 0f);
		}
		if (((uint)num & (isAi ? 1u : 0u)) != 0)
		{
			ClientRPC(RpcTarget.NetworkGroup("CL_OnAttack"));
		}
		return (byte)num != 0;
	}

	public void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		using (TimeWarning.New("ServersideMountedWeapon.ServerInput"))
		{
			if (inputState.IsDown(BUTTON.FIRE_PRIMARY) && !IsReloading)
			{
				if (player.InSafeZone())
				{
					return;
				}
				if (Fire())
				{
					player.MarkHostileFor();
					ClientRPC(RpcTarget.NetworkGroup("CL_OnAttack"));
				}
			}
			inputState.IsDown(BUTTON.FIRE_SECONDARY);
			if (inputState.IsDown(BUTTON.RELOAD) && !IsReloading)
			{
				PooledList<Item> val = Pool.Get<PooledList<Item>>();
				try
				{
					player.inventory.FindAmmo((List<Item>)(object)val, (AmmoTypes)2);
					bool flag = false;
					foreach (Item item in (List<Item>)(object)val)
					{
						if (item.info.itemid == _ammoItem.itemid)
						{
							flag = true;
						}
					}
					if (flag)
					{
						BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
						BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
						if ((Object)(object)baseProjectile != (Object)null && baseProjectile.AmmoFraction() >= 1f && ((Object)(object)baseProjectile2 == (Object)null || baseProjectile2.AmmoFraction() >= 1f))
						{
							player.ShowToast(GameTip.Styles.Blue_Normal, _ammoFullPhrase, false);
							return;
						}
						if (_reloadTime == 0f)
						{
							CalculateReloadTime();
						}
						_reloadServerTimer = _reloadTime;
						StartReload();
					}
					else
					{
						player.ShowToast(GameTip.Styles.Blue_Normal, _ammoPhrase, false);
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			if (!IsReloading)
			{
				HandleAiming(inputState, player, asClient: false, _worldPitch, _worldYaw);
			}
		}
	}

	public void CheckAiReload()
	{
		bool isReloading = false;
		bool flag = false;
		if (GetWeaponEntity() is BaseProjectile baseProjectile)
		{
			if (baseProjectile.ServerIsReloading())
			{
				isReloading = true;
			}
			if (!baseProjectile.ServerIsReloading() && baseProjectile.primaryMagazine.contents <= 0)
			{
				flag = true;
				baseProjectile.ServerReload();
			}
		}
		if (GetWeaponEntity2() is BaseProjectile baseProjectile2)
		{
			if (baseProjectile2.ServerIsReloading())
			{
				isReloading = true;
			}
			if (!baseProjectile2.ServerIsReloading() && baseProjectile2.primaryMagazine.contents <= 0)
			{
				flag = true;
				baseProjectile2.ServerReload();
			}
		}
		IsReloading = isReloading;
		if (flag)
		{
			float time = Time.time;
			float arg = time + _reloadTime;
			ClientRPC(RpcTarget.NetworkGroup("CL_StartReloading"), time, arg);
		}
	}

	public void AimAt(Vector3 origin, Vector3 desiredGunForward, bool forceFlip)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		float worldYaw = Mathf.Atan2(desiredGunForward.x, desiredGunForward.z) * 57.29578f;
		float worldPitch = (0f - Mathf.Asin(desiredGunForward.y)) * 57.29578f;
		WorldAngleToTurretAngle(worldYaw, worldPitch, out var turretYaw, out var turretPitch);
		if (forceFlip)
		{
			turretPitch = 0f - turretPitch;
		}
		turretYaw = NormalizeAngle(turretYaw);
		turretPitch = NormalizeAngle(turretPitch);
		TurretAngleToWorldAngle(turretYaw, turretPitch, out var worldYaw2, out var worldPitch2);
		_worldYaw = Mathf.MoveTowardsAngle(_worldYaw, worldYaw2, Time.deltaTime * 105f);
		_worldPitch = Mathf.MoveTowardsAngle(_worldPitch, worldPitch2, Time.deltaTime * 105f);
		SetTargetAngles(_worldYaw, _worldPitch, set: true);
		if (Snapshot == null || Mathf.Abs(Mathf.DeltaAngle(Snapshot.yaw, _worldYaw)) > 3f || Mathf.Abs(Mathf.DeltaAngle(Snapshot.pitch, _worldPitch)) > 3f)
		{
			UpdateClient(force: true);
		}
	}

	private void UpdateClient(bool force = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		Snapshot = new ServersideMountedWeaponSnapshot
		{
			time = Time.realtimeSinceStartup - _startTime,
			yaw = _worldYaw,
			pitch = _worldPitch,
			force = force
		};
	}

	private bool CanAcceptItem(Item item, int targetSlot)
	{
		if (Check.IsValidWeapon(item, checkCanUseTurret: true) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			return true;
		}
		return false;
	}

	private bool TryFireWeapon(HeldEntity heldEntity)
	{
		if ((Object)(object)heldEntity == (Object)null)
		{
			return false;
		}
		if ((Object)(object)_seat != (Object)null)
		{
			BasePlayer mounted = _seat.GetMounted();
			heldEntity.forcedOwner = mounted;
			heldEntity.useOwnerForward = false;
		}
		if (heldEntity is BaseProjectile baseProjectile)
		{
			if (baseProjectile.NextAttackTime > Time.time)
			{
				return false;
			}
			if (baseProjectile.primaryMagazine.contents <= 0)
			{
				ClientRPC(RpcTarget.NetworkGroup("CL_OnDryFire"), _mountedPlayer);
				baseProjectile.StartAttackCooldown(1f);
				return false;
			}
			if (baseProjectile is ITurretNotify turretNotify)
			{
				turretNotify.WarmupTick(wantsShoot: true);
				if (!turretNotify.CanShoot())
				{
					return false;
				}
			}
		}
		heldEntity.ServerUse();
		heldEntity.useOwnerForward = false;
		heldEntity.forcedOwner = null;
		return true;
	}

	private void UpdateAttachedWeapon(ItemDefinition weapon, Transform attachPoint, bool second = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		HeldEntity heldEntity = AutoTurret.TryAddWeaponToTurret(ItemManager.Create(second ? _weapon2 : weapon, 1, 0uL, isServerSide: true, 0uL), attachPoint, this, -0.5f);
		((Component)heldEntity).transform.localPosition = Vector3.zero;
		if (HasSecondWeapon)
		{
			Vector3 val = (_attachPoint.position + _attachPoint2.position) / 2f + attachPoint.forward * 10f - attachPoint.position;
			Quaternion val2 = Quaternion.LookRotation(((Vector3)(ref val)).normalized, ((Component)this).transform.up);
			((Component)heldEntity).transform.localRotation = Quaternion.Inverse(attachPoint.rotation) * val2;
		}
		bool flag = (Object)(object)heldEntity != (Object)null;
		if (flag)
		{
			if (!second)
			{
				_attachedEntity.Set(heldEntity);
				GunId = _attachedEntity.uid;
			}
			else
			{
				_attachedEntity2.Set(heldEntity);
				Gun2Id = _attachedEntity2.uid;
			}
		}
		else
		{
			HeldEntity heldEntity2 = GetWeaponEntity();
			if (second)
			{
				heldEntity2 = GetWeaponEntity2();
			}
			if ((Object)(object)heldEntity2 != (Object)null)
			{
				heldEntity2.SetGenericVisible(wantsVis: false);
				heldEntity2.SetLightsOn(isOn: false);
				if (heldEntity2 is ITurretNotify turretNotify)
				{
					turretNotify.WarmupTick(wantsShoot: false);
				}
			}
			if (!second)
			{
				_attachedEntity.Set(null);
				GunId = _attachedEntity.uid;
			}
			else
			{
				_attachedEntity2.Set(null);
				Gun2Id = _attachedEntity2.uid;
			}
		}
		SetupTurretsWithLoadData();
		SetFlagLocal(Flags.Reserved15, flag);
		SendNetworkUpdate();
	}

	private void StartReload()
	{
		if ((Object)(object)_seat == (Object)null)
		{
			return;
		}
		BasePlayer mounted = _seat.GetMounted();
		if (!((Object)(object)mounted == (Object)null))
		{
			CalculateReloadTime();
			BaseProjectile baseProjectile = GetWeaponEntity() as BaseProjectile;
			BaseProjectile baseProjectile2 = GetWeaponEntity2() as BaseProjectile;
			_reloadStartMag[0] = (Object.op_Implicit((Object)(object)baseProjectile) ? baseProjectile.primaryMagazine.contents : 0);
			_reloadStartMag[1] = (Object.op_Implicit((Object)(object)baseProjectile2) ? baseProjectile2.primaryMagazine.contents : 0);
			bool num = (Object)(object)baseProjectile != (Object)null && baseProjectile.ServerTryReload(mounted.inventory);
			bool flag = (Object)(object)baseProjectile2 != (Object)null && baseProjectile2.ServerTryReload(mounted.inventory);
			int num2 = (Object.op_Implicit((Object)(object)baseProjectile) ? baseProjectile.primaryMagazine.contents : 0);
			int num3 = (Object.op_Implicit((Object)(object)baseProjectile2) ? baseProjectile2.primaryMagazine.contents : 0);
			_reloadTaken[0] = Mathf.Max(0, num2 - _reloadStartMag[0]);
			_reloadTaken[1] = Mathf.Max(0, num3 - _reloadStartMag[1]);
			bool num4 = num | flag;
			if (_reloadTime == 0f)
			{
				CalculateReloadTime();
			}
			float time = Time.time;
			float arg = time + _reloadTime;
			if (num4)
			{
				IsEmpty = false;
				IsReloading = true;
				mounted.userID.Get();
				_reloadServerTimer = 0f;
				ClientRPC(RpcTarget.NetworkGroup("CL_StartReloading"), time, arg);
				InvokeRepeating(ProcessServerReloadTimer, 0f, 0f);
			}
		}
	}

	private void ProcessServerReloadTimer()
	{
		_reloadServerTimer += Time.deltaTime;
		if (_reloadServerTimer >= _reloadTime)
		{
			IsReloading = false;
			CancelInvoke(ProcessServerReloadTimer);
			_reloadServerTimer = 0f;
		}
	}

	[RPC_Server.InputValidation(new Type[] { typeof(ServersideMountedWeaponSnapshot) })]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(100uL)]
	[RPC_Server.MaxDistance(3f)]
	private void SV_ReceiveClientAim(RPCMessage msg)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (!_clientAuthority)
		{
			return;
		}
		BasePlayer player = msg.player;
		if ((Object)(object)_seat == (Object)null)
		{
			return;
		}
		BasePlayer mounted = _seat.GetMounted();
		if ((Object)(object)mounted == (Object)null || (Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)mounted || (Object)(object)mounted != (Object)(object)player)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = Mathf.Clamp(realtimeSinceStartup - _lastAimRpcTime, 0.0001f, 0.2f);
		ServersideMountedWeaponSnapshot val = msg.read.Proto<ServersideMountedWeaponSnapshot>((ServersideMountedWeaponSnapshot)null);
		try
		{
			float yaw = val.yaw;
			float pitch = val.pitch;
			WorldAngleToTurretAngle(yaw, pitch, out var turretYaw, out var turretPitch);
			Vector2 yawClamp = GetSeat().GetYawClamp();
			Vector2 pitchClamp = GetSeat().GetPitchClamp();
			if (antihack_level >= 1 && (turretYaw < yawClamp.x || turretYaw > yawClamp.y || turretPitch < pitchClamp.x || turretPitch > pitchClamp.y))
			{
				turretYaw = Mathf.Clamp(turretYaw, yawClamp.x, yawClamp.y);
				turretPitch = Mathf.Clamp(turretPitch, pitchClamp.x, pitchClamp.y);
				TurretAngleToWorldAngle(turretYaw, turretPitch, out _worldYaw, out _worldPitch);
				UpdateClient(force: true);
				return;
			}
			float num2 = Mathf.Abs(Mathf.DeltaAngle(_lastAimYaw, yaw));
			float num3 = Mathf.Abs(Mathf.DeltaAngle(_lastAimPitch, pitch));
			if (antihack_level >= 2 && (num2 > antihack_max_snap_degrees || num3 > antihack_max_snap_degrees))
			{
				UpdateClient(force: true);
				return;
			}
			if (antihack_level >= 3)
			{
				float num4 = num2 / num;
				float num5 = num3 / num;
				if (num4 > antihack_max_degrees_per_second_yaw || num5 > antihack_max_degrees_per_second_pitch)
				{
					UpdateClient(force: true);
					return;
				}
			}
			_worldYaw = yaw;
			_worldPitch = pitch;
			SetTargetAngles(_worldYaw, _worldPitch, set: true);
			if (Snapshot == null || Time.realtimeSinceStartup - _startTime - Snapshot.time >= 0.05f)
			{
				UpdateClient();
			}
			_lastAimRpcTime = realtimeSinceStartup;
			_lastAimYaw = yaw;
			_lastAimPitch = pitch;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Snapshot for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			using (TimeWarning.New("Objects"))
			{
				if (__sync_Snapshot == null)
				{
					__sync_Snapshot = Pool.Get<ServersideMountedWeaponSnapshot>();
				}
				SyncVarNetWrite<ServersideMountedWeaponSnapshot>(writer, __sync_Snapshot);
				return true;
			}
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: GunId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_GunId);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: Gun2Id for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_Gun2Id);
			return true;
		case 3:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: IsReloading for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_IsReloading);
			return true;
		case 4:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: IsEmpty for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_IsEmpty);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			try
			{
				ServersideMountedWeaponSnapshot val = __sync_Snapshot;
				ServersideMountedWeaponSnapshot _sync_Snapshot = reader.Proto<ServersideMountedWeaponSnapshot>((ServersideMountedWeaponSnapshot)null);
				__sync_Snapshot = _sync_Snapshot;
				if (fromAutoSave)
				{
					val = null;
				}
				if (val != null)
				{
					val.Dispose();
				}
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_GunId;
				NetworkableId _sync_GunId = reader.EntityID();
				__sync_GunId = _sync_GunId;
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_Gun2Id;
				NetworkableId _sync_Gun2Id = reader.EntityID();
				__sync_Gun2Id = _sync_Gun2Id;
			}
			catch (Exception ex5)
			{
				Debug.LogException(ex5);
			}
			return true;
		case 3:
			try
			{
				_ = __sync_IsReloading;
				bool _sync_IsReloading = reader.Bool();
				__sync_IsReloading = _sync_IsReloading;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 4:
			try
			{
				_ = __sync_IsEmpty;
				bool _sync_IsEmpty = reader.Bool();
				__sync_IsEmpty = _sync_IsEmpty;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"Snapshot" => 0, 
			"GunId" => 1, 
			"Gun2Id" => 2, 
			"IsReloading" => 3, 
			"IsEmpty" => 4, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
		WriteSyncVar(4, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(4, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_Snapshot = null;
		__sync_GunId = default(NetworkableId);
		__sync_Gun2Id = default(NetworkableId);
		__sync_IsReloading = false;
		__sync_IsEmpty = false;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			3 => true, 
			4 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	static MountedWeapon()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		antihack_level = 0;
		antihack_max_snap_degrees = 35f;
		antihack_max_degrees_per_second_yaw = 720f;
		antihack_max_degrees_per_second_pitch = 720f;
		ENABLE_CLIENT_AUTHORITY = true;
		DEBUG = false;
		Up = Animator.StringToHash("up");
		_ammoPhrase = new Phrase("mountedweapon.reload.tip", "You need regular 5.56 ammo in your inventory to reload.");
		_ammoFullPhrase = new Phrase("mountedweapon.reload.full.tip", "Can't reload. Ammo is already full!");
	}
}
