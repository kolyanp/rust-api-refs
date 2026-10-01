using UnityEngine.Animations;

public static class SafeAnimationScriptPlayableEx
{
	public static void SetJobData<TJob>(this SafePlayable<AnimationScriptPlayable> playable, TJob job) where TJob : struct, IAnimationJob
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimationScriptPlayable raw = playable.Raw;
			raw.SetJobData<TJob>(job);
		}
	}

	public static TJob GetJobData<TJob>(this SafePlayable<AnimationScriptPlayable> playable) where TJob : struct, IAnimationJob
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return default;
		}
		AnimationScriptPlayable raw = playable.Raw;
		return raw.GetJobData<TJob>();
	}

	public static void SetProcessInputs(this SafePlayable<AnimationScriptPlayable> playable, bool processInputs)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimationScriptPlayable raw = playable.Raw;
			raw.SetProcessInputs(processInputs);
		}
	}
}
