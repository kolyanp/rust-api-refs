using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Carbon.Pooling;

public class HookStringPool
{
	public static Dictionary<string, uint> HookNamePoolString = new Dictionary<string, uint>();

	public static Dictionary<uint, string> HookNamePoolInt = new Dictionary<uint, string>();

	public static uint GetOrAdd(string name)
	{
		if (HookNamePoolString.TryGetValue(name, out var value))
		{
			return value;
		}
		value = ManifestHash(name);
		HookNamePoolString[name] = value;
		HookNamePoolInt[value] = name;
		return value;
	}

	public static string GetOrAdd(uint name)
	{
		if (!HookNamePoolInt.TryGetValue(name, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	public static uint Get(string name)
	{
		if (!HookNamePoolString.TryGetValue(name, out var value))
		{
			return 0u;
		}
		return value;
	}

	public static string Get(uint name)
	{
		if (!HookNamePoolInt.TryGetValue(name, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	private static uint ManifestHash(string str)
	{
		if (!string.IsNullOrEmpty(str))
		{
			return BitConverter.ToUInt32(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str)), 0);
		}
		return 0u;
	}
}
