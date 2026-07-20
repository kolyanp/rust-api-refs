using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterSystemJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct AdjustByTopologyJob : IJobParallelForDefer
{
	[ReadOnly]
	public NativeList<Ray> rays;

	public NativeArray<bool> hitResults;

	public NativeArray<Vector3> hitNormals;

	public ReadOnly<Vector3> hitPositions;

	[ReadOnly]
	public NativeArray<int> TopologyData;

	[ReadOnly]
	public int TopologyRes;

	[ReadOnly]
	public Vector2 DataOrigin;

	[ReadOnly]
	public Vector2 DataScale;

	public void Execute(int index)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (hitResults[index])
		{
			hitNormals[index] = Vector3.up;
			float num = (hitPositions[index].x - DataOrigin.x) * DataScale.x;
			float num2 = (hitPositions[index].z - DataOrigin.y) * DataScale.y;
			int num3 = Math.Clamp((int)(num * (float)TopologyRes), 0, TopologyRes - 1);
			int num4 = Math.Clamp((int)(num2 * (float)TopologyRes), 0, TopologyRes - 1) * TopologyRes + num3;
			hitResults[index] = (TopologyData[num4] & 0x180) != 0;
		}
	}
}
