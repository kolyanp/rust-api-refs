using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_AnimalChase : State_MoveToTarget
{
	[SerializeField]
	[Tooltip("How far from an off-navmesh target to look for somewhere the animal can actually stand.")]
	public float offNavmeshSampleRadius = 6f;

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!Agent.hasPath && Senses.FindTargetPosition(out var targetPosition))
		{
			Vector3 val = Vector3Ex.NormalizeXZ(targetPosition - ((Component)Owner).transform.position);
			if (val.sqrMagnitude > 0.001f)
			{
				((Component)Owner).transform.rotation = Quaternion.RotateTowards(((Component)Owner).transform.rotation, Quaternion.LookRotation(val), Agent.angularSpeed * deltaTime);
			}
		}
		return base.OnStateUpdate(deltaTime);
	}

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!Senses.FindTargetPosition(out var targetPosition))
		{
			destination = NavVector3.zero;
			return false;
		}
		NavVector3 navVector = Agent.WorldToNavSpace(targetPosition);
		if (Agent.SamplePosition(navVector, out var hitNS, 0.5f))
		{
			destination = hitNS.position;
			return true;
		}
		if (Agent.SamplePosition(navVector, out var hitNS2, offNavmeshSampleRadius))
		{
			destination = hitNS2.position;
			return true;
		}
		destination = navVector;
		return true;
	}
}
