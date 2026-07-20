using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct ReadPositionDataTransformJob : IJobParallelForTransform
{
	public NativeArray<Vector3> Positions;

	public void Execute(int index, TransformAccess transform)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (((TransformAccess)(ref transform)).isValid)
		{
			Positions[index] = ((TransformAccess)(ref transform)).position;
		}
	}
}
