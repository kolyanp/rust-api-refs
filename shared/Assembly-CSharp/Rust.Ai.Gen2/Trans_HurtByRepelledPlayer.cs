using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_HurtByRepelledPlayer : FSMTransitionBase
{
	public const float HurtWithin = 5f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		if (Owner is BaseCombatEntity { SecondsSinceAttacked: <5f } baseCombatEntity)
		{
			return Trans_TargetHasRepellent.Carries(baseCombatEntity.lastAttacker);
		}
		return false;
	}
}
