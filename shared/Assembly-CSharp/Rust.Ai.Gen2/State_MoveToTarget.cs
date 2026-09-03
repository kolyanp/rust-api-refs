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

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
		}
		base.Agent.ResetPath();
		if (!GetMoveDestination(out var destination) || !base.Agent.SetDestinationWithParams(destination, stopAtDestination, speed, (accelerationOverride > 0f) ? new float?(accelerationOverride) : ((float?)null), (decelerationOverride > 0f) ? new float?(decelerationOverride) : ((float?)null)))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (!base.Agent.hasPath && succeedWhenDestinationIsReached)
		{
			return EFSMStateStatus.Success;
		}
		if (!GetMoveDestination(out var destination) || !base.Agent.SetDestinationWithParams(destination, stopAtDestination, speed, (accelerationOverride > 0f) ? new float?(accelerationOverride) : ((float?)null), (decelerationOverride > 0f) ? new float?(decelerationOverride) : ((float?)null)))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	protected virtual bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			destination = NavVector3.zero;
			return false;
		}
		NavVector3 navVector = base.Agent.WorldToNavSpace(targetPosition);
		destination = navVector;
		return true;
	}
}
