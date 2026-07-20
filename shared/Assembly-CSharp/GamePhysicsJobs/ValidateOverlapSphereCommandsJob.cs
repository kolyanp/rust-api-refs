using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct ValidateOverlapSphereCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[ReadOnly]
	public ReadOnly<OverlapSphereCommand> Commands;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapSphereCommand val = Commands[i];
			if (((OverlapSphereCommand)(ref val)).radius < 0f)
			{
				InvalidIndices.AddNoResize(i);
			}
		}
	}
}
