using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckBoundsJob : IJob
{
	public NativeReference<bool> Result;

	[NativeDisableContainerSafetyRestriction]
	public CoarseQueryGrid Grid;

	public Bounds CheckBounds;

	public void Execute()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Result.Value = Grid.Check(CheckBounds);
	}
}
