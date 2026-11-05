using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_RecentlyHurt : FSMTransitionBase
{
	[SerializeField]
	private float thresholdSeconds = 5f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		if (Owner is BaseCombatEntity { lastAttackedTime: >0f } baseCombatEntity)
		{
			return Time.time - baseCombatEntity.lastAttackedTime < thresholdSeconds;
		}
		return false;
	}
}
