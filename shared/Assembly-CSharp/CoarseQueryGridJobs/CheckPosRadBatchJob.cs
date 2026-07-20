using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckPosRadBatchJob : IJob
{
	[WriteOnly]
	public NativeList<int> OverlapIndices;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<float> Radii;

	public void Execute()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Pos.Length; i++)
		{
			if (Grid.Check(Pos[i], Radii[i]))
			{
				OverlapIndices.AddNoResize(i);
			}
		}
	}
}
