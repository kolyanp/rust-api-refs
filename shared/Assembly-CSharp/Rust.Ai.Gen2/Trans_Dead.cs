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
		Debug.Log((object)("Transitioning to dead state from " + from.Name));
	}
}
