using UnityEngine.Playables;

public static class SafePlayableOutputEx
{
	public static void SetSourcePlayable<TOutput, TSource>(this TOutput output, SafePlayable<TSource> source) where TOutput : struct, IPlayableOutput where TSource : struct, IPlayable
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (PlayableOutputExtensions.IsOutputValid<TOutput>(output))
		{
			if (source.IsValid())
			{
				PlayableOutputExtensions.SetSourcePlayable<TOutput, TSource>(output, source.Raw);
			}
			else
			{
				PlayableOutputExtensions.SetSourcePlayable<TOutput, Playable>(output, Playable.Null);
			}
		}
	}

	public static void ClearSourcePlayable<TOutput>(this TOutput output) where TOutput : struct, IPlayableOutput
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (PlayableOutputExtensions.IsOutputValid<TOutput>(output))
		{
			PlayableOutputExtensions.SetSourcePlayable<TOutput, Playable>(output, Playable.Null);
		}
	}
}
