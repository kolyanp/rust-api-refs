using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherWasInAirStatesJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public ReadOnly<AntiHack.PlayerFlyhackState> PlayerStates;

	public ReadOnly<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			AntiHack.PlayerFlyhackState playerFlyhackState = PlayerStates[num];
			Results[i] = playerFlyhackState.IsInAir;
		}
	}
}
