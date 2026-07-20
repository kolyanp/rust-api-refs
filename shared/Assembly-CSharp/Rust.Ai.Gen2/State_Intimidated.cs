using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Intimidated : State_PlayAnimationRM
{
	private static readonly float facingAwayDotThreshold = Mathf.Cos(MathF.PI / 2f);

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		FaceTarget = true;
		if (base.Senses.FindTargetPosition(out var targetPosition))
		{
			Vector3 forward = ((Component)Owner).transform.forward;
			Vector3 val = ((Component)Owner).transform.position - targetPosition;
			if (Vector3.Dot(forward, ((Vector3)(ref val)).normalized) > facingAwayDotThreshold)
			{
				return EFSMStateStatus.Success;
			}
		}
		return base.OnStateEnter(payload);
	}
}
