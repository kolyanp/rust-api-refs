using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_MoveToTarget : FSMStateBase
{
	[SerializeField]
	public RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.FullSprint;

	[SerializeField]
	public bool succeedWhenDestinationIsReached = true;

	[SerializeField]
	public bool stopAtDestination = true;

	[SerializeField]
	public float accelerationOverride;

	[SerializeField]
	public float decelerationOverride;

	[Tooltip("How close to the destination counts as arrived, or zero for the agent's own. A chase that does not brake needs a real one, or arriving on a target means walking into them.")]
	[SerializeField]
	public float stoppingDistanceOverride;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if ((Object)(object)payload.entity != (Object)null)
		{
			Senses.TrySetTarget(payload.entity);
		}
		Agent.ResetPath();
		if (!GetMoveDestination(out var destination) || !Move(destination))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!Agent.hasPath && succeedWhenDestinationIsReached)
		{
			return EFSMStateStatus.Success;
		}
		if (!GetMoveDestination(out var destination) || !Move(destination))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateUpdate(deltaTime);
	}

	private bool Move(NavVector3 destination)
	{
		RustNavMeshAgent agent = Agent;
		bool autoBraking = stopAtDestination;
		RustNavMeshAgent.Speeds? gait = speed;
		float? acceleration = ((accelerationOverride > 0f) ? new float?(accelerationOverride) : ((float?)null));
		float? deceleration = ((decelerationOverride > 0f) ? new float?(decelerationOverride) : ((float?)null));
		float? stoppingDistance = ((stoppingDistanceOverride > 0f) ? new float?(stoppingDistanceOverride) : ((float?)null));
		return agent.SetDestinationWithParams(destination, autoBraking, gait, acceleration, deceleration, null, null, stoppingDistance);
	}

	public override void OnStateExit()
	{
		Agent.ResetPath();
		base.OnStateExit();
	}

	protected virtual bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindTargetPosition(out var targetPosition))
		{
			destination = NavVector3.zero;
			return false;
		}
		NavVector3 navVector = Agent.WorldToNavSpace(targetPosition);
		destination = navVector;
		return true;
	}
}
