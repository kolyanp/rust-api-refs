using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile]
public struct SelectMaxWaterLevelJobIndirect : IJob
{
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<float> DynamicHeights;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public float OceanLevel;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			float num2 = Heights[num];
			float num3 = OceanLevel + DynamicHeights[num];
			Heights[num] = Mathf.Max(num2, num3);
		}
	}
}
