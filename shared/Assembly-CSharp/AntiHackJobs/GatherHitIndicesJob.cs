using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherHitIndicesJob : IJob
{
	[WriteOnly]
	public NativeList<int> Results;

	public ReadOnly<bool> Hits;

	public void Execute()
	{
		for (int i = 0; i < Hits.Length; i++)
		{
			if (Hits[i])
			{
				Results.AddNoResize(i);
			}
		}
	}
}
