using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class NavStressStats
{
	public static bool enabled;

	public static int setDestinationCalls;

	public static int setDestinationFails;

	public static int corridorInvalidations;

	public static int replans;

	public static int cornerRefreshes;

	public static int pathResets;

	public static int destinationPatches;

	public static int pathQueries;

	public static int pathQueryFails;

	public static double pathQueryMs;

	private static readonly Stopwatch stopwatch = new Stopwatch();

	private static readonly Stopwatch driverStopwatch = new Stopwatch();

	private static readonly Stopwatch pathQueryStopwatch = new Stopwatch();

	private static readonly List<double> frameMs = new List<double>(32768);

	private static readonly List<double> driverMs = new List<double>(32768);

	private static long agentTicks;

	private static long allocBytes;

	private static long allocAtFrameStart;

	private static bool allocChecked;

	private static bool allocSupported;

	private static ProfilerRecorder allocRecorder;

	[Conditional("DEBUG")]
	public static void CountCornerRefresh()
	{
		cornerRefreshes++;
	}

	[Conditional("DEBUG")]
	public static void CountCorridorInvalidation()
	{
		corridorInvalidations++;
	}

	[Conditional("DEBUG")]
	public static void CountReplan()
	{
		replans++;
	}

	[Conditional("DEBUG")]
	public static void CountPathReset()
	{
		pathResets++;
	}

	[Conditional("DEBUG")]
	public static void CountDestinationPatch()
	{
		destinationPatches++;
	}

	[Conditional("DEBUG")]
	public static void PathQueryBegin()
	{
		if (enabled)
		{
			pathQueryStopwatch.Restart();
		}
	}

	[Conditional("DEBUG")]
	public static void PathQueryEnd(bool complete)
	{
		if (enabled)
		{
			pathQueryStopwatch.Stop();
			pathQueryMs += pathQueryStopwatch.Elapsed.TotalMilliseconds;
			pathQueries++;
			if (!complete)
			{
				pathQueryFails++;
			}
		}
	}

	[Conditional("DEBUG")]
	public static void FrameBegin()
	{
		if (enabled)
		{
			if (IsAllocationMeasurable())
			{
				allocAtFrameStart = allocRecorder.CurrentValue;
			}
			stopwatch.Restart();
		}
	}

	private static bool IsAllocationMeasurable()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (allocChecked)
		{
			return allocSupported;
		}
		allocChecked = true;
		allocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1, (ProfilerRecorderOptions)24);
		if (!allocRecorder.Valid)
		{
			return false;
		}
		long currentValue = allocRecorder.CurrentValue;
		byte[] array = new byte[4096];
		long currentValue2 = allocRecorder.CurrentValue;
		System.GC.KeepAlive(array);
		allocSupported = currentValue2 - currentValue >= array.Length;
		return allocSupported;
	}

	[Conditional("DEBUG")]
	public static void FrameEnd(int agentCount)
	{
		if (enabled)
		{
			stopwatch.Stop();
			if (frameMs.Count < 200000)
			{
				frameMs.Add(stopwatch.Elapsed.TotalMilliseconds);
				driverMs.Add(driverStopwatch.Elapsed.TotalMilliseconds);
			}
			driverStopwatch.Reset();
			agentTicks += agentCount;
			if (allocSupported)
			{
				allocBytes += allocRecorder.CurrentValue - allocAtFrameStart;
			}
		}
	}

	[Conditional("DEBUG")]
	public static void DriverCallBegin()
	{
		if (enabled)
		{
			driverStopwatch.Start();
		}
	}

	[Conditional("DEBUG")]
	public static void DriverCallEnd()
	{
		if (enabled)
		{
			driverStopwatch.Stop();
		}
	}

	public static void ResetSamples()
	{
		frameMs.Clear();
		driverMs.Clear();
		agentTicks = 0L;
		allocBytes = 0L;
		setDestinationCalls = 0;
		setDestinationFails = 0;
		corridorInvalidations = 0;
		replans = 0;
		cornerRefreshes = 0;
		pathResets = 0;
		destinationPatches = 0;
		pathQueries = 0;
		pathQueryFails = 0;
		pathQueryMs = 0.0;
	}

	public static double MeanMs()
	{
		if (frameMs.Count == 0)
		{
			return 0.0;
		}
		double num = 0.0;
		for (int i = 0; i < frameMs.Count; i++)
		{
			num += frameMs[i];
		}
		return num / (double)frameMs.Count;
	}

	public static double MeanDriverMs()
	{
		if (driverMs.Count == 0)
		{
			return 0.0;
		}
		double num = 0.0;
		for (int i = 0; i < driverMs.Count; i++)
		{
			num += driverMs[i];
		}
		return num / (double)driverMs.Count;
	}

	public static string Report(int agentCount, string label)
	{
		int count = frameMs.Count;
		if (count == 0)
		{
			return "[" + label + "] no samples";
		}
		List<double> list = new List<double>(frameMs);
		list.Sort();
		double num = MeanMs();
		double num2 = MeanDriverMs();
		double num3 = list[count / 2];
		double num4 = list[Mathf.Min(count - 1, Mathf.FloorToInt((float)count * 0.95f))];
		double num5 = list[count - 1];
		double num6 = ((agentTicks > 0) ? (num * 1000.0 * (double)count / (double)agentTicks) : 0.0);
		string text = (allocSupported ? $"{(double)allocBytes / (double)count:F0}B/frame" : "cannot measure");
		return string.Format("[{0}] {1} agents, {2} frames: tick {3:F1}us + driver {4:F1}us p50 {5:F1}us p95 {6:F1}us max {7:F1}us, {8:F2}us/agent, alloc {9}, setDest {10} ({11} fail, {12} patched), corridorInvalid {13}, replans {14}, cornerPulls {15}, pathResets {16}", new object[17]
		{
			label,
			agentCount,
			count,
			num * 1000.0,
			num2 * 1000.0,
			num3 * 1000.0,
			num4 * 1000.0,
			num5 * 1000.0,
			num6,
			text,
			setDestinationCalls,
			setDestinationFails,
			destinationPatches,
			corridorInvalidations,
			replans,
			cornerRefreshes,
			pathResets
		});
	}
}
