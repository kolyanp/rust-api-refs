using UnityEngine;
using UnityEngine.Animations;

public static class SafeAnimationLayerMixerPlayableEx
{
	public static void SetLayerAdditive(this SafePlayable<AnimationLayerMixerPlayable> mixer, uint layer, bool additive)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (mixer.HasInput((int)layer))
		{
			AnimationLayerMixerPlayable raw = mixer.Raw;
			raw.SetLayerAdditive(layer, additive);
		}
	}

	public static void SetLayerMaskFromAvatarMask(this SafePlayable<AnimationLayerMixerPlayable> mixer, uint layer, AvatarMask mask)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mask == (Object)null) && mixer.HasInput((int)layer))
		{
			AnimationLayerMixerPlayable raw = mixer.Raw;
			raw.SetLayerMaskFromAvatarMask(layer, mask);
		}
	}
}
