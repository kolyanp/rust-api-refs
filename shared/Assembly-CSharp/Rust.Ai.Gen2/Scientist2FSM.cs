using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(NpcZoneComponent), typeof(NPCEncounterTimer), typeof(NpcShootingComponent))]
[SoftRequireComponent(typeof(RustNavMeshAgent), typeof(RootMotionPlayer), typeof(SenseComponent))]
[SoftRequireComponent(typeof(BlackboardComponent), typeof(NpcBarkManager))]
public class Scientist2FSM : FSMComponent
{
	public State_PatrolIdle idle = new State_PatrolIdle();

	public State_Patrol patrol = new State_Patrol();

	public State_Search search = new State_Search();

	public State_ScientistRush chase = new State_ScientistRush();

	public State_DogFight dogFight = new State_DogFight();

	public State_MoveToCoverHiddenFromTarget goHide = new State_MoveToCoverHiddenFromTarget();

	public State_MoveToPointWithLosOnTarget popOut = new State_MoveToPointWithLosOnTarget();

	public State_StayInCover stayInCover = new State_StayInCover();

	public State_ScientistSurprised surprised = new State_ScientistSurprised();

	public State_Flank flank = new State_Flank();

	public State_ThrowGrenade throwGrenade = new State_ThrowGrenade();

	public State_ScriptedNade throwScriptedGrenade = new State_ScriptedNade();

	private Trans_Triggerable_HitInfo HurtTrans;

	[NonSerialized]
	public Trans_Triggerable RushPositionTrans;

	[NonSerialized]
	public Trans_Triggerable SearchTrans;

	private NpcBarkComponent _barkComponent;

	private NpcBarkComponent BarkComponent => _barkComponent ?? (_barkComponent = ((Component)baseEntity).GetComponent<NpcBarkComponent>());

	protected override AliveLayout BuildAlive(State_Nothing onNavmesh)
	{
		HurtTrans = new Trans_Triggerable_HitInfo();
		RushPositionTrans = new Trans_Triggerable();
		SearchTrans = new Trans_Triggerable();
		State_Nothing state_Nothing = new State_Nothing
		{
			Name = "No target"
		};
		State_Nothing state_Nothing2 = new State_Nothing
		{
			Name = "Has target"
		};
		State_Nothing state_Nothing3 = new State_Nothing
		{
			Name = "Searching"
		};
		State_Nothing state_Nothing4 = new State_Nothing
		{
			Name = "Ready to assist rush"
		};
		State_Nothing state_Nothing5 = new State_Nothing
		{
			Name = "Looking for grenade opportunities"
		};
		State_Search state_Search = search.Clone() as State_Search;
		state_Search.Name = "SearchPastLKP";
		state_Search.predict = true;
		state_Search.loop = false;
		FSMTransitionBase fSMTransitionBase = ~new Trans_IsReloading();
		Trans_IsReloading trans_IsReloading = new Trans_IsReloading();
		Trans_CanSeeTarget trans_CanSeeTarget = new Trans_CanSeeTarget();
		FSMTransitionBase fSMTransitionBase2 = ~new Trans_CanSeeTarget();
		Trans_And trans_And = new Trans_TargetLkpInRange
		{
			Range = 50f
		} & new Trans_IsTargetLkpInOurZone();
		Trans_Or trans_Or = ~new Trans_TargetLkpInRange
		{
			Range = 50f
		} | ~new Trans_IsTargetLkpInOurZone();
		Trans_And trans_And2 = new Trans_Cooldown
		{
			cooldown = 5f
		} & new Trans_TargetSurprised();
		Trans_And transition = new Trans_Cooldown
		{
			cooldown = 5f
		} & trans_And;
		Trans_And transition2 = new Trans_Cooldown
		{
			cooldown = 5f
		} & fSMTransitionBase2 & (new Trans_TargetIsLowHealth() | (new Trans_TargetIsUndergeared() & trans_And)) & ~new Trans_IsTargetInWater();
		Trans_And transition3 = ~new Trans_HasBlackboardBool
		{
			Key = "ThrownGrenadeRecently"
		} & fSMTransitionBase & new Trans_TargetLost
		{
			minLostDuration = 2f
		} & new Trans_TargetCamping() & ~new Trans_TargetLkpInRange
		{
			Range = 6f
		} & new Trans_TargetLkpInRange
		{
			Range = 50f
		} & new Trans_CanThrowGrenade();
		onNavmesh.AddChildren(state_Nothing.AddTickTransition(surprised, new Trans_HasTarget() & trans_And2).AddTickTransition(dogFight, new Trans_HasTarget() & trans_CanSeeTarget & trans_And).AddTickTransition(goHide, new Trans_HasTarget()) + patrol.AddFailureTransition(dead, new Trans_Dead()).AddEndTransition(idle) + idle.AddTickTransition(patrol, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 1.0,
			MaxDuration = 10.0
		}), state_Nothing2.AddTickTransition(patrol, ~new Trans_HasTarget()) + (state_Nothing4.AddTickTransition(chase, fSMTransitionBase & RushPositionTrans) + (state_Nothing5.AddTickTransition(throwGrenade, transition3).AddTickTransition(search, SearchTrans & trans_And) + dogFight.AddTickTransition(goHide, new Trans_Cooldown
		{
			cooldown = 5f
		} & (trans_Or | (trans_IsReloading & new Trans_Bark
		{
			category = ENPCVoicelineCategory.Reload
		}))).AddTickTransition(chase, transition2).AddTickTransition(goHide, new Trans_ElapsedTimeRandomized
		{
			MinDuration = 1.0,
			MaxDuration = 4.0
		})
			.AddFailureTransition(goHide) + goHide.AddTickBranchingTrans(dogFight, trans_CanSeeTarget & fSMTransitionBase & new Trans_TargetInRange
		{
			Range = 3f
		}, surprised, trans_And2).AddTickTransition(chase, transition2).AddFailureTransition(dogFight)
			.AddEndTransition(popOut, new Trans_RandomChance
			{
				Chance = 0.5f
			} & fSMTransitionBase & trans_And)
			.AddEndTransition(stayInCover) + stayInCover.AddTickTransition(goHide, trans_IsReloading & (trans_CanSeeTarget | HurtTrans | (new Trans_IsTargetRunning() & new Trans_TargetInRange
		{
			Range = 20f
		}))).AddTickBranchingTrans(dogFight, fSMTransitionBase & (trans_CanSeeTarget | HurtTrans), surprised, trans_And2).AddTickTransition(popOut, trans_And & fSMTransitionBase & new Trans_ElapsedTimeRandomized
		{
			MinDuration = 5.0,
			MaxDuration = 10.0
		})
			.AddTickTransition(goHide, new Trans_ElapsedTimeRandomized
			{
				MinDuration = 0.75,
				MaxDuration = 1.25
			} & new Trans_IsFlankedByTarget()) + popOut.AddTickBranchingTrans(dogFight, trans_CanSeeTarget, surprised, trans_And2).AddEndTransition(chase, new Trans_RandomChance
		{
			Chance = 0.1f
		} & new Trans_TargetLost() & trans_And).AddEndTransition(flank, transition)
			.AddEndTransition(goHide) + flank.AddTickBranchingTrans(dogFight, trans_CanSeeTarget, surprised, trans_And2).AddFailureTransition(goHide).AddEndTransition(state_Search)) + surprised.AddEndTransition(dogFight, trans_CanSeeTarget & trans_And).AddEndTransition(goHide) + (state_Nothing3.AddTickBranchingTrans(dogFight, trans_CanSeeTarget, surprised, trans_And2).AddTickBranchingTrans(goHide, new Trans_Cooldown
		{
			cooldown = 5f
		} & fSMTransitionBase2 & new Trans_HeardNoise(), chase, trans_And).AddFailureTransition(goHide) + state_Search.AddEndTransition(search, new Trans_Bark
		{
			category = ENPCVoicelineCategory.Search
		}) + search)) + throwGrenade.AddFailureTransition(goHide).AddEndTransition(chase) + throwScriptedGrenade.AddFailureTransition(goHide).AddEndTransition(chase) + chase.AddTickBranchingTrans(dogFight, trans_CanSeeTarget, surprised, trans_And2).AddFailureTransition(goHide).AddEndTransition(state_Search));
		return new AliveLayout
		{
			entry = patrol,
			idle = idle
		};
	}

	public override void Hurt(HitInfo hitInfo)
	{
		HurtTrans.Trigger(hitInfo);
	}
}
