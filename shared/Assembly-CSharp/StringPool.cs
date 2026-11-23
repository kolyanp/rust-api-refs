using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StringPool
{
	public static Dictionary<uint, string> toString;

	public static Dictionary<string, uint> toNumber;

	private static Dictionary<uint, string> toShortName;

	public static bool initialized;

	public static uint closest;

	public static void Init()
	{
		if (initialized)
		{
			return;
		}
		toString = new Dictionary<uint, string>();
		toNumber = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		toShortName = new Dictionary<uint, string>();
		GameManifest gameManifest = FileSystem.Load<GameManifest>("Assets/manifest.asset", true);
		for (uint num = 0u; num < gameManifest.pooledStrings.Length; num++)
		{
			string str = gameManifest.pooledStrings[num].str;
			uint hash = gameManifest.pooledStrings[num].hash;
			if (toString.TryGetValue(hash, out var value))
			{
				if (str != value)
				{
					Debug.LogWarning((object)$"Hash collision: {hash} already exists in string pool. `{str}` and `{value}` have the same hash.");
				}
			}
			else
			{
				Add(hash, str);
			}
		}
		initialized = true;
		closest = Get("closest");
	}

	public static string Get(uint i)
	{
		if (i == 0)
		{
			return string.Empty;
		}
		Init();
		if (toString.TryGetValue(i, out var value))
		{
			return value;
		}
		Debug.LogWarning((object)("StringPool.GetString - no string for ID" + i));
		return "";
	}

	public static bool TryGet(uint i, out string str)
	{
		if (i == 0)
		{
			str = string.Empty;
			return false;
		}
		Init();
		return toString.TryGetValue(i, out str);
	}

	public static string GetShortName(uint i)
	{
		if (i == 0)
		{
			return string.Empty;
		}
		Init();
		if (toShortName.TryGetValue(i, out var value))
		{
			return value;
		}
		Debug.LogWarning((object)("StringPool.GetShortName - no short name for ID" + i));
		return "";
	}

	public static uint Get(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return 0u;
		}
		Init();
		if (toNumber.TryGetValue(str, out var value))
		{
			return value;
		}
		Debug.LogWarning((object)("StringPool.GetNumber - no number for string " + str));
		return 0u;
	}

	public static uint Add(string str)
	{
		uint value = 0u;
		if (!toNumber.TryGetValue(str, out value))
		{
			value = StringEx.ManifestHash(str);
			Add(value, str);
		}
		return value;
	}

	private static void Add(uint hash, string str)
	{
		toString.Add(hash, str);
		toNumber.Add(str, hash);
		toShortName.Add(hash, Path.GetFileNameWithoutExtension(str));
	}
}
