using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TestAreSpeedingJob : IJobFor
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerSpeedhackState> PlayerStates;

	public ReadOnly<(float Dist, float Budget)> DistAndBudget;

	public ReadOnly<float> DeltaTime;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public float ForgivenessInertia;

	[ReadOnly]
	public float Forgiveness;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		ref AntiHack.PlayerSpeedhackState reference = ref NativeArray<AntiHack.PlayerSpeedhackState>.op_Implicit(ref PlayerStates)[num];
		float num2 = Mathf.Max((reference.PauseTime > 0f) ? ForgivenessInertia : Forgiveness, 0.1f);
		float num3 = num2 + Mathf.Max(Forgiveness, 0.1f);
		reference.Distance = Mathf.Clamp(reference.Distance, 0f - num3, num3);
		(float, float) tuple = DistAndBudget[num];
		float item = tuple.Item1;
		float item2 = tuple.Item2;
		float num4 = ((reference.ExtraSpeedTime > 0f) ? (reference.ExtraSpeed * DeltaTime[num]) : 0f);
		reference.Distance = Mathf.Clamp(reference.Distance - item2 - num4, 0f - num3, num3);
		if (reference.Distance > num2)
		{
			Results[jobInd] = true;
			return;
		}
		reference.Distance = Mathf.Clamp(reference.Distance + item, 0f - num3, num3);
		if (reference.Distance > num2)
		{
			Results[jobInd] = true;
		}
		else
		{
			Results[jobInd] = false;
		}
	}
}
