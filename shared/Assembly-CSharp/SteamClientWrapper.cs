using UnityEngine;

public class SteamClientWrapper : SingletonComponent<SteamClientWrapper>
{
	public Texture2D DefaultAvatar;

	private static readonly Phrase TimelineDeathTitle;

	private static readonly Phrase TimelineKillTitle;

	static SteamClientWrapper()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		TimelineDeathTitle = new Phrase("timeline.death", "Death");
		TimelineKillTitle = new Phrase("timeline.kill", "Kill");
	}
}
