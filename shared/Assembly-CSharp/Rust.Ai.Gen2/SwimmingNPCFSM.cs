using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public abstract class SwimmingNPCFSM : FSMComponent
{
	[Serializable]
	public class Trans_NoPlayerConnectionsInRange : FSMTransitionBase
	{
		private bool noPlayersInRange;

		private TimeSince timeSinceLastCheck;

		private float checkTimeout = 10f;

		public override void Init(BaseEntity owner)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			timeSinceLastCheck = TimeSince.op_Implicit(999f);
			base.Init(owner);
		}

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			if (TimeSince.op_Implicit(timeSinceLastCheck) >= checkTimeout)
			{
				noPlayersInRange = !BaseNetworkable.HasCloseConnections(((Component)Owner).transform.position, 128f);
				timeSinceLastCheck = TimeSince.op_Implicit(0f);
			}
			return noPlayersInRange;
		}
	}

	[Serializable]
	public class State_Deactivate : FSMStateBase
	{
		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			SetDeactivated(toggle: true);
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			return EFSMStateStatus.None;
		}

		public override void OnStateExit()
		{
			SetDeactivated(toggle: false);
			base.OnStateExit();
		}

		private void SetDeactivated(bool toggle)
		{
			if (!(Owner is SwimmingNPC swimmingNPC))
			{
				Debug.LogWarning((object)"[SwimmingNPC] Deactivate state owner is not a SwimmingNPC, flag not set", (Object)(object)Owner);
			}
			else
			{
				swimmingNPC.SetDeactivatedFlag(toggle);
			}
		}
	}

	[Serializable]
	public abstract class State_SwimToPoint : FSMStateBase
	{
		protected virtual bool IsUrgent => false;

		protected abstract bool TryPickDestination(SwimmingNPC swimmer, out Vector3 destination);

		protected virtual void SetUrgent(SwimmingNPC swimmer, bool urgent)
		{
		}

		protected EFSMStateStatus SetDestination()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			SwimmingNPC swimmingNPC = default;
			if (!((Component)Owner).TryGetComponent<SwimmingNPC>(ref swimmingNPC))
			{
				return EFSMStateStatus.Failure;
			}
			if (!TryPickDestination(swimmingNPC, out var destination))
			{
				return EFSMStateStatus.Success;
			}
			swimmingNPC.destination = swimmingNPC.WaterClamp(destination);
			if (IsUrgent)
			{
				SetUrgent(swimmingNPC, urgent: true);
			}
			return EFSMStateStatus.None;
		}

		public override void OnStateExit()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			SwimmingNPC swimmingNPC = default;
			if (((Component)Owner).TryGetComponent<SwimmingNPC>(ref swimmingNPC))
			{
				SetUrgent(swimmingNPC, urgent: false);
				swimmingNPC.destination = ((Component)swimmingNPC).transform.position;
			}
			base.OnStateExit();
		}
	}

	[Serializable]
	public class State_RoamWater : State_SwimToPoint
	{
		[SerializeField]
		private Vector2 distanceRange = new Vector2(10f, 25f);

		[Tooltip("How far the swimmer will drift from home before its next leg is aimed back towards it.")]
		[SerializeField]
		private float homeRadius = 50f;

		[Tooltip("How close counts as arrived. The swimmer steers rather than paths, so it never lands exactly on the point.")]
		[SerializeField]
		private float arriveDistance = 3f;

		[Tooltip("Water this shallow is not worth swimming to. What passes for navigable without a navmesh.")]
		[SerializeField]
		private float minWaterDepth = 3f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("How far down the swimmer aims to swim, as a fraction of the water depth at its destination. 0 hugs the surface and 1 hugs the floor. Without this the destination inherits whatever height the swimmer already had, so it never recovers depth once it sinks.")]
		private float targetDepthFraction = 0.3f;

		[SerializeField]
		[Tooltip("How much each leg's depth varies around the target, as a fraction of local depth, so the swimmer does not swim at one fixed height. WaterClamp keeps it in bounds.")]
		[Range(0f, 0.5f)]
		private float depthJitterFraction = 0.15f;

		[SerializeField]
		private int maxAttempts = 4;

		[Tooltip("How long a single swim \"path\" can take before the swimmer gives up and picks another.")]
		[SerializeField]
		private float legTimeout = 30f;

		private Vector3? spawnPosition;

		private TimeSince timeSinceLegStarted;

		protected virtual Vector3 HomePosition
		{
			get
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return spawnPosition ?? ((Component)Owner).transform.position;
			}
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 valueOrDefault = spawnPosition.GetValueOrDefault();
			if (!spawnPosition.HasValue)
			{
				valueOrDefault = ((Component)Owner).transform.position;
				spawnPosition = valueOrDefault;
			}
			timeSinceLegStarted = TimeSince.op_Implicit(0f);
			EFSMStateStatus eFSMStateStatus = SetDestination();
			if (eFSMStateStatus != EFSMStateStatus.None)
			{
				return eFSMStateStatus;
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			SwimmingNPC swimmingNPC = default;
			if (!((Component)Owner).TryGetComponent<SwimmingNPC>(ref swimmingNPC))
			{
				return EFSMStateStatus.Failure;
			}
			if (Vector3.Distance(((Component)swimmingNPC).transform.position, swimmingNPC.destination) <= arriveDistance)
			{
				return EFSMStateStatus.Success;
			}
			if (TimeSince.op_Implicit(timeSinceLegStarted) > legTimeout)
			{
				return EFSMStateStatus.Success;
			}
			return base.OnStateUpdate(deltaTime);
		}

		protected override bool TryPickDestination(SwimmingNPC swimmer, out Vector3 destination)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)swimmer).transform.position;
			Vector3 homePosition = HomePosition;
			bool flag = Vector3.Distance(homePosition, position) > homeRadius;
			Vector3? val = null;
			for (int i = 0; i < maxAttempts; i++)
			{
				float num = Mathf.Lerp(distanceRange.x, distanceRange.y, (float)i / (float)maxAttempts);
				Vector3 val2;
				if (!val.HasValue)
				{
					if (!flag)
					{
						Vector2 insideUnitCircle = Random.insideUnitCircle;
						val2 = Vector3Ex.XZ3D(insideUnitCircle.normalized);
					}
					else
					{
						val2 = Quaternion.Euler(0f, Random.Range(-60f, 60f), 0f) * Vector3Ex.NormalizeXZ(homePosition - position);
					}
				}
				else
				{
					val2 = Quaternion.Euler(0f, Random.Range(-45f, 45f), 0f) * val.Value;
				}
				Vector3 val3 = val2;
				Vector3 val4 = position + val3 * num;
				float overallWaterDepth = WaterLevel.GetOverallWaterDepth(val4, waves: false, volumes: false);
				if (overallWaterDepth < minWaterDepth)
				{
					Vector3? val5 = val;
					if (!val5.HasValue)
					{
						val = GetAwayFromShore(position);
					}
					continue;
				}
				float num2 = overallWaterDepth * depthJitterFraction;
				val4.y = GetTargetAltitude(val4) + Random.Range(0f - num2, num2);
				destination = val4;
				return true;
			}
			destination = homePosition;
			return false;
		}

		private float GetTargetAltitude(Vector3 point)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			var (num, num2) = WaterLevel.GetWaterAndTerrainSurface(point, waves: false, volumes: false);
			return Mathf.Lerp(num, num2, targetDepthFraction);
		}

		private static Vector3? GetAwayFromShore(Vector3 pos)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			TerrainTexturing instance = TerrainTexturing.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return null;
			}
			(Vector3 shoreDir, float shoreDist) coarseVectorToShore = instance.GetCoarseVectorToShore(pos);
			Vector3 item = coarseVectorToShore.shoreDir;
			Vector3 val = ((coarseVectorToShore.shoreDist >= 0f) ? (-item) : item);
			if (val.sqrMagnitude < 0.001f)
			{
				return null;
			}
			return Vector3Ex.NormalizeXZ(val);
		}

		public State_RoamWater()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	protected override TickFSMWorkQueue workQueue => CritterAnimalFSM.critterWorkQueue;
}
