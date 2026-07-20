using System;

namespace Carbon.Extensions;

public class RandomEx
{
	public static Random rand = new Random();

	public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

	private static readonly char[] uiId = new char[4];

	public static string GetRandomString(int size)
	{
		char[] array = ((size == 4) ? uiId : new char[size]);
		for (int i = 0; i < size; i++)
		{
			array[i] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[rand.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".Length)];
		}
		string result = new string(array);
		Array.Clear(array, 0, array.Length);
		return result;
	}

	public static string GetRandomString(int size, string chars)
	{
		char[] array = new char[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = chars[rand.Next(chars.Length)];
		}
		string result = new string(array);
		Array.Clear(array, 0, array.Length);
		return result;
	}

	public static string GetRandomString(int size, int seed)
	{
		Random random = new Random(seed);
		char[] array = new char[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[random.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".Length)];
		}
		string result = new string(array);
		Array.Clear(array, 0, array.Length);
		return result;
	}

	public static string GetRandomString(int size, string chars, int seed)
	{
		Random random = new Random(seed);
		char[] array = new char[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = chars[random.Next(chars.Length)];
		}
		string result = new string(array);
		Array.Clear(array, 0, array.Length);
		return result;
	}

	public static int GetRandomInteger(int min, int max)
	{
		return rand.Next(min, max);
	}

	public static int GetRandomInteger()
	{
		return rand.Next(int.MinValue, int.MaxValue);
	}

	public static int GetRandomInteger(int seed)
	{
		Random random = new Random(seed);
		return random.Next(int.MinValue, int.MaxValue);
	}

	public static int GetRandomInteger(int min, int max, int seed)
	{
		Random random = new Random(seed);
		return random.Next(min, max);
	}

	public static float GetRandomFloat(float min, float max)
	{
		return (float)rand.NextDouble() * (max - min) + min;
	}

	public static float GetRandomFloat()
	{
		return (float)rand.NextDouble();
	}

	public static float GetRandomFloat(int seed)
	{
		Random random = new Random(seed);
		return (float)random.NextDouble() * float.PositiveInfinity + float.MinValue;
	}

	public static float GetRandomFloat(float min, float max, int seed)
	{
		Random random = new Random(seed);
		return (float)random.NextDouble() * (max - min) + min;
	}

	public static string GetShuffledString(string str)
	{
		char[] array = str.ToCharArray();
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int num2 = rand.Next(num + 1);
			char c = array[num2];
			array[num2] = array[num];
			array[num] = c;
		}
		return new string(array);
	}

	public static string GetShuffledString(string str, int seed)
	{
		Random random = new Random(seed);
		char[] array = str.ToCharArray();
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int num2 = random.Next(num + 1);
			char c = array[num2];
			array[num2] = array[num];
			array[num] = c;
		}
		return new string(array);
	}
}
