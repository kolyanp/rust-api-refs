using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_IsTargetProtectedByMount : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_IsTargetProtectedByMount"))
		{
			if (!base.Senses.FindTarget(out var target) || !target.ToNonNpcPlayer(out var player))
			{
				return false;
			}
			BaseMountable castedUnityObject;
			return BaseNetworkableEx.Is<BaseMountable>((Object)(object)player.GetMounted(), out castedUnityObject) && castedUnityObject.ProtectsFromAnimals;
		}
	}
}
