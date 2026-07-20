using System.Collections.Generic;
using System.Linq;
using Facepunch;

namespace Carbon.Extensions;

public static class StringArrayEx
{
	public static string ToString(this IEnumerable<string> array, string separator, string lastSeparator = null)
	{
		if (string.IsNullOrEmpty(lastSeparator))
		{
			lastSeparator = separator;
		}
		int num = array.Count();
		switch (num)
		{
		case 0:
			return string.Empty;
		case 1:
			return array.First();
		default:
		{
			string text = string.Join(separator, array.Take(num - 1));
			return text + $"{lastSeparator}{array.ElementAt(num - 1)}";
		}
		}
	}

	public static string ToString(this IEnumerable<string> array, int startIndex, string separator = " ", bool throwError = false)
	{
		int num = array.Count();
		switch (num)
		{
		case 0:
			return string.Empty;
		case 1:
			return array.First();
		default:
			if (startIndex > num)
			{
				if (!throwError)
				{
					return null;
				}
				return $"ERROR! The start index ({startIndex}) is over the length of the arguments ({num}).";
			}
			return string.Join(separator, new object[3]
			{
				array,
				startIndex,
				num - startIndex
			});
		}
	}

	public static string ToString(this IEnumerable<string> array, int startIndex, string separator, string lastSeparator = null, bool throwError = false)
	{
		if (lastSeparator == null)
		{
			lastSeparator = separator;
		}
		int num = array.Count();
		switch (num)
		{
		case 0:
			return string.Empty;
		case 1:
			return array.First();
		default:
			if (startIndex > num)
			{
				if (!throwError)
				{
					return null;
				}
				return $"ERROR! The start index ({startIndex}) is over the length of the arguments ({num}).";
			}
			return string.Join(separator, new object[3]
			{
				array,
				startIndex,
				num - startIndex
			}) + $"{lastSeparator}{array.ElementAt(num - 1)}";
		}
	}

	public static string[] ToStringArray(this object[] array)
	{
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i]?.ToString();
		}
		return array2;
	}

	public static StringView[] ToStringViewArray(this object[] array)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		StringView[] array2 = (StringView[])(object)new StringView[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new StringView(array[i]?.ToString());
		}
		return array2;
	}

	public static string[] Split(this string text, int chunkSize, bool includeLeftovers = true)
	{
		IEnumerable<string> enumerable = from i in Enumerable.Range(0, text.Length / chunkSize)
			select text.Substring(i * chunkSize, chunkSize);
		string text2 = text.Replace(enumerable.ToString(""), "");
		List<string> list = Pool.Get<List<string>>();
		list.AddRange(enumerable);
		if (includeLeftovers && !string.IsNullOrEmpty(text2))
		{
			list.Add(text2);
		}
		string[] result = list.ToArray();
		Pool.FreeUnmanaged<string>(ref list);
		return result;
	}
}
