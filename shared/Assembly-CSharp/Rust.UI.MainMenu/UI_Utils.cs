using UnityEngine;

namespace Rust.UI.MainMenu;

public static class UI_Utils
{
	private static Phrase _monthSingularPhrase = new Phrase("time.month", "month");

	private static Phrase _monthsPhrase = new Phrase("time.months", "months");

	private static Phrase _weekSingularPhrase = new Phrase("time.week", "week");

	private static Phrase _weeksPhrase = new Phrase("time.weeks", "weeks");

	private static Phrase _daysSingularPhrase = new Phrase("time.day", "day");

	private static Phrase _daysPhrase = new Phrase("time.days", "days");

	private static Phrase _hourSingularPhrase = new Phrase("time.hour", "hour");

	private static Phrase _hoursPhrase = new Phrase("time.hours", "hours");

	private static Phrase _minuteSingularPhrase = new Phrase("time.minute", "minute");

	private static Phrase _minutesPhrase = new Phrase("time.minutes", "minutes");

	private static Phrase _secondSingularPhrase = new Phrase("time.second", "second");

	private static Phrase _secondsPhrase = new Phrase("time.seconds", "seconds");

	private static bool HasStreamerMode(BasePlayer ply = null)
	{
		if ((Object)(object)ply != (Object)null)
		{
			return ply.net.connection.info.GetBool("global.streamermode");
		}
		return false;
	}

	public static string StreamerModeSanitize(string text, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return "STREAMER MODE ENABLED";
		}
		return text;
	}

	public static string StreamerModeSanitizeCustomMessage(string text, string message, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return message;
		}
		return text;
	}

	public static string StreamerModeSanitizeShort(string text, BasePlayer ply = null)
	{
		if (HasStreamerMode(ply))
		{
			return "?";
		}
		return text;
	}
}
