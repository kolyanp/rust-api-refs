using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct TestAreFlyingJob : IJob
{
	public NativeArray<bool> Results;

	public NativeArray<AntiHack.PlayerFlyhackState> PlayerStates;

	public ReadOnly<int> Indices;

	public ReadOnly<int> BatchMap;

	public ReadOnly<Vector3> OldPoses;

	public ReadOnly<Vector3> NewPoses;

	public ReadOnly<bool> PlayersInAir;

	public ReadOnly<bool> WasInAirStates;

	[ReadOnly]
	public float ForgivenessVerticalInertia;

	[ReadOnly]
	public float ForgivenessVertical;

	[ReadOnly]
	public float ForgivenessHorizontalInertia;

	[ReadOnly]
	public float ForgivenessHorizontal;

	[ReadOnly]
	public float TimeSinceStartup;

	public void Execute()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		Span<AntiHack.PlayerFlyhackState> span = NativeArray<AntiHack.PlayerFlyhackState>.op_Implicit(ref PlayerStates);
		for (int i = 0; i < OldPoses.Length; i++)
		{
			int num = BatchMap[i];
			if (Results[num])
			{
				continue;
			}
			int index = Indices[num];
			ref AntiHack.PlayerFlyhackState reference = ref span[index];
			if (PlayersInAir[i])
			{
				bool flag = false;
				float num2 = ((reference.PauseTime > 0f) ? ForgivenessVerticalInertia : ForgivenessVertical);
				float num3 = ((reference.PauseTime > 0f) ? ForgivenessHorizontalInertia : ForgivenessHorizontal);
				Vector3 val = NewPoses[i] - OldPoses[i];
				float num4 = Mathf.Abs(val.y);
				float num5 = Vector3Ex.Magnitude2D(val);
				if (val.y >= 0f)
				{
					reference.VerticalDistance += val.y;
					flag = true;
				}
				if (num4 < num5)
				{
					reference.HorizontalDistance += num5;
					flag = true;
				}
				if (flag)
				{
					float num6 = BasePlayer.GetJumpHeight() + num2;
					if (reference.VerticalDistance > num6)
					{
						Results[num] = true;
					}
					float num7 = 5f + num3;
					if (reference.HorizontalDistance > num7)
					{
						Results[num] = true;
					}
				}
			}
			else
			{
				if (WasInAirStates[num])
				{
					reference.LastInAirTime = TimeSinceStartup;
				}
				reference.HorizontalDistance = 0f;
				reference.VerticalDistance = 0f;
			}
		}
	}
}
