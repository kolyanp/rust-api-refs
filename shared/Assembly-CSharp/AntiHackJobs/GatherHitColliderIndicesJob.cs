using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct GatherHitColliderIndicesJob : IJob
{
	public NativeList<int> Results;

	public ReadOnly<ColliderHit> Hits;

	[ReadOnly]
	public int ResultsPerQuery;

	public void Execute()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = Hits.Length / ResultsPerQuery;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * ResultsPerQuery;
			for (int j = 0; j < ResultsPerQuery; j++)
			{
				ColliderHit val = Hits[num2 + j];
				if (((ColliderHit)(ref val)).instanceID == 0)
				{
					break;
				}
				Results.AddNoResize(num2 + j);
			}
		}
	}
}
