using System;
using Carbon.Core;
using Carbon.Extensions;

namespace Carbon.Components;

public static class HookMemory
{
	public enum Modes : byte
	{
		None,
		Estimate,
		Precise
	}

	public struct Scope
	{
		public Modes Mode;

		internal long heap;

		internal long nested;
	}

	[ThreadStatic]
	private static long _nested;

	public static Scope Begin()
	{
		Config.DebuggingConfig debugging = Community.Runtime.Config.Debugging;
		Scope result = default;
		if (!debugging.TrackHookMemory)
		{
			return result;
		}
		if (debugging.PreciseHookMemory && ThreadEx.IsOnMainThread() && MonoProfiler.BeginAllocationScope())
		{
			result.Mode = Modes.Precise;
			return result;
		}
		result.Mode = Modes.Estimate;
		result.nested = _nested;
		_nested = 0L;
		result.heap = GC.GetTotalMemory(forceFullCollection: false);
		return result;
	}

	public static long End(in Scope scope)
	{
		switch (scope.Mode)
		{
		case Modes.Precise:
			return MonoProfiler.EndAllocationScope();
		case Modes.Estimate:
		{
			long num = Math.Max(GC.GetTotalMemory(forceFullCollection: false) - scope.heap, 0L);
			long result = Math.Max(num - _nested, 0L);
			_nested = scope.nested + num;
			return result;
		}
		default:
			return 0L;
		}
	}
}
