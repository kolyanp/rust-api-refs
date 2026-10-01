using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_PlayAnimation : State_PlayAnimationBase
{
	[SerializeField]
	public AnimationClip Animation;

	public bool HasAnimation => (Object)(object)Animation != (Object)null;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		EFSMStateStatus result = base.OnStateEnter(payload);
		animState = AnimPlayer.PlayServerAndTakeFromPool(Animation);
		return result;
	}

	protected virtual AnimationClip GetAnimation()
	{
		return Animation;
	}
}
