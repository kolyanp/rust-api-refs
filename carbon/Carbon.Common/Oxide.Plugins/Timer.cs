using System;
using Carbon;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins;

public class Timer : IDisposable
{
	public Plugin Plugin { get; set; }

	public Action Activity { get; set; }

	public Action Callback { get; set; }

	public Plugin.Persistence Persistence { get; set; }

	public int Repetitions { get; set; }

	public float Delay { get; set; }

	public int TimesTriggered { get; set; }

	public bool Destroyed { get; set; }

	public Timer()
	{
	}

	public Timer(Plugin.Persistence persistence, Action activity, Plugin plugin = null)
	{
		Persistence = persistence;
		Activity = activity;
		Plugin = plugin;
	}

	public void Reset(float delay = -1f, int repetitions = 1)
	{
		TimesTriggered = 0;
		Repetitions = repetitions;
		if (delay < 0f)
		{
			delay = Delay;
		}
		else
		{
			Delay = delay;
		}
		if (Destroyed)
		{
			Logger.Warn("You cannot restart a timer that has been destroyed.");
			return;
		}
		if ((Object)(object)Persistence != (Object)null)
		{
			((FacepunchBehaviour)Persistence).CancelInvoke(Callback);
			((FacepunchBehaviour)Persistence).CancelInvokeFixedTime(Callback);
		}
		if (Repetitions == 1)
		{
			Callback = delegate
			{
				try
				{
					Activity?.Invoke();
					int timesTriggered = TimesTriggered;
					TimesTriggered = timesTriggered + 1;
				}
				catch (Exception ex)
				{
					Logger.Error($"Timer of {delay}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
				}
				Destroy();
			};
			((FacepunchBehaviour)Persistence).Invoke(Callback, delay);
			return;
		}
		Callback = delegate
		{
			try
			{
				Activity?.Invoke();
				int timesTriggered = TimesTriggered;
				TimesTriggered = timesTriggered + 1;
				if (TimesTriggered >= Repetitions)
				{
					Dispose();
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Timer of {delay}s has failed in '{Plugin.ToPrettyString()}' [callback]", ex);
				Destroy();
			}
		};
		((FacepunchBehaviour)Persistence).InvokeRepeating(Callback, delay, delay);
	}

	public bool Destroy()
	{
		if (Destroyed)
		{
			return false;
		}
		Destroyed = true;
		if ((Object)(object)Persistence != (Object)null)
		{
			((FacepunchBehaviour)Persistence).CancelInvoke(Callback);
		}
		if (Callback != null)
		{
			Callback = null;
		}
		return true;
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
