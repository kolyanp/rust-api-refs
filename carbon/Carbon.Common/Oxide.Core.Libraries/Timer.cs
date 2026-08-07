using System;
using System.Collections.Generic;
using Carbon;
using Facepunch;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Core.Libraries;

public class Timer : Library
{
	public class TimerInstance : IDisposable
	{
		public Plugin Plugin { get; set; }

		internal Timer OwnerTimers { get; set; }

		public Action Activity { get; set; }

		public Action Callback { get; set; }

		public Plugin.Persistence Persistence { get; set; }

		public int Repetitions { get; set; }

		public float Delay { get; set; }

		public float ExpiresAt { get; set; }

		public bool StartupRepeating { get; set; }

		public int TimesTriggered { get; set; }

		public bool Destroyed { get; set; }

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
			TimesTriggered = 0;
			Repetitions = repetitions;
			StartupRepeating = repetitions != 1;
			if (delay < 0f)
			{
				delay = Delay;
			}
			else
			{
				Delay = delay;
			}
			if ((Object)(object)Persistence == (Object)null)
			{
				Logger.Warn("Cannot restart a timer for '" + (Plugin?.ToPrettyString() ?? "unknown plugin") + "' because persistence is null.");
				return;
			}
			RemoveStartupTimer(this);
			if (Callback != null)
			{
				((FacepunchBehaviour)Persistence).CancelInvoke(Callback);
				((FacepunchBehaviour)Persistence).CancelInvokeFixedTime(Callback);
			}
			Destroyed = false;
			OwnerTimers?.TrackTimer(this);
			if (Repetitions == 1)
			{
				Action callback = null;
				callback = delegate
				{
					try
					{
						Activity?.Invoke();
						if (Destroyed || Callback != callback)
						{
							return;
						}
						int timesTriggered = TimesTriggered;
						TimesTriggered = timesTriggered + 1;
					}
					catch (Exception ex)
					{
						Logger.Error($"Timer of {delay}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
						Destroy();
						return;
					}
					Destroy();
				};
				Callback = callback;
				if (Community.IsServerInitialized)
				{
					((FacepunchBehaviour)Persistence).Invoke(Callback, delay);
					return;
				}
				ExpiresAt = Time.realtimeSinceStartup + delay;
				QueueStartupTimer(this);
				return;
			}
			Action callback2 = null;
			callback2 = delegate
			{
				try
				{
					Activity?.Invoke();
					if (!Destroyed && !(Callback != callback2))
					{
						int timesTriggered = TimesTriggered;
						TimesTriggered = timesTriggered + 1;
						if (Repetitions > 0 && TimesTriggered >= Repetitions)
						{
							Destroy();
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Timer of {delay}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
					Destroy();
				}
			};
			Callback = callback2;
			if (Community.IsServerInitialized)
			{
				((FacepunchBehaviour)Persistence).InvokeRepeating(Callback, delay, delay);
				return;
			}
			ExpiresAt = Time.realtimeSinceStartup + NormalizeStartupRepeatDelay(delay);
			QueueStartupTimer(this);
		}

		public bool Destroy()
		{
			bool destroyed = Destroyed;
			Destroyed = true;
			RemoveStartupTimer(this);
			OwnerTimers?.UntrackTimer(this);
			if (Callback != null)
			{
				Plugin.Persistence persistence = Persistence;
				if (persistence != null)
				{
					((FacepunchBehaviour)persistence).CancelInvoke(Callback);
				}
				Callback = null;
			}
			return !destroyed;
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

	private static readonly object StartupTimerLock = new object();

	private static readonly List<TimerInstance> StartupTimers = new List<TimerInstance>();

	private static float _nextStartupTimerAt = float.PositiveInfinity;

	private const int MaxStartupTimersPerFrame = 256;

	private const float StartupTimerDueTolerance = 0.001f;

	private const float MinimumStartupRepeatDelay = 0.001f;

	public Plugin Plugin { get; }

	internal List<TimerInstance> _timers { get; set; } = new List<TimerInstance>();

	public Plugin.Persistence Persistence => Plugin.persistence;

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
		if (_timers == null)
		{
			List<TimerInstance> list = (_timers = new List<TimerInstance>());
		}
		if (!_timers.Contains(timer))
		{
			_timers.Add(timer);
		}
	}

	internal void UntrackTimer(TimerInstance timer)
	{
		if (timer.OwnerTimers == this)
		{
			_timers?.Remove(timer);
		}
	}

	public TimerInstance In(float time, Action action, Plugin plugin = null)
	{
		if (!IsValid())
		{
			return null;
		}
		TimerInstance timer = new TimerInstance(Persistence, action, plugin ?? Plugin);
		TrackTimer(timer);
		timer.Repetitions = 1;
		Action action2 = delegate
		{
			try
			{
				Action callback = timer.Callback;
				action?.Invoke();
				if (!timer.Destroyed && !(timer.Callback != callback))
				{
					timer.TimesTriggered++;
					timer.Destroy();
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{(plugin ?? Plugin).ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Delay = time;
		timer.Callback = action2;
		if (Community.IsServerInitialized)
		{
			((FacepunchBehaviour)Persistence).Invoke(action2, time);
		}
		else
		{
			timer.ExpiresAt = Time.realtimeSinceStartup + time;
			QueueStartupTimer(timer);
		}
		return timer;
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
		TimerInstance timer = new TimerInstance(Persistence, action, plugin ?? Plugin);
		TrackTimer(timer);
		Action action2 = delegate
		{
			try
			{
				Action callback = timer.Callback;
				action?.Invoke();
				if (!timer.Destroyed && !(timer.Callback != callback))
				{
					timer.TimesTriggered++;
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{(plugin ?? Plugin).ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Delay = time;
		timer.Repetitions = 0;
		timer.StartupRepeating = true;
		timer.Callback = action2;
		if (Community.IsServerInitialized)
		{
			((FacepunchBehaviour)Persistence).InvokeRepeating(action2, time, time);
		}
		else
		{
			timer.ExpiresAt = Time.realtimeSinceStartup + NormalizeStartupRepeatDelay(time);
			QueueStartupTimer(timer);
		}
		return timer;
	}

	public TimerInstance Repeat(float time, int times, Action action, Plugin plugin = null)
	{
		if (!IsValid())
		{
			return null;
		}
		TimerInstance timer = new TimerInstance(Persistence, action, plugin ?? Plugin);
		TrackTimer(timer);
		Action action2 = delegate
		{
			try
			{
				Action callback = timer.Callback;
				action?.Invoke();
				if (!timer.Destroyed && !(timer.Callback != callback))
				{
					timer.TimesTriggered++;
					if (times > 0 && timer.TimesTriggered >= times)
					{
						timer.Destroy();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{(plugin ?? Plugin).ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Delay = time;
		timer.Repetitions = times;
		timer.StartupRepeating = times != 1;
		timer.Callback = action2;
		if (Community.IsServerInitialized)
		{
			((FacepunchBehaviour)Persistence).InvokeRepeating(action2, time, time);
		}
		else if (timer.StartupRepeating)
		{
			timer.ExpiresAt = Time.realtimeSinceStartup + NormalizeStartupRepeatDelay(time);
			QueueStartupTimer(timer);
		}
		else
		{
			timer.ExpiresAt = Time.realtimeSinceStartup + time;
			QueueStartupTimer(timer);
		}
		return timer;
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
		if (_timers != null)
		{
			while (_timers.Count > 0)
			{
				List<TimerInstance> timers = _timers;
				TimerInstance timerInstance = timers[timers.Count - 1];
				_timers.RemoveAt(_timers.Count - 1);
				timerInstance.Destroy();
			}
		}
	}

	internal static float NormalizeStartupRepeatDelay(float delay)
	{
		if (!(delay > 0.001f))
		{
			return 0.001f;
		}
		return delay;
	}

	internal static void QueueStartupTimer(TimerInstance timer)
	{
		lock (StartupTimerLock)
		{
			if (StartupTimers.Contains(timer))
			{
				RefreshNextStartupTimerAt();
				return;
			}
			StartupTimers.Add(timer);
			TrackNextStartupTimerAt(timer);
		}
	}

	internal static void RemoveStartupTimer(TimerInstance timer)
	{
		if (Community.IsServerInitialized)
		{
			return;
		}
		lock (StartupTimerLock)
		{
			if (StartupTimers.Remove(timer))
			{
				RefreshNextStartupTimerAt();
			}
		}
	}

	internal static void UpdateStartupTimers()
	{
		if (!Community.IsServerInitialized)
		{
			FireDueStartupTimers(256);
		}
	}

	internal static void FireDueStartupTimers(int maxTimers = int.MaxValue)
	{
		if (maxTimers <= 0)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!HasDueStartupTimers(realtimeSinceStartup))
		{
			return;
		}
		List<TimerInstance> timers = Pool.Get<List<TimerInstance>>();
		List<Action> callbacks = Pool.Get<List<Action>>();
		try
		{
			CollectDueStartupTimers(timers, callbacks, realtimeSinceStartup, maxTimers);
			FireStartupTimers(timers, callbacks);
		}
		finally
		{
			Pool.FreeUnmanaged<TimerInstance>(ref timers);
			Pool.FreeUnmanaged<Action>(ref callbacks);
		}
	}

	internal static void ConvertRemainingStartupTimersToInvokes()
	{
		List<TimerInstance> list = Pool.Get<List<TimerInstance>>();
		try
		{
			lock (StartupTimerLock)
			{
				for (int i = 0; i < StartupTimers.Count; i++)
				{
					list.Add(StartupTimers[i]);
				}
				StartupTimers.Clear();
				_nextStartupTimerAt = float.PositiveInfinity;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			for (int j = 0; j < list.Count; j++)
			{
				TimerInstance timerInstance = list[j];
				if (!timerInstance.Destroyed && !((Object)(object)timerInstance.Persistence == (Object)null) && timerInstance.Callback != null)
				{
					float num = Math.Max(0f, timerInstance.ExpiresAt - realtimeSinceStartup);
					if (timerInstance.StartupRepeating)
					{
						((FacepunchBehaviour)timerInstance.Persistence).InvokeRepeating(timerInstance.Callback, num, timerInstance.Delay);
					}
					else
					{
						((FacepunchBehaviour)timerInstance.Persistence).Invoke(timerInstance.Callback, num);
					}
				}
			}
		}
		finally
		{
			Pool.FreeUnmanaged<TimerInstance>(ref list);
		}
	}

	private static bool ShouldRequeueStartupTimer(TimerInstance timer)
	{
		if (!timer.StartupRepeating || timer.Destroyed || Community.IsServerInitialized)
		{
			return false;
		}
		if (timer.Repetitions > 0)
		{
			return timer.TimesTriggered < timer.Repetitions;
		}
		return true;
	}

	private static bool HasDueStartupTimers(float now)
	{
		lock (StartupTimerLock)
		{
			return StartupTimers.Count > 0 && IsStartupTimerDue(_nextStartupTimerAt, now);
		}
	}

	private static void CollectDueStartupTimers(List<TimerInstance> timers, List<Action> callbacks, float now, int maxTimers)
	{
		lock (StartupTimerLock)
		{
			if (StartupTimers.Count == 0 || !IsStartupTimerDue(_nextStartupTimerAt, now))
			{
				return;
			}
			for (int i = 0; i < StartupTimers.Count; i++)
			{
				TimerInstance timerInstance = StartupTimers[i];
				if (timerInstance.Destroyed)
				{
					StartupTimers.RemoveAt(i);
					i--;
				}
				else if (!(timerInstance.ExpiresAt - now > 0.001f))
				{
					StartupTimers.RemoveAt(i);
					i--;
					timers.Add(timerInstance);
					callbacks.Add(timerInstance.Callback);
					if (timers.Count >= maxTimers)
					{
						break;
					}
				}
			}
			RefreshNextStartupTimerAt();
		}
	}

	private static void FireStartupTimers(List<TimerInstance> timers, List<Action> callbacks)
	{
		for (int i = 0; i < timers.Count; i++)
		{
			TimerInstance timerInstance = timers[i];
			Action action = callbacks[i];
			if (!timerInstance.Destroyed && action != null && !(timerInstance.Callback != action))
			{
				action();
				if (timerInstance.Callback == action && ShouldRequeueStartupTimer(timerInstance))
				{
					timerInstance.ExpiresAt = Time.realtimeSinceStartup + NormalizeStartupRepeatDelay(timerInstance.Delay);
					QueueStartupTimer(timerInstance);
				}
			}
		}
	}

	private static void TrackNextStartupTimerAt(TimerInstance timer)
	{
		if (!timer.Destroyed && timer.ExpiresAt < _nextStartupTimerAt)
		{
			_nextStartupTimerAt = timer.ExpiresAt;
		}
	}

	private static void RefreshNextStartupTimerAt()
	{
		_nextStartupTimerAt = float.PositiveInfinity;
		for (int i = 0; i < StartupTimers.Count; i++)
		{
			TimerInstance timerInstance = StartupTimers[i];
			if (timerInstance.Destroyed)
			{
				StartupTimers.RemoveAt(i);
				i--;
			}
			else
			{
				TrackNextStartupTimerAt(timerInstance);
			}
		}
	}

	private static bool IsStartupTimerDue(float expiresAt, float now)
	{
		return expiresAt - now <= 0.001f;
	}
}
