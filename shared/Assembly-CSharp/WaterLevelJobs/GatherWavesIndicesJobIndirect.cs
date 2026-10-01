using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GatherWavesIndicesJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<int> WaveIndices;

	[WriteOnly]
	public NativeReference<int> WaveIndexCount;

	public ReadOnly<Vector3> Positions;

	public ReadOnly<int> Topologies;

	public ReadOnly<float> Heights;

	public ReadOnly<int> Indices;

	public ReadOnly<float> WaterLevels;

	public void Execute()
	{
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			bool flag = Heights[num] < WaterLevels[num];
			bool flag2 = (Topologies[num] & 0x180) != 0;
			if (flag & flag2)
			{
				WaveIndices[value++] = num;
			}
		}
		WaveIndexCount.Value = value;
	}
}
