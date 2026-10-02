using System;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_HasStraightPathToTarget : FSMTransitionBase
{
	[Tooltip("Aim at the spot CanReach would path to rather than at the target itself, so a target standing against an obstacle does not read as behind it.")]
	[SerializeField]
	public bool ProjectTarget;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Trans_HasStraightPathToTarget"))
		{
			if (!Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			if (ProjectTarget)
			{
				return Agent.HasStraightPathTo(targetPosition);
			}
			NavMeshHit hitWS;
			return !Agent.Raycast(targetPosition, out hitWS);
		}
	}
}
