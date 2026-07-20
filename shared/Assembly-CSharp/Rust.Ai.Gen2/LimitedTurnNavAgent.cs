using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(RustNavMeshAgent))]
public class LimitedTurnNavAgent : EntityComponent<BaseEntity>
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

	public enum SteeringMode
	{
		FaceTarget,
		LimitedTurnRate
	}

	[SerializeField]
	private RustNavMeshAgent agent;

	[SerializeField]
	private SteeringMode steeringMode = SteeringMode.LimitedTurnRate;

	[Header("Speed")]
	[SerializeField]
	private float sneakSpeed = 0.6f;

	[SerializeField]
	private float walkSpeed = 0.89f;

	[SerializeField]
	private float jogSpeed = 2.45f;

	[SerializeField]
	private float runSpeed = 4.4f;

	[SerializeField]
	private float sprintSpeed = 6f;

	[SerializeField]
	private float fullSprintSpeed = 9f;

	[SerializeField]
	public bool canSwim;

	[SerializeField]
	private float swimSpeed = 0.6f;

	[SerializeField]
	private float swimSprintSpeed = 0.89f;

	public ResettableFloat desiredSwimDepth = new ResettableFloat(0.7f);

	public ResettableFloat acceleration = new ResettableFloat(10f);

	public ResettableFloat deceleration = new ResettableFloat(2f);

	[SerializeField]
	private float maxTurnRadius = 2f;

	[SerializeField]
	private bool canOpenDoors;

	[SerializeField]
	private Enum preferedTopology = (Enum)537002081;

	[SerializeField]
	private Enum preferedBiome = (Enum)15;

	public const BaseEntity.Flags FLAG_IS_SWIMMING = BaseEntity.Flags.Reserved1;

	public const BaseEntity.Flags FLAG_IS_JUMPING = BaseEntity.Flags.Reserved2;

	private const float emergencyDeceleration = 10f;

	private static RustNavMeshPath path;

	[NonSerialized]
	public UnityEvent onPathFailed = new UnityEvent();

	private LockState movementLock = new LockState();

	private bool isNavMeshReady;

	private int? lastFrameCall;

	[NonSerialized]
	public float currentDeviation;

	[NonSerialized]
	public bool shouldStopAtDestination = true;

	[NonSerialized]
	public float? overrideAngularSpeed;

	private float cachedPathLength;

	private Vector3? previousLocalPosition;

	private float curSpeed;

	private float desiredSpeed;

	private static ListHashSet<LimitedTurnNavAgent> steeringComponents = new ListHashSet<LimitedTurnNavAgent>();

	private Vector3? _overrideDirection;

	public bool IsSwimming
	{
		get
		{
			return (base.baseEntity.flags & BaseEntity.Flags.Reserved1) == BaseEntity.Flags.Reserved1;
		}
		private set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved1, value);
		}
	}

	public bool IsJumping
	{
		get
		{
			return (base.baseEntity.flags & BaseEntity.Flags.Reserved2) == BaseEntity.Flags.Reserved2;
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved2, value);
		}
	}

	public Vector3 NavPosition => agent.nextPosition;

	public bool IsSprinting => curSpeed >= sprintSpeed;

	public bool isPaused => movementLock.IsLocked;

	public bool IsNavmeshReady => isNavMeshReady;

	public Vector3? lastValidDestination
	{
		get
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (lastValidPath.Count <= 0)
			{
				return null;
			}
			List<Vector3> list = lastValidPath;
			return list[list.Count - 1];
		}
	}

	public List<Vector3> lastValidPath { get; private set; } = new List<Vector3>();

	private float AngularSpeed
	{
		get
		{
			if (!overrideAngularSpeed.HasValue)
			{
				return agent.angularSpeed;
			}
			return overrideAngularSpeed.Value;
		}
	}

	public float RemainingDistance => agent.remainingDistance;

	public bool IsFollowingPath
	{
		get
		{
			if (agent.hasPath)
			{
				return agent.remainingDistance > (shouldStopAtDestination ? 0.05f : maxTurnRadius);
			}
			return false;
		}
	}

	public Vector3? overrideDirection
	{
		get
		{
			return _overrideDirection;
		}
		set
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			Vector3? val = value;
			Vector3? val2 = _overrideDirection;
			if (val.HasValue != val2.HasValue || (val.HasValue && !(val.GetValueOrDefault() == val2.GetValueOrDefault())))
			{
				_overrideDirection = value;
				if (base.baseEntity.isServer)
				{
					base.baseEntity.SendNetworkUpdate();
				}
			}
		}
	}

	public LockState.LockHandle Pause()
	{
		if (!movementLock.IsLocked)
		{
			OnPaused();
		}
		return movementLock.AddLock();
	}

	public bool Unpause(ref LockState.LockHandle handle)
	{
		bool result = movementLock.RemoveLock(ref handle);
		if (!movementLock.IsLocked)
		{
			OnUnpaused();
		}
		return result;
	}

	public void Move(Vector3 offset)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:Move"))
		{
			agent.Move(offset);
			lastFrameCall = Time.frameCount;
			if (canSwim)
			{
				Vector3 nextPosition = agent.nextPosition;
				WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(nextPosition, waves: false, volumes: false);
				IsSwimming = waterInfo.currentDepth > desiredSwimDepth.Value;
				if (IsSwimming)
				{
					nextPosition.y = ((Component)base.baseEntity).transform.position.y;
					nextPosition.y = Mathf.MoveTowards(nextPosition.y, waterInfo.surfaceLevel - desiredSwimDepth.Value, 1f * Time.deltaTime);
					nextPosition.y = Mathf.Max(nextPosition.y, waterInfo.terrainHeight);
					base.baseEntity.ServerNavMeshPos = nextPosition;
				}
				else
				{
					base.baseEntity.ServerNavMeshPos = agent.nextPosition;
				}
			}
			else
			{
				IsSwimming = false;
				base.baseEntity.ServerNavMeshPos = agent.nextPosition;
			}
			if (!canOpenDoors)
			{
				return;
			}
			PooledList<NPCDoorTriggerBox> val = Pool.Get<PooledList<NPCDoorTriggerBox>>();
			try
			{
				NPCDoorTriggerBox.AllDoors.GetNeighboors(((Component)base.baseEntity).transform.position, (List<NPCDoorTriggerBox>)(object)val);
				foreach (NPCDoorTriggerBox item in (List<NPCDoorTriggerBox>)(object)val)
				{
					item.TryOpenDoorFor(base.baseEntity);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void ResetPath()
	{
		using (TimeWarning.New("LimitedTurnNavAgent:ResetPath"))
		{
			shouldStopAtDestination = true;
			acceleration.Reset();
			deceleration.Reset();
			currentDeviation = 0f;
			SetSpeed(0f);
			if (agent.hasPath)
			{
				agent.ResetPath();
			}
		}
	}

	public bool CanReach(Vector3 location, bool updateLastValidPath = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:CanReach"))
		{
			if (!IsPositionOnNavmesh(location, out var sample))
			{
				DebugShowFailedPath(location);
				return false;
			}
			if (!CalculatePathCustom(sample, path))
			{
				return false;
			}
			bool num = (int)path.status == 0;
			if (!num)
			{
				DebugShowFailedPath(sample, path);
			}
			else if (updateLastValidPath)
			{
				lastValidPath.Clear();
				lastValidPath.AddRange(path.corners);
			}
			return num;
		}
	}

	public bool SetDestination(Vector3 newDestination, bool allowPartialPaths = false)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:SetDestination"))
		{
			if (shouldStopAtDestination && agent.hasPath && Vector3.Distance(agent.destination, newDestination) < 1f)
			{
				return true;
			}
			if (!CalculatePathCustom(newDestination, path))
			{
				return false;
			}
			if (!allowPartialPaths && (int)path.status != 0)
			{
				return false;
			}
			SetPath(path);
			return true;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		if (path == null)
		{
			path = new RustNavMeshPath();
		}
	}

	private void OnPaused()
	{
		if (((Behaviour)agent).enabled && agent.isOnNavMesh)
		{
			ResetPath();
		}
	}

	private void OnUnpaused()
	{
	}

	public bool SetPath(RustNavMeshPath newPath)
	{
		using (TimeWarning.New("LimitedTurnNavAgent:SetPath"))
		{
			if (!agent.SetPath(newPath))
			{
				return false;
			}
			cachedPathLength = newPath.GetPathLength();
			lastValidPath.Clear();
			lastValidPath.AddRange(newPath.corners);
			return true;
		}
	}

	private void DebugShowFailedPath(Vector3? destination, RustNavMeshPath failedPath = null)
	{
	}

	private float GetSpeedForGait(Speeds gait)
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

	public void SetSpeed(Speeds gait)
	{
		SetSpeed(GetSpeedForGait(gait));
	}

	public bool IsSpeedGTE(Speeds minGait)
	{
		return curSpeed >= GetSpeedForGait(minGait) - 0.01f;
	}

	public void SetSpeed(float speed)
	{
		desiredSpeed = speed;
	}

	public void SetSpeedRatio(float ratio, Speeds minSpeed = Speeds.Sneak, Speeds maxSpeed = Speeds.Sprint, int offset = 0)
	{
		int num = Mathf.FloorToInt(Mathf.Lerp((float)minSpeed, (float)maxSpeed, ratio));
		num = Mathf.Clamp(num + offset, (int)minSpeed, (int)maxSpeed);
		SetSpeed((Speeds)num);
	}

	private void OnEnable()
	{
		steeringComponents.TryAdd(this);
	}

	private void OnDisable()
	{
		steeringComponents.Remove(this);
	}

	public static void TickSteering()
	{
		for (int num = steeringComponents.Count - 1; num >= 0; num--)
		{
			LimitedTurnNavAgent limitedTurnNavAgent = steeringComponents[num];
			if (ObjectEx.IsUnityNull(limitedTurnNavAgent) || !limitedTurnNavAgent.baseEntity.IsValid())
			{
				steeringComponents.RemoveAt(num);
			}
			else
			{
				limitedTurnNavAgent.Tick();
			}
		}
	}

	private void Tick()
	{
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		if (!base.baseEntity.isServer)
		{
			return;
		}
		using (TimeWarning.New("LimitedTurnNavAgent:Tick"))
		{
			try
			{
				if (!AI.move)
				{
					return;
				}
				if (!isNavMeshReady)
				{
					isNavMeshReady = (Object)(object)agent != (Object)null && ((Behaviour)agent).enabled;
					if (isNavMeshReady && !agent.isOnNavMesh)
					{
						if (base.baseEntity.ServerNavMeshPos != base.baseEntity.ServerWorldPosition)
						{
							Vector3 serverWorldPosition = base.baseEntity.ServerWorldPosition;
							isNavMeshReady = agent.Warp(base.baseEntity.ServerNavMeshPos);
							base.baseEntity.ServerWorldPosition = serverWorldPosition;
						}
						else
						{
							isNavMeshReady = false;
						}
					}
					if (!isNavMeshReady)
					{
						return;
					}
					agent.updateRotation = false;
					agent.updatePosition = false;
					agent.isStopped = true;
				}
				if (movementLock.IsLocked)
				{
					if (previousLocalPosition.HasValue)
					{
						Vector3 val = ((Component)base.baseEntity).transform.localPosition - previousLocalPosition.Value;
						curSpeed = ((Vector3)(ref val)).magnitude / Time.deltaTime;
					}
				}
				else if (IsSwimming && curSpeed > swimSprintSpeed)
				{
					if (AI.logIssues)
					{
						Debug.LogError((object)$"Speed is too high: {curSpeed}/{swimSprintSpeed}");
					}
					curSpeed = swimSpeed;
				}
				else if (!IsSwimming && curSpeed > fullSprintSpeed)
				{
					if (AI.logIssues)
					{
						Debug.LogError((object)$"Speed is too high: {curSpeed}/{fullSprintSpeed}");
					}
					curSpeed = fullSprintSpeed;
				}
				if (!movementLock.IsLocked)
				{
					if (steeringMode == SteeringMode.LimitedTurnRate && (!shouldStopAtDestination || IsFollowingPath))
					{
						SteerTowardsWaypoint();
					}
					else if (steeringMode == SteeringMode.FaceTarget && IsFollowingPath)
					{
						curSpeed = desiredSpeed;
						agent.isStopped = true;
						Vector3 val2 = agent.steeringTarget - agent.nextPosition;
						Vector3 normalized = ((Vector3)(ref val2)).normalized;
						Vector3 val3 = normalized * (curSpeed * Time.deltaTime);
						val3 = Vector3.ClampMagnitude(val3, ((Vector3)(ref val2)).magnitude);
						Move(val3);
						if (!overrideDirection.HasValue)
						{
							SenseComponent senseComponent = default(SenseComponent);
							if (!IsSprinting && ((Component)base.baseEntity).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.FindTargetLKP(out var lkp, applyHeightOffset: true, predict: false, ignoreCrouch: false))
							{
								Vector3 val4 = Vector3Ex.NormalizeXZ(lkp - ((Component)this).transform.position);
								if (((Vector3)(ref val4)).magnitude > 0.001f)
								{
									SetRotation(Quaternion.LookRotation(val4));
								}
							}
							else if (((Vector3)(ref val3)).magnitude > 0.001f)
							{
								Matrix4x4 navMeshToWorldSpace = base.baseEntity.NavMeshToWorldSpace;
								normalized = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyVector(normalized);
								Vector3 val5 = Vector3Ex.XZ3D(normalized);
								if (((Vector3)(ref val5)).magnitude > 0.001f)
								{
									SetRotation(Quaternion.LookRotation(val5));
								}
							}
						}
					}
					else
					{
						curSpeed = Mathf.Max(desiredSpeed, curSpeed - 10f * Time.deltaTime);
						ResetPath();
					}
				}
				SenseComponent senseComponent2 = default(SenseComponent);
				Vector3 lkp2;
				if (overrideDirection.HasValue)
				{
					SetDirection(overrideDirection.Value);
				}
				else if (steeringMode == SteeringMode.FaceTarget && !IsSprinting && ((Component)base.baseEntity).TryGetComponent<SenseComponent>(ref senseComponent2) && senseComponent2.FindTargetLKP(out lkp2, applyHeightOffset: true))
				{
					Vector3 direction = Vector3Ex.NormalizeXZ(lkp2 - ((Component)this).transform.position);
					SetDirection(direction);
				}
			}
			finally
			{
				previousLocalPosition = ((Component)base.baseEntity).transform.localPosition;
			}
		}
	}

	private void SetDirection(Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Ex.XZ3D(direction);
		if (((Vector3)(ref val)).magnitude > 0.001f)
		{
			SetRotation(Quaternion.LookRotation(val));
		}
	}

	private void SetRotation(Quaternion rotation)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = Quaternion.RotateTowards(((Component)this).transform.rotation, rotation, AngularSpeed * Time.deltaTime);
	}

	private static float GetBrakingDistance(float speed, float brakingDeceleration)
	{
		float num = speed / Mathf.Max(brakingDeceleration, 0.001f);
		return 0.5f * brakingDeceleration * num * num;
	}

	private float AdjustSpeedForSwimming(float speed)
	{
		if (!IsSwimming || speed <= 0f)
		{
			return speed;
		}
		if (!(speed < sprintSpeed))
		{
			return swimSprintSpeed;
		}
		return swimSpeed;
	}

	private void SteerTowardsWaypoint()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SteerTowardsWaypoint"))
		{
			Transform transform = ((Component)base.baseEntity).transform;
			Vector3 val;
			if (!IsFollowingPath)
			{
				val = transform.forward;
			}
			else
			{
				Vector3 val2 = agent.steeringTarget - transform.position;
				val = ((Vector3)(ref val2)).normalized;
			}
			Vector3 val3 = val;
			if (Mathf.Abs(cachedPathLength - Vector3.Distance(transform.position, agent.destination)) < 5f)
			{
				val3 = Quaternion.AngleAxis(currentDeviation, Vector3.up) * val3;
			}
			float num = AdjustSpeedForSwimming(desiredSpeed);
			if (shouldStopAtDestination && agent.remainingDistance - maxTurnRadius < GetBrakingDistance(curSpeed, deceleration.Value))
			{
				curSpeed = Mathf.Max(1f, curSpeed - deceleration.Value * Time.deltaTime);
			}
			else if (curSpeed > num)
			{
				float num2 = (curSpeed - num) / deceleration.Value;
				float num3 = ((curSpeed > walkSpeed && num2 > 1f) ? 10f : deceleration.Value);
				curSpeed = Mathf.Max(num, curSpeed - num3 * Time.deltaTime);
			}
			else if (curSpeed < num)
			{
				curSpeed = Mathf.Min(num, curSpeed + acceleration.Value * Time.deltaTime);
			}
			agent.isStopped = true;
			if (!(((Vector3)(ref val3)).magnitude < 0.01f))
			{
				float num4 = (shouldStopAtDestination ? Mathx.RemapValClamped(agent.remainingDistance, maxTurnRadius * 2f, 0f, maxTurnRadius, 0.001f) : maxTurnRadius);
				float num5 = curSpeed / num4;
				Vector3 val4 = Vector3.RotateTowards(transform.forward, val3, num5 * Time.deltaTime, 0f);
				Vector3 offset = val4 * (curSpeed * Time.deltaTime);
				transform.rotation = Quaternion.LookRotation(Vector3Ex.WithY(val4, 0f));
				Move(offset);
			}
		}
	}

	public bool IsPositionOnNavmesh(Vector3 position, out Vector3 sample)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return SamplePosition(position, out sample, 0.5f);
	}

	public bool SampleGroundPositionWithPhysics(Vector3 position, out RaycastHit hitInfo, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SampleGroundPositionWithPhysics"))
		{
			Vector3 val = position + Vector3.up * radius * 1.5f;
			float maxDistance2 = maxDistance + radius * 1.5f;
			if (!GamePhysics.TraceRealm(GamePhysics.Realm.Server, new Ray(val, Vector3.down), radius, out hitInfo, maxDistance2, layerMask, (QueryTriggerInteraction)1))
			{
				((RaycastHit)(ref hitInfo)).point = position;
				return false;
			}
			if (radius > 0f && ((RaycastHit)(ref hitInfo)).distance <= 0f)
			{
				((RaycastHit)(ref hitInfo)).point = position;
			}
			return true;
		}
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

	public bool SamplePosition(Vector3 position, out Vector3 sample, float maxDistance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SamplePosition"))
		{
			sample = position;
			NavMeshHit hitNS;
			bool num = agent.SamplePosition(position, out hitNS, maxDistance);
			float num2 = 1f;
			if (num && maxDistance > num2 && Mathf.Abs(((NavMeshHit)(ref hitNS)).position.y - position.y) > num2 && SampleGroundPositionWithPhysics(position, out var hitInfo, 3.5f) && agent.SamplePosition(((RaycastHit)(ref hitInfo)).point, out var hitNS2, num2))
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

	public bool Raycast(Vector3 startPosition, Vector3 targetPosition, out NavMeshHit hitInfo)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:RaycastAgent"))
		{
			return agent.Raycast(startPosition, targetPosition, out hitInfo);
		}
	}

	public bool Raycast(Vector3 targetPosition, out NavMeshHit hitInfo)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:Raycast"))
		{
			return agent.Raycast(targetPosition, out hitInfo);
		}
	}

	public bool CalculatePathCustom(Vector3 startPosition, Vector3 destination, RustNavMeshPath path)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:CalculatePathCustom"))
		{
			return agent.CalculatePath(startPosition, destination, path);
		}
	}

	public bool CalculatePathCustom(Vector3 destination, RustNavMeshPath path)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("LimitedTurnNavAgent:CalculatePathCustomAgent"))
		{
			return agent.CalculatePath(destination, path);
		}
	}
}
