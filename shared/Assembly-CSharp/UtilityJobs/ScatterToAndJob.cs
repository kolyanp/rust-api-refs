using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterToAndJob : IJob
{
	public NativeArray<bool> To;

	public ReadOnly<bool> From;

	public ReadOnly<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			ref NativeArray<bool> to = ref To;
			int num = Indices[i];
			to[num] &= From[i];
		}
	}
}
