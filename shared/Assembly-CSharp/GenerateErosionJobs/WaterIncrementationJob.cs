using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct WaterIncrementationJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public float WaterFillRate;

	public float DT;

	public void Execute(int index)
	{
		ref NativeArray<float> waterMap = ref WaterMap;
		waterMap[index] += WaterFillRate * DT;
	}
}
