using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
[SoftRequireComponent(typeof(RustNavMeshAgent), typeof(SenseComponent), typeof(BlackboardComponent))]
public class State_ApproachFood : State_MoveToTarget
{
	public const string TriedToApproachUnreachableFood = "TriedToApproachUnreachableFood";

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindFood(out var food))
		{
			return EFSMStateStatus.Failure;
		}
		if (food.WaterFactor() > 0f || !Agent.CanReach(((Component)food).transform.position))
		{
			Blackboard.Add("TriedToApproachUnreachableFood");
			Senses.IgnoreFood(food);
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindFood(out var food))
		{
			destination = NavVector3.zero;
			return false;
		}
		destination = Agent.WorldToNavSpace(((Component)food).transform.position);
		return true;
	}
}
