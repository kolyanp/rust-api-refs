using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace BasePlayerJobs;

[BurstCompile]
public struct RecacheTransforms : IJobParallelForTransform
{
	public NativeArray<Vector3> LocalPos;

	public NativeArray<Vector3> Pos;

	public NativeArray<Quaternion> LocalRots;

	public NativeArray<Quaternion> Rots;

	public void Execute(int index, TransformAccess transf)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LocalPos[index] = ((TransformAccess)(ref transf)).localPosition;
		Pos[index] = ((TransformAccess)(ref transf)).position;
		LocalRots[index] = ((TransformAccess)(ref transf)).localRotation;
		Rots[index] = ((TransformAccess)(ref transf)).rotation;
	}
}
