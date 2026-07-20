using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_CrocTurn : State_PlayAnimationRM
{
	[SerializeField]
	public RootMotionData turn90L;

	[SerializeField]
	public RootMotionData turn90R;

	[SerializeField]
	public RootMotionData turn180;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 val = targetPosition - ((Component)Owner).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = Vector3.SignedAngle(((Component)Owner).transform.forward, normalized, Vector3.up);
		if (Mathf.Abs(num) > 130f)
		{
			Animation = turn180;
		}
		else if (num > 0f)
		{
			Animation = turn90R;
		}
		else
		{
			Animation = turn90L;
		}
		return base.OnStateEnter(payload);
	}
}
