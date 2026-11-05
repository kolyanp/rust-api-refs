using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class BaseMountedUseable : BaseVehicleSeat
{
	[Header("Mounted Gun")]
	[SerializeField]
	protected bool isMountedOnVehicle = true;

	[SerializeField]
	private float turnSensivity = 2f;

	[SerializeField]
	private float fovMultiplier = 1f;

	[SerializeField]
	private CapsuleCollider playerServerCollider;

	[SerializeField]
	protected bool alignRotationToParent;

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
	[Tooltip("Applies all of the pitch/yaw transform in late update to allow for blending with animators.")]
	protected bool runInLateUpdate;

	[SerializeField]
	private bool useVehicleParentYaw;

	[SerializeField]
	protected bool canUseWhenRestrained = true;

	protected Vector3 aimDir;

	[Header("IK")]
	[SerializeField]
	public Transform leftHandTarget;

	[SerializeField]
	public Transform rightHandTarget;

	[Tooltip("Plants the gunner's feet on the ground under them and steps them as the gun turns. Turn it off where a collider covers the floor they stand on")]
	[SerializeField]
	private bool plantGunnerFeet = true;

	[Header("Sounds")]
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

	private float verticalRatio;

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations for this system (seismic sensor range sphere, escape capture state, etc.); editor/admin-only")]
	public static bool debug;

	private static readonly int AnimatorUp = Animator.StringToHash("up");

	private RealTimeSinceEx timeSinceLastServerTick;

	private Vector3 lastSentAimDir = Vector3.zero;

	public virtual bool RunInLateUpdate => runInLateUpdate;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMountedUseable.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected virtual bool CanRotate()
	{
		return AnyMounted();
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	private bool UpdateManualAim(InputState inputState)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		float pitchAdjustAmount = (0f - inputState.current.mouseDelta.y) * turnSensivity;
		float yawAdjustAmount = inputState.current.mouseDelta.x * turnSensivity;
		float num = turnSensivity * 2.5f;
		if (inputState.IsDown(BUTTON.LEFT))
		{
			yawAdjustAmount -= num;
		}
		if (inputState.IsDown(BUTTON.RIGHT))
		{
			yawAdjustAmount += num;
		}
		if (inputState.IsDown(BUTTON.BACKWARD))
		{
			pitchAdjustAmount += num;
		}
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			pitchAdjustAmount += 0f - num;
		}
		LimitAimInput(ref pitchAdjustAmount, ref yawAdjustAmount);
		Quaternion val = Quaternion.LookRotation(aimDir, ((Component)this).transform.up);
		Vector3 val2 = val.eulerAngles + new Vector3(pitchAdjustAmount, yawAdjustAmount, 0f);
		val2.x = ClampPitch(val2.x);
		Transform val3 = ((isMountedOnVehicle && (Object)(object)((Component)this).transform.parent != (Object)null) ? ((Component)this).transform.parent : ((Component)this).transform);
		float y = val2.y;
		val = Quaternion.LookRotation(val3.forward, ((Component)this).transform.up);
		val2.y = ClampYaw(y, val.eulerAngles.y);
		Vector3 val4 = Quaternion.Euler(val2) * Vector3.forward;
		bool result = !Mathf.Approximately(aimDir.x, val4.x) || !Mathf.Approximately(aimDir.y, val4.y) || !Mathf.Approximately(aimDir.z, val4.z);
		if (!CanAim())
		{
			return result;
		}
		aimDir = val4;
		return result;
	}

	protected virtual void LimitAimInput(ref float pitchAdjustAmount, ref float yawAdjustAmount)
	{
	}

	protected virtual bool CanAim()
	{
		return true;
	}

	public void RotateGun(float dt)
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
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
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
			float num2 = ClampYaw(val3.eulerAngles.y, 0f);
			Quaternion val4 = Quaternion.Euler(0f, num2, 0f);
			bool flag = TryGetAppliedAimDir(out var localPitchRot);
			Quaternion val5 = val.rotation * val4;
			bool flag2 = false;
			if (yawTransform.rotation != val5)
			{
				flag2 = !TryRotate(yawTransform, Mathx.Lerp(yawTransform.rotation, val5, num, dt));
			}
			if (((!RunInLateUpdate && ShouldApplyAimDir()) & flag) && pitchTransform.localRotation != localPitchRot)
			{
				Quaternion targetRotation = pitchTransform.parent.rotation * Mathx.Lerp(pitchTransform.localRotation, localPitchRot, num, dt);
				flag2 |= !TryRotate(pitchTransform, targetRotation);
			}
			if (flag2)
			{
				ResetAimToCurrentRotation();
			}
		}
	}

	protected virtual bool TryRotate(Transform rotatingTransform, Quaternion targetRotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		rotatingTransform.rotation = targetRotation;
		return true;
	}

	private void ResetAimToCurrentRotation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		aimDir = pitchTransform.forward;
		Transform transform = ((Component)GetRootParentEntity()).transform;
		if (useVehicleParentYaw && (Object)(object)transform != (Object)(object)((Component)this).transform)
		{
			aimDir = transform.InverseTransformDirection(aimDir);
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
		float num = Mathf.InverseLerp(pitchClamp.y, pitchClamp.x, Mathf.DeltaAngle(0f, val.eulerAngles.x));
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
		float num = ClampPitch(val3.eulerAngles.x);
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

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(ServerTick, Random.Range(0f, 1f), 0.015f);
	}

	private void ServerTick()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (isServer)
		{
			float dt = (float)(double)timeSinceLastServerTick;
			timeSinceLastServerTick = 0.0;
			if (CanRotate())
			{
				RotateGun(dt);
			}
			BasePlayer mounted = GetMounted();
			if ((Object)(object)mounted != (Object)null && (Object)(object)((Component)mounted).transform.parent != (Object)(object)mountAnchor)
			{
				((Component)mounted).transform.SetPositionAndRotation(mountAnchor.position, mountAnchor.rotation);
			}
			if (ChecksOwnSeatClipping() && AnyMounted() && IsSeatClipping(this))
			{
				DismountAllPlayers();
			}
		}
	}

	protected virtual bool ChecksOwnSeatClipping()
	{
		return true;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		UpdateManualAim(inputState);
	}

	public override bool CanFireMountedWeapon(BasePlayer player)
	{
		if (!canUseWhenRestrained)
		{
			return !player.IsRestrained;
		}
		return true;
	}

	public override bool CanFireMountedWeaponAt(Vector3 direction)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Transform val = (isMountedOnVehicle ? ((Component)this).transform.parent : ((Component)this).transform);
		if (useVehicleParentYaw)
		{
			val = ((Component)GetRootParentEntity()).transform;
		}
		if ((Object)(object)val == (Object)null || direction == Vector3.zero)
		{
			return false;
		}
		Quaternion val2 = Quaternion.LookRotation(val.InverseTransformDirection(direction), Vector3.up);
		Vector3 eulerAngles = val2.eulerAngles;
		float num = Mathf.DeltaAngle(0f, eulerAngles.x);
		float num2 = Mathf.DeltaAngle(0f, eulerAngles.y);
		if (num >= pitchClamp.x - 15f && num <= pitchClamp.y + 15f && num2 >= yawClamp.x - 15f)
		{
			return num2 <= yawClamp.y + 15f;
		}
		return false;
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		base.OnParentChanging(oldParent, newParent);
		if (isMountedOnVehicle && (Object)(object)newParent != (Object)null && aimDir == Vector3.zero)
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
		SendAimDirImmediate(force: true);
		InvokeRandomized(SendAimDir, Random.Range(0f, 1f), 0.2f, 0.05f);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		TogglePlayerServerCollider(active: false);
		if (IsInvoking(SendAimDir))
		{
			CancelInvoke(SendAimDir);
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

	public void TogglePlayerServerCollider(bool active)
	{
		if ((Object)(object)playerServerCollider != (Object)null)
		{
			((Collider)playerServerCollider).enabled = active;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (CanRotate() || !isMountedOnVehicle)
		{
			RotateGun(1000f);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (SavesGunState())
		{
			info.msg.ballistaGun = Pool.Get<BallistaGun>();
			info.msg.ballistaGun.aimDir = aimDir;
		}
	}

	protected virtual bool SavesGunState()
	{
		return true;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (info.msg.ballistaGun != null && isServer)
		{
			aimDir = info.msg.ballistaGun.aimDir;
		}
		base.Load(info);
	}

	public BaseMountedUseable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
	}
}
