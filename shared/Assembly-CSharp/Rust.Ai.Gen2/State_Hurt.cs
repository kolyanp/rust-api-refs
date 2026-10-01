using System;
using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Hurt : State_PlayAnimationRM
{
	[SerializeField]
	private RootMotionData StrongHitL;

	[SerializeField]
	private RootMotionData StrongHitR;

	[SerializeField]
	private RootMotionData WeakHit;

	[SerializeField]
	private float StaggerRatio = 0.5f;

	public virtual bool HasStaggerAnimation
	{
		get
		{
			if (StrongHitL != null)
			{
				return StrongHitR != null;
			}
			return false;
		}
	}

	protected virtual RootMotionData PickStrongHit(HitInfo hitInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!(Vector3.Dot(hitInfo.attackNormal, ((Component)Owner).transform.right) > 0f))
		{
			return StrongHitR;
		}
		return StrongHitL;
	}

	public bool ShouldStagger(BaseEntity owner, HitInfo hitInfo)
	{
		float num = owner.Health() - hitInfo.damageTypes.Total();
		float num2 = owner.MaxHealth() * StaggerRatio;
		if (owner.Health() > num2)
		{
			return num < num2;
		}
		return false;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (payload.hitInfo == null && AI.logIssues && AI.logIssues)
		{
			Debug.LogError((object)$"Entering {Name} without HitInfo payload, this should not happen and may cause issues with stats tracking. Owner: {Owner}", (Object)(object)Owner);
		}
		if (payload.hitInfo.damageTypes.Has(DamageType.Heat))
		{
			Blackboard.Add("HitByFire");
		}
		if (WeakHit == null || ShouldStagger(Owner, payload.hitInfo))
		{
			Animation = PickStrongHit(payload.hitInfo);
		}
		else
		{
			Animation = WeakHit;
		}
		if (payload.hitInfo.Initiator is BaseCombatEntity baseCombatEntity)
		{
			bool flag = true;
			if (Senses.FindTarget(out var target))
			{
				bool flag2 = Owner.Distance((BaseEntity)baseCombatEntity) < 16f;
				bool flag3 = !target.IsNonNpcPlayer() && baseCombatEntity.IsNonNpcPlayer();
				flag = flag2 | flag3;
			}
			if (flag)
			{
				Senses.TrySetTarget(baseCombatEntity);
			}
		}
		return base.OnStateEnter(payload);
	}
}
