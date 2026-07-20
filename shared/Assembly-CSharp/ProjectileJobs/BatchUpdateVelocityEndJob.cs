using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct BatchUpdateVelocityEndJob : IJobParallelForTransform
{
	public struct BatchData
	{
		public int DebugStableIndex;

		public Vector3 CurrentPosition;

		public Vector3 CurrentVelocity;

		public float TumbleSpeed;

		public Vector3 TumbleAxis;
	}

	public ReadOnly<int> BatchedIndices;

	public ReadOnly<BatchData> BatchedData;

	public float DeltaTime;

	public void Execute(int index, TransformAccess transform)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (((TransformAccess)(ref transform)).isValid && BatchedIndices.Contains(index))
		{
			BatchData batchData = BatchedData[index];
			if (index != batchData.DebugStableIndex)
			{
				throw new Exception($"{batchData.DebugStableIndex} {index}");
			}
			Quaternion val = ((!(batchData.TumbleSpeed > 0f)) ? Quaternion.LookRotation(batchData.CurrentVelocity) : (((TransformAccess)(ref transform)).rotation * Quaternion.AngleAxis(batchData.TumbleSpeed * DeltaTime, batchData.TumbleAxis)));
			((TransformAccess)(ref transform)).SetPositionAndRotation(batchData.CurrentPosition, val);
		}
	}
}
