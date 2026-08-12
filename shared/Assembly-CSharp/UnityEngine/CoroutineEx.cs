using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facepunch;
using Rust;

namespace UnityEngine;

public static class CoroutineEx
{
	public static WaitForEndOfFrame waitForEndOfFrame;

	public static WaitForFixedUpdate waitForFixedUpdate;

	private static Dictionary<float, WaitForSeconds> waitForSecondsBuffer;

	public static WaitForSeconds waitForSeconds(float seconds)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (!waitForSecondsBuffer.TryGetValue(seconds, out var value))
		{
			value = new WaitForSeconds(seconds);
			waitForSecondsBuffer.Add(seconds, value);
		}
		return value;
	}

	public static WaitForSecondsRealtimeEx waitForSecondsRealtime(float seconds)
	{
		WaitForSecondsRealtimeEx obj = Pool.Get<WaitForSecondsRealtimeEx>();
		obj.WaitTime = seconds;
		return obj;
	}

	public static IEnumerator Combine(params IEnumerator[] coroutines)
	{
		while (true)
		{
			bool flag = true;
			foreach (IEnumerator enumerator in coroutines)
			{
				if (enumerator != null && enumerator.MoveNext())
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			yield return waitForEndOfFrame;
		}
	}

	public static Task AsTask(this IEnumerator coroutine)
	{
		if (coroutine == null)
		{
			return Task.CompletedTask;
		}
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
		((MonoBehaviour)Global.Runner).StartCoroutine(RunImpl());
		return tcs.Task;
		IEnumerator RunImpl()
		{
			yield return coroutine;
			tcs.SetResult(null);
		}
	}

	static CoroutineEx()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		waitForEndOfFrame = new WaitForEndOfFrame();
		waitForFixedUpdate = new WaitForFixedUpdate();
		waitForSecondsBuffer = new Dictionary<float, WaitForSeconds>();
	}
}
