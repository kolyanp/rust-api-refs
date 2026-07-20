using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct SortHitsJob<CompT> : IJobFor where CompT : unmanaged, IComparer<RaycastHit>
{
	[NativeDisableParallelForRestriction]
	public NativeArray<RaycastHit> Hits;

	[ReadOnly]
	public CompT Comp;

	[ReadOnly]
	public int MaxHitsPerRay;

	public void Execute(int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = index * MaxHitsPerRay;
		for (int i = 0; i < MaxHitsPerRay; i++)
		{
			RaycastHit val = Hits[num2 + i];
			if (((RaycastHit)(ref val)).normal == Vector3.zero)
			{
				break;
			}
			num++;
		}
		if (num > 1)
		{
			NativeSortExtension.Sort<RaycastHit, CompT>(Hits.GetSubArray(num2, num), Comp);
		}
	}
}
