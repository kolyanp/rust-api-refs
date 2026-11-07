using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class SeagullGroupFSM : FSMComponent
{
	[Serializable]
	public class Trans_HeardLoudNoise : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is SeagullGroup seagullGroup)
			{
				return seagullGroup.RecentlyHeardLoudNoise;
			}
			return false;
		}
	}

	[Serializable]
	public class State_Landed : FSMStateBase
	{
		[SerializeField]
		[Tooltip("How far from a land point a group that spawned on it can be and still claim it.")]
		private float claimRadius = 5f;

		[Tooltip("How close a player the group has seen can get before it takes off, in metres.")]
		public float startleDistance = 6f;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is SeagullGroup { LandPoint: null } seagullGroup)
			{
				SeagullLandPoint seagullLandPoint = SeagullLandPoint.FindFree(seagullGroup, ((Component)seagullGroup).transform.position, 0f, claimRadius);
				if ((Object)(object)seagullLandPoint != (Object)null && seagullLandPoint.TryClaim(seagullGroup))
				{
					seagullGroup.LandPoint = seagullLandPoint;
				}
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			return EFSMStateStatus.None;
		}
	}

	[Serializable]
	public abstract class State_SeagullFly : BirdFlockFSM.State_Fly
	{
		[Tooltip("Height above the ground or water the group flies at, in metres.")]
		[SerializeField]
		protected float altitude = 25f;

		protected Vector3 Home
		{
			get
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				if (!(Owner is SeagullGroup seagullGroup))
				{
					return ((Component)Owner).transform.position;
				}
				return seagullGroup.HomePosition;
			}
		}

		protected override float GetTargetHeight(Vector3 position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return BirdFlockFSM.State_Fly.GetSurfaceHeight(position) + altitude;
		}

		protected override void MoveTo(Vector3 position, Quaternion rotation)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is SeagullGroup seagullGroup)
			{
				seagullGroup.MoveTo(position, rotation);
			}
			else
			{
				base.MoveTo(position, rotation);
			}
		}
	}

	[Serializable]
	public class State_FlyAway : State_SeagullFly
	{
		[Tooltip("How long the group flies straight away from the scare before it turns back to circle home, in seconds.")]
		[SerializeField]
		private Vector2 durationRange = new Vector2(3f, 6f);

		[Tooltip("Played for every player in range as the group is startled into the air, attached to the group so it follows the flock away.")]
		[SerializeField]
		private GameObjectRef startleEffect;

		[Tooltip("How long the group takes to build up to full speed, in seconds. Keeps it over the birds while they run and take off, so they stay inside its bounds and shootable.")]
		[SerializeField]
		private float speedRampTime = 3f;

		[Tooltip("How far the way the group takes off has to be clear, in metres. It turns aside from walls rather than flying the birds into them.")]
		[SerializeField]
		private float takeoffClearance = 60f;

		private const float clearTurnStep = 30f;

		private const float clearProbeClimb = 0.35f;

		private const float clearProbeHeight = 1f;

		private const float minStartleDistance = 10f;

		private TimeUntil flightEnd;

		private TimeSince timeSinceTakeoff;

		public State_FlyAway()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			speed = 8f;
			altitude = 30f;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)Owner).transform.position;
			Vector3 val = (Senses.FindTargetPosition(out var targetPosition) ? targetPosition : position);
			Vector3 val2 = Vector3Ex.NormalizeXZ(position - val);
			if (val2 == Vector3.zero)
			{
				Vector2 insideUnitCircle = Rng.InsideUnitCircle;
				val2 = Vector3Ex.XZ3D(insideUnitCircle.normalized);
			}
			Vector3 val3 = FindClearDirection(position, val2);
			if (val3 != val2)
			{
				val2 = val3;
				val = position - val2 * Mathf.Max(Vector3Ex.Distance2D(position, val), 10f);
			}
			MoveTo(position, Quaternion.LookRotation(val2));
			destination = position + val2 * 1000f;
			flightEnd = TimeUntil.op_Implicit(Rng.Range(durationRange.x, durationRange.y));
			timeSinceTakeoff = TimeSince.op_Implicit(0f);
			speedScale = 0f;
			if (Owner is SeagullGroup seagullGroup)
			{
				seagullGroup.StartlePosition = val;
				seagullGroup.FlightSeed = Rng.NextSeed();
				seagullGroup.SetFlying(flying: true);
			}
			if (startleEffect.isValid)
			{
				Effect.server.Run(startleEffect.resourcePath, Owner, 0u, Vector3.zero, Vector3.up);
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(flightEnd) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			float num = ((speedRampTime > 0f) ? Mathf.Clamp01(TimeSince.op_Implicit(timeSinceTakeoff) / speedRampTime) : 1f);
			speedScale = num * num;
			Fly(deltaTime);
			return EFSMStateStatus.None;
		}

		private Vector3 FindClearDirection(Vector3 position, Vector3 away)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = position + Vector3.up * 1f;
			Vector3 result = away;
			float num = 0f;
			RaycastHit val4 = default;
			for (float num2 = 0f; num2 <= 180f; num2 += 30f)
			{
				for (float num3 = 1f; num3 >= -1f; num3 -= 2f)
				{
					Vector3 val2 = Quaternion.Euler(0f, num2 * num3, 0f) * away;
					Vector3 val3 = val2 + Vector3.up * 0.35f;
					if (!Physics.Raycast(val, val3.normalized, ref val4, takeoffClearance, 1084293377, (QueryTriggerInteraction)1))
					{
						return val2;
					}
					if (val4.distance > num)
					{
						result = val2;
						num = val4.distance;
					}
				}
			}
			return result;
		}
	}

	[Serializable]
	public class State_CircleHome : State_SeagullFly
	{
		[Tooltip("How long the group circles before it looks for somewhere to land, in seconds.")]
		[SerializeField]
		private Vector2 durationRange = new Vector2(15f, 60f);

		[Tooltip("Radius of the circle around home, in metres. Rolled each time the group starts circling.")]
		[SerializeField]
		private Vector2 radiusRange = new Vector2(25f, 50f);

		[Tooltip("How far ahead round the circle the group aims, in radians. Too little and it cuts inside, too much and it drifts outside.")]
		[SerializeField]
		private float leadAngle = 0.6f;

		private TimeUntil circleEnd;

		private float radius;

		private float direction;

		public State_CircleHome()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			speed = 7f;
			altitude = 20f;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			circleEnd = TimeUntil.op_Implicit(Rng.Range(durationRange.x, durationRange.y));
			radius = Rng.Range(radiusRange.x, radiusRange.y);
			direction = ((Rng.Range(0f, 1f) < 0.5f) ? 1f : (-1f));
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(circleEnd) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			destination = AimAroundCircle(Home, radius, leadAngle * direction);
			Fly(deltaTime);
			return EFSMStateStatus.None;
		}
	}

	[Serializable]
	public class State_FlyToLandPoint : State_SeagullFly
	{
		[Tooltip("How far from home the group looks for a land point to fly to, in metres.")]
		[SerializeField]
		private float homeRadius = 150f;

		[Tooltip("How far away a land point can be to fly to when every point near home is taken, in metres. The rare case where the group leaves.")]
		[SerializeField]
		private Vector2 searchDistance = new Vector2(30f, 500f);

		[Tooltip("Radius the group circles home at while it waits for a land point to come free, in metres.")]
		[SerializeField]
		private float waitRadius = 40f;

		[Tooltip("How close to the land point counts as arrived, where the group stops steering and comes straight down.")]
		[SerializeField]
		private float arriveDistance = 15f;

		private const float searchInterval = 5f;

		private const float waitLeadAngle = 0.6f;

		private SeagullLandPoint target;

		private TimeSince timeSinceEnter;

		private TimeSince timeSinceSearch;

		public State_FlyToLandPoint()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			speed = 8f;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			target = null;
			timeSinceEnter = TimeSince.op_Implicit(0f);
			Search();
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)target == (Object)null && TimeSince.op_Implicit(timeSinceSearch) >= 5f)
			{
				Search();
			}
			if ((Object)(object)target == (Object)null)
			{
				destination = AimAroundCircle(Home, waitRadius, 0.6f);
			}
			else
			{
				destination = ((Component)target).transform.position;
				if (Vector3Ex.Distance2D(((Component)Owner).transform.position, destination) <= arriveDistance)
				{
					return EFSMStateStatus.Success;
				}
			}
			Fly(deltaTime);
			return EFSMStateStatus.None;
		}

		private void Search()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			timeSinceSearch = TimeSince.op_Implicit(0f);
			if (!(Owner is SeagullGroup seagullGroup))
			{
				return;
			}
			SeagullLandPoint seagullLandPoint = SeagullLandPoint.FindFree(seagullGroup, seagullGroup.HomePosition, 0f, homeRadius) ?? SeagullLandPoint.FindFree(seagullGroup, ((Component)seagullGroup).transform.position, searchDistance.x, searchDistance.y) ?? seagullGroup.LandPoint;
			if (!((Object)(object)seagullLandPoint == (Object)null) && seagullLandPoint.TryClaim(seagullGroup))
			{
				if ((Object)(object)seagullLandPoint != (Object)(object)seagullGroup.LandPoint)
				{
					seagullGroup.LandPoint?.Release(seagullGroup);
				}
				seagullGroup.LandPoint = seagullLandPoint;
				target = seagullLandPoint;
			}
		}
	}

	[Serializable]
	public class State_Land : FSMStateBase
	{
		[SerializeField]
		[Tooltip("How fast the group closes on the spot above the land point, in metres per second.")]
		private float approachSpeed = 8f;

		[Tooltip("How fast the group comes down onto the land point, in metres per second.")]
		[SerializeField]
		private float descentSpeed = 5f;

		[Tooltip("How far above or below the land point the group looks for the ground to land on, in metres.")]
		[SerializeField]
		private float groundSearchDistance = 2f;

		[Tooltip("How long the group waits after touching down for its last birds to glide in, in seconds. It keeps the bounds that reach up to them until then.")]
		[SerializeField]
		private float settleTime = 3f;

		private const float maxDeltaTime = 0.25f;

		private TimeSince timeSinceTouchdown;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is SeagullGroup { LandPoint: not null } seagullGroup))
			{
				return EFSMStateStatus.Failure;
			}
			seagullGroup.LandPoint.GetLocation(out var pos, out var _);
			RaycastHit val = default;
			seagullGroup.Touchdown = (Physics.Raycast(pos + Vector3.up * groundSearchDistance, Vector3.down, ref val, groundSearchDistance * 2f, 1084293377, (QueryTriggerInteraction)1) ? val.point : pos);
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is SeagullGroup seagullGroup))
			{
				return EFSMStateStatus.Failure;
			}
			if (!seagullGroup.IsFlying())
			{
				if (!(TimeSince.op_Implicit(timeSinceTouchdown) >= settleTime))
				{
					return EFSMStateStatus.None;
				}
				return EFSMStateStatus.Success;
			}
			deltaTime = Mathf.Min(deltaTime, 0.25f);
			Vector3 val = default;
			Quaternion rotation = default;
			((Component)seagullGroup).transform.GetPositionAndRotation(ref val, ref rotation);
			Vector3 touchdown = seagullGroup.Touchdown;
			if (!seagullGroup.IsLanding())
			{
				Vector3 val2 = new Vector3(touchdown.x, val.y, touchdown.z);
				Vector3 val3 = val2 - val;
				float num = approachSpeed * deltaTime;
				if (val3.sqrMagnitude > num * num)
				{
					seagullGroup.MoveTo(val + val3.normalized * num, Quaternion.LookRotation(Vector3Ex.NormalizeXZ(val3)));
					return EFSMStateStatus.None;
				}
				seagullGroup.MoveTo(val2, rotation);
				seagullGroup.SetLanding(landing: true);
				return EFSMStateStatus.None;
			}
			float num2 = descentSpeed * deltaTime;
			if (val.y - touchdown.y > num2)
			{
				seagullGroup.MoveTo(val + Vector3.down * num2, rotation);
				return EFSMStateStatus.None;
			}
			seagullGroup.MoveTo(touchdown, rotation);
			seagullGroup.SetFlying(flying: false);
			timeSinceTouchdown = TimeSince.op_Implicit(0f);
			return EFSMStateStatus.None;
		}

		public override void OnStateExit()
		{
			if (Owner is SeagullGroup seagullGroup)
			{
				seagullGroup.SetLanding(landing: false);
			}
			base.OnStateExit();
		}
	}

	public State_Landed landed = new State_Landed();

	public State_FlyAway flyAway = new State_FlyAway();

	public State_CircleHome circleHome = new State_CircleHome();

	public State_FlyToLandPoint flyToLandPoint = new State_FlyToLandPoint();

	public State_Land land = new State_Land();

	protected override TickFSMWorkQueue workQueue => CritterAnimalFSM.critterWorkQueue;

	protected override FSMTransitionBase NewNavmeshReadyTransition()
	{
		return new Trans_AlwaysValid();
	}

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		onNavmesh.AddChildren(landed.AddTickTransition(flyAway, new Trans_TargetInRange
		{
			Range = landed.startleDistance
		} | new Trans_RecentlyHurt() | new Trans_HeardLoudNoise()), flyAway.AddEndTransition(circleHome), circleHome.AddEndTransition(flyToLandPoint), flyToLandPoint.AddEndTransition(land), land.AddFailureTransition(flyToLandPoint).AddEndTransition(landed));
		return new AliveLayout
		{
			entry = landed,
			idle = landed
		};
	}
}
