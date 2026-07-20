using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_TargetLkpInRange : FSMTransitionBase
{
	public float Range = 10f;

	public bool Predict;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_TargetLkpInRange"))
		{
			if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, Predict))
			{
				return false;
			}
			return Vector3.Distance(((Component)Owner).transform.position, lkp) <= Range;
		}
	}
}
