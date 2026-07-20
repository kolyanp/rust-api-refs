using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public static class DamageUtil
{
	public static void RadiusDamage(BaseEntity attackingPlayer, BaseEntity weaponPrefab, Vector3 pos, float minradius, float radius, List<DamageTypeEntry> damage, int layers, bool useLineOfSight, bool ignoreAI = false, bool ignoreAttackingPlayer = false, bool extendedLineOfSight = false, List<DamageTypeEntry> playerDamage = null, bool removeWallpaper = false, bool includeBoatBuildingPieces = true, BaseEntity ignoreEntity = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("DamageUtil.RadiusDamage"))
		{
			List<HitInfo> list = Pool.Get<List<HitInfo>>();
			List<BaseEntity> list2 = Pool.Get<List<BaseEntity>>();
			List<BaseEntity> list3 = Pool.Get<List<BaseEntity>>();
			List<BaseEntity> list4 = Pool.Get<List<BaseEntity>>();
			BaseEntity baseEntity = null;
			Vis.Entities(pos, radius, list4, layers, (QueryTriggerInteraction)2);
			for (int i = 0; i < list4.Count; i++)
			{
				BaseEntity baseEntity2 = list4[i];
				if (!baseEntity2.isServer || (Object)(object)ignoreEntity == (Object)(object)baseEntity2 || list2.Contains(baseEntity2) || (!includeBoatBuildingPieces && (baseEntity2 is BoatBuildingBlock || baseEntity2 is global::IBoatBuildingPiece)))
				{
					continue;
				}
				baseEntity = baseEntity2.GetParentEntity();
				bool flag = ShouldSingleHitSharedParentEntity(baseEntity2, baseEntity);
				if ((flag && list3.Contains(baseEntity)) || (ignoreAI && IsIgnoredAI(baseEntity2)))
				{
					continue;
				}
				Vector3 val = baseEntity2.ClosestPoint(pos);
				float num = Mathf.Clamp01((Vector3.Distance(val, pos) - minradius) / (radius - minradius));
				if (num > 1f)
				{
					continue;
				}
				float amount = 1f - num;
				if (flag)
				{
					amount = 0.85f;
				}
				if (removeWallpaper && baseEntity2 is BuildingBlock buildingBlock)
				{
					buildingBlock.RemoveWallpaper(0);
					buildingBlock.RemoveWallpaper(1);
				}
				if ((extendedLineOfSight && !GamePhysics.LineOfSight(baseEntity2.CenterPoint(), pos, 1218519041, baseEntity2)) || (useLineOfSight && !baseEntity2.IsVisible(pos)))
				{
					continue;
				}
				if (useLineOfSight && baseEntity2 is BasePlayer basePlayer && basePlayer.IsDucked())
				{
					Bounds colliderBounds = basePlayer.GetColliderBounds();
					if (((Bounds)(ref colliderBounds)).max.y - val.y < 0.1f && !GamePhysics.LineOfSight(pos, Vector3Ex.WithY(((Bounds)(ref colliderBounds)).center, ((Bounds)(ref colliderBounds)).max.y - 0.1f), 1218519041, baseEntity2))
					{
						continue;
					}
				}
				HitInfo hitInfo = new HitInfo();
				hitInfo.Initiator = attackingPlayer;
				hitInfo.WeaponPrefab = weaponPrefab;
				if (playerDamage != null && playerDamage.Count > 0 && baseEntity2 is BasePlayer)
				{
					hitInfo.damageTypes.Add(playerDamage);
				}
				else
				{
					hitInfo.damageTypes.Add(damage);
				}
				hitInfo.damageTypes.ScaleAll(amount);
				hitInfo.HitPositionWorld = val;
				Vector3 val2 = pos - val;
				hitInfo.HitNormalWorld = ((Vector3)(ref val2)).normalized;
				hitInfo.PointStart = pos;
				hitInfo.PointEnd = hitInfo.HitPositionWorld;
				list.Add(hitInfo);
				list2.Add(baseEntity2);
				if (flag)
				{
					list3.Add(baseEntity);
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				BaseEntity baseEntity3 = list2[j];
				HitInfo info = list[j];
				if (!ignoreAttackingPlayer || !((Object)(object)attackingPlayer != (Object)null) || !baseEntity3.EqualNetID((BaseNetworkable)attackingPlayer))
				{
					baseEntity3.OnAttacked(info);
				}
			}
			Pool.FreeUnmanaged<HitInfo>(ref list);
			Pool.FreeUnmanaged<BaseEntity>(ref list2);
			Pool.FreeUnmanaged<BaseEntity>(ref list3);
			Pool.FreeUnmanaged<BaseEntity>(ref list4);
		}
	}

	public static bool ShouldSingleHitSharedParentEntity(BaseEntity hitEnt, BaseEntity parentEntity)
	{
		if ((Object)(object)parentEntity == (Object)null)
		{
			return false;
		}
		if (parentEntity is PlayerBoat)
		{
			if (!(hitEnt is BoatBuildingBlock))
			{
				return hitEnt is global::IBoatBuildingPiece;
			}
			return true;
		}
		return false;
	}

	private static bool IsIgnoredAI(BaseEntity ent)
	{
		return ent is ScientistNPC;
	}
}
