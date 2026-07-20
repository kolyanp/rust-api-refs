using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct ProcessFlyhackPauseTimeJob : IJob
{
	[WriteOnly]
	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public ReadOnly<int> Indices;

	public ReadOnly<float> DeltaTimes;

	public void Execute()
	{
		Span<AntiHack.PlayerFlyhackState> span = NativeArray<AntiHack.PlayerFlyhackState>.op_Implicit(ref PlayerStates);
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			ref AntiHack.PlayerFlyhackState reference = ref span[num];
			reference.PauseTime = Mathf.Max(0f, reference.PauseTime - DeltaTimes[num]);
		}
	}
}
