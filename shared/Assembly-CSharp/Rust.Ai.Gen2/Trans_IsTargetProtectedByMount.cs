using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsTargetProtectedByMount : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_IsTargetProtectedByMount"))
		{
			BaseEntity target;
			return Senses.FindTarget(out target) && IsProtected(target);
		}
	}

	public static bool IsProtected(BaseEntity target)
	{
		if (!target.ToNonNpcPlayer(out var player))
		{
			return false;
		}
		if (BaseNetworkableEx.Is<BaseMountable>((Object)(object)player.GetMounted(), out BaseMountable castedUnityObject))
		{
			return castedUnityObject.ProtectsFromAnimals;
		}
		return false;
	}
}
