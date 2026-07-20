using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct FillAsOceanTopologyJob : IJobParallelFor
{
	public NativeArray<Vector4> vectors;

	public void Execute(int index)
	{
		BurstUtil.Get<Vector4>(ref vectors, index).w = 1f;
	}
}
