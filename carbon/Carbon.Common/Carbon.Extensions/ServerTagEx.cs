using Steamworks;

namespace Carbon.Extensions;

public static class ServerTagEx
{
	public static void SetRequiredTag(string tag, bool compact)
	{
		string gameTags = SteamServer.GameTags;
		if (!gameTags.Contains("," + tag))
		{
			if (compact)
			{
				int num = gameTags.IndexOf('^');
				SteamServer.GameTags = ((num > 0) ? gameTags.Insert(num, tag + ",") : (gameTags + (gameTags.EndsWith(",") ? string.Empty : ",") + tag));
			}
			else
			{
				SteamServer.GameTags = gameTags + "," + tag;
			}
		}
	}

	public static void UnsetRequiredTag(string tag, bool compact)
	{
		string gameTags = SteamServer.GameTags;
		if (compact)
		{
			if (gameTags.Contains(tag))
			{
				SteamServer.GameTags = gameTags.Replace(tag, string.Empty);
			}
		}
		else if (gameTags.Contains("," + tag))
		{
			SteamServer.GameTags = gameTags.Replace("," + tag, string.Empty);
		}
	}
}
