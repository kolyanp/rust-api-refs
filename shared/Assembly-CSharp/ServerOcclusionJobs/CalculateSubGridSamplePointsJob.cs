using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ServerOcclusionJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalculateSubGridSamplePointsJob : IJobFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Vector3> Posi;

	[ReadOnly]
	public ReadOnly<ServerOcclusion.SubGrid> SubGridCells;

	[ReadOnly]
	public ReadOnly<Vector3> GridOffsets;

	[ReadOnly]
	public Vector3 CellOffset;

	public void Execute(int index)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		int length = GridOffsets.Length;
		ServerOcclusion.SubGrid subGrid = SubGridCells[index];
		Vector3 val = new Vector3((float)subGrid.x - CellOffset.x, (float)subGrid.y - CellOffset.y, (float)subGrid.z - CellOffset.z) * 2f;
		for (int i = 0; i < length; i++)
		{
			Posi[index * length + i] = val + GridOffsets[i];
		}
	}
}
