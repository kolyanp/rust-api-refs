using UnityEngine;

public class SteamClientWrapper : SingletonComponent<SteamClientWrapper>
{
	public Texture2D DefaultAvatar;

	private static readonly Phrase TimelineDeathTitle = new Phrase("timeline.death", "Death");

	private static readonly Phrase TimelineKillTitle = new Phrase("timeline.kill", "Kill");
}
