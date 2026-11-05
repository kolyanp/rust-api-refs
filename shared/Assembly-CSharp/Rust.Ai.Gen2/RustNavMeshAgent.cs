using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;

namespace Rust.Ai.Gen2;

public class RustNavMeshAgent : EntityComponent<BaseEntity>, IServerComponent
{
	public enum ArrivalOutcome
	{
		None,
		Docking,
		Docked,
		Aborted
	}

	internal struct PathFollowingSpeed
	{
		public float current;

		public float desired;

		public float acceleration;

		public float deceleration;

		public float emergencyDeceleration;

		public float walkSpeed;

		public float brakeFloor;

		public float slowDownRadius;

		public float remaining;

		public float stopping;

		public byte unityBraking;

		public byte autoBraking;

		public float Advance(float dt, out byte adjustment)
		{
			adjustment = 0;
			bool flag = autoBraking != 0;
			if (unityBraking != 0)
			{
				if (flag && remaining < slowDownRadius && remaining * desired < slowDownRadius * current)
				{
					adjustment = 1;
					return Mathf.Max(0f, current - dt * current * current / (2f * Mathf.Max(remaining, 0.001f)));
				}
				flag = false;
			}
			if (flag && remaining - stopping < GetBrakingDistance(current, deceleration))
			{
				adjustment = 1;
				return Mathf.Max(brakeFloor, current - deceleration * dt);
			}
			if (current > desired)
			{
				adjustment = 2;
				float num = (current - desired) / deceleration;
				float num2 = ((current > walkSpeed && num > 1f) ? emergencyDeceleration : deceleration);
				return Mathf.Max(desired, current - num2 * dt);
			}
			if (current < desired)
			{
				adjustment = 3;
				return Mathf.Min(desired, current + acceleration * dt);
			}
			return current;
		}
	}

	internal struct PathFollowingUpdate
	{
		public IntPtr navmesh;

		public IntPtr corridor;

		public Vector3 position;

		public Vector3 waypoint;

		public Vector3 destination;

		public Vector3 optimizeTarget;

		public Vector3 forward;

		public Vector3 velocity;

		public PathFollowingSpeed speed;

		public float deviation;

		public float turnRadius;

		public float baseOffset;

		public int moved;

		public byte flags;

		public byte adjustment;
	}

	public enum Speeds
	{
		Sneak,
		Walk,
		Jog,
		Run,
		Sprint,
		FullSprint
	}

	internal struct MovementTransformUpdate
	{
		public Vector3 position;

		public Quaternion rotation;

		public float maxDegreesDelta;

		public byte flags;

		public ushort space;

		public byte swimming;

		public float swimTarget;

		public float swimFloor;

		public PathFollowingUpdate path;
	}

	[BurstCompile(CompileSynchronously = true)]
	internal struct MovementTransformJob : IJobParallelForTransform
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<MovementTransformUpdate> updates;

		[ReadOnly]
		public NativeArray<int> slotUpdates;

		[NativeDisableParallelForRestriction]
		[ReadOnly]
		public NativeArray<IntPtr> queries;

		[NativeDisableParallelForRestriction]
		[ReadOnly]
		public NativeArray<float4x4> spaces;

		[NativeDisableUnsafePtrRestriction]
		public unsafe BodyWalls* bodies;

		public float deltaTime;

		[NativeSetThreadIndex]
		private int threadIndex;

		public unsafe void Execute(int index, TransformAccess transform)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			int num = slotUpdates[index] - 1;
			if (num < 0 || num >= updates.Length)
			{
				return;
			}
			MovementTransformUpdate update = updates[num];
			if (update.flags == 0 || !transform.isValid)
			{
				return;
			}
			float4x4 navToWorld = ((update.space == 0) ? float4x4.identity : spaces[update.space - 1]);
			bool flag = (update.flags & 0x88) != 0;
			IntPtr query = (flag ? queries[threadIndex] : IntPtr.Zero);
			float currentY = (((update.flags & 0x10) != 0) ? transform.position.y : 0f);
			BodyWalls* body = ((bodies != null) ? (bodies + index) : null);
			Quaternion val = ResolveMovement(ref update, query, body, transform.rotation, currentY, in navToWorld, deltaTime);
			if (flag)
			{
				updates[num] = update;
			}
			if ((update.flags & 1) != 0)
			{
				if ((update.flags & 2) != 0)
				{
					transform.SetPositionAndRotation(update.position, val);
				}
				else
				{
					transform.position = update.position;
				}
			}
			else if ((update.flags & 2) != 0)
			{
				transform.rotation = val;
			}
		}
	}

	internal struct BodyWalls
	{
		public unsafe fixed float segments[80];

		public unsafe fixed uint tiles[12];

		public float2 centre;

		public IntPtr navmesh;

		public float front;

		public float back;

		public float halfWidth;

		public int count;

		public int tileCount;

		public int tileVersion;

		public uint stamp;
	}

	public bool letUnityMoveAgentIfPossible = true;

	private static ListHashSet<RustNavMeshAgent> enabledComponents = new ListHashSet<RustNavMeshAgent>();

	private static RustNavMeshPath tempPath = new RustNavMeshPath();

	private static RustNavMeshPath scratchPath = new RustNavMeshPath();

	private RustNavMeshPath CurPathNS = new RustNavMeshPath();

	private NavMeshAgent _agent;

	private Transform cachedTransform;

	private BaseEntity cachedEntity;

	private NavVector3 previousPositionNS;

	private IntPtr corridor;

	private bool followingPath;

	private readonly List<NavVector3> corners = new List<NavVector3>();

	private float remainingCornerTailDistance;

	private bool lastCornerIsEnd;

	private bool cornersDirty = true;

	private const float CorridorOptimizationRange = 20f;

	private const float CornerRepullDistance = 0.25f;

	private const float SteeringCornerRepullMoveDistance = 0.35f;

	private const float SteeringCornerRepullMaxInterval = 0.25f;

	private NavVector3 lastCornerPullPositionNS;

	private float lastCornerPullTime;

	private int lastSeenTileVersion = -1;

	private int anchorCheckedTileDataVersion = -1;

	private uint lastCheckedFlagStamp;

	private bool lastCheckedNpcDoors;

	private RustNavmesh boundNavmesh;

	[NonSerialized]
	public readonly List<NavVector3> lastValidPath = new List<NavVector3>();

	private bool isScientist;

	private bool npcDoorsOpenForEntity;

	private SenseComponent senses;

	private const float ReachSnapRange = 1f;

	private const float ReachSnapHeight = 2f;

	private const float ReachSnapClimb = 0.75f;

	private const float ReachSnapDropRange = 0.35f;

	private const float StraightPathEndSlack = 0.01f;

	[SerializeField]
	[Header("Base fields")]
	private int _agentTypeID;

	[SerializeField]
	private float _baseOffset;

	[SerializeField]
	private float _desiredSpeed;

	[SerializeField]
	private float _angularSpeed;

	[SerializeField]
	public ResettableFloat _acceleration = new ResettableFloat(10f);

	[SerializeField]
	private float _stoppingDistance;

	[SerializeField]
	private bool _autoBraking;

	[SerializeField]
	private float _height;

	[SerializeField]
	private int _avoidancePriority;

	[SerializeField]
	private int _areaMask;

	[SerializeField]
	private bool _updatePosition = true;

	[SerializeField]
	private bool _updateRotation = true;

	private ObstacleAvoidanceType _obstacleAvoidanceType;

	private bool _isStopped;

	private NavVector3 _velocityNS = NavVector3.zero;

	private NavVector3 _nextPositionNS = NavVector3.zero;

	[NonSerialized]
	private float _stoppingDistanceOverride = -1f;

	private const float FindClosestEdgeMaxRadius = 10f;

	private const float MovingTargetPatchMaxDistance = 3f;

	private const float MovingTargetFullReplanInterval = 1f;

	private float lastFullPlanTime = float.NegativeInfinity;

	public Nullable overrideDirectionWS;

	private const float CurvePlanningRange = 5f;

	private const float CurveOnMesh = 0.05f;

	private const int CurveReplans = 2;

	private static readonly float[] CurveTurnRadiusScales = new float[3] { 1f, 0.5f, 0.25f };

	private const float CurveMostTurning = 4.712389f;

	private const float NoLaneMostTurning = MathF.PI / 2f;

	private const float CurvePlanInterval = 0.25f;

	private const float CurveSampleSpacing = 0.3f;

	private const float CurveMinimumStraight = 1f;

	private const float CurveDeceleration = 1.5f;

	private const float CurveCreepSpeed = 0.15f;

	private const float CurveAllowedDrift = 0.3f;

	private const float HandoverMargin = 0.5f;

	private const float DockedSideways = 0.3f;

	private const float DockedAlong = 0.3f;

	private const float DockedHeading = 25f;

	private const float SameArrival = 0.05f;

	private bool followingCurve;

	private NavVector3 arrivalStandNS;

	private NavVector3 arrivalForwardNS;

	private float arrivalLaneLength;

	private float nextCurvePlan;

	private PoseCurve curve;

	private float curveDistance;

	private float curveSpeed;

	private int curveReplansLeft;

	[Header("Doors")]
	public bool canOpenDoors;

	private const byte PathFollowingUpdateFlag = 8;

	private const byte PathSteering = 1;

	private const byte PathClampStep = 2;

	private const byte PathAutoBraking = 4;

	private const byte PathCachedForward = 8;

	private const byte PathOptimize = 16;

	private const byte PathNpcDoorsWillOpen = 32;

	private static IntPtr[] movementQueries;

	private HashSet<object> pausingSources = new HashSet<object>();

	[Header("Movement speed")]
	public float sneakSpeed = 0.6f;

	public float walkSpeed = 0.89f;

	public float jogSpeed = 2.45f;

	public float runSpeed = 4.4f;

	public float sprintSpeed = 6f;

	public float fullSprintSpeed = 9f;

	[Tooltip("Slowest gait this species has an animation for. A shared state may ask for something slower, but with no clip authored at that speed it reads as sliding - livestock have no sneak, for instance.")]
	public Speeds minimumGait;

	public ResettableFloat deceleration = new ResettableFloat(2f);

	public float emergencyDeceleration = 10f;

	private float currentSpeed;

	private float dampVelocity;

	private float _agentTypeRadius = -1f;

	[NonSerialized]
	[Tooltip("Gaits to shift whatever is asked for. Positive drops it, so an old animal jogs where it used to run. Negative raises it, which is how a calf keeps up: the clips are shared with its mother, so at her gait the same animation carries its shorter legs along far too slowly.")]
	public int gaitPenalty;

	[Header("Steering")]
	public bool canSteer = true;

	public float maxTurnRadius = 2f;

	[NonSerialized]
	public float currentDeviation;

	private NavVector3 cachedSteeringForwardNS;

	private bool hasCachedSteeringForward;

	private const float SteerToRestStep = 0.25f;

	private const int SteerToRestSteps = 64;

	[Header("Swimming")]
	public bool canSwim;

	public float swimSpeed = 0.6f;

	public float swimSprintSpeed = 0.89f;

	public ResettableFloat desiredSwimDepth = new ResettableFloat(0.7f);

	[Header("Terrain Preferences")]
	public Enum preferedTopology = (Enum)537002081;

	public Enum preferedBiome = (Enum)15;

	private const byte PositionUpdate = 1;

	private const byte RotationUpdate = 2;

	private const byte InterpolateRotation = 4;

	private const byte SwimUpdate = 16;

	private const byte StartFromRotation = 32;

	private const byte KeepRotation = 64;

	private const byte DeltaMoveUpdate = 128;

	private const byte MovementUpdates = 136;

	private const byte FacingUpdates = 102;

	private const ushort UnresolvedSpace = ushort.MaxValue;

	private const int MinimumTransformBatch = 32;

	private const int IdleTicksBeforeLeaving = 8;

	private static TransformAccessArray movementTransforms;

	private static MovementTransformUpdate[] movementUpdates;

	private static RustNavMeshAgent[] pendingTransformAgents;

	private static MovementTransformUpdate[] landingUpdates;

	private static RustNavMeshAgent[] landingAgents;

	private static RustNavMeshAgent[] residentAgents;

	private static int[] slotUpdates;

	private static int[] slotLastQueued;

	private static int residentCount;

	private static int movementTick;

	private static IndependantNavmesh[] spaceNavmeshes;

	private static float4x4[] spaceMatrices;

	private static int spaceCount;

	private static bool transformBatchActive;

	private static int pendingTransformUpdates;

	private bool inEnabledSet;

	private bool placedWhilePaused;

	private int transformSlot = -1;

	[Tooltip("Steers so the whole body, the entity's bounds, stays inside the navmesh walls rather than only its pivot.")]
	public bool keepBodyOffWalls;

	private const byte PathBodyOffWalls = 64;

	private const int BodyWallSegments = 16;

	private const int BodyWallTiles = 12;

	private const float BodyWallRefreshDistance = 0.5f;

	private const float BodyLookaheadSeconds = 0.3f;

	private const float BodyMaxLookahead = 0.5f;

	private const float BodyTightestTurnRadius = 0.75f;

	private const float BodySlowestTurnDegrees = 90f;

	private const float BodySlowestStep = 0.25f;

	private const float BodyStuckStep = 0.1f;

	private const int BodySearchAngles = 5;

	private const int BodyGentlestTurnShare = 8;

	private const float BodyTurnOutClearance = 0.04f;

	private const float BodyClearance = 0.02f;

	private const float BodyTouchDistance = 0.01f;

	private const int BodyTurnSamples = 16;

	private const float BoxOutlineSpacing = 0.1f;

	private static BodyWalls[] slotBodies;

	private Transform AgentTransform => cachedTransform ?? (cachedTransform = ((Component)this).transform);

	private bool HasLiveEntity
	{
		get
		{
			if (cachedEntity != null)
			{
				return cachedEntity.net != null;
			}
			return false;
		}
	}

	public IndependantNavmesh independantNavmesh { get; private set; }

	public bool HasValidIndependantNavmesh
	{
		get
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if ((Object)(object)independantNavmesh != (Object)null && independantNavmesh.Navmesh != null)
			{
				return independantNavmesh.Navmesh.IsBuilt();
			}
			return false;
		}
	}

	public bool IsNavMeshBuilt
	{
		get
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (boundNavmesh == null || !boundNavmesh.IsValid())
			{
				TryBindNavmesh();
			}
			if (boundNavmesh != null)
			{
				return boundNavmesh.IsBuilt();
			}
			return false;
		}
	}

	public bool npcDoorsWillOpen
	{
		get
		{
			if (!canOpenDoors)
			{
				return npcDoorsOpenForEntity;
			}
			return true;
		}
	}

	public NavVector3 forward
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return WorldToNavDirection(((Component)baseEntity).transform.forward);
		}
	}

	public NavVector3 right
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return WorldToNavDirection(((Component)baseEntity).transform.right);
		}
	}

	public NavVector3 up
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return WorldToNavDirection(((Component)baseEntity).transform.up);
		}
	}

	public OffMeshLinkData currentOffMeshLinkData
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.currentOffMeshLinkData;
			}
			return default;
		}
	}

	public int agentTypeID
	{
		get
		{
			return _agentTypeID;
		}
		set
		{
			_agentTypeID = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.agentTypeID = value;
			}
		}
	}

	public int areaMask
	{
		get
		{
			return _areaMask;
		}
		set
		{
			_areaMask = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.areaMask = value;
			}
		}
	}

	public float speed
	{
		get
		{
			return _desiredSpeed;
		}
		set
		{
			_desiredSpeed = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.speed = value;
			}
		}
	}

	public float angularSpeed
	{
		get
		{
			return _angularSpeed;
		}
		set
		{
			_angularSpeed = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.angularSpeed = value;
			}
		}
	}

	public float acceleration
	{
		get
		{
			return _acceleration.Value;
		}
		set
		{
			_acceleration.Value = value;
			if (letUnityMoveAgentIfPossible)
			{
				deceleration.Value = value;
			}
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.acceleration = value;
			}
		}
	}

	public bool updatePosition
	{
		get
		{
			return _updatePosition;
		}
		set
		{
			_updatePosition = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.updatePosition = value;
			}
		}
	}

	public bool updateRotation
	{
		get
		{
			return _updateRotation;
		}
		set
		{
			_updateRotation = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.updateRotation = value;
			}
		}
	}

	public float stoppingDistance
	{
		get
		{
			return _stoppingDistance;
		}
		set
		{
			_stoppingDistance = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.stoppingDistance = value;
			}
		}
	}

	public float currentStoppingDistance
	{
		get
		{
			if (!(_stoppingDistanceOverride >= 0f))
			{
				return _stoppingDistance;
			}
			return _stoppingDistanceOverride;
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.height = value;
			}
		}
	}

	public ObstacleAvoidanceType obstacleAvoidanceType
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _obstacleAvoidanceType;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			_obstacleAvoidanceType = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.obstacleAvoidanceType = value;
			}
		}
	}

	public int avoidancePriority
	{
		get
		{
			return _avoidancePriority;
		}
		set
		{
			_avoidancePriority = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.avoidancePriority = value;
			}
		}
	}

	public bool isStopped
	{
		get
		{
			return _isStopped;
		}
		set
		{
			_isStopped = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.isStopped = value;
			}
		}
	}

	public bool autoBraking
	{
		get
		{
			return _autoBraking;
		}
		set
		{
			_autoBraking = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.autoBraking = value;
			}
		}
	}

	public float baseOffset
	{
		get
		{
			return _baseOffset;
		}
		set
		{
			_baseOffset = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.baseOffset = value;
			}
		}
	}

	public Vector3 nextPositionWS
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return NavToWorldSpace(nextPosition);
		}
	}

	public NavVector3 nextPosition
	{
		get
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (TryGetAgent(out var agent))
				{
					return new NavVector3(agent.nextPosition);
				}
				if (AI.logIssues)
				{
					Debug.LogError((object)"RustNavMeshAgent.nextPosition called with useUnityNavmesh enabled, but no NavMeshAgent was found on the entity.");
				}
				return _nextPositionNS;
			}
			if (currentPolyRef != 0L)
			{
				return _nextPositionNS;
			}
			return WorldToNavSpace(AgentTransform.position);
		}
	}

	public NavMeshPathStatus pathStatus
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return (NavMeshPathStatus)2;
				}
				return agent.pathStatus;
			}
			if (!hasPath)
			{
				return (NavMeshPathStatus)2;
			}
			return CurPathNS.status;
		}
	}

	public bool isOnOffMeshLink
	{
		get
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.isOnOffMeshLink;
			}
			return false;
		}
	}

	public bool pathPending
	{
		get
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.pathPending;
			}
			return false;
		}
	}

	public NavVector3 steeringTarget
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.steeringTarget);
			}
			if (corners.Count <= 0)
			{
				return _nextPositionNS;
			}
			return corners[0];
		}
	}

	public NavVector3 velocity
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.velocity);
			}
			return _velocityNS;
		}
		set
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			_velocityNS = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.velocity = value.Value;
			}
			if (!AI.useUnityNavmesh && AI.logIssues)
			{
				RustNavigation.LogError("Setting velocity on RustNavMeshAgent has no effect when not using Unity NavMesh. Use Move() to move the agent instead.");
			}
		}
	}

	public Vector3 desiredVelocityWS
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return NavToWorldDirection(desiredVelocity);
		}
	}

	public NavVector3 desiredVelocity
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.desiredVelocity);
			}
			return velocity;
		}
	}

	public bool hasPath
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				return agent.hasPath;
			}
			if (!followingPath)
			{
				return followingCurve;
			}
			return true;
		}
	}

	public Vector3 destinationWS
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return NavToWorldSpace(destination);
		}
	}

	public NavVector3 destination
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.destination);
			}
			if (!hasPath)
			{
				return _nextPositionNS;
			}
			return CurPathNS.GetDestinationNS();
		}
		set
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (TryGetAgent(out var agent))
				{
					agent.destination = value.Value;
				}
			}
			else
			{
				SetDestination(value);
			}
		}
	}

	public float remainingDistance
	{
		get
		{
			using (TimeWarning.New("RustNavMeshAgent.remainingDistance"))
			{
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent))
					{
						return 0f;
					}
					return agent.remainingDistance;
				}
				if (!hasPath)
				{
					return 0f;
				}
				if (corners.Count > 0)
				{
					return RemainingDistanceAlongCorners();
				}
				return CurPathNS.GetPathLength();
			}
		}
	}

	public bool isOnNavMesh
	{
		get
		{
			using (TimeWarning.New("RustNavMeshAgent.isOnNavMesh"))
			{
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent))
					{
						return false;
					}
					return agent.isOnNavMesh;
				}
				ulong num = currentPolyRef;
				if (num == 0L)
				{
					TryBindNavmesh();
				}
				else if (boundNavmesh != null && boundNavmesh.IsValidPolyRef(num))
				{
					return true;
				}
				NavHit hitNS;
				return SamplePosition(_nextPositionNS, out hitNS, 2f, debugDraw: false);
			}
		}
	}

	public NpcRandom Rng => NpcRandom.Of(baseEntity);

	public ulong CurrentPolyRef => currentPolyRef;

	public int NavmeshChangeVersion
	{
		get
		{
			if (boundNavmesh == null)
			{
				return 0;
			}
			return boundNavmesh.TileChangeVersion;
		}
	}

	public RustNavmesh BoundNavmesh => boundNavmesh;

	private ulong currentPolyRef
	{
		get
		{
			if (!(corridor != IntPtr.Zero))
			{
				return 0uL;
			}
			return RecastWrapper.CorridorGetFirstPoly(corridor);
		}
	}

	private bool HasStaleAnchor
	{
		get
		{
			if (!followingPath || lastSeenTileVersion != boundNavmesh.TileChangeVersion)
			{
				return !boundNavmesh.IsValidPolyRef(currentPolyRef);
			}
			return false;
		}
	}

	private bool OnMovingNavmesh
	{
		get
		{
			if (independantNavmesh != null && independantNavmesh.canMove)
			{
				return HasValidIndependantNavmesh;
			}
			return false;
		}
	}

	public ArrivalOutcome LastArrival { get; private set; }

	private bool hasArrival => LastArrival == ArrivalOutcome.Docking;

	public bool CanArriveFacing
	{
		get
		{
			if (!AI.useUnityNavmesh)
			{
				return IsSteering;
			}
			return false;
		}
	}

	private NavVector3 Heading => ((HasPendingTransform && hasCachedSteeringForward) ? cachedSteeringForwardNS : forward).Flat();

	public bool IsJumping
	{
		get
		{
			return baseEntity.HasFlag(BaseEntity.Flags.Reserved2);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved2, value);
		}
	}

	public bool IsPaused
	{
		get
		{
			if (pausingSources.Count == 0)
			{
				return false;
			}
			bool wasPaused = pausingSources.Count > 0;
			bool flag;
			do
			{
				flag = false;
				foreach (object pausingSource in pausingSources)
				{
					if (ObjectEx.IsUnityNull(pausingSource))
					{
						if (AI.logIssues)
						{
							RustNavigation.LogError("Removing null pausing source from " + ((Object)baseEntity).name + ".");
						}
						pausingSources.Remove(pausingSource);
						flag = true;
						break;
					}
				}
			}
			while (flag);
			OnChange(wasPaused);
			return pausingSources.Count > 0;
		}
	}

	private float agentTypeRadius
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (_agentTypeRadius < 0f)
			{
				NavMeshBuildSettings settingsByID = NavMesh.GetSettingsByID(_agentTypeID);
				float agentRadius = settingsByID.agentRadius;
				_agentTypeRadius = ((agentRadius > 0f) ? agentRadius : 0.5f);
			}
			return _agentTypeRadius;
		}
	}

	public bool IsSprinting => currentSpeed >= sprintSpeed;

	private bool IsSteering
	{
		get
		{
			if (!letUnityMoveAgentIfPossible)
			{
				return canSteer;
			}
			return false;
		}
	}

	private NavVector3 SteeringForwardNS
	{
		get
		{
			if (!hasCachedSteeringForward)
			{
				return forward;
			}
			return cachedSteeringForwardNS;
		}
	}

	public bool IsSwimming
	{
		get
		{
			return baseEntity.HasFlag(BaseEntity.Flags.Reserved1);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved1, value);
		}
	}

	private bool HasPendingTransform
	{
		get
		{
			if (transformSlot >= 0)
			{
				return slotUpdates[transformSlot] != 0;
			}
			return false;
		}
	}

	private ref MovementTransformUpdate PendingUpdate => ref movementUpdates[slotUpdates[transformSlot] - 1];

	private ushort QueuedSpace
	{
		get
		{
			if (!OnMovingNavmesh)
			{
				return 0;
			}
			return ushort.MaxValue;
		}
	}

	private float4x4 NavToWorldNow
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			if (!OnMovingNavmesh)
			{
				return float4x4.identity;
			}
			return float4x4.op_Implicit(independantNavmesh.NavToWorldMatrix);
		}
	}

	private bool BodyWallsKnown
	{
		get
		{
			if (keepBodyOffWalls && HasLiveEntity && boundNavmesh != null && boundNavmesh.IsValid() && corridor != IntPtr.Zero && !AI.useUnityNavmesh)
			{
				return RecastWrapper.WallsAroundAvailable;
			}
			return false;
		}
	}

	private bool TryGetAgent(out NavMeshAgent agent)
	{
		agent = null;
		if (!RustNavigation.EnsureUnityNavmesh())
		{
			return false;
		}
		if ((Object)(object)_agent == (Object)null)
		{
			_agent = ((Component)this).gameObject.GetComponent<NavMeshAgent>();
		}
		if ((Object)(object)_agent == (Object)null)
		{
			if (AI.logIssues)
			{
				BaseEntity baseEntity = base.baseEntity;
				RustNavigation.LogError("Entity " + (((baseEntity != null) ? ((Object)baseEntity).name : null) ?? "unknown entity") + " has RustNavMeshAgent but no NavMeshAgent component. This component will not function correctly without a NavMeshAgent.");
			}
			return false;
		}
		agent = _agent;
		return true;
	}

	public NavVector3 WorldToNavSpace(Vector3 positionWS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			Matrix4x4 worldToNavMeshSpace = baseEntity.WorldToNavMeshSpace;
			return new NavVector3(worldToNavMeshSpace.MultiplyPoint3x4(positionWS));
		}
		if (independantNavmesh == null)
		{
			return new NavVector3(positionWS);
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformPointFromWorldSpaceToNavSpace(positionWS);
		}
		return new NavVector3(positionWS);
	}

	public Vector3 NavToWorldSpace(NavVector3 positionNS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			Matrix4x4 navMeshToWorldSpace = baseEntity.NavMeshToWorldSpace;
			return navMeshToWorldSpace.MultiplyPoint3x4(positionNS.Value);
		}
		if (independantNavmesh == null)
		{
			return positionNS.Value;
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformPointFromNavSpaceToWorldSpace(positionNS);
		}
		return positionNS.Value;
	}

	public Vector3 NavToWorldDirection(NavVector3 directionNS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			Matrix4x4 navMeshToWorldSpace = baseEntity.NavMeshToWorldSpace;
			return navMeshToWorldSpace.MultiplyVector(directionNS.Value);
		}
		if (independantNavmesh == null)
		{
			return directionNS.Value;
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(directionNS);
		}
		return directionNS.Value;
	}

	public NavVector3 WorldToNavDirection(Vector3 directionWS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			Matrix4x4 worldToNavMeshSpace = baseEntity.WorldToNavMeshSpace;
			return new NavVector3(worldToNavMeshSpace.MultiplyVector(directionWS));
		}
		if (independantNavmesh == null)
		{
			return new NavVector3(directionWS);
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformDirectionFromWorldSpaceToNavSpace(directionWS);
		}
		return new NavVector3(directionWS);
	}

	public void ApplySerializedSettingsToAgent(NavMeshAgent agent)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		agent.baseOffset = _baseOffset;
		agent.agentTypeID = agentTypeID;
		agent.speed = _desiredSpeed;
		agent.angularSpeed = _angularSpeed;
		agent.acceleration = _acceleration.Value;
		agent.autoBraking = _autoBraking;
		agent.stoppingDistance = _stoppingDistance;
		agent.height = _height;
		agent.obstacleAvoidanceType = _obstacleAvoidanceType;
		agent.avoidancePriority = _avoidancePriority;
		agent.areaMask = _areaMask;
		agent.updatePosition = _updatePosition;
		agent.updateRotation = _updateRotation;
		((Behaviour)agent).enabled = false;
	}

	private bool SamplePositionInternal(NavVector3 positionNS, out NavHit hitNS, float maxDistance, out ulong nearestPolyRef, bool includeGatedDoorPolys = false)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SamplePositionInternal"))
		{
			hitNS = default;
			nearestPolyRef = 0uL;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				NavMeshHit unityHitNS = default;
				if (!NavMesh.SamplePosition(positionNS.Value, ref unityHitNS, maxDistance, agent.areaMask))
				{
					return false;
				}
				hitNS = NavHit.FromUnity(in unityHitNS);
				return true;
			}
			if (boundNavmesh != null)
			{
				return boundNavmesh.SamplePositionPoly(positionNS, out hitNS, maxDistance * Vector3.one, out nearestPolyRef, includeGatedDoorPolys, npcDoorsWillOpen);
			}
			return false;
		}
	}

	public bool SamplePosition(Vector3 positionWS, out NavMeshHit hitWS, float maxDistance, bool debugDraw = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		hitWS = default;
		if (!SamplePosition(WorldToNavSpace(positionWS), out var hitNS, maxDistance, debugDraw))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool SamplePosition(NavVector3 positionNS, out NavHit hitNS, float maxDistance, bool debugDraw = true)
	{
		ulong nearestPolyRef;
		return SamplePositionPoly(positionNS, out hitNS, maxDistance, out nearestPolyRef, debugDraw);
	}

	public bool SamplePositionPoly(NavVector3 positionNS, out NavHit hitNS, float maxDistance, out ulong nearestPolyRef, bool debugDraw = true, bool includeGatedDoorPolys = false)
	{
		using (TimeWarning.New("RustNavMeshAgent.SamplePosition"))
		{
			bool flag = SamplePositionInternal(positionNS, out hitNS, maxDistance, out nearestPolyRef, includeGatedDoorPolys);
			float num = 1f;
			if (flag && maxDistance > num && Mathf.Abs(hitNS.position.y - positionNS.y) > num && SampleGroundPositionWithPhysics(positionNS, out var hitInfoNS, 3.5f) && SamplePositionInternal(hitInfoNS.point, out var hitNS2, num, out var nearestPolyRef2, includeGatedDoorPolys))
			{
				nearestPolyRef = nearestPolyRef2;
				hitNS = hitNS2;
			}
			if (!flag)
			{
				return false;
			}
			return true;
		}
	}

	public bool Raycast(NavVector3 startNS, NavVector3 endNS, out NavHit hitNS)
	{
		ulong startRef = 0uL;
		return Raycast(ref startRef, startNS, endNS, out hitNS);
	}

	private bool Raycast(ref ulong startRef, NavVector3 startNS, NavVector3 endNS, out NavHit hitNS)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Raycast"))
		{
			hitNS = default;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				NavMeshHit unityHitNS = default;
				if (!NavMesh.Raycast(startNS.Value, endNS.Value, ref unityHitNS, agent.areaMask))
				{
					return false;
				}
				hitNS = NavHit.FromUnity(in unityHitNS);
				return true;
			}
			if (boundNavmesh == null)
			{
				if (AI.logIssues)
				{
					BaseEntity baseEntity = base.baseEntity;
					RustNavigation.LogError(string.Format("No navmesh bound, cannot raycast for {0} from {1} to {2}", ((baseEntity != null) ? ((Object)baseEntity).name : null) ?? "unknown entity", startNS, endNS));
				}
				return false;
			}
			return boundNavmesh.Raycast(ref startRef, startNS, endNS, out hitNS, npcDoorsWillOpen);
		}
	}

	public bool CalculatePath(NavVector3 startNS, NavVector3 endNS, RustNavMeshPath pathNS)
	{
		ulong startRef = 0uL;
		return CalculatePath(ref startRef, startNS, endNS, pathNS);
	}

	private bool CalculatePath(ref ulong startRef, NavVector3 startNS, NavVector3 endNS, RustNavMeshPath pathNS)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.CalculatePath"))
		{
			pathNS.Reset();
			bool flag = false;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				flag = RustNavMeshHelpers.CalculatePath(startNS.Value, endNS.Value, agent.areaMask, pathNS);
			}
			else if (boundNavmesh != null)
			{
				flag = boundNavmesh.CalculatePath(ref startRef, startNS, endNS, pathNS, npcDoorsWillOpen);
			}
			else
			{
				flag = false;
				if (AI.logIssues)
				{
					BaseEntity baseEntity = base.baseEntity;
					RustNavigation.LogError(string.Format("No navmesh bound, cannot calculate path for {0} from {1} to {2}", ((baseEntity != null) ? ((Object)baseEntity).name : null) ?? "unknown entity", startNS, endNS));
				}
			}
			if (flag)
			{
				_ = pathNS.status;
			}
			return flag;
		}
	}

	private bool SampleReachTarget(NavVector3 locationNS, out NavHit hitNS)
	{
		if (!SamplePosition(locationNS, out hitNS, 2f))
		{
			return false;
		}
		float num = ((locationNS.y - hitNS.position.y > 0.75f) ? 0.35f : 1f);
		return NavVector3.DistanceXZ(hitNS.position, locationNS) <= num;
	}

	public bool CanReach(Vector3 locationWS, bool updateLastValidPath = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return CanReach(WorldToNavSpace(locationWS), updateLastValidPath);
	}

	public bool CanReach(NavVector3 locationNS, bool updateLastValidPath = false)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		using (TimeWarning.New("RustNavMeshAgent.CanReach"))
		{
			if (!SampleReachTarget(locationNS, out var hitNS))
			{
				return false;
			}
			if (!CalculatePath(hitNS.position, tempPath))
			{
				return false;
			}
			bool result = (int)tempPath.status == 0;
			if (updateLastValidPath)
			{
				lastValidPath.Clear();
				lastValidPath.AddRange(tempPath.corners);
			}
			return result;
		}
	}

	public bool HasStraightPathTo(Vector3 locationWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return HasStraightPathTo(WorldToNavSpace(locationWS));
	}

	public bool HasStraightPathTo(NavVector3 locationNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.HasStraightPathTo"))
		{
			if (!SampleReachTarget(locationNS, out var hitNS))
			{
				return false;
			}
			NavHit hitNS2;
			return !Raycast(hitNS.position, out hitNS2) || NavVector3.DistanceXZ(hitNS2.position, hitNS.position) <= 0.01f;
		}
	}

	public bool SetDestinationWithParams(Vector3 targetPositionWS, bool autoBraking = true, Speeds? gait = null, float? acceleration = null, float? deceleration = null, float? deviation = null, float? swimDepth = null, float? stoppingDistance = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return SetDestinationWithParams(WorldToNavSpace(targetPositionWS), autoBraking, gait, acceleration, deceleration, deviation, swimDepth, stoppingDistance);
	}

	public bool SetDestinationWithParams(NavVector3 targetPositionNS, bool autoBraking = true, Speeds? gait = null, float? acceleration = null, float? deceleration = null, float? deviation = null, float? swimDepth = null, float? stoppingDistance = null)
	{
		if (!SetDestination(targetPositionNS))
		{
			return false;
		}
		this.autoBraking = autoBraking;
		SetStoppingDistanceOverride(stoppingDistance ?? (-1f));
		if (gait.HasValue)
		{
			SetGait(gait.Value);
		}
		if (acceleration.HasValue)
		{
			this.acceleration = acceleration.Value;
		}
		if (deceleration.HasValue)
		{
			this.deceleration.Value = deceleration.Value;
		}
		if (deviation.HasValue)
		{
			currentDeviation = deviation.Value;
		}
		if (swimDepth.HasValue)
		{
			desiredSwimDepth.Value = swimDepth.Value;
		}
		return true;
	}

	private void OnEnable()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		enabledComponents.TryAdd(this);
		inEnabledSet = true;
		cachedEntity = baseEntity;
		isScientist = BaseNetworkableEx.Is<ScientistNPC2>((Object)(object)baseEntity, out ScientistNPC2 _);
		npcDoorsOpenForEntity = baseEntity.IsNpcPlayer();
		((Component)this).TryGetComponent<SenseComponent>(ref senses);
		if (AI.useUnityNavmesh)
		{
			_nextPositionNS = WorldToNavSpace(AgentTransform.position);
			if (SamplePositionPoly(_nextPositionNS, out var hitNS, 10f, out var _))
			{
				_nextPositionNS = hitNS.position;
			}
		}
		else
		{
			if (corridor == IntPtr.Zero)
			{
				corridor = RecastWrapper.CreateCorridor();
			}
			TryBindNavmesh();
		}
		previousPositionNS = _nextPositionNS;
		TrySyncWorldPosWithNavPos();
	}

	internal void TryBindNavmesh()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		BindNavmeshAt(AgentTransform.position);
		_nextPositionNS = WorldToNavSpace(AgentTransform.position);
		if (SamplePositionPoly(_nextPositionNS, out var hitNS, 10f, out var nearestPolyRef, debugDraw: true, includeGatedDoorPolys: true))
		{
			_nextPositionNS = hitNS.position;
		}
		AnchorCorridor(nearestPolyRef, _nextPositionNS);
		previousPositionNS = _nextPositionNS;
	}

	private void BindNavmeshAt(Vector3 positionWS)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		lastSeenTileVersion = -1;
		lastCheckedFlagStamp = 0u;
		anchorCheckedTileDataVersion = -1;
		independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(positionWS);
		if (HasValidIndependantNavmesh)
		{
			boundNavmesh = independantNavmesh.Navmesh;
		}
		else
		{
			boundNavmesh = (RustNavigation.Instance.IsDefaultNavmeshBuilt() ? RustNavigation.Instance.DefaultNavmesh : null);
		}
	}

	private void OnDestroy()
	{
		if (corridor != IntPtr.Zero)
		{
			RecastWrapper.FreeCorridor(corridor);
			corridor = IntPtr.Zero;
		}
	}

	private void RefreshCorners()
	{
		cornersDirty = false;
		lastCornerPullPositionNS = _nextPositionNS;
		lastCornerPullTime = Time.time;
		boundNavmesh.CorridorFindCorners(corridor, corners, 256, out lastCornerIsEnd);
		remainingCornerTailDistance = 0f;
		for (int i = 1; i < corners.Count; i++)
		{
			remainingCornerTailDistance += NavVector3.Distance(corners[i - 1], corners[i]);
		}
	}

	private bool ShouldRepullSteeringCorners()
	{
		if (corners.Count == 0)
		{
			return true;
		}
		float num = _nextPositionNS.x - corners[0].x;
		float num2 = _nextPositionNS.z - corners[0].z;
		if (num * num + num2 * num2 <= 0.0625f)
		{
			return true;
		}
		float num3 = _nextPositionNS.x - lastCornerPullPositionNS.x;
		num2 = _nextPositionNS.z - lastCornerPullPositionNS.z;
		if (num3 * num3 + num2 * num2 >= 0.122499995f)
		{
			return true;
		}
		return Time.time - lastCornerPullTime >= 0.25f;
	}

	private float RemainingDistanceAlongCorners()
	{
		using (TimeWarning.New("Tick.RemainingAlongCorners"))
		{
			return (corners.Count == 0) ? 0f : (NavVector3.Distance(_nextPositionNS, corners[0]) + remainingCornerTailDistance);
		}
	}

	private void OnDisable()
	{
		inEnabledSet = false;
		enabledComponents.Remove(this);
		try
		{
			LeaveMovementTransforms();
		}
		finally
		{
			if (enabledComponents.Count == 0 && !transformBatchActive)
			{
				DisposeTransformBatch();
			}
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				((Behaviour)agent).enabled = false;
			}
			if (!AI.useUnityNavmesh)
			{
				ResetPath();
			}
		}
	}

	public static void RebindAgentsAfterNavmeshSwap()
	{
		int count = enabledComponents.Count;
		for (int i = 0; i < count; i++)
		{
			RustNavMeshAgent rustNavMeshAgent = enabledComponents[i];
			if (!ObjectEx.IsUnityNull(rustNavMeshAgent) && (rustNavMeshAgent.boundNavmesh == null || !rustNavMeshAgent.boundNavmesh.IsValid()))
			{
				rustNavMeshAgent.TryBindNavmesh();
				rustNavMeshAgent.ResetPath();
			}
		}
	}

	public static void TickEnabledComponents()
	{
		float deltaTime = Time.deltaTime;
		int count = enabledComponents.Count;
		movementTick++;
		transformBatchActive = true;
		try
		{
			for (int i = 0; i < count && i < enabledComponents.Count; i++)
			{
				RustNavMeshAgent rustNavMeshAgent = enabledComponents[i];
				if (rustNavMeshAgent.HasLiveEntity)
				{
					rustNavMeshAgent.Tick(deltaTime);
				}
			}
		}
		finally
		{
			try
			{
				FlushTransformBatch();
			}
			finally
			{
				transformBatchActive = false;
				if (enabledComponents.Count == 0)
				{
					DisposeTransformBatch();
				}
				else
				{
					LeaveIdleSlots();
				}
			}
		}
	}

	private void Tick(float deltaTime)
	{
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		Nullable pendingRotation = null;
		try
		{
			using (TimeWarning.New("RustNavMeshAgent.Tick"))
			{
				if (AI.useUnityNavmesh && TryGetAgent(out var agent))
				{
					if (letUnityMoveAgentIfPossible)
					{
						return;
					}
					if (((Behaviour)agent).enabled && agent.isOnNavMesh)
					{
						agent.isStopped = true;
					}
					agent.updatePosition = false;
					agent.updateRotation = false;
				}
				if (updateRotation && (overrideDirectionWS.HasValue || (isScientist && (Object)(object)senses != (Object)null)))
				{
					using (TimeWarning.New("Tick.Rotation"))
					{
						hasCachedSteeringForward = false;
						Quaternion val;
						if (overrideDirectionWS.HasValue)
						{
							val = Quaternion.RotateTowards(AgentTransform.rotation, Quaternion.LookRotation(Vector3Ex.NormalizeXZ(overrideDirectionWS.Value)), _angularSpeed * deltaTime);
						}
						else
						{
							Matrix4x4 eyeTransform = senses.GetEyeTransform();
							val = Quaternion.LookRotation(Vector3Ex.NormalizeXZ(eyeTransform.rotation * Vector3.forward));
						}
						if (AI.useUnityNavmesh)
						{
							AgentTransform.rotation = val;
						}
						else
						{
							pendingRotation = val;
						}
					}
				}
				if (followingCurve && !IsPaused && !_isStopped)
				{
					TickCurve(deltaTime);
					return;
				}
				if (!hasPath || IsPaused || _isStopped)
				{
					if (!hasPath)
					{
						EnsureAnchorValidAfterTileRebuild();
					}
					_velocityNS = ((_nextPositionNS == previousPositionNS) ? NavVector3.zero : ((_nextPositionNS - previousPositionNS).Flat() / deltaTime));
					if (currentSpeed == 0f && !(previousPositionNS != _nextPositionNS))
					{
						return;
					}
					using (TimeWarning.New("Tick.NoPath"))
					{
						currentSpeed = Mathf.SmoothDamp(currentSpeed, (_nextPositionNS - previousPositionNS).magnitude / deltaTime, ref dampVelocity, 1f, 9999f, deltaTime * 10f);
						if (currentSpeed < 0.005f)
						{
							currentSpeed = 0f;
						}
						return;
					}
				}
				bool isSteering = IsSteering;
				float num = RestingDistance(currentStoppingDistance);
				NavVector3 navVector;
				float num2;
				if (AI.useUnityNavmesh)
				{
					if (TryGetAgent(out var agent2))
					{
						navVector = new NavVector3(agent2.steeringTarget);
						num2 = remainingDistance;
						if (num2 <= num)
						{
							ResetPath();
							return;
						}
						NavVector3 navVector2 = navVector - _nextPositionNS;
						float magnitude = navVector2.magnitude;
						AdjustCurrentSpeedFromDesiredSpeed(_nextPositionNS, _autoBraking, num2, num, deltaTime);
						float num3 = currentSpeed * deltaTime;
						num3 = Mathf.Clamp(num3, 0f - magnitude, magnitude);
						NavVector3 navVector3 = ((magnitude > 1E-05f) ? (navVector2 * (num3 / magnitude)) : NavVector3.zero);
						MoveInternal(navVector3, num2);
						_velocityNS = navVector3 / deltaTime;
					}
					return;
				}
				if (boundNavmesh == null || corridor == IntPtr.Zero)
				{
					ResetPath();
					return;
				}
				if (!boundNavmesh.IsValid())
				{
					ResetPath();
					TryBindNavmesh();
					return;
				}
				bool flag = npcDoorsWillOpen;
				if (lastSeenTileVersion != boundNavmesh.TileChangeVersion || flag != lastCheckedNpcDoors)
				{
					lastSeenTileVersion = boundNavmesh.TileChangeVersion;
					if (flag != lastCheckedNpcDoors)
					{
						lastCheckedNpcDoors = flag;
						lastCheckedFlagStamp = 0u;
					}
					bool flag2 = boundNavmesh.CorridorIsValid(corridor, int.MaxValue, flag, lastCheckedFlagStamp, out var currentStamp);
					lastCheckedFlagStamp = currentStamp;
					if (!flag2)
					{
						using (TimeWarning.New("Tick.Replan"))
						{
							ulong startRef = currentPolyRef;
							if (!boundNavmesh.CalculatePath(ref startRef, _nextPositionNS, CurPathNS.GetDestinationNS(), tempPath, npcDoorsWillOpen) || (int)tempPath.status != 0 || !SetPath(tempPath))
							{
								ResetPath();
								return;
							}
						}
					}
				}
				if (HasPendingTransform)
				{
					LandOwnUpdate();
					if (!hasPath)
					{
						return;
					}
				}
				if (cornersDirty || (isSteering && ShouldRepullSteeringCorners()))
				{
					RefreshCorners();
				}
				if (corners.Count == 0)
				{
					ResetPath();
					return;
				}
				navVector = corners[0];
				num2 = RemainingDistanceAlongCorners();
				if (hasArrival && !ApplyArrival(ref num2))
				{
					return;
				}
				if (lastCornerIsEnd && num2 <= num)
				{
					ResetPath();
					return;
				}
				if (!isSteering)
				{
					float num4 = _nextPositionNS.x - navVector.x;
					float num5 = _nextPositionNS.z - navVector.z;
					if (num4 * num4 + num5 * num5 <= 0.0625f)
					{
						cornersDirty = true;
					}
				}
				QueuePathFollowing(navVector, num2, num, isSteering, ref pendingRotation);
			}
		}
		finally
		{
			previousPositionNS = _nextPositionNS;
			if (pendingRotation.HasValue)
			{
				QueueRotationOverride(pendingRotation.Value);
			}
		}
	}

	public void SetStoppingDistanceOverride(float distance)
	{
		_stoppingDistanceOverride = distance;
	}

	public void ActivateCurrentOffMeshLink(bool activated)
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			agent.ActivateCurrentOffMeshLink(activated);
		}
	}

	public void CompleteOffMeshLink()
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			agent.CompleteOffMeshLink();
		}
	}

	public bool FindClosestEdge(out NavHit hitNS)
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			NavMeshHit unityHitNS = default;
			bool result = agent.FindClosestEdge(ref unityHitNS);
			hitNS = NavHit.FromUnity(in unityHitNS);
			return result;
		}
		hitNS = default;
		if (boundNavmesh != null)
		{
			ulong startRef = currentPolyRef;
			return boundNavmesh.FindDistanceToWall(ref startRef, _nextPositionNS, 10f, out hitNS, npcDoorsWillOpen);
		}
		return false;
	}

	public bool FindClosestEdge(NavVector3 positionNS, out NavHit hitNS)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			hitNS = default;
			if (!TryGetAgent(out var agent))
			{
				return false;
			}
			NavMeshHit unityHitNS = default;
			if (!NavMesh.FindClosestEdge(positionNS.Value, ref unityHitNS, agent.areaMask))
			{
				return false;
			}
			hitNS = NavHit.FromUnity(in unityHitNS);
			return true;
		}
		hitNS = default;
		ulong startRef = 0uL;
		if (boundNavmesh != null)
		{
			return boundNavmesh.FindDistanceToWall(ref startRef, positionNS, 10f, out hitNS, npcDoorsWillOpen);
		}
		return false;
	}

	public bool SampleConnectedPositions(float maxRadius, float minRadius, int count, List<NavVector3> resultsNS, float angleOffset = -1f)
	{
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return false;
		}
		if (angleOffset < 0f)
		{
			angleOffset = Rng.Range(0f, MathF.PI * 2f);
		}
		ulong startRef = currentPolyRef;
		return boundNavmesh.FindDonutPointsInCircle(ref startRef, _nextPositionNS, maxRadius, minRadius, angleOffset, count, resultsNS, npcDoorsWillOpen);
	}

	public bool SampleConnectedPositions(NavVector3 centerNS, float maxRadius, float minRadius, int count, List<NavVector3> resultsNS, float angleOffset = -1f)
	{
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return false;
		}
		if (angleOffset < 0f)
		{
			angleOffset = Rng.Range(0f, MathF.PI * 2f);
		}
		ulong startRef = 0uL;
		if (currentPolyRef != 0L && NavVector3.Distance(centerNS, _nextPositionNS) <= maxRadius)
		{
			startRef = currentPolyRef;
		}
		return boundNavmesh.FindDonutPointsInCircle(ref startRef, centerNS, maxRadius, minRadius, angleOffset, count, resultsNS, npcDoorsWillOpen);
	}

	public bool FindFloor(ulong startRef, float maxArea, ulong[] refs, out int count, uint[] tiles, out int tileCount, out float area, out Vector3 centreWS, out float radius, out bool open, out uint stamp)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		count = 0;
		tileCount = 0;
		area = 0f;
		centreWS = default;
		radius = 0f;
		open = false;
		stamp = 0u;
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return false;
		}
		ulong startRef2 = startRef;
		if (!boundNavmesh.FindFloor(ref startRef2, _nextPositionNS, maxArea, refs, out count, tiles, out tileCount, out area, out var boundsMinNS, out var boundsMaxNS, out open, out stamp, npcDoorsWillOpen))
		{
			return false;
		}
		centreWS = NavToWorldSpace((boundsMinNS + boundsMaxNS) * 0.5f);
		radius = NavVector3.Distance(boundsMinNS, boundsMaxNS) * 0.5f;
		return true;
	}

	public ulong WalkablePolyRef()
	{
		if (!SamplePositionPoly(_nextPositionNS, out var _, 2.5f, out var nearestPolyRef, debugDraw: false))
		{
			return 0uL;
		}
		return nearestPolyRef;
	}

	public bool FloorUnchangedSince(uint[] tiles, int count, uint stamp)
	{
		if (!AI.useUnityNavmesh && boundNavmesh != null)
		{
			return boundNavmesh.TileStampsUnchangedSince(tiles, count, stamp);
		}
		return false;
	}

	public void FloorBorder(ulong[] refs, int count, List<Vector3> segmentsWS)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return;
		}
		PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
		try
		{
			boundNavmesh.FloorBorder(refs, count, (List<NavVector3>)(object)val, npcDoorsWillOpen);
			foreach (NavVector3 item in (List<NavVector3>)(object)val)
			{
				segmentsWS.Add(NavToWorldSpace(item));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool SetDestination(Vector3 targetPositionWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return SetDestination(WorldToNavSpace(targetPositionWS));
	}

	public bool SetDestination(NavVector3 targetPositionNS)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SetDestination"))
		{
			StopArrival();
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				if (letUnityMoveAgentIfPossible)
				{
					return agent.SetDestination(targetPositionNS.Value);
				}
			}
			if (hasPath && CurPathNS.GetDestinationNS() == targetPositionNS)
			{
				return true;
			}
			if (TryPatchMovingDestination(targetPositionNS))
			{
				return true;
			}
			if (!CalculatePath(targetPositionNS, tempPath) || (int)tempPath.status != 0)
			{
				return false;
			}
			if (!SetPath(tempPath))
			{
				return false;
			}
			return true;
		}
	}

	private bool TryPatchMovingDestination(NavVector3 targetPositionNS)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh)
		{
			return false;
		}
		if (corridor == IntPtr.Zero || boundNavmesh == null)
		{
			return false;
		}
		if (Time.time - lastFullPlanTime >= 1f)
		{
			return false;
		}
		bool flag = followingPath;
		if (flag)
		{
			if ((int)CurPathNS.status != 0 || CurPathNS.corners.Count == 0)
			{
				return false;
			}
			if (NavVector3.Distance(CurPathNS.GetDestinationNS(), targetPositionNS) > 3f)
			{
				return false;
			}
		}
		else
		{
			ulong num = currentPolyRef;
			if (num == 0L)
			{
				return false;
			}
			if (NavVector3.Distance(_nextPositionNS, targetPositionNS) > 3f)
			{
				return false;
			}
			AnchorCorridor(num, _nextPositionNS);
		}
		if (!boundNavmesh.CorridorMoveTargetPosition(corridor, targetPositionNS, out var resultTargetNS, npcDoorsWillOpen))
		{
			return false;
		}
		NavVector3 navVector = resultTargetNS - targetPositionNS;
		if (Mathf.Abs(navVector.x) > 0.5f || Mathf.Abs(navVector.y) > 2f || Mathf.Abs(navVector.z) > 0.5f)
		{
			return false;
		}
		if (flag)
		{
			CurPathNS.corners[CurPathNS.corners.Count - 1] = resultTargetNS;
		}
		else
		{
			CurPathNS.corners.Clear();
			CurPathNS.corners.Add(_nextPositionNS);
			CurPathNS.corners.Add(resultTargetNS);
			CurPathNS.polyRefCount = 0;
			CurPathNS.status = (NavMeshPathStatus)0;
		}
		followingPath = true;
		cornersDirty = true;
		_isStopped = false;
		return true;
	}

	public void TryEnableInternalUnityAgent()
	{
		if (AI.useUnityNavmesh && ((Behaviour)this).enabled && TryGetAgent(out var agent))
		{
			TryEnableAgent(agent);
		}
	}

	private bool TryEnableAgent(NavMeshAgent Agent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)Agent).enabled)
		{
			return Agent.isOnNavMesh;
		}
		((Behaviour)Agent).enabled = true;
		if (!Agent.isOnNavMesh)
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError($"{baseEntity} is not on navmesh at {((Component)baseEntity).transform.position} in {MapHelper.PositionToString(((Component)baseEntity).transform.position)}");
			}
			return false;
		}
		return true;
	}

	public bool SetPath(RustNavMeshPath pathNS)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SetPath"))
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (pathNS.unityPath == null)
				{
					if (AI.logIssues)
					{
						RustNavigation.LogError("Trying to set a path on a RustNavMeshAgent with useUnityNavmesh enabled, but the provided path doesn't have a Unity NavMeshPath.");
					}
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				return agent.SetPath(pathNS.unityPath);
			}
			if ((int)pathNS.status == 2)
			{
				ResetPath();
				return false;
			}
			if (pathNS.polyRefCount == 0 || pathNS.polyRefs[0] != currentPolyRef)
			{
				ulong startRef = currentPolyRef;
				if (boundNavmesh == null || !boundNavmesh.CalculatePath(ref startRef, _nextPositionNS, pathNS.GetDestinationNS(), scratchPath, npcDoorsWillOpen) || (int)scratchPath.status != 0)
				{
					ResetPath();
					return false;
				}
				pathNS = scratchPath;
			}
			CurPathNS.CopyFrom(pathNS);
			IntPtr intPtr = corridor;
			ulong[] polyRefs = CurPathNS.polyRefs;
			int polyRefCount = CurPathNS.polyRefCount;
			NavVector3 destinationNS = CurPathNS.GetDestinationNS();
			RecastWrapper.CorridorSetPath(intPtr, polyRefs, polyRefCount, in destinationNS.Value);
			followingPath = true;
			cornersDirty = true;
			_isStopped = false;
			lastFullPlanTime = Time.time;
			lastValidPath.Clear();
			lastValidPath.AddRange(CurPathNS.corners);
			return true;
		}
	}

	private void AnchorCorridor(ulong polyRef, NavVector3 positionNS)
	{
		if (corridor != IntPtr.Zero)
		{
			RecastWrapper.CorridorReset(corridor, polyRef, in positionNS.Value);
		}
	}

	public void Move(NavVector3 deltaNS)
	{
		if (AI.useUnityNavmesh || !QueueDeltaMove(deltaNS))
		{
			MoveInternal(deltaNS, null);
		}
	}

	private bool TurnsWithMove(NavVector3 flatOffsetNS)
	{
		if (_updateRotation && !IsPaused && !overrideDirectionWS.HasValue)
		{
			return flatOffsetNS.Value.sqrMagnitude > 1E-06f;
		}
		return false;
	}

	private void MoveInternal(NavVector3 deltaNS, float? remainingDistanceHint)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Move"))
		{
			if (AI.useUnityNavmesh && letUnityMoveAgentIfPossible)
			{
				if (TryGetAgent(out var agent) && TryEnableAgent(agent))
				{
					agent.Move(deltaNS.Value);
				}
				return;
			}
			if (!letUnityMoveAgentIfPossible && canSteer && !IsPaused)
			{
				deltaNS = AdjustMovementForSteering(deltaNS, remainingDistanceHint);
			}
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent2) || !TryEnableAgent(agent2))
				{
					return;
				}
				agent2.Move(deltaNS.Value);
				_nextPositionNS = new NavVector3(agent2.nextPosition);
			}
			else
			{
				if (boundNavmesh == null || !boundNavmesh.IsValid())
				{
					TryBindNavmesh();
				}
				if (boundNavmesh != null && corridor != IntPtr.Zero)
				{
					if (HasStaleAnchor)
					{
						TryBindNavmesh();
						ResetPath();
						return;
					}
					if (boundNavmesh.CorridorMove(corridor, _nextPositionNS + deltaNS, out var resultPos, out var _, npcDoorsWillOpen))
					{
						_nextPositionNS = resultPos;
					}
					else if (followingPath)
					{
						ResetPath();
					}
				}
			}
			Nullable newRotation = null;
			bool flag = false;
			NavVector3 navVector = deltaNS.Flat();
			if (TurnsWithMove(navVector))
			{
				using (TimeWarning.New("Move.RotationWrite"))
				{
					Vector3 val = NavToWorldDirection(navVector);
					if (!letUnityMoveAgentIfPossible && canSteer)
					{
						newRotation = Quaternion.LookRotation(val);
						cachedSteeringForwardNS = navVector.NormalizeXZ();
						hasCachedSteeringForward = true;
					}
					else
					{
						newRotation = Quaternion.LookRotation(val);
						flag = true;
						hasCachedSteeringForward = false;
					}
				}
			}
			else
			{
				hasCachedSteeringForward = false;
			}
			if (flag)
			{
				newRotation = Quaternion.RotateTowards(AgentTransform.rotation, newRotation.Value, _angularSpeed * Time.deltaTime);
			}
			SyncWorldPosWithNavPos(newRotation);
			TryOpenDoors();
		}
	}

	private Vector3 GetMovementWorldPosition()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!canSwim)
		{
			return NavToWorldSpace(_nextPositionNS + NavVector3.up * _baseOffset);
		}
		return CalculateSwimmingWorldPosition();
	}

	private bool TrySyncWorldPosWithNavPos()
	{
		return SyncWorldPosWithNavPos(null);
	}

	private bool SyncWorldPosWithNavPos(Nullable newRotation)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.TrySyncWorldPosWithNavPos"))
		{
			LandOwnUpdate();
			if (!_updatePosition)
			{
				if (newRotation.HasValue)
				{
					AgentTransform.rotation = newRotation.Value;
				}
			}
			else if (newRotation.HasValue)
			{
				AgentTransform.SetPositionAndRotation(GetMovementWorldPosition(), newRotation.Value);
			}
			else
			{
				AgentTransform.position = GetMovementWorldPosition();
			}
			return _updatePosition;
		}
	}

	public bool Warp(Vector3 newPositionWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return WarpToWorldPosition(newPositionWS);
	}

	public bool Warp(NavVector3 newPositionNS)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Warp"))
		{
			LandOwnUpdate();
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				return agent.Warp(newPositionNS.Value);
			}
			if (!SamplePositionPoly(newPositionNS, out var hitNS, 1f, out var nearestPolyRef))
			{
				return false;
			}
			_nextPositionNS = hitNS.position;
			previousPositionNS = _nextPositionNS;
			AnchorCorridor(nearestPolyRef, _nextPositionNS);
			ResetPath();
			TrySyncWorldPosWithNavPos();
			return true;
		}
	}

	public bool WarpToWorldPosition(Vector3 newPositionWS)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LandOwnUpdate();
		if (!AI.useUnityNavmesh)
		{
			BindNavmeshAt(newPositionWS);
		}
		return Warp(WorldToNavSpace(newPositionWS));
	}

	private void EnsureAnchorValid()
	{
		if (AI.useUnityNavmesh || corridor == IntPtr.Zero)
		{
			return;
		}
		if (boundNavmesh == null || !boundNavmesh.IsValid())
		{
			TryBindNavmesh();
			return;
		}
		ulong num = currentPolyRef;
		if (num == 0L || !boundNavmesh.IsValidPolyRef(num))
		{
			if (SamplePositionPoly(_nextPositionNS, out var hitNS, 2.5f, out var nearestPolyRef, debugDraw: false, includeGatedDoorPolys: true) && nearestPolyRef != 0L)
			{
				_nextPositionNS = hitNS.position;
				AnchorCorridor(nearestPolyRef, _nextPositionNS);
				previousPositionNS = _nextPositionNS;
				ResetPath();
			}
			else
			{
				TryBindNavmesh();
				ResetPath();
			}
			if (currentPolyRef != 0L && !IsPaused)
			{
				TrySyncWorldPosWithNavPos();
			}
		}
	}

	private void EnsureAnchorValidAfterTileRebuild()
	{
		if (!AI.useUnityNavmesh && boundNavmesh != null && anchorCheckedTileDataVersion != boundNavmesh.TileDataVersion && !IsPaused)
		{
			anchorCheckedTileDataVersion = boundNavmesh.TileDataVersion;
			if (currentPolyRef != 0L)
			{
				EnsureAnchorValid();
			}
		}
	}

	public bool CalculatePath(Vector3 targetPositionWS, RustNavMeshPath pathNS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return CalculatePath(WorldToNavSpace(targetPositionWS), pathNS);
	}

	public bool CalculatePath(NavVector3 targetPositionNS, RustNavMeshPath pathNS)
	{
		EnsureAnchorValid();
		ulong startRef = currentPolyRef;
		return CalculatePath(ref startRef, _nextPositionNS, targetPositionNS, pathNS);
	}

	public bool Raycast(Vector3 targetPositionWS, out NavMeshHit hitWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		hitWS = default;
		if (!Raycast(WorldToNavSpace(targetPositionWS), out var hitNS))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool Raycast(NavVector3 targetPositionNS, out NavHit hitNS)
	{
		ulong startRef = currentPolyRef;
		return Raycast(ref startRef, _nextPositionNS, targetPositionNS, out hitNS);
	}

	public bool IsPositionOnNavmesh(Vector3 positionWS, out NavMeshHit hitWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		hitWS = default;
		if (!IsPositionOnNavmesh(WorldToNavSpace(positionWS), out var hitNS))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool IsPositionOnNavmesh(NavVector3 positionNS, out NavHit hitNS, float maxDistance = 2f)
	{
		return SamplePosition(positionNS, out hitNS, maxDistance);
	}

	public void ResetPath()
	{
		using (TimeWarning.New("RustNavMeshAgent.ResetPath"))
		{
			LandOwnUpdate();
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && ((Behaviour)agent).enabled && agent.isOnNavMesh)
			{
				agent.ResetPath();
			}
			StopArrival();
			CurPathNS.Reset();
			followingPath = false;
			corners.Clear();
			cornersDirty = true;
			lastCornerIsEnd = false;
			AnchorCorridor(currentPolyRef, _nextPositionNS);
			autoBraking = true;
			_desiredSpeed = 0f;
			if (!letUnityMoveAgentIfPossible)
			{
				_acceleration.Reset();
				deceleration.Reset();
			}
			currentDeviation = 0f;
			_stoppingDistanceOverride = -1f;
		}
	}

	public bool TryGetArrival(out Vector3 standWS, out Vector3 facingWS)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		standWS = (hasArrival ? NavToWorldSpace(arrivalStandNS) : default(Vector3));
		facingWS = (hasArrival ? NavToWorldDirection(arrivalForwardNS) : default(Vector3));
		return hasArrival;
	}

	public float ClearLaneBehind(Vector3 standWS, Vector3 facingWS, float maxLength)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 navVector = WorldToNavSpace(standWS);
		NavVector3 navVector2 = WorldToNavDirection(-facingWS).NormalizeXZ();
		if (!Raycast(navVector, navVector + navVector2 * maxLength, out var hitNS))
		{
			if (!float.IsNaN(hitNS.position.x))
			{
				return maxLength;
			}
			return 0f;
		}
		return NavVector3.DistanceXZ(navVector, hitNS.position);
	}

	public bool IsDockedAt(Vector3 standWS, Vector3 facingWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return IsDockedAt(WorldToNavSpace(standWS), WorldToNavDirection(facingWS).NormalizeXZ());
	}

	public bool SetDestinationFacing(Vector3 standWS, Vector3 facingWS, float laneLength, Speeds? gait = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 navVector = WorldToNavSpace(standWS);
		NavVector3 navVector2 = WorldToNavDirection(facingWS).NormalizeXZ();
		if (!CanArriveFacing)
		{
			return SetDestinationWithParams(navVector, autoBraking: true, gait);
		}
		if (hasArrival && NavVector3.DistanceXZ(navVector, arrivalStandNS) < 0.05f && NavVector3.Dot(navVector2, arrivalForwardNS) > 0.999f)
		{
			return true;
		}
		if (IsDockedAt(navVector, navVector2))
		{
			ResetPath();
			LastArrival = ArrivalOutcome.Docked;
			return true;
		}
		ResetPath();
		arrivalStandNS = navVector;
		arrivalForwardNS = navVector2;
		arrivalLaneLength = Mathf.Max(0f, laneLength);
		curveReplansLeft = 2;
		if (IsWithinCurvePlanningRange() && TryPlanCurve(allowTurnOnTheSpot: true, out var best))
		{
			BeginCurve(in best);
			return true;
		}
		NavVector3 targetPositionNS = navVector - navVector2 * arrivalLaneLength;
		float? deviation = 0f;
		if (!SetDestinationWithParams(targetPositionNS, autoBraking: true, gait, null, null, deviation))
		{
			LastArrival = ArrivalOutcome.Aborted;
			return false;
		}
		nextCurvePlan = 0f;
		LastArrival = ArrivalOutcome.Docking;
		return true;
	}

	private void StopArrival()
	{
		followingCurve = false;
		if (hasArrival)
		{
			LastArrival = ArrivalOutcome.Aborted;
		}
	}

	private bool IsDockedAt(NavVector3 standNS, NavVector3 forwardNS)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 navVector = (standNS - _nextPositionNS).Flat();
		float num = NavVector3.Dot(navVector, forwardNS);
		float magnitude = (navVector - forwardNS * num).magnitude;
		if (Mathf.Abs(num) <= 0.3f && magnitude <= 0.3f)
		{
			return Vector3.Angle(Heading.Value, forwardNS.Value) <= 25f;
		}
		return false;
	}

	private bool IsWithinCurvePlanningRange()
	{
		return NavVector3.DistanceXZ(_nextPositionNS, arrivalStandNS) <= arrivalLaneLength + 5f;
	}

	private bool ApplyArrival(ref float remaining)
	{
		bool flag = lastCornerIsEnd && remaining <= RestingDistance(currentStoppingDistance) + 0.5f;
		if ((flag || Time.time >= nextCurvePlan) && IsWithinCurvePlanningRange())
		{
			nextCurvePlan = Time.time + 0.25f;
			if (TryPlanCurve(flag, out var best))
			{
				BeginCurve(in best);
				return false;
			}
		}
		if (flag)
		{
			ResetPath();
			return false;
		}
		remaining += arrivalLaneLength;
		return true;
	}

	private bool TryPlanCurve(bool allowTurnOnTheSpot, out PoseCurve best)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		best = default;
		Vector2 val = new Vector2(_nextPositionNS.x, _nextPositionNS.z);
		float num = PoseCurve.HeadingOf(new Vector2(Heading.x, Heading.z));
		Vector2 val2 = new Vector2(arrivalStandNS.x, arrivalStandNS.z);
		Vector2 val3 = new Vector2(arrivalForwardNS.x, arrivalForwardNS.z);
		float num2 = PoseCurve.HeadingOf(val3);
		PooledList<PoseCurve> val4 = Pool.Get<PooledList<PoseCurve>>();
		try
		{
			float[] curveTurnRadiusScales = CurveTurnRadiusScales;
			foreach (float num3 in curveTurnRadiusScales)
			{
				if (TryShortestClearCurve(val, num, val2, val3, num2, maxTurnRadius * num3, alongTheLane: true, float.NegativeInfinity, 4.712389f, val4, out best))
				{
					return true;
				}
			}
			curveTurnRadiusScales = CurveTurnRadiusScales;
			foreach (float num4 in curveTurnRadiusScales)
			{
				if (TryShortestClearCurve(val, num, val2, val3, num2, maxTurnRadius * num4, alongTheLane: false, float.NegativeInfinity, MathF.PI / 2f, val4, out best))
				{
					return true;
				}
			}
			for (int num5 = CurveTurnRadiusScales.Length - 1; num5 >= 0; num5--)
			{
				if (TryShortestClearCurve(val, num, val2, val3, num2, maxTurnRadius * CurveTurnRadiusScales[num5], alongTheLane: true, 4.712389f, float.PositiveInfinity, val4, out best))
				{
					return true;
				}
			}
			if (!allowTurnOnTheSpot)
			{
				return false;
			}
			float num6 = Mathf.Min(arrivalLaneLength, 1f);
			PoseCurve candidate = PoseCurve.TurnWalkTurn(val, num, val2 - val3 * num6, num2);
			candidate.Append(PoseCurve.Piece.Straight, num6);
			if (!IsClearOnNavmesh(in candidate))
			{
				return false;
			}
			best = candidate;
			return true;
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private bool TryShortestClearCurve(Vector2 from, float heading, Vector2 stand, Vector2 forward, float forwardHeading, float radius, bool alongTheLane, float turningMoreThan, float turningAtMost, PooledList<PoseCurve> candidates, out PoseCurve best)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		best = default;
		((List<PoseCurve>)(object)candidates).Clear();
		if (alongTheLane)
		{
			AddLaneCurves(from, heading, stand, forward, forwardHeading, radius, candidates);
		}
		else
		{
			AddLaneCurve(from, heading, stand, forward, forwardHeading, radius, 0f, candidates);
		}
		SortByLength(candidates);
		foreach (PoseCurve item in (List<PoseCurve>)(object)candidates)
		{
			PoseCurve candidate = item;
			if (!(candidate.Turning <= turningMoreThan) && !(candidate.Turning > turningAtMost) && IsClearOnNavmesh(in candidate))
			{
				best = candidate;
				return true;
			}
		}
		return false;
	}

	private void AddLaneCurves(Vector2 from, float heading, Vector2 stand, Vector2 forward, float forwardHeading, float radius, PooledList<PoseCurve> candidates)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		AddLaneCurve(from, heading, stand, forward, forwardHeading, radius, arrivalLaneLength, candidates);
		float num = Mathf.Min(arrivalLaneLength, 1f);
		if (num < arrivalLaneLength - 0.01f)
		{
			AddLaneCurve(from, heading, stand, forward, forwardHeading, radius, num, candidates);
		}
	}

	private static void AddLaneCurve(Vector2 from, float heading, Vector2 stand, Vector2 forward, float forwardHeading, float radius, float straight, PooledList<PoseCurve> candidates)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		int count = ((List<PoseCurve>)(object)candidates).Count;
		PoseCurve.AddDubins(from, heading, stand - forward * straight, forwardHeading, radius, candidates);
		for (int i = count; i < ((List<PoseCurve>)(object)candidates).Count; i++)
		{
			PoseCurve value = ((List<PoseCurve>)(object)candidates)[i];
			value.Append(PoseCurve.Piece.Straight, straight);
			((List<PoseCurve>)(object)candidates)[i] = value;
		}
	}

	private static void SortByLength(PooledList<PoseCurve> candidates)
	{
		for (int i = 1; i < ((List<PoseCurve>)(object)candidates).Count; i++)
		{
			PoseCurve value = ((List<PoseCurve>)(object)candidates)[i];
			float length = value.Length;
			int num = i - 1;
			while (num >= 0 && ((List<PoseCurve>)(object)candidates)[num].Length > length)
			{
				((List<PoseCurve>)(object)candidates)[num + 1] = ((List<PoseCurve>)(object)candidates)[num];
				num--;
			}
			((List<PoseCurve>)(object)candidates)[num + 1] = value;
		}
	}

	private bool IsClearOnNavmesh(in PoseCurve candidate)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 navVector = _nextPositionNS;
		ulong startRef = currentPolyRef;
		float length = candidate.Length;
		float num = 0.3f;
		while (true)
		{
			bool flag = num >= length;
			candidate.Sample(flag ? length : num, out var position, out var _);
			NavVector3 navVector2 = new NavVector3(new Vector3(position.x, navVector.y, position.y));
			if (NavVector3.DistanceXZ(navVector, navVector2) > 0.001f)
			{
				if (!SamplePositionPoly(navVector2, out var hitNS, 1f, out var nearestPolyRef, debugDraw: false) || NavVector3.DistanceXZ(hitNS.position, navVector2) > 0.05f)
				{
					return false;
				}
				if (Raycast(ref startRef, navVector, hitNS.position, out var _))
				{
					return false;
				}
				navVector = hitNS.position;
				startRef = nearestPolyRef;
			}
			if (flag)
			{
				break;
			}
			num += 0.3f;
		}
		return true;
	}

	private void BeginCurve(in PoseCurve planned)
	{
		float num = currentSpeed;
		ResetPath();
		followingCurve = true;
		curve = planned;
		curveDistance = 0f;
		curveSpeed = num;
		LastArrival = ArrivalOutcome.Docking;
	}

	private void TickCurve(float deltaTime)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		float length = curve.Length;
		float speedForGait = GetSpeedForGait(PenalisedGait(Speeds.Walk));
		float num = Mathf.Sqrt(3f * Mathf.Max(0f, length - curveDistance));
		curveSpeed = Mathf.MoveTowards(curveSpeed, Mathf.Min(speedForGait, num), 1.5f * deltaTime);
		curveDistance = Mathf.Min(length, curveDistance + Mathf.Max(curveSpeed, 0.15f) * deltaTime);
		curve.Sample(curveDistance, out var position, out var heading);
		NavVector3 navVector = new NavVector3(new Vector3(position.x, _nextPositionNS.y, position.y));
		Vector2 val = PoseCurve.Direction(heading);
		NavVector3 forwardNS = new NavVector3(new Vector3(val.x, 0f, val.y));
		if (!MoveAlongCurve(navVector, forwardNS, deltaTime))
		{
			return;
		}
		if (NavVector3.DistanceXZ(_nextPositionNS, navVector) > 0.3f)
		{
			if (curveReplansLeft > 0 && TryPlanCurve(allowTurnOnTheSpot: true, out var best))
			{
				curveReplansLeft--;
				BeginCurve(in best);
			}
			else
			{
				ResetPath();
			}
		}
		else if (!(curveDistance < length))
		{
			LastArrival = ArrivalOutcome.Docked;
			ResetPath();
			MoveAlongCurve(_nextPositionNS, arrivalForwardNS, deltaTime);
		}
	}

	private bool MoveAlongCurve(NavVector3 wantedNS, NavVector3 forwardNS, float deltaTime)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		NavVector3 nextPositionNS = _nextPositionNS;
		if (boundNavmesh == null || !boundNavmesh.IsValid() || corridor == IntPtr.Zero || !boundNavmesh.IsValidPolyRef(currentPolyRef))
		{
			TryBindNavmesh();
			ResetPath();
			return false;
		}
		if (!boundNavmesh.CorridorMove(corridor, wantedNS, out var resultPos, out var _, npcDoorsWillOpen))
		{
			ResetPath();
			return false;
		}
		_nextPositionNS = resultPos;
		_velocityNS = (_nextPositionNS - nextPositionNS) / Mathf.Max(deltaTime, 0.0001f);
		currentSpeed = (followingCurve ? _velocityNS.Flat().magnitude : 0f);
		Nullable newRotation = null;
		if (_updateRotation && !overrideDirectionWS.HasValue)
		{
			newRotation = Quaternion.LookRotation(NavToWorldDirection(forwardNS));
			cachedSteeringForwardNS = forwardNS;
			hasCachedSteeringForward = true;
		}
		SyncWorldPosWithNavPos(newRotation);
		return true;
	}

	public bool TryOpenDoors()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!canOpenDoors)
		{
			return false;
		}
		using (TimeWarning.New("RustNavMeshAgent.TryOpenDoors"))
		{
			bool flag = false;
			PooledList<NPCDoorTriggerBox> val = Pool.Get<PooledList<NPCDoorTriggerBox>>();
			try
			{
				NPCDoorTriggerBox.AllDoors.GetNeighboors(((Component)baseEntity).transform.position, (List<NPCDoorTriggerBox>)(object)val);
				foreach (NPCDoorTriggerBox item in (List<NPCDoorTriggerBox>)(object)val)
				{
					flag |= item.TryOpenDoorFor(baseEntity);
				}
				return flag;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private PathFollowingSpeed GetPathFollowingSpeed(bool autoBraking, float remaining, float stopping)
	{
		return new PathFollowingSpeed
		{
			current = currentSpeed,
			desired = AdjustDesiredSpeedWhenSwimming(_desiredSpeed, sprintSpeed),
			acceleration = _acceleration.Value,
			deceleration = deceleration.Value,
			emergencyDeceleration = emergencyDeceleration,
			walkSpeed = walkSpeed,
			brakeFloor = Mathf.Min(1f, GetSpeedForGait(minimumGait) * 0.5f),
			slowDownRadius = (letUnityMoveAgentIfPossible ? (2f * agentTypeRadius) : 0f),
			remaining = remaining,
			stopping = stopping,
			unityBraking = (byte)(letUnityMoveAgentIfPossible ? 1 : 0),
			autoBraking = (byte)(autoBraking ? 1 : 0)
		};
	}

	private void QueuePathFollowing(NavVector3 waypoint, float remaining, float stopping, bool steering, ref Nullable pendingRotation)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		bool onMovingNavmesh = OnMovingNavmesh;
		byte b = 8;
		if (_updatePosition)
		{
			b |= 1;
		}
		if (_updateRotation)
		{
			b |= 2;
		}
		if (canSwim)
		{
			b |= 0x10;
		}
		byte b2 = 0;
		if (steering)
		{
			b2 |= 1;
		}
		if (lastCornerIsEnd && corners.Count == 1)
		{
			b2 |= 2;
		}
		if (_autoBraking)
		{
			b2 |= 4;
		}
		if (hasCachedSteeringForward | onMovingNavmesh)
		{
			b2 |= 8;
		}
		if (steering)
		{
			b2 |= 0x10;
		}
		if (npcDoorsWillOpen)
		{
			b2 |= 0x20;
		}
		if (steering && keepBodyOffWalls && RecastWrapper.WallsAroundAvailable && GatherBodyWalls())
		{
			b2 |= 0x40;
		}
		NavVector3 navVector = cachedSteeringForwardNS;
		Quaternion val = Quaternion.identity;
		if (pendingRotation.HasValue)
		{
			val = pendingRotation.Value;
			pendingRotation = null;
			b = (byte)(b | (overrideDirectionWS.HasValue ? 64 : 32));
			if (steering)
			{
				b2 |= 8;
				navVector = WorldToNavDirection(val * Vector3.forward);
			}
		}
		else if ((steering & onMovingNavmesh) && !hasCachedSteeringForward)
		{
			navVector = forward;
		}
		MovementTransformUpdate update = new MovementTransformUpdate
		{
			flags = b,
			rotation = val,
			maxDegreesDelta = _angularSpeed * Time.deltaTime,
			space = (ushort)(onMovingNavmesh ? ushort.MaxValue : 0),
			path = new PathFollowingUpdate
			{
				navmesh = boundNavmesh.NavMeshHandle,
				corridor = corridor,
				position = _nextPositionNS.Value,
				waypoint = waypoint.Value,
				destination = CurPathNS.GetDestinationNS().Value,
				optimizeTarget = (steering ? corners[Mathf.Min(1, corners.Count - 1)].Value : default(Vector3)),
				forward = navVector.Value,
				deviation = currentDeviation,
				turnRadius = maxTurnRadius,
				baseOffset = _baseOffset,
				moved = -1,
				flags = b2
			}
		};
		if (canSwim)
		{
			TakeWaterSample(ref update);
		}
		update.path.speed = GetPathFollowingSpeed(_autoBraking, remaining, stopping);
		EnqueueMovementUpdate(in update);
	}

	private static void EnsureMovementQueries(int needed)
	{
		if (movementQueries == null)
		{
			movementQueries = new IntPtr[JobsUtility.ThreadIndexCount];
		}
		try
		{
			for (int i = 0; i < needed; i++)
			{
				if (!(movementQueries[i] != IntPtr.Zero))
				{
					movementQueries[i] = RecastWrapper.CreateMovementQuery();
					if (movementQueries[i] == IntPtr.Zero)
					{
						throw new OutOfMemoryException("Could not allocate movement query scratch storage.");
					}
				}
			}
		}
		catch
		{
			DisposeMovementQueries();
			throw;
		}
	}

	private static void DisposeMovementQueries()
	{
		if (movementQueries == null)
		{
			return;
		}
		IntPtr[] array = movementQueries;
		foreach (IntPtr intPtr in array)
		{
			if (intPtr != IntPtr.Zero)
			{
				RecastWrapper.FreeMovementQuery(intPtr);
			}
		}
		movementQueries = null;
	}

	private unsafe static void CalculatePathFollowing(ref MovementTransformUpdate update, IntPtr query, BodyWalls* body, Quaternion currentRotation, float dt)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		ref PathFollowingUpdate path = ref update.path;
		path.speed.current = path.speed.Advance(dt, out path.adjustment);
		Vector3 val = path.waypoint - path.position;
		val.y = 0f;
		float magnitude = val.magnitude;
		float num = path.speed.current * dt;
		if ((path.flags & 2) != 0)
		{
			num = Mathf.Clamp(num, 0f - magnitude, magnitude);
		}
		Vector3 delta = ((magnitude > 1E-05f) ? (val * (num / magnitude)) : Vector3.zero);
		path.velocity = delta / dt;
		bool flag = (path.flags & 1) != 0;
		bool flag2 = false;
		if (flag)
		{
			if (path.deviation != 0f && Mathf.Abs(path.speed.remaining - Vector3.Distance(path.position, path.destination)) < 5f)
			{
				delta = float3.op_Implicit(math.mul(quaternion.AxisAngle(math.up(), path.deviation * (MathF.PI / 180f)), float3.op_Implicit(delta)));
			}
			float num2 = SteeringTurnRadius(path.speed.remaining, path.turnRadius, (path.flags & 4) != 0);
			Vector3 positionNS = (((path.flags & 8) != 0) ? path.forward : (currentRotation * Vector3.forward));
			delta = RotateTowardsFlat(new NavVector3(positionNS), new NavVector3(delta), path.speed.current / num2 * dt).Value * delta.magnitude;
			if ((path.flags & 0x40) != 0 && body != null)
			{
				flag2 = KeepBodyOffWalls(body, ref path, ref delta, positionNS, val, dt);
			}
		}
		MoveCorridor(ref update, query, path.position + delta);
		delta.y = 0f;
		if ((update.flags & 0x40) != 0)
		{
			return;
		}
		if ((update.flags & 2) != 0 && (delta.sqrMagnitude > 1E-06f || (flag2 && delta.sqrMagnitude > 0f)))
		{
			update.rotation = quaternion.op_Implicit(quaternion.LookRotationSafe(float3.op_Implicit(delta), math.up()));
			if (flag)
			{
				path.forward = delta.normalized;
			}
			else
			{
				update.flags |= 4;
			}
		}
		else if ((update.flags & 0x20) != 0)
		{
			update.flags |= 64;
			update.flags &= 251;
		}
		else
		{
			update.flags &= 253;
		}
	}

	private static void CalculateDeltaMove(ref MovementTransformUpdate update, IntPtr query)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		MoveCorridor(ref update, query, update.path.waypoint);
	}

	private unsafe static void MoveCorridor(ref MovementTransformUpdate update, IntPtr query, Vector3 target)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		ref PathFollowingUpdate path = ref update.path;
		Vector3 val = target;
		Vector3 position = path.position;
		Vector3 optimizeTarget = path.optimizeTarget;
		path.moved = RecastWrapper.CorridorMoveWithQuery(path.navmesh, path.corridor, query, &val, ((path.flags & 0x10) != 0) ? (&optimizeTarget) : null, 20f, &position, ((path.flags & 0x20) != 0) ? 1 : 0);
		if (path.moved != 0)
		{
			path.position = position;
		}
	}

	private void ApplyPathFollowingResult(in MovementTransformUpdate update)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		currentSpeed = update.path.speed.current;
		_nextPositionNS = new NavVector3(update.path.position);
		previousPositionNS = _nextPositionNS;
		_velocityNS = new NavVector3(update.path.velocity);
		hasCachedSteeringForward = (update.path.flags & 1) != 0 && (update.flags & 2) != 0 && (update.flags & 0x40) == 0;
		if (hasCachedSteeringForward)
		{
			cachedSteeringForwardNS = new NavVector3(update.path.forward);
		}
		if (update.path.moved == 0)
		{
			ResetPath();
		}
	}

	public bool Pause(object source)
	{
		bool wasPaused = pausingSources.Count > 0;
		bool result = pausingSources.Add(source);
		OnChange(wasPaused);
		return result;
	}

	public bool Unpause(object source)
	{
		bool wasPaused = pausingSources.Count > 0;
		bool result = pausingSources.Remove(source);
		OnChange(wasPaused);
		return result;
	}

	private void OnChange(bool wasPaused)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (!wasPaused && pausingSources.Count > 0)
		{
			ResetPath();
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				((Behaviour)agent).enabled = false;
			}
		}
		else
		{
			if (!wasPaused || pausingSources.Count != 0)
			{
				return;
			}
			if (!placedWhilePaused && StandsOnItsNavPosition())
			{
				EnsureAnchorValid();
				return;
			}
			placedWhilePaused = false;
			LandOwnUpdate();
			Vector3 position = ((Component)this).transform.position;
			if (IsSwimming)
			{
				position.y = WaterLevel.GetWaterInfo(position, waves: false, volumes: false).terrainHeight;
			}
			bool flag = Warp(WorldToNavSpace(position));
			if (AI.logIssues && !flag)
			{
				Debug.LogError((object)$"Failed to reproject {((Object)baseEntity).name} to current position {((Component)this).transform.position} after unpausing.", (Object)(object)this);
			}
		}
	}

	private static float GetBrakingDistance(float speed, float brakingDeceleration)
	{
		float num = speed / Mathf.Max(brakingDeceleration, 0.001f);
		return 0.5f * brakingDeceleration * num * num;
	}

	public float GetSpeedForGait(Speeds gait)
	{
		return gait switch
		{
			Speeds.Sneak => sneakSpeed, 
			Speeds.Walk => walkSpeed, 
			Speeds.Jog => jogSpeed, 
			Speeds.Run => runSpeed, 
			Speeds.Sprint => sprintSpeed, 
			Speeds.FullSprint => fullSprintSpeed, 
			_ => walkSpeed, 
		};
	}

	public Speeds PenalisedGait(Speeds gait)
	{
		return (Speeds)Mathf.Clamp((int)(gait - gaitPenalty), (int)minimumGait, 5);
	}

	public void SetGait(Speeds gait)
	{
		speed = GetSpeedForGait(PenalisedGait(gait));
	}

	public void SetSpeedRatio(float ratio, Speeds minSpeed = Speeds.Sneak, Speeds maxSpeed = Speeds.Sprint, int offset = 0)
	{
		if (maxSpeed < minSpeed)
		{
			minSpeed = maxSpeed;
		}
		int num = Mathf.FloorToInt(Mathf.Lerp((float)minSpeed, (float)maxSpeed, ratio));
		num = Mathf.Clamp(num + offset, (int)minSpeed, (int)maxSpeed);
		SetGait((Speeds)num);
	}

	public void AdjustCurrentSpeedFromDesiredSpeed(NavVector3 position, bool shouldStopAtDestination, float remainingDistance, float stoppingDistance, float dt)
	{
		using (TimeWarning.New("Tick.SpeedAdjust"))
		{
			currentSpeed = GetPathFollowingSpeed(shouldStopAtDestination, remainingDistance, stoppingDistance).Advance(dt, out var _);
		}
	}

	public float RestingDistance(float stoppingDistance)
	{
		if (!IsSteering)
		{
			return stoppingDistance;
		}
		return Mathf.Max(maxTurnRadius, stoppingDistance);
	}

	private static float SteeringTurnRadius(float remaining, float turnRadius, bool braking)
	{
		if (!braking)
		{
			return turnRadius;
		}
		return Mathx.RemapValClamped(remaining, turnRadius * 2f, 0f, turnRadius, 0.001f);
	}

	public bool TryGetRestingPoint(float restingDistance, out Vector3 pointWS, out Vector3 headingWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		pointWS = default;
		headingWS = default;
		if (!followingPath || cornersDirty || !lastCornerIsEnd || corners.Count == 0)
		{
			return false;
		}
		float num = restingDistance;
		NavVector3 navVector = corners[corners.Count - 1];
		for (int num2 = corners.Count - 1; num2 >= 0; num2--)
		{
			NavVector3 navVector2 = ((num2 > 0) ? corners[num2 - 1] : _nextPositionNS);
			float num3 = NavVector3.Distance(navVector2, navVector);
			if (num3 >= num)
			{
				float num4 = ((num3 > 0.0001f) ? (num / num3) : 0f);
				pointWS = NavToWorldSpace(navVector + (navVector2 - navVector) * num4);
				headingWS = NavToWorldDirection(navVector - navVector2);
				return true;
			}
			num -= num3;
			navVector = navVector2;
		}
		return false;
	}

	public bool TryPredictTurningRest(Vector3 fromWS, Vector3 forwardWS, Vector3 destinationWS, float stoppingDistance, bool braking, out Vector3 restWS, out Vector3 headingWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		restWS = default;
		headingWS = default;
		if (!IsSteering || !TrySteerToRest(WorldToNavSpace(fromWS).Value, WorldToNavDirection(forwardWS).Value, WorldToNavSpace(destinationWS).Value, RestingDistance(stoppingDistance), braking, out var rest, out var heading))
		{
			return false;
		}
		restWS = NavToWorldSpace(new NavVector3(rest));
		headingWS = NavToWorldDirection(new NavVector3(heading));
		return true;
	}

	public bool TryGetTurningRestingPoint(float restingDistance, out Vector3 pointWS, out Vector3 headingWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		pointWS = default;
		headingWS = default;
		if (!IsSteering || !followingPath)
		{
			return false;
		}
		NavVector3 navVector;
		if (!cornersDirty && lastCornerIsEnd && corners.Count == 1)
		{
			navVector = corners[0];
		}
		else
		{
			if (!cornersDirty || CurPathNS.corners.Count != 2)
			{
				return false;
			}
			navVector = CurPathNS.corners[1];
		}
		if (!TrySteerToRest(_nextPositionNS.Value, SteeringForwardNS.Value, navVector.Value, restingDistance, _autoBraking, out var rest, out var heading))
		{
			return false;
		}
		pointWS = NavToWorldSpace(new NavVector3(rest));
		headingWS = NavToWorldDirection(new NavVector3(heading));
		return true;
	}

	private bool TrySteerToRest(Vector3 from, Vector3 forward, Vector3 to, float restingDistance, bool braking, out Vector3 rest, out Vector3 heading)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = from;
		Vector3 val2 = Vector3Ex.WithY(forward, 0f);
		rest = default;
		heading = default;
		for (int i = 0; i < 64; i++)
		{
			Vector3 val3 = Vector3Ex.WithY(to - val, 0f);
			float magnitude = val3.magnitude;
			if (magnitude <= restingDistance)
			{
				rest = Vector3Ex.WithY(val, to.y);
				heading = val2;
				return i > 0;
			}
			Vector3 val4 = val3 / magnitude;
			float num = 0.25f / SteeringTurnRadius(magnitude, maxTurnRadius, braking);
			if (val2.sqrMagnitude < 0.0001f || Vector3.Dot(val2.normalized, val4) >= Mathf.Cos(num))
			{
				rest = to - val4 * restingDistance;
				heading = val4;
				return i > 0;
			}
			val2 = RotateTowardsFlat(new NavVector3(val2), new NavVector3(val4), num).Value;
			val += val2 * 0.25f;
		}
		return false;
	}

	public NavVector3 AdjustMovementForSteering(NavVector3 deltaNS)
	{
		return AdjustMovementForSteering(deltaNS, null);
	}

	public NavVector3 AdjustMovementForSteering(NavVector3 deltaNS, float? remainingDistanceHint)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AdjustMovementForSteering"))
		{
			NavVector3 targetNS = deltaNS;
			float num = remainingDistanceHint ?? remainingDistance;
			if (currentDeviation != 0f && Mathf.Abs(num - NavVector3.Distance(_nextPositionNS, CurPathNS.GetDestinationNS())) < 5f)
			{
				targetNS = Quaternion.AngleAxis(currentDeviation, Vector3.up) * deltaNS;
			}
			float num2 = SteeringTurnRadius(num, maxTurnRadius, _autoBraking);
			float num3 = currentSpeed / num2;
			return RotateTowardsFlat(SteeringForwardNS, targetNS, num3 * Time.deltaTime) * targetNS.magnitude;
		}
	}

	private static NavVector3 RotateTowardsFlat(NavVector3 currentNS, NavVector3 targetNS, float maxRadiansDelta)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		float x = currentNS.x;
		float z = currentNS.z;
		float x2 = targetNS.x;
		float z2 = targetNS.z;
		float num = Mathf.Sqrt(x * x + z * z);
		float num2 = Mathf.Sqrt(x2 * x2 + z2 * z2);
		if (num < 1E-06f || num2 < 1E-06f)
		{
			return targetNS.NormalizeXZ();
		}
		x /= num;
		z /= num;
		x2 /= num2;
		z2 /= num2;
		if (x * x2 + z * z2 >= Mathf.Cos(maxRadiansDelta))
		{
			return new NavVector3(new Vector3(x2, 0f, z2));
		}
		float num3 = ((x * z2 - z * x2 >= 0f) ? maxRadiansDelta : (0f - maxRadiansDelta));
		float num4 = Mathf.Cos(num3);
		float num5 = Mathf.Sin(num3);
		return new NavVector3(new Vector3(x * num4 - z * num5, 0f, x * num5 + z * num4));
	}

	public Vector3 CalculateSwimmingWorldPosition()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.CalculateSwimmingWorldPosition"))
		{
			MovementTransformUpdate update = new MovementTransformUpdate
			{
				flags = 16,
				path = new PathFollowingUpdate
				{
					position = _nextPositionNS.Value
				}
			};
			TakeWaterSample(ref update);
			return PlaceOnNavmesh(in update, NavToWorldNow, ((Component)baseEntity).transform.position.y, Time.deltaTime);
		}
	}

	public float AdjustDesiredSpeedWhenSwimming(float desiredGroundSpeed, float groundSprintSpeed)
	{
		if (!IsSwimming || desiredGroundSpeed <= 0f)
		{
			return desiredGroundSpeed;
		}
		if (!(desiredGroundSpeed < groundSprintSpeed))
		{
			return swimSprintSpeed;
		}
		return swimSpeed;
	}

	public bool IsPositionOnFavoredTerrain(NavVector3 positionNS)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsPositionOnFavoredTerrain(NavToWorldSpace(positionNS));
	}

	public bool IsPositionOnFavoredTerrain(Vector3 positionWS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsPositionOnFavoredTerrain"))
		{
			return IsPositionAtTopologyRequirement(positionWS, preferedTopology) && IsPositionABiomeRequirement(positionWS, preferedBiome);
		}
	}

	public bool IsPositionAtTopologyRequirement(NavVector3 positionNS, Enum topologyRequirement)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return IsPositionAtTopologyRequirement(NavToWorldSpace(positionNS), topologyRequirement);
	}

	public bool IsPositionAtTopologyRequirement(Vector3 positionWS, Enum topologyRequirement)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsPositionAtTopologyRequirement"))
		{
			if ((Object)(object)TerrainMeta.TopologyMap == (Object)null)
			{
				return false;
			}
			Enum val = (Enum)TerrainMeta.TopologyMap.GetTopology(positionWS);
			if ((topologyRequirement & val) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsPositionABiomeRequirement(NavVector3 positionNS, Enum biomeRequirement)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return IsPositionABiomeRequirement(NavToWorldSpace(positionNS), biomeRequirement);
	}

	public bool IsPositionABiomeRequirement(Vector3 positionWS, Enum biomeRequirement)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsPositionABiomeRequirement"))
		{
			if ((int)biomeRequirement == 0)
			{
				return true;
			}
			if ((Object)(object)TerrainMeta.BiomeMap == (Object)null)
			{
				return false;
			}
			Enum val = (Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(positionWS);
			if ((biomeRequirement & val) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsPositionASplatRequirement(NavVector3 positionNS, Enum splatRequirement)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return IsPositionASplatRequirement(NavToWorldSpace(positionNS), splatRequirement);
	}

	public bool IsPositionASplatRequirement(Vector3 positionWS, Enum splatRequirement)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsPositionASplatRequirement"))
		{
			if ((int)splatRequirement == 0)
			{
				return true;
			}
			if ((Object)(object)TerrainMeta.SplatMap == (Object)null)
			{
				return false;
			}
			Enum val = (Enum)TerrainMeta.SplatMap.GetSplatMaxType(positionWS);
			if ((splatRequirement & val) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsInWater(NavVector3 positionNS)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsInWater(NavToWorldSpace(positionNS));
	}

	public bool IsInWater(Vector3 positionWS)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsInWater"))
		{
			if ((Object)(object)baseEntity.GetParentEntity() != (Object)null)
			{
				return false;
			}
			if (WaterLevel.GetWaterDepth(positionWS, waves: false, volumes: false) >= 0.3f)
			{
				return true;
			}
			return false;
		}
	}

	public bool SampleGroundPositionWithPhysics(Vector3 positionWS, out NavGroundHit hitInfoNS, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return SampleGroundPositionWithPhysics(WorldToNavSpace(positionWS), out hitInfoNS, maxDistance, radius, layerMask);
	}

	public bool SampleGroundPositionWithPhysics(NavVector3 positionNS, out NavGroundHit hitInfoNS, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SampleGroundPositionWithPhysics"))
		{
			Vector3 val = NavToWorldSpace(positionNS) + Vector3.up * radius * 1.5f;
			float maxDistance2 = maxDistance + radius * 1.5f;
			bool flag = GamePhysics.TraceRealm(GamePhysics.Realm.Server, new Ray(val, Vector3.down), radius, out var hitInfo, maxDistance2, layerMask, (QueryTriggerInteraction)1);
			hitInfoNS = new NavGroundHit
			{
				distance = hitInfo.distance,
				collider = hitInfo.collider,
				rawHitWS = hitInfo
			};
			if (!flag)
			{
				hitInfoNS.point = positionNS;
				hitInfoNS.normal = NavVector3.up;
				return false;
			}
			hitInfoNS.point = WorldToNavSpace(hitInfo.point);
			hitInfoNS.normal = WorldToNavDirection(hitInfo.normal);
			if (radius > 0f && hitInfoNS.distance <= 0f)
			{
				hitInfoNS.point = positionNS;
			}
			return true;
		}
	}

	public static void LandQueuedMoves()
	{
		if (pendingTransformUpdates != 0)
		{
			FlushTransformBatch();
		}
	}

	public void Face(Quaternion rotation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AgentTransform.rotation = rotation;
		hasCachedSteeringForward = false;
		if (HasPendingTransform)
		{
			ref MovementTransformUpdate pendingUpdate = ref PendingUpdate;
			pendingUpdate.flags &= 153;
			pendingUpdate.maxDegreesDelta = 0f;
		}
	}

	public void Place(Vector3 positionWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Place(positionWS, AgentTransform.rotation);
	}

	public void Place(Vector3 positionWS, Quaternion rotation)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		DiscardOwnUpdate();
		AgentTransform.SetPositionAndRotation(positionWS, rotation);
		hasCachedSteeringForward = false;
		if (IsPaused)
		{
			placedWhilePaused = true;
		}
		else
		{
			Warp(positionWS);
		}
	}

	private bool StandsOnItsNavPosition()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh || !_updatePosition)
		{
			return false;
		}
		Vector3 val = AgentTransform.position - NavToWorldSpace(_nextPositionNS + NavVector3.up * _baseOffset);
		if (canSwim)
		{
			val.y = 0f;
		}
		return val.sqrMagnitude < 0.0001f;
	}

	private static void EnsureMovementCapacity()
	{
		if (movementUpdates == null || pendingTransformUpdates == movementUpdates.Length)
		{
			int newSize = Mathf.NextPowerOfTwo(pendingTransformUpdates + 32);
			Array.Resize(ref movementUpdates, newSize);
			Array.Resize(ref pendingTransformAgents, newSize);
		}
	}

	private static void EnsureResidentCapacity(int needed)
	{
		if (residentAgents == null || needed > residentAgents.Length)
		{
			int newSize = Mathf.NextPowerOfTwo(needed + 32);
			Array.Resize(ref residentAgents, newSize);
			Array.Resize(ref slotUpdates, newSize);
			Array.Resize(ref slotLastQueued, newSize);
			Array.Resize(ref slotBodies, newSize);
		}
	}

	private bool JoinMovementTransforms()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (transformSlot >= 0)
		{
			return true;
		}
		if (!inEnabledSet || AI.useUnityNavmesh)
		{
			return false;
		}
		if (!movementTransforms.isCreated)
		{
			movementTransforms = new TransformAccessArray(128, -1);
		}
		EnsureResidentCapacity(residentCount + 1);
		transformSlot = residentCount++;
		movementTransforms.Add(AgentTransform);
		residentAgents[transformSlot] = this;
		slotUpdates[transformSlot] = 0;
		slotLastQueued[transformSlot] = movementTick;
		slotBodies[transformSlot] = default;
		return true;
	}

	private void LeaveMovementTransforms()
	{
		if (transformSlot < 0)
		{
			return;
		}
		try
		{
			LandOwnUpdate();
		}
		finally
		{
			if (transformSlot >= 0)
			{
				ReleaseTransformSlot();
			}
		}
	}

	private void ReleaseTransformSlot()
	{
		int num = transformSlot;
		int num2 = --residentCount;
		movementTransforms.RemoveAtSwapBack(num);
		RustNavMeshAgent rustNavMeshAgent = residentAgents[num2];
		residentAgents[num] = rustNavMeshAgent;
		slotUpdates[num] = slotUpdates[num2];
		slotLastQueued[num] = slotLastQueued[num2];
		slotBodies[num] = slotBodies[num2];
		slotBodies[num2] = default;
		rustNavMeshAgent.transformSlot = num;
		residentAgents[num2] = null;
		slotUpdates[num2] = 0;
		transformSlot = -1;
	}

	private static void LeaveIdleSlots()
	{
		for (int num = residentCount - 1; num >= 0; num--)
		{
			if (slotUpdates[num] == 0 && movementTick - slotLastQueued[num] > 8)
			{
				residentAgents[num].ReleaseTransformSlot();
			}
		}
	}

	private void QueueRotationOverride(Quaternion rotation)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (HasPendingTransform)
		{
			ref MovementTransformUpdate pendingUpdate = ref PendingUpdate;
			pendingUpdate.rotation = rotation;
			pendingUpdate.flags = (byte)((pendingUpdate.flags | 2 | 0x40) & -37);
		}
		else
		{
			MovementTransformUpdate update = new MovementTransformUpdate
			{
				rotation = rotation,
				flags = 66
			};
			EnqueueMovementUpdate(in update);
		}
	}

	private void EnqueueMovementUpdate(in MovementTransformUpdate update)
	{
		if (JoinMovementTransforms())
		{
			EnsureMovementCapacity();
			int num = pendingTransformUpdates;
			movementUpdates[num] = update;
			slotUpdates[transformSlot] = num + 1;
			slotLastQueued[transformSlot] = movementTick;
			pendingTransformAgents[num] = this;
			pendingTransformUpdates = num + 1;
		}
	}

	private void TakeWaterSample(ref MovementTransformUpdate update)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(NavToWorldSpace(_nextPositionNS), waves: false, volumes: false);
		float value = desiredSwimDepth.Value;
		bool flag = (IsSwimming = waterInfo.currentDepth > value);
		update.swimming = (byte)(flag ? 1 : 0);
		update.swimTarget = waterInfo.surfaceLevel - value;
		update.swimFloor = waterInfo.terrainHeight;
	}

	private bool QueueDeltaMove(NavVector3 deltaNS)
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		if (!inEnabledSet)
		{
			return false;
		}
		if (HasPendingTransform && (PendingUpdate.flags & 0x80) == 0)
		{
			LandOwnUpdate();
		}
		if (boundNavmesh == null || !boundNavmesh.IsValid())
		{
			TryBindNavmesh();
		}
		if (boundNavmesh == null || corridor == IntPtr.Zero)
		{
			return false;
		}
		if (HasStaleAnchor)
		{
			TryBindNavmesh();
			ResetPath();
			return true;
		}
		if (!letUnityMoveAgentIfPossible && canSteer && !IsPaused)
		{
			deltaNS = AdjustMovementForSteering(deltaNS, null);
		}
		NavVector3 flatOffsetNS = deltaNS.Flat();
		bool flag = TurnsWithMove(flatOffsetNS);
		bool interpolates = letUnityMoveAgentIfPossible || !canSteer;
		if (HasPendingTransform)
		{
			ref MovementTransformUpdate pendingUpdate = ref PendingUpdate;
			if ((pendingUpdate.flags & 0x80) != 0 && pendingUpdate.path.navmesh == boundNavmesh.NavMeshHandle)
			{
				ref Vector3 waypoint = ref pendingUpdate.path.waypoint;
				waypoint += deltaNS.Value;
				if (_updatePosition)
				{
					pendingUpdate.flags |= 1;
				}
				if (flag && (pendingUpdate.flags & 0x40) == 0)
				{
					TurnWithStep(ref pendingUpdate, flatOffsetNS, interpolates);
				}
				if (npcDoorsWillOpen)
				{
					pendingUpdate.path.flags |= 32;
				}
				return true;
			}
			LandOwnUpdate();
		}
		byte b = 128;
		if (_updatePosition)
		{
			b |= 1;
		}
		if (canSwim)
		{
			b |= 0x10;
		}
		byte b2 = 0;
		if (npcDoorsWillOpen)
		{
			b2 |= 0x20;
		}
		MovementTransformUpdate update = new MovementTransformUpdate
		{
			rotation = Quaternion.identity,
			flags = b,
			space = QueuedSpace,
			path = new PathFollowingUpdate
			{
				navmesh = boundNavmesh.NavMeshHandle,
				corridor = corridor,
				position = _nextPositionNS.Value,
				waypoint = (_nextPositionNS + deltaNS).Value,
				baseOffset = _baseOffset,
				moved = -1,
				flags = b2
			}
		};
		if (flag)
		{
			TurnWithStep(ref update, flatOffsetNS, interpolates);
		}
		if (canSwim)
		{
			TakeWaterSample(ref update);
		}
		EnqueueMovementUpdate(in update);
		return true;
	}

	private void TurnWithStep(ref MovementTransformUpdate update, NavVector3 flatOffsetNS, bool interpolates)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		update.rotation = Quaternion.LookRotation(flatOffsetNS.Value);
		update.path.forward = flatOffsetNS.NormalizeXZ().Value;
		update.maxDegreesDelta += _angularSpeed * Time.deltaTime;
		update.flags |= 2;
		if (interpolates)
		{
			update.flags |= 4;
		}
		else
		{
			update.flags &= 251;
		}
	}

	private unsafe static void CalculateMovement(ref MovementTransformUpdate update, IntPtr query, BodyWalls* body, Quaternion current, float dt)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((update.flags & 8) != 0)
		{
			CalculatePathFollowing(ref update, query, body, current, dt);
			return;
		}
		CalculateDeltaMove(ref update, query);
		if ((update.flags & 0x42) == 2 && update.path.moved == 0)
		{
			update.flags &= 249;
		}
	}

	private static Vector3 PlaceOnNavmesh(in MovementTransformUpdate update, in float4x4 navToWorld, float currentY, float dt)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if ((update.flags & 0x10) == 0)
		{
			return float3.op_Implicit(math.transform(navToWorld, float3.op_Implicit(update.path.position + Vector3.up * update.path.baseOffset)));
		}
		Vector3 result = float3.op_Implicit(math.transform(navToWorld, float3.op_Implicit(update.path.position)));
		if (update.swimming != 0)
		{
			result.y = Mathf.Max(Mathf.MoveTowards(currentY, update.swimTarget, dt), update.swimFloor);
		}
		return result;
	}

	private static Quaternion RotateTowards(quaternion current, quaternion target, float maxDegreesDelta)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float num = math.min(math.abs(math.dot(current.value, target.value)), 1f);
		if (num > 0.999999f)
		{
			return quaternion.op_Implicit(target);
		}
		float num2 = 2f * math.acos(num) * 57.29578f;
		return quaternion.op_Implicit(math.slerp(current, target, math.min(1f, maxDegreesDelta / num2)));
	}

	private unsafe static Quaternion ResolveMovement(ref MovementTransformUpdate update, IntPtr query, BodyWalls* body, Quaternion current, float currentY, in float4x4 navToWorld, float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = (((update.flags & 0x20) != 0) ? update.rotation : current);
		if ((update.flags & 0x88) != 0)
		{
			CalculateMovement(ref update, query, body, val, dt);
			update.position = PlaceOnNavmesh(in update, in navToWorld, currentY, dt);
		}
		Quaternion val2 = update.rotation;
		if (update.space != 0 && (update.flags & 0x42) == 2)
		{
			val2 = quaternion.op_Implicit(quaternion.LookRotationSafe(math.rotate(navToWorld, math.mul(quaternion.op_Implicit(val2), new float3(0f, 0f, 1f))), math.up()));
		}
		if ((update.flags & 4) != 0)
		{
			val2 = RotateTowards(quaternion.op_Implicit(val), quaternion.op_Implicit(val2), update.maxDegreesDelta);
		}
		return val2;
	}

	private static void WriteTransform(Transform target, in MovementTransformUpdate update, Quaternion rotation)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((update.flags & 1) != 0)
		{
			if ((update.flags & 2) != 0)
			{
				target.SetPositionAndRotation(update.position, rotation);
			}
			else
			{
				target.position = update.position;
			}
		}
		else if ((update.flags & 2) != 0)
		{
			target.rotation = rotation;
		}
	}

	private unsafe void ResolveAndWrite(ref MovementTransformUpdate update, in float4x4 navToWorld)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Transform agentTransform = AgentTransform;
		IntPtr query = (((update.flags & 0x88) != 0) ? movementQueries[0] : IntPtr.Zero);
		float currentY = (((update.flags & 0x10) != 0) ? agentTransform.position.y : 0f);
		Quaternion rotation;
		fixed (BodyWalls* ptr = slotBodies)
		{
			rotation = ResolveMovement(ref update, query, (ptr != null && transformSlot >= 0) ? (ptr + transformSlot) : null, agentTransform.rotation, currentY, in navToWorld, Time.deltaTime);
		}
		WriteTransform(agentTransform, in update, rotation);
	}

	private bool IsBoundTo(IntPtr navmesh)
	{
		if (boundNavmesh != null && navmesh != IntPtr.Zero)
		{
			return boundNavmesh.NavMeshHandle == navmesh;
		}
		return false;
	}

	private bool LostItsNavmesh(ref MovementTransformUpdate update)
	{
		if ((update.flags & 0x88) == 0 || IsBoundTo(update.path.navmesh))
		{
			return false;
		}
		update.flags = (byte)(((update.flags & 0x40) != 0) ? 66 : 0);
		update.space = 0;
		return true;
	}

	private static ushort ResolveSpace(RustNavMeshAgent agent)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!agent.OnMovingNavmesh)
		{
			return 0;
		}
		IndependantNavmesh independantNavmesh = agent.independantNavmesh;
		for (int i = 0; i < spaceCount; i++)
		{
			if (spaceNavmeshes[i] == independantNavmesh)
			{
				return (ushort)(i + 1);
			}
		}
		if (spaceNavmeshes == null || spaceCount == spaceNavmeshes.Length)
		{
			int newSize = Mathf.NextPowerOfTwo(spaceCount + 4);
			Array.Resize(ref spaceNavmeshes, newSize);
			Array.Resize(ref spaceMatrices, newSize);
		}
		spaceNavmeshes[spaceCount] = independantNavmesh;
		spaceMatrices[spaceCount] = float4x4.op_Implicit(independantNavmesh.NavToWorldMatrix);
		return (ushort)(++spaceCount);
	}

	private static float4x4 SpaceMatrix(ushort space)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (space != 0)
		{
			return spaceMatrices[space - 1];
		}
		return float4x4.identity;
	}

	private static bool PrepareBatch(int count)
	{
		bool result = false;
		spaceCount = 0;
		for (int i = 0; i < count; i++)
		{
			ref MovementTransformUpdate reference = ref movementUpdates[i];
			if ((reference.flags & 0x88) != 0 && !pendingTransformAgents[i].LostItsNavmesh(ref reference))
			{
				result = true;
				if (reference.space == ushort.MaxValue)
				{
					reference.space = ResolveSpace(pendingTransformAgents[i]);
				}
			}
		}
		return result;
	}

	private void LandOwnUpdate()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!HasPendingTransform)
		{
			return;
		}
		MovementTransformUpdate update = PendingUpdate;
		DiscardOwnUpdate();
		if (!LostItsNavmesh(ref update) || update.flags != 0)
		{
			float4x4 navToWorld = float4x4.identity;
			if ((update.flags & 0x88) != 0)
			{
				navToWorld = NavToWorldNow;
				update.space = (ushort)(OnMovingNavmesh ? 1 : 0);
				EnsureMovementQueries(1);
			}
			ResolveAndWrite(ref update, in navToWorld);
			ApplyMovementResult(in update);
			FinishLanding(in update);
		}
	}

	private void DiscardOwnUpdate()
	{
		if (HasPendingTransform)
		{
			int num = slotUpdates[transformSlot] - 1;
			slotUpdates[transformSlot] = 0;
			movementUpdates[num].flags = 0;
			movementUpdates[num].path.moved = -1;
		}
	}

	private void ApplyMovementResult(in MovementTransformUpdate update)
	{
		if (update.path.moved >= 0)
		{
			if ((update.flags & 8) != 0)
			{
				ApplyPathFollowingResult(in update);
			}
			else if ((update.flags & 0x80) != 0)
			{
				ApplyDeltaMoveResult(in update);
			}
		}
	}

	private static bool StepsThroughDoors(in MovementTransformUpdate update)
	{
		if ((update.flags & 8) == 0)
		{
			if ((update.flags & 0x80) != 0)
			{
				return update.path.moved > 0;
			}
			return false;
		}
		return true;
	}

	private void FinishLanding(in MovementTransformUpdate update)
	{
		if (StepsThroughDoors(in update) && canOpenDoors && HasLiveEntity)
		{
			TryOpenDoors();
		}
	}

	private void ApplyDeltaMoveResult(in MovementTransformUpdate update)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		hasCachedSteeringForward = (update.flags & 0x46) == 2;
		if (hasCachedSteeringForward)
		{
			cachedSteeringForwardNS = new NavVector3(update.path.forward);
		}
		if (update.path.moved != 0)
		{
			_nextPositionNS = new NavVector3(update.path.position);
		}
		else if (!boundNavmesh.IsValidPolyRef(currentPolyRef))
		{
			if (!followingPath || IsPaused || _isStopped || lastSeenTileVersion == boundNavmesh.TileChangeVersion)
			{
				TryBindNavmesh();
				ResetPath();
			}
		}
		else if (followingPath)
		{
			ResetPath();
		}
	}

	private static void FlushTransformBatch()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		int num = pendingTransformUpdates;
		if (num == 0)
		{
			return;
		}
		using (TimeWarning.New("RustNavMeshAgent.TransformBatch"))
		{
			try
			{
				bool flag = PrepareBatch(num);
				if (num < 32)
				{
					if (flag)
					{
						EnsureMovementQueries(1);
					}
					for (int i = 0; i < num; i++)
					{
						if (movementUpdates[i].flags != 0)
						{
							pendingTransformAgents[i].ResolveAndWrite(ref movementUpdates[i], SpaceMatrix(movementUpdates[i].space));
						}
					}
				}
				else
				{
					if (flag)
					{
						EnsureMovementQueries(JobsUtility.ThreadIndexCount);
					}
					RunMovementTransformJob(num);
				}
			}
			finally
			{
				ApplyBatch(num);
			}
		}
	}

	private static void ApplyBatch(int count)
	{
		MovementTransformUpdate[] array = movementUpdates;
		RustNavMeshAgent[] array2 = pendingTransformAgents;
		movementUpdates = landingUpdates;
		pendingTransformAgents = landingAgents;
		landingUpdates = array;
		landingAgents = array2;
		pendingTransformUpdates = 0;
		if (residentCount > 0)
		{
			Array.Clear(slotUpdates, 0, residentCount);
		}
		for (int i = 0; i < count; i++)
		{
			array2[i].ApplyMovementResult(in array[i]);
		}
		for (int j = 0; j < count; j++)
		{
			RustNavMeshAgent rustNavMeshAgent = array2[j];
			array2[j] = null;
			rustNavMeshAgent.FinishLanding(in array[j]);
		}
	}

	private unsafe static void RunMovementTransformJob(int count)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		fixed (MovementTransformUpdate* ptr = movementUpdates)
		{
			fixed (IntPtr* ptr2 = movementQueries)
			{
				fixed (int* ptr3 = slotUpdates)
				{
					fixed (float4x4* ptr4 = spaceMatrices)
					{
						fixed (BodyWalls* bodies = slotBodies)
						{
							NativeArray<MovementTransformUpdate> updates = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<MovementTransformUpdate>((void*)ptr, count, (Allocator)1);
							NativeArray<IntPtr> queries = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<IntPtr>((void*)ptr2, movementQueries?.Length ?? 0, (Allocator)1);
							NativeArray<int> val = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>((void*)ptr3, residentCount, (Allocator)1);
							NativeArray<float4x4> spaces = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float4x4>((void*)ptr4, spaceCount, (Allocator)1);
							MovementTransformJob movementTransformJob = new MovementTransformJob
							{
								updates = updates,
								slotUpdates = val,
								queries = queries,
								spaces = spaces,
								bodies = bodies,
								deltaTime = Time.deltaTime
							};
							TransformAccessArray val2 = movementTransforms;
							JobHandle val3 = default;
							val3 = IJobParallelForTransformExtensions.Schedule<MovementTransformJob>(movementTransformJob, val2, val3);
							val3.Complete();
						}
					}
				}
			}
		}
	}

	private static void DisposeTransformBatch()
	{
		FlushTransformBatch();
		for (int i = 0; i < residentCount; i++)
		{
			residentAgents[i].transformSlot = -1;
			residentAgents[i] = null;
		}
		residentCount = 0;
		if (movementTransforms.isCreated)
		{
			movementTransforms.Dispose();
		}
		DisposeMovementQueries();
		movementUpdates = null;
		pendingTransformAgents = null;
		landingUpdates = null;
		landingAgents = null;
		residentAgents = null;
		slotUpdates = null;
		slotLastQueued = null;
		slotBodies = null;
		spaceNavmeshes = null;
		spaceMatrices = null;
		spaceCount = 0;
	}

	[Conditional("DEBUG")]
	private void CheckOwnPose()
	{
	}

	[Conditional("DEBUG")]
	private void RecordOwnPose()
	{
	}

	[Conditional("DEBUG")]
	private void ForgetOwnPose()
	{
	}

	private unsafe bool GatherBodyWalls()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (!HasLiveEntity || boundNavmesh == null || corridor == IntPtr.Zero || !JoinMovementTransforms())
		{
			return false;
		}
		fixed (BodyWalls* ptr = &slotBodies[transformSlot])
		{
			if (ptr->tileCount == 0 || !StillHoldsBodyWalls(ptr))
			{
				GatherBodyWallsInto(ptr, _nextPositionNS, 0.5f);
			}
			if (ptr->count == 0)
			{
				return false;
			}
			float num = DistanceToSegment(ptr->centre, new float2(*ptr->segments, ptr->segments[1]), new float2(ptr->segments[2], ptr->segments[3]));
			float num2 = math.distance(new float2(_nextPositionNS.x, _nextPositionNS.z), ptr->centre);
			return num - num2 <= BodyReach(ptr, 0.5f) + currentSpeed * Time.deltaTime;
		}
	}

	private unsafe static float BodyReach(BodyWalls* body, float lookahead)
	{
		return Mathf.Max(body->front + lookahead, body->back);
	}

	private unsafe void GatherBodyWallsInto(BodyWalls* body, NavVector3 centreNS, float lookahead)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.GatherBodyWalls"))
		{
			Bounds bounds = cachedEntity.bounds;
			body->front = bounds.center.z + bounds.extents.z;
			body->back = bounds.extents.z - bounds.center.z;
			body->halfWidth = bounds.extents.x;
			GatherWallsInto(body, centreNS, BodyReach(body, lookahead) + 0.5f);
		}
	}

	private unsafe void GatherWallsInto(BodyWalls* body, NavVector3 centreNS, float radius)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		body->count = boundNavmesh.GetWallsAround(currentPolyRef, centreNS, radius, body->segments, 16, body->tiles, 12, out body->tileCount, out body->stamp, npcDoorsWillOpen);
		System.Runtime.CompilerServices.Unsafe.Write(&body->centre, new float2(centreNS.x, centreNS.z));
		body->navmesh = boundNavmesh.NavMeshHandle;
		body->tileVersion = boundNavmesh.TileChangeVersion;
	}

	public unsafe bool BoxInsideWalls(Bounds box, Vector3 positionWS, Vector3 forwardWS)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!BodyWallsKnown)
		{
			return true;
		}
		NavVector3 centreNS = WorldToNavSpace(positionWS);
		NavVector3 navVector = WorldToNavDirection(forwardWS);
		float radius = math.length(new float2(math.max(0f - box.min.x, box.max.x), math.max(0f - box.min.z, box.max.z)));
		BodyWalls bodyWalls = default;
		GatherWallsInto(&bodyWalls, centreNS, radius);
		if (bodyWalls.count == 0)
		{
			return true;
		}
		float2 val = new float2(centreNS.x, centreNS.z);
		return BoxPastWalls(&bodyWalls, OffTheWalls(&bodyWalls, val), val, math.normalizesafe(new float2(navVector.x, navVector.z), default(float2)), box, 0.02f) <= 0.02f;
	}

	private unsafe static float BoxPastWalls(BodyWalls* body, float2 inside, float2 pivot, float2 heading, Bounds box, float limit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		float2 val = new float2(heading.y, 0f - heading.x);
		int num = math.max(1, (int)math.ceil(box.size.x / 0.1f));
		int num2 = math.max(1, (int)math.ceil(box.size.z / 0.1f));
		int num3 = 2 * (num + num2);
		float num4 = 0f;
		for (int i = 0; i < num3; i++)
		{
			float2 val2 = BoxOutlinePoint(box, i, num, num2);
			float2 point = pivot + val * val2.x + heading * val2.y;
			float* ptr = body->segments;
			int num5 = 0;
			while (num5 < body->count)
			{
				num4 = math.max(num4, PastWall(inside, point, new float2(*ptr, ptr[1]), new float2(ptr[2], ptr[3])) - ptr[4]);
				num5++;
				ptr += 5;
			}
			if (num4 > limit)
			{
				return num4;
			}
		}
		return num4;
	}

	private static float2 BoxOutlinePoint(Bounds box, int k, int across, int along)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = box.min;
		Vector3 max = box.max;
		if (k < across)
		{
			return new float2(math.lerp(min.x, max.x, (float)k / (float)across), max.z);
		}
		k -= across;
		if (k < along)
		{
			return new float2(max.x, math.lerp(max.z, min.z, (float)k / (float)along));
		}
		k -= along;
		if (k < across)
		{
			return new float2(math.lerp(max.x, min.x, (float)k / (float)across), min.z);
		}
		k -= across;
		return new float2(min.x, math.lerp(min.z, max.z, (float)k / (float)along));
	}

	public unsafe bool CanPlayTurnInPlace(RootMotionData turn, float rootBoneLocalZOffset)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if (turn == null || turn.yRotationCurve == null || (Object)(object)turn.inPlaceAnimation == (Object)null || !BodyWallsKnown)
		{
			return true;
		}
		NavVector3 centreNS = WorldToNavSpace(AgentTransform.position);
		float2 val = new float2(centreNS.x, centreNS.z);
		BodyWalls bodyWalls = default;
		GatherBodyWallsInto(&bodyWalls, centreNS, 0f);
		if (bodyWalls.tileCount == 0)
		{
			return !RecastWrapper.WallsAroundAvailable;
		}
		NavVector3 navVector = WorldToNavDirection(AgentTransform.forward);
		float2 val2 = OffTheWalls(&bodyWalls, val);
		float num = math.max(BodyReachPastWalls(&bodyWalls, val2, val, math.normalizesafe(new float2(navVector.x, navVector.z), default(float2)), 0f, float.MaxValue), 0.02f);
		RootMotionPlayer.PlayServerState playServerState = RootMotionPlayer.PlayServerState.TakeFromPool(turn, AgentTransform);
		try
		{
			Vector3 location = AgentTransform.position;
			Quaternion rotation = AgentTransform.rotation;
			float deltaTime = (turn.inPlaceAnimation.length - 0.25f) / 16f;
			playServerState.Step(deltaTime, ref location, ref rotation, rootBoneLocalZOffset);
			for (int i = 1; i <= 16; i++)
			{
				playServerState.Step(deltaTime, ref location, ref rotation, rootBoneLocalZOffset);
				NavVector3 centreNS2 = WorldToNavSpace(location);
				NavVector3 navVector2 = WorldToNavDirection(rotation * Vector3.forward);
				float2 val3 = new float2(centreNS2.x, centreNS2.z);
				if (math.distancesq(val3, bodyWalls.centre) > 0.25f)
				{
					GatherBodyWallsInto(&bodyWalls, centreNS2, 0f);
					if (bodyWalls.tileCount == 0)
					{
						return false;
					}
					val2 = OffTheWalls(&bodyWalls, val);
				}
				float2 val4 = OffTheWalls(&bodyWalls, val3);
				if (CrossesAnyWall(&bodyWalls, val2, val4))
				{
					return false;
				}
				if (BodyReachPastWalls(&bodyWalls, val4, val3, math.normalizesafe(new float2(navVector2.x, navVector2.z), default(float2)), 0f, num) > num)
				{
					return false;
				}
				val = val3;
				val2 = val4;
			}
			return true;
		}
		finally
		{
			Pool.Free<RootMotionPlayer.PlayServerState>(ref playServerState);
		}
	}

	private unsafe static bool CrossesAnyWall(BodyWalls* body, float2 from, float2 to)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float* ptr = body->segments;
		int num = 0;
		while (num < body->count)
		{
			if (Crosses(from, to, new float2(*ptr, ptr[1]), new float2(ptr[2], ptr[3])))
			{
				return true;
			}
			num++;
			ptr += 5;
		}
		return false;
	}

	private unsafe static float2 OffTheWalls(BodyWalls* body, float2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		float2 val = float2.zero;
		float* ptr = body->segments;
		int num = 0;
		while (num < body->count)
		{
			float2 val2 = new float2(*ptr, ptr[1]);
			float2 val3 = new float2(ptr[2], ptr[3]);
			if (DistanceToSegment(point, val2, val3) <= 0.01f)
			{
				val += math.normalizesafe(new float2(val3.y - val2.y, val2.x - val3.x), default(float2));
			}
			num++;
			ptr += 5;
		}
		return point + math.normalizesafe(val, default(float2)) * 0.01f;
	}

	private unsafe bool StillHoldsBodyWalls(BodyWalls* body)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (body->navmesh != boundNavmesh.NavMeshHandle)
		{
			return false;
		}
		float num = math.max(0f, 0.5f - currentSpeed * Time.deltaTime);
		if (math.distancesq(new float2(_nextPositionNS.x, _nextPositionNS.z), body->centre) > num * num)
		{
			return false;
		}
		if (body->tileVersion == boundNavmesh.TileChangeVersion)
		{
			return true;
		}
		if (!boundNavmesh.TileStampsUnchangedSince(body->tiles, body->tileCount, body->stamp))
		{
			return false;
		}
		body->tileVersion = boundNavmesh.TileChangeVersion;
		return true;
	}

	private unsafe static bool KeepBodyOffWalls(BodyWalls* body, ref PathFollowingUpdate path, ref Vector3 delta, Vector3 forward, Vector3 toWaypoint, float dt)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		float num = math.sqrt(delta.x * delta.x + delta.z * delta.z);
		if (num < 0.0001f)
		{
			return false;
		}
		float2 val = new float2(path.position.x, path.position.z);
		float2 inside = OffTheWalls(body, val);
		float2 val2 = new float2(delta.x, delta.z) / num;
		float lookahead = math.min(path.speed.current * 0.3f, 0.5f);
		if (StepClear(body, inside, val, val2, num, lookahead))
		{
			return false;
		}
		float2 val3 = math.normalizesafe(new float2(forward.x, forward.z), default(float2));
		float num2 = math.atan2(val3.x * val2.y - val3.y * val2.x, math.dot(val3, val2));
		for (int num3 = 2; num3 <= 8; num3 *= 2)
		{
			float2 val4 = RotateFlat(val3, num2 / (float)num3);
			if (StepClear(body, inside, val, val4, num, lookahead, 0.04f))
			{
				delta = new Vector3(val4.x * num, delta.y, val4.y * num);
				return true;
			}
		}
		float num4 = math.atan2(val2.x * toWaypoint.z - val2.y * toWaypoint.x, val2.x * toWaypoint.x + val2.y * toWaypoint.z);
		float num5 = ((num4 >= 0f) ? 1f : (-1f));
		float num6 = math.max(num / 0.75f, math.radians(90f) * dt);
		float radians = math.clamp(num4, 0f - num6, num6);
		float num7 = 0.1f;
		int num8 = 0;
		while (num8 < 5)
		{
			float num9 = math.radians(BodySearchDegrees(num8));
			if (StepClear(body, inside, val, RotateFlat(val2, num9 * num5), num, lookahead))
			{
				radians = num5 * math.min(num9, num6);
			}
			else
			{
				if (!StepClear(body, inside, val, RotateFlat(val2, (0f - num9) * num5), num, lookahead))
				{
					num8++;
					continue;
				}
				radians = (0f - num5) * num6;
			}
			num7 = math.clamp(num6 / num9, 0.25f, 1f);
			break;
		}
		float num10 = ((num7 < 1f) ? math.min(path.speed.current, path.speed.desired * num7) : path.speed.current);
		float num11 = ((path.speed.current > 0f) ? (num10 / path.speed.current) : 1f);
		float2 val5 = RotateFlat(val2, radians) * (num * num11);
		path.speed.current = num10;
		ref Vector3 reference = ref path.velocity;
		reference *= num11;
		delta = new Vector3(val5.x, delta.y, val5.y);
		return true;
	}

	private static float BodySearchDegrees(int i)
	{
		return i switch
		{
			0 => 20f, 
			1 => 40f, 
			2 => 70f, 
			3 => 100f, 
			_ => 135f, 
		};
	}

	private unsafe static bool StepClear(BodyWalls* body, float2 inside, float2 start, float2 direction, float length, float lookahead, float clearance = 0.02f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return BodyReachPastWalls(body, inside, start + direction * length, direction, lookahead, clearance) <= clearance;
	}

	private unsafe static float BodyReachPastWalls(BodyWalls* body, float2 inside, float2 pivot, float2 heading, float lookahead, float limit)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		float2 val = pivot + heading * (body->front - body->halfWidth + lookahead);
		float2 val2 = pivot - heading * (body->back - body->halfWidth);
		float num = 0f;
		float* ptr = body->segments;
		for (int i = 0; i < body->count; i++, ptr += 5)
		{
			float2 a = new float2(*ptr, ptr[1]);
			float2 b = new float2(ptr[2], ptr[3]);
			float num2 = body->halfWidth - ptr[4];
			float num3 = math.max(PastWall(inside, val, a, b), PastWall(inside, val2, a, b));
			if (num3 < 0f)
			{
				if (num2 <= num)
				{
					continue;
				}
				num3 = 0f - SegmentGap(val2, val, a, b);
			}
			num = math.max(num, num3 + num2);
			if (num > limit)
			{
				return num;
			}
		}
		return num;
	}

	private static float PastWall(float2 inside, float2 point, float2 a, float2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!Crosses(inside, point, a, b))
		{
			return -1f;
		}
		return DistanceToSegment(point, a, b);
	}

	private static float SegmentGap(float2 p, float2 q, float2 a, float2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return math.sqrt(math.min(math.min(DistanceSqToSegment(p, a, b), DistanceSqToSegment(q, a, b)), math.min(DistanceSqToSegment(a, p, q), DistanceSqToSegment(b, p, q))));
	}

	private static bool Crosses(float2 p, float2 q, float2 a, float2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		float2 val = q - p;
		float2 val2 = b - a;
		float num = val.x * val2.y - val.y * val2.x;
		if (math.abs(num) < 1E-07f)
		{
			return false;
		}
		float2 val3 = a - p;
		float num2 = (val3.x * val2.y - val3.y * val2.x) / num;
		float num3 = (val3.x * val.y - val3.y * val.x) / num;
		if (num2 >= 0f && num2 <= 1f && num3 >= 0f)
		{
			return num3 <= 1f;
		}
		return false;
	}

	private static float DistanceToSegment(float2 point, float2 a, float2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return math.sqrt(DistanceSqToSegment(point, a, b));
	}

	private static float DistanceSqToSegment(float2 point, float2 a, float2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float2 val = b - a;
		float num = math.lengthsq(val);
		float num2 = ((num > 1E-07f) ? math.saturate(math.dot(point - a, val) / num) : 0f);
		return math.distancesq(point, a + val * num2);
	}

	private static float2 RotateFlat(float2 direction, float radians)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float num = default;
		float num2 = default;
		math.sincos(radians, ref num, ref num2);
		return new float2(direction.x * num2 - direction.y * num, direction.x * num + direction.y * num2);
	}

	public RustNavMeshAgent()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
	}
}
