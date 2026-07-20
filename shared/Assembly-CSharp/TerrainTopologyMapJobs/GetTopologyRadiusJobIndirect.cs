using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTopologyMapJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GetTopologyRadiusJobIndirect : IJobParallelFor
{
	public float WorldX;

	public float WorldZ;

	public float OneOverSizeX;

	public float OneOverSizeZ;

	public ReadOnly<int> Src;

	public int Res;

	public ReadOnly<Vector3> WorldPositions;

	public ReadOnly<float> Radii;

	[WriteOnly]
	public NativeArray<int> Topologies;

	public void Execute(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = WorldPositions[index];
		float normX = (val.x - WorldX) * OneOverSizeX;
		float normZ = (val.z - WorldZ) * OneOverSizeZ;
		Topologies[index] = TerrainTopologyMapJobUtil.GetTopologyRadius(Src, Res, OneOverSizeX, Radii[index], normX, normZ);
	}
}
