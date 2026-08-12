using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_DeadlyAttack : State_Attack
{
	public float range = 4f;

	public SoundDefinition impactSound;

	private static readonly Vector3 force;

	protected override void DoDamage()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		if (!base.Senses.FindTarget(out var target) || !(target is BaseCombatEntity baseCombatEntity))
		{
			return;
		}
		float amount = Damage;
		if (target.IsNonNpcPlayer())
		{
			float num = baseCombatEntity.baseProtection.Get(DamageType);
			float num2 = (1f - num) * 1.2f;
			float num3 = Damage * num2;
			if (num == 0f && baseCombatEntity.health >= 30f && num3 > baseCombatEntity.health)
			{
				amount = (baseCombatEntity.health - 1f) * (1f / num2);
			}
		}
		baseCombatEntity.OnAttacked(amount, DamageType, Owner, ignoreShield: false);
		Owner.ClientRPC(RpcTarget.NetworkGroup("RPC_PlayNPCAttackImpactSound"));
		if (baseCombatEntity.ToNonNpcPlayer(out var player))
		{
			Vector3 val = ((Vector3.Dot(((Component)Owner).transform.right, Vector3Ex.NormalizeXZ(((Component)player).transform.position - ((Component)Owner).transform.position)) > 0f) ? ((Component)Owner).transform.right : (-((Component)Owner).transform.right));
			player.DoPush(((Component)Owner).transform.forward * force.z + val * force.x + Vector3.up * force.y);
		}
	}

	static State_DeadlyAttack()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		force = new Vector3(15f, 3f, 15f);
	}
}
