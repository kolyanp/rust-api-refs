using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GatherPosFromOBBsJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Posi;

	[ReadOnly]
	public ReadOnly<OBB> OBBs;

	public void Execute()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < OBBs.Length; i++)
		{
			Posi[i] = OBBs[i].position;
		}
	}
}
