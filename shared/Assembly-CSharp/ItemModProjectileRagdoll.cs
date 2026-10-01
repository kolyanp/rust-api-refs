using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ItemModProjectileRagdoll : ItemModProjectileMod
{
	public float radius = 0.5f;

	public float velocityInheritFactor = 0.25f;

	public override void ServerProjectileHit(HitInfo info)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Vis.Entities(info.HitPositionWorld, radius, list, 131072, (QueryTriggerInteraction)2);
		Vector3 position = info.HitPositionWorld - info.ProjectileVelocity.normalized * 0.1f;
		foreach (BasePlayer item in list)
		{
			if (!item.isServer || item.IsDead() || item.InSafeZone())
			{
				continue;
			}
			Vector3 val;
			if ((Object)(object)info.HitEntity == (Object)(object)item)
			{
				val = info.ProjectileVelocity * velocityInheritFactor;
			}
			else
			{
				if (!item.IsVisible(position))
				{
					continue;
				}
				Vector3 val2 = ((Component)item).transform.position - info.HitPositionWorld + Vector3.up;
				val = val2.normalized * 7.5f;
			}
			item.Ragdoll(item.GetWorldVelocity() + val);
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}
}
