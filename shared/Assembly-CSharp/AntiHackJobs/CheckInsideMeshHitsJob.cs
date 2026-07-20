using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CheckInsideMeshHitsJob : IJobFor
{
	[WriteOnly]
	public NativeArray<bool> Results;

	public ReadOnly<RaycastHit> Hits;

	public void Execute(int index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = Hits[index];
		Results[index] = ((RaycastHit)(ref val)).colliderInstanceID != 0 && Vector3.Dot(Vector3.up, ((RaycastHit)(ref val)).normal) > 0f;
	}
}
