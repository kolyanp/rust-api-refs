using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_PlayAnimationRM : State_PlayAnimationBase
{
	[SerializeField]
	public RootMotionData Animation;

	public bool HasAnimation
	{
		get
		{
			if (Animation != null)
			{
				return (Object)(object)Animation.inPlaceAnimation != (Object)null;
			}
			return false;
		}
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		EFSMStateStatus result = base.OnStateEnter(payload);
		animState = AnimPlayer.PlayServerAndTakeFromPool(GetAnimation());
		return result;
	}

	protected virtual RootMotionData GetAnimation()
	{
		return Animation;
	}
}
