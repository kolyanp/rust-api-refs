using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherPlayersWithTicksJob : IJob
{
	[WriteOnly]
	public NativeList<int> ValidIndices;

	[ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<int> enumerator = Indices.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				if (TickCache.Infos[current].Count > 0)
				{
					ValidIndices.AddNoResize(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
