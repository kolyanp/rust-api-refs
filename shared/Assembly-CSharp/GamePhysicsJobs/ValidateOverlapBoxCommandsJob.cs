using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct ValidateOverlapBoxCommandsJob : IJob
{
	[WriteOnly]
	public NativeList<int> InvalidIndices;

	[ReadOnly]
	public ReadOnly<OverlapBoxCommand> Commands;

	public void Execute()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Commands.Length; i++)
		{
			OverlapBoxCommand val = Commands[i];
			if (Vector3Ex.IsNaNOrInfinity(((OverlapBoxCommand)(ref val)).halfExtents))
			{
				InvalidIndices.AddNoResize(i);
			}
			else if (((OverlapBoxCommand)(ref val)).halfExtents.x <= 0f || ((OverlapBoxCommand)(ref val)).halfExtents.y <= 0f || ((OverlapBoxCommand)(ref val)).halfExtents.z <= 0f)
			{
				InvalidIndices.AddNoResize(i);
			}
		}
	}
}
