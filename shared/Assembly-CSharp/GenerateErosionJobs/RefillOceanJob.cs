using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct RefillOceanJob : IJobParallelFor
{
	public ReadOnly<int> OceanIndices;

	public ReadOnly<float> HeightMap;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> WaterMap;

	public float OceanLevel;

	public void Execute(int index)
	{
		int num = OceanIndices[index];
		float num2 = HeightMap[num];
		WaterMap[num] = OceanLevel - num2;
	}
}
