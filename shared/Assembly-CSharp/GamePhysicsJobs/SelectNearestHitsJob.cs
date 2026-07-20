using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SelectNearestHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<RaycastHit> Results;

	[ReadOnly]
	public ReadOnly<RaycastHit> Hits;

	[ReadOnly]
	public int HitsPerBatch;

	public void Execute()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (HitsPerBatch < 1)
		{
			Debug.LogError((object)$"Invalid HitsPerBatch: {HitsPerBatch}");
			return;
		}
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = -1;
			float num3 = float.MaxValue;
			for (int j = 0; j < HitsPerBatch; j++)
			{
				RaycastHit val = Hits[i * HitsPerBatch + j];
				if (((RaycastHit)(ref val)).normal == Vector3.zero)
				{
					break;
				}
				if (((RaycastHit)(ref val)).distance < num3)
				{
					num3 = ((RaycastHit)(ref val)).distance;
					num2 = j;
				}
			}
			if (num2 != -1)
			{
				Results[i] = Hits[i * HitsPerBatch + num2];
			}
			else
			{
				Results[i] = default(RaycastHit);
			}
		}
	}
}
