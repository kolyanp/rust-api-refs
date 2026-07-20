using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GatherNoClipBatchesJob : IJob
{
	[WriteOnly]
	public NativeList<Vector3> From;

	[WriteOnly]
	public NativeList<Vector3> To;

	public NativeArray<AntiHack.Batch> Batches;

	[ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	public ReadOnly<Matrix4x4> Matrices;

	public ReadOnly<int> Indices;

	public ReadOnly<float> DeltaTimes;

	[ReadOnly]
	public int MaxSteps;

	[ReadOnly]
	public float DefaultStepSize;

	[ReadOnly]
	public float LagThreshold;

	[ReadOnly]
	public bool TickBufferPrevention;

	[ReadOnly]
	public float MaxTickCount;

	[ReadOnly]
	public int DefaultProtection;

	public void Execute()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = BasePlayer.NoClipOffset();
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, num);
			Matrix4x4 val2 = Matrices[i];
			bool flag = ((Matrix4x4)(ref val2))[15] == 0f;
			Vector3 val3 = (flag ? playerTickIterator.StartPoint : ((Matrix4x4)(ref val2)).MultiplyPoint3x4(playerTickIterator.StartPoint));
			Vector3 val4 = (flag ? playerTickIterator.EndPoint : ((Matrix4x4)(ref val2)).MultiplyPoint3x4(playerTickIterator.EndPoint));
			AntiHack.Batch batch = Batches[i];
			bool num2 = DeltaTimes[num] < LagThreshold && TickBufferPrevention;
			int count = batch.Count;
			int num3 = DefaultProtection;
			if (num2 && (float)count >= MaxTickCount)
			{
				num3 = Mathf.Min(2, num3);
			}
			if (num3 >= 3)
			{
				float distance = Mathf.Max(playerTickIterator.Length / (float)MaxSteps, DefaultStepSize);
				int num4 = 0;
				while (playerTickIterator.MoveNext(distance))
				{
					num4++;
					val4 = (flag ? playerTickIterator.CurrentPoint : ((Matrix4x4)(ref val2)).MultiplyPoint3x4(playerTickIterator.CurrentPoint));
					From.AddNoResize(val3 + val);
					To.AddNoResize(val4 + val);
					val3 = val4;
				}
				batch.Count = num4;
			}
			else
			{
				From.AddNoResize(val3 + val);
				To.AddNoResize(val4 + val);
				batch.Count = 1;
			}
			Batches[i] = batch;
		}
	}
}
