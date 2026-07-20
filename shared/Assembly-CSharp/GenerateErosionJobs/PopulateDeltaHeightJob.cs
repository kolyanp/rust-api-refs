using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PopulateDeltaHeightJob : IJobParallelFor
{
	public ReadOnly<float> HeightMapOriginal;

	public ReadOnly<float> HeightMap;

	[WriteOnly]
	public NativeArray<float> DeltaHeightMap;

	public void Execute(int index)
	{
		DeltaHeightMap[index] = HeightMapOriginal[index] - HeightMap[index];
	}
}
