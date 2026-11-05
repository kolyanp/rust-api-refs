using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_HasStraightPathToTarget : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_HasStraightPathToTarget"))
		{
			if (!Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			return Agent.HasStraightPathTo(targetPosition);
		}
	}
}
