using System;

namespace Oxide.Core;

public static class Random
{
	private static readonly System.Random random;

	static Random()
	{
		random = new System.Random();
	}

	public static int Range(int min, int max)
	{
		return random.Next(min, max);
	}

	public static int Range(int max)
	{
		return random.Next(max);
	}

	public static double Range(double min, double max)
	{
		return min + random.NextDouble() * (max - min);
	}

	public static float Range(float min, float max)
	{
		return (float)Range((double)min, (double)max);
	}
}
