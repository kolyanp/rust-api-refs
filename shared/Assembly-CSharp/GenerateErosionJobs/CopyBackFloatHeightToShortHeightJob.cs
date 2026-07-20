using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct CopyBackFloatHeightToShortHeightJob : IJobParallelFor
{
	public ReadOnly<float> HeightMapAsFloat;

	[WriteOnly]
	public NativeArray<short> HeightMapAsShort;

	public float TerrainOneOverSizeY;

	public float TerrainPositionY;

	public void Execute(int index)
	{
		HeightMapAsShort[index] = BitUtility.Float2Short(TerrainOneOverSizeY * (HeightMapAsFloat[index] - TerrainPositionY));
	}
}
