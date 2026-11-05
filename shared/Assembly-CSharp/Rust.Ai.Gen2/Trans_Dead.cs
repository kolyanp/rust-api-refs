using ConVar;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (AI.logIssues)
		{
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
}
