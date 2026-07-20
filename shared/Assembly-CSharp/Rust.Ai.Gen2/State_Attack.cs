using System;
using ConVar;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Attack : State_PlayAnimationRM
{
	[SerializeField]
	public float Damage = 30f;

	[SerializeField]
	public float Delay = 0.5f;

	[SerializeField]
	public DamageType DamageType = DamageType.Bite;

	private Action _doDamageAction;

	protected Action DoDamageAction => _doDamageAction ?? (_doDamageAction = DoDamage);

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(Delay < Animation.inPlaceAnimation.length);
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		if (!FaceTarget)
		{
			Vector3 val = Vector3Ex.NormalizeXZ(((Component)Owner).transform.position - targetPosition);
			Vector3 val2 = Vector3.Cross(Vector3.up, val);
			targetPosition += ((Random.value > 0.5f) ? 1f : (-1f)) * val2;
			Vector3 val3 = Vector3Ex.NormalizeXZ(targetPosition - ((Component)Owner).transform.position);
			((Component)Owner).transform.rotation = Quaternion.LookRotation(val3);
		}
		Owner.Invoke(DoDamageAction, Delay + AI.defaultInterpolationDelay);
		return base.OnStateEnter(payload);
	}

	public override void OnStateExit()
	{
		Owner.CancelInvoke(DoDamageAction);
		base.OnStateExit();
	}

	protected virtual void DoDamage()
	{
		if (base.Senses.FindTarget(out var target) && target is BaseCombatEntity baseCombatEntity)
		{
			baseCombatEntity.OnAttacked(Damage, DamageType, Owner, ignoreShield: false);
		}
	}
}
