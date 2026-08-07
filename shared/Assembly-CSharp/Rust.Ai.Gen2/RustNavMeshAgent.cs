using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class RustNavMeshAgent : EntityComponent<BaseEntity>, IServerComponent
{
	public enum Speeds
	{
		Sneak,
		Walk,
		Jog,
		Run,
		Sprint,
		FullSprint
	}

	public bool letUnityMoveAgentIfPossible = true;

	private static ListHashSet<RustNavMeshAgent> enabledComponents = new ListHashSet<RustNavMeshAgent>();

	private static RustNavMeshPath tempPath = new RustNavMeshPath();

	private RustNavMeshPath CurPathNS = new RustNavMeshPath();

	private NavMeshAgent _agent;

	private Vector3 previousPositionNS;

	private int? curWaypointIndex;

	[NonSerialized]
	public readonly List<Vector3> lastValidPath = new List<Vector3>();

	private bool isScientist;

	private SenseComponent senses;

	[Header("Base fields")]
	[SerializeField]
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

	private Vector3 _velocityNS = Vector3.zero;

	private Vector3 _nextPositionNS = Vector3.zero;

	public Vector3? overrideDirectionWS;

	private ulong currentPolyRef;

	[Header("Doors")]
	public bool canOpenDoors;

	private HashSet<object> pausingSources = new HashSet<object>();

	[Header("Movement speed")]
	public float sneakSpeed = 0.6f;

	public float walkSpeed = 0.89f;

	public float jogSpeed = 2.45f;

	public float runSpeed = 4.4f;

	public float sprintSpeed = 6f;

	public float fullSprintSpeed = 9f;

	public ResettableFloat deceleration = new ResettableFloat(2f);

	public float emergencyDeceleration = 10f;

	private float currentSpeed;

	private float dampVelocity;

	[Header("Steering")]
	public bool canSteer = true;

	public float maxTurnRadius = 2f;

	[NonSerialized]
	public float currentDeviation;

	[Header("Swimming")]
	public bool canSwim;

	public float swimSpeed = 0.6f;

	public float swimSprintSpeed = 0.89f;

	public ResettableFloat desiredSwimDepth = new ResettableFloat(0.7f);

	[Header("Terrain Preferences")]
	public Enum preferedTopology = (Enum)537002081;

	public Enum preferedBiome = (Enum)15;

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
			return default(OffMeshLinkData);
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

	public Vector3 nextPosition
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.nextPosition;
			}
			return _nextPositionNS;
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

	public Vector3 steeringTarget
	{
		get
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return Vector3.zero;
				}
				return agent.steeringTarget;
			}
			return CurPathNS.corners[curWaypointIndex.Value];
		}
	}

	public Vector3 velocity
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return Vector3.zero;
				}
				return agent.velocity;
			}
			return _velocityNS;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			_velocityNS = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.velocity = value;
			}
			if (!AI.useUnityNavmesh && AI.logIssues)
			{
				RustNavigation.LogError("Setting velocity on RustNavMeshAgent has no effect when not using Unity NavMesh. Use Move() to move the agent instead.");
			}
		}
	}

	public Vector3 desiredVelocity
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return Vector3.zero;
				}
				return agent.desiredVelocity;
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
			return curWaypointIndex.HasValue;
		}
	}

	public Vector3 destination
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return Vector3.zero;
				}
				return agent.destination;
			}
			if (!hasPath)
			{
				return _nextPositionNS;
			}
			return CurPathNS.GetDestinationNS();
		}
		set
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (AI.useUnityNavmesh)
			{
				if (TryGetAgent(out var agent))
				{
					agent.destination = value;
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
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
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
				if (!NavMeshHelpers.CalculateRemainingPathLength(CurPathNS.corners, _nextPositionNS, curWaypointIndex.Value, out var length))
				{
					return 0f;
				}
				return length;
			}
		}
	}

	public bool isOnNavMesh
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
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
				NavMeshHit hitNS;
				return SamplePosition(_nextPositionNS, out hitNS, 1f, debugDraw: false);
			}
		}
	}

	public bool IsJumping
	{
		get
		{
			return base.baseEntity.HasFlag(BaseEntity.Flags.Reserved2);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved2, value);
		}
	}

	public bool IsPaused
	{
		get
		{
			bool wasPaused = pausingSources.Count > 0;
			foreach (object pausingSource in pausingSources)
			{
				if (ObjectEx.IsUnityNull(pausingSource))
				{
					if (AI.logIssues)
					{
						RustNavigation.LogError("Removing null pausing source from " + ((Object)base.baseEntity).name + ".");
					}
					pausingSources.Remove(pausingSource);
				}
			}
			OnChange(wasPaused);
			return pausingSources.Count > 0;
		}
	}

	public bool IsSprinting => currentSpeed >= sprintSpeed;

	public bool IsSwimming
	{
		get
		{
			return base.baseEntity.HasFlag(BaseEntity.Flags.Reserved1);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
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
				BaseEntity obj = base.baseEntity;
				RustNavigation.LogError("Entity " + (((obj != null) ? ((Object)obj).name : null) ?? "unknown entity") + " has RustNavMeshAgent but no NavMeshAgent component. This component will not function correctly without a NavMeshAgent.");
			}
			return false;
		}
		agent = _agent;
		return true;
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

	private bool SamplePositionInternal(Vector3 positionNS, out NavMeshHit hitNS, float maxDistance, out ulong nearestPolyRef)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SamplePositionInternal"))
		{
			hitNS = default(NavMeshHit);
			nearestPolyRef = 0uL;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				return RustNavMeshHelpers.SamplePosition(positionNS, out hitNS, maxDistance, agent.areaMask);
			}
			if (HasValidIndependantNavmesh)
			{
				return independantNavmesh.Navmesh.SamplePositionPoly(positionNS, out hitNS, maxDistance * Vector3.one, out nearestPolyRef);
			}
			if (RustNavigation.Instance.IsDefaultNavmeshBuilt())
			{
				return RustNavigation.Instance.DefaultNavmesh.SamplePositionPoly(positionNS, out hitNS, maxDistance * Vector3.one, out nearestPolyRef);
			}
			return false;
		}
	}

	public bool SamplePosition(Vector3 positionNS, out NavMeshHit hitNS, float maxDistance, bool debugDraw = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ulong nearestPolyRef;
		return SamplePositionPoly(positionNS, out hitNS, maxDistance, out nearestPolyRef, debugDraw);
	}

	public bool SamplePositionPoly(Vector3 positionNS, out NavMeshHit hitNS, float maxDistance, out ulong nearestPolyRef, bool debugDraw = true)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.SamplePosition"))
		{
			bool num = SamplePositionInternal(positionNS, out hitNS, maxDistance, out nearestPolyRef);
			float num2 = 1f;
			if (num && maxDistance > num2 && Mathf.Abs(((NavMeshHit)(ref hitNS)).position.y - positionNS.y) > num2 && SampleGroundPositionWithPhysics(positionNS, out var hitInfoNS, 3.5f) && SamplePositionInternal(((RaycastHit)(ref hitInfoNS)).point, out var hitNS2, num2, out nearestPolyRef))
			{
				hitNS = hitNS2;
			}
			if (!num)
			{
				return false;
			}
			return true;
		}
	}

	public bool Raycast(Vector3 startNS, Vector3 endNS, out NavMeshHit hitNS)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Raycast"))
		{
			hitNS = default(NavMeshHit);
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				return RustNavMeshHelpers.Raycast(startNS, endNS, out hitNS, agent.areaMask);
			}
			if (HasValidIndependantNavmesh)
			{
				return independantNavmesh.Navmesh.Raycast(startNS, endNS, out hitNS);
			}
			if (!RustNavigation.Instance.IsDefaultNavmeshBuilt())
			{
				if (AI.logIssues)
				{
					BaseEntity obj = base.baseEntity;
					RustNavigation.LogError(string.Format("Default navmesh is not built, cannot raycast for {0} from {1} to {2}", ((obj != null) ? ((Object)obj).name : null) ?? "unknown entity", startNS, endNS));
				}
				return false;
			}
			return RustNavigation.Instance.DefaultNavmesh.Raycast(startNS, endNS, out hitNS);
		}
	}

	public bool CalculatePath(Vector3 startNS, Vector3 endNS, RustNavMeshPath pathNS)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
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
				flag = RustNavMeshHelpers.CalculatePath(startNS, endNS, agent.areaMask, pathNS);
			}
			else if (HasValidIndependantNavmesh)
			{
				flag = independantNavmesh.Navmesh.CalculatePath(startNS, endNS, pathNS);
			}
			else if (RustNavigation.Instance.IsDefaultNavmeshBuilt())
			{
				flag = RustNavigation.Instance.DefaultNavmesh.CalculatePath(startNS, endNS, pathNS);
			}
			else
			{
				flag = false;
				if (AI.logIssues)
				{
					BaseEntity obj = base.baseEntity;
					RustNavigation.LogError(string.Format("Default navmesh is not built, cannot calculate path for {0} from {1} to {2}", ((obj != null) ? ((Object)obj).name : null) ?? "unknown entity", startNS, endNS));
				}
			}
			if (flag)
			{
				_ = pathNS.status;
			}
			return flag;
		}
	}

	public bool CanReach(Vector3 locationNS, bool updateLastValidPath = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		using (TimeWarning.New("RustNavMeshAgent.CanReach"))
		{
			if (!IsPositionOnNavmesh(locationNS, out var hitNS))
			{
				return false;
			}
			if (!CalculatePath(((NavMeshHit)(ref hitNS)).position, tempPath))
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

	public bool SetDestinationWithParams(Vector3 targetPositionNS, bool autoBraking = true, Speeds? gait = null, float? acceleration = null, float? deceleration = null, float? deviation = null, float? swimDepth = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (!SetDestination(targetPositionNS))
		{
			return false;
		}
		this.autoBraking = autoBraking;
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
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		enabledComponents.TryAdd(this);
		isScientist = BaseNetworkableEx.Is<ScientistNPC2>((Object)(object)base.baseEntity, out ScientistNPC2 _);
		((Component)this).TryGetComponent<SenseComponent>(ref senses);
		_nextPositionNS = ((Component)this).transform.position;
		if (AI.useUnityNavmesh)
		{
			Matrix4x4 worldToNavMeshSpace = base.baseEntity.WorldToNavMeshSpace;
			_nextPositionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint3x4(((Component)this).transform.position);
		}
		else
		{
			independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(((Component)this).transform.position);
			if (HasValidIndependantNavmesh)
			{
				_nextPositionNS = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(((Component)this).transform.position);
			}
		}
		if (SamplePositionPoly(_nextPositionNS, out var hitNS, 10f, out currentPolyRef))
		{
			_nextPositionNS = ((NavMeshHit)(ref hitNS)).position;
		}
		previousPositionNS = _nextPositionNS;
		TrySyncWorldPosWithNavPos();
	}

	private void OnDisable()
	{
		enabledComponents.Remove(this);
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			((Behaviour)agent).enabled = false;
		}
		if (!AI.useUnityNavmesh)
		{
			ResetPath();
		}
	}

	public static void TickEnabledComponents()
	{
		for (int num = enabledComponents.Count - 1; num >= 0; num--)
		{
			RustNavMeshAgent rustNavMeshAgent = enabledComponents[num];
			if (ObjectEx.IsUnityNull(rustNavMeshAgent) || !rustNavMeshAgent.baseEntity.IsValid())
			{
				enabledComponents.RemoveAt(num);
			}
			else
			{
				rustNavMeshAgent.Tick(Time.deltaTime);
			}
		}
	}

	private void Tick(float deltaTime)
	{
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
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
				if (updateRotation)
				{
					if (overrideDirectionWS.HasValue)
					{
						((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, Quaternion.LookRotation(overrideDirectionWS.Value), _angularSpeed * deltaTime);
					}
					else if (isScientist && (Object)(object)senses != (Object)null)
					{
						Transform transform = ((Component)this).transform;
						Matrix4x4 eyeTransform = senses.GetEyeTransform();
						transform.rotation = Quaternion.LookRotation(Vector3Ex.NormalizeXZ(((Matrix4x4)(ref eyeTransform)).rotation * Vector3.forward));
					}
				}
				if (!hasPath || IsPaused || _isStopped)
				{
					float num = currentSpeed;
					Vector3 val = _nextPositionNS - previousPositionNS;
					currentSpeed = Mathf.SmoothDamp(num, ((Vector3)(ref val)).magnitude / deltaTime, ref dampVelocity, 1f, 9999f, Time.smoothDeltaTime * 10f);
					return;
				}
				float num2 = ((canSteer && _autoBraking) ? maxTurnRadius : _stoppingDistance);
				Vector3 val2;
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent2))
					{
						return;
					}
					val2 = agent2.steeringTarget;
					if (remainingDistance <= num2)
					{
						ResetPath();
						return;
					}
				}
				else
				{
					bool reachedEnd = false;
					if (!NavMeshHelpers.FindNextWayPointIndex(CurPathNS.corners, _nextPositionNS, curWaypointIndex.Value, out var newWaypointIndex, out reachedEnd, num2) || reachedEnd)
					{
						ResetPath();
						return;
					}
					curWaypointIndex = newWaypointIndex;
					val2 = CurPathNS.corners[curWaypointIndex.Value];
				}
				Vector3 val3 = val2 - _nextPositionNS;
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				AdjustCurrentSpeedFromDesiredSpeed(_nextPositionNS, _autoBraking, remainingDistance, num2, deltaTime);
				Vector3 val4 = normalized * (currentSpeed * deltaTime);
				val4 = Vector3.ClampMagnitude(val4, ((Vector3)(ref val3)).magnitude);
				Move(val4);
				_velocityNS = val4 / deltaTime;
			}
		}
		finally
		{
			previousPositionNS = _nextPositionNS;
		}
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

	public bool FindClosestEdge(out NavMeshHit hitNS)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			return agent.FindClosestEdge(ref hitNS);
		}
		hitNS = default(NavMeshHit);
		return false;
	}

	public bool SetDestination(Vector3 targetPositionNS)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
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
					return agent.SetDestination(targetPositionNS);
				}
			}
			if (hasPath && CurPathNS.GetDestinationNS() == targetPositionNS)
			{
				return true;
			}
			if (!CalculatePath(_nextPositionNS, targetPositionNS, tempPath) || (int)tempPath.status != 0)
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
				RustNavigation.LogError($"{base.baseEntity} is not on navmesh at {((Component)base.baseEntity).transform.position} in {MapHelper.PositionToString(((Component)base.baseEntity).transform.position)}");
			}
			return false;
		}
		return true;
	}

	public bool SetPath(RustNavMeshPath pathNS)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
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
			CurPathNS.CopyFrom(pathNS);
			curWaypointIndex = 1;
			_isStopped = false;
			lastValidPath.Clear();
			lastValidPath.AddRange(CurPathNS.corners);
			return true;
		}
	}

	public void Move(Vector3 deltaNS)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Move"))
		{
			if (AI.useUnityNavmesh && letUnityMoveAgentIfPossible)
			{
				if (TryGetAgent(out var agent) && TryEnableAgent(agent))
				{
					agent.Move(deltaNS);
				}
				return;
			}
			if (canSteer && !IsPaused)
			{
				deltaNS = AdjustMovementForSteering(deltaNS);
			}
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent2) || !TryEnableAgent(agent2))
				{
					return;
				}
				agent2.Move(deltaNS);
				_nextPositionNS = agent2.nextPosition;
			}
			else
			{
				if (currentPolyRef == 0L)
				{
					RustNavigation.LogError("RustNavMeshAgent.Move called with no currentPolyRef. This should never happen.");
				}
				if (HasValidIndependantNavmesh)
				{
					independantNavmesh.Navmesh.Move(currentPolyRef, _nextPositionNS, _nextPositionNS + deltaNS, out currentPolyRef, out _nextPositionNS);
				}
				else if (RustNavigation.Instance.IsDefaultNavmeshBuilt())
				{
					RustNavigation.Instance.DefaultNavmesh.Move(currentPolyRef, _nextPositionNS, _nextPositionNS + deltaNS, out currentPolyRef, out _nextPositionNS);
				}
			}
			TrySyncWorldPosWithNavPos();
			Vector3 val = Vector3Ex.XZ3D(deltaNS);
			if (_updateRotation && !IsPaused && !overrideDirectionWS.HasValue && ((Vector3)(ref val)).magnitude > 0.001f)
			{
				Vector3 val2 = val;
				if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
				{
					val2 = independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(val);
				}
				if (canSteer)
				{
					((Component)this).transform.rotation = Quaternion.LookRotation(val2);
				}
				else
				{
					((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, Quaternion.LookRotation(val2), _angularSpeed * Time.deltaTime);
				}
			}
			TryOpenDoors();
		}
	}

	private bool TrySyncWorldPosWithNavPos()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.TrySyncWorldPosWithNavPos"))
		{
			if (!_updatePosition)
			{
				return false;
			}
			if (canSwim)
			{
				((Component)this).transform.position = CalculateSwimmingWorldPosition();
			}
			else if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
			{
				((Component)this).transform.position = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(_nextPositionNS + Vector3.up * _baseOffset);
			}
			else if (AI.useUnityNavmesh)
			{
				Transform transform = ((Component)this).transform;
				Matrix4x4 navMeshToWorldSpace = base.baseEntity.NavMeshToWorldSpace;
				transform.position = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint3x4(_nextPositionNS + Vector3.up * _baseOffset);
			}
			else
			{
				((Component)this).transform.position = _nextPositionNS + Vector3.up * _baseOffset;
			}
			return true;
		}
	}

	public bool Warp(Vector3 newPositionNS)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavMeshAgent.Warp"))
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
				return agent.Warp(newPositionNS);
			}
			if (!SamplePositionPoly(newPositionNS, out var hitNS, 1f, out var nearestPolyRef))
			{
				return false;
			}
			currentPolyRef = nearestPolyRef;
			_nextPositionNS = ((NavMeshHit)(ref hitNS)).position;
			TrySyncWorldPosWithNavPos();
			return true;
		}
	}

	public bool CalculatePath(Vector3 targetPositionNS, RustNavMeshPath pathNS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return CalculatePath(_nextPositionNS, targetPositionNS, pathNS);
	}

	public bool Raycast(Vector3 targetPositionNS, out NavMeshHit hitNS)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Raycast(_nextPositionNS, targetPositionNS, out hitNS);
	}

	public bool IsPositionOnNavmesh(Vector3 positionNS, out NavMeshHit hitNS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return SamplePosition(positionNS, out hitNS, 2f);
	}

	public void ResetPath()
	{
		using (TimeWarning.New("RustNavMeshAgent.ResetPath"))
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.ResetPath();
			}
			CurPathNS.Reset();
			curWaypointIndex = null;
			autoBraking = true;
			_desiredSpeed = 0f;
			if (!letUnityMoveAgentIfPossible)
			{
				_acceleration.Reset();
				deceleration.Reset();
			}
			currentDeviation = 0f;
		}
	}

	public bool TryOpenDoors()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!canOpenDoors)
		{
			return false;
		}
		bool result = false;
		PooledList<NPCDoorTriggerBox> val = Pool.Get<PooledList<NPCDoorTriggerBox>>();
		try
		{
			NPCDoorTriggerBox.AllDoors.GetNeighboors(((Component)base.baseEntity).transform.position, (List<NPCDoorTriggerBox>)(object)val);
			foreach (NPCDoorTriggerBox item in (List<NPCDoorTriggerBox>)(object)val)
			{
				item.TryOpenDoorFor(base.baseEntity);
				result = true;
			}
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
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
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
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
			Matrix4x4 worldToNavMeshSpace = base.baseEntity.WorldToNavMeshSpace;
			bool flag = Warp(((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint3x4(((Component)this).transform.position));
			if (AI.logIssues && !flag)
			{
				Debug.LogError((object)$"Failed to reproject {((Object)base.baseEntity).name} to current position {((Component)this).transform.position} after unpausing.", (Object)(object)this);
			}
		}
	}

	private static float GetBrakingDistance(float speed, float brakingDeceleration)
	{
		float num = speed / Mathf.Max(brakingDeceleration, 0.001f);
		return 0.5f * brakingDeceleration * num * num;
	}

	public bool IsSprintingOnClient(float speed)
	{
		return speed >= sprintSpeed;
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

	public void SetGait(Speeds gait)
	{
		speed = GetSpeedForGait(gait);
	}

	public void SetSpeedRatio(float ratio, Speeds minSpeed = Speeds.Sneak, Speeds maxSpeed = Speeds.Sprint, int offset = 0)
	{
		int num = Mathf.FloorToInt(Mathf.Lerp((float)minSpeed, (float)maxSpeed, ratio));
		num = Mathf.Clamp(num + offset, (int)minSpeed, (int)maxSpeed);
		SetGait((Speeds)num);
	}

	public void AdjustCurrentSpeedFromDesiredSpeed(Vector3 position, bool shouldStopAtDestination, float remainingDistance, float stoppingDistance, float dt)
	{
		float num = AdjustDesiredSpeedWhenSwimming(_desiredSpeed, sprintSpeed);
		if (shouldStopAtDestination && remainingDistance - stoppingDistance < GetBrakingDistance(currentSpeed, deceleration.Value))
		{
			currentSpeed = Mathf.Max(1f, currentSpeed - deceleration.Value * dt);
		}
		else if (currentSpeed > num)
		{
			float num2 = (currentSpeed - num) / deceleration.Value;
			float num3 = ((currentSpeed > walkSpeed && num2 > 1f) ? emergencyDeceleration : deceleration.Value);
			currentSpeed = Mathf.Max(num, currentSpeed - num3 * dt);
		}
		else if (currentSpeed < num)
		{
			currentSpeed = Mathf.Min(num, currentSpeed + _acceleration.Value * dt);
		}
	}

	public Vector3 AdjustMovementForSteering(Vector3 deltaNS)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AdjustMovementForSteering"))
		{
			Vector3 val = deltaNS;
			float num = remainingDistance;
			if (Mathf.Abs(num - Vector3.Distance(_nextPositionNS, CurPathNS.GetDestinationNS())) < 5f)
			{
				val = Quaternion.AngleAxis(currentDeviation, Vector3.up) * deltaNS;
			}
			float num2 = (_autoBraking ? Mathx.RemapValClamped(num, maxTurnRadius * 2f, 0f, maxTurnRadius, 0.001f) : maxTurnRadius);
			float num3 = currentSpeed / num2;
			Vector3 val2 = ((Component)base.baseEntity).transform.forward;
			if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
			{
				val2 = independantNavmesh.TransformDirectionFromWorldSpaceToNavSpace(((Component)base.baseEntity).transform.forward);
			}
			return Vector3.RotateTowards(val2, val, num3 * Time.deltaTime, 0f) * ((Vector3)(ref val)).magnitude;
		}
	}

	public Vector3 CalculateSwimmingWorldPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = _nextPositionNS;
		if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
		{
			val = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(_nextPositionNS);
		}
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(val, waves: false, volumes: false);
		if (IsSwimming = waterInfo.currentDepth > desiredSwimDepth.Value)
		{
			val.y = ((Component)base.baseEntity).transform.position.y;
			val.y = Mathf.MoveTowards(val.y, waterInfo.surfaceLevel - desiredSwimDepth.Value, 1f * Time.deltaTime);
			val.y = Mathf.Max(val.y, waterInfo.terrainHeight);
		}
		return val;
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

	public bool IsPositionOnFavoredTerrain(Vector3 position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsPositionOnFavoredTerrain"))
		{
			return IsPositionAtTopologyRequirement(position, preferedTopology) && IsPositionABiomeRequirement(position, preferedBiome);
		}
	}

	public bool IsPositionAtTopologyRequirement(Vector3 position, Enum topologyRequirement)
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
			Enum val = (Enum)TerrainMeta.TopologyMap.GetTopology(position);
			if ((topologyRequirement & val) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsPositionABiomeRequirement(Vector3 position, Enum biomeRequirement)
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
			Enum val = (Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position);
			if ((biomeRequirement & val) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsInWater(Vector3 position)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("IsInWater"))
		{
			if ((Object)(object)base.baseEntity.GetParentEntity() != (Object)null)
			{
				return false;
			}
			if (WaterLevel.GetWaterDepth(position, waves: false, volumes: false) >= 0.3f)
			{
				return true;
			}
			return false;
		}
	}

	public bool SamplePositionRobust(RustNavMeshAgent agent, Vector3 position, out Vector3 sample, float maxDistance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SamplePositionRobust"))
		{
			sample = position;
			NavMeshHit hitNS;
			bool num = agent.SamplePosition(position, out hitNS, maxDistance);
			float num2 = 1f;
			if (num && maxDistance > num2 && Mathf.Abs(((NavMeshHit)(ref hitNS)).position.y - position.y) > num2 && SampleGroundPositionWithPhysics(position, out var hitInfoNS, 3.5f) && agent.SamplePosition(((RaycastHit)(ref hitInfoNS)).point, out var hitNS2, num2))
			{
				hitNS = hitNS2;
			}
			if (!num)
			{
				return false;
			}
			sample = ((NavMeshHit)(ref hitNS)).position;
			return ((NavMeshHit)(ref hitNS)).hit;
		}
	}

	public bool SampleGroundPositionWithPhysics(Vector3 positionNS, out RaycastHit hitInfoNS, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SampleGroundPositionWithPhysics"))
		{
			Vector3 val = positionNS;
			if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
			{
				val = independantNavmesh.TransformPointFromNavSpaceToWorldSpace(positionNS);
			}
			Vector3 val2 = val + Vector3.up * radius * 1.5f;
			float maxDistance2 = maxDistance + radius * 1.5f;
			RaycastHit hitInfo;
			bool num = GamePhysics.TraceRealm(GamePhysics.Realm.Server, new Ray(val2, Vector3.down), radius, out hitInfo, maxDistance2, layerMask, (QueryTriggerInteraction)1);
			hitInfoNS = hitInfo;
			if (!num)
			{
				((RaycastHit)(ref hitInfoNS)).point = positionNS;
				return false;
			}
			if (radius > 0f && ((RaycastHit)(ref hitInfoNS)).distance <= 0f)
			{
				((RaycastHit)(ref hitInfoNS)).point = positionNS;
			}
			else if (!AI.useUnityNavmesh && HasValidIndependantNavmesh)
			{
				((RaycastHit)(ref hitInfoNS)).point = independantNavmesh.TransformPointFromWorldSpaceToNavSpace(((RaycastHit)(ref hitInfo)).point);
				((RaycastHit)(ref hitInfoNS)).normal = independantNavmesh.TransformDirectionFromWorldSpaceToNavSpace(((RaycastHit)(ref hitInfo)).normal);
			}
			return true;
		}
	}
}
