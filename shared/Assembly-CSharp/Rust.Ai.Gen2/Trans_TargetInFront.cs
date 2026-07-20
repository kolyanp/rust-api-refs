using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetInFront : FSMTransitionBase
{
	[SerializeField]
	public float Angle = 90f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_TargetInFront"))
		{
			if (!base.Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			Vector3 val = targetPosition - ((Component)Owner).transform.position;
			return Vector3.Angle(((Component)Owner).transform.forward, val) < Angle;
		}
	}

	public override string GetName()
	{
		return string.Format("{0} {1}{2}°", base.GetName(), Inverted ? ">=" : "<", Angle);
	}
}
