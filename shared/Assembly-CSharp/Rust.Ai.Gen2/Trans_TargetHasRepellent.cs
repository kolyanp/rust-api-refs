using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetHasRepellent : FSMTransitionBase
{
	public const float FleeWithin = 16f;

	public const float RetryCooldown = 2f;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_TargetHasRepellent"))
		{
			BaseEntity target;
			return Senses.FindTarget(out target) && Carries(target);
		}
	}

	public static bool Carries(BaseEntity entity)
	{
		if (entity is BasePlayer { IsNpc: false } basePlayer && (Object)(object)basePlayer.modifiers != (Object)null)
		{
			return basePlayer.modifiers.GetValue(Modifier.ModifierType.AnimalRepellent) > 0f;
		}
		return false;
	}

	public static FSMTransitionBase Near(FSMTransitionBase repelled = null)
	{
		return new Trans_Cooldown
		{
			cooldown = 2f
		} & StillNear(repelled);
	}

	public static FSMTransitionBase StillNear(FSMTransitionBase repelled = null)
	{
		return (repelled ?? new Trans_TargetHasRepellent()) & (new Trans_TargetInRange
		{
			Range = 16f
		} | new Trans_HurtByRepelledPlayer());
	}
}
