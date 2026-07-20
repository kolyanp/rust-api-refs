using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ResolveWaterInfosJobIndirect : IJob
{
	public NativeArray<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<Vector3> Starts;

	[ReadOnly]
	public ReadOnly<Vector3> Ends;

	[ReadOnly]
	public ReadOnly<float> Radii;

	[ReadOnly]
	public ReadOnly<float> WaterHeights;

	[ReadOnly]
	public ReadOnly<float> TerrainHeights;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public unsafe void Execute()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = Starts[num];
			Vector3 val2 = Ends[num];
			float num2 = Radii[num];
			float num3 = Mathf.Min(val.y, val2.y) - num2;
			float num4 = WaterHeights[num];
			float num5 = TerrainHeights[num];
			ref WaterLevel.WaterInfo reference = ref UnsafeUtility.ArrayElementAsRef<WaterLevel.WaterInfo>(NativeArrayUnsafeUtility.GetUnsafePtr<WaterLevel.WaterInfo>(Infos), num);
			reference.currentDepth = Mathf.Max(0f, num4 - num3);
			reference.overallDepth = Mathf.Max(0f, num4 - num5);
			reference.surfaceLevel = num4;
			reference.terrainHeight = num5;
		}
	}
}
