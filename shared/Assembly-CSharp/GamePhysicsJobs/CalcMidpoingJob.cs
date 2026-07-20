using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct CalcMidpoingJob : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Results;

	[ReadOnly]
	public ReadOnly<Vector3> From;

	[ReadOnly]
	public ReadOnly<Vector3> To;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < From.Length; i++)
		{
			Vector3 val = From[i];
			Vector3 val2 = To[i];
			Results[i] = (val + val2) * 0.5f;
		}
	}
}
