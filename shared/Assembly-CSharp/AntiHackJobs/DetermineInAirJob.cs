using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct DetermineInAirJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerFlyhackState> PlayerFlyStates;

	public ReadOnly<BasePlayer.CachedState> PlayerStates;

	public ReadOnly<AntiHack.FlyingBatch> FlyingBatches;

	public ReadOnly<Flag> PlayerMSFlags;

	public ReadOnly<Vector3> OldPoses;

	public ReadOnly<bool> WaterValidStates;

	public ReadOnly<bool> ElevatorValidStates;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public bool verifyGrounded;

	public void Execute()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		Span<AntiHack.PlayerFlyhackState> span = NativeArray<AntiHack.PlayerFlyhackState>.op_Implicit(ref PlayerFlyStates);
		int num = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			AntiHack.FlyingBatch flyingBatch = FlyingBatches[i];
			int num2 = Indices[i];
			BasePlayer.CachedState cachedState = PlayerStates[num2];
			Flag val = PlayerMSFlags[num2];
			_ = ref span[num2];
			for (int j = 0; j < flyingBatch.Count; j++)
			{
				bool flag = (val & 4) > 0;
				if (verifyGrounded)
				{
					if (cachedState.IsOnLadder)
					{
						Results[num] = false;
					}
					else if (WaterValidStates[num])
					{
						Results[num] = false;
					}
					else if (ElevatorValidStates[num])
					{
						Results[num] = false;
					}
				}
				else
				{
					Results[num] = !cachedState.IsOnLadder && !cachedState.IsSwimming && !flag;
				}
				num++;
			}
		}
	}
}
