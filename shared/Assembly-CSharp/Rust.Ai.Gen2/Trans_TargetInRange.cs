using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetInRange : FSMTransitionBase
{
	[SerializeField]
	public float Range = 4f;

	[SerializeField]
	public float TimeToPredict;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_TargetInRange"))
		{
			if (!base.Senses.FindTarget(out var target))
			{
				return false;
			}
			Vector3 val = ((Component)target).transform.position;
			if (TimeToPredict > 0f && target.ToNonNpcPlayer(out var player))
			{
				Vector3 inferedVelocity = player.inferedVelocity;
				inferedVelocity = Vector3.ProjectOnPlane(inferedVelocity, ((Component)Owner).transform.right);
				val += inferedVelocity * TimeToPredict;
			}
			return Vector3.Distance(val, ((Component)Owner).transform.position) <= Range;
		}
	}

	public override string GetName()
	{
		return string.Format("{0} {1}{2}m", base.GetName(), Inverted ? ">=" : "<", Range);
	}
}
