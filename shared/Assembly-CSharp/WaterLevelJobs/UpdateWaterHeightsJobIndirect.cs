using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct UpdateWaterHeightsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> WaterHeights;

	[ReadOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public NativeArray<int> Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (Infos[num].isValid)
			{
				WaterHeights[num] = Infos[num].surfaceLevel;
			}
		}
	}
}
