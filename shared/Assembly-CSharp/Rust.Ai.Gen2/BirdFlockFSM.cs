using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class BirdFlockFSM : FSMComponent
{
	[Serializable]
	public class Trans_Startled : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is BirdFlock { startlePosition: var startlePosition })
			{
				return startlePosition.HasValue;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_FoundInterestPoint : FSMSlowTransitionBase
	{
		protected override bool EvaluateAtInterval(ref FSMPayload payload)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is BirdFlock birdFlock) || TimeUntil.op_Implicit(birdFlock.timeUntilInterested) > 0f)
			{
				return false;
			}
			birdFlock.interestPointId = BirdInterestPointManager.Claim(birdFlock);
			return birdFlock.interestPointId != 0;
		}
	}

	[Serializable]
	public abstract class State_Fly : FSMStateBase
	{
		[SerializeField]
		protected float speed = 6f;

		[Tooltip("Turn rate in radians per second. Low makes wide, lazy arcs.")]
		[SerializeField]
		private float turnSpeed = 0.4f;

		[Tooltip("How quickly the flock climbs or sinks towards its altitude.")]
		[SerializeField]
		private float altitudeEaseRate = 0.5f;

		[Tooltip("Fastest the flock climbs or sinks, in metres per second. Kept below what the birds can climb, or the anchor leaves them behind.")]
		[SerializeField]
		protected float maxClimbSpeed = 3f;

		[Tooltip("How far ahead the flock looks for something too tall to fly through, in metres. Needs to be far enough to climb in time at maxClimbSpeed.")]
		[SerializeField]
		private float lookAheadDistance = 150f;

		[Tooltip("How high above the top of an obstacle ahead the flock climbs, in metres.")]
		[SerializeField]
		private float obstacleClearance = 15f;

		private const float maxDeltaTime = 0.25f;

		private const float probeInterval = 0.5f;

		protected Vector3 destination;

		protected float speedScale = 1f;

		private TimeSince timeSinceProbe;

		private float obstacleTopY;

		private TimeUntil obstacleClimbEnd;

		protected abstract float GetTargetHeight(Vector3 position);

		protected void Fly(float deltaTime)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			Quaternion val2 = default;
			Vector3 val = default;
			((Component)Owner).transform.GetPositionAndRotation(ref val, ref val2);
			deltaTime = Mathf.Min(deltaTime, 0.25f);
			Vector3 val3 = Vector3Ex.NormalizeXZ(destination - val);
			if (val3 != Vector3.zero)
			{
				Quaternion val4 = Quaternion.LookRotation(val3, Vector3.up);
				val2 = Quaternion.RotateTowards(val2, val4, turnSpeed * 57.29578f * deltaTime);
			}
			Vector3 val5 = val2 * Vector3.forward;
			if (TimeSince.op_Implicit(timeSinceProbe) >= 0.5f)
			{
				ProbeAhead(val, val5);
			}
			float num = GetTargetHeight(val);
			if (TimeUntil.op_Implicit(obstacleClimbEnd) > 0f)
			{
				num = Mathf.Max(num, obstacleTopY + obstacleClearance);
			}
			val += val5 * (speed * speedScale * deltaTime);
			val.y += Mathf.Clamp((num - val.y) * altitudeEaseRate, 0f - maxClimbSpeed, maxClimbSpeed) * deltaTime;
			MoveTo(val, val2);
		}

		protected virtual void MoveTo(Vector3 position, Quaternion rotation)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			((Component)Owner).transform.SetPositionAndRotation(position, rotation);
		}

		private void ProbeAhead(Vector3 position, Vector3 forward)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			timeSinceProbe = TimeSince.op_Implicit(0f);
			RaycastHit val = default;
			RaycastHit val2 = default;
			if (Physics.Raycast(position, forward, ref val, lookAheadDistance, 1084293377) && Physics.Raycast(val.point + forward * 0.5f + Vector3.up * 500f, Vector3.down, ref val2, 1000f, 1084293377))
			{
				obstacleTopY = ((TimeUntil.op_Implicit(obstacleClimbEnd) > 0f) ? Mathf.Max(obstacleTopY, val2.point.y) : val2.point.y);
				obstacleClimbEnd = TimeUntil.op_Implicit((val.distance + lookAheadDistance) / Mathf.Max(speed * speedScale, 1f));
			}
		}

		protected Vector3 AimAroundCircle(Vector3 center, float radius, float leadAngle)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ((Component)Owner).transform.position - center;
			float num = Mathf.Atan2(val.z, val.x) + leadAngle;
			float num2 = radius / Mathf.Cos(leadAngle);
			return center + new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num)) * num2;
		}

		protected static bool IsInArea(Vector3 position, bool inDeepSea)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			if (!inDeepSea)
			{
				return !TerrainMeta.OutOfBounds(position);
			}
			return DeepSeaManager.IsInsideDeepSea(position);
		}

		protected static float GetSurfaceHeight(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			var (num, num2) = WaterLevel.GetWaterAndTerrainSurface(position, waves: false, volumes: false);
			return Mathf.Max(num, num2);
		}
	}

	[Serializable]
	public class State_FlyRoam : State_Fly
	{
		[SerializeField]
		private Vector2 distanceRange = new Vector2(150f, 500f);

		[Tooltip("How far the flock will wander from home before its next leg is aimed back towards it. Home is where it spawned, or where it last fled to.")]
		[SerializeField]
		private float homeRadius = 1500f;

		[SerializeField]
		[Tooltip("Height above the ground or water the flock flies at, in metres. Rolled per leg.")]
		private Vector2 altitudeRange = new Vector2(25f, 45f);

		[SerializeField]
		[Tooltip("How close counts as arrived. The flock steers rather than paths, so it never lands exactly on the point.")]
		private float arriveDistance = 15f;

		[SerializeField]
		[Tooltip("How long a single leg can take before the flock gives up and picks another.")]
		private float legTimeout = 120f;

		private Nullable homePosition;

		private float altitude;

		private TimeSince timeSinceLegStarted;

		public void MoveHome(Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			homePosition = position;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 valueOrDefault = homePosition.GetValueOrDefault();
			if (!homePosition.HasValue)
			{
				valueOrDefault = ((Component)Owner).transform.position;
				homePosition = valueOrDefault;
			}
			PickDestination();
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (Vector3Ex.Distance2D(((Component)Owner).transform.position, destination) <= arriveDistance || TimeSince.op_Implicit(timeSinceLegStarted) > legTimeout)
			{
				PickDestination();
			}
			Fly(deltaTime);
			return base.OnStateUpdate(deltaTime);
		}

		protected override float GetTargetHeight(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return State_Fly.GetSurfaceHeight(position) + altitude;
		}

		private void PickDestination()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)Owner).transform.position;
			Vector3 value = homePosition.Value;
			float num = Rng.Range(distanceRange.x, distanceRange.y);
			Vector2 insideUnitCircle = Rng.InsideUnitCircle;
			Vector3 val = Vector3Ex.XZ3D(insideUnitCircle.normalized);
			Vector3 position2 = position + val * num;
			bool flag = DeepSeaManager.IsInsideDeepSea(value);
			if (Vector3.Distance(value, position) > homeRadius)
			{
				val = TurnTowards(value);
			}
			else if (!State_Fly.IsInArea(position2, flag))
			{
				val = TurnTowards(flag ? value : TerrainMeta.Center);
			}
			destination = position + val * num;
			altitude = Rng.Range(altitudeRange.x, altitudeRange.y);
			timeSinceLegStarted = TimeSince.op_Implicit(0f);
			Vector3 TurnTowards(Vector3 point)
			{
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				return Quaternion.Euler(0f, Rng.Range(-60f, 60f), 0f) * Vector3Ex.NormalizeXZ(point - position);
			}
		}

		public State_FlyRoam()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_Interested : State_Fly
	{
		[SerializeField]
		[Tooltip("How far ahead round the circle the flock aims, in radians. Too little and it cuts inside, too much and it drifts outside.")]
		private float leadAngle = 0.6f;

		[SerializeField]
		[Tooltip("Share of its normal speed the entity circles at once it is over the point. The birds do the moving there, so slower keeps the flock settled over the point.")]
		private float circlingSpeedScale = 0.5f;

		[Tooltip("How long the flock stays over the point once it gets there, in seconds. Rolled per visit.")]
		[SerializeField]
		private Vector2 visitDuration = new Vector2(120f, 240f);

		[SerializeField]
		[Tooltip("How long after a visit before the flock looks for another point, in seconds.")]
		private float revisitCooldown = 300f;

		[SerializeField]
		[Tooltip("Height above the ground or water the flock flies at on its way to the point, in metres.")]
		private float travelAltitude = 35f;

		[Tooltip("Added to the height every interest point asks this flock to circle at, in metres.")]
		[SerializeField]
		private float extraCircleHeight;

		private const float circleClearance = 10f;

		private const float arrivedRadiusScale = 2f;

		private float visitTimeLeft;

		private BirdInterestPointManager.InterestPoint point;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			if (!(Owner is BirdFlock birdFlock) || !BirdInterestPointManager.TryGet(birdFlock.interestPointId, out point))
			{
				return EFSMStateStatus.Success;
			}
			visitTimeLeft = Rng.Range(visitDuration.x, visitDuration.y);
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is BirdFlock birdFlock) || !BirdInterestPointManager.TryGet(birdFlock.interestPointId, out point))
			{
				return EFSMStateStatus.Success;
			}
			bool flag = IsCircling(((Component)Owner).transform.position);
			birdFlock.SetNetworkedFlag(BaseEntity.Flags.Reserved9, flag);
			speedScale = (flag ? circlingSpeedScale : 1f);
			if (flag)
			{
				visitTimeLeft -= deltaTime;
			}
			if (visitTimeLeft <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			destination = AimAroundCircle(point.position, point.circleRadius, leadAngle);
			Fly(deltaTime);
			return base.OnStateUpdate(deltaTime);
		}

		public override void OnStateExit()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is BirdFlock birdFlock)
			{
				birdFlock.ReleaseInterestPoint();
				birdFlock.SetNetworkedFlag(BaseEntity.Flags.Reserved9, value: false);
				birdFlock.timeUntilInterested = TimeUntil.op_Implicit(revisitCooldown);
			}
			speedScale = 1f;
			base.OnStateExit();
		}

		protected override float GetTargetHeight(Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			float num = (IsCircling(position) ? 10f : travelAltitude);
			return Mathf.Max(point.position.y + point.circleHeight + extraCircleHeight, State_Fly.GetSurfaceHeight(position) + num);
		}

		private bool IsCircling(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return Vector3Ex.Distance2D(position, point.position) <= point.circleRadius * 2f;
		}

		public State_Interested()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_Flee : State_Fly
	{
		[Tooltip("How far the flock flees before settling down again, in metres.")]
		[SerializeField]
		private float fleeDistance = 1000f;

		[Tooltip("Height above the ground or water the flock flees at, in metres.")]
		[SerializeField]
		private float fleeAltitude = 60f;

		[Tooltip("How long after fleeing the flock ignores interest points, in seconds. Stops crows going straight back to the bodies they were scared off.")]
		[SerializeField]
		private float interestCooldown = 120f;

		[NonSerialized]
		public State_FlyRoam roam;

		private const float minFleeDistance = 50f;

		public State_Flee()
		{
			speed = 14f;
			maxClimbSpeed = 6f;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			AimAway();
			if (Owner is BirdFlock birdFlock)
			{
				birdFlock.ReleaseInterestPoint();
				birdFlock.SetNetworkedFlag(BaseEntity.Flags.Reserved8, value: true);
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is BirdFlock { startlePosition: not null })
			{
				AimAway();
			}
			if (Vector3Ex.Distance2D(((Component)Owner).transform.position, destination) <= 20f)
			{
				return EFSMStateStatus.Success;
			}
			Fly(deltaTime);
			return base.OnStateUpdate(deltaTime);
		}

		public override void OnStateExit()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is BirdFlock birdFlock)
			{
				birdFlock.SetNetworkedFlag(BaseEntity.Flags.Reserved8, value: false);
				birdFlock.timeUntilInterested = TimeUntil.op_Implicit(interestCooldown);
			}
			roam?.MoveHome(((Component)Owner).transform.position);
			base.OnStateExit();
		}

		protected override float GetTargetHeight(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return State_Fly.GetSurfaceHeight(position) + fleeAltitude;
		}

		private void AimAway()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is BirdFlock { startlePosition: { } startlePosition } birdFlock)
			{
				birdFlock.startlePosition = null;
				Vector3 position = ((Component)Owner).transform.position;
				Vector3 val = Vector3Ex.NormalizeXZ(position - startlePosition);
				if (val == Vector3.zero)
				{
					Vector2 insideUnitCircle = Rng.InsideUnitCircle;
					val = Vector3Ex.XZ3D(insideUnitCircle.normalized);
				}
				bool inDeepSea = DeepSeaManager.IsInsideDeepSea(position);
				float num = fleeDistance;
				while (num > 50f && !State_Fly.IsInArea(position + val * num, inDeepSea))
				{
					num *= 0.5f;
				}
				destination = position + val * num;
			}
		}
	}

	public State_FlyRoam flyRoam = new State_FlyRoam();

	public State_Interested interested = new State_Interested();

	public State_Flee flee = new State_Flee();

	protected override TickFSMWorkQueue workQueue => CritterAnimalFSM.critterWorkQueue;

	protected override FSMTransitionBase NewNavmeshReadyTransition()
	{
		return new Trans_AlwaysValid();
	}

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		flee.roam = flyRoam;
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Calm"
		};
		onNavmesh.AddChildren(state_Nothing.AddTickTransition(flee, new Trans_Startled()).AddChildren(flyRoam.AddTickTransition(interested, new Trans_FoundInterestPoint()), interested.AddEndTransition(flyRoam)), flee.AddEndTransition(flyRoam));
		return new AliveLayout
		{
			entry = flyRoam,
			idle = flyRoam
		};
	}
}
