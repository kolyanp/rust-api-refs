using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SelectNearestNHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<RaycastHit> Results;

	[ReadOnly]
	public ReadOnly<RaycastHit> Hits;

	[ReadOnly]
	public int HitsPerBatch;

	[ReadOnly]
	public int SelectCount;

	public void Execute()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (HitsPerBatch < 1)
		{
			Debug.LogError((object)$"Invalid HitsPerBatch: {HitsPerBatch}");
			return;
		}
		if (SelectCount > HitsPerBatch)
		{
			Debug.LogError((object)$"Invalid SelectCount: {SelectCount}");
			return;
		}
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < SelectCount; j++)
			{
				RaycastHit val = Hits[i * HitsPerBatch + j];
				if (((RaycastHit)(ref val)).normal == Vector3.zero)
				{
					Results[i * SelectCount + j] = default(RaycastHit);
					break;
				}
				Results[i * SelectCount + j] = val;
			}
		}
	}
}
