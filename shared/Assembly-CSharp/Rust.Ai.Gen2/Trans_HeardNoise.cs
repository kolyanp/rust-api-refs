using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_HeardNoise : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_HeardNoise"))
		{
			if (base.Senses.FindMostRelevantNoise(out var mostRelevantNoise))
			{
				payload.entity = mostRelevantNoise.Initiator;
				payload.position = mostRelevantNoise.GuessedInitiatorPosition;
				return true;
			}
			return false;
		}
	}
}
