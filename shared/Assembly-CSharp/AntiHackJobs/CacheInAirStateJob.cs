using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CacheInAirStateJob : IJob
{
	[WriteOnly]
	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public ReadOnly<bool> PlayersInAir;

	public ReadOnly<int> Indices;

	public ReadOnly<int> BatchMap;

	public ReadOnly<Vector3> OldPoses;

	public void Execute()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Span<AntiHack.PlayerFlyhackState> span = NativeArray<AntiHack.PlayerFlyhackState>.op_Implicit(ref PlayerStates);
		for (int i = 0; i < PlayersInAir.Length; i++)
		{
			int index = Indices[BatchMap[i]];
			ref AntiHack.PlayerFlyhackState reference = ref span[index];
			if (!reference.IsInAir)
			{
				reference.LastGroundedPosition = OldPoses[i];
			}
			reference.IsInAir = PlayersInAir[i];
		}
	}
}
