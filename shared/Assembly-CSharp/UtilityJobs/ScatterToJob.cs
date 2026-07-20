using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UtilityJobs;

[BurstCompile]
public struct ScatterToJob<T> : IJob where T : unmanaged
{
	[WriteOnly]
	public NativeArray<T> Results;

	public ReadOnly<T> Source;

	public ReadOnly<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			Results[Indices[i]] = Source[i];
		}
	}
}
