using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct FilterRaycastHitsJob : IJob
{
	public NativeList<RaycastHit> ColliderHits;

	public NativeList<int> ColliderIndices;

	public NativeList<Vector3> WaterHits;

	public NativeList<int> WaterIndices;

	public ReadOnly<RaycastHit> Hits;

	public int HitsPerBatch;

	public void Execute()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		int num = Hits.Length / HitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * HitsPerBatch;
			int num3 = num2 + HitsPerBatch;
			for (int j = num2; j < num3; j++)
			{
				RaycastHit val = Hits[j];
				if (((RaycastHit)(ref val)).normal == Vector3.zero)
				{
					break;
				}
				if (((RaycastHit)(ref val)).colliderInstanceID != 0)
				{
					ColliderHits.AddNoResize(val);
					ColliderIndices.AddNoResize(j);
				}
				else
				{
					WaterHits.AddNoResize(((RaycastHit)(ref val)).point);
					WaterIndices.AddNoResize(j);
				}
			}
		}
	}
}
