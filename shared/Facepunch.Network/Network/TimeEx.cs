using System;
using System.Diagnostics;

namespace Network;

public static class TimeEx
{
	private static Stopwatch stopwatch = Stopwatch.StartNew();

	public static double realtimeSinceStartup => stopwatch.Elapsed.TotalSeconds;

	public static double currentTimestamp => (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
}
