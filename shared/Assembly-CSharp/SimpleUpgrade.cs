using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

internal static class SimpleUpgrade
{
	public static bool CanUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if ((Object)(object)upgradeItem == (Object)null)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (player.IsBuildingBlocked(((Component)entity).transform.position, ((Component)entity).transform.rotation, entity.bounds))
		{
			return false;
		}
		if ((Object)(object)((Component)upgradeItem).GetComponent<ItemModDeployable>() == (Object)null)
		{
			return false;
		}
		if (IsUpgradeBlocked(entity, upgradeItem, player))
		{
			return false;
		}
		if (!CanAffordUpgrade(entity, upgradeItem, player))
		{
			return false;
		}
		return true;
	}

	public static bool CanAffordUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		ISimpleUpgradable simpleUpgradable = entity as ISimpleUpgradable;
		if ((Object)(object)entity == (Object)null)
		{
			return false;
		}
		if (player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		if (simpleUpgradable.CostIsItem())
		{
			return player.inventory.GetAmount(upgradeItem) > 0;
		}
		if ((Object)(object)upgradeItem.Blueprint == (Object)null)
		{
			return false;
		}
		if (!ItemModStudyBlueprint.IsBlueprintUnlocked(upgradeItem, player))
		{
			return false;
		}
		foreach (ItemAmount ingredient in upgradeItem.Blueprint.GetIngredients())
		{
			if ((float)player.inventory.GetAmount(ingredient.itemid) < ingredient.amount)
			{
				return false;
			}
		}
		return true;
	}

	public static void PayForUpgrade(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		if ((Object)(object)player == (Object)null || (player.IsInCreativeMode && Creative.freeBuild) || !(entity is ISimpleUpgradable simpleUpgradable))
		{
			return;
		}
		List<Item> list = new List<Item>();
		if (simpleUpgradable.CostIsItem())
		{
			player.inventory.Take(list, upgradeItem.itemid, 1);
			player.Command("note.inv " + upgradeItem.itemid + " " + -1);
		}
		else
		{
			foreach (ItemAmount ingredient in upgradeItem.Blueprint.GetIngredients())
			{
				player.inventory.Take(list, ingredient.itemid, (int)ingredient.amount);
				player.Command("note.inv " + ingredient.itemid + " " + ingredient.amount * -1f);
			}
		}
		foreach (Item item in list)
		{
			item.Remove();
		}
	}

	public static void DoUpgrade(BaseEntity entity, BasePlayer player, ItemDefinition upgradeItem)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		if (!(entity is ISimpleUpgradable simpleUpgradable) || !simpleUpgradable.CanUpgrade(player, upgradeItem))
		{
			return;
		}
		PayForUpgrade(entity, upgradeItem, player);
		EntityRef[] slots = entity.GetSlots();
		BaseEntity parentEntity = entity.GetParentEntity();
		bool flag = entity is DecayEntity decayEntity && decayEntity.HasFlag(BaseEntity.Flags.Reserved2);
		ItemModDeployable component = ((Component)upgradeItem).GetComponent<ItemModDeployable>();
		BaseEntity baseEntity = GameManager.server.CreateEntity(component.entityPrefab.resourcePath, ((Component)entity).transform.position, ((Component)entity).transform.rotation);
		baseEntity.SetParent(parentEntity);
		baseEntity.OwnerID = player.userID;
		Deployable component2 = component.entityPrefab.Get().GetComponent<Deployable>();
		if (component2 != null && component2.placeEffect.isValid)
		{
			Effect.server.Run(component2.placeEffect.resourcePath, ((Component)entity).transform.position, Vector3.up);
		}
		DecayEntity decayEntity2 = baseEntity as DecayEntity;
		if ((Object)(object)decayEntity2 != (Object)null)
		{
			decayEntity2.timePlaced = entity.GetNetworkTime();
		}
		List<BaseEntity.ChildPreserveInfo> list = Pool.Get<List<BaseEntity.ChildPreserveInfo>>();
		foreach (BaseEntity child in entity.children)
		{
			list.Add(new BaseEntity.ChildPreserveInfo
			{
				targetEntity = child,
				targetBone = child.parentBone,
				localPosition = ((Component)child).transform.localPosition,
				localRotation = ((Component)child).transform.localRotation
			});
		}
		foreach (BaseEntity.ChildPreserveInfo item in list)
		{
			item.targetEntity.SetParent(null, worldPositionStays: true);
		}
		entity.Kill();
		if (baseEntity is DecayEntity decayEntity3)
		{
			decayEntity3.AttachToBuilding(null);
		}
		baseEntity.Spawn();
		foreach (BaseEntity.ChildPreserveInfo item2 in list)
		{
			item2.targetEntity.SetParent(baseEntity, item2.targetBone, worldPositionStays: true);
			((Component)item2.targetEntity).transform.localPosition = item2.localPosition;
			((Component)item2.targetEntity).transform.localRotation = item2.localRotation;
			item2.targetEntity.SendNetworkUpdate();
		}
		baseEntity.SetSlots(slots);
		if (!flag && baseEntity is DecayEntity decayEntity4)
		{
			decayEntity4.StopBeingDemolishable();
		}
		Pool.FreeUnmanaged<BaseEntity.ChildPreserveInfo>(ref list);
	}

	public static bool IsUpgradeBlocked(BaseEntity entity, ItemDefinition upgradeItem, BasePlayer player)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)upgradeItem == (Object)null)
		{
			return true;
		}
		if ((Object)(object)entity == (Object)null)
		{
			return true;
		}
		if (entity is DecorDeployable)
		{
			return false;
		}
		if (entity is BaseLock)
		{
			return false;
		}
		ItemModDeployable component = ((Component)upgradeItem).GetComponent<ItemModDeployable>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(component.entityPrefab.resourceID);
		if (DeployVolume.Check(((Component)entity).transform.position, ((Component)entity).transform.rotation, volumes, ~((1 << ((Component)entity).gameObject.layer) | 0x20000000)))
		{
			if ((Object)(object)DeployVolume.LastDeployHit != (Object)null)
			{
				string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit);
				if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
				{
					Construction.lastPlacementError = Phrase.op_Implicit(blockedByErrorFromCollider);
					Construction.lastPlacementErrorIsDetailed = true;
				}
			}
			return true;
		}
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(component.entityPrefab.resourceID);
		Construction.Target target = new Construction.Target
		{
			position = ((Component)entity).transform.position,
			rotation = ((Component)entity).transform.eulerAngles,
			normal = ((Component)entity).transform.up,
			ray = player.eyes.HeadRay()
		};
		Construction.Placement placement = new Construction.Placement(target)
		{
			position = target.position,
			rotation = ((Component)entity).transform.rotation,
			ignoredEntity = entity
		};
		Socket_Base[] array2 = array;
		foreach (Socket_Base socket_Base in array2)
		{
			if (socket_Base.male && !socket_Base.CheckSocketMods(ref placement))
			{
				return true;
			}
		}
		return false;
	}
}
