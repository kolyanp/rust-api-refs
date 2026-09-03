using System;
using System.Collections.Generic;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_StayInCover : FSMStateBase
{
	private NPCHumanoidAnimController _clientAnim;

	private RustNavMeshPath _pathToLkp;

	private NPCHumanoidAnimController ClientAnim => _clientAnim ?? (_clientAnim = ((Component)Owner).GetComponentInChildren<NPCHumanoidAnimController>());

	private RustNavMeshPath PathToLkp => _pathToLkp ?? (_pathToLkp = new RustNavMeshPath());

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 position = ((Component)Owner).transform.position;
		if (base.Senses.FindLKP(target, out var lkp, applyHeightOffset: true) && (base.Senses.CanBeSeenAtFrom(position + 1.1f * Vector3.up, lkp, "navigation") || base.Senses.CanBeSeenAtFrom(position + 0.1f * Vector3.up, lkp, "navigation")))
		{
			return EFSMStateStatus.Failure;
		}
		ClientAnim.IsCrouching = true;
		Vector3 val2;
		if (base.Senses.FindLKP(target, out var lkp2) && Vector3.Distance(lkp2, position) < 40f && Mathf.Abs(lkp2.y - position.y) < 10f)
		{
			NavVector3 positionNS = base.Agent.WorldToNavSpace(lkp2);
			if (base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f) && base.Agent.CalculatePath(hitNS.position, PathToLkp) && PathToLkp.corners.Count >= 2)
			{
				Vector3 val = base.Agent.NavToWorldSpace(PathToLkp.corners[1]);
				RustNavMeshAgent agent = base.Agent;
				val2 = val - position;
				agent.overrideDirectionWS = ((Vector3)(ref val2)).normalized;
			}
		}
		if (!base.Agent.overrideDirectionWS.HasValue && base.Agent.lastValidPath.Count >= 2)
		{
			List<NavVector3> lastValidPath = base.Agent.lastValidPath;
			NavVector3 navVector = lastValidPath[lastValidPath.Count - 1];
			List<NavVector3> lastValidPath2 = base.Agent.lastValidPath;
			NavVector3 normalized = (navVector - lastValidPath2[lastValidPath2.Count - 2]).normalized;
			Vector3 value = base.Agent.NavToWorldDirection(normalized);
			base.Agent.overrideDirectionWS = value;
			val2 = lkp - base.Senses.EyePosition;
			Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
			if (Vector3.Dot(base.Agent.overrideDirectionWS.Value, normalized2) < 0f)
			{
				base.Agent.overrideDirectionWS = normalized2;
			}
		}
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		ClientAnim.IsCrouching = false;
		base.Agent.overrideDirectionWS = null;
		base.OnStateExit();
	}
}
