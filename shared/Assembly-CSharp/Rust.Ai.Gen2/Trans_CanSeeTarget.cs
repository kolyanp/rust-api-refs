namespace Rust.Ai.Gen2;

public class Trans_CanSeeTarget : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_CanSeeTarget"))
		{
			if (!Senses.FindTarget(out var target))
			{
				return false;
			}
			if (!Senses.GetVisibilityStatus(target, out var status))
			{
				return false;
			}
			return status.IsVisible && status.IsAware;
		}
	}
}
