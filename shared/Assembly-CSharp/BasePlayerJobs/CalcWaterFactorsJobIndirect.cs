using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalcWaterFactorsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Factors;

	[ReadOnly]
	public ReadOnly<int> Indices;

	[ReadOnly]
	public ReadOnly<WaterLevel.WaterInfo> Infos;

	[ReadOnly]
	public ReadOnly<Vector3> Starts;

	[ReadOnly]
	public ReadOnly<Vector3> Ends;

	[ReadOnly]
	public ReadOnly<float> Radii;

	public void Execute()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Factors[num] = WaterLevel.Factor(Infos[num], Starts[num], Ends[num], Radii[num]);
		}
	}
}
