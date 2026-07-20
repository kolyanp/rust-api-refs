using System;
using System.Collections.Generic;
using Carbon;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins;

public class Timers : Library
{
	public Plugin Plugin { get; }

	internal List<Timer> _timers { get; set; } = new List<Timer>();

	public Plugin.Persistence Persistence => Plugin.persistence;

	public Timers()
	{
	}

	public Timers(Plugin plugin)
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
		if (_timers == null)
		{
			return;
		}
		foreach (Timer timer in _timers)
		{
			timer.Destroy();
		}
		_timers.Clear();
		_timers = null;
	}

	public Timer In(float time, Action action)
	{
		if (!IsValid())
		{
			return null;
		}
		Timer timer = new Timer(Persistence, action, Plugin);
		Action action2 = delegate
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Delay = time;
		timer.Callback = action2;
		if (Community.IsServerInitialized)
		{
			((FacepunchBehaviour)Persistence).Invoke(action2, time);
		}
		return timer;
	}

	public Timer Once(float time, Action action)
	{
		return In(time, action);
	}

	public Timer Every(float time, Action action)
	{
		if (!IsValid())
		{
			return null;
		}
		Timer timer = new Timer(Persistence, action, Plugin);
		Action action2 = delegate
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Callback = action2;
		((FacepunchBehaviour)Persistence).InvokeRepeating(action2, time, time);
		return timer;
	}

	public Timer Repeat(float time, int times, Action action)
	{
		if (!IsValid())
		{
			return null;
		}
		Timer timer = new Timer(Persistence, action, Plugin);
		Action action2 = delegate
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;
				if (times != 0 && timer.TimesTriggered >= times && !((Object)(object)Persistence == (Object)null))
				{
					((FacepunchBehaviour)Persistence).CancelInvoke(timer.Callback);
					((FacepunchBehaviour)Persistence).CancelInvokeFixedTime(timer.Callback);
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {time}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
				timer.Destroy();
			}
		};
		timer.Delay = time;
		timer.Callback = action2;
		((FacepunchBehaviour)Persistence).InvokeRepeating(action2, time, time);
		return timer;
	}

	public void Destroy(ref Timer timer)
	{
		if (timer != null)
		{
			timer.Destroy();
		}
		timer = null;
	}

	public void DestroyAll()
	{
		foreach (Timer timer in _timers)
		{
			timer.Destroy();
		}
		_timers.Clear();
	}
}
