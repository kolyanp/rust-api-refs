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

	[SerializeField]
	[Tooltip("Line the attack up on the target as it starts. Turn off for an attack that deliberately does not face what it hits, such as a backwards kick.")]
	public bool AlignToTarget = true;

	private Action _doDamageAction;

	protected Action DoDamageAction => _doDamageAction ?? (_doDamageAction = DoDamage);

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
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
			targetPosition += ((Rng.Value > 0.5f) ? 1f : (-1f)) * val2;
			Vector3 val3 = Vector3Ex.NormalizeXZ(targetPosition - ((Component)Owner).transform.position);
			Agent.Face(Quaternion.LookRotation(val3));
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
