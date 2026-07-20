using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct FindSphereCmdsInCapsuleCmdsJob : IJob
{
	[WriteOnly]
	public NativeList<int> SphereIndices;

	[ReadOnly]
	public ReadOnly<OverlapCapsuleCommand> Commands;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapCapsuleCommand val = Commands[i];
			Vector3 val2 = ((OverlapCapsuleCommand)(ref val)).point1 - ((OverlapCapsuleCommand)(ref val)).point0;
			if (((Vector3)(ref val2)).magnitude / 2f <= 0f)
			{
				SphereIndices.AddNoResize(i);
			}
		}
	}
}
