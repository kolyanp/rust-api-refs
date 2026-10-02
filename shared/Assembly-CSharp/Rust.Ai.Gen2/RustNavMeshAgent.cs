using System;
using System.Collections.Generic;
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

		public PathFollowingUpdate path;
	}

	[BurstCompile(CompileSynchronously = true)]
	internal struct MovementTransformJob : IJobParallelForTransform
	{
		public NativeArray<MovementTransformUpdate> updates;

		[ReadOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<IntPtr> queries;

		public float deltaTime;

		[NativeSetThreadIndex]
		private int threadIndex;

		public void Execute(int index, TransformAccess transform)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			MovementTransformUpdate update = updates[index];
			if (update.flags == 0 || !transform.isValid)
			{
				return;
			}
			if ((update.flags & 8) != 0)
			{
				CalculatePathFollowing(ref update, queries[threadIndex], transform.rotation, deltaTime);
				updates[index] = update;
			}
			Quaternion val = update.rotation;
			if ((update.flags & 4) != 0)
			{
				quaternion val2 = quaternion.op_Implicit(transform.rotation);
				quaternion val3 = quaternion.op_Implicit(val);
				float num = math.min(math.abs(math.dot(val2.value, val3.value)), 1f);
				if (num <= 0.999999f)
				{
					float num2 = 2f * math.acos(num) * 57.29578f;
					val = quaternion.op_Implicit(math.slerp(val2, val3, math.min(1f, update.maxDegreesDelta / num2)));
				}
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

	public bool letUnityMoveAgentIfPossible = true;

	private static ListHashSet<RustNavMeshAgent> enabledComponents = new ListHashSet<RustNavMeshAgent>();

	private static RustNavMeshPath tempPath = new RustNavMeshPath();

	private static RustNavMeshPath scratchPath = new RustNavMeshPath();

	private RustNavMeshPath CurPathNS = new RustNavMeshPath();

	private NavMeshAgent _agent;

	private Transform cachedTransform;

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

	private NavVector3 pendingOptimizeTargetNS;

	private bool hasPendingOptimize;

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

	public Vector3? overrideDirectionWS;

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

	private static bool pendingPathFollowing;

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

	private const int MinimumTransformBatch = 32;

	private static TransformAccessArray movementTransforms;

	private static MovementTransformUpdate[] movementUpdates;

	private static RustNavMeshAgent[] pendingTransformAgents;

	private static Transform[] registeredTransforms;

	private static bool transformBatchActive;

	private static RustNavMeshAgent transformBatchAgent;

	private static int pendingTransformUpdates;

	private bool hasPendingTransform;

	private Transform AgentTransform => cachedTransform ?? (cachedTransform = ((Component)this).transform);

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
			return followingPath;
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
			if (flag && maxDistance > num && Mathf.Abs(hitNS.position.y - positionNS.y) > num && SampleGroundPositionWithPhysics(positionNS, out var hitInfoNS, 3.5f) && SamplePositionInternal(hitInfoNS.point, out var hitNS2, num, out nearestPolyRef, includeGatedDoorPolys))
			{
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
			NavHit hitNS;
			NavHit hitNS2;
			return SampleReachTarget(locationNS, out hitNS) && !Raycast(hitNS.position, out hitNS2);
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
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		enabledComponents.TryAdd(this);
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
		enabledComponents.Remove(this);
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
		BeginTransformBatch(count);
		try
		{
			for (int num = count - 1; num >= 0; num--)
			{
				RustNavMeshAgent rustNavMeshAgent = enabledComponents[num];
				if (ObjectEx.IsUnityNull(rustNavMeshAgent) || !rustNavMeshAgent.baseEntity.IsValid())
				{
					enabledComponents.RemoveAt(num);
				}
				else
				{
					transformBatchAgent = rustNavMeshAgent;
					rustNavMeshAgent.Tick(deltaTime);
				}
			}
		}
		finally
		{
			transformBatchAgent = null;
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
			}
		}
	}

	private void Tick(float deltaTime)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
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
						if (overrideDirectionWS.HasValue)
						{
							AgentTransform.rotation = Quaternion.RotateTowards(AgentTransform.rotation, Quaternion.LookRotation(Vector3Ex.NormalizeXZ(overrideDirectionWS.Value)), _angularSpeed * deltaTime);
						}
						else
						{
							Transform agentTransform = AgentTransform;
							Matrix4x4 eyeTransform = senses.GetEyeTransform();
							agentTransform.rotation = Quaternion.LookRotation(Vector3Ex.NormalizeXZ(eyeTransform.rotation * Vector3.forward));
						}
					}
				}
				if (!hasPath || IsPaused || _isStopped)
				{
					if (!hasPath)
					{
						EnsureAnchorValidAfterTileRebuild();
					}
					if (currentSpeed == 0f && !(previousPositionNS != _nextPositionNS))
					{
						return;
					}
					using (TimeWarning.New("Tick.NoPath"))
					{
						currentSpeed = Mathf.SmoothDamp(currentSpeed, (_nextPositionNS - previousPositionNS).magnitude / deltaTime, ref dampVelocity, 1f, 9999f, Time.smoothDeltaTime * 10f);
						if (currentSpeed < 0.005f)
						{
							currentSpeed = 0f;
						}
						return;
					}
				}
				bool flag = !letUnityMoveAgentIfPossible && canSteer;
				float num = RestingDistance(currentStoppingDistance);
				NavVector3 navVector;
				float num2;
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent2))
					{
						return;
					}
					navVector = new NavVector3(agent2.steeringTarget);
					num2 = remainingDistance;
					if (num2 <= num)
					{
						ResetPath();
						return;
					}
				}
				else
				{
					if (boundNavmesh == null || corridor == IntPtr.Zero)
					{
						ResetPath();
						return;
					}
					bool flag2 = npcDoorsWillOpen;
					if (lastSeenTileVersion != boundNavmesh.TileChangeVersion || flag2 != lastCheckedNpcDoors)
					{
						lastSeenTileVersion = boundNavmesh.TileChangeVersion;
						if (flag2 != lastCheckedNpcDoors)
						{
							lastCheckedNpcDoors = flag2;
							lastCheckedFlagStamp = 0u;
						}
						bool flag3 = boundNavmesh.CorridorIsValid(corridor, int.MaxValue, flag2, lastCheckedFlagStamp, out var currentStamp);
						lastCheckedFlagStamp = currentStamp;
						if (!flag3)
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
					if (cornersDirty || (flag && ShouldRepullSteeringCorners()))
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
					if (lastCornerIsEnd && num2 <= num)
					{
						ResetPath();
						return;
					}
					if (flag)
					{
						pendingOptimizeTargetNS = corners[Mathf.Min(1, corners.Count - 1)];
						hasPendingOptimize = true;
					}
					else
					{
						float num3 = _nextPositionNS.x - navVector.x;
						float num4 = _nextPositionNS.z - navVector.z;
						if (num3 * num3 + num4 * num4 <= 0.0625f)
						{
							cornersDirty = true;
						}
					}
				}
				bool flag4 = !AI.useUnityNavmesh;
				if (AI.useUnityNavmesh || !TryQueuePathFollowing(navVector, num2, num, flag))
				{
					NavVector3 navVector2 = navVector - _nextPositionNS;
					if (flag4)
					{
						navVector2 = navVector2.Flat();
					}
					float magnitude = navVector2.magnitude;
					AdjustCurrentSpeedFromDesiredSpeed(_nextPositionNS, _autoBraking, num2, num, deltaTime);
					float num5 = currentSpeed * deltaTime;
					if (!flag4 || (lastCornerIsEnd && corners.Count == 1))
					{
						num5 = Mathf.Clamp(num5, 0f - magnitude, magnitude);
					}
					NavVector3 navVector3 = ((magnitude > 1E-05f) ? (navVector2 * (num5 / magnitude)) : NavVector3.zero);
					MoveInternal(navVector3, num2);
					_velocityNS = navVector3 / deltaTime;
				}
			}
		}
		finally
		{
			previousPositionNS = _nextPositionNS;
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
			angleOffset = Random.Range(0f, MathF.PI * 2f);
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
			angleOffset = Random.Range(0f, MathF.PI * 2f);
		}
		ulong startRef = 0uL;
		if (currentPolyRef != 0L && NavVector3.Distance(centerNS, _nextPositionNS) <= maxRadius)
		{
			startRef = currentPolyRef;
		}
		return boundNavmesh.FindDonutPointsInCircle(ref startRef, centerNS, maxRadius, minRadius, angleOffset, count, resultsNS, npcDoorsWillOpen);
	}

	public bool SetDestination(Vector3 targetPositionWS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return SetDestination(WorldToNavSpace(targetPositionWS));
	}

	public bool SetDestination(NavVector3 targetPositionNS)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SetDestination"))
		{
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
		FlushTransformBatch();
		try
		{
			MoveInternal(deltaNS, null);
		}
		finally
		{
			FlushTransformBatch();
		}
	}

	private void MoveInternal(NavVector3 deltaNS, float? remainingDistanceHint)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
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
					if ((!followingPath || lastSeenTileVersion != boundNavmesh.TileChangeVersion) && !boundNavmesh.IsValidPolyRef(currentPolyRef))
					{
						TryBindNavmesh();
						ResetPath();
						return;
					}
					bool flag;
					NavVector3 resultPosNS;
					if (hasPendingOptimize)
					{
						hasPendingOptimize = false;
						flag = boundNavmesh.CorridorOptimizeAndMove(corridor, pendingOptimizeTargetNS, 20f, _nextPositionNS + deltaNS, out resultPosNS, npcDoorsWillOpen);
					}
					else
					{
						flag = boundNavmesh.CorridorMove(corridor, _nextPositionNS + deltaNS, out resultPosNS, out var _, npcDoorsWillOpen);
					}
					if (flag)
					{
						_nextPositionNS = resultPosNS;
					}
					else if (followingPath)
					{
						ResetPath();
					}
				}
			}
			Quaternion? val = null;
			bool flag2 = false;
			NavVector3 directionNS = deltaNS.Flat();
			if (_updateRotation && !IsPaused && !overrideDirectionWS.HasValue && directionNS.Value.sqrMagnitude > 1E-06f)
			{
				using (TimeWarning.New("Move.RotationWrite"))
				{
					Vector3 val2 = NavToWorldDirection(directionNS);
					if (!letUnityMoveAgentIfPossible && canSteer)
					{
						val = Quaternion.LookRotation(val2);
						cachedSteeringForwardNS = directionNS.NormalizeXZ();
						hasCachedSteeringForward = true;
					}
					else
					{
						val = Quaternion.LookRotation(val2);
						flag2 = true;
						hasCachedSteeringForward = false;
					}
				}
			}
			else
			{
				hasCachedSteeringForward = false;
			}
			if (!TryQueueTransformUpdate(val, flag2))
			{
				if (flag2)
				{
					val = Quaternion.RotateTowards(AgentTransform.rotation, val.Value, _angularSpeed * Time.deltaTime);
				}
				SyncWorldPosWithNavPos(val);
				TryOpenDoors();
			}
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

	private bool SyncWorldPosWithNavPos(Quaternion? newRotation)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.TrySyncWorldPosWithNavPos"))
		{
			FlushTransformBatch();
			if (!_updatePosition)
			{
				if (newRotation.HasValue)
				{
					AgentTransform.rotation = newRotation.Value;
				}
				return false;
			}
			Vector3 movementWorldPosition = GetMovementWorldPosition();
			if (newRotation.HasValue)
			{
				AgentTransform.SetPositionAndRotation(movementWorldPosition, newRotation.Value);
			}
			else
			{
				AgentTransform.position = movementWorldPosition;
			}
			return true;
		}
	}

	public bool Warp(Vector3 newPositionWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return WarpToWorldPosition(newPositionWS);
	}

	public bool Warp(NavVector3 newPositionNS)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Warp"))
		{
			FlushTransformBatch();
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
			AnchorCorridor(nearestPolyRef, _nextPositionNS);
			ResetPath();
			TrySyncWorldPosWithNavPos();
			return true;
		}
	}

	public bool WarpToWorldPosition(Vector3 newPositionWS)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		FlushTransformBatch();
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
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && ((Behaviour)agent).enabled && agent.isOnNavMesh)
			{
				agent.ResetPath();
			}
			CurPathNS.Reset();
			followingPath = false;
			corners.Clear();
			cornersDirty = true;
			lastCornerIsEnd = false;
			hasPendingOptimize = false;
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

	private bool TryQueuePathFollowing(NavVector3 waypoint, float remaining, float stopping, bool steering)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.batch_navmesh_pathfollowing || !transformBatchActive || transformBatchAgent != this || canOpenDoors || canSwim || hasPendingTransform || independantNavmesh != null || overrideDirectionWS.HasValue || isScientist || boundNavmesh == null || !boundNavmesh.IsValid() || corridor == IntPtr.Zero)
		{
			return false;
		}
		EnsureMovementCapacity();
		byte b = 8;
		if (_updatePosition)
		{
			b |= 1;
		}
		if (_updateRotation)
		{
			b |= 2;
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
		if (hasCachedSteeringForward)
		{
			b2 |= 8;
		}
		if (hasPendingOptimize)
		{
			b2 |= 0x10;
		}
		if (npcDoorsWillOpen)
		{
			b2 |= 0x20;
		}
		movementUpdates[pendingTransformUpdates] = new MovementTransformUpdate
		{
			flags = b,
			maxDegreesDelta = _angularSpeed * Time.deltaTime,
			path = new PathFollowingUpdate
			{
				navmesh = boundNavmesh.NavMeshHandle,
				corridor = corridor,
				position = _nextPositionNS.Value,
				waypoint = waypoint.Value,
				destination = CurPathNS.GetDestinationNS().Value,
				optimizeTarget = pendingOptimizeTargetNS.Value,
				forward = cachedSteeringForwardNS.Value,
				speed = GetPathFollowingSpeed(_autoBraking, remaining, stopping),
				deviation = currentDeviation,
				turnRadius = maxTurnRadius,
				baseOffset = _baseOffset,
				moved = -1,
				flags = b2
			}
		};
		pendingTransformAgents[pendingTransformUpdates++] = this;
		hasPendingTransform = true;
		pendingPathFollowing = true;
		return true;
	}

	private static void EnsureMovementQueries()
	{
		if (movementQueries != null)
		{
			return;
		}
		movementQueries = new IntPtr[JobsUtility.ThreadIndexCount];
		try
		{
			for (int i = 0; i < movementQueries.Length; i++)
			{
				movementQueries[i] = RecastWrapper.CreateMovementQuery();
				if (movementQueries[i] == IntPtr.Zero)
				{
					throw new OutOfMemoryException("Could not allocate movement query scratch storage.");
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

	private unsafe static void CalculatePathFollowing(ref MovementTransformUpdate update, IntPtr query, Quaternion currentRotation, float dt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		ref PathFollowingUpdate path = ref update.path;
		path.speed.current = path.speed.Advance(dt, out var _);
		Vector3 val = path.waypoint - path.position;
		val.y = 0f;
		float magnitude = val.magnitude;
		float num = path.speed.current * dt;
		if ((path.flags & 2) != 0)
		{
			num = Mathf.Clamp(num, 0f - magnitude, magnitude);
		}
		Vector3 val2 = ((magnitude > 1E-05f) ? (val * (num / magnitude)) : Vector3.zero);
		path.velocity = val2 / dt;
		bool flag = (path.flags & 1) != 0;
		if (flag)
		{
			if (path.deviation != 0f && Mathf.Abs(path.speed.remaining - Vector3.Distance(path.position, path.destination)) < 5f)
			{
				val2 = float3.op_Implicit(math.mul(quaternion.AxisAngle(math.up(), path.deviation * (MathF.PI / 180f)), float3.op_Implicit(val2)));
			}
			float num2 = (((path.flags & 4) != 0) ? Mathx.RemapValClamped(path.speed.remaining, path.turnRadius * 2f, 0f, path.turnRadius, 0.001f) : path.turnRadius);
			val2 = RotateTowardsFlat(new NavVector3(((path.flags & 8) != 0) ? path.forward : (currentRotation * Vector3.forward)), new NavVector3(val2), path.speed.current / num2 * dt).Value * val2.magnitude;
		}
		Vector3 val3 = path.position + val2;
		Vector3 position = path.position;
		Vector3 optimizeTarget = path.optimizeTarget;
		path.moved = RecastWrapper.CorridorMoveWithQuery(path.navmesh, path.corridor, query, &val3, ((path.flags & 0x10) != 0) ? (&optimizeTarget) : null, 20f, &position, ((path.flags & 0x20) != 0) ? 1 : 0);
		if (path.moved != 0)
		{
			path.position = position;
		}
		update.position = path.position + Vector3.up * path.baseOffset;
		val2.y = 0f;
		if ((update.flags & 2) != 0 && val2.sqrMagnitude > 1E-06f)
		{
			update.rotation = quaternion.op_Implicit(quaternion.LookRotationSafe(float3.op_Implicit(val2), math.up()));
			if (flag)
			{
				path.forward = val2.normalized;
			}
			else
			{
				update.flags |= 4;
			}
		}
		else
		{
			update.flags &= 253;
		}
	}

	private void ApplyPathFollowingResult(in MovementTransformUpdate update)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		currentSpeed = update.path.speed.current;
		_nextPositionNS = new NavVector3(update.path.position);
		previousPositionNS = _nextPositionNS;
		_velocityNS = new NavVector3(update.path.velocity);
		hasPendingOptimize = false;
		hasCachedSteeringForward = (update.path.flags & 1) != 0 && (update.flags & 2) != 0;
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
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!wasPaused && pausingSources.Count > 0)
		{
			ResetPath();
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				((Behaviour)agent).enabled = false;
			}
		}
		else if (wasPaused && pausingSources.Count == 0)
		{
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
		if (letUnityMoveAgentIfPossible || !canSteer)
		{
			return stoppingDistance;
		}
		return Mathf.Max(maxTurnRadius, stoppingDistance);
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
			float num2 = (_autoBraking ? Mathx.RemapValClamped(num, maxTurnRadius * 2f, 0f, maxTurnRadius, 0.001f) : maxTurnRadius);
			float num3 = currentSpeed / num2;
			return RotateTowardsFlat(hasCachedSteeringForward ? cachedSteeringForwardNS : forward, targetNS, num3 * Time.deltaTime) * targetNS.magnitude;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.CalculateSwimmingWorldPosition"))
		{
			Vector3 val = NavToWorldSpace(_nextPositionNS);
			WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(val, waves: false, volumes: false);
			if (IsSwimming = waterInfo.currentDepth > desiredSwimDepth.Value)
			{
				val.y = ((Component)baseEntity).transform.position.y;
				val.y = Mathf.MoveTowards(val.y, waterInfo.surfaceLevel - desiredSwimDepth.Value, 1f * Time.deltaTime);
				val.y = Mathf.Max(val.y, waterInfo.terrainHeight);
			}
			return val;
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

	private static void BeginTransformBatch(int count)
	{
		transformBatchActive = false;
		if (!AI.batch_navmesh_transforms || AI.useUnityNavmesh || count < 32)
		{
			return;
		}
		int num = 0;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			RustNavMeshAgent rustNavMeshAgent = enabledComponents[num2];
			if (rustNavMeshAgent != null && rustNavMeshAgent.followingPath && !rustNavMeshAgent._isStopped && rustNavMeshAgent.pausingSources.Count == 0)
			{
				if (rustNavMeshAgent.canOpenDoors)
				{
					num = 0;
				}
				else if ((rustNavMeshAgent._updatePosition || rustNavMeshAgent._updateRotation) && ++num >= 32)
				{
					transformBatchActive = true;
					break;
				}
			}
		}
	}

	private bool TryQueueTransformUpdate(Quaternion? rotation, bool interpolate)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!transformBatchActive || transformBatchAgent != this)
		{
			return false;
		}
		if (canOpenDoors || hasPendingTransform)
		{
			FlushTransformBatch();
			return false;
		}
		byte b = (byte)(_updatePosition ? 1 : 0);
		if (rotation.HasValue)
		{
			b |= 2;
		}
		if (interpolate)
		{
			b |= 4;
		}
		if (b == 0)
		{
			return true;
		}
		EnsureMovementCapacity();
		movementUpdates[pendingTransformUpdates] = new MovementTransformUpdate
		{
			position = (_updatePosition ? GetMovementWorldPosition() : default(Vector3)),
			rotation = rotation.GetValueOrDefault(),
			maxDegreesDelta = _angularSpeed * Time.deltaTime,
			flags = b
		};
		pendingTransformAgents[pendingTransformUpdates++] = this;
		hasPendingTransform = true;
		return true;
	}

	private static void EnsureMovementCapacity()
	{
		if (movementUpdates == null || pendingTransformUpdates == movementUpdates.Length)
		{
			int newSize = Mathf.NextPowerOfTwo(pendingTransformUpdates + 32);
			Array.Resize(ref movementUpdates, newSize);
			Array.Resize(ref pendingTransformAgents, newSize);
			Array.Resize(ref registeredTransforms, newSize);
		}
	}

	private static void FlushTransformBatch()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		int num = pendingTransformUpdates;
		if (num == 0)
		{
			return;
		}
		using (TimeWarning.New("RustNavMeshAgent.TransformBatch"))
		{
			try
			{
				if (pendingPathFollowing)
				{
					EnsureMovementQueries();
				}
				if (num < 32)
				{
					for (int i = 0; i < num; i++)
					{
						RustNavMeshAgent rustNavMeshAgent = pendingTransformAgents[i];
						if (ObjectEx.IsUnityNull(rustNavMeshAgent))
						{
							continue;
						}
						MovementTransformUpdate update = movementUpdates[i];
						Transform agentTransform = rustNavMeshAgent.AgentTransform;
						if ((update.flags & 8) != 0)
						{
							CalculatePathFollowing(ref update, movementQueries[0], agentTransform.rotation, Time.deltaTime);
							movementUpdates[i] = update;
						}
						Quaternion val = update.rotation;
						if ((update.flags & 4) != 0)
						{
							val = Quaternion.RotateTowards(agentTransform.rotation, val, update.maxDegreesDelta);
						}
						if ((update.flags & 1) != 0)
						{
							if ((update.flags & 2) != 0)
							{
								agentTransform.SetPositionAndRotation(update.position, val);
							}
							else
							{
								agentTransform.position = update.position;
							}
						}
						else if ((update.flags & 2) != 0)
						{
							agentTransform.rotation = val;
						}
					}
				}
				else
				{
					PrepareMovementTransforms(num);
					RunMovementTransformJob(num);
				}
			}
			finally
			{
				pendingTransformUpdates = 0;
				pendingPathFollowing = false;
				for (int j = 0; j < num; j++)
				{
					RustNavMeshAgent rustNavMeshAgent2 = pendingTransformAgents[j];
					rustNavMeshAgent2.hasPendingTransform = false;
					if ((movementUpdates[j].flags & 8) != 0 && movementUpdates[j].path.moved >= 0 && !ObjectEx.IsUnityNull(rustNavMeshAgent2))
					{
						rustNavMeshAgent2.ApplyPathFollowingResult(in movementUpdates[j]);
					}
					pendingTransformAgents[j] = null;
				}
			}
		}
	}

	private static void PrepareMovementTransforms(int count)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!movementTransforms.isCreated)
		{
			movementTransforms = new TransformAccessArray(Mathf.NextPowerOfTwo(count), -1);
		}
		int length = movementTransforms.length;
		for (int num = length - 1; num >= count; num--)
		{
			movementTransforms.RemoveAtSwapBack(num);
			registeredTransforms[num] = null;
		}
		for (int i = 0; i < count; i++)
		{
			RustNavMeshAgent rustNavMeshAgent = pendingTransformAgents[i];
			Transform val = (ObjectEx.IsUnityNull(rustNavMeshAgent) ? null : rustNavMeshAgent.AgentTransform);
			if (i >= length)
			{
				movementTransforms.Add(val);
			}
			else if (registeredTransforms[i] != val)
			{
				movementTransforms[i] = val;
			}
			registeredTransforms[i] = val;
		}
	}

	private unsafe static void RunMovementTransformJob(int count)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		fixed (MovementTransformUpdate* ptr = movementUpdates)
		{
			fixed (IntPtr* ptr2 = movementQueries)
			{
				NativeArray<MovementTransformUpdate> updates = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<MovementTransformUpdate>((void*)ptr, count, (Allocator)1);
				NativeArray<IntPtr> queries = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<IntPtr>((void*)ptr2, movementQueries?.Length ?? 0, (Allocator)1);
				MovementTransformJob movementTransformJob = new MovementTransformJob
				{
					updates = updates,
					queries = queries,
					deltaTime = Time.deltaTime
				};
				TransformAccessArray val = movementTransforms;
				JobHandle val2 = default;
				val2 = IJobParallelForTransformExtensions.Schedule<MovementTransformJob>(movementTransformJob, val, val2);
				val2.Complete();
			}
		}
	}

	private static void DisposeTransformBatch()
	{
		FlushTransformBatch();
		if (movementTransforms.isCreated)
		{
			movementTransforms.Dispose();
		}
		DisposeMovementQueries();
		movementUpdates = null;
		pendingTransformAgents = null;
		registeredTransforms = null;
	}

	public RustNavMeshAgent()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
	}
}
