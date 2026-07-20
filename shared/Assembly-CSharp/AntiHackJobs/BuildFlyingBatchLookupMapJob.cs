using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct BuildFlyingBatchLookupMapJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Lookup;

	public ReadOnly<AntiHack.FlyingBatch> Batches;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Batches.Length; i++)
		{
			AntiHack.FlyingBatch flyingBatch = Batches[i];
			for (int j = 0; j < flyingBatch.Count; j++)
			{
				int num2 = num + j;
				Lookup[num2] = i;
			}
			num += flyingBatch.Count;
		}
	}
}
