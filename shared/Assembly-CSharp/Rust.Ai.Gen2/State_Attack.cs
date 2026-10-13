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

	[Tooltip("Line the attack up on the target as it starts. Turn off for an attack that deliberately does not face what it hits, such as a backwards kick.")]
	[SerializeField]
	public bool AlignToTarget = true;

	private Action _doDamageAction;

	protected Action DoDamageAction => _doDamageAction ?? (_doDamageAction = DoDamage);

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!HasAnimation)
		{
			return EFSMStateStatus.Failure;
		}
		Assert.IsTrue(Delay < Animation.inPlaceAnimation.length);
		if (!Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Failure;
		}
		if (!FaceTarget && AlignToTarget)
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
		if (Senses.FindTarget(out var target) && target is BaseCombatEntity baseCombatEntity)
		{
			baseCombatEntity.OnAttacked(Damage, DamageType, Owner, ignoreShield: false);
		}
	}
}
