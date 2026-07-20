using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct InitialValidateSimpleInfoJobIndirect : IJob
{
	public NativeArray<WaterLevel.WaterInfo> Results;

	[ReadOnly]
	public ReadOnly<Vector3> Poses;

	[ReadOnly]
	public ReadOnly<float> WaterHeights;

	[ReadOnly]
	public ReadOnly<float> TerrainHeights;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public unsafe void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = Poses[num];
			if (val.y > WaterHeights[num])
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Results), num).isValid = false;
			}
			else if (val.y < TerrainHeights[num] - 1f)
			{
				UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Results), num).isValid = false;
			}
		}
	}
}
