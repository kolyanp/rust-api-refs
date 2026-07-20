using System;

namespace Carbon.Extensions;

public static class TimeEx
{
	public static string Format<T>(T v, bool shortName = true, bool showMiliseconds = false) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
	{
		float num = (float)Convert.ChangeType(v, typeof(float));
		int num2 = (int)((num - (float)(int)num) * 10f);
		long num3 = (long)num;
		double num4 = Math.Floor((double)num / 60.0);
		double num5 = Math.Floor(num4 / 60.0);
		double num6 = Math.Floor(num5 / 24.0);
		double num7 = Math.Floor(num6 / 7.0);
		string arg = "";
		string arg2 = "";
		string arg3 = "";
		string text = "";
		string text2 = "";
		string text3 = "";
		if (shortName)
		{
			if (showMiliseconds && num3 < 5)
			{
				if (num3 != 0L || num2 != 0)
				{
					return string.Format("{0}{1}", string.Format((num3 == 0L) ? "" : "{0}s", num3), string.Format("{0}ms", num2, arg));
				}
				return string.Format("0s", Array.Empty<object>());
			}
			if (num3 < 60)
			{
				return $"{num3}s";
			}
			if (num4 < 60.0)
			{
				return string.Format("{1}{0}", new object[5]
				{
					string.Format((num3 % 60 == 0L) ? "" : "{0}s", num3 % 60, arg2),
					string.Format("{0}m", num4, arg3),
					num5,
					num6,
					num7
				});
			}
			if (num5 < 48.0)
			{
				return string.Format("{2}{1}{0}", new object[5]
				{
					string.Format((num3 % 60 == 0L) ? "" : "{0}s", num3 % 60),
					string.Format((num4 % 60.0 == 0.0) ? "" : "{0}m", num4 % 60.0),
					$"{num5}h",
					num6,
					num7
				});
			}
			if (num6 < 7.0)
			{
				return string.Format("{3}{2}{1}{0}", new object[5]
				{
					string.Format((num3 % 60 == 0L) ? "" : "{0}s", num3 % 60),
					string.Format((num4 % 60.0 == 0.0) ? "" : "{0}m", num4 % 60.0),
					$"{num5 % 24.0}h",
					$"{num6 % 7.0}d",
					num7
				});
			}
			return string.Format("{4}{3}{2}{1}{0}", new object[5]
			{
				string.Format((num3 % 60 == 0L) ? "" : "{0}s", num3 % 60),
				string.Format((num4 % 60.0 == 0.0) ? "" : "{0}m", num4 % 60.0),
				string.Format((num5 % 24.0 == 0.0) ? "" : "{0}h", num5 % 24.0),
				string.Format((num6 % 7.0 == 0.0) ? "" : "{0}d", num6 % 7.0),
				$"{num7}w"
			});
		}
		if (showMiliseconds && num3 < 5)
		{
			arg = ((num2 != 1) ? "Miliseconds" : "Milisecond");
			arg2 = ((num3 != 1) ? "Seconds" : "Second");
			if (num3 != 0L || num2 != 0)
			{
				return string.Format("{0}{1}", string.Format((num3 == 0L) ? "" : "{0} {1}, ", num3, arg2), $"{num2} {arg}");
			}
			return string.Format("0 Seconds", Array.Empty<object>());
		}
		if (num3 < 60)
		{
			arg2 = ((num3 != 1) ? "Seconds" : "Second");
			return $"{num3} {arg2}";
		}
		if (num4 < 60.0)
		{
			arg2 = ((num3 % 60 != 1) ? "Seconds" : "Second");
			arg3 = ((num4 != 1.0) ? "Minutes" : "Minute");
			return string.Format("{1}{0}", new object[5]
			{
				string.Format((num3 % 60 == 0L) ? "" : " and {0} {1}", num3 % 60, arg2),
				$"{num4} {arg3}",
				num5,
				num6,
				num7
			});
		}
		if (num5 < 24.0)
		{
			arg2 = ((num3 % 60 != 1) ? "Seconds" : "Second");
			arg3 = ((num4 % 60.0 != 1.0) ? "Minutes" : "Minute");
			text = ((num5 != 1.0) ? "Hours" : "Hour");
			return string.Format("{2}{1}{0}", new object[5]
			{
				string.Format((num3 % 60 == 0L) ? "" : " and {0} {1}", num3 % 60, arg2),
				string.Format((num4 % 60.0 == 0.0) ? "" : ((num3 % 60 == 0L) ? " and {0} {1}" : ", {0} {1}"), num4 % 60.0, arg3),
				$"{num5} {text}",
				num6,
				num7
			});
		}
		if (num6 < 7.0)
		{
			arg2 = ((num3 % 60 != 1) ? "Seconds" : "Second");
			arg3 = ((num4 % 60.0 != 1.0) ? "Minutes" : "Minute");
			text = ((num5 % 24.0 != 1.0) ? "Hours" : "Hour");
			text2 = ((num6 % 7.0 != 1.0) ? "Days" : "Day");
			return string.Format("{3}{2}{1}{0}", new object[5]
			{
				string.Format((num3 % 60 == 0L) ? "" : " and {0} {1}", num3 % 60, arg2),
				string.Format((num4 % 60.0 == 0.0) ? "" : " and {0} {1}", num4 % 60.0, arg3),
				string.Format((num4 % 60.0 > 0.0) ? ", {0} {1}" : " and {0} {1}", num5 % 24.0, text),
				$"{num6 % 7.0} {text2}",
				num7
			});
		}
		arg2 = ((num3 % 60 != 1) ? "Seconds" : "Second");
		arg3 = ((num4 % 60.0 != 1.0) ? "Minutes" : "Minute");
		text = ((num5 % 24.0 != 1.0) ? "Hours" : "Hour");
		text2 = ((num6 % 7.0 != 1.0) ? "Days" : "Day");
		text3 = ((num7 != 1.0) ? "Weeks" : "Week");
		return string.Format("{4}{3}{2}{1}{0}", new object[5]
		{
			string.Format((num3 % 60 == 0L) ? "" : " and {0} {1}", num3 % 60, arg2),
			string.Format((num4 % 60.0 == 0.0) ? "" : ((num3 % 60 == 0L) ? " and {0} {1}" : ", {0} {1}"), num4 % 60.0, arg3),
			string.Format((num5 % 24.0 == 0.0) ? "" : ", {0} {1}", num5 % 24.0, text),
			string.Format((num6 % 7.0 == 0.0) ? "" : ", {0} {1}", num6 % 7.0, text2),
			$"{num7} {text3}"
		});
	}

	public static string FormatPlayer<T>(T v, string format = "[m]:[s].[ms]", string integerFormat = "00")
	{
		float num = (float)Convert.ChangeType(v, typeof(float));
		int num2 = (int)((num - (float)(int)num) * 10f);
		long num3 = (long)num % 60;
		double num4 = Math.Floor((double)num / 60.0);
		double num5 = Math.Floor(num4 / 60.0);
		double num6 = Math.Floor(num5 / 24.0);
		double num7 = Math.Floor(num6 / 7.0);
		format = format.Replace("[ms]", num2.ToString(integerFormat));
		format = format.Replace("[s]", num3.ToString(integerFormat));
		format = format.Replace("[m]", num4.ToString(integerFormat));
		format = format.Replace("[h]", num5.ToString(integerFormat));
		format = format.Replace("[d]", num6.ToString(integerFormat));
		format = format.Replace("[w]", num7.ToString(integerFormat));
		return format;
	}

	public static string FormatPlayer<T>(T v, bool showMiliseconds, string integerFormat = "00")
	{
		string text = "";
		float num = (float)Convert.ChangeType(v, typeof(float));
		int num2 = (int)((num - (float)(int)num) * 10f);
		long num3 = (long)num % 60;
		double num4 = Math.Floor((double)num / 60.0);
		double num5 = Math.Floor(num4 / 60.0);
		double num6 = Math.Floor(num5 / 24.0);
		double num7 = Math.Floor(num6 / 7.0);
		text += ((num7 > 1.0) ? (num7.ToString(integerFormat) + ":") : "");
		text += ((num6 % 7.0 > 1.0) ? (num6.ToString(integerFormat) + ":") : ((num7 > 0.0) ? (num6.ToString(integerFormat) + ":") : ""));
		text += ((num5 % 24.0 > 0.0) ? (num5.ToString(integerFormat) + ":") : ((num6 > 0.0 || num7 > 0.0) ? (num5.ToString(integerFormat) + ":") : ""));
		text = text + (num4 % 60.0).ToString(integerFormat) + ":";
		text += (num3 % 60).ToString(integerFormat);
		if (showMiliseconds)
		{
			text = text + "." + num2.ToString(integerFormat);
		}
		return text;
	}
}
