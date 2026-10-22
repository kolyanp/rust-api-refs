using System;
using System.Runtime.CompilerServices;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

public class PlayerModel : ListComponent<PlayerModel>
{
	[Serializable]
	public struct TurnSegment
	{
		public float Onset;

		public float Gain;

		public float Max;

		public float SmoothTime;

		public float Capacity(float absoluteOffset)
		{
			float num = absoluteOffset - Onset;
			if (num <= 0f || Max <= 0f)
			{
				return 0f;
			}
			return Max * (1f - Mathf.Exp((0f - num) * Gain / Max));
		}
	}

	public enum MountPoses
	{
		Chair = 0,
		Driving = 1,
		Horseback = 2,
		HeliUnarmed = 3,
		HeliArmed = 4,
		HandMotorBoat = 5,
		MotorBoatPassenger = 6,
		SitGeneric = 7,
		SitRaft = 8,
		StandDrive = 9,
		SitShootingGeneric = 10,
		SitMinicopter_Pilot = 11,
		SitMinicopter_Passenger = 12,
		ArcadeLeft = 13,
		ArcadeRight = 14,
		SitSummer_Ring = 15,
		SitSummer_BoogieBoard = 16,
		SitCarPassenger = 17,
		SitSummer_Chair = 18,
		SitRaft_NoPaddle = 19,
		Sit_SecretLab = 20,
		Sit_Workcart = 21,
		Sit_Cardgame = 22,
		Sit_Crane = 23,
		Sit_Snowmobile_Shooting = 24,
		Sit_RetroSnowmobile_Shooting = 25,
		Driving_Snowmobile = 26,
		ZiplineHold = 27,
		Sit_Locomotive = 28,
		Sit_Throne = 29,
		Parachute = 30,
		Sit_DPV = 31,
		Standing_Ballista = 32,
		Sit_BatteringRam = 33,
		VineSwinging = 34,
		Sit_Ejectorseat = 35,
		Sit_Beanbag = 36,
		Steering_Boat = 37,
		Run_WaterWheel = 38,
		Standing = 128
	}

	public Transform[] Shoulders;

	public Transform[] AdditionalSpineBones;

	[Header("Turn Cascade")]
	public float TurnAimSmoothTime = 0.03f;

	public TurnSegment TurnHead = new TurnSegment
	{
		Onset = 0f,
		Gain = 1.6f,
		Max = 30f,
		SmoothTime = 0.02f
	};

	public TurnSegment TurnChest = new TurnSegment
	{
		Onset = 14f,
		Gain = 1f,
		Max = 20f,
		SmoothTime = 0.05f
	};

	public TurnSegment TurnSpine = new TurnSegment
	{
		Onset = 28f,
		Gain = 0.9f,
		Max = 14f,
		SmoothTime = 0.08f
	};

	public TurnSegment TurnPelvis = new TurnSegment
	{
		Onset = 42f,
		Gain = 0.8f,
		Max = 11f,
		SmoothTime = 0.11f
	};

	public float TurnChestLowerShare = 0.5f;

	public float TurnSpineLowerShare = 0.45f;

	public float TurnHeadLockAngle = 100f;

	public float TurnHeadSmoothTime = 0.25f;

	public float TurnHeadMaxSpeed = 120f;

	public float TurnHeadAimFadeSpeed = 6f;

	[Header("Turn Commit")]
	public TurnClipSet TurnClips;

	public AnimationCurve TurnSpeedByBacklog = new AnimationCurve(new Keyframe[3]
	{
		new Keyframe(45f, 1f),
		new Keyframe(90f, 1.35f),
		new Keyframe(180f, 1.6f)
	});

	public float TurnMoveThreshold = 0.5f;

	public bool TurnSuppressProceduralLegs = true;

	public float TurnLegBlendTime = 0.12f;

	[Header("Turn Pelvis")]
	public bool TurnPelvisEnabled = true;

	public float TurnPelvisFootPinWeight = 1f;

	public float TurnArmFadeSpeed = 1.4f;

	public float TurnArmFadeWeight = 0.3f;

	protected static int speed = Animator.StringToHash("speed");

	protected static int acceleration = Animator.StringToHash("acceleration");

	protected static int rotationYaw = Animator.StringToHash("rotationYaw");

	protected static int turnAngle = Animator.StringToHash("turnAngle");

	protected static int turning = Animator.StringToHash("turning");

	protected static int forward = Animator.StringToHash("forward");

	protected static int right = Animator.StringToHash("right");

	protected static int up = Animator.StringToHash("up");

	protected static int ducked = Animator.StringToHash("ducked");

	protected static int grounded = Animator.StringToHash("grounded");

	protected static int crawling = Animator.StringToHash("crawling");

	protected static int waterlevel = Animator.StringToHash("waterlevel");

	protected static int relaxFloat = Animator.StringToHash("relaxFloat");

	protected static int attack = Animator.StringToHash("attack");

	protected static int attack_alt = Animator.StringToHash("attack_alt");

	protected static int deploy = Animator.StringToHash("deploy");

	protected static int reload = Animator.StringToHash("reload");

	protected static int throwWeapon = Animator.StringToHash("throw");

	protected static int holster = Animator.StringToHash("holster");

	protected static int aiming = Animator.StringToHash("aiming");

	protected static int onLadder = Animator.StringToHash("onLadder");

	protected static int posing = Animator.StringToHash("posing");

	protected static int poseType = Animator.StringToHash("poseType");

	protected static int relaxGunPose = Animator.StringToHash("relaxGunPose");

	protected static int vehicle_aim_yaw = Animator.StringToHash("vehicleAimYaw");

	protected static int vehicle_aim_speed = Animator.StringToHash("vehicleAimYawSpeed");

	protected static int usePoseTransition = Animator.StringToHash("usePoseTransition");

	protected static int leftFootIK = Animator.StringToHash("leftFootIK");

	protected static int rightFootIK = Animator.StringToHash("rightFootIK");

	protected static int vehicleSteering = Animator.StringToHash("vehicleSteering");

	protected static int playerBoatSteering = Animator.StringToHash("playerBoatSteering");

	protected static int ladderType = Animator.StringToHash("ladderType");

	protected static int hasParachute = Animator.StringToHash("hasParachute");

	protected static int nonGroundedTime = Animator.StringToHash("nonGroundedTime");

	protected static int deployParachuteTrigger = Animator.StringToHash("deployParachute");

	protected static int catching = Animator.StringToHash("waitingToCatch");

	protected static int sittingStateParam = Animator.StringToHash("sitting");

	private static readonly int[] AnimatorFloatParamsToCopy = new int[5] { forward, right, speed, vehicle_aim_yaw, poseType };

	private static readonly int[] AnimatorBoolParamsToCopy = new int[1] { posing };

	public BoxCollider collision;

	public GameObject censorshipCube;

	public GameObject censorshipCubeBreasts;

	public GameObject jawBone;

	public GameObject neckBone;

	public GameObject headBone;

	public GameObject leftEye;

	public GameObject rightEye;

	public EyeController eyeController;

	public EyeBlink blinkController;

	public Transform[] SpineBones;

	public Transform leftFootBone;

	public Transform rightFootBone;

	public Transform leftHandPropBone;

	public Transform rightHandPropBone;

	public Vector3 rightHandTarget;

	public Transform leftClavicleBone;

	public bool isPreview;

	[Header("IK")]
	public Vector3 leftHandTargetPosition;

	public Quaternion leftHandTargetRotation;

	public Vector3 rightHandTargetPosition;

	public Quaternion rightHandTargetRotation;

	public Transform leftUlnaTwistBone;

	public Transform rightUlnaTwistBone;

	[Range(0f, 1f)]
	public float leftHandTwistRelaxWeight = 1f;

	[Range(0f, 1f)]
	public float rightHandTwistRelaxWeight = 1f;

	public PlayerTwistBoneSetup[] TwistBones;

	public float steeringTargetDegrees;

	public float playerBoatSteeringTargetDegrees;

	public Vector3 rightFootTargetPosition;

	public Quaternion rightFootTargetRotation;

	public Vector3 leftFootTargetPosition;

	public Quaternion leftFootTargetRotation;

	public LegsAnimator legsAnimator;

	public AvatarMask upperBodyMask;

	public AvatarMask fullMask;

	public RuntimeAnimatorController CinematicAnimationController;

	public Avatar DefaultAvatar;

	public Avatar CinematicAvatar;

	public RuntimeAnimatorController DefaultHoldType;

	public RuntimeAnimatorController SleepGesture;

	public RuntimeAnimatorController CrawlToIncapacitatedGesture;

	public RuntimeAnimatorController CrawlToIncapacitatedGestureHandcuff;

	public RuntimeAnimatorController StandToIncapacitatedGesture;

	[NonSerialized]
	public RuntimeAnimatorController CurrentGesture;

	[Header("Skin")]
	public SkinSetCollection MaleSkin;

	public SkinSetCollection FemaleSkin;

	public SkinSetCollection MannequinSkin;

	public SubsurfaceProfile subsurfaceProfile;

	[Header("Parameters")]
	[Range(0f, 1f)]
	public float voiceVolume;

	[Range(0f, 1f)]
	public float skinColor = 1f;

	[Range(0f, 1f)]
	public float skinNumber = 1f;

	[Range(0f, 1f)]
	public float meshNumber;

	[Range(0f, 1f)]
	public float meshSubNumber;

	[Range(0f, 1f)]
	public float hairNumber;

	[Range(0f, 1f)]
	public float dyeNumber;

	[Range(0f, 1f)]
	public int skinType;

	[Header("Mouth")]
	public float mouthFlapPosRange = 0.02f;

	public float mouthFlapRotRange = 10f;

	public float mouthSidePosRange = 0.1f;

	public float mouthSideFrequency = 15f;

	[Header("Arm Secondary Motion")]
	public float ArmMotionWeight = 1f;

	public float ArmGravity = 2f;

	public AnimationCurve ArmGravityByPitch = new AnimationCurve(new Keyframe[3]
	{
		new Keyframe(-70f, 1f),
		new Keyframe(0f, 0f),
		new Keyframe(70f, 1f)
	});

	public float ArmStiffness = 110f;

	public float ArmDamping = 11f;

	public float ArmMotionSpeedLimit = 1f;

	public float ArmFlexAngle = 35f;

	public AnimationCurve ArmFlexByPitch = new AnimationCurve(new Keyframe[3]
	{
		new Keyframe(-70f, 0f),
		new Keyframe(0f, 0f),
		new Keyframe(70f, 1f)
	});

	public float ArmSpinFadeStart = 250f;

	public float ArmSpinFadeEnd = 500f;

	[Header("Spine IK")]
	public float SpineLookUpLimit = -45f;

	public float SpineTwistClamp = 90f;

	public float SpineHeadLookWeightVertical = 0.2f;

	public float HeadYawLimit = 70f;

	public float SpineLowerFollow = 0.3f;

	public float SpineFollowSmoothSpeed = 32f;

	public MovementSounds movementSounds;

	public bool showSash;

	public int tempPoseType;

	public uint underwearSkin;

	[NonSerialized]
	public int paintballColor;

	public ulong overrideSkinSeed { get; private set; }

	public bool IsFemale => skinType == 1;

	public SkinSetCollection SkinSet
	{
		get
		{
			if (!IsMannequin)
			{
				if (!IsFemale)
				{
					return MaleSkin;
				}
				return FemaleSkin;
			}
			return MannequinSkin;
		}
	}

	public Quaternion AimAngles
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Quaternion LookAngles
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public bool IsMannequin { get; set; }

	private static Vector3 GetFlat(Vector3 dir)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		dir.y = 0f;
		return dir.normalized;
	}

	public static void RebuildAll()
	{
	}

	public bool TryGetPlayer(out BasePlayer player)
	{
		player = GameObjectEx.ToBaseEntity(((Component)this).gameObject) as BasePlayer;
		return (Object)(object)player != (Object)null;
	}

	public PlayerModel()
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected Obj, but got Unknown
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Expected Obj, but got Unknown
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected Obj, but got Unknown
	}
}
