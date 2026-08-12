using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
internal struct AppendRaycastHitsJob : IJob
{
	public NativeArray<RaycastHit> Dst;

	public ReadOnly<RaycastHit> Src;

	public int DstMaxHitsPerBatch;

	public int SrcMaxHitsPerBatch;

	public void Execute()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (Src.Length == 0 || Dst.Length == 0)
		{
			return;
		}
		int num = Src.Length / SrcMaxHitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = GamePhysicsJobs.Util.FindFreeSlot(i, in Dst, DstMaxHitsPerBatch, out var endInd);
			int num3 = i * SrcMaxHitsPerBatch;
			int num4 = num3 + SrcMaxHitsPerBatch;
			while (num2 < endInd && num3 < num4)
			{
				RaycastHit val = Src[num3++];
				if (((RaycastHit)(ref val)).normal == Vector3.zero)
				{
					break;
				}
				Dst[num2++] = val;
			}
			if (num2 < endInd)
			{
				Dst[num2] = default(RaycastHit);
			}
		}
	}
}
