using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct PosRotReadJob : IJobParallelForTransform
{
	public NativeArray<Vector3> Positions;

	public NativeArray<Quaternion> Rotations;

	public void Execute(int index, TransformAccess transform)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (((TransformAccess)(ref transform)).isValid)
		{
			Vector3 val = default(Vector3);
			Quaternion val2 = default(Quaternion);
			((TransformAccess)(ref transform)).GetPositionAndRotation(ref val, ref val2);
			Positions[index] = val;
			Rotations[index] = val2;
		}
	}
}
