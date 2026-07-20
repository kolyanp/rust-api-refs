using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct EvaporationJob : IJobParallelFor
{
	public NativeArray<float> WaterMap;

	public float DT;

	public float EvaporationRate;

	public void Execute(int index)
	{
		ref NativeArray<float> waterMap = ref WaterMap;
		waterMap[index] *= 1f - EvaporationRate * DT;
	}
}
