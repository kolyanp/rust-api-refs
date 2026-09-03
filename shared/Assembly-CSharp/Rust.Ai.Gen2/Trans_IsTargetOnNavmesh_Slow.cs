using System;
using Rust.Ai.Gen2.Nav;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_IsTargetOnNavmesh_Slow : FSMSlowTransitionBase
{
	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_IsTargetOnNavmesh_Slow"))
		{
			if (!base.Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			NavVector3 positionNS = base.Agent.WorldToNavSpace(targetPosition);
			NavHit hitNS;
			return base.Agent.SamplePosition(positionNS, out hitNS, 2f);
		}
	}
}
