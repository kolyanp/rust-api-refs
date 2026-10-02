using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rust.Ai.Gen2;

public class LivestockAnimalFSM : FSMComponent
{
	[Serializable]
	public class State_Follow : FSMStateBase
	{
		[SerializeField]
		protected RustNavMeshAgent.Speeds minSpeed;

		[SerializeField]
		protected RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

		[SerializeField]
		protected float idealDistance = 3f;

		[SerializeField]
		protected float laneWidth = 1.5f;

		public const float StandingSpeed = 0.5f;

		private const float ArrivedDistance = 1f;

		private const float LaneSearchInterval = 0.5f;

		private float laneOffset;

		private TimeUntil nextLaneSearch;

		public virtual BasePlayer GetTarget()
		{
			if (!(Owner is LivestockAnimal { LeadingPlayer: var leadingPlayer }))
			{
				return null;
			}
			return leadingPlayer.Get(Owner.isServer);
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			nextLaneSearch = TimeUntil.op_Implicit(0f);
			if (!KeepUp())
			{
				return EFSMStateStatus.Failure;
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			if (!KeepUp())
			{
				return EFSMStateStatus.Failure;
			}
			return base.OnStateUpdate(deltaTime);
		}

		private bool KeepUp()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			BasePlayer target = GetTarget();
			if ((Object)(object)target == (Object)null)
			{
				return true;
			}
			Vector3 position = ((Component)target).transform.position;
			Vector3 val = LanePosition(target);
			if (!KeepUpWith(target, val))
			{
				if (val != position)
				{
					return KeepUpWith(target, position);
				}
				return false;
			}
			return true;
		}

		private Vector3 LanePosition(BasePlayer target)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(nextLaneSearch) <= 0f)
			{
				nextLaneSearch = TimeUntil.op_Implicit(0.5f);
				laneOffset = 0f;
				if (Owner is LivestockAnimal livestockAnimal)
				{
					livestockAnimal.CountWalkingAbreast(target, out var onLeft, out var onRight);
					laneOffset = (float)(onLeft - onRight) * 0.5f * laneWidth;
				}
			}
			Vector3 position = ((Component)target).transform.position;
			Vector3 val = Vector3Ex.NormalizeXZ(position - ((Component)Owner).transform.position);
			Vector3 val2 = position + Vector3.Cross(Vector3.up, val) * laneOffset;
			if (laneOffset != 0f && Agent.Raycast(Agent.WorldToNavSpace(position), Agent.WorldToNavSpace(val2), out var hitNS))
			{
				return Agent.NavToWorldSpace(hitNS.position);
			}
			return val2;
		}

		private bool KeepUpWith(BasePlayer target, Vector3 position)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			float num = Vector3.Distance(((Component)Owner).transform.position, position);
			float estimatedSpeed2D = target.estimatedSpeed2D;
			if (num <= 1f)
			{
				return true;
			}
			if (estimatedSpeed2D < 0.5f)
			{
				if (num > idealDistance)
				{
					RustNavMeshAgent agent = Agent;
					RustNavMeshAgent.Speeds? gait = maxSpeed;
					float? stoppingDistance = idealDistance;
					return agent.SetDestinationWithParams(position, autoBraking: true, gait, null, null, null, null, stoppingDistance);
				}
				if (Agent.hasPath)
				{
					RustNavMeshAgent agent2 = Agent;
					RustNavMeshAgent.Speeds? gait2 = minSpeed;
					float? stoppingDistance = Agent.emergencyDeceleration;
					return agent2.SetDestinationWithParams(position, autoBraking: true, gait2, null, stoppingDistance);
				}
				return true;
			}
			RustNavMeshAgent.Speeds value = ((num > idealDistance) ? maxSpeed : ClosestGait(estimatedSpeed2D * num / idealDistance));
			return Agent.SetDestinationWithParams(position, autoBraking: false, value);
		}

		private RustNavMeshAgent.Speeds ClosestGait(float speed)
		{
			RustNavMeshAgent.Speeds result = minSpeed;
			float num = float.MaxValue;
			for (RustNavMeshAgent.Speeds speeds = minSpeed; speeds <= maxSpeed; speeds++)
			{
				float num2 = Mathf.Abs(Agent.GetSpeedForGait(Agent.PenalisedGait(speeds)) - speed);
				if (num2 < num)
				{
					result = speeds;
					num = num2;
				}
			}
			return result;
		}
	}

	[Serializable]
	public class Trans_IsFollowingPlayer : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return livestockAnimal.IsLeading();
			}
			return false;
		}
	}

	[Serializable]
	public class State_TurnToLeader : State_TurnToTarget
	{
		protected override bool FindTurnTarget(out Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			position = Vector3.zero;
			if (Owner is LivestockAnimal { LeadingPlayer: var leadingPlayer })
			{
				BasePlayer basePlayer = leadingPlayer.Get(Owner.isServer);
				if (basePlayer != null)
				{
					position = ((Component)basePlayer).transform.position;
					return true;
				}
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_LeaderBehind : FSMTransitionBase
	{
		private const float BehindAngle = 120f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is LivestockAnimal { LeadingPlayer: var leadingPlayer } livestockAnimal)
			{
				BasePlayer basePlayer = leadingPlayer.Get(Owner.isServer);
				if (basePlayer != null)
				{
					if ((Agent.hasPath ? Agent.velocity.magnitude : 0f) >= Agent.jogSpeed || basePlayer.estimatedSpeed2D < 0.5f || !livestockAnimal.IsPulledAwayBy(basePlayer))
					{
						return false;
					}
					Vector3 val = Vector3Ex.NormalizeXZ(((Component)basePlayer).transform.position - ((Component)Owner).transform.position);
					return Vector3.Angle(Vector3Ex.NormalizeXZ(((Component)Owner).transform.forward), val) > 120f;
				}
			}
			return false;
		}
	}

	[Serializable]
	public class State_LivestockRoam : State_Roam
	{
		protected override Vector3 HomePosition
		{
			get
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				if (!(Owner is LivestockAnimal livestockAnimal))
				{
					return base.HomePosition;
				}
				return livestockAnimal.HomePosition;
			}
		}

		protected override bool HealsWhenUndisturbed => false;
	}

	[Serializable]
	public class State_CuriousFollow : State_Follow
	{
		[SerializeField]
		[Tooltip("How long (in seconds) after a walk that could not be finished before she sets off after somebody again.")]
		private float retryDelay = 15f;

		private bool walkFailed;

		public State_CuriousFollow()
		{
			minSpeed = RustNavMeshAgent.Speeds.Sneak;
			maxSpeed = RustNavMeshAgent.Speeds.Walk;
		}

		public override BasePlayer GetTarget()
		{
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return null;
			}
			return livestockAnimal.CuriousAbout;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			walkFailed = false;
			if (!(Owner is LivestockAnimal livestockAnimal) || (Object)(object)livestockAnimal.StartCuriousFollow() == (Object)null)
			{
				return Record(EFSMStateStatus.Failure);
			}
			return Record(base.OnStateEnter(payload));
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			return Record(base.OnStateUpdate(deltaTime));
		}

		public override void OnStateExit()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.StopCuriousFollow();
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (walkFailed && ((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				livestockAnimalFSM.NextCuriousFollow = TimeUntil.op_Implicit(retryDelay);
			}
			base.OnStateExit();
		}

		private EFSMStateStatus Record(EFSMStateStatus status)
		{
			walkFailed = status == EFSMStateStatus.Failure;
			return status;
		}
	}

	[Serializable]
	public class Trans_IsCuriousAboutPlayer : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal { ActsCurious: not false } livestockAnimal))
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && TimeUntil.op_Implicit(livestockAnimalFSM.NextCuriousFollow) > 0f)
			{
				return false;
			}
			return livestockAnimal.WouldFollowOutOfCuriosity(livestockAnimal.NearbyTrustedPlayer);
		}
	}

	[Serializable]
	public class Trans_HasKick : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return livestockAnimalFSM.kick.HasAnimation;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_HasCanterTurn : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return livestockAnimalFSM.turnCantering.HasTurnAnimation;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_StillCuriousAboutPlayer : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return (Object)(object)livestockAnimal.CuriousAbout != (Object)null;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_RecentlyHurt : FSMTransitionBase
	{
		[SerializeField]
		private float thresholdSeconds = 5f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is BaseCombatEntity { lastAttackedTime: >0f } baseCombatEntity)
			{
				return Time.time - baseCombatEntity.lastAttackedTime < thresholdSeconds;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_LoudNoiseNearby : FSMTransitionBase
	{
		[Tooltip("How close the noise has to have been. 0 is anywhere within earshot, which is what a cow runs from; the bull answers only for shots near his herd.")]
		[SerializeField]
		public float range;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return false;
			}
			if (!(range > 0f))
			{
				return livestockAnimal.RecentlyHeardLoudNoise;
			}
			return livestockAnimal.HeardLoudNoiseWithin(range);
		}
	}

	[Serializable]
	public class Trans_ChasedFromBehind : FSMTransitionBase
	{
		[Tooltip("How much clear air there may be between them and our body. Measured off the animal's bounds rather than its origin, so this is the gap you actually see.")]
		[SerializeField]
		public float range = 4f;

		[Tooltip("Past this many degrees off our forward counts as being on our back.")]
		[SerializeField]
		public float rearAngle = 100f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsInfant() || range <= 0f)
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !livestockAnimalFSM.kickBehind.HasAnimation)
			{
				return false;
			}
			if (!livestockAnimal.TryGetRememberedAggressor(out var attacker))
			{
				return false;
			}
			if (Trans_IsTargetProtectedByMount.IsProtected(attacker))
			{
				return false;
			}
			if (livestockAnimal.InSafeZone())
			{
				return false;
			}
			Vector3 position = ((Component)attacker).transform.position;
			if (Vector3.Distance(position, Owner.ClosestPoint(position)) > range)
			{
				return false;
			}
			Vector3 val = Vector3Ex.NormalizeXZ(Vector3Ex.WithY(position - ((Component)Owner).transform.position, 0f));
			if (val.sqrMagnitude > 0.001f && Vector3.Angle(((Component)Owner).transform.forward, val) < rearAngle)
			{
				return false;
			}
			Senses.TrySetTarget(attacker);
			return true;
		}
	}

	[Serializable]
	public class Trans_AggressorNearby : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.TryGetRememberedAggressor(out var attacker))
			{
				return false;
			}
			float grudgeRange = Livestock.grudgeRange;
			if (grudgeRange <= 0f || Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)attacker).transform.position) > grudgeRange)
			{
				return false;
			}
			Senses.TrySetTarget(attacker);
			return true;
		}
	}

	[Serializable]
	public abstract class Trans_IsConsumeLocationInRange : FSMTransitionBase
	{
		private enum DeployableSearch
		{
			None,
			Found,
			NothingToGain,
			Busy
		}

		[FormerlySerializedAs("barrelStandoff")]
		[SerializeField]
		[FormerlySerializedAs("troughStandoff")]
		private float standoff = 1.5f;

		private const float MuzzleHeight = 0.8f;

		private const int ConsumeBlockers = 2162688;

		[SerializeField]
		[FormerlySerializedAs("waterRetryDelay")]
		[FormerlySerializedAs("grassRetryDelay")]
		private float retryDelay = 30f;

		[SerializeField]
		[FormerlySerializedAs("drinkTopology")]
		[FormerlySerializedAs("grazeTopology")]
		private Enum topology;

		private TimeUntil nextTerrainSearch;

		private static readonly float[] StandingBearings = new float[13]
		{
			0f, 15f, -15f, 30f, -30f, 45f, -45f, 60f, -60f, 75f,
			-75f, 90f, -90f
		};

		private static BaseEntity RootOf(BaseEntity entity)
		{
			BaseEntity baseEntity = entity;
			while ((Object)(object)baseEntity != (Object)null)
			{
				BaseEntity parentEntity = baseEntity.GetParentEntity();
				if ((Object)(object)parentEntity == (Object)null)
				{
					break;
				}
				baseEntity = parentEntity;
			}
			return baseEntity;
		}

		protected Trans_IsConsumeLocationInRange(Enum defaultTopology)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			topology = defaultTopology;
		}

		protected abstract bool IsInNeed(LivestockAnimal self);

		protected abstract void FindDeployables(LivestockAnimal self, List<BaseEntity> results);

		protected virtual bool IsUsableTerrain(LivestockAnimal self, Vector3 position)
		{
			return true;
		}

		protected virtual void OnTerrainSearchFailed(LivestockAnimalFSM fsm)
		{
		}

		protected virtual float TerrainSearchRadius(LivestockAnimal self)
		{
			return self.ConsumeSearchRadius;
		}

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal self))
			{
				return false;
			}
			if (TOD_Sky.Instance.IsNight)
			{
				return false;
			}
			if (!IsInNeed(self))
			{
				return false;
			}
			LivestockAnimalFSM fsm = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref fsm))
			{
				return false;
			}
			if (TryFindTerrain(self, fsm, out var searched))
			{
				return true;
			}
			switch (TryFindDeployable(self, fsm))
			{
			case DeployableSearch.Found:
				return true;
			case DeployableSearch.NothingToGain:
				OnNothingToGainFromDeployables(self);
				return false;
			case DeployableSearch.Busy:
				return false;
			default:
				if (searched)
				{
					OnTerrainSearchFailed(fsm);
				}
				return false;
			}
		}

		protected virtual bool CanGainFromDeployables(LivestockAnimal self)
		{
			return true;
		}

		protected virtual void OnNothingToGainFromDeployables(LivestockAnimal self)
		{
		}

		private DeployableSearch TryFindDeployable(LivestockAnimal self, LivestockAnimalFSM fsm)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				FindDeployables(self, (List<BaseEntity>)(object)val);
				if (((List<BaseEntity>)(object)val).Count > 0 && !CanGainFromDeployables(self))
				{
					return DeployableSearch.NothingToGain;
				}
				PooledList<LivestockFootprint> val2 = Pool.Get<PooledList<LivestockFootprint>>();
				try
				{
					self.GatherRestingFootprints(((Component)self).transform.position, val2);
					float distanceSqr = -1f;
					BaseEntity baseEntity = null;
					Vector3 spot = default;
					float num = float.MinValue;
					for (int i = 0; i < 3; i++)
					{
						BaseEntity baseEntity2 = Eqs.NearestBeyond((List<BaseEntity>)(object)val, ((Component)self).transform.position, distanceSqr, out distanceSqr);
						if ((Object)(object)baseEntity2 == (Object)null)
						{
							break;
						}
						if (TryFindStandingSpot(self, baseEntity2, val2, out var spot2, out var clearance))
						{
							if (LivestockAnimal.IsClear(clearance))
							{
								return Found(fsm, baseEntity2, spot2);
							}
							if (clearance > num)
							{
								baseEntity = baseEntity2;
								spot = spot2;
								num = clearance;
							}
						}
					}
					if ((Object)(object)baseEntity == (Object)null)
					{
						return DeployableSearch.None;
					}
					return (num < 0f) ? DeployableSearch.Busy : Found(fsm, baseEntity, spot);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		private bool TryFindStandingSpot(LivestockAnimal self, BaseEntity deployable, PooledList<LivestockFootprint> taken, out Vector3 spot, out float clearance)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			spot = default;
			clearance = float.MinValue;
			Vector3 val = Vector3Ex.WithY(((Component)self).transform.position - ((Component)deployable).transform.position, 0f);
			Vector3 val2 = ((val.sqrMagnitude > 0.01f) ? val.normalized : ((Component)self).transform.forward);
			PooledList<Vector3> val3 = Pool.Get<PooledList<Vector3>>();
			try
			{
				PooledList<float> val4 = Pool.Get<PooledList<float>>();
				try
				{
					float[] standingBearings = StandingBearings;
					foreach (float num in standingBearings)
					{
						Vector3 positionWS = ((Component)deployable).transform.position + Quaternion.Euler(0f, num, 0f) * val2 * standoff;
						if (Agent.SamplePosition(positionWS, out var hitWS, 2f))
						{
							((List<Vector3>)(object)val3).Add(hitWS.position);
							((List<float>)(object)val4).Add(LivestockAnimal.ClearanceOf(taken, self.RestingFootprintFor(hitWS.position, Agent.stoppingDistance)));
						}
					}
					for (int j = 0; j < ((List<Vector3>)(object)val3).Count; j++)
					{
						if (LivestockAnimal.IsClear(((List<float>)(object)val4)[j]))
						{
							if (CanUseFrom(deployable, ((List<Vector3>)(object)val3)[j]))
							{
								spot = ((List<Vector3>)(object)val3)[j];
								clearance = ((List<float>)(object)val4)[j];
								return true;
							}
							((List<float>)(object)val4)[j] = float.MinValue;
						}
					}
					int num2;
					while (true)
					{
						num2 = -1;
						for (int k = 0; k < ((List<Vector3>)(object)val3).Count; k++)
						{
							if (((List<float>)(object)val4)[k] > float.MinValue && (num2 < 0 || ((List<float>)(object)val4)[k] > ((List<float>)(object)val4)[num2]))
							{
								num2 = k;
							}
						}
						if (num2 < 0)
						{
							return false;
						}
						if (CanUseFrom(deployable, ((List<Vector3>)(object)val3)[num2]))
						{
							break;
						}
						((List<float>)(object)val4)[num2] = float.MinValue;
					}
					spot = ((List<Vector3>)(object)val3)[num2];
					clearance = ((List<float>)(object)val4)[num2];
					return true;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}

		private bool CanUseFrom(BaseEntity deployable, Vector3 spot)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (!Agent.CanReach(spot))
			{
				return false;
			}
			BaseEntity baseEntity = RootOf(deployable);
			return GamePhysics.LineOfSight(spot + Vector3.up * 0.8f, baseEntity.CenterPoint(), 2162688, baseEntity);
		}

		private static DeployableSearch Found(LivestockAnimalFSM fsm, BaseEntity deployable, Vector3 spot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			fsm.ConsumeLocation = spot;
			fsm.ConsumeSource.Set(deployable);
			return DeployableSearch.Found;
		}

		private bool TryFindTerrain(LivestockAnimal self, LivestockAnimalFSM fsm, out bool searched)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			searched = false;
			if (TimeUntil.op_Implicit(nextTerrainSearch) > 0f)
			{
				return false;
			}
			searched = true;
			NavVector3 nextPosition = Agent.nextPosition;
			if (self.IsInfant() && (Object)(object)self.MotherAnimal != (Object)null)
			{
				nextPosition = ((Component)self.MotherAnimal).GetComponent<RustNavMeshAgent>().nextPosition;
			}
			float num = TerrainSearchRadius(self);
			PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
			try
			{
				Eqs.SampleNavigablePositions(Agent, nextPosition, (List<NavVector3>)(object)val, num, num * 0.25f, 16);
				PooledList<LivestockFootprint> val2 = Pool.Get<PooledList<LivestockFootprint>>();
				try
				{
					self.GatherRestingFootprints(nextPosition.Value, val2);
					bool flag = false;
					bool flag2 = false;
					float num2 = float.MaxValue;
					float num3 = float.MinValue;
					foreach (NavVector3 item in (List<NavVector3>)(object)val)
					{
						if (Agent.SamplePosition(item, out var hitNS, 10f) && Agent.IsPositionAtTopologyRequirement(hitNS.position, topology) && IsOnOpenGround(hitNS.position.Value) && IsUsableTerrain(self, hitNS.position.Value))
						{
							float num4 = LivestockAnimal.ClearanceOf(val2, self.RestingFootprintFor(hitNS.position.Value, Agent.stoppingDistance));
							bool flag3 = LivestockAnimal.IsClear(num4);
							Vector3 val3 = hitNS.position.Value - nextPosition.Value;
							float sqrMagnitude = val3.sqrMagnitude;
							bool flag4;
							if (flag3 != flag2)
							{
								flag4 = flag3;
							}
							else
							{
								flag4 = (flag3 ? (sqrMagnitude < num2) : (num4 > num3));
							}
							if (flag4)
							{
								num2 = sqrMagnitude;
								num3 = num4;
								flag2 = flag3;
								fsm.ConsumeLocation = hitNS.position.Value;
								fsm.ConsumeSource = default;
								flag = true;
							}
						}
					}
					if (flag && Agent.CanReach(fsm.ConsumeLocation.Value))
					{
						return true;
					}
					fsm.ConsumeLocation = null;
					nextTerrainSearch = TimeUntil.op_Implicit(retryDelay);
					return false;
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		private static bool IsOnOpenGround(Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return !GamePhysics.CheckSphere(GamePhysics.Realm.Server, position - Vector3.up * 0.2f, 0.25f, 2097408, (QueryTriggerInteraction)1);
		}
	}

	[Serializable]
	public class Trans_IsGrazeLocationInRange : Trans_IsConsumeLocationInRange
	{
		[SerializeField]
		private float searchRadius = 10f;

		[SerializeField]
		[Tooltip("Ground that counts as grass, compared with the splat that dominates the spot. Field topology alone covers snow, sand and bare dirt.")]
		[InspectorFlags]
		private Enum grazingSplats = (Enum)48;

		[SerializeField]
		[Tooltip("Biomes with grass to graze. Anywhere else the herd has to be fed at a trough.")]
		private Enum grazingBiomes = (Enum)23;

		public Trans_IsGrazeLocationInRange()
			: base((Enum)97)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		}

		protected override bool IsInNeed(LivestockAnimal self)
		{
			return self.NeedsFood();
		}

		protected override void FindDeployables(LivestockAnimal self, List<BaseEntity> results)
		{
			self.FindFoodTroughs(results);
		}

		protected override bool CanGainFromDeployables(LivestockAnimal self)
		{
			return self.Fullness.NeedValue < self.TroughFullnessCeiling;
		}

		protected override void OnNothingToGainFromDeployables(LivestockAnimal self)
		{
			self.RecordHelpingSource(fromTrough: true);
		}

		protected override bool IsUsableTerrain(LivestockAnimal self, Vector3 position)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (Agent.IsPositionASplatRequirement(position, grazingSplats) && Agent.IsPositionABiomeRequirement(position, grazingBiomes))
			{
				return !OvergrazedArea.IsOvergrazed(position);
			}
			return false;
		}

		protected override float TerrainSearchRadius(LivestockAnimal self)
		{
			return searchRadius;
		}

		protected override void OnTerrainSearchFailed(LivestockAnimalFSM fsm)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			fsm.LastFoodSearchFailed = TimeSince.op_Implicit(0f);
		}
	}

	[Serializable]
	public abstract class State_ConsumeAtLocation : FSMStateBase
	{
		private enum Phase
		{
			Moving,
			Consuming
		}

		[SerializeField]
		private Vector2 distanceRange = new Vector2(10f, 20f);

		[SerializeField]
		private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

		[SerializeField]
		[FormerlySerializedAs("grazeDurationRange")]
		[FormerlySerializedAs("drinkDurationRange")]
		private Vector2 durationRange = new Vector2(5f, 15f);

		[SerializeField]
		private float postAnimationDuration = 3f;

		[SerializeField]
		private RustNavMeshAgent.Speeds minSpeed;

		private Phase phase;

		private TimeUntil endTime;

		private EntityRef<BaseEntity> source;

		public float MaxDuration => durationRange.y;

		protected abstract bool CanConsumeFrom(LivestockAnimal self, BaseEntity source);

		protected abstract void Consume(LivestockAnimal self, BaseEntity source);

		protected abstract void SetConsuming(LivestockAnimal self, bool consuming);

		protected virtual bool HasNothingLeftToGain(LivestockAnimal self)
		{
			return false;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !livestockAnimalFSM.ConsumeLocation.HasValue)
			{
				return EFSMStateStatus.Failure;
			}
			Vector3 value = livestockAnimalFSM.ConsumeLocation.Value;
			source = livestockAnimalFSM.ConsumeSource;
			livestockAnimalFSM.ConsumeLocation = null;
			livestockAnimalFSM.ConsumeSource = default;
			if (!Agent.SamplePosition(value, out var hitWS, 10f))
			{
				return EFSMStateStatus.Failure;
			}
			if (!MoveWithRampedSpeed(Agent, hitWS.position, distanceRange.y, minSpeed, maxSpeed))
			{
				return EFSMStateStatus.Failure;
			}
			phase = Phase.Moving;
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal self))
			{
				return EFSMStateStatus.Failure;
			}
			switch (phase)
			{
			case Phase.Moving:
			{
				if (Agent.hasPath)
				{
					return base.OnStateUpdate(deltaTime);
				}
				BaseEntity baseEntity = source.Get(serverside: true);
				if ((Object)(object)baseEntity != (Object)null && !CanConsumeFrom(self, baseEntity))
				{
					return EFSMStateStatus.Failure;
				}
				Consume(self, baseEntity);
				phase = Phase.Consuming;
				endTime = TimeUntil.op_Implicit(Random.Range(durationRange.x, durationRange.y));
				SetConsuming(self, consuming: true);
				return base.OnStateUpdate(deltaTime);
			}
			case Phase.Consuming:
				if (HasNothingLeftToGain(self))
				{
					SetConsuming(self, consuming: false);
					return EFSMStateStatus.Success;
				}
				if (TimeUntil.op_Implicit(endTime) <= postAnimationDuration)
				{
					SetConsuming(self, consuming: false);
				}
				if (!(TimeUntil.op_Implicit(endTime) <= 0f))
				{
					return EFSMStateStatus.None;
				}
				return EFSMStateStatus.Success;
			default:
				return base.OnStateUpdate(deltaTime);
			}
		}

		public override void OnStateExit()
		{
			Agent.ResetPath();
			source = default;
			if (Owner is LivestockAnimal self)
			{
				SetConsuming(self, consuming: false);
			}
			base.OnStateExit();
		}

		protected State_ConsumeAtLocation()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_Graze : State_ConsumeAtLocation
	{
		protected override bool CanConsumeFrom(LivestockAnimal self, BaseEntity source)
		{
			if (source is HitchTrough trough)
			{
				Item foundItem;
				return self.CanEatFrom(trough, out foundItem);
			}
			return false;
		}

		protected override void Consume(LivestockAnimal self, BaseEntity source)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			HitchTrough hitchTrough = source as HitchTrough;
			self.SetGrazingFromTrough(hitchTrough);
			self.RecordHelpingSource((Object)(object)hitchTrough != (Object)null);
			if ((Object)(object)hitchTrough == (Object)null)
			{
				OvergrazedArea.RegisterGraze(((Component)self).transform.position);
			}
		}

		protected override void SetConsuming(LivestockAnimal self, bool consuming)
		{
			self.SetGrazing(consuming);
			if (!consuming)
			{
				self.SetGrazingFromTrough(null);
			}
		}

		protected override bool HasNothingLeftToGain(LivestockAnimal self)
		{
			if (!self.IsGrazingFromTrough)
			{
				return self.Fullness.NeedValue >= 1f;
			}
			if (!(self.Fullness.NeedValue >= self.TroughFullnessCeiling))
			{
				return !self.HasTroughFoodLeft;
			}
			return true;
		}
	}

	[Serializable]
	public class Trans_IsDrinkSourceInRange : Trans_IsConsumeLocationInRange
	{
		public Trans_IsDrinkSourceInRange()
			: base((Enum)245760)
		{
		}

		protected override bool IsInNeed(LivestockAnimal self)
		{
			return self.NeedsWater();
		}

		protected override void FindDeployables(LivestockAnimal self, List<BaseEntity> results)
		{
			self.FindDrinkBarrels(results);
		}

		protected override bool IsUsableTerrain(LivestockAnimal self, Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return self.IsDrinkableShore(position);
		}
	}

	[Serializable]
	public class State_Drink : State_ConsumeAtLocation
	{
		protected override bool CanConsumeFrom(LivestockAnimal self, BaseEntity source)
		{
			if (source is LiquidContainer container)
			{
				return self.CanDrinkFrom(container);
			}
			return false;
		}

		protected override void Consume(LivestockAnimal self, BaseEntity source)
		{
			if (source is LiquidContainer container)
			{
				self.ConsumeFromBarrel(container);
			}
		}

		protected override void SetConsuming(LivestockAnimal self, bool consuming)
		{
			self.SetDrinking(consuming);
		}
	}

	[Serializable]
	public class State_Patrol_Points : FSMStateBase
	{
		public bool Enabled;

		[SerializeField]
		private Transform[] TargetPoints;

		[Tooltip("Speeds are cycled through, one every 'speedCycleInterval' seconds, to preview movement animations")]
		[SerializeField]
		private RustNavMeshAgent.Speeds[] speeds = new RustNavMeshAgent.Speeds[4]
		{
			RustNavMeshAgent.Speeds.Walk,
			RustNavMeshAgent.Speeds.Jog,
			RustNavMeshAgent.Speeds.Run,
			RustNavMeshAgent.Speeds.Sprint
		};

		[Tooltip("How often (in seconds) to advance to the next speed in the array")]
		[SerializeField]
		private float speedCycleInterval = 10f;

		private int currentPointIndex;

		private int currentSpeedIndex;

		private double nextSpeedChangeTime;

		private RustNavMeshAgent.Speeds CurrentSpeed
		{
			get
			{
				if (speeds == null || speeds.Length == 0)
				{
					return RustNavMeshAgent.Speeds.Walk;
				}
				return speeds[currentSpeedIndex];
			}
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			if (TargetPoints == null || TargetPoints.Length == 0)
			{
				if (AI.logIssues)
				{
					Debug.LogError((object)$"[FSM] {Name} has no patrol points set on {Owner}", (Object)(object)Owner);
				}
				return EFSMStateStatus.Failure;
			}
			currentPointIndex = 0;
			currentSpeedIndex = 0;
			nextSpeedChangeTime = Time.timeAsDouble + (double)speedCycleInterval;
			if (!MoveToCurrentPoint())
			{
				return EFSMStateStatus.Failure;
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			if (speeds != null && speeds.Length != 0 && Time.timeAsDouble >= nextSpeedChangeTime)
			{
				currentSpeedIndex = (currentSpeedIndex + 1) % speeds.Length;
				nextSpeedChangeTime = Time.timeAsDouble + (double)speedCycleInterval;
				Agent.SetGait(speeds[currentSpeedIndex]);
			}
			if (!Agent.hasPath)
			{
				currentPointIndex = (currentPointIndex + 1) % TargetPoints.Length;
				if (!MoveToCurrentPoint())
				{
					return EFSMStateStatus.Failure;
				}
			}
			return base.OnStateUpdate(deltaTime);
		}

		public override void OnStateExit()
		{
			Agent.ResetPath();
			base.OnStateExit();
		}

		private bool MoveToCurrentPoint()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Transform val = TargetPoints[currentPointIndex];
			if ((Object)(object)val == (Object)null)
			{
				return false;
			}
			if (!Agent.SamplePosition(val.position, out var hitWS, 10f))
			{
				return false;
			}
			return Agent.SetDestinationWithParams(hitWS.position, autoBraking: true, CurrentSpeed);
		}
	}

	[Serializable]
	public class Trans_CanBreed : FSMSlowTransitionBase
	{
		[Tooltip("How long (in seconds) before searching again after finding nobody worth walking to")]
		[SerializeField]
		private float retryDelay = 15f;

		protected override bool EvaluateAtInterval(ref FSMPayload payload)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("Trans_CanBreed"))
			{
				if (!Livestock.breedingEnabled)
				{
					return false;
				}
				if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.CanBreed())
				{
					return false;
				}
				if (!livestockAnimal.IsHappy)
				{
					return false;
				}
				LivestockAnimalFSM livestockAnimalFSM = default;
				if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || TimeUntil.op_Implicit(livestockAnimalFSM.NextMateSearch) > 0f)
				{
					return false;
				}
				PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
				try
				{
					BaseEntity.Query.Server.GetBrainsInSphere(((Component)Owner).transform.position, Livestock.breedSearchRadius, (List<LivestockAnimal>)(object)val);
					for (int num = ((List<LivestockAnimal>)(object)val).Count - 1; num >= 0; num--)
					{
						if (!IsValidMate(livestockAnimal, ((List<LivestockAnimal>)(object)val)[num]))
						{
							((List<LivestockAnimal>)(object)val).RemoveAt(num);
						}
					}
					float distanceSqr = -1f;
					for (int i = 0; i < 3; i++)
					{
						LivestockAnimal livestockAnimal2 = Eqs.NearestBeyond((List<LivestockAnimal>)(object)val, ((Component)Owner).transform.position, distanceSqr, out distanceSqr);
						if ((Object)(object)livestockAnimal2 == (Object)null)
						{
							break;
						}
						if (Agent.CanReach(((Component)livestockAnimal2).transform.position))
						{
							livestockAnimalFSM.MateTarget.Set(livestockAnimal2);
							return true;
						}
					}
					livestockAnimalFSM.NextMateSearch = TimeUntil.op_Implicit(retryDelay);
					return false;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}

		private static bool IsValidMate(LivestockAnimal self, LivestockAnimal candidate)
		{
			if (!self.IsHerdMate(candidate))
			{
				return false;
			}
			if (!candidate.CanBePregnant() || candidate.IsLeading())
			{
				return false;
			}
			if (!candidate.IsHappy || !self.IsHappy)
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)candidate).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return false;
			}
			LivestockAnimal livestockAnimal = livestockAnimalFSM.MateSuitor.Get(serverside: true);
			if (!((Object)(object)livestockAnimal == (Object)null))
			{
				return (Object)(object)livestockAnimal == (Object)(object)self;
			}
			return true;
		}
	}

	[Serializable]
	public class Trans_HasMateRequest : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("Trans_HasMateRequest"))
			{
				LivestockAnimalFSM livestockAnimalFSM = default;
				if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || TimeUntil.op_Implicit(livestockAnimalFSM.MateRequestExpiry) <= 0f)
				{
					return false;
				}
				return (Object)(object)livestockAnimalFSM.MateSuitor.Get(serverside: true) != (Object)null;
			}
		}
	}

	[Serializable]
	public class State_SeekMate : FSMStateBase
	{
		private enum Phase
		{
			Approaching,
			Withdrawing
		}

		[SerializeField]
		private RustNavMeshAgent.Speeds minSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Jog;

		[Tooltip("Distance at which the male moves at maxSpeed; he eases off as he closes in")]
		[SerializeField]
		private float fullSpeedDistance = 20f;

		[SerializeField]
		[Tooltip("How close the male has to get before he has met her and leads her away")]
		private float breedDistance = 2f;

		[Tooltip("Give up if the walk over takes longer than this")]
		[SerializeField]
		private float approachTimeout = 30f;

		[Tooltip("How far off the pair look for somewhere quiet, once he has reached her. Zero has them stay where they met.")]
		[SerializeField]
		private Vector2 withdrawRange = new Vector2(12f, 25f);

		[Tooltip("How much room the quiet spot needs, with no other herd mate standing inside it.")]
		[SerializeField]
		private float withdrawClearance = 10f;

		[Tooltip("Give up on the walk out and settle where they got to after this long. Counted in the 45 seconds a pair can be away from the herd before their Social runs down.")]
		[SerializeField]
		private float withdrawTimeout = 20f;

		[Tooltip("How long (in seconds) after leaving this state before looking for a mate again")]
		[SerializeField]
		private float retryDelay = 15f;

		private Phase phase;

		private EntityRef<LivestockAnimal> mate;

		private TimeUntil approachDeadline;

		private TimeUntil withdrawDeadline;

		private Vector3 quietSpot;

		private bool handingOver;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			mate = default;
			handingOver = false;
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!(Owner is LivestockAnimal self) || !((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal = livestockAnimalFSM.MateTarget.Get(serverside: true);
			livestockAnimalFSM.MateTarget = default;
			if ((Object)(object)livestockAnimal == (Object)null || !livestockAnimal.CanBePregnant() || !TryClaim(self, livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			mate.Set(livestockAnimal);
			livestockAnimalFSM.MatePartner.Set(livestockAnimal);
			phase = Phase.Approaching;
			approachDeadline = TimeUntil.op_Implicit(approachTimeout);
			Senses.TrySetTarget(livestockAnimal);
			if (Vector3.Distance(((Component)Owner).transform.position, ((Component)livestockAnimal).transform.position) <= breedDistance)
			{
				return StartWithdrawing(self, livestockAnimal);
			}
			if (!MoveToMate(livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal2 = mate.Get(serverside: true);
			if ((Object)(object)livestockAnimal2 == (Object)null || !livestockAnimal2.CanBePregnant())
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)livestockAnimal2).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || (Object)(object)livestockAnimalFSM.MateSuitor.Get(serverside: true) != (Object)(object)livestockAnimal)
			{
				return EFSMStateStatus.Failure;
			}
			switch (phase)
			{
			case Phase.Approaching:
			{
				if (TimeUntil.op_Implicit(approachDeadline) <= 0f)
				{
					return EFSMStateStatus.Failure;
				}
				Vector3 position = ((Component)livestockAnimal2).transform.position;
				if (Vector3.Distance(((Component)Owner).transform.position, position) <= breedDistance)
				{
					return StartWithdrawing(livestockAnimal, livestockAnimal2);
				}
				if (Vector3.Distance(Agent.destination.Value, position) > breedDistance * 3f && !MoveToMate(livestockAnimal2))
				{
					return EFSMStateStatus.Failure;
				}
				if (!Agent.hasPath)
				{
					return EFSMStateStatus.Failure;
				}
				return base.OnStateUpdate(deltaTime);
			}
			case Phase.Withdrawing:
				if (TimeUntil.op_Implicit(withdrawDeadline) <= 0f || Vector3.Distance(((Component)Owner).transform.position, quietSpot) <= breedDistance || !Agent.hasPath)
				{
					return HandOver();
				}
				return base.OnStateUpdate(deltaTime);
			default:
				return base.OnStateUpdate(deltaTime);
			}
		}

		private EFSMStateStatus StartWithdrawing(LivestockAnimal self, LivestockAnimal target)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			if (withdrawRange.y <= 0f || !self.TryFindSecludedSpot(Agent, target, withdrawRange.x, withdrawRange.y, withdrawClearance, out quietSpot))
			{
				return HandOver();
			}
			if (!MoveWithRampedSpeed(Agent, quietSpot, fullSpeedDistance, minSpeed, maxSpeed))
			{
				return HandOver();
			}
			phase = Phase.Withdrawing;
			withdrawDeadline = TimeUntil.op_Implicit(withdrawTimeout);
			return EFSMStateStatus.None;
		}

		private EFSMStateStatus HandOver()
		{
			handingOver = true;
			return EFSMStateStatus.Success;
		}

		public override void OnStateExit()
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetMating(mating: false);
				LivestockAnimal livestockAnimal2 = mate.Get(serverside: true);
				LivestockAnimalFSM livestockAnimalFSM = default;
				if (!handingOver && (Object)(object)livestockAnimal2 != (Object)null && ((Component)livestockAnimal2).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && (Object)(object)livestockAnimalFSM.MateSuitor.Get(serverside: true) == (Object)(object)livestockAnimal)
				{
					livestockAnimalFSM.MateSuitor = default;
					livestockAnimal2.SetMating(mating: false);
				}
			}
			mate = default;
			LivestockAnimalFSM livestockAnimalFSM2 = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM2))
			{
				livestockAnimalFSM2.NextMateSearch = TimeUntil.op_Implicit(retryDelay);
				if (!handingOver)
				{
					livestockAnimalFSM2.MatePartner = default;
				}
			}
			base.OnStateExit();
		}

		private bool TryClaim(LivestockAnimal self, LivestockAnimal target)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			LivestockAnimalFSM livestockAnimalFSM = default;
			LivestockAnimalFSM livestockAnimalFSM2 = default;
			if (!((Component)target).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !((Component)self).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM2))
			{
				return false;
			}
			LivestockAnimal livestockAnimal = livestockAnimalFSM.MateSuitor.Get(serverside: true);
			if ((Object)(object)livestockAnimal != (Object)null && (Object)(object)livestockAnimal != (Object)(object)self)
			{
				return false;
			}
			livestockAnimalFSM.MateSuitor.Set(self);
			livestockAnimalFSM.MateRequestExpiry = TimeUntil.op_Implicit(approachTimeout + withdrawTimeout + livestockAnimalFSM2.frolic.MaxDuration + livestockAnimalFSM2.mate.MaxDuration + 5f);
			return true;
		}

		private bool MoveToMate(LivestockAnimal target)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return MoveWithRampedSpeed(Agent, ((Component)target).transform.position, fullSpeedDistance, minSpeed, maxSpeed);
		}

		public State_SeekMate()
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_AwaitMate : FSMStateBase
	{
		[SerializeField]
		private RustNavMeshAgent.Speeds minSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Jog;

		[SerializeField]
		[Tooltip("How far she lets him get before she follows. Inside it she stands and waits.")]
		private float courtDistance = 6f;

		[Tooltip("How far he has to move from where she was already headed before she re-paths.")]
		[SerializeField]
		private float repathTolerance = 3f;

		[Tooltip("Distance at which she moves at maxSpeed; she eases off as she closes in")]
		[SerializeField]
		private float fullSpeedDistance = 15f;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal = livestockAnimalFSM.MateSuitor.Get(serverside: true);
			if ((Object)(object)livestockAnimal == (Object)null)
			{
				return EFSMStateStatus.Failure;
			}
			livestockAnimalFSM.MatePartner.Set(livestockAnimal);
			Agent.ResetPath();
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			if (livestockAnimal.IsPregnant())
			{
				return EFSMStateStatus.Success;
			}
			if (livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || TimeUntil.op_Implicit(livestockAnimalFSM.MateRequestExpiry) <= 0f)
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal2 = livestockAnimalFSM.MateSuitor.Get(serverside: true);
			if ((Object)(object)livestockAnimal2 == (Object)null || livestockAnimal2.IsDead())
			{
				return EFSMStateStatus.Failure;
			}
			KeepUpWith(livestockAnimal2);
			return base.OnStateUpdate(deltaTime);
		}

		private void KeepUpWith(LivestockAnimal suitor)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)suitor).transform.position;
			if (Vector3.Distance(((Component)Owner).transform.position, position) <= courtDistance)
			{
				if (Agent.hasPath)
				{
					Agent.ResetPath();
				}
			}
			else if (!Agent.hasPath || !(Vector3.Distance(Agent.destination.Value, position) <= repathTolerance))
			{
				MoveWithRampedSpeed(Agent, position, fullSpeedDistance, minSpeed, maxSpeed);
			}
		}

		public override void OnStateExit()
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetMating(mating: false);
			}
			base.OnStateExit();
		}
	}

	[Serializable]
	public class Trans_SuitorIsFrolicking : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return false;
			}
			LivestockAnimal livestockAnimal = livestockAnimalFSM.MateSuitor.Get(serverside: true);
			LivestockAnimalFSM livestockAnimalFSM2 = default;
			if ((Object)(object)livestockAnimal != (Object)null && ((Component)livestockAnimal).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM2))
			{
				return livestockAnimalFSM2.CurrentState == livestockAnimalFSM2.frolic;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_IsMale : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return livestockAnimal.IsMale;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_IsCourting : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return livestockAnimalFSM.WantsToBeAlone;
			}
			return false;
		}
	}

	[Serializable]
	public class State_Frolic : State_Circle
	{
		[Tooltip("How long (in seconds) the pair circle each other before they settle. Counted in the 45 seconds a pair can be away from the herd before their Social runs down.")]
		[SerializeField]
		private float duration = 12f;

		[SerializeField]
		[Tooltip("The gait for the stretches between canters. Circling at one speed for the whole frolic reads as a loop rather than as play.")]
		private RustNavMeshAgent.Speeds walkGait = RustNavMeshAgent.Speeds.Walk;

		[Tooltip("How long a stretch of cantering, or of walking, lasts before it swaps to the other. Rolled fresh each time so the two of them fall in and out of step.")]
		[SerializeField]
		private Vector2 gaitStretchRange = new Vector2(1.5f, 3.5f);

		private TimeUntil frolicEnd;

		private RustNavMeshAgent.Speeds canterGait;

		private TimeUntil nextGaitStretch;

		private bool cantering;

		public float MaxDuration => duration;

		public State_Frolic()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			radius = 5f;
			speed = RustNavMeshAgent.Speeds.Run;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			canterGait = speed;
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading() || !((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return EFSMStateStatus.Failure;
			}
			if ((Object)(object)livestockAnimalFSM.MatePartner.Get(serverside: true) == (Object)null)
			{
				return EFSMStateStatus.Failure;
			}
			frolicEnd = TimeUntil.op_Implicit(duration);
			EFSMStateStatus eFSMStateStatus = base.OnStateEnter(payload);
			if (eFSMStateStatus == EFSMStateStatus.Failure)
			{
				return eFSMStateStatus;
			}
			StartStretch(livestockAnimal, livestockAnimalFSM, canter: true);
			return eFSMStateStatus;
		}

		private void StartStretch(LivestockAnimal self, LivestockAnimalFSM fsm, bool canter)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			cantering = canter;
			speed = (canter ? canterGait : walkGait);
			nextGaitStretch = TimeUntil.op_Implicit(Random.Range(gaitStretchRange.x, gaitStretchRange.y));
			self.SetFrisky(canter && fsm.friskyCanter.HasFriskyCanter);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			if (TimeUntil.op_Implicit(frolicEnd) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			if ((Object)(object)GetPartner() == (Object)null)
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimalFSM fsm = default;
			if (TimeUntil.op_Implicit(nextGaitStretch) <= 0f && ((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref fsm))
			{
				StartStretch(livestockAnimal, fsm, !cantering);
			}
			EFSMStateStatus eFSMStateStatus = base.OnStateUpdate(deltaTime);
			if (eFSMStateStatus != EFSMStateStatus.Failure)
			{
				return eFSMStateStatus;
			}
			return EFSMStateStatus.None;
		}

		public override void OnStateExit()
		{
			speed = canterGait;
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetFrisky(frisky: false);
			}
			base.OnStateExit();
		}

		protected override bool GetCircleOrigin(out Vector3 origin)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			origin = default;
			LivestockAnimal partner = GetPartner();
			if ((Object)(object)partner == (Object)null)
			{
				return false;
			}
			origin = ((Component)partner).transform.position;
			return true;
		}

		private LivestockAnimal GetPartner()
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return null;
			}
			return livestockAnimalFSM.MatePartner.Get(serverside: true);
		}
	}

	[Serializable]
	public class State_Mate : FSMStateBase
	{
		private enum Phase
		{
			Closing,
			Mating,
			Lingering
		}

		[SerializeField]
		private RustNavMeshAgent.Speeds minSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Jog;

		[Tooltip("Distance at which he moves at maxSpeed; he eases off as he closes in")]
		[SerializeField]
		private float fullSpeedDistance = 10f;

		[Tooltip("How close he has to get before mating starts")]
		[SerializeField]
		private float breedDistance = 2f;

		[SerializeField]
		[Tooltip("Longest he spends closing the last of the distance to her")]
		private float closeTimeout = 10f;

		[Tooltip("How long (in seconds) the pair stand together before she becomes pregnant")]
		[SerializeField]
		private float matingDuration = 3f;

		[SerializeField]
		[Tooltip("How long he stays with her afterwards, watching her go down, before he gets on with his day. Counted in the 45 seconds a pair can be away from the herd.")]
		private float afterMatingLinger = 8f;

		[Tooltip("How long (in seconds) after leaving this state before looking for a mate again")]
		[SerializeField]
		private float retryDelay = 15f;

		private Phase phase;

		private EntityRef<LivestockAnimal> mate;

		private TimeUntil closeDeadline;

		private TimeUntil matingEndTime;

		private TimeUntil lingerEndTime;

		public float MaxDuration => closeTimeout + matingDuration + afterMatingLinger;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			mate = default;
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading() || !((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal2 = livestockAnimalFSM.MatePartner.Get(serverside: true);
			if ((Object)(object)livestockAnimal2 == (Object)null || !livestockAnimal2.CanBePregnant() || !StillClaimedBy(livestockAnimal, livestockAnimal2))
			{
				return EFSMStateStatus.Failure;
			}
			mate.Set(livestockAnimal2);
			phase = Phase.Closing;
			closeDeadline = TimeUntil.op_Implicit(closeTimeout);
			Senses.TrySetTarget(livestockAnimal2);
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal2 = mate.Get(serverside: true);
			if ((Object)(object)livestockAnimal2 == (Object)null || !StillClaimedBy(livestockAnimal, livestockAnimal2))
			{
				return EFSMStateStatus.Failure;
			}
			switch (phase)
			{
			case Phase.Closing:
				if (!livestockAnimal2.CanBePregnant())
				{
					return EFSMStateStatus.Failure;
				}
				if (Vector3.Distance(((Component)Owner).transform.position, ((Component)livestockAnimal2).transform.position) <= breedDistance)
				{
					Agent.ResetPath();
					phase = Phase.Mating;
					matingEndTime = TimeUntil.op_Implicit(matingDuration);
					livestockAnimal.SetMating(mating: true);
					livestockAnimal2.SetMating(mating: true);
					return EFSMStateStatus.None;
				}
				if (TimeUntil.op_Implicit(closeDeadline) <= 0f)
				{
					return EFSMStateStatus.Failure;
				}
				MoveWithRampedSpeed(Agent, ((Component)livestockAnimal2).transform.position, fullSpeedDistance, minSpeed, maxSpeed);
				return base.OnStateUpdate(deltaTime);
			case Phase.Mating:
				if (TimeUntil.op_Implicit(matingEndTime) > 0f)
				{
					return EFSMStateStatus.None;
				}
				if (!livestockAnimal2.MakePregnant(livestockAnimal))
				{
					return EFSMStateStatus.Failure;
				}
				livestockAnimal.SetMating(mating: false);
				livestockAnimal2.SetMating(mating: false);
				phase = Phase.Lingering;
				lingerEndTime = TimeUntil.op_Implicit(afterMatingLinger);
				return EFSMStateStatus.None;
			case Phase.Lingering:
				if (!(TimeUntil.op_Implicit(lingerEndTime) > 0f))
				{
					return EFSMStateStatus.Success;
				}
				return EFSMStateStatus.None;
			default:
				return base.OnStateUpdate(deltaTime);
			}
		}

		public override void OnStateExit()
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetMating(mating: false);
				LivestockAnimal livestockAnimal2 = mate.Get(serverside: true);
				LivestockAnimalFSM livestockAnimalFSM = default;
				if ((Object)(object)livestockAnimal2 != (Object)null && ((Component)livestockAnimal2).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && (Object)(object)livestockAnimalFSM.MateSuitor.Get(serverside: true) == (Object)(object)livestockAnimal)
				{
					livestockAnimalFSM.MateSuitor = default;
					livestockAnimalFSM.MatePartner = default;
					livestockAnimal2.SetMating(mating: false);
				}
			}
			mate = default;
			LivestockAnimalFSM livestockAnimalFSM2 = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM2))
			{
				livestockAnimalFSM2.MatePartner = default;
				livestockAnimalFSM2.NextMateSearch = TimeUntil.op_Implicit(retryDelay);
			}
			base.OnStateExit();
		}

		private static bool StillClaimedBy(LivestockAnimal self, LivestockAnimal target)
		{
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)target).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				return (Object)(object)livestockAnimalFSM.MateSuitor.Get(serverside: true) == (Object)(object)self;
			}
			return false;
		}
	}

	[Serializable]
	public class State_Pregnant : FSMStateBase
	{
		[Tooltip("How long (in seconds) she stays sat after giving birth, so the animation has time to play out before she walks off.")]
		public float StandUpDuration = 5f;

		[SerializeField]
		[Tooltip("How long (in seconds) she stays lying down after calving, before she gets up. Standing the instant the calf lands looks like nothing happened to her.")]
		private float afterBirthLieDown = 8f;

		[Tooltip("Longest (in seconds) she waits on a calf that is still down, on top of the stand up. A calf killed or stuck mid sequence would otherwise hold her forever.")]
		[SerializeField]
		private float waitForCalfTimeout = 30f;

		private float standUpTime;

		private TimeUntil giveUpOnCalf;

		private TimeUntil stayDownUntil;

		private bool lyingIn;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			standUpTime = 0f;
			lyingIn = false;
			giveUpOnCalf = TimeUntil.op_Implicit(StandUpDuration + waitForCalfTimeout);
			if (Owner is LivestockAnimal livestockAnimal && livestockAnimal.IsPregnant())
			{
				livestockAnimal.SetLyingIn(lyingIn: true);
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			if (livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			if (livestockAnimal.IsCarrying)
			{
				return EFSMStateStatus.None;
			}
			if (livestockAnimal.IsPregnant())
			{
				if (!lyingIn)
				{
					lyingIn = true;
					stayDownUntil = TimeUntil.op_Implicit(afterBirthLieDown);
				}
				if (TimeUntil.op_Implicit(stayDownUntil) > 0f)
				{
					return EFSMStateStatus.None;
				}
				livestockAnimal.FinishPregnancy();
			}
			standUpTime += deltaTime;
			if (standUpTime <= StandUpDuration)
			{
				return EFSMStateStatus.None;
			}
			if (livestockAnimal.TryGetNewbornCalf(out var calf) && TimeUntil.op_Implicit(giveUpOnCalf) > 0f)
			{
				Senses.TrySetTarget(calf);
				return EFSMStateStatus.None;
			}
			return EFSMStateStatus.Success;
		}

		public override void OnStateExit()
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				if (livestockAnimal.HasCalved)
				{
					livestockAnimal.FinishPregnancy();
				}
				livestockAnimal.SetLyingIn(lyingIn: false);
			}
			if (Senses.FindTarget(out var target) && target is LivestockAnimal)
			{
				Senses.TrySetTarget(null);
			}
			base.OnStateExit();
		}
	}

	[Serializable]
	public class Trans_IsPregnant : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return livestockAnimal.IsCalving;
			}
			return false;
		}
	}

	[Serializable]
	public class State_Newborn : FSMStateBase
	{
		private enum Phase
		{
			Asleep,
			HeadUp,
			Standing
		}

		[SerializeField]
		[Tooltip("How long (in seconds) it lies there before lifting its head.")]
		private float sleepDuration = 6f;

		[Tooltip("How long (in seconds) it sits up with its head about before trying to stand.")]
		[SerializeField]
		private float restDuration = 8f;

		[Tooltip("How long (in seconds) to let the getting up play out before the state ends.")]
		[SerializeField]
		private float standUpDuration = 2.5f;

		private Phase phase;

		private TimeUntil nextPhase;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			phase = Phase.Asleep;
			nextPhase = TimeUntil.op_Implicit(sleepDuration);
			Agent.ResetPath();
			livestockAnimal.SetSleeping(sleeping: true);
			livestockAnimal.SetResting(resting: false);
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			if (TimeUntil.op_Implicit(nextPhase) > 0f)
			{
				return EFSMStateStatus.None;
			}
			switch (phase)
			{
			case Phase.Asleep:
				livestockAnimal.SetSleeping(sleeping: false);
				livestockAnimal.SetResting(resting: true);
				phase = Phase.HeadUp;
				nextPhase = TimeUntil.op_Implicit(restDuration);
				return EFSMStateStatus.None;
			case Phase.HeadUp:
				livestockAnimal.SetResting(resting: false);
				phase = Phase.Standing;
				nextPhase = TimeUntil.op_Implicit(standUpDuration);
				return EFSMStateStatus.None;
			default:
				return EFSMStateStatus.Success;
			}
		}

		public override void OnStateExit()
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetSleeping(sleeping: false);
				livestockAnimal.SetResting(resting: false);
				livestockAnimal.FinishFindingItsFeet();
			}
			base.OnStateExit();
		}
	}

	[Serializable]
	public class Trans_IsNewborn : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return livestockAnimal.IsFindingItsFeet;
			}
			return false;
		}
	}

	[Serializable]
	public class State_FriskyCanter : State_MoveToTarget
	{
		[SerializeField]
		[Tooltip("Whether this species has the happy canter on its animator. Without it the flag would be networked to a controller that does nothing with it, and the animal would just canter about for no visible reason.")]
		private bool hasFriskyCanter;

		[SerializeField]
		[Tooltip("How far short of whoever she is running to she pulls up. She is coming over to say hello, not to barge into them.")]
		private float standoff = 4f;

		[Tooltip("How far off she runs when there is nobody to run to, picked in this range.")]
		[SerializeField]
		private Vector2 wanderRange = new Vector2(12f, 22f);

		[Tooltip("Longest (in seconds) the canter lasts, however far the destination turned out to be. Stops an unreachable one keeping her skipping about all day.")]
		[SerializeField]
		private float maxDuration = 8f;

		private TimeUntil canterTimeout;

		private Vector3? wanderTo;

		public bool HasFriskyCanter => hasFriskyCanter;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			wanderTo = null;
			canterTimeout = TimeUntil.op_Implicit(maxDuration);
			EFSMStateStatus eFSMStateStatus = base.OnStateEnter(payload);
			if (eFSMStateStatus == EFSMStateStatus.Failure)
			{
				return eFSMStateStatus;
			}
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetFrisky(frisky: true);
			}
			return eFSMStateStatus;
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(canterTimeout) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			return base.OnStateUpdate(deltaTime);
		}

		public override void OnStateExit()
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetFrisky(frisky: false);
			}
			base.OnStateExit();
		}

		protected override bool GetMoveDestination(out NavVector3 destination)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			destination = default;
			BasePlayer basePlayer = ((Owner is LivestockAnimal livestockAnimal) ? livestockAnimal.NearbyTrustedPlayer : null);
			Vector3 targetPosition = (((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero);
			if ((Object)(object)basePlayer != (Object)null || Senses.FindTargetPosition(out targetPosition))
			{
				Vector3 val = Vector3Ex.NormalizeXZ(((Component)Owner).transform.position - targetPosition);
				Vector3 positionWS = ((val.sqrMagnitude < 0.001f) ? targetPosition : (targetPosition + val * standoff));
				if (Agent.SamplePosition(positionWS, out var hitWS, 3f) && Agent.CanReach(hitWS.position))
				{
					destination = Agent.WorldToNavSpace(hitWS.position);
					return true;
				}
			}
			if (!wanderTo.HasValue)
			{
				Vector3 val2 = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
				Vector3 positionWS2 = ((Component)Owner).transform.position + val2 * Random.Range(wanderRange.x, wanderRange.y);
				if (!Agent.SamplePosition(positionWS2, out var hitWS2, 5f))
				{
					return false;
				}
				wanderTo = hitWS2.position;
			}
			destination = Agent.WorldToNavSpace(wanderTo.Value);
			return true;
		}

		public State_FriskyCanter()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Trans_IsVeryHappy : FSMTransitionBase
	{
		[Range(0f, 1f)]
		[Tooltip("Chance of going for a canter, each time an idling animal is content enough for one.")]
		[SerializeField]
		private float chance = 0.15f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading())
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !livestockAnimalFSM.friskyCanter.HasFriskyCanter)
			{
				return false;
			}
			if (TOD_Sky.Instance.IsNight)
			{
				return false;
			}
			if (!livestockAnimal.IsVeryHappy)
			{
				return false;
			}
			if (livestockAnimal.TryGetRememberedAggressor(out var _))
			{
				return false;
			}
			return Random.value < chance;
		}
	}

	[Serializable]
	public class State_StayNearMother : FSMStateBase
	{
		[SerializeField]
		private RustNavMeshAgent.Speeds minSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Run;

		[Tooltip("Fastest gait an animal that is not answering for the herd will use to get back. A bull crossing a field at a run reads as urgency; a cow doing it reads as panic.")]
		[SerializeField]
		private RustNavMeshAgent.Speeds dependentMaxSpeed = RustNavMeshAgent.Speeds.Walk;

		[Tooltip("Distance at which the infant moves at maxSpeed; it eases off as it closes in")]
		[SerializeField]
		private float fullSpeedDistance = 25f;

		[Tooltip("How close the infant has to get before it settles down beside her again")]
		[SerializeField]
		private float reunitedDistance = 4f;

		[Tooltip("Give up if the walk back takes longer than this")]
		[SerializeField]
		private float returnTimeout = 30f;

		[Tooltip("How long (in seconds) after a failed walk back before trying again")]
		[SerializeField]
		private float retryDelay = 15f;

		private EntityRef<LivestockAnimal> companion;

		private TimeUntil returnDeadline;

		private bool walkFailed;

		protected virtual LivestockAnimal FindCompanion(LivestockAnimal self)
		{
			return self.MotherAnimal;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			companion = default;
			walkFailed = false;
			if (!(Owner is LivestockAnimal self))
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal = FindCompanion(self);
			if ((Object)(object)livestockAnimal == (Object)null || livestockAnimal.IsDead())
			{
				return EFSMStateStatus.Failure;
			}
			companion.Set(livestockAnimal);
			returnDeadline = TimeUntil.op_Implicit(returnTimeout);
			if (!MoveToCompanion(livestockAnimal))
			{
				return GiveUp();
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsLeading())
			{
				return EFSMStateStatus.Failure;
			}
			LivestockAnimal livestockAnimal2 = companion.Get(serverside: true);
			if ((Object)(object)livestockAnimal2 == (Object)null || livestockAnimal2.IsDead())
			{
				return EFSMStateStatus.Failure;
			}
			if (TimeUntil.op_Implicit(returnDeadline) <= 0f)
			{
				return GiveUp();
			}
			Vector3 position = ((Component)livestockAnimal2).transform.position;
			if (Vector3.Distance(((Component)Owner).transform.position, position) <= reunitedDistance)
			{
				return EFSMStateStatus.Success;
			}
			if (Vector3.Distance(Agent.destination.Value, position) > reunitedDistance && !MoveToCompanion(livestockAnimal2))
			{
				return GiveUp();
			}
			if (!Agent.hasPath)
			{
				return GiveUp();
			}
			return base.OnStateUpdate(deltaTime);
		}

		public override void OnStateExit()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			companion = default;
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (walkFailed && ((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				livestockAnimalFSM.NextCompanionSearch = TimeUntil.op_Implicit(retryDelay);
			}
			base.OnStateExit();
		}

		private EFSMStateStatus GiveUp()
		{
			walkFailed = true;
			return EFSMStateStatus.Failure;
		}

		private bool MoveToCompanion(LivestockAnimal target)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			RustNavMeshAgent.Speeds speeds = ((Owner is LivestockAnimal { IsDependent: not false }) ? dependentMaxSpeed : maxSpeed);
			return MoveWithRampedSpeed(Agent, ((Component)target).transform.position, fullSpeedDistance, minSpeed, speeds);
		}
	}

	[Serializable]
	public class State_ReturnToHerd : State_StayNearMother
	{
		protected override LivestockAnimal FindCompanion(LivestockAnimal self)
		{
			return self.FindHerdToReturnTo();
		}
	}

	[Serializable]
	public class Trans_StrayedFromHerd : FSMSlowTransitionBase
	{
		[Tooltip("How far a bull can get from the nearest cow or calf before he heads back.")]
		[SerializeField]
		public float strayDistance = 20f;

		protected override bool EvaluateAtInterval(ref FSMPayload payload)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsDead())
			{
				return false;
			}
			if (livestockAnimal.IsLeading() || livestockAnimal.IsInfant())
			{
				return false;
			}
			if (livestockAnimal.IsPregnant())
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				if (TimeUntil.op_Implicit(livestockAnimalFSM.NextCompanionSearch) > 0f)
				{
					return false;
				}
				if (livestockAnimalFSM.CurrentState == livestockAnimalFSM.rest)
				{
					return false;
				}
			}
			LivestockAnimal livestockAnimal2 = livestockAnimal.FindHerdToReturnTo();
			if ((Object)(object)livestockAnimal2 == (Object)null)
			{
				return false;
			}
			return Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)livestockAnimal2).transform.position) > strayDistance;
		}
	}

	[Serializable]
	public class Trans_StrayedFromMother : FSMTransitionBase
	{
		[Tooltip("How far (in metres) an infant can get from its mother before it heads back to her.")]
		[SerializeField]
		private float strayDistance = 15f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.IsInfant())
			{
				return false;
			}
			if (livestockAnimal.IsSleeping())
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && TimeUntil.op_Implicit(livestockAnimalFSM.NextCompanionSearch) > 0f)
			{
				return false;
			}
			LivestockAnimal motherAnimal = livestockAnimal.MotherAnimal;
			if ((Object)(object)motherAnimal == (Object)null || motherAnimal.IsDead())
			{
				return false;
			}
			return Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)motherAnimal).transform.position) > strayDistance;
		}
	}

	[Serializable]
	public class State_Sleep : FSMStateBase
	{
		private enum Phase
		{
			Asleep,
			WalkingHome,
			Gathering
		}

		public float WakeDuration = 10f;

		[SerializeField]
		[Tooltip("How long (in seconds) the animal spends walking in to the herd before giving up and lying down where it stands. Stops one animal on the wrong side of a wall keeping the whole night awake.")]
		private float gatherTimeout = 20f;

		[Tooltip("How long (in seconds) the walk home at dusk gets before the animal gives up and beds down where it stands. Longer than the herd gather, because home can be the whole roaming radius away while a companion is only metres off.")]
		[SerializeField]
		private float homeTimeout = 90f;

		private float wakeTime;

		[SerializeField]
		private RustNavMeshAgent.Speeds gatherMinSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		private RustNavMeshAgent.Speeds gatherMaxSpeed = RustNavMeshAgent.Speeds.Walk;

		[SerializeField]
		[Tooltip("How far out the walk in to the herd runs at full speed before easing down.")]
		private float gatherFullSpeedDistance = 10f;

		private Phase phase;

		private TimeUntil gatherDeadline;

		private EntityRef<LivestockAnimal> gatherCompanion;

		private Vector3 gatherAim;

		private TimeSince dawnSeen;

		private bool sawDawn;

		public bool IsGathering => phase == Phase.Gathering;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			wakeTime = 0f;
			sawDawn = false;
			gatherCompanion = default;
			Vector3 home;
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				LieDown();
			}
			else if (livestockAnimal.TryGetBedtimeHome(out home) && MoveToBedtimeAim(home))
			{
				phase = Phase.WalkingHome;
				gatherDeadline = TimeUntil.op_Implicit(homeTimeout);
			}
			else if (!StartGathering(livestockAnimal))
			{
				LieDown();
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			if (phase == Phase.WalkingHome)
			{
				if (livestockAnimal.TryGetBedtimeHome(out var home) && TimeUntil.op_Implicit(gatherDeadline) > 0f)
				{
					if (Vector3.Distance(gatherAim, home) > livestockAnimal.HerdGatherDistance)
					{
						MoveToBedtimeAim(home);
					}
					return base.OnStateUpdate(deltaTime);
				}
				if (!StartGathering(livestockAnimal))
				{
					LieDown();
				}
			}
			if (phase == Phase.Gathering)
			{
				LivestockAnimal livestockAnimal2 = gatherCompanion.Get(serverside: true);
				if (!((Object)(object)livestockAnimal2 == (Object)null) && !livestockAnimal2.IsDead() && !(Vector3.Distance(((Component)Owner).transform.position, ((Component)livestockAnimal2).transform.position) <= livestockAnimal.HerdGatherDistance) && TimeUntil.op_Implicit(gatherDeadline) > 0f)
				{
					if (Vector3.Distance(gatherAim, ((Component)livestockAnimal2).transform.position) > livestockAnimal.HerdGatherDistance)
					{
						MoveToCompanion(livestockAnimal2);
					}
					return base.OnStateUpdate(deltaTime);
				}
				LieDown();
			}
			if (TOD_Sky.Instance.IsNight)
			{
				sawDawn = false;
				return EFSMStateStatus.None;
			}
			if (!sawDawn)
			{
				sawDawn = true;
				dawnSeen = TimeSince.op_Implicit(0f);
			}
			if (TimeSince.op_Implicit(dawnSeen) < livestockAnimal.SleepOffset)
			{
				return EFSMStateStatus.None;
			}
			SetSleeping(sleeping: false);
			wakeTime += deltaTime;
			if (!(wakeTime > WakeDuration))
			{
				return EFSMStateStatus.None;
			}
			return EFSMStateStatus.Success;
		}

		private bool MoveToCompanion(LivestockAnimal companion)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return MoveToBedtimeAim(((Component)companion).transform.position);
		}

		private bool MoveToBedtimeAim(Vector3 aim)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			gatherAim = aim;
			return MoveWithRampedSpeed(Agent, gatherAim, gatherFullSpeedDistance, gatherMinSpeed, gatherMaxSpeed);
		}

		private bool StartGathering(LivestockAnimal animal)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			LivestockAnimal livestockAnimal = animal.FindSleepingCompanion();
			if ((Object)(object)livestockAnimal == (Object)null || !MoveToCompanion(livestockAnimal))
			{
				return false;
			}
			phase = Phase.Gathering;
			gatherCompanion.Set(livestockAnimal);
			gatherDeadline = TimeUntil.op_Implicit(gatherTimeout);
			return true;
		}

		private void LieDown()
		{
			phase = Phase.Asleep;
			gatherCompanion = default;
			Agent.ResetPath();
			SetSleeping(sleeping: true);
		}

		public override void OnStateExit()
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.IsDead())
			{
				SetSleeping(sleeping: false);
			}
			base.OnStateExit();
		}

		private void SetSleeping(bool sleeping)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetSleeping(sleeping);
			}
		}
	}

	[Serializable]
	public class Trans_WantsBedtimeRest : FSMTransitionBase
	{
		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Chance an animal dozes before dropping off rather than going straight to sleep.")]
		private float chance = 0.5f;

		private bool rolled;

		private bool dozing;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsSleeping())
			{
				return false;
			}
			if (!TOD_Sky.Instance.IsNight)
			{
				rolled = false;
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !livestockAnimalFSM.rest.HasRestingPose)
			{
				return false;
			}
			if (!rolled)
			{
				rolled = true;
				dozing = Random.value <= chance;
			}
			return dozing;
		}

		public override void OnTransitionTaken(FSMStateBase from, FSMStateBase to)
		{
			dozing = false;
		}
	}

	[Serializable]
	public abstract class Trans_AfterFailedFoodSearch : FSMTransitionBase
	{
		[Tooltip("How soon (in seconds) after a failed food search the animal still reacts to it, so the reaction is tied to the search that just failed rather than to a long tail.")]
		[SerializeField]
		protected float window;

		[SerializeField]
		[Tooltip("Shortest gap (in seconds) between two reactions. Without it the animal repeats this every time idle ends for as long as there is nothing to eat.")]
		protected float minimumInterval;

		private double? lastTakenTime;

		protected Trans_AfterFailedFoodSearch(float window, float minimumInterval)
		{
			this.window = window;
			this.minimumInterval = minimumInterval;
		}

		protected abstract bool StillWants(LivestockAnimal animal, LivestockAnimalFSM fsm);

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal animal))
			{
				return false;
			}
			if (lastTakenTime.HasValue && Time.timeAsDouble - lastTakenTime.Value < (double)minimumInterval)
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && TimeSince.op_Implicit(livestockAnimalFSM.LastFoodSearchFailed) < window)
			{
				return StillWants(animal, livestockAnimalFSM);
			}
			return false;
		}

		public override void OnTransitionTaken(FSMStateBase from, FSMStateBase to)
		{
			lastTakenTime = Time.timeAsDouble;
		}
	}

	[Serializable]
	public class Trans_FoodSearchFailed : Trans_AfterFailedFoodSearch
	{
		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Fullness has to be at or under this, not merely under the consume threshold. The animal is meant to look like it cannot find food, not merely peckish.")]
		private float starvingBelow = 0.2f;

		public Trans_FoodSearchFailed()
			: base(5f, 60f)
		{
		}

		protected override bool StillWants(LivestockAnimal animal, LivestockAnimalFSM fsm)
		{
			if (animal.Fullness.NeedValue <= starvingBelow)
			{
				return fsm.foodSearch.HasAnimation;
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_LooksElsewhereForFood : Trans_AfterFailedFoodSearch
	{
		public Trans_LooksElsewhereForFood()
			: base(20f, 30f)
		{
		}

		protected override bool StillWants(LivestockAnimal animal, LivestockAnimalFSM fsm)
		{
			return animal.NeedsFood();
		}
	}

	[Serializable]
	public class State_Rest : FSMStateBase
	{
		[Tooltip("Whether this species has the lying down pose set up on its animator. Without it the flag would be networked to a controller that does nothing with it.")]
		[SerializeField]
		private bool hasRestingPose;

		[SerializeField]
		[Tooltip("How long (in seconds) the animal stays down, picked in this range. Long enough that the standing gap between two rests still leaves the day's resting share reachable - short lie downs with a fixed gap between them cannot add up to it.")]
		private Vector2 durationRange = new Vector2(40f, 70f);

		[Tooltip("Scales the lie down after dark, where it is a doze on the way to sleep rather than a rest in its own right.")]
		[SerializeField]
		private float nightScale = 0.35f;

		[Tooltip("How long (in seconds) to let the getting up play out before the state ends.")]
		[SerializeField]
		private float standUpDuration = 2.5f;

		private TimeUntil stayDownUntil;

		private float standingUpTime;

		private bool gettingUp;

		private TimeSince stoodUp;

		private bool hasStoodUp;

		public bool HasRestingPose => hasRestingPose;

		public float SinceStoodUp
		{
			get
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				if (!hasStoodUp)
				{
					return float.MaxValue;
				}
				return TimeSince.op_Implicit(stoodUp);
			}
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			gettingUp = false;
			standingUpTime = 0f;
			float num = Random.Range(durationRange.x, durationRange.y);
			if (Owner is LivestockAnimal livestockAnimal)
			{
				num *= (TOD_Sky.Instance.IsNight ? nightScale : livestockAnimal.AgeRestDurationScale);
				livestockAnimal.SetResting(resting: true);
			}
			stayDownUntil = TimeUntil.op_Implicit(num);
			Agent.ResetPath();
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Failure;
			}
			if (!gettingUp)
			{
				if (TimeUntil.op_Implicit(stayDownUntil) > 0f && !livestockAnimal.IsStarving())
				{
					return EFSMStateStatus.None;
				}
				gettingUp = true;
				livestockAnimal.SetResting(resting: false);
			}
			standingUpTime += deltaTime;
			if (!(standingUpTime > standUpDuration))
			{
				return EFSMStateStatus.None;
			}
			return EFSMStateStatus.Success;
		}

		public override void OnStateExit()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is LivestockAnimal livestockAnimal && !livestockAnimal.IsDead())
			{
				livestockAnimal.SetResting(resting: false);
			}
			stoodUp = TimeSince.op_Implicit(0f);
			hasStoodUp = true;
			base.OnStateExit();
		}

		public State_Rest()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Trans_WantsRest : FSMTransitionBase
	{
		[Tooltip("How long (in seconds) after getting up before the animal will lie down again.")]
		[SerializeField]
		private float minimumTimeStanding = 30f;

		[Tooltip("How long (in seconds) a food search that found nothing lets a hungry animal lie down anyway. A hungry animal with food in reach tops up first, one with none rests rather than pacing all day.")]
		[SerializeField]
		private float foodSearchFailedMemory = 60f;

		[Tooltip("Chance that an animal on its way to bed, or just woken up, lies down for a while first instead. Stops a herd going from standing to asleep in one step.")]
		[Range(0f, 1f)]
		[SerializeField]
		private float aroundBedtimeChance = 0.5f;

		[Tooltip("How long (in seconds) that lie down around bedtime lasts, picked in this range.")]
		[SerializeField]
		private Vector2 aroundBedtimeDuration = new Vector2(10f, 20f);

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return false;
			}
			if (TOD_Sky.Instance.IsNight)
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) || !livestockAnimalFSM.rest.HasRestingPose)
			{
				return false;
			}
			if (livestockAnimalFSM.rest.SinceStoodUp < minimumTimeStanding)
			{
				return false;
			}
			if (livestockAnimal.NeedsFood() && TimeSince.op_Implicit(livestockAnimalFSM.LastFoodSearchFailed) >= foodSearchFailedMemory)
			{
				return false;
			}
			return true;
		}

		public bool RollBedtimeRest(out float duration)
		{
			duration = Random.Range(aroundBedtimeDuration.x, aroundBedtimeDuration.y);
			return Random.value <= aroundBedtimeChance;
		}

		public Trans_WantsRest()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Trans_WantsSleep : FSMTransitionBase
	{
		private TimeSince duskSeen;

		private bool sawDusk;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsSleeping())
			{
				return false;
			}
			if (!TOD_Sky.Instance.IsNight)
			{
				sawDusk = false;
				return false;
			}
			if (!sawDusk)
			{
				sawDusk = true;
				duskSeen = TimeSince.op_Implicit(0f);
			}
			return TimeSince.op_Implicit(duskSeen) >= livestockAnimal.SleepOffset;
		}
	}

	[Serializable]
	public class State_ShovingAttack : State_Attack
	{
		[Tooltip("How hard the hit shoves a player. x lifts them off the ground enough for it to read, y drives them away from the animal.")]
		[SerializeField]
		private Vector2 pushForce = new Vector2(4f, 16f);

		[SerializeField]
		[Tooltip("How far the blow reaches when it lands. Further than this and it whiffs, which is what makes the charge sidesteppable.")]
		private float hitRange = 2.5f;

		[Tooltip("How wide the blow is, in degrees either side of our forward at the moment it lands. 180 removes the check, for an attack that does not face what it hits.")]
		[SerializeField]
		private float hitAngle = 60f;

		protected override void DoDamage()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			if (!Senses.FindTarget(out var target) || !(target is BaseCombatEntity baseCombatEntity))
			{
				return;
			}
			Vector3 val = ((Component)target).transform.position - ((Component)Owner).transform.position;
			if (!(Vector3Ex.MagnitudeXZ(val) > hitRange) && (!(hitAngle < 180f) || !(Vector3.Angle(((Component)Owner).transform.forward, Vector3Ex.NormalizeXZ(val)) > hitAngle)))
			{
				baseCombatEntity.OnAttacked(Damage, DamageType, Owner, ignoreShield: false);
				if (baseCombatEntity.ToNonNpcPlayer(out var player))
				{
					player.DoPush(Vector3Ex.NormalizeXZ(val) * pushForce.y + Vector3.up * pushForce.x);
				}
			}
		}

		public State_ShovingAttack()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_HoldGround : FSMStateBase
	{
		[Tooltip("Gait once he is in position between the threat and whoever he is shielding.")]
		[SerializeField]
		public RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.Walk;

		[Tooltip("How close to the threat a cow or calf has to be for him to put himself in front of it. Zero makes him only stand his ground.")]
		[SerializeField]
		private float shieldSearchRadius = 20f;

		[Tooltip("How often the search for someone to shield runs, in seconds. This state ticks every frame and the search is a sphere query, so it is not run every one.")]
		[SerializeField]
		private float shieldSearchInterval = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("How far along the line from the threat to the animal he is shielding he tries to stand. 0.5 is squarely between the two of them.")]
		private float interposeFraction = 0.5f;

		[Tooltip("He will not close on the threat past this, so the halfway point gets pushed back out when it would put him too close. Keep it above the range the charge triggers at or he will charge instead of ever settling.")]
		[SerializeField]
		private float minStandoff = 6f;

		[Tooltip("Gait used while he is still getting into position. Once he is there he drops to the state's own speed.")]
		[SerializeField]
		private RustNavMeshAgent.Speeds repositionSpeed = RustNavMeshAgent.Speeds.Run;

		[Tooltip("How far off his shielding spot counts as out of position, and worth hurrying.")]
		[SerializeField]
		private float inPositionTolerance = 1f;

		[SerializeField]
		[Tooltip("How much room he leaves in front of the animal he is guarding, so he stands off her shoulder rather than in her.")]
		private float shieldClearance = 3f;

		[Tooltip("How far to either side of the shielding spot a defender may stand, so two of them answering the same threat line up shoulder to shoulder instead of in each other. Wants to be about a body width. Zero puts them all on the same point.")]
		[SerializeField]
		private float shieldSlotSpread = 1.5f;

		private EntityRef<LivestockAnimal> shielded;

		private TimeUntil nextShieldSearch;

		private float shieldSlotOffset;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)payload.entity != (Object)null)
			{
				Senses.TrySetTarget(payload.entity);
			}
			shielded = default;
			nextShieldSearch = TimeUntil.op_Implicit(0f);
			shieldSlotOffset = 0f;
			return base.OnStateEnter(payload);
		}

		public override void OnStateExit()
		{
			Agent.ResetPath();
			base.OnStateExit();
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if (!Senses.FindTargetPosition(out var targetPosition) || !TryGetShieldPosition(targetPosition, out var destination) || Agent.Raycast(destination, out var _))
			{
				return StandHisGround();
			}
			RustNavMeshAgent.Speeds value = ((Vector3.Distance(((Component)Owner).transform.position, destination) > inPositionTolerance) ? repositionSpeed : speed);
			if (!Agent.SetDestinationWithParams(Agent.WorldToNavSpace(destination), autoBraking: false, value))
			{
				return StandHisGround();
			}
			return EFSMStateStatus.None;
		}

		private EFSMStateStatus StandHisGround()
		{
			if (Agent.hasPath)
			{
				Agent.ResetPath();
			}
			return EFSMStateStatus.None;
		}

		private bool TryGetShieldPosition(Vector3 threatPosition, out Vector3 destination)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			destination = default;
			LivestockAnimal livestockAnimal = FindAnimalToShield(threatPosition);
			if ((Object)(object)livestockAnimal == (Object)null)
			{
				return false;
			}
			Vector3 val = ((Component)livestockAnimal).transform.position - threatPosition;
			float num = Vector3Ex.MagnitudeXZ(val);
			Vector3 val2 = Vector3Ex.NormalizeXZ(val);
			if (val2.sqrMagnitude < 0.001f)
			{
				return false;
			}
			float num2 = Mathf.Max(num * interposeFraction, minStandoff);
			num2 = Mathf.Min(num2, Mathf.Max(0f, num - shieldClearance));
			num2 = Mathf.Min(num2, Vector3Ex.MagnitudeXZ(((Component)Owner).transform.position - threatPosition));
			Vector3 val3 = new Vector3(0f - val2.z, 0f, val2.x) * shieldSlotOffset;
			destination = threatPosition + val2 * num2 + val3;
			return true;
		}

		private float SlotToOffset(int slot)
		{
			if (slot <= 0)
			{
				return 0f;
			}
			int num = (slot + 1) / 2;
			return (float)((slot % 2 == 1) ? num : (-num)) * shieldSlotSpread;
		}

		private LivestockAnimal FindAnimalToShield(Vector3 threatPosition)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (shieldSearchRadius <= 0f)
			{
				return null;
			}
			if (TimeUntil.op_Implicit(nextShieldSearch) > 0f)
			{
				LivestockAnimal livestockAnimal = shielded.Get(serverside: true);
				if ((Object)(object)livestockAnimal != (Object)null && !livestockAnimal.IsDead())
				{
					return livestockAnimal;
				}
			}
			nextShieldSearch = TimeUntil.op_Implicit(shieldSearchInterval);
			if (!(Owner is LivestockAnimal livestockAnimal2))
			{
				return null;
			}
			LivestockAnimal livestockAnimal3 = livestockAnimal2.FindNearestDependent(threatPosition, shieldSearchRadius);
			shieldSlotOffset = (((Object)(object)livestockAnimal3 == (Object)null) ? 0f : SlotToOffset(livestockAnimal3.ClaimShieldSlot(livestockAnimal2)));
			shielded.Set(livestockAnimal3);
			livestockAnimal2.RememberShielded(livestockAnimal3);
			return livestockAnimal3;
		}
	}

	[Serializable]
	public class State_Stomp : State_PlayAnimationBase
	{
		[SerializeField]
		[Tooltip("Preferred. A stomp with extracted root motion, so the step in it carries him a little way towards whoever earned it.")]
		public RootMotionData RootMotionAnimation;

		[SerializeField]
		[Tooltip("Fallback for a species with no extracted stomp. Plays on the spot.")]
		public AnimationClip Animation;

		public bool HasAnimation
		{
			get
			{
				if (!(RootMotionAnimation != null))
				{
					return (Object)(object)Animation != (Object)null;
				}
				return true;
			}
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			FaceTarget = false;
			EFSMStateStatus result = base.OnStateEnter(payload);
			animState = ((RootMotionAnimation != null) ? AnimPlayer.PlayServerAndTakeFromPool(RootMotionAnimation) : AnimPlayer.PlayServerAndTakeFromPool(Animation));
			return result;
		}
	}

	[Serializable]
	public class State_SquareUpApproach : State_MoveToTarget
	{
		[Tooltip("Hand over once the target is within this many degrees of our forward.")]
		[SerializeField]
		private float faceAngle = 25f;

		[SerializeField]
		[Tooltip("Longest he spends walking in. Someone circling him would never let him start otherwise, and the walk is only ever meant to be the turn.")]
		private float maxDuration = 2f;

		private TimeUntil walkTimeout;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (IsTargetAhead())
			{
				return EFSMStateStatus.Success;
			}
			walkTimeout = TimeUntil.op_Implicit(maxDuration);
			EFSMStateStatus eFSMStateStatus = base.OnStateEnter(payload);
			if (eFSMStateStatus != EFSMStateStatus.Failure)
			{
				return eFSMStateStatus;
			}
			return EFSMStateStatus.Success;
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (IsTargetAhead() || TimeUntil.op_Implicit(walkTimeout) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			EFSMStateStatus eFSMStateStatus = base.OnStateUpdate(deltaTime);
			if (eFSMStateStatus != EFSMStateStatus.Failure)
			{
				return eFSMStateStatus;
			}
			return EFSMStateStatus.Success;
		}

		private bool IsTargetAhead()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			if (!Senses.FindTargetPosition(out var targetPosition))
			{
				return true;
			}
			Vector3 val = Vector3Ex.NormalizeXZ(targetPosition - ((Component)Owner).transform.position);
			if (val.sqrMagnitude < 0.001f)
			{
				return true;
			}
			return Vector3.Angle(((Component)Owner).transform.forward, val) <= faceAngle;
		}
	}

	[Serializable]
	public class State_HerdFlee : State_Flee
	{
		[Tooltip("Keep running while whatever we are fleeing is still this close, rather than giving up on it. 0 leaves the base behaviour alone.")]
		[SerializeField]
		private float keepRunningWithin = 12f;

		[Tooltip("Longest (in seconds) that persistence lasts, so an animal with nowhere left to run stops rather than scrabbling at a corner forever.")]
		[SerializeField]
		private float maxPersistence = 20f;

		[Tooltip("How far a startled animal moves off, instead of the full flee distance. Somebody grabbing at an animal that will not have them is a shove, not a gunshot.")]
		[SerializeField]
		private float startleDistance = 6f;

		private bool ran;

		private TimeSince fleeingFor;

		private float authoredDistance;

		private float authoredDesiredDistance;

		private bool shortened;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			fleeingFor = TimeSince.op_Implicit(0f);
			shortened = Owner is LivestockAnimal livestockAnimal && livestockAnimal.TakeStartle();
			if (shortened)
			{
				authoredDistance = distance;
				authoredDesiredDistance = desiredDistance;
				distance = startleDistance;
				desiredDistance = startleDistance;
			}
			EFSMStateStatus eFSMStateStatus = base.OnStateEnter(payload);
			ran = eFSMStateStatus != EFSMStateStatus.Success;
			return eFSMStateStatus;
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			EFSMStateStatus eFSMStateStatus = base.OnStateUpdate(deltaTime);
			if (eFSMStateStatus != EFSMStateStatus.Success)
			{
				return eFSMStateStatus;
			}
			if (keepRunningWithin > 0f && TimeSince.op_Implicit(fleeingFor) < maxPersistence && Senses.FindTargetPosition(out var targetPosition) && Vector3.Distance(((Component)Owner).transform.position, targetPosition) <= keepRunningWithin)
			{
				return MoveAwayFromTarget();
			}
			return eFSMStateStatus;
		}

		public override void OnStateExit()
		{
			if (ran && Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.ClearSpook();
			}
			if (shortened)
			{
				distance = authoredDistance;
				desiredDistance = authoredDesiredDistance;
				shortened = false;
			}
			base.OnStateExit();
		}

		protected override NavVector3 GetFleeDirection(NavVector3 posNS, Vector3 targetPosition)
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (Owner is LivestockAnimal livestockAnimal)
			{
				if (livestockAnimal.TryGetHerdFleeHeading(out var heading))
				{
					livestockAnimal.PublishHerdFleeHeading(heading);
					return ToNavSpace(heading);
				}
				if (livestockAnimal.TryGetHerdCentroid(out var centroid))
				{
					Vector3 heading2 = Vector3Ex.WithY(centroid - targetPosition, 0f);
					if (heading2.sqrMagnitude > 1E-06f)
					{
						livestockAnimal.PublishHerdFleeHeading(heading2);
						return ToNavSpace(heading2);
					}
				}
				livestockAnimal.PublishHerdFleeHeading(((Component)Owner).transform.position - targetPosition);
			}
			return base.GetFleeDirection(posNS, targetPosition);
		}

		private NavVector3 ToNavSpace(Vector3 heading)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position = ((Component)Owner).transform.position;
			return (Agent.WorldToNavSpace(position + heading) - Agent.WorldToNavSpace(position)).NormalizeXZ();
		}
	}

	[Serializable]
	public class Trans_HerdMateHurt : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is LivestockAnimal livestockAnimal)
			{
				return livestockAnimal.HerdMateRecentlyHurt;
			}
			return false;
		}

		public override void OnTransitionTaken(FSMStateBase from, FSMStateBase to)
		{
			base.OnTransitionTaken(from, to);
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.ClearHerdDistress();
			}
		}
	}

	[Serializable]
	public class Trans_WouldRunFromAHit : FSMSlowTransitionBase
	{
		[Tooltip("A bull runs once his health drops below this fraction of his maximum. Cows and calves run from any hit at all.")]
		[SerializeField]
		public float bullStandsGroundAbove = 0.8f;

		protected override bool EvaluateAtInterval(ref FSMPayload payload)
		{
			return Evaluate(Owner);
		}

		public bool Evaluate(BaseEntity owner)
		{
			if (!(owner is LivestockAnimal livestockAnimal))
			{
				return true;
			}
			if (livestockAnimal.InSafeZone())
			{
				return false;
			}
			if (livestockAnimal.IsLeading())
			{
				return true;
			}
			if (livestockAnimal.IsInfant())
			{
				return !livestockAnimal.HasProtectiveMotherNearby();
			}
			if (livestockAnimal.IsMale ? livestockAnimal.HasDependentsNearby() : livestockAnimal.IsProtectiveOfCalf)
			{
				return false;
			}
			if (livestockAnimal.IsMale && !(livestockAnimal.healthFraction < bullStandsGroundAbove))
			{
				return IsTargetOutOfReach(livestockAnimal);
			}
			return true;
		}

		private static bool IsTargetOutOfReach(LivestockAnimal animal)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			SenseComponent senseComponent = default;
			RustNavMeshAgent rustNavMeshAgent = default;
			if (((Component)animal).TryGetComponent<SenseComponent>(ref senseComponent) && senseComponent.FindTargetPosition(out var targetPosition) && ((Component)animal).TryGetComponent<RustNavMeshAgent>(ref rustNavMeshAgent))
			{
				return !rustNavMeshAgent.CanReach(targetPosition);
			}
			return false;
		}
	}

	[Serializable]
	public class Trans_TargetNearHerd : FSMSlowTransitionBase
	{
		[Tooltip("How close the target has to get to a cow or calf to count as threatening it.")]
		[SerializeField]
		public float threatDistance = 10f;

		protected override bool EvaluateAtInterval(ref FSMPayload payload)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || threatDistance <= 0f)
			{
				return false;
			}
			if (!Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			if ((Object)(object)livestockAnimal.FindNearestDependent(targetPosition, threatDistance) != (Object)null)
			{
				return true;
			}
			if (livestockAnimal.IsMale && livestockAnimal.IsAdult())
			{
				return AnyAdoptedPlayerNear(livestockAnimal, targetPosition, threatDistance);
			}
			return false;
		}

		private static bool AnyAdoptedPlayerNear(LivestockAnimal self, Vector3 targetPosition, float threatDistance)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			float maxTrust = Livestock.maxTrust;
			if (maxTrust <= 0f)
			{
				return false;
			}
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				BaseEntity.Query.Server.GetPlayersInSphere(targetPosition, threatDistance, (List<BasePlayer>)(object)val);
				foreach (BasePlayer item in (List<BasePlayer>)(object)val)
				{
					if (!((Object)(object)item == (Object)null) && !item.IsDead() && !item.IsNpc && self.TrustOf(item) >= maxTrust)
					{
						return true;
					}
				}
				return false;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	[Serializable]
	public class Trans_LeadOutranksTarget : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.IsLeading())
			{
				return false;
			}
			BasePlayer basePlayer = livestockAnimal.LeadingPlayer.Get(serverside: true);
			if ((Object)(object)basePlayer == (Object)null || livestockAnimal.TrustOf(basePlayer) < Livestock.trustToLead)
			{
				return false;
			}
			if (Senses.FindTarget(out var target))
			{
				return !((Object)(object)target == (Object)(object)basePlayer);
			}
			return true;
		}
	}

	[Serializable]
	public class Trans_HasThreatTarget : FSMTransitionBase
	{
		[Tooltip("Asks for a threat sat in a car or anything else that protects them from animals, the one to run from, instead of refusing them.")]
		[SerializeField]
		public bool targetProtectedByMount;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			if (!(Owner is LivestockAnimal livestockAnimal) || livestockAnimal.IsDead())
			{
				return false;
			}
			if (livestockAnimal.IsInfant() || livestockAnimal.IsLeading())
			{
				return false;
			}
			if (!Senses.FindTarget(out var target))
			{
				return false;
			}
			if (Trans_IsTargetProtectedByMount.IsProtected(target) != targetProtectedByMount)
			{
				return false;
			}
			if (target.InSafeZone() || livestockAnimal.InSafeZone())
			{
				return false;
			}
			if (target.ToNonNpcPlayer(out var _) && Senses.GetVisibilityStatus(target, out var status) && status.isInWaterCached)
			{
				return false;
			}
			if (WaterLevel.GetWaterDepth(((Component)livestockAnimal).transform.position, waves: false, volumes: false) >= 1f)
			{
				return false;
			}
			if (target is BasePlayer player2)
			{
				if (livestockAnimal.TrustOf(player2) >= Livestock.trustToTolerate)
				{
					return false;
				}
			}
			else if (!livestockAnimal.IsSoreAt(target))
			{
				return false;
			}
			return IsRightSort(livestockAnimal);
		}

		protected virtual bool IsRightSort(LivestockAnimal animal)
		{
			return true;
		}
	}

	[Serializable]
	public class Trans_IsProtective : Trans_HasThreatTarget
	{
		protected override bool IsRightSort(LivestockAnimal animal)
		{
			if (!animal.IsMale)
			{
				return animal.IsProtectiveOfCalf;
			}
			return true;
		}
	}

	[Serializable]
	public class Trans_IsLyingDown : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.IsLyingDown())
			{
				return false;
			}
			LivestockAnimalFSM livestockAnimalFSM = default;
			if (((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
			{
				livestockAnimalFSM.StartledOutOfSleep = livestockAnimal.IsSleeping();
			}
			return true;
		}
	}

	[Serializable]
	public class State_GetUp : State_PlayAnimation
	{
		[Tooltip("Scrambles straight from the sleeping pose to standing, replacing the animator's slower wake and stand. Must be authored from the sleeping pose.")]
		[SerializeField]
		private AnimationClip scrambleFromSleep;

		[SerializeField]
		[Tooltip("The same for a daytime lie down. Must be authored from the lying pose, not the sleeping one - the two are a long way apart.")]
		private AnimationClip scrambleFromRest;

		public bool HasSleepScramble => (Object)(object)scrambleFromSleep != (Object)null;

		public bool HasRestScramble => (Object)(object)scrambleFromRest != (Object)null;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			Agent.ResetPath();
			LivestockAnimalFSM livestockAnimalFSM = default;
			bool flag = ((Component)Owner).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM) && livestockAnimalFSM.StartledOutOfSleep;
			Animation = (flag ? scrambleFromSleep : scrambleFromRest);
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetResting(resting: false);
				livestockAnimal.SetSleeping(sleeping: false);
				livestockAnimal.SetLyingIn(lyingIn: false);
			}
			if (!HasAnimation)
			{
				return EFSMStateStatus.None;
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			if (!(Owner is LivestockAnimal livestockAnimal))
			{
				return EFSMStateStatus.Success;
			}
			if (!HasAnimation)
			{
				if (!livestockAnimal.IsOffItsFeet())
				{
					return EFSMStateStatus.Success;
				}
				return EFSMStateStatus.None;
			}
			EFSMStateStatus eFSMStateStatus = base.OnStateUpdate(deltaTime);
			if (eFSMStateStatus != EFSMStateStatus.None)
			{
				livestockAnimal.MarkOnItsFeet();
			}
			return eFSMStateStatus;
		}
	}

	[Serializable]
	public class State_RaiseHead : FSMStateBase
	{
		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			Agent.ResetPath();
			if (Owner is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.SetGrazing(grazing: false);
				livestockAnimal.SetDrinking(drinking: false);
			}
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			if (!(Owner is LivestockAnimal livestockAnimal) || !livestockAnimal.IsHeadDown())
			{
				return EFSMStateStatus.Success;
			}
			return base.OnStateUpdate(deltaTime);
		}
	}

	[Serializable]
	public class State_LivestockHurt : State_HurtWithAdditive
	{
		[Tooltip("Knocked down from a standstill.")]
		[SerializeField]
		private RootMotionData StandKnockdown;

		[Tooltip("Knocked down mid stride, for an animal already running.")]
		[SerializeField]
		private RootMotionData CanterKnockdown;

		[Tooltip("The gait from which a knockdown reads as being taken off its feet mid stride rather than dropping where it stood.")]
		[SerializeField]
		private RustNavMeshAgent.Speeds canterFrom = RustNavMeshAgent.Speeds.Jog;

		public override bool HasStaggerAnimation
		{
			get
			{
				if (!(StandKnockdown != null))
				{
					return CanterKnockdown != null;
				}
				return true;
			}
		}

		protected override RootMotionData PickStrongHit(HitInfo hitInfo)
		{
			RootMotionData rootMotionData;
			if (!((Object)(object)Agent != (Object)null) || !(Agent.velocity.magnitude >= Agent.GetSpeedForGait(canterFrom)))
			{
				rootMotionData = StandKnockdown;
				if ((object)rootMotionData == null)
				{
					return CanterKnockdown;
				}
			}
			else
			{
				rootMotionData = CanterKnockdown ?? StandKnockdown;
			}
			return rootMotionData;
		}
	}

	[Serializable]
	public class State_TurnAndRun : State_PlayAnimationRM
	{
		[SerializeField]
		[Tooltip("Turns away to the animal's left, for a threat on its right.")]
		private RootMotionData TurnLeft;

		[SerializeField]
		[Tooltip("Turns away to the animal's right, for a threat on its left.")]
		private RootMotionData TurnRight;

		[Tooltip("The full about-turn, for a threat dead ahead.")]
		[SerializeField]
		private RootMotionData TurnAbout;

		[Tooltip("Only bridges from a standstill. Already moving faster than this and the animal can just run, a turn montage would snap it back to a stop.")]
		[SerializeField]
		private float maxSpeedToTurn = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("How far off dead ahead a hit can land and still read as frontal, as a dot product against the animal's forward. 0 is straight to the side.")]
		private float frontalDot = 0.35f;

		public bool HasTurnAnimation
		{
			get
			{
				if (!(TurnAbout != null) && !(TurnLeft != null))
				{
					return TurnRight != null;
				}
				return true;
			}
		}

		public bool ShouldBridgeToFlee(BaseEntity owner, RustNavMeshAgent agent, HitInfo hitInfo)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			if (!HasTurnAnimation || hitInfo == null || (Object)(object)owner == (Object)null || (Object)(object)agent == (Object)null)
			{
				return false;
			}
			if (agent.velocity.magnitude > maxSpeedToTurn)
			{
				return false;
			}
			return Vector3.Dot(hitInfo.attackNormal, ((Component)owner).transform.forward) < 0f - frontalDot;
		}

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			Animation = PickTurn(payload.hitInfo);
			return base.OnStateEnter(payload);
		}

		private RootMotionData PickTurn(HitInfo hitInfo)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (hitInfo == null)
			{
				return TurnAbout ?? TurnLeft ?? TurnRight;
			}
			float num = Vector3.Dot(hitInfo.attackNormal, ((Component)Owner).transform.right);
			if (num < 0f - frontalDot)
			{
				return TurnLeft ?? TurnAbout ?? TurnRight;
			}
			if (num > frontalDot)
			{
				return TurnRight ?? TurnAbout ?? TurnLeft;
			}
			return TurnAbout ?? TurnLeft ?? TurnRight;
		}
	}

	public const float WadingDepth = 1f;

	public const float StompDistance = 10.4f;

	public const float ChargeDistance = 4.8f;

	public State_PlayRandomAnimation randomIdle = new State_PlayRandomAnimation();

	public State_LivestockRoam roam = new State_LivestockRoam();

	public Trans_LooksElsewhereForFood foodRoamTransition = new Trans_LooksElsewhereForFood();

	public State_FriskyCanter friskyCanter = new State_FriskyCanter();

	public Trans_IsVeryHappy veryHappyTransition = new Trans_IsVeryHappy();

	public State_Follow follow = new State_Follow();

	public State_TurnToLeader turnToLeader = new State_TurnToLeader();

	public State_CuriousFollow curiousFollow = new State_CuriousFollow();

	public Trans_IsCuriousAboutPlayer curiousTransition = new Trans_IsCuriousAboutPlayer();

	public State_Patrol_Points patrol = new State_Patrol_Points();

	public State_Graze graze = new State_Graze();

	public Trans_IsGrazeLocationInRange grazeTransition = new Trans_IsGrazeLocationInRange();

	public State_Drink drink = new State_Drink();

	public Trans_IsDrinkSourceInRange drinkTransition = new Trans_IsDrinkSourceInRange();

	public State_HerdFlee flee = new State_HerdFlee();

	public State_Sleep sleep = new State_Sleep();

	public State_Rest rest = new State_Rest();

	public State_PlayAnimation foodSearch = new State_PlayAnimation();

	public Trans_FoodSearchFailed foodSearchFailedTransition = new Trans_FoodSearchFailed();

	public Trans_WantsBedtimeRest bedtimeRestTransition = new Trans_WantsBedtimeRest();

	public Trans_WantsRest restTransition = new Trans_WantsRest();

	public State_SeekMate seekMate = new State_SeekMate();

	public State_AwaitMate awaitMate = new State_AwaitMate();

	public State_Frolic frolic = new State_Frolic();

	public State_Mate mate = new State_Mate();

	public State_Pregnant pregnant = new State_Pregnant();

	public State_Newborn newborn = new State_Newborn();

	public Trans_IsNewborn newbornTransition = new Trans_IsNewborn();

	public State_StayNearMother stayNearMother = new State_StayNearMother();

	public Trans_CanBreed canBreedTransition = new Trans_CanBreed();

	public Trans_HasMateRequest mateRequestTransition = new Trans_HasMateRequest();

	public Trans_SuitorIsFrolicking suitorFrolickingTransition = new Trans_SuitorIsFrolicking();

	public Trans_IsMale maleTransition = new Trans_IsMale();

	public Trans_StrayedFromMother strayedFromMotherTransition = new Trans_StrayedFromMother();

	public State_LivestockHurt hurt = new State_LivestockHurt();

	public State_TurnAndRun turnAndRun = new State_TurnAndRun();

	public State_GetUp getUp = new State_GetUp();

	public State_RaiseHead raiseHead = new State_RaiseHead();

	public Trans_IsLyingDown lyingDownTransition = new Trans_IsLyingDown();

	public State_SquareUpApproach warnApproach = new State_SquareUpApproach();

	public State_Stomp warn = new State_Stomp();

	public State_MoveToTarget chargeRun = new State_MoveToTarget();

	public State_AttackWithTracking headbutt = new State_AttackWithTracking();

	public State_ShovingAttack kick = new State_ShovingAttack();

	public State_ShovingAttack kickBehind = new State_ShovingAttack();

	public State_TurnToTarget turnToFace = new State_TurnToTarget();

	public State_TurnToTarget turnCantering = new State_TurnToTarget();

	public State_HoldGround observe = new State_HoldGround();

	[FormerlySerializedAs("protectiveMaleTransition")]
	public Trans_IsProtective protectiveTransition = new Trans_IsProtective();

	public Trans_TargetNearHerd herdThreatenedTransition = new Trans_TargetNearHerd();

	public Trans_WouldRunFromAHit wouldRunFromAHitTransition = new Trans_WouldRunFromAHit();

	public Trans_HerdMateHurt herdMateHurtTransition = new Trans_HerdMateHurt();

	public State_ReturnToHerd returnToHerd = new State_ReturnToHerd();

	public Trans_StrayedFromHerd strayedFromHerdTransition = new Trans_StrayedFromHerd();

	public State_Dead dead = new State_Dead();

	[NonSerialized]
	public Vector3? ConsumeLocation;

	[NonSerialized]
	public EntityRef<BaseEntity> ConsumeSource;

	[NonSerialized]
	public EntityRef<LivestockAnimal> MateTarget;

	[NonSerialized]
	public EntityRef<LivestockAnimal> MateSuitor;

	[NonSerialized]
	public TimeUntil MateRequestExpiry;

	[NonSerialized]
	public EntityRef<LivestockAnimal> MatePartner;

	[NonSerialized]
	public TimeUntil NextMateSearch;

	[NonSerialized]
	public TimeUntil NextCompanionSearch;

	[NonSerialized]
	public TimeUntil NextCuriousFollow;

	[NonSerialized]
	public TimeSince LastFoodSearchFailed;

	[NonSerialized]
	public bool StartledOutOfSleep;

	private Trans_Triggerable_HitInfo DeathTrans;

	private Trans_Triggerable_HitInfo HurtTrans;

	private Trans_Triggerable_HitInfo TurnAndRunTrans;

	public LivestockAnimal.DayActivity CurrentDayActivity
	{
		get
		{
			FSMStateBase currentState = CurrentState;
			if (currentState == graze)
			{
				if (!(baseEntity is LivestockAnimal livestockAnimal) || !livestockAnimal.IsGrazing())
				{
					return LivestockAnimal.DayActivity.Idling;
				}
				return LivestockAnimal.DayActivity.Grazing;
			}
			if (currentState == rest)
			{
				return LivestockAnimal.DayActivity.Resting;
			}
			if (currentState == randomIdle || currentState == roam || currentState == foodSearch)
			{
				return LivestockAnimal.DayActivity.Idling;
			}
			return LivestockAnimal.DayActivity.Other;
		}
	}

	public bool WantsToBeAlone
	{
		get
		{
			if (CurrentState != seekMate && CurrentState != awaitMate && CurrentState != frolic)
			{
				return CurrentState == mate;
			}
			return true;
		}
	}

	public override void InitShared()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		if (baseEntity.isServer)
		{
			LastFoodSearchFailed = TimeSince.op_Implicit(float.MaxValue);
			State_Nothing state_Nothing = new State_Nothing
			{
				Name = "Root"
			};
			State_Nothing state_Nothing2 = new State_Nothing
			{
				Name = "Alive"
			};
			State_Nothing state_Nothing3 = new State_Nothing
			{
				Name = "WaitForNavMesh"
			};
			State_Nothing state_Nothing4 = new State_Nothing
			{
				Name = "Can flee"
			};
			State_Nothing state_Nothing5 = new State_Nothing
			{
				Name = "Can follow"
			};
			State_Nothing state_Nothing6 = new State_Nothing
			{
				Name = "Not hurt"
			};
			State_Nothing state_Nothing7 = new State_Nothing
			{
				Name = "Square up"
			};
			State_Nothing state_Nothing8 = new State_Nothing
			{
				Name = "Not engaged"
			};
			Trans_Cooldown trans_Cooldown = new Trans_Cooldown
			{
				cooldown = 3f,
				onlyWhenStarted = true
			};
			Trans_And trans_And = new Trans_TargetInRange
			{
				Range = 3f
			} & ~new Trans_TargetInFront
			{
				Angle = 150f
			};
			Trans_And trans_And2 = new Trans_TargetInRange
			{
				Range = 3f,
				MaxHeightDifference = 1.5f
			} & ~new Trans_TargetInFront
			{
				Angle = 100f
			};
			Trans_And trans_And3 = new Trans_CanReachTarget_Slow() & new Trans_HasStraightPathToTarget
			{
				ProjectTarget = true
			};
			Trans_And trans_And4 = trans_And & new Trans_HasKick() & trans_Cooldown & trans_And3;
			Trans_And transition = trans_And2 & new Trans_HasKick() & trans_Cooldown & trans_And3;
			Trans_And trans_And5 = trans_And & new Trans_HasKick
			{
				Inverted = true
			} & new Trans_HasCanterTurn() & trans_And3;
			Trans_And transition2 = trans_And2 & new Trans_HasKick
			{
				Inverted = true
			} & new Trans_HasCanterTurn() & trans_And3;
			Trans_And transition3 = new Trans_ChasedFromBehind
			{
				range = 4f
			} & new Trans_Cooldown
			{
				cooldown = 3f
			} & trans_And3;
			Trans_Or trans_Or = new Trans_RecentlyHurt() | new Trans_LoudNoiseNearby() | new Trans_AggressorNearby() | (protectiveTransition & herdThreatenedTransition);
			DeathTrans = new Trans_Triggerable_HitInfo();
			HurtTrans = new Trans_Triggerable_HitInfo();
			TurnAndRunTrans = new Trans_Triggerable_HitInfo();
			state_Nothing.AddChildren(state_Nothing2.AddTickTransition(dead, DeathTrans), dead);
			if (patrol.Enabled)
			{
				state_Nothing2.AddChildren(state_Nothing3.AddTickTransition(patrol, new Trans_IsNavmeshReady()), patrol);
			}
			else
			{
				state_Nothing2.AddChildren(state_Nothing6.AddTickTransition(getUp, lyingDownTransition & trans_Or).AddTickTransition(hurt, HurtTrans).AddTickTransition(turnAndRun, TurnAndRunTrans)
					.AddTickTransition(turnToLeader, new Trans_LeaderBehind())
					.AddChildren(state_Nothing3.AddTickTransition(newborn, newbornTransition).AddTickTransition(randomIdle, new Trans_IsNavmeshReady()), state_Nothing4.AddTickTransition(follow, new Trans_LeadOutranksTarget()).AddTickTransition(flee, (new Trans_RecentlyHurt() | new Trans_LoudNoiseNearby() | new Trans_AggressorNearby()) & wouldRunFromAHitTransition & new Trans_IsFollowingPlayer
					{
						Inverted = true
					} & new Trans_Cooldown
					{
						cooldown = 5f
					}).AddTickTransition(flee, new Trans_IsProtective
					{
						targetProtectedByMount = true
					} & (new Trans_TargetInRange
					{
						Range = 10.4f
					} | herdThreatenedTransition))
						.AddChildren(state_Nothing8.AddTickTransition(kick, protectiveTransition & trans_And4).AddTickTransition(turnCantering, protectiveTransition & trans_And5).AddTickTransition(kickBehind, transition3)
							.AddTickTransition(chargeRun, protectiveTransition & (new Trans_RecentlyHurt() | herdMateHurtTransition | new Trans_LoudNoiseNearby
							{
								range = 20f
							}) & new Trans_CanReachTarget_Slow())
							.AddTickTransition(chargeRun, protectiveTransition & new Trans_IsCourting
							{
								Inverted = true
							} & new Trans_TargetInRange
							{
								Range = 4.8f
							} & (new Trans_TargetInFront
							{
								Angle = 150f
							} | herdThreatenedTransition) & new Trans_CanReachTarget_Slow())
							.AddTickTransition(warnApproach, protectiveTransition & new Trans_IsCourting
							{
								Inverted = true
							} & ((new Trans_TargetInRange
							{
								Range = 10.4f
							} & new Trans_TargetInFront
							{
								Angle = 150f
							}) | herdThreatenedTransition) & new Trans_Cooldown
							{
								cooldown = 4f
							} & new Trans_CanReachTarget_Slow())
							.AddChildren(state_Nothing5.AddTickTransition(newborn, newbornTransition).AddTickTransition(follow, new Trans_IsFollowingPlayer()).AddTickTransition(stayNearMother, strayedFromMotherTransition)
								.AddTickTransition(pregnant, new Trans_IsPregnant())
								.AddTickTransition(seekMate, canBreedTransition)
								.AddTickTransition(awaitMate, mateRequestTransition)
								.AddTickTransition(curiousFollow, curiousTransition)
								.AddTickTransition(returnToHerd, strayedFromHerdTransition)
								.AddChildren(randomIdle.AddTickTransition(graze, grazeTransition).AddEndTransition(rest, bedtimeRestTransition).AddEndTransition(sleep, new Trans_WantsSleep())
									.AddEndTransition(drink, drinkTransition)
									.AddEndTransition(graze, grazeTransition)
									.AddEndTransition(friskyCanter, veryHappyTransition)
									.AddEndTransition(foodSearch, foodSearchFailedTransition)
									.AddEndTransition(roam, foodRoamTransition)
									.AddEndTransition(rest, restTransition)
									.AddEndTransition(randomIdle), graze.AddEndTransition(randomIdle), drink.AddEndTransition(randomIdle), roam.AddEndTransition(randomIdle), friskyCanter.AddEndTransition(randomIdle), rest.AddEndTransition(randomIdle), foodSearch.AddEndTransition(randomIdle), sleep.AddEndTransition(randomIdle)), follow.AddTickTransition(randomIdle, new Trans_IsFollowingPlayer
							{
								Inverted = true
							}), curiousFollow.AddTickTransition(randomIdle, new Trans_StillCuriousAboutPlayer
							{
								Inverted = true
							}).AddFailureTransition(randomIdle), stayNearMother.AddEndTransition(randomIdle), newborn.AddEndTransition(randomIdle), seekMate.AddFailureTransition(randomIdle).AddEndTransition(frolic), awaitMate.AddTickTransition(frolic, suitorFrolickingTransition).AddEndTransition(randomIdle), frolic.AddEndTransition(mate, maleTransition).AddEndTransition(randomIdle), mate.AddFailureTransition(randomIdle).AddEndTransition(randomIdle), pregnant.AddEndTransition(randomIdle)), returnToHerd.AddTickTransition(kick, protectiveTransition & trans_And4).AddTickTransition(turnCantering, protectiveTransition & trans_And5).AddTickTransition(chargeRun, protectiveTransition & (new Trans_RecentlyHurt() | herdMateHurtTransition) & new Trans_TargetInRange
						{
							Range = 15f
						} & new Trans_CanReachTarget_Slow())
							.AddTickTransition(chargeRun, protectiveTransition & herdThreatenedTransition & new Trans_TargetInRange
							{
								Range = 4.8f
							} & new Trans_CanReachTarget_Slow())
							.AddTickTransition(warnApproach, protectiveTransition & herdThreatenedTransition & new Trans_Cooldown
							{
								cooldown = 4f
							} & new Trans_CanReachTarget_Slow())
							.AddFailureTransition(randomIdle)
							.AddEndTransition(randomIdle), state_Nothing7.AddTickTransition(randomIdle, new Trans_HasTarget
						{
							Inverted = true
						}).AddTickTransition(randomIdle, new Trans_TargetIsInSafeZone() | new Trans_IsInSafeZone()).AddTickTransition(randomIdle, new Trans_IsInWater_Slow
						{
							minDepth = 1f
						} | new Trans_IsTargetInWater())
							.AddChildren(warnApproach.AddTickTransition(kick, trans_And4).AddTickTransition(turnCantering, trans_And5).AddTickTransition(chargeRun, new Trans_RecentlyHurt() | herdMateHurtTransition)
								.AddTickTransition(chargeRun, new Trans_TargetInRange
								{
									Range = 4.8f
								} & new Trans_CanReachTarget_Slow())
								.AddTickTransition(observe, ~new Trans_TargetInRange
								{
									Range = 10.4f
								} & new Trans_TargetNearHerd
								{
									Inverted = true,
									threatDistance = 12.5f
								})
								.AddEndTransition(warn), warn.AddTickTransition(kick, trans_And4).AddTickTransition(turnCantering, trans_And5).AddTickTransition(chargeRun, new Trans_RecentlyHurt() | herdMateHurtTransition)
								.AddTickTransition(chargeRun, new Trans_TargetInRange
								{
									Range = 4.8f
								} & new Trans_CanReachTarget_Slow())
								.AddEndTransition(chargeRun, herdThreatenedTransition & new Trans_TargetInRange
								{
									Range = 10.4f
								} & new Trans_CanReachTarget_Slow())
								.AddEndTransition(observe), chargeRun.AddTickTransition(headbutt, new Trans_TargetInRange
							{
								Range = 4.7f,
								TimeToPredict = 0.35f,
								MaxHeightDifference = 1.5f
							} & new Trans_TargetInFront
							{
								Angle = 100f
							} & new Trans_HasStraightPathToTarget
							{
								ProjectTarget = true
							}).AddTickTransition(kick, transition).AddTickTransition(turnCantering, transition2)
								.AddTickTransition(observe, new Trans_ElapsedTime
								{
									Duration = 6.0
								})
								.AddFailureTransition(observe)
								.AddEndTransition(observe), headbutt.AddEndTransition(turnCantering, ~new Trans_TargetInRange
							{
								Range = 8f
							}).AddEndTransition(turnToFace, ~new Trans_TargetInFront
							{
								Angle = 60f
							}).AddFailureTransition(observe)
								.AddEndTransition(observe), turnCantering.AddFailureTransition(observe).AddEndTransition(chargeRun, new Trans_TargetInRange
							{
								Range = 4.8f
							} & new Trans_CanReachTarget_Slow()).AddEndTransition(warnApproach, new Trans_TargetInRange
							{
								Range = 10.4f
							} & new Trans_CanReachTarget_Slow())
								.AddEndTransition(observe), turnToFace.AddFailureTransition(observe).AddEndTransition(chargeRun, new Trans_TargetInRange
							{
								Range = 4.8f
							} & new Trans_CanReachTarget_Slow()).AddEndTransition(warnApproach, new Trans_TargetInRange
							{
								Range = 10.4f
							} & new Trans_CanReachTarget_Slow())
								.AddEndTransition(observe), observe.AddTickTransition(kick, trans_And4).AddTickTransition(turnCantering, trans_And5).AddTickTransition(chargeRun, (new Trans_RecentlyHurt() | herdMateHurtTransition) & new Trans_CanReachTarget_Slow())
								.AddTickTransition(chargeRun, new Trans_TargetInRange
								{
									Range = 4.8f
								} & new Trans_CanReachTarget_Slow())
								.AddTickTransition(warnApproach, new Trans_TargetEntersRange
								{
									Range = 10.4f
								} & new Trans_CanReachTarget_Slow())
								.AddTickTransition(randomIdle, new Trans_ElapsedTime
								{
									Duration = 4.0
								} & new Trans_CanReachTarget_Slow
								{
									Inverted = true
								})
								.AddTickTransition(randomIdle, new Trans_ElapsedTime
								{
									Duration = 2.0
								} & ~new Trans_TargetInRange
								{
									Range = 12f
								} & new Trans_TargetNearHerd
								{
									Inverted = true,
									threatDistance = 12.5f
								})
								.AddFailureTransition(randomIdle))), kick.AddTickTransition(randomIdle, new Trans_HasTarget
					{
						Inverted = true
					}).AddFailureTransition(observe).AddEndTransition(turnToFace), kickBehind.AddEndTransition(flee), flee.AddTickTransition(kickBehind, transition3).AddTickTransition(chargeRun, protectiveTransition & (new Trans_RecentlyHurt() | herdMateHurtTransition) & new Trans_CanReachTarget_Slow()).AddEndTransition(chargeRun, new Trans_IsProtective() & new Trans_TargetInRange
					{
						Range = 8f
					} & new Trans_CanReachTarget_Slow())
						.AddEndTransition(randomIdle), raiseHead.AddEndTransition(flee, trans_Or & wouldRunFromAHitTransition & new Trans_IsFollowingPlayer
					{
						Inverted = true
					}).AddEndTransition(randomIdle)), hurt.AddEndTransition(flee, wouldRunFromAHitTransition).AddEndTransition(randomIdle), turnAndRun.AddEndTransition(flee), turnToLeader.AddTickTransition(randomIdle, new Trans_IsFollowingPlayer
				{
					Inverted = true
				}).AddEndTransition(follow), getUp.AddEndTransition(flee, trans_Or & wouldRunFromAHitTransition & new Trans_IsFollowingPlayer
				{
					Inverted = true
				}).AddEndTransition(randomIdle));
			}
			RegisterDebugMoveTo(state_Nothing2);
			RegisterMounting(state_Nothing2, randomIdle);
			SetState(state_Nothing3);
			SetFsmActive(newActive: true);
		}
	}

	protected override FSMStateBase RedirectStateChange(FSMStateBase newState)
	{
		if (CurrentState == null)
		{
			return newState;
		}
		if (!(baseEntity is LivestockAnimal livestockAnimal))
		{
			return newState;
		}
		if (!livestockAnimal.IsOffItsFeet())
		{
			return RedirectHeadDown(livestockAnimal, newState);
		}
		if (CurrentState == getUp)
		{
			return newState;
		}
		if (newState == getUp || newState == dead || newState == rest || newState == newborn || newState == pregnant)
		{
			return newState;
		}
		if (livestockAnimal.IsSettlingDown() && (newState == hurt || newState == turnAndRun))
		{
			return newState;
		}
		StartledOutOfSleep = livestockAnimal.IsSleeping();
		return getUp;
	}

	private FSMStateBase RedirectHeadDown(LivestockAnimal animal, FSMStateBase newState)
	{
		if (!animal.IsHeadDown() || CurrentState == raiseHead)
		{
			return newState;
		}
		if (newState == raiseHead || newState == dead || newState == hurt || newState == turnAndRun || newState == mountVehicle)
		{
			return newState;
		}
		return raiseHead;
	}

	protected override void OnTicked(float deltaTime)
	{
		using (LivestockProfiler.Sample(LivestockProfiler.Section.Fsm))
		{
			base.OnTicked(deltaTime);
			if (baseEntity is LivestockAnimal livestockAnimal)
			{
				livestockAnimal.TickDayBudget(CurrentDayActivity, deltaTime);
			}
		}
	}

	public override bool OnDied(HitInfo hitInfo)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		LivestockCensus.RecordDeath(hitInfo, ((Component)baseEntity).transform.position);
		DeathTrans.Trigger(hitInfo);
		return false;
	}

	public override void Hurt(HitInfo hitInfo)
	{
		if (CurrentState == dead)
		{
			return;
		}
		if ((Object)(object)hurt.WeakHitAdditive != (Object)null)
		{
			((Component)this).GetComponent<RootMotionPlayer>().PlayServerAdditive(hurt.WeakHitAdditive);
		}
		if (CurrentState == hurt || CurrentState == turnAndRun || CurrentState == getUp)
		{
			return;
		}
		if (baseEntity is LivestockAnimal livestockAnimal && livestockAnimal.IsOffItsFeet() && !livestockAnimal.IsSettlingDown())
		{
			ForceTickOnTheNextUpdate();
		}
		else if (!baseEntity.InSafeZone())
		{
			if (hurt.HasStaggerAnimation && hurt.ShouldStagger(baseEntity, hitInfo))
			{
				HurtTrans.Trigger(hitInfo);
				ForceTickOnTheNextUpdate();
			}
			else if (turnAndRun.ShouldBridgeToFlee(baseEntity, ((Component)this).GetComponent<RustNavMeshAgent>(), hitInfo) && wouldRunFromAHitTransition.Evaluate(baseEntity))
			{
				TurnAndRunTrans.Trigger(hitInfo);
				ForceTickOnTheNextUpdate();
			}
		}
	}

	private static bool MoveWithRampedSpeed(RustNavMeshAgent agent, Vector3 destinationWS, float fullSpeedDistance, RustNavMeshAgent.Speeds minSpeed, RustNavMeshAgent.Speeds maxSpeed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!agent.SetDestinationWithParams(destinationWS))
		{
			return false;
		}
		float ratio = Mathf.InverseLerp(0f, fullSpeedDistance, Vector3.Distance(((Component)agent).transform.position, destinationWS));
		agent.SetSpeedRatio(ratio, minSpeed, maxSpeed);
		return true;
	}
}
