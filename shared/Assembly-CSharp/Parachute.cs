using Facepunch.Rust;
using Rust;
using UnityEngine;

public class Parachute : BaseVehicle, SamSite.ISamSiteTarget
{
	public Collider ParachuteCollider;

	public ItemDefinition PackedParachute;

	public GameObjectRef DetachedParachute;

	public Transform DetachedSpawnPoint;

	public float ConditionLossPerUse;

	public float HurtDeployTime;

	public float HurtAmount;

	public Animator ColliderAnimator;

	public Animator ColliderWorldAnimator;

	public float UprightLerpForce;

	public float ConstantForwardForce;

	public ForceMode ForwardForceMode;

	public float TurnForce;

	public ForceMode TurnForceMode;

	public float ForwardTiltAcceleration;

	public float BackInputForceMultiplier;

	public float DeployAnimationLength;

	public float TargetDrag;

	public float TargetAngularDrag;

	public AnimationCurve DragCurve;

	public AnimationCurve DragDamageCurve;

	public AnimationCurve MassDamageCurve;

	public AnimationCurve DamageHorizontalVelocityCurve;

	[Range(0f, 1f)]
	public float DamageTester;

	public float AnimationInputSmoothness;

	public Vector2 AnimationInputScale;

	public ParachuteWearable FirstPersonCanopy;

	public GameObjectRef ParachuteLandScreenBounce;

	private static readonly int AnimatorInputXParameter = Animator.StringToHash("InputX");

	private static readonly int AnimatorInputYParameter = Animator.StringToHash("InputY");

	private TimeSince mountTime;

	public const Flags Flag_InputForward = Flags.Reserved1;

	public const Flags Flag_InputBack = Flags.Reserved2;

	public const Flags Flag_InputLeft = Flags.Reserved3;

	public const Flags Flag_InputRight = Flags.Reserved4;

	public SoundDefinition deploySoundDef;

	public SoundDefinition releaseSoundDef;

	public SoundDefinition flightLoopSoundDef;

	public SoundDefinition steerSoundDef;

	public AnimationCurve flightLoopPitchCurve;

	public AnimationCurve flightLoopGainCurve;

	[ServerVar(Saved = true)]
	public static bool BypassRepack = false;

	[ServerVar(Saved = true)]
	public static bool LandingAnimations = false;

	public bool collisionDeath;

	public Vector3 collisionImpulse;

	private float startHeight;

	private float distanceTravelled;

	private Vector3 lastPosition;

	private Vector2 lerpedInput;

	public Vector3 collisionLocalPos;

	public Vector3 collisionWorldNormal;

	protected override bool BypassClothingMountBlocks => true;

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeVehicle;

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerMounted(player, seat);
		rigidBody.linearVelocity = player.estimatedVelocity;
		mountTime = TimeSince.op_Implicit(0f);
		startHeight = ((Component)this).transform.position.y;
		distanceTravelled = 0f;
		canTriggerParent = false;
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		ParachuteCollider.enabled = false;
		bool dismountPosition = base.GetDismountPosition(player, out res, silent);
		ParachuteCollider.enabled = true;
		return dismountPosition;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerServerInput(inputState, player);
		player.PlayHeavyLandingAnimation = true;
		Vector3 position = ((Component)this).transform.position;
		float num = Vector3.Distance(lastPosition, position);
		distanceTravelled += num;
		lastPosition = position;
		if (WaterLevel.Test(((Component)this).transform.position, waves: true, volumes: true, this))
		{
			DismountAllPlayers();
		}
		else if (!(TimeSince.op_Implicit(mountTime) < DeployAnimationLength))
		{
			Vector2 val = ProcessInputVector(inputState, player);
			lerpedInput = Vector2.Lerp(lerpedInput, val, Time.deltaTime * 5f);
			ColliderAnimator.SetFloat(AnimatorInputXParameter, lerpedInput.x);
			ColliderAnimator.SetFloat(AnimatorInputYParameter, lerpedInput.y);
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, inputState.IsDown(BUTTON.FORWARD));
			flagsUpdateScope.Set(Flags.Reserved2, inputState.IsDown(BUTTON.BACKWARD));
			flagsUpdateScope.Set(Flags.Reserved3, inputState.IsDown(BUTTON.LEFT));
			flagsUpdateScope.Set(Flags.Reserved4, inputState.IsDown(BUTTON.RIGHT));
		}
	}

	public override void VehicleFixedUpdate()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Parachute.VehicleFixedUpdate"))
		{
			base.VehicleFixedUpdate();
			TriggerParachuteForceVolume triggerParachuteForceVolume = FindTrigger<TriggerParachuteForceVolume>();
			float num = base.healthFraction * DamageTester;
			float num2 = DragCurve.Evaluate(TimeSince.op_Implicit(mountTime));
			float num3 = DragDamageCurve.Evaluate(num);
			float mass = MassDamageCurve.Evaluate(num);
			rigidBody.mass = mass;
			rigidBody.linearDamping = Mathf.Lerp(0f, TargetDrag * num3, num2);
			rigidBody.angularDamping = Mathf.Lerp(0f, TargetAngularDrag * num3, num2);
			float num4 = Mathf.Clamp01(TimeSince.op_Implicit(mountTime) / 1f);
			Vector3 forward = ((Component)this).transform.forward;
			Vector3 val = (forward * ConstantForwardForce + forward * (ForwardTiltAcceleration * Mathf.Clamp(lerpedInput.y, 0f, 1f))) * Time.fixedDeltaTime * num4;
			if ((Object)(object)triggerParachuteForceVolume != (Object)null)
			{
				val *= triggerParachuteForceVolume.GetSpeedMultiplierForParachute(this);
			}
			if (lerpedInput.y < -0.1f)
			{
				val *= 1f - BackInputForceMultiplier * Mathf.Abs(lerpedInput.y);
			}
			val *= num;
			rigidBody.AddForce(val, ForwardForceMode);
			Quaternion rotation;
			if (lerpedInput.x != 0f)
			{
				rotation = rigidBody.rotation;
				Quaternion val2 = Quaternion.Euler(Vector3Ex.WithZ(((Quaternion)(ref rotation)).eulerAngles, Mathx.RemapValClamped(lerpedInput.x, -1f, 1f, 40f, -40f)));
				rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, val2, Time.fixedDeltaTime * 30f));
				rigidBody.AddTorque(((Component)this).transform.TransformDirection(Vector3.up * (TurnForce * num * 0.2f * lerpedInput.x)), TurnForceMode);
			}
			if (lerpedInput.y > 0f)
			{
				rotation = rigidBody.rotation;
				Quaternion val3 = Quaternion.Euler(Vector3Ex.WithX(((Quaternion)(ref rotation)).eulerAngles, Mathx.RemapValClamped(lerpedInput.y, -1f, 1f, -50f, 60f)));
				rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, val3, Time.fixedDeltaTime * 60f));
			}
			rotation = rigidBody.rotation;
			Quaternion val4 = Quaternion.Euler(Vector3Ex.WithZ(Vector3Ex.WithX(((Quaternion)(ref rotation)).eulerAngles, 0f), 0f));
			rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, val4, Time.fixedDeltaTime * UprightLerpForce);
			float num5 = DamageHorizontalVelocityCurve.Evaluate(num);
			Vector3 linearVelocity = rigidBody.linearVelocity;
			linearVelocity.x = Mathf.Clamp(linearVelocity.x, 0f - num5, num5);
			linearVelocity.z = Mathf.Clamp(linearVelocity.z, 0f - num5, num5);
			rigidBody.linearVelocity = linearVelocity;
		}
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerDismounted(player, seat);
		if (collisionDeath)
		{
			if (TimeSince.op_Implicit(mountTime) < HurtDeployTime)
			{
				float num = 1f - Mathf.Clamp01(TimeSince.op_Implicit(mountTime) / HurtDeployTime);
				player.Hurt(HurtAmount * num, DamageType.Fall);
			}
			else
			{
				float magnitude = ((Vector3)(ref collisionImpulse)).magnitude;
				if (magnitude > 50f)
				{
					float amount = Mathx.RemapValClamped(magnitude, 50f, 400f, 5f, 50f);
					player.Hurt(amount, DamageType.Fall);
				}
			}
		}
		if (BypassRepack)
		{
			Item item = ItemManager.Create(PackedParachute, 1, skinID, isServerSide: true, 0uL);
			item.RepairCondition(item.maxCondition);
			player.inventory.containerWear.GiveItem(item);
		}
		Analytics.Azure.OnParachuteUsed(player, distanceTravelled, startHeight, TimeSince.op_Implicit(mountTime));
		if (collisionDeath && LandingAnimations)
		{
			Effect.server.Run(ParachuteLandScreenBounce.resourcePath, player, 0u, Vector3.zero, Vector3.zero);
			if (collisionLocalPos.y < 0.15f)
			{
				player.Server_StartGesture(GestureCollection.HeavyLandingId);
				player.PlayHeavyLandingAnimation = false;
			}
		}
		ProcessDeath();
		collisionDeath = false;
	}

	public void ProcessDeath()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		float num = base.healthFraction;
		num -= ConditionLossPerUse;
		bool num2 = num > 0f;
		if (num2 && !BypassRepack)
		{
			ParachuteUnpacked parachuteUnpacked = GameManager.server.CreateEntity(DetachedParachute.resourcePath, DetachedSpawnPoint.position, DetachedSpawnPoint.rotation) as ParachuteUnpacked;
			if ((Object)(object)parachuteUnpacked != (Object)null)
			{
				parachuteUnpacked.skinID = skinID;
				parachuteUnpacked.Spawn();
				parachuteUnpacked.Hurt(parachuteUnpacked.MaxHealth() * (1f - num), DamageType.Generic, null, useProtection: false);
				Rigidbody val = default(Rigidbody);
				if (((Component)parachuteUnpacked).TryGetComponent<Rigidbody>(ref val))
				{
					val.linearVelocity = rigidBody.linearVelocity;
				}
			}
		}
		DestroyMode mode = DestroyMode.None;
		if (!num2)
		{
			mode = DestroyMode.Gib;
		}
		Kill(mode);
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hitEntity == (Object)null)
		{
			hitEntity = GameObjectEx.ToBaseEntity(collision.collider);
		}
		if (!((Object)(object)hitEntity == (Object)(object)this) && (!((Object)(object)hitEntity != (Object)null) || hitEntity.isServer == base.isServer) && base.isServer && !(hitEntity is TimedExplosive) && !collisionDeath)
		{
			collisionImpulse = collision.impulse;
			Transform transform = ((Component)this).transform;
			ContactPoint contact = collision.GetContact(0);
			collisionLocalPos = transform.InverseTransformPoint(((ContactPoint)(ref contact)).point);
			contact = collision.GetContact(0);
			collisionWorldNormal = ((ContactPoint)(ref contact)).normal;
			collisionDeath = true;
			Invoke(DelayedDismount, 0f);
		}
	}

	public void DelayedDismount()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (collisionDeath && distanceTravelled > 0f && (!((Object)(object)mountPoints[0].mountable != (Object)null) || !GetDismountPosition(mountPoints[0].mountable.GetMounted(), out var _)))
		{
			Transform transform = ((Component)this).transform;
			transform.position += collisionWorldNormal * 0.35f;
		}
		DismountAllPlayers();
	}

	public override float AntiHackVelocity()
	{
		return 13.5f;
	}

	public override bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(mountTime) < 1.5f)
		{
			return false;
		}
		return base.AllowPlayerInstigatedDismount(player);
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(mountTime) > 1f)
		{
			return !InSafeZone();
		}
		return false;
	}

	private Vector2 ProcessInputVector(InputState inputState, BasePlayer player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player.GetHeldEntity() != (Object)null)
		{
			return Vector2.zero;
		}
		bool leftDown = inputState.IsDown(BUTTON.LEFT);
		bool rightDown = inputState.IsDown(BUTTON.RIGHT);
		bool forwardDown = inputState.IsDown(BUTTON.FORWARD);
		bool backDown = inputState.IsDown(BUTTON.BACKWARD);
		return ProcessInputVector(leftDown, rightDown, forwardDown, backDown);
	}

	private Vector2 ProcessInputVectorFromFlags(BasePlayer player)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player.GetHeldEntity() != (Object)null)
		{
			return Vector2.zero;
		}
		bool leftDown = HasFlag(Flags.Reserved3);
		bool rightDown = HasFlag(Flags.Reserved4);
		bool forwardDown = HasFlag(Flags.Reserved1);
		bool backDown = HasFlag(Flags.Reserved2);
		return ProcessInputVector(leftDown, rightDown, forwardDown, backDown);
	}

	private static Vector2 ProcessInputVector(bool leftDown, bool rightDown, bool forwardDown, bool backDown)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		if (leftDown & rightDown)
		{
			leftDown = (rightDown = false);
		}
		if (forwardDown & backDown)
		{
			forwardDown = (backDown = false);
		}
		if (forwardDown)
		{
			zero.y = 1f;
		}
		else if (backDown)
		{
			zero.y = -1f;
		}
		if (rightDown)
		{
			zero.x = 1f;
		}
		else if (leftDown)
		{
			zero.x = -1f;
		}
		return zero;
	}

	public Parachute()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		ConditionLossPerUse = 0.2f;
		HurtDeployTime = 1f;
		HurtAmount = 80f;
		UprightLerpForce = 5f;
		ConstantForwardForce = 2f;
		ForwardForceMode = (ForceMode)5;
		TurnForce = 2f;
		TurnForceMode = (ForceMode)5;
		ForwardTiltAcceleration = 2f;
		BackInputForceMultiplier = 0.2f;
		DeployAnimationLength = 3f;
		TargetDrag = 1f;
		TargetAngularDrag = 1f;
		DragCurve = new AnimationCurve();
		DragDamageCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		MassDamageCurve = AnimationCurve.Linear(0f, 30f, 1f, 1f);
		DamageHorizontalVelocityCurve = AnimationCurve.Linear(0f, 5f, 1f, 20f);
		DamageTester = 1f;
		AnimationInputSmoothness = 1f;
		AnimationInputScale = new Vector2(0.5f, 0.5f);
		collisionImpulse = Vector3.zero;
		lastPosition = Vector3.zero;
		lerpedInput = Vector2.zero;
		base._002Ector();
	}
}
