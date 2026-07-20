using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GenerateErosionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PrepareMapJob : IJobParallelForBatch
{
	public ReadOnly<short> HeightMapAsShort;

	[WriteOnly]
	public NativeArray<float> HeightMapAsFloat;

	public ParallelWriter<int> OceanIndicesWriter;

	public float TerrainPositionY;

	public float TerrainSizeY;

	public float OceanLevel;

	public void Execute(int startIndex, int count)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		NativeList<int> val = default(NativeList<int>);
		val._002Ector(count, AllocatorHandle.op_Implicit((Allocator)2));
		for (int i = startIndex; i < startIndex + count; i++)
		{
			float num = (HeightMapAsFloat[i] = TerrainPositionY + BitUtility.Short2Float((int)HeightMapAsShort[i]) * TerrainSizeY);
			if (num <= OceanLevel)
			{
				val.Add(ref i);
			}
		}
		OceanIndicesWriter.AddRangeNoResize(val);
	}
}
