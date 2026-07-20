using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapBoxCommandsFromOBBsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapBoxCommand> BoxCommands;

	[ReadOnly]
	public ReadOnly<OBB> OBBs;

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
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		QueryParameters val = default(QueryParameters);
		for (int i = 0; i < OBBs.Length; i++)
		{
			((QueryParameters)(ref val))._002Ector(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			BoxCommands[i] = new OverlapBoxCommand(OBBs[i].position, OBBs[i].extents, OBBs[i].rotation, val);
		}
	}
}
