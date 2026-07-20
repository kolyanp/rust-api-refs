using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsFlankedByTarget : FSMTransitionBase
{
	private Vector3? previousLkp;

	public override void OnStateEnter()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.OnStateEnter();
		previousLkp = null;
		if (base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true))
		{
			previousLkp = lkp;
		}
	}

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsFlankedByTarget"))
		{
			if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true))
			{
				return false;
			}
			if (previousLkp.HasValue && Vector3.Distance(previousLkp.Value, lkp) > 2f && Vector3.Angle(lkp - ((Component)Owner).transform.position, previousLkp.Value - ((Component)Owner).transform.position) > 75f)
			{
				return true;
			}
			previousLkp = lkp;
			return false;
		}
	}

	public override void OnStateExit()
	{
		base.OnStateExit();
		previousLkp = null;
	}
}
