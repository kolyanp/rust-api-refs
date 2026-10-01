using UnityEngine;

public class SteamClientWrapper : SingletonComponent<SteamClientWrapper>
{
	public Texture2D DefaultAvatar;

	public const ulong MinSteamId = 76500000000000000uL;

	private static readonly Phrase TimelineDeathTitle = new Phrase("timeline.death", "Death");

	private static readonly Phrase TimelineKillTitle = new Phrase("timeline.kill", "Kill");

	static SteamClientWrapper()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}
