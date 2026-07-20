using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckRayJob : IJob
{
	public NativeReference<bool> Result;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	public Vector3 Start;

	public Vector3 End;

	public float CheckRad;

	public void Execute()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Result.Value = Grid.Check(Start, End, CheckRad);
	}
}
