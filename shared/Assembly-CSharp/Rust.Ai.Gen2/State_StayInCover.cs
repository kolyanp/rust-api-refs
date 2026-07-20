using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
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
		Matrix4x4 navMeshToWorldSpace = Owner.NavMeshToWorldSpace;
		Vector3 val2;
		if (base.Senses.FindLKP(target, out var lkp2) && Vector3.Distance(lkp2, position) < 40f && Mathf.Abs(lkp2.y - position.y) < 10f)
		{
			Matrix4x4 worldToNavMeshSpace = Owner.WorldToNavMeshSpace;
			Vector3 positionNS = ((Matrix4x4)(ref worldToNavMeshSpace)).MultiplyPoint(lkp2);
			if (base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f) && base.Agent.CalculatePath(((NavMeshHit)(ref hitNS)).position, PathToLkp) && PathToLkp.corners.Count >= 2)
			{
				Vector3 val = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyPoint(PathToLkp.corners[1]);
				RustNavMeshAgent agent = base.Agent;
				val2 = val - position;
				agent.overrideDirectionWS = ((Vector3)(ref val2)).normalized;
			}
		}
		if (!base.Agent.overrideDirectionWS.HasValue && base.Agent.lastValidPath.Count >= 2)
		{
			List<Vector3> lastValidPath = base.Agent.lastValidPath;
			Vector3 val3 = lastValidPath[lastValidPath.Count - 1];
			List<Vector3> lastValidPath2 = base.Agent.lastValidPath;
			val2 = val3 - lastValidPath2[lastValidPath2.Count - 2];
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			Vector3 value = ((Matrix4x4)(ref navMeshToWorldSpace)).MultiplyVector(normalized);
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
