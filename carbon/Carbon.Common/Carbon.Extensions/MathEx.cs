using System;
using System.Globalization;

namespace Carbon.Extensions;

public static class MathEx
{
	public static int Clamp(this int value, int min, int max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
		return value;
	}

	public static float Clamp(this float value, float min, float max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}
		return value;
	}

	public static float Percentage(this float value, float total, float percent = 100f)
	{
		return (float)Math.Round((double)percent * (double)value) / total;
	}

	public static float Percentage(this int value, int total, float percent = 100f)
	{
		return (float)Math.Round((double)percent * (double)value) / (float)total;
	}

	public static float Percentage(this long value, long total, float percent = 100f)
	{
		return (float)Math.Round((double)percent * (double)value) / (float)total;
	}

	public static float Scale(this float oldValue, float oldMin, float oldMax, float newMin, float newMax)
	{
		float num = oldMax - oldMin;
		float num2 = newMax - newMin;
		return (oldValue - oldMin) * num2 / num + newMin;
	}

	public static int Scale(this int oldValue, int oldMin, int oldMax, int newMin, int newMax)
	{
		int num = oldMax - oldMin;
		int num2 = newMax - newMin;
		return (oldValue - oldMin) * num2 / num + newMin;
	}

	public static long Scale(this long oldValue, long oldMin, long oldMax, long newMin, long newMax)
	{
		long num = oldMax - oldMin;
		long num2 = newMax - newMin;
		return (oldValue - oldMin) * num2 / num + newMin;
	}

	public static ulong Scale(this ulong oldValue, ulong oldMin, ulong oldMax, ulong newMin, ulong newMax)
	{
		ulong num = oldMax - oldMin;
		ulong num2 = newMax - newMin;
		return (oldValue - oldMin) * num2 / num + newMin;
	}

	public static ulong Max(ulong a, ulong b)
	{
		if (a <= b)
		{
			return b;
		}
		return a;
	}

	public static ulong Min(ulong a, ulong b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	public static uint Max(uint a, uint b)
	{
		if (a <= b)
		{
			return b;
		}
		return a;
	}

	public static uint Min(uint a, uint b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	public static double Max(double a, double b)
	{
		if (!(a > b))
		{
			return b;
		}
		return a;
	}

	public static double Min(double a, double b)
	{
		if (!(a < b))
		{
			return b;
		}
		return a;
	}

	public static int RoundUpToNearest(this int value, int nearest)
	{
		if (value % nearest == 0)
		{
			return value;
		}
		return value / nearest * nearest + nearest;
	}

	public static string ToHex(this int value)
	{
		return value.ToString("X");
	}

	public static int FromHex(this string value)
	{
		return int.Parse(value, NumberStyles.HexNumber);
	}

	public static string ToBinary(this int value)
	{
		return Convert.ToString(value, 2);
	}

	public static int FromBinary(this string value)
	{
		return Convert.ToInt32(value, 2);
	}

	public static int RoundUpToNearestCount(this int number, double count)
	{
		return (int)(Math.Ceiling((double)number / count) * count);
	}

	public static double RoundUpToNearestCount(this double number, double count)
	{
		return Math.Ceiling(number / count) * count;
	}

	public static float RoundUpToNearestCount(this float number, double count)
	{
		return (float)(Math.Ceiling((double)number / count) * count);
	}
}
