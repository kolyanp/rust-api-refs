using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct GenerateInsideMeshCommandsJob : IJobFor
{
	[WriteOnly]
	public NativeArray<RaycastCommand> Commands;

	public ReadOnly<Vector3> Posi;

	[ReadOnly]
	public float Distance;

	public void Execute(int index)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		QueryParameters val = default(QueryParameters);
		((QueryParameters)(ref val))._002Ector(65536, false, (QueryTriggerInteraction)0, true);
		Commands[index] = new RaycastCommand(Posi[index], Vector3.up, val, Distance);
	}
}
