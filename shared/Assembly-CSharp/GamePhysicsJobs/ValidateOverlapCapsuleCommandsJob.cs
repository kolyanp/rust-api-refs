using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ValidateOverlapCapsuleCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[ReadOnly]
	public ReadOnly<OverlapCapsuleCommand> Commands;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapCapsuleCommand val = Commands[i];
			if (!(((OverlapCapsuleCommand)(ref val)).radius <= 0f))
			{
				Vector3 val2 = ((OverlapCapsuleCommand)(ref val)).point1 - ((OverlapCapsuleCommand)(ref val)).point0;
				if (!(((Vector3)(ref val2)).magnitude / 2f <= 0f))
				{
					continue;
				}
			}
			InvalidIndices.AddNoResize(i);
		}
	}
}
