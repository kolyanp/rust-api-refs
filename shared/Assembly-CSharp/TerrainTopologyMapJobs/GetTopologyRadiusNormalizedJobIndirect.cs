using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GetTopologyRadiusNormalizedJobIndirect : IJobParallelFor
{
	public float OneOverSizeX;

	public ReadOnly<int> Src;

	public int Res;

	public ReadOnly<Vector2> WorldNXZ;

	public ReadOnly<float> Radii;

	[WriteOnly]
	public NativeArray<int> Topologies;

	public void Execute(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		float x = WorldNXZ[index].x;
		float y = WorldNXZ[index].y;
		Topologies[index] = TerrainTopologyMapJobUtil.GetTopologyRadius(Src, Res, OneOverSizeX, Radii[index], x, y);
	}
}
