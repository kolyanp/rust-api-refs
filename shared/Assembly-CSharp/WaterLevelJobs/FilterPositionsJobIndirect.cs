using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct FilterPositionsJobIndirect : IJob
{
	[WriteOnly]
	public NativeList<int> OverworldIndices;

	[WriteOnly]
	public NativeList<int> DeepSeaIndices;

	public ReadOnly<Vector3> Positions;

	public ReadOnly<int> Indices;

	public Bounds DeepSeaBounds;

	public void Execute()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (((Bounds)(ref DeepSeaBounds)).Contains(Positions[num]))
			{
				DeepSeaIndices.AddNoResize(num);
			}
			else
			{
				OverworldIndices.AddNoResize(num);
			}
		}
	}
}
