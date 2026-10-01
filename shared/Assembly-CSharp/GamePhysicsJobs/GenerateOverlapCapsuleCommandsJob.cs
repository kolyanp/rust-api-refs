using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
public struct GenerateOverlapCapsuleCommandsJob : IJob
{
	[WriteOnly]
	public NativeArray<OverlapCapsuleCommand> CapsuleCommands;

	[ReadOnly]
	public ReadOnly<Vector3> From;

	[ReadOnly]
	public ReadOnly<Vector3> To;

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
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < From.Length; i++)
		{
			QueryParameters val = new QueryParameters(LayerMasks[i], HitMultipleFaces, TriggerInteraction, HitBackfaces);
			CapsuleCommands[i] = new OverlapCapsuleCommand(From[i], To[i], Radiii[i], val);
		}
	}
}
