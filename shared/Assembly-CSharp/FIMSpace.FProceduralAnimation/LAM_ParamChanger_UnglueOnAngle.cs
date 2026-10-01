using UnityEngine;

namespace FIMSpace.FProceduralAnimation;

public class LAM_ParamChanger_UnglueOnAngle : LegsAnimatorControlModuleBase
{
	private LegsAnimator.Variable _UnglueAngleOnMoving;

	private float initialUnglueOn = 30f;

	public override void OnInit(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		_UnglueAngleOnMoving = helper.RequestVariable("Unglue Angle On Moving", 70f);
		initialUnglueOn = LA.UnglueOn;
	}

	public override void OnUpdate(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		LA.UnglueOn = Mathf.Lerp(initialUnglueOn, _UnglueAngleOnMoving.GetFloat(), LA.IsMovingBlend);
	}
}
