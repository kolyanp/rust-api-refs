using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_DebugMoveToPosition : State_MoveToTarget
{
	public const float navmeshSampleRadius = 5f;

	[NonSerialized]
	public Vector3 destinationWS;

	private NpcShootingComponent _shooting;

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if ((Object)(object)Shooting != (Object)null)
		{
			Shooting.AllowShooting = false;
		}
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		if ((Object)(object)Shooting != (Object)null)
		{
			Shooting.AllowShooting = true;
		}
		base.OnStateExit();
	}

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		destination = default;
		Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
		Vector3 positionWS = worldToNavMeshSpace.MultiplyPoint(destinationWS);
		if (!Agent.SamplePosition(positionWS, out var hitWS, 5f))
		{
			return false;
		}
		destination = Agent.WorldToNavSpace(hitWS.position);
		return true;
	}
}
