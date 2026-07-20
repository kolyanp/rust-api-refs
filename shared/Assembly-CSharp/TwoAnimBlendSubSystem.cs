using UnityEngine;

public class TwoAnimBlendSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip ClipA;

	[SerializeField]
	private AnimationClip ClipB;

	[SubSystemVariable]
	[SerializeField]
	[Range(0f, 1f)]
	private float Blend;
}
