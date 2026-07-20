using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct ProgressSpeedingStatesJob : IJobFor
{
	public NativeArray<AntiHack.PlayerSpeedhackState> SpeedStates;

	public ReadOnly<float> DeltaTime;

	public ReadOnly<int> Indices;

	public void Execute(int jobInd)
	{
		int num = Indices[jobInd];
		ref AntiHack.PlayerSpeedhackState reference = ref NativeArray<AntiHack.PlayerSpeedhackState>.op_Implicit(ref SpeedStates)[num];
		float num2 = DeltaTime[num];
		reference.PauseTime = Mathf.Max(0f, reference.PauseTime - num2);
		reference.ExtraSpeedTime = Mathf.Max(0f, reference.ExtraSpeedTime - num2);
	}
}
