using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Carbon;
using Carbon.Extensions;
using Facepunch;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Core.Libraries;

public class Timer : Library
{
	public class TimerInstance : IDisposable
	{
		internal double ExpiresAtDouble;

		internal double DueAt;

		internal int HeapIndex = -1;

		internal int Generation;

		internal int CollectedGeneration;

		public Plugin Plugin { get; set; }

		public Plugin Owner => Plugin;

		internal Timer OwnerTimers { get; set; }

		public Action Activity { get; set; }

		public Action Callback { get; set; }

		public Plugin.Persistence Persistence { get; set; }

		public int Repetitions { get; set; }

		public float Delay { get; set; }

		public float ExpiresAt
		{
			get
			{
				return (float)ExpiresAtDouble;
			}
			set
			{
				ExpiresAtDouble = value;
			}
		}

		public bool Repeating { get; set; }

		public int TimesTriggered { get; set; }

		public bool Destroyed { get; set; }

		public bool Scheduled => HeapIndex >= 0;

		public TimerInstance()
		{
		}

		public TimerInstance(Plugin.Persistence persistence, Action activity, Plugin plugin = null)
		{
			Persistence = persistence;
			Activity = activity;
			Plugin = plugin;
		}

		public void Reset(float delay = -1f, int repetitions = 1)
		{
			if ((Object)(object)Persistence == (Object)null)
			{
				Logger.Warn("Cannot restart a timer for '" + (Plugin?.ToPrettyString() ?? "unknown plugin") + "' because persistence is null.");
				return;
			}
			lock (SchedulerLock)
			{
				TimesTriggered = 0;
				Repetitions = repetitions;
				Repeating = repetitions != 1;
				if (delay < 0f)
				{
					delay = Delay;
				}
				else
				{
					Delay = delay;
				}
				Unschedule(this);
				Generation++;
				Destroyed = false;
				Callback = Activity;
				OwnerTimers?.TrackTimer(this);
				ScheduleIn(this, Repeating ? NormalizeRepeatDelay(delay) : delay);
			}
		}

		public bool Destroy()
		{
			lock (SchedulerLock)
			{
				bool destroyed = Destroyed;
				Destroyed = true;
				Generation++;
				Unschedule(this);
				OwnerTimers?.UntrackTimer(this);
				Callback = null;
				return !destroyed;
			}
		}

		public void DestroyToPool()
		{
			Destroy();
		}

		public void Dispose()
		{
			Destroy();
		}
	}

	private struct ScheduledEntry
	{
		public double At;

		public long Sequence;

		public TimerInstance Instance;
	}

	internal readonly HashSet<TimerInstance> _timers = new HashSet<TimerInstance>();

	private static readonly object SchedulerLock;

	private static ScheduledEntry[] Heap;

	private static int HeapCount;

	private static long HeapSequence;

	private const int InitialHeapCapacity = 1024;

	private const int MaxTimersPerFrame = 8192;

	private const int LivenessChecksPerFrame = 50;

	private const float MinimumRepeatDelay = 0.001f;

	private static bool ClockPrimed;

	private static bool ProcessingTimers;

	private static int LivenessIndex;

	private static readonly double TimestampToSeconds;

	private static double ClockOffset;

	public Plugin Plugin { get; }

	public Plugin.Persistence Persistence => Plugin.persistence;

	internal static double CurrentTime => (double)Stopwatch.GetTimestamp() * TimestampToSeconds + Volatile.Read(in ClockOffset);

	public Timer()
	{
	}

	public Timer(Plugin plugin)
	{
		Plugin = plugin;
	}

	public bool IsValid()
	{
		if (Plugin != null)
		{
			return (Object)(object)Plugin.persistence != (Object)null;
		}
		return false;
	}

	public void Clear()
	{
		DestroyAll();
	}

	internal void TrackTimer(TimerInstance timer)
	{
		timer.OwnerTimers = this;
		lock (SchedulerLock)
		{
			_timers.Add(timer);
		}
	}

	internal void UntrackTimer(TimerInstance timer)
	{
		if (timer.OwnerTimers != this)
		{
			return;
		}
		lock (SchedulerLock)
		{
			_timers.Remove(timer);
		}
	}

	public TimerInstance In(float time, Action action, Plugin plugin = null)
	{
		if (!IsValid())
		{
			return null;
		}
		TimerInstance timerInstance = new TimerInstance(Persistence, action, plugin ?? Plugin);
		timerInstance.Delay = time;
		timerInstance.Repetitions = 1;
		timerInstance.Callback = action;
		TrackTimer(timerInstance);
		ScheduleIn(timerInstance, time);
		return timerInstance;
	}

	public TimerInstance Once(float time, Action action, Plugin plugin = null)
	{
		return In(time, action, plugin);
	}

	public TimerInstance Every(float time, Action action, Plugin plugin = null)
	{
		if (!IsValid())
		{
			return null;
		}
		TimerInstance timerInstance = new TimerInstance(Persistence, action, plugin ?? Plugin);
		timerInstance.Delay = time;
		timerInstance.Repetitions = 0;
		timerInstance.Repeating = true;
		timerInstance.Callback = action;
		TrackTimer(timerInstance);
		ScheduleIn(timerInstance, NormalizeRepeatDelay(time));
		return timerInstance;
	}

	public TimerInstance Repeat(float time, int times, Action action, Plugin plugin = null)
	{
		if (!IsValid())
		{
			return null;
		}
		TimerInstance timerInstance = new TimerInstance(Persistence, action, plugin ?? Plugin);
		timerInstance.Delay = time;
		timerInstance.Repetitions = times;
		timerInstance.Repeating = times != 1;
		timerInstance.Callback = action;
		TrackTimer(timerInstance);
		ScheduleIn(timerInstance, timerInstance.Repeating ? NormalizeRepeatDelay(time) : time);
		return timerInstance;
	}

	public void Destroy(ref TimerInstance timer)
	{
		if (timer != null)
		{
			timer.Destroy();
		}
		timer = null;
	}

	public void DestroyAll()
	{
		List<TimerInstance> list = Pool.Get<List<TimerInstance>>();
		try
		{
			lock (SchedulerLock)
			{
				if (_timers.Count == 0)
				{
					return;
				}
				list.AddRange(_timers);
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Destroy();
			}
		}
		finally
		{
			Pool.FreeUnmanaged<TimerInstance>(ref list);
		}
	}

	static Timer()
	{
		SchedulerLock = new object();
		Heap = new ScheduledEntry[1024];
		TimestampToSeconds = 1.0 / (double)Stopwatch.Frequency;
		ClockOffset = (double)(-Stopwatch.GetTimestamp()) * TimestampToSeconds;
		try
		{
			if (ThreadEx.IsOnMainThread())
			{
				PrimeClock();
			}
		}
		catch
		{
		}
	}

	internal static void PrimeClock()
	{
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		long timestamp = Stopwatch.GetTimestamp();
		lock (SchedulerLock)
		{
			UpdateClock(realtimeSinceStartupAsDouble, timestamp);
		}
	}

	private static void UpdateClock(double realtime, long timestamp)
	{
		double num = realtime - (double)timestamp * TimestampToSeconds;
		if (!ClockPrimed)
		{
			ClockPrimed = true;
			double num2 = num - ClockOffset;
			for (int i = 0; i < HeapCount; i++)
			{
				Heap[i].At += num2;
				Heap[i].Instance.ExpiresAtDouble += num2;
			}
		}
		Volatile.Write(ref ClockOffset, num);
	}

	internal static float NormalizeRepeatDelay(float delay)
	{
		if (!(delay > 0.001f))
		{
			return 0.001f;
		}
		return delay;
	}

	internal static void ScheduleIn(TimerInstance timer, float delay)
	{
		lock (SchedulerLock)
		{
			Schedule(timer, CurrentTime + (double)delay);
		}
	}

	internal static void Schedule(TimerInstance timer, double at)
	{
		if (!timer.Destroyed)
		{
			if (double.IsNaN(at))
			{
				at = double.NegativeInfinity;
			}
			if (timer.HeapIndex >= 0)
			{
				RemoveAt(timer.HeapIndex);
			}
			timer.ExpiresAtDouble = at;
			Push(new ScheduledEntry
			{
				At = at,
				Sequence = ++HeapSequence,
				Instance = timer
			});
		}
	}

	internal static void Unschedule(TimerInstance timer)
	{
		if (timer.HeapIndex >= 0)
		{
			RemoveAt(timer.HeapIndex);
			timer.HeapIndex = -1;
		}
	}

	internal static void ProcessTimers(int maxTimers = 8192)
	{
		if (ProcessingTimers)
		{
			return;
		}
		List<TimerInstance> list = null;
		ProcessingTimers = true;
		try
		{
			double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
			long timestamp = Stopwatch.GetTimestamp();
			bool flag;
			lock (SchedulerLock)
			{
				UpdateClock(realtimeSinceStartupAsDouble, timestamp);
				flag = HasDueTimers(realtimeSinceStartupAsDouble);
			}
			PurgeDeadTimers();
			if (flag)
			{
				list = Pool.Get<List<TimerInstance>>();
				CollectDueTimers(list, realtimeSinceStartupAsDouble, maxTimers);
				FireTimers(list, realtimeSinceStartupAsDouble);
			}
		}
		finally
		{
			ProcessingTimers = false;
			if (list != null)
			{
				Pool.FreeUnmanaged<TimerInstance>(ref list);
			}
		}
	}

	private static void PurgeDeadTimers()
	{
		List<TimerInstance> list = null;
		lock (SchedulerLock)
		{
			if (HeapCount == 0)
			{
				LivenessIndex = 0;
				return;
			}
			if (LivenessIndex >= HeapCount)
			{
				LivenessIndex = 0;
			}
			int num = Math.Min(LivenessIndex + 50, HeapCount);
			while (LivenessIndex < num)
			{
				TimerInstance instance = Heap[LivenessIndex].Instance;
				if (instance.Destroyed || (Object)(object)instance.Persistence == (Object)null || instance.Callback == null)
				{
					if (list == null)
					{
						list = Pool.Get<List<TimerInstance>>();
					}
					list.Add(instance);
				}
				LivenessIndex++;
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Destroy();
			}
			Pool.FreeUnmanaged<TimerInstance>(ref list);
		}
	}

	private static bool HasDueTimers(double now)
	{
		if (HeapCount > 0)
		{
			return Heap[0].At <= now;
		}
		return false;
	}

	private static void CollectDueTimers(List<TimerInstance> timers, double now, int maxTimers)
	{
		lock (SchedulerLock)
		{
			while (timers.Count < maxTimers && HasDueTimers(now))
			{
				TimerInstance instance = Heap[0].Instance;
				instance.DueAt = Heap[0].At;
				RemoveAt(0);
				instance.HeapIndex = -1;
				if (instance.Destroyed || (Object)(object)instance.Persistence == (Object)null || instance.Callback == null)
				{
					instance.Destroyed = true;
					instance.Callback = null;
					instance.OwnerTimers?.UntrackTimer(instance);
				}
				else
				{
					instance.CollectedGeneration = instance.Generation;
					timers.Add(instance);
				}
			}
		}
	}

	private static void FireTimers(List<TimerInstance> timers, double now)
	{
		for (int i = 0; i < timers.Count; i++)
		{
			FireTimer(timers[i], now);
		}
	}

	private static void FireTimer(TimerInstance timer, double now)
	{
		int collectedGeneration = timer.CollectedGeneration;
		try
		{
			FireCollectedTimer(timer, collectedGeneration, now);
		}
		catch (Exception ex)
		{
			lock (SchedulerLock)
			{
				if (!timer.Destroyed && timer.Generation == collectedGeneration)
				{
					timer.Destroy();
				}
			}
			try
			{
				Logger.Error(string.Format("Failed processing a timer of {0}s in '{1}'", timer.Delay, timer.Plugin?.ToPrettyString() ?? "unknown plugin"), ex);
			}
			catch
			{
			}
		}
	}

	private static void FireCollectedTimer(TimerInstance timer, int generation, double now)
	{
		if (timer.Destroyed || timer.Generation != generation)
		{
			return;
		}
		if ((Object)(object)timer.Persistence == (Object)null)
		{
			timer.Destroy();
			return;
		}
		try
		{
			timer.Activity?.Invoke();
		}
		catch (Exception ex)
		{
			Logger.Error(string.Format("Timer of {0}s has failed in '{1}' [callback]", timer.Delay, timer.Plugin?.ToPrettyString() ?? "unknown plugin"), ex);
			timer.Destroy();
		}
		lock (SchedulerLock)
		{
			if (!timer.Destroyed && timer.Generation == generation)
			{
				timer.TimesTriggered++;
				if (ShouldRequeue(timer))
				{
					double num = NormalizeRepeatDelay(timer.Delay);
					double at = ((now <= timer.DueAt) ? (timer.DueAt + num) : (now + num - (now - timer.DueAt) % num));
					Schedule(timer, at);
				}
				else
				{
					timer.Destroy();
				}
			}
		}
	}

	private static bool ShouldRequeue(TimerInstance timer)
	{
		if (!timer.Repeating || timer.Destroyed || (Object)(object)timer.Persistence == (Object)null)
		{
			return false;
		}
		if (timer.Repetitions > 0)
		{
			return timer.TimesTriggered < timer.Repetitions;
		}
		return true;
	}

	private static void Push(ScheduledEntry entry)
	{
		if (HeapCount == Heap.Length)
		{
			Array.Resize(ref Heap, Heap.Length << 1);
		}
		Heap[HeapCount] = entry;
		SiftUp(HeapCount);
		HeapCount++;
	}

	private static void RemoveAt(int index)
	{
		HeapCount--;
		if (index == HeapCount)
		{
			Heap[index] = default;
			return;
		}
		Heap[index] = Heap[HeapCount];
		Heap[HeapCount] = default;
		TimerInstance instance = Heap[index].Instance;
		instance.HeapIndex = index;
		SiftDown(index);
		if (instance.HeapIndex == index)
		{
			SiftUp(index);
		}
	}

	private static void SiftUp(int index)
	{
		ScheduledEntry a = Heap[index];
		while (index > 0)
		{
			int num = index - 1 >> 1;
			if (!IsBefore(in a, in Heap[num]))
			{
				break;
			}
			Heap[index] = Heap[num];
			Heap[index].Instance.HeapIndex = index;
			index = num;
		}
		Heap[index] = a;
		a.Instance.HeapIndex = index;
	}

	private static void SiftDown(int index)
	{
		ScheduledEntry b = Heap[index];
		while (true)
		{
			int num = (index << 1) + 1;
			if (num >= HeapCount)
			{
				break;
			}
			if (num + 1 < HeapCount && IsBefore(in Heap[num + 1], in Heap[num]))
			{
				num++;
			}
			if (!IsBefore(in Heap[num], in b))
			{
				break;
			}
			Heap[index] = Heap[num];
			Heap[index].Instance.HeapIndex = index;
			index = num;
		}
		Heap[index] = b;
		b.Instance.HeapIndex = index;
	}

	private static bool IsBefore(in ScheduledEntry a, in ScheduledEntry b)
	{
		if (a.At != b.At)
		{
			return a.At < b.At;
		}
		return a.Sequence < b.Sequence;
	}
}
