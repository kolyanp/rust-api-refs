using System;
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
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
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

	protected virtual bool GetMoveDestination(out Vector3 destination)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			destination = Vector3.zero;
			return false;
		}
		destination = targetPosition;
		return true;
	}
}
