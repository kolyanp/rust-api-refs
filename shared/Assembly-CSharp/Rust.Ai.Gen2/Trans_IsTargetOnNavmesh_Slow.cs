using System;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_IsTargetOnNavmesh_Slow : FSMSlowTransitionBase
{
	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsTargetOnNavmesh_Slow"))
		{
			if (!base.Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			NavMeshHit hitNS;
			return base.Agent.SamplePosition(targetPosition, out hitNS, 2f);
		}
	}
}
