using UnityEngine;
using UnityEngine.Serialization;

public class OverrideAnimationSubSystem : AnimationSubSystem
{
	[SerializeField]
	[FormerlySerializedAs("TelephoneIdleClip")]
	private AnimationClip ClipToPlay;
}
