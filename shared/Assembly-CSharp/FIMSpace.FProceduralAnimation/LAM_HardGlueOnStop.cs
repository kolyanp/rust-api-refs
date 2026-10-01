namespace FIMSpace.FProceduralAnimation;

public class LAM_HardGlueOnStop : LegsAnimatorControlModuleBase
{
	public float FrontMargin = 0.3f;

	public float ForceForSeconds = 0.6f;

	private LegsAnimator.Variable _beforeV;

	public override void OnInit(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		_beforeV = helper.RequestVariable("Hard Glue Before Move", 0f);
	}

	public override void OnPreLateUpdate(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (!LA.IsMoving && LA.IsGrounded && LA.StoppedTime < ForceForSeconds)
		{
			for (int i = 0; i < LA.Legs.Count; i++)
			{
				LegsAnimator.Leg leg = LA.Legs[i];
				if (leg.AnkleH.LastKeyframeRootPos.z > (0f - LA.ScaleReferenceNoScale) * FrontMargin)
				{
					leg.G_CustomForceAttach = true;
				}
			}
		}
		if (!(_beforeV.GetFloat() > 0f) || !LA.IsMoving || !LA.IsGrounded || !(LA.MovingTime < _beforeV.GetFloat()))
		{
			return;
		}
		for (int j = 0; j < LA.Legs.Count; j++)
		{
			LegsAnimator.Leg leg2 = LA.Legs[j];
			if (leg2.IKProcessor.GetStretchValue(leg2.IKProcessor.IKTargetPosition) < 1.01f)
			{
				leg2.G_CustomForceAttach = true;
			}
		}
	}
}
