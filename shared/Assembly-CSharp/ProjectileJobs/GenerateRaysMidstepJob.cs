using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace ProjectileJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
internal struct GenerateRaysMidstepJob : IJobParallelForTransform
{
	public ReadOnly<Vector3> PositionData;

	public ReadOnly<int> Indices;

	public ReadOnly<RayGenBatchData> Data;

	public NativeArray<RayGenOutput> Out;

	public float Time;

	public float DeltaTime;

	public bool IsClientDemo;

	public void Execute(int index, TransformAccess transform)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (((TransformAccess)(ref transform)).isValid && Indices.Contains(index))
		{
			RayGenBatchData data = Data[index];
			Vector3 position = PositionData[index];
			Out[index] = RayGenUtil.GenerateRayGenOutput(transform, in data, position, Time, DeltaTime, IsClientDemo);
		}
	}
}
