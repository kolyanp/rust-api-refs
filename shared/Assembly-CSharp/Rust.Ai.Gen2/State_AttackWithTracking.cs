using System;
using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_AttackWithTracking : State_PlayAnimationRM
{
	public DamageType DamageType = DamageType.Bite;

	public float damageDelay = 0.5f;

	public float damage = 30f;

	public float forceScale = 1f;

	public float trackingSpeed = 90f;

	public float trackingDuration = 9999f;

	public float radius = 4f;

	public bool doesStrafeDodge;

	private Action _doDamageAction;

	private static readonly Vector3 force = new Vector3(15f, 3f, 15f);

	private double startTime;

	private Action DoDamageAction => _doDamageAction ?? (_doDamageAction = DoDamage);

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		FaceTarget = false;
		startTime = Time.timeAsDouble;
		Owner.Invoke(DoDamageAction, damageDelay + AI.defaultInterpolationDelay);
		return base.OnStateEnter(payload);
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (trackingDuration > 0f && trackingSpeed > 0f && Time.timeAsDouble - startTime < (double)(trackingDuration + AI.defaultInterpolationDelay) && base.Senses.FindTarget(out var target))
		{
			base.AnimPlayer.Track(((Component)target).transform.position, trackingSpeed);
		}
		return base.OnStateUpdate(deltaTime);
	}

	protected virtual void DoDamage()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target) || !(target is BaseCombatEntity baseCombatEntity))
		{
			return;
		}
		if (baseCombatEntity.ToNonNpcPlayer(out var player) && doesStrafeDodge)
		{
			Vector3 estimatedVelocity = player.estimatedVelocity;
			if (Mathf.Abs(Vector3.Dot(((Vector3)(ref estimatedVelocity)).normalized, ((Component)Owner).transform.right)) > 0.7f)
			{
				return;
			}
		}
		if (base.Senses.GetVisibilityStatus(target, out var status) && status.timeNotVisible < 1f && Vector3.Distance(((Component)Owner).transform.position, ((Component)baseCombatEntity).transform.position) < radius)
		{
			baseCombatEntity.OnAttacked(damage, DamageType, Owner, ignoreShield: false);
			if (forceScale > 0f && (Object)(object)player != (Object)null)
			{
				Vector3 val = ((Vector3.Dot(((Component)Owner).transform.right, Vector3Ex.NormalizeXZ(((Component)player).transform.position - ((Component)Owner).transform.position)) > 0f) ? ((Component)Owner).transform.right : (-((Component)Owner).transform.right));
				player.DoPush((((Component)Owner).transform.forward * force.z + val * force.x + Vector3.up * force.y) * forceScale);
			}
		}
	}

	public override void OnStateExit()
	{
		Owner.CancelInvoke(DoDamageAction);
		base.OnStateExit();
	}
}
