using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Components;

namespace Carbon.Extensions;

public static class StringEx
{
	internal static readonly string[] _unitsMapCamel = new string[20]
	{
		"Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
		"Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
	};

	internal static readonly string[] _unitsMapNonCamel = new string[20]
	{
		"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
		"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
	};

	internal static readonly string[] _tensMapCamel = new string[10] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

	internal static readonly string[] _tensMapNonCamel = new string[10] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

	private const string Dot = ".";

	private const string Dash = "-";

	public static Dictionary<char, string> MorseMapping { get; set; } = new Dictionary<char, string>
	{
		{
			'a',
			"." + "-"
		},
		{
			'b',
			"-" + "." + "." + "."
		},
		{
			'c',
			"-" + "." + "-" + "."
		},
		{
			'd',
			"-" + "." + "."
		},
		{
			'e',
			".".ToString()
		},
		{
			'f',
			"." + "." + "-" + "."
		},
		{
			'g',
			"-" + "-" + "."
		},
		{
			'h',
			"." + "." + "." + "."
		},
		{
			'i',
			"." + "."
		},
		{
			'j',
			"." + "-" + "-" + "-"
		},
		{
			'k',
			"-" + "." + "-"
		},
		{
			'l',
			"." + "-" + "." + "."
		},
		{
			'm',
			"-" + "-"
		},
		{
			'n',
			"-" + "."
		},
		{
			'o',
			"-" + "-" + "-"
		},
		{
			'p',
			"." + "-" + "-" + "."
		},
		{
			'q',
			"-" + "-" + "." + "-"
		},
		{
			'r',
			"." + "-" + "."
		},
		{
			's',
			"." + "." + "."
		},
		{
			't',
			string.Concat(new string[1] { "-" })
		},
		{
			'u',
			"." + "." + "-"
		},
		{
			'v',
			"." + "." + "." + "-"
		},
		{
			'w',
			"." + "-" + "-"
		},
		{
			'x',
			"-" + "." + "." + "-"
		},
		{
			'y',
			"-" + "." + "-" + "-"
		},
		{
			'z',
			"-" + "-" + "." + "."
		},
		{
			'0',
			"-" + "-" + "-" + "-" + "-"
		},
		{
			'1',
			"." + "-" + "-" + "-" + "-"
		},
		{
			'2',
			"." + "." + "-" + "-" + "-"
		},
		{
			'3',
			"." + "." + "." + "-" + "-"
		},
		{
			'4',
			"." + "." + "." + "." + "-"
		},
		{
			'5',
			"." + "." + "." + "." + "."
		},
		{
			'6',
			"-" + "." + "." + "." + "."
		},
		{
			'7',
			"-" + "-" + "." + "." + "."
		},
		{
			'8',
			"-" + "-" + "-" + "." + "."
		},
		{
			'9',
			"-" + "-" + "-" + "-" + "."
		},
		{
			'?',
			"." + "." + "-" + "-" + "." + "."
		},
		{
			'.',
			"." + "-" + "." + "-" + "." + "-"
		},
		{
			',',
			"-" + "-" + "." + "." + "-" + "-"
		},
		{
			'\'',
			"." + "-" + "-" + "-" + "-" + "."
		},
		{
			'!',
			"-" + "." + "-" + "." + "-" + "-"
		},
		{
			'/',
			"-" + "." + "." + "-" + "."
		},
		{
			'(',
			"-" + "." + "-" + "-" + "."
		},
		{
			')',
			"-" + "." + "-" + "-" + "." + "-"
		},
		{
			'&',
			"." + "-" + "." + "." + "."
		},
		{
			':',
			"-" + "-" + "-" + "." + "." + "."
		},
		{
			';',
			"-" + "." + "-" + "." + "-" + "."
		},
		{
			'=',
			"-" + "." + "." + "." + "-"
		},
		{
			'+',
			"." + "-" + "." + "-" + "."
		},
		{
			'-',
			"-" + "." + "." + "." + "." + "-"
		},
		{
			'_',
			"." + "." + "-" + "-" + "." + "-"
		},
		{
			'"',
			"." + "-" + "." + "." + "-" + "."
		},
		{
			'$',
			"." + "." + "." + "-" + "." + "." + "-"
		},
		{
			'@',
			"." + "-" + "-" + "." + "-" + "."
		}
	};

	public static Dictionary<char, string> L33tMapping { get; set; } = new Dictionary<char, string>
	{
		{ 'a', "4" },
		{ 'b', "13" },
		{ 'c', "(" },
		{ 'd', "[)" },
		{ 'e', "3" },
		{ 'f', "|=" },
		{ 'g', "6" },
		{ 'h', "|-|" },
		{ 'i', "|" },
		{ 'j', ".]" },
		{ 'k', "|<" },
		{ 'l', "1" },
		{ 'm', "|Y|" },
		{ 'n', "/\\/" },
		{ 'o', "0" },
		{ 'p', "|>" },
		{ 'q', "0," },
		{ 'r', "|2" },
		{ 's', "5" },
		{ 't', "7" },
		{ 'u', "[_]" },
		{ 'v', "\\/" },
		{ 'w', "\\v/" },
		{ 'x', "}{" },
		{ 'y', "'/" },
		{ 'z', "2" }
	};

	public static float ToFloat(this string value, float @default = 0f)
	{
		return (float)value.ToDecimal((decimal)@default);
	}

	public static int ToInt(this string value, int @default = 0)
	{
		decimal num = value.ToDecimal(@default);
		if (num <= -2147483648m)
		{
			return int.MinValue;
		}
		if (!(num >= 2147483647m))
		{
			return (int)num;
		}
		return int.MaxValue;
	}

	public static uint ToUint(this string value, uint @default = 0u)
	{
		if (!uint.TryParse(value, out var result))
		{
			return @default;
		}
		return result;
	}

	public static bool ToBool(this string value, bool @default = false)
	{
		if (string.IsNullOrEmpty(value) || value == null)
		{
			return @default;
		}
		if (value == "1")
		{
			return true;
		}
		value = value.Trim().ToLower();
		switch (value)
		{
		default:
			return value == "1";
		case "true":
		case "t":
		case "yes":
		case "y":
			return true;
		}
	}

	public static decimal ToDecimal(this string value, decimal @default = 0m)
	{
		if (!decimal.TryParse(value, out var result))
		{
			return @default;
		}
		return result;
	}

	public static long ToLong(this string value, long @default = 0L)
	{
		if (!long.TryParse(value, out var result))
		{
			return @default;
		}
		return result;
	}

	public static ulong ToUlong(this string value, ulong @default = 0uL)
	{
		if (!ulong.TryParse(value, out var result))
		{
			return @default;
		}
		return result;
	}

	public static byte[] ToBytes(this string value)
	{
		return Convert.FromBase64String(value);
	}

	public static string ToCamelCase(this string str)
	{
		string[] array = str.Split(' ');
		string result = array.Aggregate("", (string current, string split) => current + split[0].ToString().ToUpper() + split.Substring(1) + " ");
		if (array.Length == 0)
		{
			result = $"{char.ToUpper(str[0])}{str.Substring(1, str.Length)}";
		}
		Array.Clear(array, 0, array.Length);
		return result;
	}

	public static bool IsValid(this string value, string validCharacters)
	{
		foreach (char value2 in value)
		{
			if (!validCharacters.Contains(value2))
			{
				return false;
			}
		}
		return true;
	}

	public static string[] IsValidComplex(this string value, string validCharacters)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < value.Length; i++)
		{
			char value2 = value[i];
			if (!validCharacters.Contains(value2))
			{
				list.Add(value2.ToString());
			}
		}
		return list.ToArray();
	}

	public static string Truncate(this string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (value.Length > maxLength)
		{
			return value.Substring(0, maxLength);
		}
		return value;
	}

	public static string Truncate(this string value, int maxLength, string elipsis, bool countElipsisLength = true)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (countElipsisLength)
		{
			if (value.Length > maxLength)
			{
				return value.Substring(0, maxLength - elipsis.Length) + elipsis;
			}
			return value;
		}
		if (value.Length > maxLength)
		{
			return value.Substring(0, maxLength) + elipsis;
		}
		return value;
	}

	public static string Plural(this int value, string singularString, string pluralString)
	{
		if (value != 1)
		{
			return pluralString;
		}
		return singularString;
	}

	public static string Plural(this uint value, string singularString, string pluralString)
	{
		if (value != 1)
		{
			return pluralString;
		}
		return singularString;
	}

	public static string Plural(this long value, string singularString, string pluralString)
	{
		if (value != 1)
		{
			return pluralString;
		}
		return singularString;
	}

	public static string Plural(this ulong value, string singularString, string pluralString)
	{
		if (value != 1)
		{
			return pluralString;
		}
		return singularString;
	}

	public static string ToNumbered(this int number, string separatingString = "-", bool camelCase = true)
	{
		if (number == 0)
		{
			if (!camelCase)
			{
				return "zero";
			}
			return "Zero";
		}
		if (number < 0)
		{
			return (camelCase ? "Minus" : "minus ") + Math.Abs(number).ToNumbered(separatingString);
		}
		string text = "";
		if (number / 1000000 > 0)
		{
			text = text + (number / 1000000).ToNumbered(separatingString) + (camelCase ? " Million" : " million ");
			number %= 1000000;
		}
		if (number / 1000 > 0)
		{
			text = text + (number / 1000).ToNumbered(separatingString) + (camelCase ? " Thousand" : " thousand ");
			number %= 1000;
		}
		if (number / 100 > 0)
		{
			text = text + (number / 100).ToNumbered(separatingString) + (camelCase ? " Hundred" : " hundred ");
			number %= 100;
		}
		if (number > 0)
		{
			if (text != "")
			{
				text += " and ";
			}
			string[] array = (camelCase ? _unitsMapCamel : _unitsMapNonCamel);
			string[] array2 = (camelCase ? _tensMapCamel : _tensMapNonCamel);
			if (number < 20)
			{
				text += array[number];
			}
			else
			{
				text += array2[number / 10];
				if (number % 10 > 0)
				{
					text = text + separatingString + array[number % 10];
				}
			}
		}
		return text;
	}

	public static string ToMorse(string value, string spacing = "/")
	{
		using StringBody stringBody = default(StringBody);
		string text = value.ToLower();
		foreach (char c in text)
		{
			if (MorseMapping.ContainsKey(c))
			{
				stringBody.Add(MorseMapping[c] + " ");
			}
			else if (c == ' ')
			{
				stringBody.Add(spacing + " ");
			}
			else
			{
				stringBody.Add($"{c} ");
			}
		}
		return stringBody.ToString();
	}

	public static string FromMorse(string value, string spacing = "/")
	{
		string[] array = value.Split(' ');
		string text = "";
		string[] array2 = array;
		foreach (string split in array2)
		{
			string text2 = split.Trim();
			text = ((!(text2 == spacing)) ? (text + MorseMapping.FirstOrDefault((KeyValuePair<char, string> x) => x.Value == split).Key) : (text + " "));
		}
		Array.Clear(array, 0, array.Length);
		return text;
	}

	public static string ToL33t(string value, string spacing = " ")
	{
		using StringBody stringBody = default(StringBody);
		foreach (char c in value)
		{
			char c2 = char.ToLower(c);
			string text = (char.IsUpper(c) ? "^" : "");
			if (L33tMapping.ContainsKey(c2))
			{
				stringBody.Add(text + L33tMapping[c2] + " ");
			}
			else if (c2 == ' ')
			{
				stringBody.Add(spacing ?? "");
			}
			else
			{
				stringBody.Add($"{text}{c2} ");
			}
		}
		return stringBody.ToString();
	}

	public static string FromL33t(string value, string spacing = " ")
	{
		string[] array = value.Split(' ');
		string text = "";
		string[] array2 = array;
		foreach (string split in array2)
		{
			string text2 = split.Trim();
			if (split.Contains(spacing))
			{
				text += " ";
			}
			else if (L33tMapping.ContainsValue(split.Replace("^", "")))
			{
				KeyValuePair<char, string> keyValuePair = L33tMapping.FirstOrDefault((KeyValuePair<char, string> x) => x.Value == split.Replace("^", ""));
				text += string.Format("{0} ", (!split.Contains("^")) ? keyValuePair.Key : char.ToUpper(keyValuePair.Key));
			}
			else
			{
				text = text + text2 + " ";
			}
		}
		Array.Clear(array, 0, array.Length);
		return text;
	}

	public static string SpacedString(this string value, int spaces, bool trimEnd = true)
	{
		if (spaces == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		for (int i = 0; i < value.Length; i++)
		{
			text += value[i];
			for (int j = 0; j < spaces; j++)
			{
				text += " ";
			}
		}
		if (!trimEnd)
		{
			return text;
		}
		return text.TrimEnd();
	}

	public static IEnumerable<string> SplitEnumerable(this string input, char separator)
	{
		if (string.IsNullOrEmpty(input))
		{
			yield break;
		}
		int num = 0;
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i] == separator)
			{
				int num2 = num;
				yield return input.Substring(num2, i - num2);
				num = i + 1;
			}
		}
		if (num < input.Length)
		{
			yield return input.Substring(num);
		}
	}
}
