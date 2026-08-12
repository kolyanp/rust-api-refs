using UnityEngine;

namespace Rust.UI.MainMenu;

public static class UI_Utils
{
	private static Phrase _monthSingularPhrase;

	private static Phrase _monthsPhrase;

	private static Phrase _weekSingularPhrase;

	private static Phrase _weeksPhrase;

	private static Phrase _daysSingularPhrase;

	private static Phrase _daysPhrase;

	private static Phrase _hourSingularPhrase;

	private static Phrase _hoursPhrase;

	private static Phrase _minuteSingularPhrase;

	private static Phrase _minutesPhrase;

	private static Phrase _secondSingularPhrase;

	private static Phrase _secondsPhrase;

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

	static UI_Utils()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		_monthSingularPhrase = new Phrase("time.month", "month");
		_monthsPhrase = new Phrase("time.months", "months");
		_weekSingularPhrase = new Phrase("time.week", "week");
		_weeksPhrase = new Phrase("time.weeks", "weeks");
		_daysSingularPhrase = new Phrase("time.day", "day");
		_daysPhrase = new Phrase("time.days", "days");
		_hourSingularPhrase = new Phrase("time.hour", "hour");
		_hoursPhrase = new Phrase("time.hours", "hours");
		_minuteSingularPhrase = new Phrase("time.minute", "minute");
		_minutesPhrase = new Phrase("time.minutes", "minutes");
		_secondSingularPhrase = new Phrase("time.second", "second");
		_secondsPhrase = new Phrase("time.seconds", "seconds");
	}
}
