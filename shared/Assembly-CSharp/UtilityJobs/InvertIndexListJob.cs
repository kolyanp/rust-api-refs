using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct InvertIndexListJob : IJob
{
	public NativeList<int> Indices;

	public NativeArray<bool> WorkBuffer;

	public void Execute()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < WorkBuffer.Length; i++)
		{
			WorkBuffer[i] = false;
		}
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				WorkBuffer[current] = true;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Indices.Clear();
		for (int j = 0; j < WorkBuffer.Length; j++)
		{
			if (!WorkBuffer[j])
			{
				Indices.AddNoResize(j);
			}
		}
	}
}
