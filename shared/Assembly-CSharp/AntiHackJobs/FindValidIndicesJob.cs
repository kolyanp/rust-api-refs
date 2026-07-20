using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct FindValidIndicesJob : IJob
{
	[WriteOnly]
	public NativeList<int> ValidIndices;

	public NativeArray<bool> WorkBuffer;

	public ReadOnly<int> InvalidIndices;

	public ReadOnly<int> AllIndices;

	public void Execute()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<int> enumerator = AllIndices.GetEnumerator();
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
		enumerator = InvalidIndices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current2 = enumerator.Current;
				WorkBuffer[current2] = false;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		enumerator = AllIndices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current3 = enumerator.Current;
				if (WorkBuffer[current3])
				{
					ValidIndices.AddNoResize(current3);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
