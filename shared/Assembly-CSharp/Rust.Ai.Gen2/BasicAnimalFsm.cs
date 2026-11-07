using UnityEngine;
using UnityEngine.Events;

namespace Rust.Ai.Gen2;

public class BasicAnimalFsm : FSMComponent
{
	public static class StateNames
	{
		public const string WaitForNavmesh = "WaitForNavmesh";

		public const string Roam = "Roam";

		public const string Idle = "Idle";

		public const string Chase = "Chase";

		public const string Attack = "Attack";

		public const string Cooldown = "Cooldown";

		public const string Flee = "Flee";

		public const string Dead = "Dead";
	}

	[Header("Basic animal")]
	[Tooltip("How close the animal has to get to bite, measured from its attack point. Also the distance the chase stops at, so it doubles as how close the animal comes to what it fights.")]
	[SerializeField]
	private float attackRange = 2f;

	[Tooltip("Where the bite reaches from, relative to the animal. Rotates with the animal, so a bear reaches further forwards than sideways.")]
	[SerializeField]
	private Vector3 attackOffset = Vector3.zero;

	[Tooltip("Minimum seconds between two bites.")]
	[SerializeField]
	private float attackIntervalSeconds = 1.5f;

	[Tooltip("How long the animal commits to one chase before backing off.")]
	[SerializeField]
	private float chaseGiveUpSeconds = 20f;

	[Tooltip("How long the animal stays uninterested after backing off, before it roams again.")]
	[SerializeField]
	private float cooldownSeconds = 5f;

	[Tooltip("Health fraction below which the animal breaks off a fight.")]
	[SerializeField]
	private float fleeBelowHealthFraction = 0.3f;

	[Tooltip("An animal that is not already fighting runs from a hit taken from further away than this, in metres. Zero turns it off.")]
	[SerializeField]
	private float fleeWhenShotBeyond = 20f;

	public State_Roam roam = new State_Roam();

	public State_AnimalIdle idle = new State_AnimalIdle();

	public State_AnimalChase chase = new State_AnimalChase();

	public State_AnimalBite attack = new State_AnimalBite();

	public State_Flee flee = new State_Flee();

	private Trans_TriggeredWithin AttackedTrans;

	private Trans_TriggeredWithin ShotFromAfarTrans;

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected Obj, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Cooldown"
		};
		roam.Name = "Roam";
		idle.Name = "Idle";
		chase.Name = "Chase";
		attack.Name = "Attack";
		attack.Range = attackRange;
		attack.AttackOffset = attackOffset;
		flee.Name = "Flee";
		dead.Name = "Dead";
		waitForNavMesh.Name = "WaitForNavmesh";
		SetCombatMovement(chase, chase.speed, attackRange);
		SetCombatMovement(attack, chase.speed, attackRange);
		attack.offNavmeshSampleRadius = chase.offNavmeshSampleRadius;
		AttackedTrans = new Trans_TriggeredWithin();
		ShotFromAfarTrans = new Trans_TriggeredWithin();
		Trans_Triggerable EncounterEndTrans = new Trans_Triggerable();
		((Component)baseEntity).GetComponent<NPCEncounterTimer>().onShouldGiveUp.AddListener((UnityAction)(() =>
		{
			EncounterEndTrans.Trigger();
		}));
		FSMTransitionBase transition = new Trans_TargetInRange
		{
			Range = attackRange,
			Offset = attackOffset
		} & new Trans_Cooldown
		{
			cooldown = attackIntervalSeconds
		} & new Trans_HasStraightPathToTarget();
		State_Nothing state_Nothing2 = new State_Nothing
		{
			Name = "Passive"
		};
		State_Nothing state_Nothing3 = new State_Nothing
		{
			Name = "Combat"
		};
		onNavmesh.AddTickTransition(flee, Trans_TargetHasRepellent.Near()).AddChildren(state_Nothing2.AddTickTransition(flee, new Trans_And
		{
			new Trans_IsAfraidOfTarget(),
			new Trans_HasTarget()
		}).AddTickTransition(flee, new Trans_And
		{
			ShotFromAfarTrans,
			new Trans_HasTarget()
		}).AddTickTransition(chase, new Trans_And
		{
			new Trans_HasTarget(),
			new Trans_TargetHasRepellent
			{
				Inverted = true
			},
			new Trans_IsInWater_Slow
			{
				minDepth = 1f,
				Inverted = true
			},
			new Trans_IsTargetInWater
			{
				Inverted = true
			}
		})
			.AddChildren(roam.AddFailureTransition(dead, new Trans_Dead()).AddEndTransition(idle), idle.AddEndTransition(roam)), state_Nothing3.AddTickTransition(state_Nothing, new Trans_HasTarget
		{
			Inverted = true
		}).AddTickTransition(flee, new Trans_TargetHasRepellent()).AddTickTransition(flee, EncounterEndTrans)
			.AddTickTransition(flee, new Trans_IsHealthBelowPercentage
			{
				percentage = fleeBelowHealthFraction
			})
			.AddTickTransition(flee, new Trans_TargetIsInSafeZone())
			.AddTickTransition(flee, new Trans_IsInWater_Slow
			{
				minDepth = 1f
			} | new Trans_IsTargetInWater())
			.AddChildren(chase.AddTickTransition(attack, transition).AddTickTransition(state_Nothing, new Trans_ElapsedTime
			{
				Duration = chaseGiveUpSeconds
			}).AddTickTransition(flee, ~new Trans_CanReachTarget_Slow())
				.AddFailureTransition(flee), attack.AddEndTransition(chase)), state_Nothing.AddTickTransition(flee, new Trans_And
		{
			new Trans_IsAfraidOfTarget(),
			new Trans_HasTarget()
		}).AddTickTransition(chase, AttackedTrans & ~new Trans_TargetHasRepellent()).AddTickTransition(roam, new Trans_ElapsedTime
		{
			Duration = cooldownSeconds
		}), flee.AddFailureTransition(chase, new Trans_And
		{
			new Trans_HasTarget(),
			new Trans_TargetHasRepellent
			{
				Inverted = true
			},
			new Trans_CanReachTarget_Slow(),
			new Trans_IsInWater_Slow
			{
				minDepth = 1f,
				Inverted = true
			},
			new Trans_IsTargetInWater
			{
				Inverted = true
			}
		}).AddSuccessTransition(flee, Trans_TargetHasRepellent.StillNear()).AddEndTransition(roam));
		return new AliveLayout
		{
			entry = roam,
			idle = roam
		};
	}

	public override void Hurt(HitInfo hitInfo)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (Senses.CanTarget(hitInfo.Initiator))
		{
			AttackedTrans.Trigger();
			if (fleeWhenShotBeyond > 0f && Vector3Ex.Distance2D(((Component)baseEntity).transform.position, ((Component)hitInfo.Initiator).transform.position) > fleeWhenShotBeyond)
			{
				ShotFromAfarTrans.Trigger();
			}
			if (CurrentState != dead)
			{
				ForceTickOnTheNextUpdate();
			}
		}
	}

	private static void SetCombatMovement(State_AnimalChase state, RustNavMeshAgent.Speeds gait, float stoppingDistance)
	{
		state.speed = gait;
		state.stopAtDestination = false;
		state.stoppingDistanceOverride = stoppingDistance;
		state.succeedWhenDestinationIsReached = false;
	}

	public BasicAnimalFsm()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	}
}
