using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ResolveIgnoreWaterInfosJob : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[WriteOnly]
	public NativeArray<float> WaterHeights;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public ReadOnly<bool> Results;

	public unsafe void Execute()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Infos.Length; i++)
		{
			int num = Indices[i];
			if (Results[num])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Infos), num).isValid = false;
				WaterHeights[num] = -1000f;
			}
		}
	}
}
