using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct InitialValidateInfoJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<WaterLevel.WaterInfo> Results;

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

	public void Execute()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 val = Starts[num];
			Vector3 val2 = Ends[num];
			float num2 = Radii[num];
			float minY = Mathf.Min(val.y, val2.y) - num2;
			float maxY = Mathf.Max(val.y, val2.y) + num2;
			Results[num] = WaterLevel.InitialValidate(minY, maxY, WaterHeights[num], TerrainHeights[num]);
		}
	}
}
