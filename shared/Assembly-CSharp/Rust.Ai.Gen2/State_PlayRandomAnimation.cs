using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_PlayRandomAnimation : State_PlayAnimationBase
{
	[SerializeField]
	public AnimationClip[] animations;

	[SerializeField]
	public float[] weights;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		EFSMStateStatus result = base.OnStateEnter(payload);
		if (animations.Length != 0)
		{
			animState = AnimPlayer.PlayServerAndTakeFromPool(PickAnimation());
		}
		return result;
	}

	private AnimationClip PickAnimation()
	{
		if (weights == null || weights.Length == 0)
		{
			return Rng.Pick(animations);
		}
		float num = 0f;
		for (int i = 0; i < animations.Length; i++)
		{
			num += WeightOf(i);
		}
		float num2 = Rng.Range(0f, num);
		for (int j = 0; j < animations.Length; j++)
		{
			num2 -= WeightOf(j);
			if (num2 < 0f)
			{
				return animations[j];
			}
		}
		return animations[animations.Length - 1];
	}

	private float WeightOf(int index)
	{
		if (index >= weights.Length)
		{
			return 1f;
		}
		return weights[index];
	}
}
