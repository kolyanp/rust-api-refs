using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ApplyHeadQueryResultsJobIndirect : IJob
{
	public NativeArray<float> WaterHeights;

	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<bool> ValidInfos;

	[ReadOnly]
	public ReadOnly<Vector3> Starts;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public unsafe void Execute()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			float num2 = WaterHeights[num];
			if (ValidInfos[num])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Infos), num).isValid = false;
				num2 = -1000f;
			}
			else
			{
				num2 = Mathf.Min(num2, Starts[num].y);
			}
			WaterHeights[num] = num2;
		}
	}
}
