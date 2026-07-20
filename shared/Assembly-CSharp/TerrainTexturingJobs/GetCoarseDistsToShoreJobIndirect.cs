using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GetCoarseDistsToShoreJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Dists;

	public ReadOnly<Vector3> Positions;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public TerrainTexturing.ShoreVectorQueryStructure QueryStructure;

	public void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 pos = Positions[num];
			Dists[num] = QueryStructure.GetCoarseDistanceToShore(pos);
		}
	}
}
