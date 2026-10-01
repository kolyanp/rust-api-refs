using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public static class PerformanceMetrics
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__10_0;

		internal void _003CSetup_003Eb__10_0()
		{
			OnBeforeRender?.Invoke();
		}
	}

	private static PerformanceSamplePoint current;

	private static Action OnBeforeRender;

	public static PerformanceSamplePoint LastFrame { get; private set; }

	public static PerformanceSamplePoint PerformancePerSecond { get; set; }

	public static void Setup()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected Obj, but got Unknown
		UnityAction val = _003C_003Ec._003C_003E9__10_0;
		if (val == null)
		{
			UnityAction val2 = () =>
			{
				OnBeforeRender?.Invoke();
			};
			_003C_003Ec._003C_003E9__10_0 = val2;
			val = val2;
		}
		Application.onBeforeRender += val;
		AddStopwatch(PerformanceSample.PreCull, ref OnBeforeRender, ref CameraUpdateHook.RustCamera_PreRender);
		AddStopwatch(PerformanceSample.Update, ref PreUpdateHook.OnUpdate, ref PostUpdateHook.OnUpdate);
		AddStopwatch(PerformanceSample.LateUpdate, ref PreUpdateHook.OnLateUpdate, ref PostUpdateHook.OnLateUpdate);
		AddStopwatch(PerformanceSample.Render, ref CameraUpdateHook.PreRender, ref CameraUpdateHook.PostRender);
		AddStopwatch(PerformanceSample.FixedUpdate, ref PreUpdateHook.OnFixedUpdate, ref PostUpdateHook.OnFixedUpdate);
		AddStopwatch(PerformanceSample.PreLateUpdate, ref PostUpdateHook.OnUpdate, ref PreUpdateHook.OnLateUpdate);
		AddStopwatch(PerformanceSample.PhysicsUpdate, ref PostUpdateHook.OnFixedUpdate, ref PreUpdateHook.PostPhysicsUpdate);
		AddCPUTimeStopwatch();
	}

	private static void AddCPUTimeStopwatch()
	{
		Stopwatch watch = new Stopwatch();
		PreUpdateHook.StartOfFrame = (Action)Delegate.Combine(PreUpdateHook.StartOfFrame, (Action)(() =>
		{
			PerformancePerSecond = PerformancePerSecond.Add(current);
			LastFrame = current;
			current = default;
			watch.Restart();
			current.CpuUpdateCount++;
		}));
		PostUpdateHook.EndOfFrame = (Action)Delegate.Combine(PostUpdateHook.EndOfFrame, (Action)(() =>
		{
			current.TotalCPU += watch.Elapsed;
		}));
	}

	private static void AddStopwatch(PerformanceSample sample, ref Action pre, ref Action post)
	{
		Stopwatch watch = new Stopwatch();
		bool active = false;
		pre = (Action)Delegate.Combine(pre, (Action)(() =>
		{
			if (!active)
			{
				active = true;
				watch.Restart();
			}
		}));
		post = (Action)Delegate.Combine(post, (Action)(() =>
		{
			if (active)
			{
				active = false;
				watch.Stop();
				switch (sample)
				{
				case PerformanceSample.Update:
					current.UpdateCount++;
					current.Update += watch.Elapsed;
					break;
				case PerformanceSample.LateUpdate:
					current.LateUpdate += watch.Elapsed;
					break;
				case PerformanceSample.FixedUpdate:
					current.FixedUpdate += watch.Elapsed;
					current.FixedUpdateCount++;
					break;
				case PerformanceSample.PreCull:
					current.PreCull += watch.Elapsed;
					break;
				case PerformanceSample.Render:
					current.Render += watch.Elapsed;
					current.RenderCount++;
					break;
				case PerformanceSample.PhysicsUpdate:
					current.PhysicsUpdate += watch.Elapsed;
					break;
				case PerformanceSample.PreLateUpdate:
					current.PreLateUpdate += watch.Elapsed;
					break;
				case PerformanceSample.NetworkMessage:
				case PerformanceSample.TotalCPU:
					break;
				}
			}
		}));
	}
}
