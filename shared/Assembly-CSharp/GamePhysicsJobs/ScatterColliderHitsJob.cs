using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct ScatterColliderHitsJob : IJob
{
	[WriteOnly]
	public NativeArray<ColliderHit> To;

	[ReadOnly]
	public ReadOnly<ColliderHit> From;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public int MaxHitsPerRay;

	public void Execute()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = i * MaxHitsPerRay;
			int num2 = Indices[i] * MaxHitsPerRay;
			NativeArray<ColliderHit>.Copy(From, num, To, num2, MaxHitsPerRay);
		}
	}
}
