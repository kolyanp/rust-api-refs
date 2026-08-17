using UnityEngine;

public class TwoAnimBlendSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip ClipA;

	[SerializeField]
	private AnimationClip ClipB;

	[SubSystemVariable]
	[Range(0f, 1f)]
	[SerializeField]
	private float Blend;
}
