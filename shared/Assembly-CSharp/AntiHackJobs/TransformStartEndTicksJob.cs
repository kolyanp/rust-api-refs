using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TransformStartEndTicksJob : IJobFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> Starts;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> Ends;

	public ReadOnly<Matrix4x4> Matrices;

	public TickInterpolatorCache.ReadOnlyState TickCache;

	public ReadOnly<int> Indices;

	public void Execute(int jobInd)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		int num = Indices[jobInd];
		Matrix4x4 val = Matrices[jobInd];
		bool flag = ((Matrix4x4)(ref val))[15] == 0f;
		TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, num);
		Starts[num] = (flag ? playerTickIterator.StartPoint : ((Matrix4x4)(ref val)).MultiplyPoint3x4(playerTickIterator.StartPoint));
		Ends[num] = (flag ? playerTickIterator.EndPoint : ((Matrix4x4)(ref val)).MultiplyPoint3x4(playerTickIterator.EndPoint));
	}
}
