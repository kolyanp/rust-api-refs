using System;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_CrocCharge : FSMStateBase
{
	private const float maxChargeDuration = 6f;

	private float remainingChargeDuration;

	private Action _reallowChargingAction;

	private Action _surpriseAction;

	private Action _resetSurpriseAction;

	private double nextSurpriseTime;

	private Action ReallowChargingAction => ResetStamina;

	private Action SurpriseAction => Surprise;

	private Action ResetSurpriseAction => ResetSurprise;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		ResetStamina();
		ResetSurprise();
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
		}
		base.Agent.ResetPath();
		if (!GetMoveDestination(out var destination) || !base.Agent.SetDestinationWithParams(destination, autoBraking: false))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
	}

	private void ResetStamina()
	{
		remainingChargeDuration = 6f;
	}

	private void ResetSurprise()
	{
		float num = Random.Range(3f, 6f);
		Owner.Invoke(SurpriseAction, num);
		Owner.Invoke(ResetSurpriseAction, num + Random.Range(3f, 6f));
	}

	private void Surprise()
	{
		nextSurpriseTime = Time.timeAsDouble + (double)Random.Range(3f, 6f);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Success;
		}
		base.Agent._acceleration.Value = 20f;
		BaseCombatEntity baseCombatEntity = Owner as BaseCombatEntity;
		float num = Mathx.RemapValClamped(baseCombatEntity.healthFraction, 1f, 0.3f, 0f, 1f);
		bool flag = false;
		BasePlayer player;
		if (base.Agent.IsSwimming)
		{
			flag = true;
			base.Agent._acceleration.Value = 2.5f;
			num = 1f;
		}
		else if (baseCombatEntity.lastAttackedTime > 0f && Time.time < baseCombatEntity.lastAttackedTime + 0.5f)
		{
			flag = true;
			num = 1f;
		}
		else if (Time.timeAsDouble > nextSurpriseTime && Time.timeAsDouble < nextSurpriseTime + 0.5)
		{
			num = 1f;
		}
		else if (base.Agent.remainingDistance < 4f)
		{
			flag = true;
			num = 1f;
		}
		else if (target.ToNonNpcPlayer(out player) && player.modelState.sprinting)
		{
			Vector3 estimatedVelocity = player.estimatedVelocity;
			if (Vector3.Dot(((Vector3)(ref estimatedVelocity)).normalized, ((Component)Owner).transform.forward) > 0.5f)
			{
				base.Agent._acceleration.Value = 2f;
				num = 1f;
			}
		}
		if (!flag && num >= 1f)
		{
			float num2 = remainingChargeDuration;
			remainingChargeDuration -= deltaTime;
			if (num2 > 0f && remainingChargeDuration <= 0f)
			{
				Owner.Invoke(ReallowChargingAction, 6f);
			}
		}
		if (!flag && remainingChargeDuration <= 0f)
		{
			num = Mathf.Min(num, 0.3f);
		}
		base.Agent.SetSpeedRatio(num, RustNavMeshAgent.Speeds.Sneak, RustNavMeshAgent.Speeds.FullSprint);
		if (base.Senses.GetVisibilityStatus(target, out var status) && status.isInWaterCached)
		{
			base.Agent.desiredSwimDepth.Value = Mathf.Max(base.Agent.desiredSwimDepth.DefaultValue, status.lastWaterInfo.Value.currentDepth - 1f);
		}
		if (!GetMoveDestination(out var destination) || !base.Agent.SetDestinationWithParams(destination, autoBraking: false))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateUpdate(deltaTime);
	}

	private bool GetMoveDestination(out NavVector3 destination)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		destination = default(NavVector3);
		if (!base.Senses.FindTarget(out var target))
		{
			return false;
		}
		Vector3 positionWS = ((Component)target).transform.position;
		if (target.IsNonNpcPlayer() && base.Agent.canSwim && base.Senses.GetVisibilityStatus(target, out var status) && status.isInWaterCached)
		{
			positionWS = Vector3Ex.WithY(((Component)target).transform.position, status.lastWaterInfo.Value.terrainHeight);
		}
		destination = base.Agent.WorldToNavSpace(positionWS);
		return true;
	}
}
