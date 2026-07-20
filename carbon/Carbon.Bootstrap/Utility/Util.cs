using System;
using System.Linq;
using System.Security.Cryptography;

namespace Utility;

public static class Util
{
	private static readonly Random _generator = new Random();

	public static string md5(byte[] raw)
	{
		if (raw == null || raw.Length == 0)
		{
			return null;
		}
		using MD5 mD = MD5.Create();
		byte[] array = mD.ComputeHash(raw);
		return BitConverter.ToString(array).Replace("-", "").ToLower();
	}

	public static string sha1(byte[] raw)
	{
		if (raw == null || raw.Length == 0)
		{
			return null;
		}
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] source = sHA1Managed.ComputeHash(raw);
		return string.Concat(source.Select((byte b) => b.ToString("x2"))).ToLower();
	}

	public static string GetRandomNumber(int digits)
	{
		if (digits <= 1)
		{
			return null;
		}
		int minValue = (int)Math.Pow(10.0, digits - 1);
		int maxValue = (int)Math.Pow(10.0, digits) - 1;
		return _generator.Next(minValue, maxValue).ToString();
	}
}
