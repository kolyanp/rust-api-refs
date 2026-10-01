using UnityEngine;

namespace FIMSpace.FProceduralAnimation;

public class LAM_DesiredDirectionFromTransform : LegsAnimatorControlModuleBase
{
	private LegsAnimator.Variable _Reaction;

	private LegsAnimator.Variable _Thres;

	private LegsAnimator.Variable _IsMov;

	private Vector3 calculatedVelo = Vector3.zero;

	private Vector3 _sd_average = Vector3.zero;

	private Vector3 previousPosition = Vector3.zero;

	public override void OnInit(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		_Reaction = helper.RequestVariable("Reaction Speed", 0.8f);
		_IsMov = helper.RequestVariable("Control 'IsMoving'", false);
		_Thres = helper.RequestVariable("Not Moving Threshold", 0.2f);
		previousPosition = LA.BaseTransform.position;
	}

	public override void OnUpdate(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = LA.BaseTransform.position - previousPosition;
		previousPosition = LA.BaseTransform.position;
		Vector3 val2 = val / LA.DeltaTime;
		val2 = LA.ToRootLocalSpaceVec(val2);
		val2.y = 0f;
		val2 = LA.RootToWorldSpaceVec(val2);
		float magnitude = calculatedVelo.magnitude;
		val2 = Vector3.Slerp(val2, val2.normalized, Mathf.InverseLerp(0f, magnitude, LA.ScaleReference));
		calculatedVelo = Vector3.SmoothDamp(calculatedVelo, val2, ref _sd_average, 5E-05f + (1f - _Reaction.GetFloat()) * 0.15f, 100000f, LA.DeltaTime);
		if (_IsMov.GetBool())
		{
			LA.User_SetIsMoving(magnitude > LA.ScaleReference * _Thres.GetFloat());
		}
		LA.User_SetDesiredMovementDirection(calculatedVelo);
	}

	public LAM_DesiredDirectionFromTransform()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
