using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapBoxCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapBoxCommand> BoxCommands;

	[ReadOnly]
	public ReadOnly<Vector3> Centers;

	[ReadOnly]
	public ReadOnly<Vector3> Extents;

	[ReadOnly]
	public ReadOnly<int> LayerMasks;

	[ReadOnly]
	public QueryTriggerInteraction TriggerInteraction;

	[ReadOnly]
	public bool HitMultipleFaces;

	[ReadOnly]
	public bool HitBackfaces;

	public void Execute()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		QueryParameters val = default(QueryParameters);
		for (int i = 0; i < Centers.Length; i++)
		{
			((QueryParameters)(ref val))._002Ector(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			BoxCommands[i] = new OverlapBoxCommand(Centers[i], Extents[i], Quaternion.identity, val);
		}
	}
}
