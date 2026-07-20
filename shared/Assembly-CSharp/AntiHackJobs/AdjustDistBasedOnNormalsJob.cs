using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct AdjustDistBasedOnNormalsJob : IJob
{
	public NativeArray<(float Dist, float Budget)> DistAndBudget;

	public ReadOnly<Vector3> Start;

	public ReadOnly<Vector3> End;

	public ReadOnly<Vector3> Normals;

	public ReadOnly<float> DeltaTime;

	public ReadOnly<int> Indices;

	[ReadOnly]
	public float SlopeSpeed;

	public void Execute()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Span<(float, float)> span = NativeArray<(float, float)>.op_Implicit(ref DistAndBudget);
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = End[num] - Start[num];
			float num2 = Mathf.Max(0f, Vector3.Dot(Vector3Ex.XZ3D(Normals[num]), Vector3Ex.XZ3D(val))) * SlopeSpeed * DeltaTime[num];
			ref(float, float) reference = ref span[num];
			reference.Item1 = Mathf.Max(0f, reference.Item1 - num2);
		}
	}
}
