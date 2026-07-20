using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GenerateRaysJob : IJobParallelForTransform
{
	public ReadOnly<int> Indices;

	public ReadOnly<RayGenBatchData> Data;

	public NativeArray<Vector3> PositionData;

	public NativeArray<RayGenOutput> Out;

	public float Time;

	public float DeltaTime;

	public bool IsClientDemo;

	public void Execute(int index, TransformAccess transform)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (((TransformAccess)(ref transform)).isValid && Indices.Contains(index))
		{
			RayGenBatchData data = Data[index];
			Vector3 val = (PositionData[index] = ((TransformAccess)(ref transform)).position);
			Vector3 position2 = val;
			Out[index] = RayGenUtil.GenerateRayGenOutput(transform, in data, position2, Time, DeltaTime, IsClientDemo);
		}
	}
}
