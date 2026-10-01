using UnityEngine;
using UnityEngine.Animations;

public static class SafeAnimationClipPlayableEx
{
	public static AnimationClip GetAnimationClip(this SafePlayable<AnimationClipPlayable> playable)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return null;
		}
		AnimationClipPlayable raw = playable.Raw;
		return raw.GetAnimationClip();
	}

	public static void SetApplyFootIK(this SafePlayable<AnimationClipPlayable> playable, bool apply)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimationClipPlayable raw = playable.Raw;
			raw.SetApplyFootIK(apply);
		}
	}

	public static void SetApplyPlayableIK(this SafePlayable<AnimationClipPlayable> playable, bool apply)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimationClipPlayable raw = playable.Raw;
			raw.SetApplyPlayableIK(apply);
		}
	}
}
