using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapSphereCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapSphereCommand> SphereCommands;

	[ReadOnly]
	public ReadOnly<Vector3> Pos;

	[ReadOnly]
	public ReadOnly<int> LayerMasks;

	[ReadOnly]
	public ReadOnly<float> Radiii;

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
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		QueryParameters val = default(QueryParameters);
		for (int i = 0; i < Pos.Length; i++)
		{
			((QueryParameters)(ref val))._002Ector(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			SphereCommands[i] = new OverlapSphereCommand(Pos[i], Radiii[i], val);
		}
	}
}
