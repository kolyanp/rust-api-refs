using UnityEngine;

namespace Rust.Ai.Gen2;

public class Trans_Dead : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		return true;
	}

	public override void OnTransitionTaken(FSMStateBase from, FSMStateBase to)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)Owner).transform.position;
		Debug.LogWarning((object)string.Format("Transitioning to dead state from {0}: {1} suicided on AI failure at {2} in {3}", new object[4]
		{
			from.Name,
			Owner.ShortPrefabName,
			position,
			MapHelper.PositionToString(position)
		}), (Object)(object)Owner);
	}
}
