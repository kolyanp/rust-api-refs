using System.Collections.Generic;
using ConVar;
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
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (entity is ISimpleUpgradable simpleUpgradable && simpleUpgradable.CanUpgrade(player, upgradeItem))
		{
			PayForUpgrade(entity, upgradeItem, player);
			GameObjectRef entityPrefab = ((Component)upgradeItem).GetComponent<ItemModDeployable>().entityPrefab;
			entity = SprayCan.DoRedirectSwap(entity, entityPrefab.resourcePath, 0uL);
			entity.OwnerID = player.userID;
			Deployable deployable = default;
			if (entityPrefab.Get().TryGetComponent<Deployable>(ref deployable) && deployable.placeEffect.isValid)
			{
				Effect.server.Run(deployable.placeEffect.resourcePath, ((Component)entity).transform.position, Vector3.up);
			}
			if (entity is DecayEntity decayEntity)
			{
				decayEntity.timePlaced = entity.GetNetworkTime();
				decayEntity.AttachToBuilding(null);
			}
		}
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
