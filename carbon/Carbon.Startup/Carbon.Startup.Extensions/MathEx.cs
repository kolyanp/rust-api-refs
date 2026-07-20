using System;

namespace Carbon.Startup.Extensions;

public static class MathEx
{
	public static float Percentage(this int value, int total, float percent = 100f)
	{
		return (float)Math.Round((double)percent * (double)value) / (float)total;
	}
}
