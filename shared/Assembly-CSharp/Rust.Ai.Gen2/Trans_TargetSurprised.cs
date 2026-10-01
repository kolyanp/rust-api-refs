namespace Rust.Ai.Gen2;

public class Trans_TargetSurprised : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_TargetSurprised"))
		{
			if (!Senses.FindTarget(out var target))
			{
				return true;
			}
			if (!Senses.GetVisibilityStatus(target, out var status))
			{
				return true;
			}
			return status.TryConsumeSurprise();
		}
	}
}
