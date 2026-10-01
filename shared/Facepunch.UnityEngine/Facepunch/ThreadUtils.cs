using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Jobs.LowLevel.Unsafe;

namespace Facepunch;

public static class ThreadUtils
{
	public static int GetBatchSize(int count, int subdivideFactor = 4, int minBatchSize = 64)
	{
		return Math.Max(count / JobsUtility.JobWorkerCount / subdivideFactor, minBatchSize);
	}

	public static void WaitForTasks(List<UniTask> tasks)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (tasks.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("WaitForTasks"))
		{
			bool flag;
			do
			{
				flag = false;
				foreach (UniTask task in tasks)
				{
					UniTask current = task;
					flag |= !UniTaskStatusExtensions.IsCompleted(current.Status);
				}
			}
			while (flag);
			foreach (UniTask task2 in tasks)
			{
				UniTask current2 = task2;
				Awaiter awaiter = current2.GetAwaiter();
				awaiter.GetResult();
			}
		}
	}

	public static void WaitForCompletion(this UniTask task)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		while (!UniTaskStatusExtensions.IsCompleted(task.Status))
		{
		}
		Awaiter awaiter = task.GetAwaiter();
		awaiter.GetResult();
	}

	public static T WaitForCompletion<T>(this UniTask<T> task)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		while (!UniTaskStatusExtensions.IsCompleted(task.Status))
		{
		}
		return task.GetAwaiter().GetResult();
	}
}
