using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
internal struct BakePhysicsMeshesJob : IJobParallelFor
{
	public ReadOnly<int> MeshIds;

	public ReadOnly<bool> Convex;

	public void Execute(int index)
	{
		Physics.BakeMesh(MeshIds[index], Convex[index]);
	}
}
