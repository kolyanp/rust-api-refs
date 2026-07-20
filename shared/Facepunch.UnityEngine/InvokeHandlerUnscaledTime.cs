using System;
using UnityEngine;

public class InvokeHandlerUnscaledTime : InvokeHandlerBase<InvokeHandlerUnscaledTime>
{
	protected override float GetTime()
	{
		return Time.unscaledTime;
	}

	public static bool IsInvoking(Behaviour sender, Action action)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance))
		{
			return false;
		}
		return SingletonComponent<InvokeHandlerUnscaledTime>.Instance.Contains(new InvokeAction(sender, action, null));
	}

	public static void Invoke(Behaviour sender, Action action, float time)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance))
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time));
	}

	public static void InvokeRepeating(Behaviour sender, Action action, float time, float repeat)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance))
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time, repeat));
	}

	public static void CancelInvoke(Behaviour sender, Action action)
	{
		if (!((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance == (Object)null))
		{
			InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
			SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueRemove(new InvokeAction(sender, action, trackingData));
		}
	}

	public static void InvokeRandomized(Behaviour sender, Action action, float time, float repeat, float random)
	{
		if (!Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance))
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time, repeat, random));
	}

	private static void CreateInstance()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		GameObject val = new GameObject
		{
			name = "InvokeHandlerDemo"
		};
		val.AddComponent<InvokeHandlerUnscaledTime>().profiler = InvokeProfiler.demo;
		Object.DontDestroyOnLoad((Object)val);
	}
}
