using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class ItemModProjectileRadialDamage : ItemModProjectileMod
{
	public float radius = 0.5f;

	public DamageTypeEntry damage;

	public GameObjectRef effect;

	public bool ignoreHitObject = true;

	public bool onlyDoors;

	public int vibrationLevel = 2;

	public override void ServerProjectileHit(HitInfo info)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		if (effect.isValid)
		{
			Effect.server.Run(effect.resourcePath, info.HitPositionWorld, info.HitNormalWorld);
		}
		List<BaseCombatEntity> list = Pool.Get<List<BaseCombatEntity>>();
		List<BaseEntity> list2 = Pool.Get<List<BaseEntity>>();
		List<BaseCombatEntity> list3 = Pool.Get<List<BaseCombatEntity>>();
		Vis.Entities(info.HitPositionWorld, radius, list3, 1237003025, (QueryTriggerInteraction)2);
		if (damage.type == DamageType.Explosion)
		{
			SeismicSensor.Notify(info.HitPositionWorld, vibrationLevel);
		}
		foreach (BaseCombatEntity item in list3)
		{
			if (!item.isServer || list.Contains(item) || (onlyDoors && !(item is Door)))
			{
				continue;
			}
			BaseEntity parentEntity = item.GetParentEntity();
			bool flag = DamageUtil.ShouldSingleHitSharedParentEntity(item, parentEntity);
			if ((flag && list2.Contains(parentEntity)) || ((Object)(object)item == (Object)(object)info.HitEntity && ignoreHitObject))
			{
				continue;
			}
			item.CenterPoint();
			Vector3 val = item.ClosestPoint(info.HitPositionWorld);
			float num = Vector3.Distance(val, info.HitPositionWorld) / radius;
			if (num > 1f)
			{
				continue;
			}
			float num2 = 1f - num;
			if (flag)
			{
				num2 = 0.9f;
			}
			if (!item.IsVisibleAndCanSeeLegacy(info.HitPositionWorld - ((Vector3)(ref info.ProjectileVelocity)).normalized * 0.1f))
			{
				continue;
			}
			Vector3 hitPositionWorld = info.HitPositionWorld;
			Vector3 val2 = val - info.HitPositionWorld;
			if (item.IsVisibleAndCanSeeLegacy(hitPositionWorld - ((Vector3)(ref val2)).normalized * 0.1f))
			{
				list.Add(item);
				if (flag)
				{
					list2.Add(parentEntity);
				}
				BasePlayer component = ((Component)item).GetComponent<BasePlayer>();
				if ((Object)(object)component != (Object)null && component.TryGetActiveShield(out var foundShield) && (Object)(object)info.Initiator != (Object)null && foundShield.SphereCastAgainstColliders(info.HitPositionWorld, radius))
				{
					foundShield.OnAttacked(new HitInfo(info.Initiator, item, damage.type, damage.amount * num2));
				}
				else
				{
					item.OnAttacked(new HitInfo(info.Initiator, item, damage.type, damage.amount * num2));
				}
			}
		}
		Pool.FreeUnmanaged<BaseCombatEntity>(ref list3);
		Pool.FreeUnmanaged<BaseCombatEntity>(ref list);
		Pool.FreeUnmanaged<BaseEntity>(ref list2);
	}
}
