using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalcCenterJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Results;

	[ReadOnly]
	public ReadOnly<Vector3> Starts;

	[ReadOnly]
	public ReadOnly<Vector3> Ends;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Results[num] = (Starts[num] + Ends[num]) * 0.5f;
		}
	}
}
