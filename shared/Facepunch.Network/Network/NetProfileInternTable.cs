using System.Collections.Generic;

namespace Network;

public class NetProfileInternTable
{
	public const int MaxEntries = 4096;

	public const int MaxLength = 64;

	private readonly Dictionary<string, ushort> lookup = new Dictionary<string, ushort>();

	private readonly List<string> strings = new List<string> { string.Empty };

	private readonly object sync = new object();

	public ushort Intern(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return 0;
		}
		if (value.Length > 64)
		{
			value = value.Substring(0, 64);
		}
		lock (sync)
		{
			if (lookup.TryGetValue(value, out var value2))
			{
				return value2;
			}
			if (strings.Count >= 4096)
			{
				return 0;
			}
			ushort num = (ushort)strings.Count;
			strings.Add(value);
			lookup.Add(value, num);
			return num;
		}
	}

	public string Get(ushort id)
	{
		lock (sync)
		{
			return (id < strings.Count) ? strings[id] : string.Empty;
		}
	}

	public string[] ToArray()
	{
		lock (sync)
		{
			return strings.ToArray();
		}
	}
}
