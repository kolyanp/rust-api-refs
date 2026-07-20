using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckPosRadBatchJobIndirect : IJob
{
	[WriteOnly]
	public NativeList<int> OverlapIndices;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<float> Radii;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (Grid.Check(Pos[num], Radii[num]))
			{
				OverlapIndices.AddNoResize(num);
			}
		}
	}
}
