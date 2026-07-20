using Oxide.Core;
using UnityEngine;

public class Hammer : BaseMelee
{
	public override bool CanHit(HitTest info)
	{
		if ((Object)(object)info.HitEntity == (Object)null)
		{
			return false;
		}
		if (info.HitEntity is BasePlayer)
		{
			return false;
		}
		return info.HitEntity is BaseCombatEntity;
	}

	public override void DoAttackShared(HitInfo info)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		BaseCombatEntity baseCombatEntity = info.HitEntity as BaseCombatEntity;
		if (base.isServer && (Object)(object)baseCombatEntity != (Object)null && baseCombatEntity.ShouldRepairViaParent())
		{
			BaseCombatEntity repairableParent = baseCombatEntity.GetRepairableParent();
			if ((Object)(object)repairableParent != (Object)null)
			{
				baseCombatEntity = repairableParent;
			}
		}
		if ((Object)(object)baseCombatEntity != (Object)null && (Object)(object)ownerPlayer != (Object)null && base.isServer)
		{
			if (Interface.CallHook("OnHammerHit", ownerPlayer, info) != null)
			{
				return;
			}
			using (TimeWarning.New("DoRepair", 50))
			{
				baseCombatEntity.DoRepair(ownerPlayer);
			}
		}
		info.DoDecals = false;
		if (base.isServer)
		{
			Effect.server.ImpactEffect(info);
		}
		else
		{
			Effect.client.ImpactEffect(info);
		}
		StartAttackCooldown(repeatDelay);
	}
}
