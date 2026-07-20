using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct CheckHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<bool> Results;

	[ReadOnly]
	public ReadOnly<ColliderHit> Hits;

	public void Execute()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Hits.Length; i++)
		{
			ref NativeArray<bool> results = ref Results;
			int num = i;
			ColliderHit val = Hits[i];
			results[num] = ((ColliderHit)(ref val)).instanceID != 0;
		}
	}
}
