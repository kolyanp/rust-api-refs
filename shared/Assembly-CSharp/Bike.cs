using System;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Bike : GroundVehicle, CarPhysics<Bike>.ICar, TriggerHurtNotChild.IHurtTriggerUser, VehicleChassisVisuals<Bike>.IClientWheelUser, IPrefabPreProcess
{
	public enum PoweredBy
	{
		Fuel,
		Human
	}

	private float _steer;

	public CarPhysics<Bike> carPhysics;

	private VehicleTerrainHandler serverTerrainHandler;

	private CarWheel[] wheels;

	public TimeSince timeSinceLastUsed;

	private const float DECAY_TICK_TIME = 60f;

	private float prevPitchStabError;

	private float prevRollStabError;

	private float prevRollStabRoll;

	public float lastCrashDamage;

	private TimeSince timeSinceBellDing;

	private bool wasWantingSlopeSprint;

	private bool inBurnoutMode;

	private TimeSince timeSinceLastBunnyhop;

	private TransformHandle sidecarPhysicsHingeHandle;

	private bool shouldBypassClippingChecks;

	public static Phrase sprintPhrase;

	public static Phrase boostPhrase;

	[SerializeField]
	[Header("Bike")]
	private Transform centreOfMassTransform;

	[SerializeField]
	private VisualCarWheel wheelFront;

	[SerializeField]
	private VisualCarWheel wheelRear;

	[SerializeField]
	private VisualCarWheel wheelExtra;

	[SerializeField]
	public bool snowmobileDrivingStyle;

	[SerializeField]
	public CarSettings carSettings;

	[SerializeField]
	public int engineKW;

	[SerializeField]
	public float idleFuelPerSec;

	[SerializeField]
	public float maxFuelPerSec;

	[Range(0f, 1f)]
	[SerializeField]
	private float pitchStabP;

	[Range(0f, 1f)]
	[SerializeField]
	private float pitchStabD;

	[Range(0f, 1f)]
	[SerializeField]
	private float twoWheelRollStabP;

	[Range(0f, 1f)]
	[SerializeField]
	private float twoWheelRollStabD;

	[SerializeField]
	[Range(1f, 500f)]
	private float manyWheelStabP;

	[SerializeField]
	[Range(1f, 100f)]
	private float manyWheelStabD;

	[Range(0f, 1f)]
	[SerializeField]
	public float airControlTorquePower;

	public float sprintTime;

	[SerializeField]
	public float sprintRegenTime;

	[SerializeField]
	public float sprintBoostPercent;

	[SerializeField]
	private ProtectionProperties riderProtection;

	[SerializeField]
	private float hurtTriggerMinSpeed;

	[SerializeField]
	private TriggerHurtNotChild hurtTriggerFront;

	[SerializeField]
	private TriggerHurtNotChild hurtTriggerRear;

	[SerializeField]
	private float maxLeanSpeed;

	[SerializeField]
	private float leftMaxLean;

	[SerializeField]
	private float rightMaxLean;

	[SerializeField]
	private float midairRotationForce;

	[SerializeField]
	private Vector3 customInertiaTensor;

	public PoweredBy poweredBy;

	[SerializeField]
	[Range(0f, 1f)]
	public float percentFood;

	[SerializeField]
	public float playerDamageThreshold;

	[SerializeField]
	public float playerDeathThreshold;

	[SerializeField]
	private bool hasBell;

	[SerializeField]
	private bool hasBunnyhop;

	[Header("Bike Visuals")]
	public float minGroundFXSpeed;

	[SerializeField]
	private BikeChassisVisuals chassisVisuals;

	[SerializeField]
	private VehicleLight[] lights;

	[SerializeField]
	private ParticleSystemContainer exhaustFX;

	[SerializeField]
	private Transform steeringLeftIK;

	[SerializeField]
	private Transform steeringRightIK;

	[SerializeField]
	private Transform steeringRightIKAcclerating;

	[SerializeField]
	private Transform leftFootIK;

	[SerializeField]
	private Transform rightFootIK;

	[SerializeField]
	private Transform passengerLeftHandIK;

	[SerializeField]
	private Transform passengerRightHandIK;

	[SerializeField]
	private Transform passengerLeftFootIK;

	[SerializeField]
	private Transform passengerRightFootIK;

	[SerializeField]
	private ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	private GameObject fxMediumDamageInstLight;

	[SerializeField]
	private ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	private GameObject fxHeavyDamageInstLight;

	[Header("Sidecar")]
	[SerializeField]
	private Rigidbody sidecarRigidBody;

	[SerializeField]
	private Transform sidecarPhysicsHinge;

	[ServerVar(Help = "How long before a bike loses all its health while outside")]
	public static float outsideDecayMinutes;

	[ServerVar(Help = "Pedal bike population active on the server (roadside spawns)", ShowInAdminUI = true)]
	public static float pedalRoadsidePopulation;

	[SerializeField]
	private Transform realSidecarCapsule;

	[ServerVar(Help = "Pedal bike population in monuments", ShowInAdminUI = true)]
	public static float pedalMonumentPopulation;

	[SerializeField]
	private Transform duplicateSidecarCapsule;

	[ServerVar(Help = "Motorbike population in monuments", ShowInAdminUI = true)]
	public static float motorbikeMonumentPopulation;

	[ServerVar(Help = "Can bike crashes cause damage or death to the rider?")]
	public static bool doPlayerDamage;

	[ServerVar(Help = "Amount of collision damage on a bike required to ragdoll the player")]
	public static float playerDamageRagdollTheshold;

	private bool hasExtraWheel;

	public bool hasSidecar;

	private bool hasDamageFX;

	private float _throttle;

	private float _brake;

	public const Flags Flag_SprintInput = Flags.Reserved6;

	public const Flags Flag_DuckInput = Flags.Reserved8;

	public const Flags Flag_IsSprinting = Flags.Reserved9;

	public const Flags Flag_IsBunnyhopping = Flags.Reserved10;

	private float _mass;

	private float cachedFuelFraction;

	private const float FORCE_MULTIPLIER = 10f;

	public float SteerInput
	{
		get
		{
			return _steer;
		}
		protected set
		{
			_steer = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public VehicleTerrainHandler.Surface OnSurface
	{
		get
		{
			if (serverTerrainHandler == null)
			{
				return VehicleTerrainHandler.Surface.Default;
			}
			return serverTerrainHandler.OnSurface;
		}
	}

	public bool SprintInput
	{
		get
		{
			return HasFlag(Flags.Reserved6);
		}
		private set
		{
			if (SprintInput != value)
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved6, value);
				}
			}
		}
	}

	public bool DuckInput
	{
		get
		{
			return HasFlag(Flags.Reserved8);
		}
		private set
		{
			if (DuckInput != value)
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved8, value);
				}
			}
		}
	}

	public float ThrottleInput
	{
		get
		{
			if (!engineController.IsOn)
			{
				return 0f;
			}
			return _throttle;
		}
		protected set
		{
			_throttle = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public float BrakeInput
	{
		get
		{
			return _brake;
		}
		protected set
		{
			_brake = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool IsBraking => BrakeInput > 0f;

	public bool CanSprint => poweredBy == PoweredBy.Human;

	public bool IsSprinting
	{
		get
		{
			return HasFlag(Flags.Reserved9);
		}
		private set
		{
			if (IsSprinting != value)
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved9, value);
				}
			}
		}
	}

	public float SprintPercentRemaining { get; protected set; }

	public bool IsBunnyhopping
	{
		get
		{
			return HasFlag(Flags.Reserved10);
		}
		private set
		{
			if (IsBunnyhopping != value)
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved10, value);
				}
			}
		}
	}

	public float SteerAngle
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.SteerAngle;
			}
			return 0f;
		}
	}

	public override float DriveWheelVelocity
	{
		get
		{
			if (base.isServer)
			{
				float num = carPhysics.DriveWheelVelocity;
				if (inBurnoutMode && ThrottleInput > 0.1f)
				{
					num += ThrottleInput * 20f;
				}
				return num;
			}
			return 0f;
		}
	}

	public float DriveWheelSlip
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.DriveWheelSlip;
			}
			return 0f;
		}
	}

	public float SidecarAngle
	{
		get
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (base.isServer)
			{
				if (BaseNetworkable.UseParallelSaves)
				{
					Quaternion localRotMT = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in sidecarPhysicsHingeHandle);
					return ((Quaternion)(ref localRotMT)).eulerAngles.z;
				}
				return sidecarPhysicsHinge.localEulerAngles.z;
			}
			return 0f;
		}
	}

	public float MaxSteerAngle => carSettings.maxSteerAngle;

	private float Mass
	{
		get
		{
			if (base.isServer)
			{
				return rigidBody.mass;
			}
			return _mass;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Bike.OnRpcMessage"))
		{
			if (rpc == 1851540757 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenFuel"));
				}
				using (TimeWarning.New("RPC_OpenFuel"))
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
							RPC_OpenFuel(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_OpenFuel");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public float GetSteerInput()
	{
		return SteerInput;
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		timeSinceLastUsed = TimeSince.op_Implicit(0f);
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		rigidBody.inertiaTensor = customInertiaTensor;
		carPhysics = new CarPhysics<Bike>(this, ((Component)this).transform, rigidBody, carSettings);
		serverTerrainHandler = new VehicleTerrainHandler(this);
		SprintPercentRemaining = 1f;
		InvokeRandomized(UpdateClients, 0f, 0.1f, 0.01f);
		InvokeRandomized(BikeDecay, Random.Range(30f, 60f), 60f, 6f);
		if ((Object)(object)sidecarPhysicsHinge != (Object)null)
		{
			sidecarPhysicsHingeHandle = ((Component)sidecarPhysicsHinge).transformHandle;
		}
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (base.isServer)
		{
			ProcessCollision(collision, sidecarRigidBody);
		}
	}

	public override void VehicleFixedUpdate()
	{
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Bike.VehicleFixedUpdate"))
		{
			base.VehicleFixedUpdate();
			float speed = GetSpeed();
			carPhysics.FixedUpdate(Time.fixedDeltaTime, speed);
			serverTerrainHandler.FixedUpdate();
			bool flag = false;
			if (IsOn())
			{
				inBurnoutMode = false;
				float fuelPerSecond = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, Mathf.Abs(ThrottleInput));
				engineController.TickFuel(fuelPerSecond);
				if (CanSprint && carPhysics.IsGrounded() && WantsSprint(speed))
				{
					SprintPercentRemaining -= Time.deltaTime / sprintTime;
					SprintPercentRemaining = Mathf.Clamp01(SprintPercentRemaining);
					flag = SprintPercentRemaining > 0f;
				}
				bool flag2 = DuckInput || (ThrottleInput > 0f && BrakeInput > 0f);
				if ((poweredBy == PoweredBy.Fuel && carPhysics.IsGrounded()) & flag2)
				{
					inBurnoutMode = true;
				}
			}
			engineController.CheckEngineState();
			if (CanSprint && !flag && SprintPercentRemaining < 1f)
			{
				SprintPercentRemaining += Time.deltaTime / sprintRegenTime;
				SprintPercentRemaining = Mathf.Clamp01(SprintPercentRemaining);
			}
			IsSprinting = flag;
			bool num = rigidBody.IsSleeping();
			if (!num)
			{
				AwakeBikePhysicsTick(speed);
			}
			RigidbodyConstraints val = (RigidbodyConstraints)(num ? 64 : 0);
			if (rigidBody.constraints != val)
			{
				rigidBody.constraints = val;
				if ((int)rigidBody.constraints == 0)
				{
					rigidBody.inertiaTensor = customInertiaTensor;
				}
			}
			((Component)hurtTriggerFront).gameObject.SetActive(speed > hurtTriggerMinSpeed);
			((Component)hurtTriggerRear).gameObject.SetActive(speed < 0f - hurtTriggerMinSpeed);
			if (hasSidecar)
			{
				if (rigidBody.isKinematic != sidecarRigidBody.isKinematic)
				{
					sidecarRigidBody.isKinematic = rigidBody.isKinematic;
				}
				if (rigidBody.IsSleeping() != sidecarRigidBody.IsSleeping())
				{
					if (rigidBody.IsSleeping())
					{
						sidecarRigidBody.Sleep();
					}
					else
					{
						sidecarRigidBody.WakeUp();
					}
				}
			}
			if (carPhysics.IsGrounded() && TimeSince.op_Implicit(timeSinceLastBunnyhop) >= 0.5f)
			{
				IsBunnyhopping = false;
			}
		}
	}

	protected virtual void AwakeBikePhysicsTick(float speed)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (rigidBody.isKinematic)
		{
			return;
		}
		bool num = carPhysics.IsGrounded();
		if (snowmobileDrivingStyle)
		{
			if (!carPhysics.IsGrounded())
			{
				StabiliseSnowmobileStyle();
				PDPitchStab();
			}
		}
		else
		{
			PDPitchStab();
			PDDirectionStab();
			PDRollStab(speed);
		}
		float num2 = 0f;
		if (!num)
		{
			if (SprintInput && !DuckInput)
			{
				num2 = 0f - airControlTorquePower;
			}
			else if (DuckInput && !SprintInput)
			{
				num2 = airControlTorquePower;
			}
		}
		if (num2 != 0f)
		{
			rigidBody.AddRelativeTorque(num2, 0f, 0f, (ForceMode)2);
		}
		if (hasSidecar)
		{
			duplicateSidecarCapsule.SetPositionAndRotation(realSidecarCapsule.position, realSidecarCapsule.rotation);
		}
	}

	private void PDPitchStab()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Component)this).transform.localEulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = 0f - num;
		float num3 = num2;
		float num4 = (num2 - prevPitchStabError) / Time.fixedDeltaTime;
		float num5 = pitchStabP * num3 + pitchStabD * num4;
		rigidBody.AddRelativeTorque(num5, 0f, 0f, (ForceMode)2);
		prevPitchStabError = num2;
	}

	private void PDDirectionStab()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector3 angularVelocity = rigidBody.angularVelocity;
		float num = (carPhysics.IsGrounded() ? (0.05f + Mathf.Abs(SteerAngle) * 0.15f) : 0.05f);
		angularVelocity.y = Mathf.Clamp(angularVelocity.y, 0f - num, num);
		rigidBody.angularVelocity = angularVelocity;
	}

	private void PDRollStab(float speed)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		float num = ((speed >= 0f) ? speed : ((0f - speed) * 0.33f));
		float num2 = 0f - SteerAngle / MaxSteerAngle * Mathf.Clamp01(num / maxLeanSpeed);
		num2 = ((!(num2 < 0f)) ? (num2 * leftMaxLean) : (num2 * rightMaxLean));
		float num3 = ((Component)this).transform.localEulerAngles.z;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		float num4 = num2 - num3;
		float num5 = num4;
		float num6 = 0f - AngleDifference(num3, prevRollStabRoll) / Time.fixedDeltaTime;
		float num7 = twoWheelRollStabP * num5 + twoWheelRollStabD * num6;
		rigidBody.AddRelativeTorque(0f, 0f, num7, (ForceMode)2);
		prevRollStabError = num4;
		prevRollStabRoll = num3;
	}

	private float AngleDifference(float a, float b)
	{
		return (a - b + 540f) % 360f - 180f;
	}

	private void StabiliseSnowmobileStyle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 10f, 1218511105, (QueryTriggerInteraction)1))
		{
			Vector3 normal = ((RaycastHit)(ref val)).normal;
			Vector3 right = ((Component)this).transform.right;
			right.y = 0f;
			normal = Vector3.ProjectOnPlane(normal, right);
			float num = Vector3.Angle(normal, Vector3.up);
			Vector3 angularVelocity = rigidBody.angularVelocity;
			float num2 = ((Vector3)(ref angularVelocity)).magnitude * 57.29578f * manyWheelStabD / manyWheelStabP;
			if (num <= 45f)
			{
				Vector3 val2 = Vector3.Cross(Quaternion.AngleAxis(num2, rigidBody.angularVelocity) * ((Component)this).transform.up, normal) * manyWheelStabP * manyWheelStabP;
				Vector3 val3 = ((Component)rigidBody).transform.InverseTransformDirection(val2);
				rigidBody.AddRelativeTorque(val3);
			}
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDriver(player))
		{
			return;
		}
		timeSinceLastUsed = TimeSince.op_Implicit(0f);
		if (inputState.IsDown(BUTTON.FIRE_THIRD))
		{
			SteerInput += inputState.MouseDelta().x * 0.1f;
		}
		else
		{
			SteerInput = 0f;
			if (inputState.IsDown(BUTTON.LEFT))
			{
				SteerInput = -1f;
			}
			else if (inputState.IsDown(BUTTON.RIGHT))
			{
				SteerInput = 1f;
			}
		}
		bool flag = inputState.IsDown(BUTTON.FORWARD);
		bool flag2 = inputState.IsDown(BUTTON.BACKWARD);
		BrakeInput = 0f;
		if (GetSpeed() > 3f)
		{
			ThrottleInput = (flag ? 1f : 0f);
			BrakeInput = (flag2 ? 1f : 0f);
		}
		else
		{
			ThrottleInput = (flag ? 1f : (flag2 ? (-1f) : 0f));
		}
		SprintInput = inputState.IsDown(BUTTON.SPRINT);
		DuckInput = inputState.IsDown(BUTTON.DUCK);
		if (engineController.IsOff && (inputState.WasJustPressed(BUTTON.FORWARD) || inputState.WasJustPressed(BUTTON.BACKWARD)))
		{
			engineController.TryStartEngine(player);
		}
		if (hasBell && inputState.WasJustPressed(BUTTON.FIRE_PRIMARY) && TimeSince.op_Implicit(timeSinceBellDing) > 1f)
		{
			ClientRPC(RpcTarget.NetworkGroup("RingBell"));
			timeSinceBellDing = TimeSince.op_Implicit(0f);
		}
		if (hasBunnyhop && inputState.WasJustPressed(BUTTON.FIRE_SECONDARY) && CanBunnyHop())
		{
			DoBunnyHop();
		}
	}

	private bool CanBunnyHop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(timeSinceLastBunnyhop) > 3f && carPhysics.IsGrounded() && !engineController.IsWaterlogged())
		{
			return (double)SprintPercentRemaining >= 0.1;
		}
		return false;
	}

	private void DoBunnyHop()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		IsBunnyhopping = true;
		float num = Mathf.Min(Mathf.Abs(GetSpeed() * 0.3f) * SprintPercentRemaining * GetPerformanceFraction(), 10f);
		Vector3 val = Vector3.up * (2.5f + num);
		rigidBody.AddRelativeForce(val, (ForceMode)2);
		SprintPercentRemaining -= 0.1f;
		SprintPercentRemaining = Mathf.Clamp01(SprintPercentRemaining);
		timeSinceLastBunnyhop = TimeSince.op_Implicit(0f);
	}

	public float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float maxDriveForce = GetMaxDriveForce();
		float num = Mathf.Lerp(0.3f, 0.75f, GetPerformanceFraction());
		float num2 = MathEx.BiasedLerp(1f - absSpeed / topSpeed, num);
		return maxDriveForce * num2;
	}

	public bool GetSteerSpeedMod(float speed)
	{
		return inBurnoutMode;
	}

	public virtual float GetSteerMaxMult(float speed)
	{
		if (speed < 0f)
		{
			return 0.5f;
		}
		if (!inBurnoutMode)
		{
			return 1f;
		}
		return 1.35f;
	}

	public override float AntiHackVelocity()
	{
		return Mathf.Max(GetMaxForwardSpeed() * 1.3f, 30f);
	}

	public CarWheel[] GetWheels()
	{
		if (wheels == null)
		{
			if (hasExtraWheel)
			{
				wheels = new CarWheel[3] { wheelFront, wheelRear, wheelExtra };
			}
			else
			{
				wheels = new CarWheel[2] { wheelFront, wheelRear };
			}
		}
		return wheels;
	}

	public float GetWheelsMidPos()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return (((Component)wheelFront.wheelCollider).transform.localPosition.z - ((Component)wheelRear.wheelCollider).transform.localPosition.z) * 0.5f;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.bike = Pool.Get<Bike>();
		info.msg.bike.steerInput = SteerAngle;
		info.msg.bike.driveWheelVel = DriveWheelVelocity;
		info.msg.bike.throttleInput = ThrottleInput;
		info.msg.bike.brakeInput = BrakeInput;
		info.msg.bike.fuelStorageID = GetFuelSystem().GetInstanceID();
		info.msg.bike.fuelFraction = GetFuelFraction();
		if (hasSidecar)
		{
			info.msg.bike.sidecarAngle = SidecarAngle;
			info.msg.bike.time = GetNetworkTime(in info.cachedTime);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		shouldBypassClippingChecks = false;
		if ((Object)(object)newParent != (Object)null && HasDriver() && (Object)(object)((Component)newParent).GetComponentInChildren<TriggerParentEnclosed>() != (Object)null)
		{
			shouldBypassClippingChecks = true;
		}
	}

	public override void SeatClippedWorld(BaseMountable mountable)
	{
		if (!shouldBypassClippingChecks)
		{
			base.SeatClippedWorld(mountable);
		}
	}

	public override void DoCollisionDamage(BaseEntity hitEntity, float damage)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		lastCrashDamage = damage;
		if (doPlayerDamage && damage > playerDamageThreshold)
		{
			float num = ((damage > playerDeathThreshold) ? 9999f : ((damage - playerDamageThreshold) / 2f));
			float num2 = ((damage > playerDeathThreshold) ? 9999f : (num * 0.5f));
			foreach (MountPointInfo mountPoint in mountPoints)
			{
				if (!((Object)(object)mountPoint.mountable != (Object)null))
				{
					continue;
				}
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((Object)(object)mounted != (Object)null)
				{
					float num3 = (mountPoint.isDriver ? num : num2);
					mounted.Hurt(num3, DamageType.Collision, this, useProtection: false);
					if (num3 > playerDamageRagdollTheshold && !mounted.IsDead())
					{
						Vector3 mountRagdollVelocity = GetMountRagdollVelocity(mounted);
						mounted.Ragdoll(mountRagdollVelocity);
					}
				}
			}
		}
		base.DoCollisionDamage(hitEntity, damage);
	}

	public override Vector3 GetMountRagdollVelocity(BasePlayer player)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(lastCrashDamage, 0f, 75f);
		return ((Component)this).transform.forward * num * 0.25f;
	}

	public override int StartingFuelUnits()
	{
		return 0;
	}

	public override bool MeetsEngineRequirements()
	{
		return HasDriver();
	}

	public void BikeDecay()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDead() && !(TimeSince.op_Implicit(timeSinceLastUsed) < 2700f))
		{
			float num = (IsOutside() ? outsideDecayMinutes : float.PositiveInfinity);
			if (!float.IsPositiveInfinity(num))
			{
				float num2 = 1f / num;
				Hurt(MaxHealth() * num2, DamageType.Decay, this, useProtection: false);
			}
		}
	}

	public override float GetModifiedDrag()
	{
		float num = base.GetModifiedDrag();
		if (!IsOn() && !HasDriver())
		{
			num = Mathf.Max(num, 0.5f);
		}
		return num;
	}

	private void UpdateClients()
	{
		if (HasDriver())
		{
			byte num = (byte)((ThrottleInput + 1f) * 7f);
			byte b = (byte)(BrakeInput * 15f);
			byte throttleAndBrake = (byte)(num + (b << 4));
			SendClientRPC(throttleAndBrake);
		}
	}

	public virtual void SendClientRPC(byte throttleAndBrake)
	{
		if (hasSidecar)
		{
			ClientRPC(RpcTarget.NetworkGroup("BikeUpdateSC"), GetNetworkTime(), SteerAngle, throttleAndBrake, DriveWheelVelocity, GetFuelFraction(), SidecarAngle);
		}
		else if (CanSprint)
		{
			ClientRPC(RpcTarget.NetworkGroup("BikeUpdateSP"), GetNetworkTime(), SteerAngle, throttleAndBrake, DriveWheelVelocity, GetFuelFraction(), SprintPercentRemaining);
		}
		else
		{
			ClientRPC(RpcTarget.NetworkGroup("BikeUpdate"), GetNetworkTime(), SteerAngle, throttleAndBrake, DriveWheelVelocity, GetFuelFraction());
		}
	}

	public override void OnEngineStartFailed()
	{
		ClientRPC(RpcTarget.NetworkGroup("EngineStartFailed"));
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		if (info.UseProtection)
		{
			riderProtection.Scale(info.damageTypes);
		}
	}

	private bool WantsSprint(float speed)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (SprintInput)
		{
			return true;
		}
		if (speed > 5f || ThrottleInput <= 0.5f || BrakeInput > 0f)
		{
			return false;
		}
		float num = ((Component)this).transform.localEulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		return wasWantingSlopeSprint = (wasWantingSlopeSprint ? (num <= -18f) : (num <= -23f));
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (CanPlayerSeeMountPoint(player))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player))
		{
			GetFuelSystem().LootFuel(player);
		}
	}

	public override void PreInitShared()
	{
		hasExtraWheel = (Object)(object)wheelExtra.wheelCollider != (Object)null;
		hasSidecar = (Object)(object)sidecarPhysicsHinge != (Object)null;
		hasDamageFX = (Object)(object)fxMediumDamage != (Object)null;
		base.PreInitShared();
	}

	public override void Load(LoadInfo info)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.bike != null)
		{
			engineController.FuelSystem.SetInstanceID(info.msg.bike.fuelStorageID);
			cachedFuelFraction = info.msg.bike.fuelFraction;
		}
	}

	public float GetMaxDriveForce()
	{
		float num = (float)engineKW * 10f * GetPerformanceFraction();
		if (IsSprinting)
		{
			num *= 1f + sprintBoostPercent;
		}
		return num;
	}

	public override float GetMaxForwardSpeed()
	{
		float num = GetMaxDriveForce() / Mass * 15f;
		if (IsSprinting)
		{
			num *= 1f + sprintBoostPercent;
		}
		return num;
	}

	public override float GetThrottleInput()
	{
		return ThrottleInput;
	}

	public override float GetBrakeInput()
	{
		return BrakeInput;
	}

	public float GetPerformanceFraction()
	{
		float num = Mathf.InverseLerp(0.25f, 0.5f, base.healthFraction);
		return Mathf.Lerp(0.5f, 1f, num);
	}

	public float GetFuelFraction()
	{
		if (base.isServer)
		{
			return Mathf.Clamp01((float)engineController.FuelSystem.GetFuelAmount() / 100f);
		}
		return cachedFuelFraction;
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (AnyMounted())
		{
			if (PlayerIsMounted(player))
			{
				return player.modelState.poseType == 26;
			}
			return false;
		}
		return true;
	}

	protected override IFuelSystem CreateFuelSystem()
	{
		if (poweredBy == PoweredBy.Fuel)
		{
			return base.CreateFuelSystem();
		}
		return new HumanFuelSystem(base.isServer, this, percentFood);
	}

	private bool CanPlayerSeeMountPoint(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.CheckCapsule(player.eyes.position, mountAnchor.position, 0.25f, 2162688, (QueryTriggerInteraction)0))
		{
			return !GamePhysics.CheckCapsule(player.eyes.position, mountAnchor.position + Vector3.up * 0.5f, 0.25f, 2162688, (QueryTriggerInteraction)0);
		}
		return false;
	}

	public Bike()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		engineKW = 59;
		idleFuelPerSec = 0.03f;
		maxFuelPerSec = 0.15f;
		pitchStabP = 0.01f;
		pitchStabD = 0.005f;
		twoWheelRollStabP = 100f;
		twoWheelRollStabD = 10f;
		manyWheelStabP = 40f;
		manyWheelStabD = 10f;
		airControlTorquePower = 0.04f;
		sprintTime = 5f;
		sprintRegenTime = 10f;
		sprintBoostPercent = 0.3f;
		hurtTriggerMinSpeed = 1f;
		maxLeanSpeed = 20f;
		leftMaxLean = 60f;
		rightMaxLean = 60f;
		midairRotationForce = 1f;
		customInertiaTensor = new Vector3(85f, 60f, 40f);
		percentFood = 0.5f;
		playerDamageThreshold = 40f;
		playerDeathThreshold = 75f;
		_mass = -1f;
		base._002Ector();
	}

	static Bike()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		sprintPhrase = new Phrase("sprint", "Sprint");
		boostPhrase = new Phrase("boost", "Boost");
		outsideDecayMinutes = 1440f;
		pedalRoadsidePopulation = 1f;
		pedalMonumentPopulation = 1f;
		motorbikeMonumentPopulation = 1f;
		doPlayerDamage = true;
		playerDamageRagdollTheshold = 10f;
	}
}
