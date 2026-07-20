using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct ProcessTopologyJob : IJobParallelFor
{
	public NativeArray<Vector4> vectors;

	public ReadOnly<int> topologies;

	public void Execute(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = vectors[index];
		int num = topologies[index];
		if ((num & 0x180) != 0)
		{
			val.w = 1f;
		}
		else if ((num & 0x32000) != 0)
		{
			val.w = 2f;
		}
		else if ((num & 0xC000) != 0)
		{
			val.w = 3f;
		}
		vectors[index] = val;
	}
}
