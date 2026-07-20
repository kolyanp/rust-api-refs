using System;
using System.Collections;
using System.Threading.Tasks;

namespace UnityEngine;

public static class ParallelEx
{
	public static void Call(Action<int, int> action)
	{
		Task[] tasks = new Task[Environment.ProcessorCount];
		for (int i = 0; i < tasks.Length; i++)
		{
			int threadId = i;
			tasks[i] = Task.Run(delegate
			{
				action(threadId, tasks.Length);
			});
		}
		Task[] array = tasks;
		for (int num = 0; num < array.Length; num++)
		{
			array[num].Wait();
		}
	}

	[Obsolete("Use Task.Run instead - see ParallelEx.Coroutine implementation for an example")]
	public static IEnumerator Coroutine(Action action)
	{
		Task task = Task.Run(action);
		while (!task.IsCompleted)
		{
			yield return null;
		}
		task.Wait();
	}
}
