using System;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_CrocHasStraightPathToTarget : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_CrocHasStraightPathToTarget"))
		{
			if (!base.Senses.FindTarget(out var target))
			{
				return false;
			}
			Vector3 targetPositionNS = ((Component)target).transform.position;
			if (target.IsNonNpcPlayer() && base.Agent.canSwim && base.Senses.GetVisibilityStatus(target, out var status) && status.isInWaterCached)
			{
				targetPositionNS = Vector3Ex.WithY(((Component)target).transform.position, status.lastWaterInfo.Value.terrainHeight);
			}
			NavMeshHit hitNS;
			return !base.Agent.Raycast(targetPositionNS, out hitNS);
		}
	}
}
