using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_ScientistRush : State_MoveToTarget
{
	public bool useSuppressiveFire;

	private NpcZoneComponent _npcZoneComponent;

	private NpcShootingComponent _shooting;

	private NpcZoneComponent NpcZoneComponent => _npcZoneComponent ?? (_npcZoneComponent = ((Component)Owner).GetComponent<NpcZoneComponent>());

	private NpcShootingComponent Shooting => _shooting ?? (_shooting = ((Component)Owner).GetComponent<NpcShootingComponent>());

	protected override bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		destination = default(NavVector3);
		if (!base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: false, predict: true))
		{
			return false;
		}
		NavVector3 positionNS = base.Agent.WorldToNavSpace(lkp);
		if (!base.Agent.SamplePosition(positionNS, out var hitNS, 3.5f) && !base.Agent.SamplePosition(positionNS, out hitNS, 20f))
		{
			return false;
		}
		Vector3 positionWS = base.Agent.NavToWorldSpace(hitNS.position);
		if (base.Agent.IsInWater(positionWS))
		{
			return false;
		}
		destination = hitNS.position;
		return true;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload assistRequest)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Shooting.OnlyShootIfTargetIsVisible = !useSuppressiveFire;
		if ((Object)(object)assistRequest.entity != (Object)null && assistRequest.position.HasValue)
		{
			base.Senses.SimulateSighting(assistRequest.entity, assistRequest.position.Value);
			base.Senses.TrySetTarget(assistRequest.entity);
		}
		else
		{
			NpcPushHelper.CoordinatePush(Owner);
		}
		return base.OnStateEnter(assistRequest);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (useSuppressiveFire && base.Senses.FindTargetLKP(out var lkp, applyHeightOffset: true, predict: true))
		{
			if (!base.Senses.IsLineOccluded(base.Senses.EyePosition, lkp, 1218519041))
			{
				RustNavMeshAgent agent = base.Agent;
				Vector3 val = lkp - base.Senses.EyePosition;
				agent.overrideDirectionWS = ((Vector3)(ref val)).normalized;
				Shooting.OnlyShootIfTargetIsVisible = true;
			}
			else
			{
				base.Agent.overrideDirectionWS = null;
				Shooting.OnlyShootIfTargetIsVisible = false;
			}
		}
		return base.OnStateUpdate(deltaTime);
	}

	public override void OnStateExit()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!NpcZoneComponent.IsPointInsideZone(((Component)Owner).transform.position))
		{
			NpcZoneComponent.AbandonZone();
		}
		Shooting.OnlyShootIfTargetIsVisible = true;
		base.Agent.overrideDirectionWS = null;
		base.OnStateExit();
	}
}
