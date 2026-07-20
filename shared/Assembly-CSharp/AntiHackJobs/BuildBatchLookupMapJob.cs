using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct BuildBatchLookupMapJob : IJob
{
	[WriteOnly]
	public NativeArray<int> Lookup;

	public ReadOnly<AntiHack.Batch> Batches;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Batches.Length; i++)
		{
			AntiHack.Batch batch = Batches[i];
			for (int j = 0; j < batch.Count; j++)
			{
				int num2 = num + j;
				Lookup[num2] = i;
			}
			num += batch.Count;
		}
	}
}
