using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsInTargetViewCone : FSMTransitionBase
{
	public float angle = 90f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		return IsInTargetViewCone(base.Senses, angle);
	}

	public static bool IsInTargetViewCone(SenseComponent senses, float testAngle)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!senses.FindTarget(out var target))
		{
			return false;
		}
		Vector3 val = ((Component)senses.GetBaseEntity()).transform.position - ((Component)target).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = (target.ToNonNpcPlayer(out var player) ? player.eyes.BodyForward() : ((Component)target).transform.forward);
		return Vector3.Dot(((Vector3)(ref val)).normalized, normalized) > Mathf.Cos(testAngle * (MathF.PI / 180f));
	}

	public override string ToString()
	{
		if (!Inverted)
		{
			return $"We are in target view cone of {angle} degrees";
		}
		return $"We are not in target view cone of {angle} degrees";
	}
}
