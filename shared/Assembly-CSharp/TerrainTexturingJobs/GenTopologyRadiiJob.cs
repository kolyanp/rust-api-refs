using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GenTopologyRadiiJob : IJobParallelFor
{
	public ReadOnly<float> heights;

	public NativeArray<float> radii;

	public void Execute(int index)
	{
		float num = heights[index];
		float num2 = Mathf.InverseLerp(4f, 0f, num);
		float num3 = Mathf.Lerp(8f, 16f, num2);
		radii[index] = num3;
	}
}
