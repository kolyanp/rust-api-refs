using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(/*Could not decode attribute arguments.*/)]
public struct GenerateSphereCmdsFromCapsuleCmdsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapSphereCommand> SphereCommands;

	[ReadOnly]
	public ReadOnly<OverlapCapsuleCommand> Commands;

	[ReadOnly]
	public ReadOnly<int> Indices;

	public void Execute()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			OverlapCapsuleCommand val = Commands[num];
			OverlapSphereCommand val2 = new OverlapSphereCommand(val.point0, val.radius, val.queryParameters);
			SphereCommands[i] = val2;
		}
	}
}
