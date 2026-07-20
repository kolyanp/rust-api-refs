using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_CircleDynamic : FSMStateBase
{
	[SerializeField]
	private RustNavMeshAgent.Speeds minSpeed;

	[SerializeField]
	private RustNavMeshAgent.Speeds maxSpeed = RustNavMeshAgent.Speeds.Sprint;

	[SerializeField]
	protected Vector2 distanceSpeedRange = new Vector2(10f, 50f);

	[SerializeField]
	private Vector2 angleRange = Vector2.op_Implicit(new Vector3(20f, 80f));

	[SerializeField]
	private Vector2 angleDurationRange = new Vector2(1f, 3f);

	[SerializeField]
	private Vector2 burstDurationRange = new Vector2(1f, 3f);

	[SerializeField]
	private Vector2 burstCooldownRange = new Vector2(1f, 10f);

	private Action _updateBurstAction;

	private Action _endBurstAction;

	private Action _updateAngleAction;

	private bool clockWise = true;

	private int burstSpeedIndexOffset;

	private float randomAngle;

	private Action UpdateBurstAction => UpdateBurst;

	private Action EndBurstAction => EndBurst;

	private Action UpdateAngleAction => UpdateAngle;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if ((Object)(object)payload.entity != (Object)null)
		{
			base.Senses.TrySetTarget(payload.entity);
		}
		clockWise = Random.value > 0.5f;
		EndBurst();
		UpdateAngle();
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		Owner.CancelInvoke(UpdateBurstAction);
		Owner.CancelInvoke(EndBurstAction);
		Owner.CancelInvoke(UpdateAngleAction);
		base.OnStateExit();
	}

	private void UpdateAngle()
	{
		randomAngle = Random.Range(angleRange.x, angleRange.y) * (float)(clockWise ? 1 : (-1));
		Owner.Invoke(UpdateAngleAction, Random.Range(angleDurationRange.x, angleDurationRange.y));
	}

	private void UpdateBurst()
	{
		burstSpeedIndexOffset = 2;
		clockWise = Random.value > 0.5f;
		float time = Random.Range(burstDurationRange.x, burstDurationRange.y);
		Owner.Invoke(EndBurstAction, time);
	}

	private void EndBurst()
	{
		burstSpeedIndexOffset = 0;
		float time = Random.Range(burstCooldownRange.x, burstCooldownRange.y);
		Owner.Invoke(UpdateBurstAction, time);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target))
		{
			return EFSMStateStatus.Failure;
		}
		Vector3 position = ((Component)target).transform.position;
		float num = Vector3.Distance(((Component)Owner).transform.position, position);
		float normalizedDist = Mathf.InverseLerp(distanceSpeedRange.x, distanceSpeedRange.y, num);
		SetSpeed(target, num, normalizedDist);
		float value = Mathx.RemapValClamped(num, distanceSpeedRange.x, distanceSpeedRange.y, randomAngle, 0f);
		Vector3 targetPositionNS = position;
		RustNavMeshAgent agent = base.Agent;
		float? deviation = value;
		if (!agent.SetDestinationWithParams(targetPositionNS, autoBraking: true, null, null, null, deviation))
		{
			return EFSMStateStatus.Failure;
		}
		return base.OnStateUpdate(deltaTime);
	}

	protected virtual void SetSpeed(BaseEntity target, float distToTarget, float normalizedDist)
	{
		base.Agent.SetSpeedRatio(normalizedDist, minSpeed, maxSpeed, burstSpeedIndexOffset);
	}
}
