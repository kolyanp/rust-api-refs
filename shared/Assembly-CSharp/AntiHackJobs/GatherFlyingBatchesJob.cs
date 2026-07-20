using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GatherFlyingBatchesJob : IJob
{
	[WriteOnly]
	public NativeList<Vector3> From;

	[WriteOnly]
	public NativeList<Vector3> To;

	[WriteOnly]
	public NativeList<Vector3> CheckPoses;

	public NativeArray<AntiHack.FlyingBatch> Batches;

	[ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	public ReadOnly<Matrix4x4> Matrices;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public int MaxSteps;

	[ReadOnly]
	public float DefaultStepSize;

	[ReadOnly]
	public int Protection;

	public void Execute()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int playerIndex = Indices[i];
			TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, playerIndex);
			Matrix4x4 val = Matrices[i];
			bool flag = ((Matrix4x4)(ref val))[15] == 0f;
			Vector3 val2 = (flag ? playerTickIterator.StartPoint : ((Matrix4x4)(ref val)).MultiplyPoint3x4(playerTickIterator.StartPoint));
			Vector3 val3 = (flag ? playerTickIterator.EndPoint : ((Matrix4x4)(ref val)).MultiplyPoint3x4(playerTickIterator.EndPoint));
			AntiHack.FlyingBatch flyingBatch = Batches[i];
			flyingBatch.PlayerIndex = playerIndex;
			playerTickIterator.Reset();
			if (!playerTickIterator.HasNext())
			{
				continue;
			}
			if (Protection >= 3)
			{
				float distance = Mathf.Max(playerTickIterator.Length / (float)MaxSteps, DefaultStepSize);
				int num2 = 0;
				while (playerTickIterator.MoveNext(distance))
				{
					val3 = (flag ? playerTickIterator.CurrentPoint : ((Matrix4x4)(ref val)).MultiplyPoint3x4(playerTickIterator.CurrentPoint));
					From.AddNoResize(val2);
					To.AddNoResize(val3);
					CheckPoses.AddNoResize((val2 + val3) * 0.5f);
					val2 = val3;
					num2++;
					num++;
				}
				flyingBatch.Count = num2;
			}
			else
			{
				From.AddNoResize(val2);
				To.AddNoResize(val3);
				CheckPoses.AddNoResize((val2 + val3) * 0.5f);
				flyingBatch.Count = 1;
				num++;
			}
			Batches[i] = flyingBatch;
		}
	}
}
