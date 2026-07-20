using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GenShoreVecBitMapJob : IJobParallelFor
{
	public ReadOnly<float> waterHeights;

	public ReadOnly<float> terrainHeights;

	[WriteOnly]
	public NativeArray<byte> bitmap;

	public void Execute(int index)
	{
		bool flag = Mathf.Max(waterHeights[index] - terrainHeights[index], 0f) <= 0f;
		bitmap[index] = (byte)(flag ? 255u : 0u);
	}
}
