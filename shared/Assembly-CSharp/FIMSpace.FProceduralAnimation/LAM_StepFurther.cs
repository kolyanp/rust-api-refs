using UnityEngine;

namespace FIMSpace.FProceduralAnimation;

public class LAM_StepFurther : LegsAnimatorControlModuleBase
{
	private LegsAnimator.Variable _hipsV;

	private LegsAnimator.Variable _powerV;

	private LegsAnimator.Variable _mulV;

	private Vector3 customVelo = Vector3.zero;

	private Vector3 velo = Vector3.zero;

	private Vector3 finalVelo = Vector3.zero;

	private Vector3 _sd_velo = Vector3.zero;

	private Vector3 lastPos;

	public bool UsingCustomVelo { get; set; }

	public void ProvideVelocity(Vector3 velocity)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		UsingCustomVelo = true;
		customVelo = velo;
	}

	public override void OnInit(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		base.OnInit(helper);
		UsingCustomVelo = false;
		_powerV = helper.RequestVariable("Predict Forward Offset", 0.1f);
		_hipsV = helper.RequestVariable("Predict Forward Hips Offset", 0f);
		_mulV = helper.RequestVariable("Extra Multiplier", 1f);
		lastPos = ((Component)LA).transform.position;
	}

	public override void OnPreLateUpdate(LegsAnimator.LegsAnimatorCustomModuleHelper helper)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vec;
		if (UsingCustomVelo)
		{
			vec = LA.ToRootLocalSpaceVec(customVelo);
		}
		else if (Object.op_Implicit((Object)(object)LA.Rigidbody) && !LA.Rigidbody.isKinematic)
		{
			vec = LA.ToRootLocalSpaceVec(LA.Rigidbody.linearVelocity);
		}
		else if (LA.usingCustomDesiredMovementDirection)
		{
			vec = LA.ToRootLocalSpaceVec(LA.DesiredMovementDirection * LA.IsMovingBlend);
		}
		else
		{
			vec = ((!(LA.DeltaTime > 0f)) ? Vector3.zero : LA.ToRootLocalSpaceVec((LegsAnim.BaseTransform.position - lastPos) / LA.DeltaTime));
		}
		lastPos = LegsAnim.BaseTransform.position;
		vec.y = 0f;
		vec = LA.RootToWorldSpaceVec(vec);
		velo = Vector3.SmoothDamp(velo, vec, ref _sd_velo, 0.1f, 1000000f, LA.DeltaTime);
		finalVelo = velo * (_powerV.GetFloat() * _mulV.GetFloat() * EffectBlend);
	}

	public override void Leg_LatePreRaycastingUpdate(LegsAnimator.LegsAnimatorCustomModuleHelper helper, LegsAnimator.Leg leg)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (_powerV.GetFloat() > 0f)
		{
			leg.OverrideFinalAndSourceIKPos(leg.GetFinalIKPos() + finalVelo);
			leg.OverrideControlPositionsWithCurrentIKState();
		}
		if (leg.PlaymodeIndex == 0 && _hipsV.GetFloat() > 0f)
		{
			LegsAnimator lA = LA;
			lA._Hips_Modules_ExtraWOffset += velo * (_hipsV.GetFloat() * EffectBlend);
		}
	}

	public LAM_StepFurther()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
	}
}
