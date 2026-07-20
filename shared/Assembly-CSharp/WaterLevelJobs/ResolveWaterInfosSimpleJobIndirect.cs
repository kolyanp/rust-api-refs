using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ResolveWaterInfosSimpleJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<Vector3> Poses;

	[ReadOnly]
	public ReadOnly<float> WaterHeights;

	[ReadOnly]
	public ReadOnly<float> TerrainHeights;

	[ReadOnly]
	public ReadOnly<bool> UseVolumeDepths;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public unsafe void Execute()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = Poses[num];
			float num2 = WaterHeights[num];
			float num3 = TerrainHeights[num];
			ref WaterLevel.WaterInfo reference = ref UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Infos), num);
			reference.currentDepth = Mathf.Max(0f, num2 - val.y);
			if (!UseVolumeDepths[num])
			{
				reference.overallDepth = Mathf.Max(0f, num2 - num3);
			}
			reference.surfaceLevel = num2;
			reference.terrainHeight = num3;
		}
	}
}
