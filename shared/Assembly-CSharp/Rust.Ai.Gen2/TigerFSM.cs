using Network;
using UnityEngine;
using UnityEngine.Events;

namespace Rust.Ai.Gen2;

public class TigerFSM : FSMComponent
{
	public State_PlayRandomAnimation randomIdle = new State_PlayRandomAnimation();

	public State_Roam roam = new State_Roam();

	public State_CircleDynamic approach = new State_CircleDynamic();

	public State_FastSneak fastSneak = new State_FastSneak();

	public State_Roar roar = new State_Roar();

	public State_Growl growlFire = new State_Growl();

	public State_MoveToTarget charge = new State_MoveToTarget();

	public State_Observe observe = new State_Observe();

	public State_DeadlyAttack attack = new State_DeadlyAttack();

	public State_Circle comboCircle = new State_Circle();

	public State_HurtWithAdditive hurt = new State_HurtWithAdditive();

	public State_FleeToHide flee = new State_FleeToHide();

	public State_Flee permaFlee = new State_Flee();

	public State_ApproachFood approachFood = new State_ApproachFood();

	public State_EatFood eatFood = new State_EatFood();

	public State_PlayAnimLoop sleep = new State_PlayAnimLoop();

	public State_AttackUnreachableWarped attackUnreachable = new State_AttackUnreachableWarped();

	public State_DragCorpse dragCorpse = new State_DragCorpse();

	private Trans_Triggerable_HitInfo HurtTrans;

	private Trans_Triggerable_HitInfo StaggerTrans;

	[ServerVar(Help = "The range at which the tiger will charge instead of fleeing if aimed at")]
	public static float chargeRange = 20f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TigerFSM.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected Obj, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected Obj, but got Unknown
		new State_MoveToTarget
		{
			speed = RustNavMeshAgent.Speeds.Walk,
			decelerationOverride = 6f,
			Name = "Step forward"
		};
		State_MoveToLastReachablePointNearTarget state_MoveToLastReachablePointNearTarget = new State_MoveToLastReachablePointNearTarget
		{
			speed = RustNavMeshAgent.Speeds.Jog,
			succeedWhenDestinationIsReached = true,
			Name = "Go to last destination slow"
		};
		State_MoveToLastReachablePointNearTarget state_MoveToLastReachablePointNearTarget2 = new State_MoveToLastReachablePointNearTarget
		{
			speed = RustNavMeshAgent.Speeds.Sprint,
			succeedWhenDestinationIsReached = true,
			Name = "Go to last destination"
		};
		HurtTrans = new Trans_Triggerable_HitInfo();
		StaggerTrans = new Trans_Triggerable_HitInfo();
		Trans_Triggerable FireMeleeTrans = new Trans_Triggerable();
		((Component)baseEntity).GetComponent<SenseComponent>().onFireMelee.AddListener((UnityAction)(() =>
		{
			FireMeleeTrans.Trigger();
		}));
		Trans_Triggerable EncounterEndTrans = new Trans_Triggerable();
		((Component)baseEntity).GetComponent<NPCEncounterTimer>().onShouldGiveUp.AddListener((UnityAction)(() =>
		{
			EncounterEndTrans.Trigger();
		}));
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Food"
		};
		State_Nothing state_Nothing2 = new State_Nothing
		{
			Name = "Roaming"
		};
		State_Nothing state_Nothing3 = new State_Nothing
		{
			Name = "Has target"
		};
		State_Nothing state_Nothing4 = new State_Nothing
		{
			Name = "Ready to strike"
		};
		State_Nothing state_Nothing5 = new State_Nothing
		{
			Name = "Not hurt"
		};
		State_Nothing state_Nothing6 = new State_Nothing
		{
			Name = "Random post idle wait"
		};
		Trans_And transition = new Trans_And
		{
			~new Trans_BlackboardCounterGte
			{
				Key = "AlreadyGrowled",
				MinValue = 2
			},
			new Trans_Cooldown
			{
				cooldown = 6f
			},
			new Trans_TargetIsNearFire
			{
				onlySeeFireWhenClose = true
			}
		};
		Trans_Or trans_Or = HurtTrans | new Trans_IsWatchedByTarget
		{
			requireAiming = true,
			minTime = 3f
		};
		Trans_Or trans_Or2 = HurtTrans | new Trans_IsWatchedByTarget
		{
			requireAiming = true,
			minTime = 0.5f
		};
		onNavmesh.AddChildren(state_Nothing5.AddTickTransition(hurt, StaggerTrans).AddTickTransition(permaFlee, Trans_TargetHasRepellent.Near()) + (state_Nothing2.AddTickTransition(approach, new Trans_HasTarget() & ~new Trans_TargetHasRepellent()).AddTickTransition(approachFood, new Trans_SeesFood()) + roam.AddEndTransition(sleep, new Trans_RandomChance
		{
			Chance = 0.25f
		}).AddFailureTransition(dead, new Trans_Dead()).AddEndTransition(randomIdle) + sleep.AddEndTransition(roam) + randomIdle.AddEndTransition(state_Nothing6) + state_Nothing6.AddTickTransition(roam, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 0.0,
			MaxDuration = 3.0
		})) + (state_Nothing3.AddTickTransition(roam, new Trans_HasTarget
		{
			Inverted = true
		}).AddTickTransition(permaFlee, new Trans_TargetHasRepellent()).AddTickTransition(permaFlee, EncounterEndTrans)
			.AddTickTransition(permaFlee, new Trans_TargetIsInSafeZone())
			.AddTickTransition(permaFlee, new Trans_IsInWater_Slow() | new Trans_IsTargetInWater()) + (state_Nothing4.AddTickTransition(growlFire, transition).AddTickTransition(approachFood, new Trans_SeesFood() & ~new Trans_HasBlackboardBool
		{
			Key = "TriedToApproachUnreachableFood"
		}).AddTickTransition(charge, new Trans_TargetInRange
		{
			Range = 10f
		})
			.AddTickTransition(charge, new Trans_TargetInRange
			{
				Range = chargeRange
			} & trans_Or)
			.AddTickTransition(flee, ~new Trans_TargetInRange
			{
				Range = chargeRange
			} & trans_Or2) + approach.AddTickTransition(observe, new Trans_And
		{
			~new Trans_HasBlackboardBool
			{
				Key = "AlreadyGrowled"
			},
			new Trans_IsWatchedByTarget
			{
				requireAiming = false
			},
			~new Trans_TargetInRange
			{
				Range = 15f
			}
		}).AddTickTransition(fastSneak, ~new Trans_IsInTargetViewCone
		{
			angle = 70f
		}).AddFailureTransition(state_MoveToLastReachablePointNearTarget) + fastSneak.AddTickTransition(approach, new Trans_IsInTargetViewCone
		{
			angle = 60f
		}).AddFailureTransition(state_MoveToLastReachablePointNearTarget) + observe.AddTickTransition(approach, new Trans_IsWatchedByTarget
		{
			wantsWatched = false,
			requireAiming = false
		}).AddTickTransition(charge, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 8.0,
			MaxDuration = 12.0
		} & new Trans_TargetInRange
		{
			Range = chargeRange
		}).AddFailureTransition(state_MoveToLastReachablePointNearTarget)) + charge.AddTickTransition(attack, new Trans_TargetInRange
		{
			Range = attack.range
		} & new Trans_HasStraightPathToTarget()).AddTickTransition(state_MoveToLastReachablePointNearTarget2, ~new Trans_CanReachTarget_Slow()).AddTickTransition(approach, new Trans_ElapsedTime
		{
			Duration = 5.0
		})
			.AddFailureTransition(state_MoveToLastReachablePointNearTarget2) + attack.AddEndTransition(comboCircle, new Trans_IsTargetDown()).AddEndTransition(flee) + comboCircle.AddTickTransition(charge, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 0.75,
			MaxDuration = 1.5
		}).AddFailureTransition(state_MoveToLastReachablePointNearTarget2).AddEndTransition(charge) + growlFire.AddTickTransition(attack, new Trans_TargetInRange
		{
			Range = 2f
		}).AddEndTransition(approach) + state_MoveToLastReachablePointNearTarget.AddTickBranchingTrans(permaFlee, trans_Or2, state_MoveToLastReachablePointNearTarget2, new Trans_TargetInRange
		{
			Range = chargeRange
		}).AddTickTransition(permaFlee, FireMeleeTrans).AddTickTransition(approach, new Trans_CanReachTarget_Slow())
			.AddFailureTransition(permaFlee)
			.AddEndTransition(attackUnreachable) + state_MoveToLastReachablePointNearTarget2.AddTickTransition(permaFlee, FireMeleeTrans).AddTickTransition(charge, new Trans_CanReachTarget_Slow()).AddFailureTransition(permaFlee)
			.AddEndTransition(attackUnreachable) + flee.AddFailureTransition(dead, new Trans_Dead()).AddEndTransition(approach)) + (state_Nothing.AddTickTransition(growlFire, transition).AddTickTransition(flee, trans_Or2) + approachFood.AddTickTransition(roam, ~new Trans_SeesFood()).AddTickTransition(charge, new Trans_TargetInRange
		{
			Range = 10f
		}).AddFailureTransition(roam)
			.AddEndTransition(eatFood) + eatFood.AddTickTransition(roam, ~new Trans_SeesFood()).AddTickTransition(dragCorpse, new Trans_TargetInRange
		{
			Range = 20f
		}).AddFailureTransition(roam)
			.AddEndTransition(roam) + dragCorpse.AddTickTransition(attack, new Trans_TargetInRange
		{
			Range = 3f
		}).AddTickTransition(permaFlee, new Trans_ElapsedTime
		{
			Duration = 1.0
		} & new Trans_TargetInRange
		{
			Range = 12f
		}).AddTickTransition(eatFood, ~new Trans_TargetInRange
		{
			Range = 30f
		})
			.AddFailureTransition(permaFlee)
			.AddEndTransition(permaFlee)) + permaFlee.AddFailureTransition(dead, new Trans_Dead()).AddSuccessTransition(permaFlee, Trans_TargetHasRepellent.StillNear()).AddEndTransition(roam), hurt.AddEndTransition(permaFlee, new Trans_TargetHasRepellent()).AddEndTransition(flee));
		alive.AddChildren(attackUnreachable.AddFailureTransition(permaFlee).AddEndTransition(permaFlee, new Trans_HasBlackboardBool
		{
			Key = "HitDuringCharge"
		}).AddEndTransition(flee));
		return new AliveLayout
		{
			entry = roam,
			idle = roam
		};
	}

	public override void Hurt(HitInfo hitInfo)
	{
		if (!((Component)this).GetComponent<SenseComponent>().CanTarget(hitInfo.Initiator))
		{
			return;
		}
		((Component)this).GetComponent<RootMotionPlayer>().PlayServerAdditive(hurt.WeakHitAdditive);
		if (CurrentState == roar || CurrentState == charge || CurrentState == attack || CurrentState == attackUnreachable)
		{
			((Component)this).GetComponent<BlackboardComponent>().Add("HitDuringCharge");
		}
		HurtTrans.Trigger(hitInfo);
		if (hurt.ShouldStagger(hitInfo))
		{
			StaggerTrans.Trigger(hitInfo);
			if (CurrentState != hurt && CurrentState != dead)
			{
				ForceTickOnTheNextUpdate();
			}
		}
	}
}
