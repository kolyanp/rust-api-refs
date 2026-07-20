using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class InvokeHandlerBase<T> : SingletonComponent<T> where T : MonoBehaviour
{
	protected ListDictionary<InvokeAction, float> curList = new ListDictionary<InvokeAction, float>(2048);

	protected ListHashSet<InvokeAction> addList = new ListHashSet<InvokeAction>(1024);

	protected ListHashSet<InvokeAction> delList = new ListHashSet<InvokeAction>(1024);

	public InvokeProfiler profiler;

	protected int nullIndex;

	protected const int nullChecks = 50;

	private Stopwatch doTickTimer = new Stopwatch();

	private Stopwatch invokeTimer = new Stopwatch();

	protected void LateUpdate()
	{
		if (profiler == null)
		{
			profiler = InvokeProfiler.update;
		}
		ApplyRemoves();
		ApplyAdds();
		DoTick();
		RemoveExpired();
		ApplyRemoves();
		ApplyAdds();
	}

	protected abstract float GetTime();

	protected void DoTick()
	{
		float[] buffer = curList.Values.Buffer;
		InvokeAction[] buffer2 = curList.Keys.Buffer;
		int count = curList.Count;
		float time = GetTime();
		bool flag = profiler.mode > 0;
		bool flag2 = profiler.mode > 1;
		if (flag)
		{
			doTickTimer.Restart();
		}
		int num = 0;
		TimeSpan executedTime = default(TimeSpan);
		for (int i = 0; i < count; i++)
		{
			if (time < buffer[i])
			{
				continue;
			}
			InvokeAction invokeAction = buffer2[i];
			if (Object.op_Implicit((Object)(object)invokeAction.sender) && !delList.Contains(invokeAction))
			{
				if (invokeAction.repeat >= 0f)
				{
					float num2 = time - buffer[i];
					float num3 = time + invokeAction.repeat - num2 % invokeAction.repeat;
					if (invokeAction.random > 0f)
					{
						num3 += Random.Range(0f - invokeAction.random, invokeAction.random);
					}
					buffer[i] = num3;
				}
				else
				{
					QueueRemove(invokeAction);
				}
				if (flag2)
				{
					invokeTimer.Restart();
				}
				invokeAction.action();
				if (flag2)
				{
					TimeSpan elapsed = invokeTimer.Elapsed;
					invokeAction.TrackingData.ExecutionTime += elapsed;
					invokeAction.TrackingData.Calls++;
					num++;
					executedTime += elapsed;
				}
			}
			else
			{
				QueueRemove(invokeAction);
			}
		}
		if (flag)
		{
			profiler.tickCount = count;
			profiler.executedCount = num;
			profiler.elapsedTime = doTickTimer.Elapsed;
			profiler.executedTime = executedTime;
		}
	}

	protected void RemoveExpired()
	{
		InvokeAction[] buffer = curList.Keys.Buffer;
		int count = curList.Count;
		if (nullIndex >= count)
		{
			nullIndex = 0;
		}
		int num = Mathf.Min(nullIndex + 50, count);
		while (nullIndex < num)
		{
			InvokeAction invoke = buffer[nullIndex];
			if (!Object.op_Implicit((Object)(object)invoke.sender))
			{
				QueueRemove(invoke);
			}
			nullIndex++;
		}
	}

	protected void QueueAdd(InvokeAction invoke)
	{
		if (invoke.action == null)
		{
			Debug.LogError((object)$"Trying to add an invoke with a null action: {new StackTrace()}");
			return;
		}
		delList.Remove(invoke);
		addList.Remove(invoke);
		addList.Add(invoke);
	}

	protected void QueueRemove(InvokeAction invoke)
	{
		delList.Remove(invoke);
		addList.Remove(invoke);
		delList.Add(invoke);
	}

	protected bool Contains(InvokeAction invoke)
	{
		if (!delList.Contains(invoke))
		{
			if (!curList.Contains(invoke))
			{
				return addList.Contains(invoke);
			}
			return true;
		}
		return false;
	}

	protected void ApplyAdds()
	{
		InvokeAction[] buffer = addList.Values.Buffer;
		int count = addList.Count;
		float time = GetTime();
		profiler.addCount += count;
		for (int i = 0; i < count; i++)
		{
			InvokeAction invokeAction = buffer[i];
			curList.Remove(invokeAction);
			curList.Add(invokeAction, time + invokeAction.initial);
			invokeAction.TrackingData.InvokeCount++;
		}
		addList.Clear();
	}

	protected void ApplyRemoves()
	{
		InvokeAction[] buffer = delList.Values.Buffer;
		int count = delList.Count;
		profiler.deletedCount += count;
		for (int i = 0; i < count; i++)
		{
			InvokeAction invokeAction = buffer[i];
			curList.Remove(invokeAction);
			invokeAction.TrackingData.InvokeCount--;
		}
		delList.Clear();
	}

	public void ForEach(Action<InvokeAction> callback)
	{
		foreach (var (obj, _) in curList)
		{
			callback(obj);
		}
	}
}
