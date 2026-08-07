using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CalcDistAndBudgetJob : IJobFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<(float Dist, float Budget)> DistAndBudget;

	[WriteOnly]
	public NativeList<int> IndicesForNormalSample;

	public ReadOnly<Vector3> Start;

	public ReadOnly<Vector3> End;

	public ReadOnly<BasePlayer.CachedState> States;

	public ReadOnly<float> Speed;

	public ReadOnly<float> DeltaTime;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public bool Use3DMagnitude;

	public void Execute(int jobInd)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = Indices[jobInd];
		bool isSwimming = States[num].IsSwimming;
		Vector3 val = End[num] - Start[num];
		float num2 = ((isSwimming && Use3DMagnitude) ? ((Vector3)(ref val)).magnitude : Vector3Ex.Magnitude2D(val));
		float num3 = Speed[jobInd] * DeltaTime[num];
		DistAndBudget[num] = (num2, num3);
		if (!isSwimming && num2 > num3)
		{
			IndicesForNormalSample.AddNoResize(num);
		}
	}
}
