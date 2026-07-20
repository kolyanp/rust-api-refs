using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct RemoveInvalidRaycastHitsJob : IJob
{
	public NativeArray<RaycastHit> Hits;

	public ReadOnly<bool> AreValid;

	public int HitsPerBatch;

	public void Execute()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * HitsPerBatch;
			int num3 = num2 + HitsPerBatch;
			int num4 = num2;
			RaycastHit val;
			for (int j = num2; j < num3; j++)
			{
				val = Hits[j];
				if (((RaycastHit)(ref val)).normal == Vector3.zero)
				{
					break;
				}
				if (AreValid[j])
				{
					Hits[num4++] = Hits[j];
				}
			}
			if (num4 < num3)
			{
				val = (Hits[num4] = default(RaycastHit));
			}
		}
	}
}
