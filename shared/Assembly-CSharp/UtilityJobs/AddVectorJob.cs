using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UtilityJobs;

[BurstCompile]
public struct AddVectorJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Results;

	public ReadOnly<Vector3> Inputs;

	[ReadOnly]
	public Vector3 Modification;

	public void Execute()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Inputs.Length; i++)
		{
			Results[i] = Inputs[i] + Modification;
		}
	}
}
