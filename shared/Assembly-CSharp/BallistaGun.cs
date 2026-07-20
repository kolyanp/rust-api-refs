using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class BallistaGun : BaseVehicleSeat
{
	[Serializable]
	private struct Ammo
	{
		public ItemDefinition item;

		public GameObject go;
	}

	[Serializable]
	private struct FiringEffect
	{
		public ItemDefinition item;

		public GameObjectRef effectPrefab;
	}

	private enum AimDirection
	{
		Left,
		Right,
		Up,
		Down
	}

	[SerializeField]
	[Header("Ballista")]
	private bool isMountedOnVehicle = true;

	[SerializeField]
	private float turnSensivity = 2f;

	[SerializeField]
	private float reloadTime = 3f;

	[SerializeField]
	private bool syncAimDirOnFire = true;

	[SerializeField]
	private bool syncAimDirOnReload = true;

	[SerializeField]
	private bool reloadPreventsAiming;

	[SerializeField]
	private float fovMultiplier = 1f;

	[SerializeField]
	private bool noHeadshots = true;

	[SerializeField]
	private CapsuleCollider playerServerCollider;

	[SerializeField]
	private bool alignRotationToParent;

	[SerializeField]
	protected BaseProjectile.Magazine magazine;

	[Space]
	[SerializeField]
	protected Transform muzzle;

	[SerializeField]
	protected Transform pitchTransform;

	[SerializeField]
	public Transform yawTransform;

	[SerializeField]
	public Transform mountTransform;

	[SerializeField]
	protected Animator animator;

	[Tooltip("Applies all of the pitch/yaw transform in late update to allow for blending with animators.")]
	[SerializeField]
	protected bool runInLateUpdate;

	[SerializeField]
	private GameObject ammoParent;

	[SerializeField]
	private bool useVehicleParentYaw;

	[SerializeField]
	protected BUTTON reloadButton = BUTTON.RELOAD;

	public DamageRenderer damageRenderer;

	protected Vector3 aimDir;

	[SerializeField]
	private Ammo[] ammoPrefabs;

	[Header("IK")]
	[SerializeField]
	public Transform leftHandTarget;

	[SerializeField]
	public Transform rightHandTarget;

	[Header("Effects")]
	[SerializeField]
	private FiringEffect[] muzzleFireEffects;

	[SerializeField]
	private SoundDefinition reloadedSound;

	[SerializeField]
	private SoundDefinition aimMovementSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementYawSoundDef;

	[SerializeField]
	private SoundDefinition aimMovementPitchSoundDef;

	[SerializeField]
	private AnimationCurve aimMovementGainCurve;

	private Sound aimMovementSound;

	private SoundModulation.Modulator aimMovementGainMod;

	private Sound aimMovementYawSound;

	private SoundModulation.Modulator aimMovementYawGainMod;

	private Sound aimMovementPitchSound;

	private SoundModulation.Modulator aimMovementPitchGainMod;

	[SerializeField]
	[Space]
	private bool runSideChecks;

	[SerializeField]
	private Transform leftGroundCheckTransform;

	[SerializeField]
	private Transform rightGroundCheckTransform;

	[SerializeField]
	private Transform[] leftSideCheckPositions;

	[SerializeField]
	private Transform[] rightSideCheckPositions;

	[SerializeField]
	private Vector3 originalLocalMountPos;

	[SerializeField]
	private bool runBoundsChecks;

	[SerializeField]
	private Bounds[] areaChecks;

	[NonSerialized]
	public Ballista ballistaOwner;

	[HideInInspector]
	public float reloadProgress;

	private bool justReloaded;

	private BasePlayer reloadingPlayer;

	private float steerInput;

	private float verticalRatio;

	private bool wasShowingLegs;

	private TimeSince lastReloadStartTime;

	private float fixedMountYRotation;

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations for this system (seismic sensor range sphere, escape capture state, etc.); editor/admin-only")]
	public static bool debug;

	private static readonly int AnimatorUp = Animator.StringToHash("up");

	protected static readonly int Reload = Animator.StringToHash("Reload");

	public const Flags Flag_Reloading = Flags.Reserved4;

	public const Flags Flag_Loaded = Flags.Reserved5;

	private readonly float progressTickRate = 0.1f;

	private RealTimeSinceEx timeSinceLastServerTick;

	private Vector3 lastSentAimDir = Vector3.zero;

	public virtual bool RunInLateUpdate => runInLateUpdate;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BallistaGun.OnRpcMessage"))
		{
			if (rpc == 1188838966 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_CancelReload"));
				}
				using (TimeWarning.New("SERVER_CancelReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1188838966u, "SERVER_CancelReload", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(1188838966u, "SERVER_CancelReload", this, player))
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
							SERVER_CancelReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_CancelReload");
					}
				}
				return true;
			}
			if (rpc == 296086248 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_FireClientProjectile"));
				}
				using (TimeWarning.New("SERVER_FireClientProjectile"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(296086248u, "SERVER_FireClientProjectile", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(296086248u, "SERVER_FireClientProjectile", this, player))
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
							SERVER_FireClientProjectile(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_FireClientProjectile");
					}
				}
				return true;
			}
			if (rpc == 2817383917u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_ReloadStart"));
				}
				using (TimeWarning.New("SERVER_ReloadStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2817383917u, "SERVER_ReloadStart", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(2817383917u, "SERVER_ReloadStart", this, player))
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
							SERVER_ReloadStart(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_ReloadStart");
					}
				}
				return true;
			}
			if (rpc == 4118009042u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SwitchAmmoTo"));
				}
				using (TimeWarning.New("SERVER_SwitchAmmoTo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4118009042u, "SERVER_SwitchAmmoTo", this, player, 5uL))
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
							SERVER_SwitchAmmoTo(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_SwitchAmmoTo");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool HasOwner()
	{
		return (Object)(object)ballistaOwner != (Object)null;
	}

	public bool IsLoaded()
	{
		if (HasFlag(Flags.Reserved5) && magazine.contents == magazine.capacity)
		{
			return reloadProgress >= 0.94f;
		}
		return false;
	}

	private bool CanFire()
	{
		if (IsLoaded() && !HasFlag(Flags.Reserved4))
		{
			return !OwnerIsWaterlogged();
		}
		return false;
	}

	protected bool CanReload()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsLoaded() && TimeSince.op_Implicit(lastReloadStartTime) > 1f)
		{
			return !OwnerIsWaterlogged();
		}
		return false;
	}

	public virtual bool OwnerIsWaterlogged()
	{
		if (HasOwner())
		{
			return ballistaOwner.IsWaterlogged();
		}
		return false;
	}

	public override void InitShared()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		originalLocalMountPos = mountAnchor.localPosition;
	}

	private bool UpdateManualAim(InputState inputState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		float y = mountAnchor.position.y;
		MoveMountAnchor();
		float num = y - mountAnchor.position.y;
		float num2 = 0f;
		if (Mathf.Abs(num) > 0.0001f)
		{
			num2 = (0f - num) * 50f;
		}
		float num3 = (0f - inputState.current.mouseDelta.y) * turnSensivity;
		float num4 = inputState.current.mouseDelta.x * turnSensivity;
		float num5 = turnSensivity * 2.5f;
		if (inputState.IsDown(BUTTON.LEFT))
		{
			num4 -= num5;
		}
		if (inputState.IsDown(BUTTON.RIGHT))
		{
			num4 += num5;
		}
		if (inputState.IsDown(BUTTON.BACKWARD))
		{
			num3 += num5;
		}
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			num3 += 0f - num5;
		}
		num3 += num2;
		if (!CanRotateInDirection(num4 > 0f))
		{
			num4 = 0f;
		}
		if (runBoundsChecks)
		{
			if (num4 != 0f && !CheckBallistaBounds((!(num4 < 0f)) ? AimDirection.Right : AimDirection.Left))
			{
				num4 = 0f;
			}
			if (num3 != 0f && !CheckBallistaBounds((num3 < 0f) ? AimDirection.Up : AimDirection.Down))
			{
				num3 = 0f;
			}
		}
		Quaternion val = Quaternion.LookRotation(aimDir, ((Component)this).transform.up);
		Vector3 val2 = ((Quaternion)(ref val)).eulerAngles + new Vector3(num3, num4, 0f);
		val2.x = ClampPitch(val2.x);
		Transform val3 = (HasOwner() ? ((Component)this).transform.parent : yawTransform);
		float y2 = val2.y;
		val = Quaternion.LookRotation(val3.forward, ((Component)this).transform.up);
		val2.y = ClampYaw(y2, ((Quaternion)(ref val)).eulerAngles.y);
		Vector3 val4 = Quaternion.Euler(val2) * Vector3.forward;
		bool result = !Mathf.Approximately(aimDir.x, val4.x) || !Mathf.Approximately(aimDir.y, val4.y) || !Mathf.Approximately(aimDir.z, val4.z);
		if (reloadPreventsAiming && HasFlag(Flags.Reserved4))
		{
			return result;
		}
		aimDir = val4;
		return result;
	}

	private bool CanRotateInDirection(bool rotatingLeft)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Transform checkTransform = (rotatingLeft ? leftGroundCheckTransform : rightGroundCheckTransform);
		Transform[] array = (rotatingLeft ? leftSideCheckPositions : rightSideCheckPositions);
		if (!HasGround(checkTransform))
		{
			return false;
		}
		if (runSideChecks)
		{
			Transform[] array2 = array;
			foreach (Transform val in array2)
			{
				if (HasColliderBlockingRotation(val.position, val.forward))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void RotateBallista(float dt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		if ((isMountedOnVehicle && (Object)(object)((Component)this).transform.parent == (Object)null) || aimDir == Vector3.zero)
		{
			return;
		}
		float num = 50f;
		Transform val = (isMountedOnVehicle ? ((Component)this).transform.parent : ((Component)this).transform);
		if (useVehicleParentYaw)
		{
			val = ((Component)GetRootParentEntity()).transform;
		}
		Vector3 val2 = val.InverseTransformDirection(aimDir);
		if (useVehicleParentYaw && (Object)(object)val != (Object)(object)((Component)this).transform)
		{
			val2 = aimDir;
		}
		if (!(val2 == Vector3.zero))
		{
			Quaternion val3 = Quaternion.LookRotation(val2, Vector3.up);
			float num2 = ClampYaw(((Quaternion)(ref val3)).eulerAngles.y, 0f);
			Quaternion val4 = Quaternion.Euler(0f, num2, 0f);
			Quaternion localPitchRot;
			bool flag = TryGetAppliedAimDir(out localPitchRot);
			Quaternion val5 = val.rotation * val4;
			if (yawTransform.rotation != val5)
			{
				yawTransform.rotation = Mathx.Lerp(yawTransform.rotation, val5, num, dt);
			}
			if (!RunInLateUpdate && ShouldApplyAimDir() && flag && pitchTransform.localRotation != localPitchRot)
			{
				pitchTransform.localRotation = Mathx.Lerp(pitchTransform.localRotation, localPitchRot, num, dt);
			}
		}
	}

	private void UpdatePlayerModelPose()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.LookRotation(muzzle.forward, Vector3.up);
		float num = Mathf.InverseLerp(pitchClamp.y, pitchClamp.x, Mathf.DeltaAngle(0f, ((Quaternion)(ref val)).eulerAngles.x));
		verticalRatio = Mathf.Lerp(-1f, 1f, num);
	}

	private float ClampPitch(float pitch)
	{
		float num = Mathf.Clamp(Mathf.DeltaAngle(0f, pitch), pitchClamp.x, pitchClamp.y);
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	private float ClampYaw(float targetYaw, float parentYaw)
	{
		float num = Mathf.DeltaAngle(parentYaw, targetYaw);
		num = Mathf.Clamp(num, yawClamp.x, yawClamp.y);
		return parentYaw + num;
	}

	protected virtual bool TryGetPitchOverride(float basePitch, out float overridePitch, out float overrideWeight)
	{
		overridePitch = basePitch;
		overrideWeight = 0f;
		return false;
	}

	protected bool TryGetAppliedAimDir(out Quaternion localPitchRot)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		localPitchRot = Quaternion.identity;
		if (isMountedOnVehicle && (Object)(object)((Component)this).transform.parent == (Object)null)
		{
			return false;
		}
		if (aimDir == Vector3.zero)
		{
			return false;
		}
		Transform val = (isMountedOnVehicle ? ((Component)this).transform.parent : ((Component)this).transform);
		if (useVehicleParentYaw)
		{
			val = ((Component)GetRootParentEntity()).transform;
		}
		Vector3 val2 = val.InverseTransformDirection(aimDir);
		if (useVehicleParentYaw && (Object)(object)val != (Object)(object)((Component)this).transform)
		{
			val2 = aimDir;
		}
		if (val2 == Vector3.zero)
		{
			return false;
		}
		Quaternion val3 = Quaternion.LookRotation(val2, Vector3.up);
		float num = ClampPitch(((Quaternion)(ref val3)).eulerAngles.x);
		if (TryGetPitchOverride(num, out var overridePitch, out var overrideWeight))
		{
			num = Mathf.LerpAngle(num, overridePitch, Mathf.Clamp01(overrideWeight));
		}
		localPitchRot = Quaternion.Euler(num, 0f, 0f);
		return true;
	}

	protected virtual bool ShouldApplyAimDir()
	{
		return true;
	}

	protected Item GetAmmoFromPlayerInventory(BasePlayer player)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Item item = player.inventory.FindItemByItemID(magazine.ammoType.itemid);
		if (item == null && !magazine.allowAmmoSwitching)
		{
			return null;
		}
		if (item == null)
		{
			Item item2 = player.inventory.FindAmmo(magazine.definition.ammoTypes);
			if (item2 == null)
			{
				return null;
			}
			item = player.inventory.FindItemByItemID(item2.info.itemid);
			if (item == null)
			{
				return null;
			}
		}
		return item;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	protected virtual bool HasGround(Transform checkTransform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		float num = 1f;
		Vector3 up = checkTransform.up;
		Vector3 val2 = -up;
		Vector3 val3 = up * 0.6f;
		if (Physics.SphereCast(checkTransform.position + val3, 0.5f, val2, ref val, num, 1503731969))
		{
			return ((RaycastHit)(ref val)).normal.y > 0f;
		}
		return false;
	}

	private bool HasColliderBlockingRotation(Vector3 origin, Vector3 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.2f;
		RaycastHit val = default(RaycastHit);
		return Physics.SphereCast(origin, 0.05f, direction, ref val, num, 1503731969);
	}

	protected virtual void MoveMountAnchor()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		float num = 2f;
		Vector3 val = mountAnchor.parent.TransformPoint(originalLocalMountPos) + Vector3.up * 0.8f;
		Vector3 down = Vector3.down;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.SphereCast(val, 0.05f, down, ref val2, num, 1503731969))
		{
			float y = ((RaycastHit)(ref val2)).point.y;
			float y2 = mountAnchor.parent.TransformPoint(originalLocalMountPos).y;
			if (Mathf.Abs(y - y2) < 0.5f)
			{
				mountAnchor.position = ((RaycastHit)(ref val2)).point;
			}
		}
	}

	private bool CanRotate()
	{
		if (HasOwner())
		{
			return !ballistaOwner.IsStationary();
		}
		return AnyMounted();
	}

	protected bool CanSeeFirePoint(BasePlayer player, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = player.eyes.center;
		Vector3 position = player.eyes.position;
		Vector3 position2 = muzzle.position;
		int layerMask = 2162689;
		if (GamePhysics.LineOfSightRadius(center, position, layerMask, radius, this))
		{
			return GamePhysics.LineOfSightRadius(position, position2, layerMask, radius, this);
		}
		return false;
	}

	private bool CheckBallistaBounds(AimDirection direction)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		List<Bounds> list = Pool.Get<List<Bounds>>();
		bool num = direction == AimDirection.Up || direction == AimDirection.Down;
		Vector3 val = pitchTransform.position;
		if (num)
		{
			Vector3 val2 = ((direction == AimDirection.Up) ? pitchTransform.up : (-pitchTransform.up));
			val += ((Vector3)(ref val2)).normalized * 0.05f;
			list.Add(areaChecks[areaChecks.Length - 1]);
		}
		else
		{
			int num2 = ((direction != AimDirection.Left) ? 1 : 0);
			list.Add(areaChecks[num2]);
			list.Add(areaChecks[areaChecks.Length - 1]);
		}
		bool result = true;
		foreach (Bounds item in list)
		{
			if (SocketMod_AreaCheck.IsInArea(val, new OBB(val, pitchTransform.rotation, item), LayerMask.op_Implicit(1503731969), out var _, wantsInside: true, shouldParent: false, null, this))
			{
				result = false;
				break;
			}
		}
		Pool.FreeUnmanaged<Bounds>(ref list);
		return result;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(ServerTick, Random.Range(0f, 1f), 0.015f);
	}

	private void ServerTick()
	{
		if (base.isServer)
		{
			float dt = (float)(double)timeSinceLastServerTick;
			timeSinceLastServerTick = 0.0;
			if (CanRotate())
			{
				RotateBallista(dt);
			}
			if (!HasOwner() && AnyMounted() && IsSeatClipping(this))
			{
				DismountAllPlayers();
			}
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		UpdateManualAim(inputState);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		base.OnParentChanging(oldParent, newParent);
		if ((Object)(object)newParent != (Object)null && aimDir == Vector3.zero)
		{
			aimDir = ((Component)newParent).transform.forward;
		}
	}

	public override void OnPlayerMounted()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.OnPlayerMounted();
		TogglePlayerServerCollider(active: true);
		aimDir = pitchTransform.forward;
		if (alignRotationToParent && (Object)(object)((Component)this).transform.parent != (Object)null)
		{
			aimDir = ((Component)this).transform.parent.InverseTransformDirection(aimDir);
		}
		SendAimDirImmediate();
		InvokeRandomized(SendAimDir, Random.Range(0f, 1f), 0.2f, 0.05f);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		TogglePlayerServerCollider(active: false);
		if (HasFlag(Flags.Reserved4))
		{
			StopReload();
		}
		if (IsInvoking(SendAimDir))
		{
			CancelInvoke(SendAimDir);
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (HasGround(rightGroundCheckTransform) && HasGround(leftGroundCheckTransform))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	protected virtual void LoadAmmo(BasePlayer player)
	{
		if ((Object)(object)player != (Object)(object)GetMounted() || IsLoaded())
		{
			return;
		}
		Item ammoFromPlayerInventory = GetAmmoFromPlayerInventory(player);
		if (ammoFromPlayerInventory != null)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: true);
			}
			magazine.ammoType = ammoFromPlayerInventory.info;
			magazine.contents = 1;
			ammoFromPlayerInventory.UseItem();
		}
	}

	public void SendAimDir()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (lastSentAimDir == Vector3.zero || Vector3.Angle(lastSentAimDir, aimDir) > 0.03f)
		{
			SendAimDirImmediate();
		}
	}

	public void SendAimDirImmediate(bool force = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		lastSentAimDir = aimDir;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_ReceiveAimDir"), aimDir, force);
	}

	protected virtual void ReloadProgress()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)reloadingPlayer == (Object)null || (Object)(object)reloadingPlayer != (Object)(object)GetMounted() || reloadingPlayer.IsDead() || reloadingPlayer.IsSleeping() || Vector3Ex.Distance2D(((Component)reloadingPlayer).transform.position, ((Component)this).transform.position) > 3f)
		{
			StopReload();
			return;
		}
		reloadProgress += progressTickRate / reloadTime;
		if (reloadProgress >= 1f)
		{
			reloadProgress = 1f;
			LoadAmmo(reloadingPlayer);
			if (syncAimDirOnReload)
			{
				Invoke(delegate
				{
					SendAimDirImmediate(force: true);
				}, 0.5f);
			}
			StopReload();
		}
		else
		{
			SendNetworkUpdateImmediate();
		}
	}

	public virtual void StopReload()
	{
		CancelInvoke(ReloadProgress);
		reloadingPlayer = null;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, b: false);
	}

	public void TogglePlayerServerCollider(bool active)
	{
		((Collider)playerServerCollider).enabled = active;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	private void SERVER_SwitchAmmoTo(RPCMessage msg)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null || (Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)mounted)
		{
			return;
		}
		int num = msg.read.Int32();
		if (num == magazine.ammoType.itemid)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(num);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return;
		}
		ItemModProjectile component = ((Component)itemDefinition).GetComponent<ItemModProjectile>();
		if (!Object.op_Implicit((Object)(object)component) || !component.IsAmmo(magazine.definition.ammoTypes))
		{
			return;
		}
		if (magazine.contents > 0)
		{
			mounted.GiveItem(ItemManager.CreateByItemID(magazine.ammoType.itemid, magazine.contents, 0uL, 0uL));
			magazine.contents = 0;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		magazine.ammoType = itemDefinition;
		SendNetworkUpdateImmediate();
		ItemManager.DoRemoves();
		mounted.inventory.ServerUpdate(0f);
	}

	[RPC_Server.FromMounted]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	private void SERVER_FireClientProjectile(RPCMessage msg)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (IsFireRPCInvalid(msg, player, out var ammoItem, out var itemModProjectile))
		{
			return;
		}
		ProjectileShoot val = msg.read.Proto<ProjectileShoot>((ProjectileShoot)null);
		try
		{
			if (val.projectiles.Count != 1)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Projectile count mismatch (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, null, "count_mismatch");
				return;
			}
			player.CleanupExpiredProjectiles();
			Guid projectileGroupId = Guid.NewGuid();
			foreach (Projectile projectile in val.projectiles)
			{
				if (player.HasFiredProjectile(projectile.projectileID))
				{
					AntiHack.Log(player, AntiHackType.ProjectileHack, "Duplicate ID (" + projectile.projectileID + ")");
					player.stats.combat.LogInvalid(player, null, "duplicate_id");
				}
				else if (ValidateFirePos(player, projectile.startPos))
				{
					player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, null, ammoItem, projectileGroupId, Vector3.zero);
					Effect effect = new Effect();
					effect.Init(Effect.Type.Projectile, projectile.startPos, projectile.startVel, msg.connection);
					((EffectData)effect).scale = 1f;
					effect.pooledString = itemModProjectile.GetOverrideProjectile(this).resourcePath;
					((EffectData)effect).number = projectile.seed;
					EffectNetwork.Send(effect);
				}
			}
			val.Dispose();
			SERVER_OnProjectileFired(msg.connection, player);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual bool IsFireRPCInvalid(RPCMessage msg, BasePlayer player, out ItemDefinition ammoItem, out ItemModProjectile itemModProjectile)
	{
		if ((Object)(object)player == (Object)null)
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!IsLoaded() || magazine.contents != 1)
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if ((Object)(object)player != (Object)(object)GetMounted())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!CanFire())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (player.InSafeZone())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		ammoItem = magazine.ammoType;
		if ((Object)(object)ammoItem == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "item_missing");
			itemModProjectile = null;
			return true;
		}
		itemModProjectile = ((Component)ammoItem).GetComponent<ItemModProjectile>();
		if ((Object)(object)itemModProjectile == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "mod_missing");
			return true;
		}
		return false;
	}

	protected void SERVER_OnProjectileFired(Connection connection, BasePlayer player)
	{
		if (syncAimDirOnFire)
		{
			SendAimDirImmediate(force: true);
		}
		player.MarkHostileFor();
		SignalBroadcast(Signal.Attack, string.Empty, connection);
		magazine.contents = 0;
		reloadProgress = 0f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		if (HasOwner())
		{
			ballistaOwner.RefreshLastUseTime();
			ballistaOwner.OnFired();
		}
		SingletonComponent<NpcNoiseManager>.Instance.OnWeaponShot(player, null);
	}

	protected bool VerifyClientRPC(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			Debug.LogWarning((object)"Received RPC from null player");
			return false;
		}
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Owner not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "owner_missing");
			return false;
		}
		if ((Object)(object)mounted != (Object)(object)player)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player mismatch (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_mismatch");
			return false;
		}
		if (player.IsDead())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player dead (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_dead");
			return false;
		}
		if (player.IsWounded())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player down (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_down");
			return false;
		}
		if (player.IsSleeping())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player sleeping (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_sleeping");
			return false;
		}
		if (player.desyncTimeRaw > ConVar.AntiHack.maxdesync)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player stalled (" + base.ShortPrefabName + " with " + player.desyncTimeRaw + "s)");
			player.stats.combat.LogInvalid(player, null, "player_stalled");
			return false;
		}
		if ((Object)(object)magazine.ammoType == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Item not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "item_missing");
			return false;
		}
		return true;
	}

	protected unsafe bool ValidateFirePos(BasePlayer player, Vector3 firePos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (Vector3Ex.IsNaNOrInfinity(firePos))
		{
			string shortPrefabName = base.ShortPrefabName;
			AntiHack.Log(player, AntiHackType.EyeHack, "Contains NaN (" + shortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "eye_nan");
			flag = false;
		}
		if (ConVar.AntiHack.eye_protection > 0)
		{
			Vector3 val;
			if (ConVar.AntiHack.eye_protection >= 1)
			{
				val = player.GetParentVelocity();
				float magnitude = ((Vector3)(ref val)).magnitude;
				val = player.GetMountVelocity();
				float num = magnitude + ((Vector3)(ref val)).magnitude + ConVar.AntiHack.eye_forgiveness;
				float num2 = Vector3.Distance(((Component)muzzle).transform.position, firePos);
				if (num2 > num)
				{
					string shortPrefabName2 = base.ShortPrefabName;
					AntiHack.Log(player, AntiHackType.EyeHack, "Distance (" + shortPrefabName2 + " on attack with " + num2 + "m > " + num + "m)");
					player.stats.combat.LogInvalid(player, null, "eye_distance");
					flag = false;
				}
			}
			int num3 = 2162688;
			if (ConVar.AntiHack.eye_terraincheck)
			{
				num3 |= 0x800000;
			}
			if (ConVar.AntiHack.eye_vehiclecheck)
			{
				num3 |= 0x8000000;
			}
			if (ConVar.AntiHack.eye_protection >= 2 && !CanSeeFirePoint(player, 0.05f))
			{
				string shortPrefabName3 = base.ShortPrefabName;
				string[] obj = new string[8] { "Line of sight (", shortPrefabName3, " on attack) ", null, null, null, null, null };
				val = player.eyes.center;
				obj[3] = ((object)(*(Vector3*)(&val))/*cast due to constrained. prefix*/).ToString();
				obj[4] = " ";
				val = player.eyes.position;
				obj[5] = ((object)(*(Vector3*)(&val))/*cast due to constrained. prefix*/).ToString();
				obj[6] = " ";
				obj[7] = ((object)(*(Vector3*)(&firePos))/*cast due to constrained. prefix*/).ToString();
				AntiHack.Log(player, AntiHackType.EyeHack, string.Concat(obj));
				player.stats.combat.LogInvalid(player, null, "eye_los");
				flag = false;
			}
		}
		if (!flag)
		{
			AntiHack.AddViolation(player, AntiHackType.EyeHack, ConVar.AntiHack.eye_penalty);
		}
		return flag;
	}

	protected override bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return signal == Signal.Attack;
	}

	protected virtual bool UnableToStartReloadServer(BasePlayer player)
	{
		return false;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.FromMounted]
	[RPC_Server]
	private void SERVER_ReloadStart(RPCMessage msg)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		BasePlayer mounted = GetMounted();
		if (!((Object)(object)mounted == (Object)null) && !((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)mounted) && !UnableToStartReloadServer(player) && Interface.CallHook("OnBallistaGunReload", this, player) == null)
		{
			reloadingPlayer = player;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved4, b: true);
			}
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartReloading"), reloadingPlayer.net.ID);
			InvokeRepeating(ReloadProgress, 0f, progressTickRate);
			if (HasOwner())
			{
				ballistaOwner.RefreshLastUseTime();
			}
			Server_OnReloadStarted();
		}
	}

	protected virtual void Server_OnReloadStarted()
	{
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.FromMounted]
	public void SERVER_CancelReload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)reloadingPlayer))
		{
			StopReload();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (CanRotate())
		{
			RotateBallista(1000f);
		}
	}

	public override void DoRepair(BasePlayer player)
	{
		if (HasParent() && HasOwner())
		{
			ballistaOwner.DoRepair(player);
		}
		else
		{
			base.DoRepair(player);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (HasParent() && HasOwner())
		{
			ballistaOwner.Hurt(info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (HasParent() && HasOwner() && !ballistaOwner.IsDead())
		{
			ballistaOwner.Die();
		}
	}

	public void AdminReload(int ammo)
	{
		reloadProgress = 1f;
		StopReload();
		ItemDefinition itemDefinition = null;
		ammo = Mathf.Clamp(ammo, 0, ammoPrefabs.Length - 1);
		itemDefinition = ammoPrefabs[ammo].item;
		SetFlagLocal(Flags.Reserved5, b: true);
		magazine.ammoType = itemDefinition;
		magazine.contents = 1;
		SendNetworkUpdateImmediate();
	}

	[ServerVar(Help = "(Generated) Forces the ballista gun nearest to the calling admin player to reload immediately; admin-only")]
	public static void reload(ConsoleSystem.Arg arg)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
		}
		else
		{
			if (!basePlayer.IsAdmin)
			{
				return;
			}
			int ammo = arg.GetInt(0);
			BallistaGun[] array = Util.FindAll<BallistaGun>();
			int num = 0;
			BallistaGun[] array2 = array;
			foreach (BallistaGun ballistaGun in array2)
			{
				if (ballistaGun.isServer && Vector3.Distance(((Component)ballistaGun).transform.position, ((Component)basePlayer).transform.position) <= 10f)
				{
					ballistaGun.AdminReload(ammo);
					num++;
				}
			}
			arg.ReplyWith($"Reloaded {num} ballistas.");
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!isMountedOnVehicle || HasOwner())
		{
			info.msg.ballistaGun = Pool.Get<BallistaGun>();
			info.msg.ballistaGun.magazine = magazine.Save();
			info.msg.ballistaGun.reloadProgress = reloadProgress;
			info.msg.ballistaGun.aimDir = aimDir;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (info.msg.ballistaGun != null)
		{
			if (info.msg.ballistaGun.magazine != null)
			{
				magazine.Load(info.msg.ballistaGun.magazine);
			}
			if (base.isServer)
			{
				reloadProgress = info.msg.ballistaGun.reloadProgress;
				aimDir = info.msg.ballistaGun.aimDir;
			}
		}
		base.Load(info);
	}
}
