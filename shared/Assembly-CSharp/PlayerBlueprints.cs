using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class PlayerBlueprints : EntityComponent<BasePlayer>
{
	public SteamInventory steamInventory;

	public void Reset()
	{
		PersistantPlayer persistantPlayerInfo = baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			persistantPlayerInfo.unlockedItems.Clear();
		}
		else
		{
			persistantPlayerInfo.unlockedItems = Pool.Get<List<int>>();
		}
		baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		baseEntity.SendNetworkUpdate();
	}

	public void UnlockAll()
	{
		PersistantPlayer persistantPlayerInfo = baseEntity.PersistantPlayerInfo;
		foreach (ItemBlueprint bp in ItemManager.bpList)
		{
			if (bp.userCraftable && !bp.defaultBlueprint && !persistantPlayerInfo.unlockedItems.Contains(bp.targetItem.itemid))
			{
				persistantPlayerInfo.unlockedItems.Add(bp.targetItem.itemid);
			}
		}
		baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		baseEntity.SendNetworkUpdateImmediate();
		baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", baseEntity), 0);
	}

	public bool IsUnlocked(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			return persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid);
		}
		return false;
	}

	public void Unlock(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = baseEntity.PersistantPlayerInfo;
		if (!persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
		{
			persistantPlayerInfo.unlockedItems.Add(itemDef.itemid);
			baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
			baseEntity.SendNetworkUpdateImmediate();
			baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", baseEntity), itemDef.itemid);
			baseEntity.stats.Add("blueprint_studied", 1, (Stats)5);
		}
	}

	public void UnlockList(List<ItemDefinition> itemDefList)
	{
		PersistantPlayer persistantPlayerInfo = baseEntity.PersistantPlayerInfo;
		foreach (ItemDefinition itemDef in itemDefList)
		{
			if (!persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
			{
				persistantPlayerInfo.unlockedItems.Add(itemDef.itemid);
			}
		}
		baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		baseEntity.SendNetworkUpdateImmediate();
		baseEntity.ClientRPC(RpcTarget.Player("UnlockedBlueprint", baseEntity));
		baseEntity.stats.Add("blueprint_studied", itemDefList.Count, (Stats)5);
	}

	public bool HasUnlocked(ItemDefinition targetItem)
	{
		if (baseEntity.IsCraftingTutorialBlocked(targetItem, out var forceUnlock))
		{
			return false;
		}
		if (forceUnlock)
		{
			return true;
		}
		if (Object.op_Implicit((Object)(object)targetItem.Blueprint))
		{
			if ((Object)(object)targetItem.Blueprint.RequireUnlockedItem != (Object)null && !HasUnlocked(targetItem.Blueprint.RequireUnlockedItem))
			{
				return false;
			}
			if (targetItem.Blueprint.NeedsSteamItem)
			{
				if ((Object)(object)targetItem.steamItem != (Object)null)
				{
					if ((Object)(object)targetItem.steamItem.UnlockedViaSteamItem != (Object)null)
					{
						if (!steamInventory.HasItem(targetItem.steamItem.UnlockedViaSteamItem.id))
						{
							return false;
						}
					}
					else if (!steamInventory.HasItem(targetItem.steamItem.id))
					{
						return false;
					}
				}
				if ((Object)(object)targetItem.steamItem == (Object)null)
				{
					bool flag = false;
					ItemSkinDirectory.Skin[] skins = targetItem.skins;
					for (int i = 0; i < skins.Length; i++)
					{
						ItemSkinDirectory.Skin skin = skins[i];
						if (steamInventory.HasItem(skin.id))
						{
							flag = true;
							break;
						}
					}
					if (!flag && steamInventory.HasWorkshopSkin(targetItem))
					{
						flag = true;
					}
					if (!flag)
					{
						return false;
					}
				}
				return true;
			}
			if (targetItem.Blueprint.NeedsSteamDLC)
			{
				return ItemPassesDlcChecks(targetItem);
			}
		}
		int[] defaultBlueprints = ItemManager.defaultBlueprints;
		for (int i = 0; i < defaultBlueprints.Length; i++)
		{
			if (defaultBlueprints[i] == targetItem.itemid)
			{
				return true;
			}
		}
		if (baseEntity.isServer)
		{
			return IsUnlocked(targetItem);
		}
		return false;
	}

	public bool ItemPassesDlcChecks(ItemDefinition targetItem)
	{
		if (!targetItem.Blueprint.NeedsSteamDLC)
		{
			return true;
		}
		if ((Object)(object)targetItem.steamDlc == (Object)null)
		{
			return false;
		}
		if (!baseEntity.DefaultSkinAccess)
		{
			if (!baseEntity.AllSkinsUnlocked)
			{
				return targetItem.steamDlc.bypassLicenseCheck;
			}
			return true;
		}
		if (targetItem.steamDlc.HasLicense(baseEntity))
		{
			return true;
		}
		return false;
	}

	public bool CanCraft(int itemid, int skinItemId, BasePlayer player)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemid);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return false;
		}
		object obj = Interface.CallHook("CanCraft", this, itemDefinition, skinItemId);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (skinItemId != 0 && !CheckSkinOwnership(skinItemId, player))
		{
			return false;
		}
		if (baseEntity.currentCraftLevel < (float)itemDefinition.Blueprint.GetWorkbenchLevel())
		{
			return false;
		}
		if (HasUnlocked(itemDefinition))
		{
			return true;
		}
		return false;
	}

	public bool CheckSkinOwnership(int skinItemId, BasePlayer player)
	{
		ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId(skinItemId);
		if ((Object)(object)skin.invItem != (Object)null && skin.invItem.HasUnlocked(player))
		{
			return true;
		}
		return steamInventory.HasItem(skinItemId);
	}
}
