using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class CritterAnimalFSM : FSMComponent
{
	[Serializable]
	public class Trans_LoudNoiseNearby : FSMTransitionBase
	{
		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			if (Owner is CritterAnimal critterAnimal)
			{
				return critterAnimal.RecentlyHeardLoudNoise;
			}
			return base.EvaluateInternal(ref payload);
		}
	}

	[Serializable]
	public class Trans_NoPlayerConnectionsInRange : FSMTransitionBase
	{
		private bool noPlayersInRange;

		private TimeSince timeSinceLastCheck;

		private float checkTimeout = 5f;

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
			if (Owner is CritterAnimal critterAnimal)
			{
				critterAnimal.SetDeactivatedFlag(toggle);
				if (toggle)
				{
					Agent.Pause(this);
				}
				else
				{
					Agent.Unpause(this);
				}
			}
		}
	}

	[Serializable]
	public class State_Idle : FSMStateBase
	{
		[SerializeField]
		[Tooltip("How long the critter stays stopped before moving on, in seconds.")]
		private Vector2 idleDurationRange = new Vector2(3f, 8f);

		private TimeUntil idleEndTime;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			Agent.ResetPath();
			idleEndTime = TimeUntil.op_Implicit(Rng.Range(idleDurationRange.x, idleDurationRange.y));
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (TimeUntil.op_Implicit(idleEndTime) <= 0f)
			{
				return EFSMStateStatus.Success;
			}
			return EFSMStateStatus.None;
		}

		public State_Idle()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class Trans_IsInterestingLocationInRange : FSMTransitionBase
	{
		[SerializeField]
		protected float searchRadius = 10f;

		[SerializeField]
		protected Enum interestTopology = (Enum)97;

		[SerializeField]
		protected float timeoutAfterInterest = 60f;

		protected override bool EvaluateInternal(ref FSMPayload payload)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			CritterAnimalFSM critterAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<CritterAnimalFSM>(ref critterAnimalFSM))
			{
				return false;
			}
			if (TimeSince.op_Implicit(critterAnimalFSM.TimeSinceLastInterest) <= timeoutAfterInterest)
			{
				return false;
			}
			NavVector3 nextPosition = Agent.nextPosition;
			PooledList<NavVector3> val = Pool.Get<PooledList<NavVector3>>();
			try
			{
				Eqs.SamplePositionsInDonutShape(nextPosition, (List<NavVector3>)(object)val, Rng, searchRadius);
				foreach (NavVector3 item in (List<NavVector3>)(object)val)
				{
					if (Agent.SamplePosition(item, out var hitNS, 10f) && Agent.IsPositionAtTopologyRequirement(hitNS.position, interestTopology))
					{
						critterAnimalFSM.InterestedLocation = hitNS.position.Value;
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

		public Trans_IsInterestingLocationInRange()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	[Serializable]
	public class State_Interested : FSMStateBase
	{
		protected enum Phase
		{
			MovingToInterest,
			InvestigatingInterest
		}

		[SerializeField]
		protected Vector2 distanceRange = new Vector2(10f, 20f);

		[SerializeField]
		protected RustNavMeshAgent.Speeds minSpeed;

		[SerializeField]
		protected RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

		[SerializeField]
		protected Vector2 interestedDurationRange = new Vector2(5f, 15f);

		[SerializeField]
		protected float postAnimationDuration = 3f;

		protected Phase phase;

		protected TimeUntil interestEndTime;

		public override EFSMStateStatus OnStateEnter(FSMPayload payload)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			CritterAnimalFSM critterAnimalFSM = default;
			if (!((Component)Owner).TryGetComponent<CritterAnimalFSM>(ref critterAnimalFSM))
			{
				return EFSMStateStatus.Failure;
			}
			if (!critterAnimalFSM.InterestedLocation.HasValue)
			{
				return base.OnStateEnter(payload);
			}
			Vector3 value = critterAnimalFSM.InterestedLocation.Value;
			critterAnimalFSM.InterestedLocation = null;
			if (!Agent.SamplePosition(value, out var hitWS, 10f))
			{
				return EFSMStateStatus.Failure;
			}
			float num = Vector3.Distance(((Component)Owner).transform.position, hitWS.position);
			if (!Agent.SetDestinationWithParams(hitWS.position))
			{
				return EFSMStateStatus.Failure;
			}
			float ratio = Mathf.InverseLerp(0f, distanceRange.y, num);
			Agent.SetSpeedRatio(ratio, minSpeed, maxSpeed);
			phase = Phase.MovingToInterest;
			return base.OnStateEnter(payload);
		}

		public override EFSMStateStatus OnStateUpdate(float deltaTime)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			switch (phase)
			{
			case Phase.MovingToInterest:
				if (Agent.hasPath)
				{
					return base.OnStateUpdate(deltaTime);
				}
				phase = Phase.InvestigatingInterest;
				interestEndTime = TimeUntil.op_Implicit(Rng.Range(interestedDurationRange.x, interestedDurationRange.y));
				if (Owner is CritterAnimal critterAnimal2)
				{
					critterAnimal2.SetInterested(interested: true);
				}
				return base.OnStateUpdate(deltaTime);
			case Phase.InvestigatingInterest:
				if (TimeUntil.op_Implicit(interestEndTime) <= postAnimationDuration && Owner is CritterAnimal critterAnimal && critterAnimal.IsInterested())
				{
					critterAnimal.SetInterested(interested: false);
				}
				if (TimeUntil.op_Implicit(interestEndTime) <= 0f)
				{
					CritterAnimalFSM critterAnimalFSM = default;
					if (((Component)Owner).TryGetComponent<CritterAnimalFSM>(ref critterAnimalFSM))
					{
						critterAnimalFSM.TimeSinceLastInterest = TimeSince.op_Implicit(0f);
					}
					return EFSMStateStatus.Success;
				}
				return EFSMStateStatus.None;
			default:
				return base.OnStateUpdate(deltaTime);
			}
		}

		public override void OnStateExit()
		{
			Agent.ResetPath();
			if (Owner is CritterAnimal critterAnimal)
			{
				critterAnimal.SetInterested(interested: false);
			}
			base.OnStateExit();
		}

		public State_Interested()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public State_PlayRandomAnimation randomIdle = new State_PlayRandomAnimation();

	public State_Roam roam = new State_Roam();

	public LivestockAnimalFSM.State_Patrol_Points patrol = new LivestockAnimalFSM.State_Patrol_Points();

	public State_Flee flee = new State_Flee();

	public State_Flee startle = new State_Flee();

	public State_Deactivate deactivate = new State_Deactivate();

	public Trans_IsInterestingLocationInRange _interestedTransition = new Trans_IsInterestingLocationInRange();

	public State_Interested _interested = new State_Interested();

	[NonSerialized]
	public Nullable InterestedLocation;

	[NonSerialized]
	public TimeSince TimeSinceLastInterest;

	public static TickFSMWorkQueue critterWorkQueue = new TickFSMWorkQueue
	{
		Name = "FSM.Critters"
	};

	public virtual FSMStateBase idle => randomIdle;

	public virtual Trans_IsInterestingLocationInRange interestedTransition => _interestedTransition;

	public virtual State_Interested interested => _interested;

	public static float critterFrameBudgetMs => AI.critters_frametime;

	protected override TickFSMWorkQueue workQueue => critterWorkQueue;

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TimeSinceLastInterest = TimeSince.op_Implicit(0f);
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Can flee"
		};
		State_Nothing state_Nothing2 = new State_Nothing
		{
			Name = "Can follow"
		};
		if (patrol.Enabled)
		{
			onNavmesh.AddChildren(patrol);
			return new AliveLayout
			{
				entry = patrol,
				idle = patrol
			};
		}
		alive.AddChildren(deactivate.AddTickTransition(idle, ~new Trans_NoPlayerConnectionsInRange()));
		onNavmesh.AddChildren(state_Nothing.AddTickTransition(deactivate, new Trans_NoPlayerConnectionsInRange()).AddTickTransition(flee, new Trans_RecentlyHurt() | new Trans_LoudNoiseNearby()).AddTickTransition(startle, new Trans_HasTarget())
			.AddChildren(state_Nothing2.AddChildren(idle.AddEndTransition(interested, interestedTransition).AddEndTransition(roam), interested.AddEndTransition(idle), roam.AddEndTransition(idle))), flee.AddEndTransition(idle), startle.AddTickTransition(flee, new Trans_RecentlyHurt() | new Trans_LoudNoiseNearby()).AddEndTransition(idle));
		return new AliveLayout
		{
			entry = deactivate,
			idle = idle
		};
	}
}
