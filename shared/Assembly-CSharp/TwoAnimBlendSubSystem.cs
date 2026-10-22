using UnityEngine;

public class TwoAnimBlendSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip ClipA;

	[SerializeField]
	private AnimationClip ClipB;

	[Range(0f, 1f)]
	[SerializeField]
	[SubSystemVariable]
	private float Blend;
}
