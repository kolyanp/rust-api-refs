using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins;

public static class ExtensionMethods
{
	public static string Basename(this string text, string extension = null)
	{
		if (extension != null)
		{
			if (!extension.Equals("*.*"))
			{
				if (extension[0] == '*')
				{
					extension = extension.Substring(1);
				}
				return Regex.Match(text, "([^\\\\/]+)\\" + extension + "+$").Groups[1].Value;
			}
			Match match = Regex.Match(text, "([^\\\\/]+)\\.[^\\.]+$");
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
		}
		return Regex.Match(text, "[^\\\\/]+$").Groups[0].Value;
	}

	public static bool Contains<T>(this T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			if (val.Equals(value))
			{
				return true;
			}
		}
		return false;
	}

	public static string Dirname(this string text)
	{
		return Regex.Match(text, "(.+)[\\/][^\\/]+$").Groups[1].Value;
	}

	public static string Humanize(this string name)
	{
		return Regex.Replace(name, "(\\B[A-Z])", " $1");
	}

	public static bool IsSteamId(this string id)
	{
		if (ulong.TryParse(id, out var result))
		{
			return result > 76561197960265728L;
		}
		return false;
	}

	public static bool IsSteamId(this ulong id)
	{
		return id > 76561197960265728L;
	}

	public static string Plaintext(this string text)
	{
		return Formatter.ToPlaintext(text);
	}

	public static string QuoteSafe(this string text)
	{
		return "\"" + text.Replace("\"", "\\\"").TrimEnd(new char[1] { '\\' }) + "\"";
	}

	public static string Quote(this string text)
	{
		return text.QuoteSafe();
	}

	public static T Sample<T>(this T[] array)
	{
		return array[Random.Range(0, array.Length)];
	}

	public static string Sanitize(this string text)
	{
		return text.Replace("{", "{{").Replace("}", "}}");
	}

	public static string SentenceCase(this string text)
	{
		return new Regex("(^[a-z])|\\.\\s+(.)", RegexOptions.ExplicitCapture).Replace(text.ToLower(), (Match s) => s.Value.ToUpper());
	}

	public static string TitleCase(this string text)
	{
		return CultureInfo.InstalledUICulture.TextInfo.ToTitleCase(Enumerable.Contains(text, '_') ? text.Replace('_', ' ') : text);
	}

	public static string Titleize(this string text)
	{
		return text.TitleCase();
	}

	public static string ToSentence<T>(this IEnumerable<T> items)
	{
		IEnumerator<T> enumerator = items.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return string.Empty;
		}
		T current = enumerator.Current;
		T val;
		if (!enumerator.MoveNext())
		{
			ref T reference = ref current;
			val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
				if (val == null)
				{
					return null;
				}
			}
			return reference.ToString();
		}
		ref T reference2 = ref current;
		val = default(T);
		object value;
		if (val == null)
		{
			val = reference2;
			reference2 = ref val;
			if (val == null)
			{
				value = null;
				goto IL_008c;
			}
		}
		value = reference2.ToString();
		goto IL_008c;
		IL_008c:
		StringBuilder stringBuilder = new StringBuilder((string?)value);
		bool flag = true;
		while (flag)
		{
			T current2 = enumerator.Current;
			flag = enumerator.MoveNext();
			stringBuilder.Append(flag ? ", " : " and ");
			stringBuilder.Append(current2);
		}
		return stringBuilder.ToString();
	}

	public static string Truncate(this string text, int max)
	{
		if (text.Length > max)
		{
			return text.Substring(0, max) + " ...";
		}
		return text;
	}

	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
	{
		return new HashSet<T>(collection);
	}
}
