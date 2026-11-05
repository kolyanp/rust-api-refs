using UnityEngine;
using UnityEngine.Events;

namespace Rust.Ai.Gen2;

public class CrocodileFSM : FSMComponent
{
	public State_PlayRandomAnimation randomIdle = new State_PlayRandomAnimation();

	public State_Roam roam = new State_Roam();

	public State_CircleDynamic approach = new State_CircleDynamic();

	public State_CrocCharge charge = new State_CrocCharge();

	public State_CrocIntimidate intimidate = new State_CrocIntimidate();

	public State_CrocTurn intimidateTurn = new State_CrocTurn();

	public State_AttackWithTracking staticAttack = new State_AttackWithTracking();

	public State_AttackWithTracking bellyFlop = new State_AttackWithTracking();

	public State_AttackWithTracking intimidateSnap = new State_AttackWithTracking();

	public State_AttackWithTracking swimAttack = new State_AttackWithTracking();

	public State_GoBackToWater flee = new State_GoBackToWater();

	public State_HurtWithAdditive hurt = new State_HurtWithAdditive();

	public State_ApproachFood approachFood = new State_ApproachFood();

	public State_BringFoodBackToWater bringFoodBackToWater = new State_BringFoodBackToWater();

	public State_TryAmbushUnderwater diveRoam = new State_TryAmbushUnderwater();

	private Trans_Triggerable_HitInfo HurtTrans;

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		State_MoveToBreakFoundation state_MoveToBreakFoundation = new State_MoveToBreakFoundation
		{
			Name = "MoveToFoundation"
		};
		State_CrocBreakFoundation state_CrocBreakFoundation = new State_CrocBreakFoundation
		{
			Name = "BreakFoundation",
			Animation = bellyFlop.Animation,
			DamageType = bellyFlop.DamageType,
			damageDelay = bellyFlop.damageDelay,
			damage = bellyFlop.damage,
			radius = 3f,
			trackingDuration = bellyFlop.trackingDuration,
			trackingSpeed = 360f,
			forceScale = bellyFlop.forceScale,
			doesStrafeDodge = false
		};
		HurtTrans = new Trans_Triggerable_HitInfo();
		Trans_Triggerable EncounterEndTrans = new Trans_Triggerable();
		((Component)baseEntity).GetComponent<NPCEncounterTimer>().onShouldGiveUp.AddListener((UnityAction)(() =>
		{
			EncounterEndTrans.Trigger();
		}));
		Trans_IsSwimming trans_IsSwimming = new Trans_IsSwimming();
		FSMTransitionBase fSMTransitionBase = ~new Trans_IsSwimming();
		FSMTransitionBase dstState2Trans = ~(new Trans_TargetInRange
		{
			Range = 30f
		} | trans_IsSwimming | new Trans_IsTargetInWater());
		Trans_CrocHasStraightPathToTarget trans_CrocHasStraightPathToTarget = new Trans_CrocHasStraightPathToTarget();
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "Reachable"
		};
		new State_Nothing().Name = "Food";
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
			Name = "Random post idle wait"
		};
		_ = onNavmesh.AddTickTransition(flee, Trans_TargetHasRepellent.Near(new Trans_TargetHasRepellent() & ~new Trans_IsSwimming())) + (state_Nothing2.AddTickBranchingTrans(charge, new Trans_HasTarget() & ~new Trans_TargetHasRepellent(), flee, dstState2Trans).AddTickTransition(approachFood, new Trans_SeesFood()) + roam.AddFailureTransition(dead, new Trans_Dead()).AddEndTransition(randomIdle, fSMTransitionBase).AddEndTransition(state_Nothing4) + randomIdle.AddEndTransition(state_Nothing4) + state_Nothing4.AddTickTransition(roam, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 7.0,
			MaxDuration = 14.0
		})) + (state_Nothing3.AddTickTransition(roam, ~new Trans_HasTarget()).AddTickTransition(flee, new Trans_TargetHasRepellent()).AddTickTransition(flee, EncounterEndTrans)
			.AddTickTransition(flee, new Trans_TargetIsInSafeZone())
			.AddTickTransition(flee, new Trans_IsTargetProtectedByMount()) + (state_Nothing.AddTickTransition(flee, new Trans_Cooldown
		{
			cooldown = 30f
		} & new Trans_IsTargetTooFarFromWater()) + charge.AddTickTransition(flee, fSMTransitionBase & new Trans_ElapsedTime
		{
			Duration = 15.0
		} & ~new Trans_TargetInRange
		{
			Range = 15f
		}).AddTickTransition(bellyFlop, fSMTransitionBase & new Trans_Cooldown
		{
			cooldown = 6f
		} & new Trans_TargetInFront
		{
			Angle = 45f
		} & new Trans_TargetInRange
		{
			Range = 5f,
			TimeToPredict = 0.46f
		} & trans_CrocHasStraightPathToTarget).AddTickTransition(intimidateTurn, fSMTransitionBase & new Trans_Cooldown
		{
			cooldown = 5f
		} & ~new Trans_TargetInFront
		{
			Angle = 80f
		} & new Trans_TargetInRange
		{
			Range = 5f
		} & trans_CrocHasStraightPathToTarget)
			.AddTickTransition(staticAttack, fSMTransitionBase & new Trans_TargetInRange
			{
				Range = 2f,
				TimeToPredict = 0.5f
			} & trans_CrocHasStraightPathToTarget)
			.AddTickTransition(swimAttack, trans_IsSwimming & new Trans_TargetInRange
			{
				Range = 5.65f,
				TimeToPredict = 0.85f
			} & trans_CrocHasStraightPathToTarget)
			.AddTickTransition(state_MoveToBreakFoundation, fSMTransitionBase & ~new Trans_CanReachTarget_Slow())
			.AddFailureTransition(state_MoveToBreakFoundation) + intimidate.AddTickTransition(charge, HurtTrans).AddTickTransition(charge, new Trans_ElapsedTime
		{
			Duration = 0.25
		} & new Trans_TargetInRange
		{
			Range = 4f
		}).AddTickTransition(charge, new Trans_ElapsedTime
		{
			Duration = 1.5
		} & new Trans_TargetInRange
		{
			Range = 6f
		})
			.AddTickBranchingTrans(charge, ~new Trans_TargetInFront
			{
				Angle = 70f
			}, intimidateTurn, new Trans_Cooldown
			{
				cooldown = 5f
			})
			.AddEndTransition(charge)) + state_MoveToBreakFoundation.AddTickTransition(charge, new Trans_CanReachTarget_Slow()).AddFailureTransition(flee).AddEndTransition(state_CrocBreakFoundation)) + approachFood.AddTickBranchingTrans(charge, HurtTrans & ~new Trans_TargetHasRepellent(), flee, dstState2Trans).AddTickTransition(charge, new Trans_TargetInRange
		{
			Range = 8f
		} & ~new Trans_TargetHasRepellent()).AddTickTransition(roam, ~new Trans_SeesFood())
			.AddFailureTransition(roam)
			.AddEndTransition(bringFoodBackToWater) + bringFoodBackToWater.AddTickBranchingTrans(charge, HurtTrans & ~new Trans_TargetHasRepellent(), flee, dstState2Trans).AddTickTransition(charge, new Trans_TargetInRange
		{
			Range = 8f
		} & ~new Trans_TargetHasRepellent()).AddFailureTransition(roam)
			.AddEndTransition(roam) + bellyFlop.AddEndTransition(intimidateTurn, fSMTransitionBase & ~new Trans_TargetInFront
		{
			Angle = 30f
		}).AddEndTransition(intimidateSnap, fSMTransitionBase).AddEndTransition(charge) + state_CrocBreakFoundation.AddEndTransition(charge) + swimAttack.AddEndTransition(charge) + staticAttack.AddEndTransition(intimidateSnap) + intimidateSnap.AddEndTransition(intimidate) + intimidateTurn.AddEndTransition(intimidateSnap, new Trans_TargetInRange
		{
			Range = 6f
		}).AddEndTransition(charge) + flee.AddTickTransition(intimidate, fSMTransitionBase & new Trans_ElapsedTime
		{
			Duration = 3.0
		} & new Trans_TargetInRange
		{
			Range = 8f
		} & ~new Trans_TargetHasRepellent()).AddTickTransition(diveRoam, trans_IsSwimming).AddFailureTransition(dead, new Trans_Dead())
			.AddEndTransition(diveRoam, ~new Trans_TooFarFromWater())
			.AddEndTransition(roam) + diveRoam.AddTickTransition(charge, ~new Trans_TargetHasRepellent() & ~new Trans_IsTargetProtectedByMount() & (new Trans_IsTargetInWater() | (new Trans_TargetInRange
		{
			Range = 8f
		} & new Trans_IsTargetOnNavmesh_Slow()))).AddTickTransition(roam, ~new Trans_TargetInRange
		{
			Range = 50f
		}).AddFailureTransition(dead, new Trans_Dead());
		return new AliveLayout
		{
			entry = roam,
			idle = roam
		};
	}

	public override void Hurt(HitInfo hitInfo)
	{
		if (((Component)this).GetComponent<SenseComponent>().CanTarget(hitInfo.Initiator))
		{
			((Component)this).GetComponent<RootMotionPlayer>().PlayServerAdditive(hurt.WeakHitAdditive);
			HurtTrans.Trigger(hitInfo);
			if (CurrentState == approach || CurrentState == approachFood || CurrentState == bringFoodBackToWater || CurrentState == intimidate)
			{
				ForceTickOnTheNextUpdate();
			}
		}
	}
}
