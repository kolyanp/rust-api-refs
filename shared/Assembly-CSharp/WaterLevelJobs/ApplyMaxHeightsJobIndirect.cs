using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ApplyMaxHeightsJobIndirect : IJob
{
	public NativeArray<float> Heights;

	[ReadOnly]
	public ReadOnly<int> Topologies;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public ReadOnly<float> WaterLevels;

	[ReadOnly]
	public float OceanLevel;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			bool num2 = Heights[num] < WaterLevels[num];
			bool flag = (Topologies[num] & 0x180) != 0;
			if (num2 & flag)
			{
				Heights[num] = Math.Max(Heights[num], OceanLevel);
			}
		}
	}
}
