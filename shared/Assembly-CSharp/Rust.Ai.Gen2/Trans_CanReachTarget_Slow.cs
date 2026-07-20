using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_CanReachTarget_Slow : FSMSlowTransitionBase
{
	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_CanReachTarget_Slow"))
		{
			if (!base.Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			return base.Agent.CanReach(targetPosition);
		}
	}
}
