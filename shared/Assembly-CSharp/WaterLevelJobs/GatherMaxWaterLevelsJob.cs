using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GatherMaxWaterLevelsJob : IJob
{
	[WriteOnly]
	public NativeArray<float> WaterLevels;

	public ReadOnly<Vector3> Positions;

	public Bounds DeepSeaBounds;

	public float waterLevelMain;

	public float waterLevelDeep;

	public void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Positions.Length; i++)
		{
			WaterLevels[i] = (((Bounds)(ref DeepSeaBounds)).Contains(Positions[i]) ? waterLevelDeep : waterLevelMain);
		}
	}
}
