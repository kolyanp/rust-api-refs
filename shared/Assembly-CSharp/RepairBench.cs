using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class RepairBench : StorageContainer
{
	public float maxConditionLostOnRepair = 0.2f;

	public GameObjectRef skinchangeEffect;

	public const float REPAIR_COST_FRACTION = 0.2f;

	private float nextSkinChangeAudioTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RepairBench.OnRpcMessage"))
		{
			if (rpc == 1942825351 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ChangeSkin"));
				}
				using (TimeWarning.New("ChangeSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1942825351u, "ChangeSkin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ChangeSkin(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ChangeSkin");
					}
				}
				return true;
			}
			if (rpc == 1178348163 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RepairItem"));
				}
				using (TimeWarning.New("RepairItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1178348163u, "RepairItem", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RepairItem(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RepairItem");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static float GetRepairFraction(Item itemToRepair)
	{
		return GetRepairFraction(itemToRepair.condition, itemToRepair.maxCondition);
	}

	public static float GetRepairFraction(float condition, float maxCondition)
	{
		return 1f - condition / maxCondition;
	}

	public static float RepairCostFraction(Item itemToRepair)
	{
		return GetRepairFraction(itemToRepair) * 0.2f;
	}

	public static float RepairCostFraction(float condition, float maxCondition)
	{
		return GetRepairFraction(condition, maxCondition) * 0.2f;
	}

	public static void GetRepairCostList(ItemBlueprint bp, List<ItemAmount> allIngredients)
	{
		ItemDefinition targetItem = bp.targetItem;
		ItemModRepair itemModRepair = ((targetItem != null) ? ((Component)targetItem).GetComponent<ItemModRepair>() : null);
		if ((Object)(object)itemModRepair != (Object)null && itemModRepair.canUseRepairBench)
		{
			return;
		}
		foreach (ItemAmount ingredient in bp.GetIngredients())
		{
			allIngredients.Add(new ItemAmount(ingredient.itemDef, ingredient.amount));
		}
		StripComponentRepairCost(allIngredients);
	}

	public static void StripComponentRepairCost(List<ItemAmount> allIngredients, float repairCostMultiplier = 1f)
	{
		if (allIngredients == null)
		{
			return;
		}
		for (int i = 0; i < allIngredients.Count; i++)
		{
			ItemAmount itemAmount = allIngredients[i];
			if (itemAmount.itemDef.category != ItemCategory.Component && !itemAmount.itemDef.treatAsComponentForRepairs)
			{
				continue;
			}
			if ((Object)(object)itemAmount.itemDef.Blueprint != (Object)null)
			{
				bool flag = false;
				ItemAmount itemAmount2 = itemAmount.itemDef.Blueprint.GetIngredients()[0];
				foreach (ItemAmount allIngredient in allIngredients)
				{
					if ((Object)(object)allIngredient.itemDef == (Object)(object)itemAmount2.itemDef)
					{
						allIngredient.amount += Mathf.Max(itemAmount2.amount * itemAmount.amount * repairCostMultiplier, 1f);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					allIngredients.Add(new ItemAmount(itemAmount2.itemDef, Mathf.Max(itemAmount2.amount * itemAmount.amount * repairCostMultiplier, 1f)));
				}
			}
			allIngredients.RemoveAt(i);
			i--;
		}
	}

	public void debugprint(string toPrint)
	{
		if (Global.developer > 0)
		{
			Debug.LogWarning((object)toPrint);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ChangeSkin(RPCMessage msg)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		int inventoryId = msg.read.Int32();
		ItemId val = ((msg.read.Unread > 0) ? new ItemId(msg.read.UInt64()) : default(ItemId));
		bool isValid = ((ItemId)(ref val)).IsValid;
		bool flag = !isValid || Time.realtimeSinceStartup > nextSkinChangeAudioTime;
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || Interface.CallHook("OnItemSkinChange", inventoryId, slot, this, player) != null || (isValid && slot.uid != val))
		{
			return;
		}
		if (inventoryId != 0 && !player.blueprints.CheckSkinOwnership(inventoryId, player))
		{
			debugprint("RepairBench.ChangeSkin player does not have item :" + inventoryId + ":");
			return;
		}
		ulong Skin = ItemDefinition.FindSkin(slot.info.itemid, inventoryId);
		if (Skin == slot.skin && (Object)(object)slot.info.isRedirectOf == (Object)null)
		{
			debugprint("RepairBench.ChangeSkin cannot apply same skin twice : " + Skin + ": " + slot.skin);
			return;
		}
		ItemSkinDirectory.Skin skin = slot.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
		ItemDefinition itemDefinition = slot.info;
		int num = 0;
		if ((Object)(object)slot.info.isRedirectOf != (Object)null)
		{
			Skin = ItemDefinition.FindSkin(slot.info.isRedirectOf.itemid, inventoryId);
			skin = slot.info.isRedirectOf.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
			if ((Object)(object)skin.invItem == (Object)null)
			{
				if (slot.info.isRedirectOf.skins2.FirstOrDefault((IPlayerItemDefinition x) => x.DefinitionId == inventoryId) != null)
				{
					itemDefinition = slot.info.isRedirectOf;
					num = inventoryId;
				}
				else
				{
					itemDefinition = slot.info.isRedirectOf;
					num = 0;
				}
			}
			else
			{
				num = skin.invItem.id;
				if (skin.invItem is ItemSkin itemSkin)
				{
					if ((Object)(object)itemSkin.Redirect != (Object)null)
					{
						itemDefinition = itemSkin.Redirect;
						num = 0;
					}
					else if ((Object)(object)itemSkin.Redirect == (Object)null && (Object)(object)slot.info.isRedirectOf != (Object)null)
					{
						itemDefinition = slot.info.isRedirectOf;
					}
				}
			}
		}
		else if (slot.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin).invItem is ItemSkin itemSkin2 && (Object)(object)itemSkin2.Redirect != (Object)null)
		{
			itemDefinition = itemSkin2.Redirect;
		}
		if (((Object)(object)itemDefinition == (Object)(object)slot.info && (Object)(object)itemDefinition.isRedirectOf != (Object)null && num == 0) || ((Object)(object)itemDefinition.isRedirectOf == (Object)null && itemDefinition.hidden))
		{
			return;
		}
		if (flag)
		{
			nextSkinChangeAudioTime = Time.realtimeSinceStartup + 0.75f;
		}
		if ((Object)(object)itemDefinition != (Object)(object)slot.info)
		{
			bool flag2 = false;
			flag2 = num != 0;
			float condition = slot.condition;
			float maxCondition = slot.maxCondition;
			int amount = slot.amount;
			ulong attachment = slot.attachment;
			int num2 = 0;
			int num3 = 0;
			ItemModContainerArmorSlot component = ((Component)slot.info).GetComponent<ItemModContainerArmorSlot>();
			if ((Object)(object)component != (Object)null && slot.contents != null)
			{
				num3 = slot.contents.capacity;
			}
			ItemDefinition ammoType = null;
			if ((Object)(object)slot.GetHeldEntity() != (Object)null && slot.GetHeldEntity() is BaseProjectile { primaryMagazine: not null } baseProjectile)
			{
				num2 = baseProjectile.primaryMagazine.contents;
				ammoType = baseProjectile.primaryMagazine.ammoType;
			}
			if ((Object)(object)slot.GetHeldEntity() != (Object)null && slot.GetHeldEntity() is Chainsaw chainsaw)
			{
				num2 = chainsaw.ammo;
			}
			List<Item> list = Pool.Get<List<Item>>();
			if (slot.contents != null && slot.contents.itemList != null && slot.contents.itemList.Count > 0)
			{
				if (slot.contents.itemList.Count > list.Capacity)
				{
					list.Capacity = slot.contents.itemList.Count;
				}
				foreach (Item item2 in slot.contents.itemList)
				{
					list.Add(item2);
				}
				foreach (Item item3 in list)
				{
					item3.RemoveFromContainer();
				}
			}
			Item item = ItemManager.Create(itemDefinition, 1, 0uL, isServerSide: true, 0uL);
			item.ownershipShares = slot.ownershipShares;
			slot.ownershipShares = null;
			slot.Remove();
			ItemManager.DoRemoves();
			item.MoveToContainer(base.inventory, 0, allowStack: false);
			item.maxCondition = maxCondition;
			item.condition = condition;
			item.amount = amount;
			if ((Object)(object)item.GetHeldEntity() != (Object)null && item.GetHeldEntity() is BaseProjectile baseProjectile2)
			{
				if (baseProjectile2.primaryMagazine != null)
				{
					baseProjectile2.SetAmmoCount(num2);
					baseProjectile2.primaryMagazine.ammoType = ammoType;
				}
				baseProjectile2.ForceModsChanged();
			}
			if ((Object)(object)item.GetHeldEntity() != (Object)null && item.GetHeldEntity() is Chainsaw chainsaw2)
			{
				chainsaw2.ammo = num2;
			}
			if (num3 > 0)
			{
				component = ((Component)item.info).GetComponent<ItemModContainerArmorSlot>();
				component.CreateAtCapacity(num3, item);
			}
			if (list.Count > 0 && item.contents != null)
			{
				if ((Object)(object)component != (Object)null)
				{
					for (int num4 = 0; num4 < list.Count; num4++)
					{
						list[num4]?.MoveToContainer(item.contents, num4, allowStack: false);
					}
				}
				else
				{
					foreach (Item item4 in list)
					{
						item4.MoveToContainer(item.contents);
					}
				}
			}
			Pool.Free<Item>(ref list, false);
			if (attachment != 0L && item.info.supportsAccessories)
			{
				item.attachment = attachment;
				item.MarkDirty();
				BaseEntity heldEntity = item.GetHeldEntity();
				if ((Object)(object)heldEntity != (Object)null)
				{
					heldEntity.attachmentID = item.attachment;
					heldEntity.SendNetworkUpdate();
				}
			}
			if (flag2)
			{
				ApplySkinToItem(item, Skin);
			}
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, item, Skin);
		}
		else
		{
			ApplySkinToItem(slot, Skin);
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, slot, Skin);
		}
		if (flag && skinchangeEffect.isValid)
		{
			Effect.server.Run(skinchangeEffect.resourcePath, this, 0u, new Vector3(0f, 1.5f, 0f), Vector3.zero);
		}
	}

	private void ApplySkinToItem(Item item, ulong Skin)
	{
		item.skin = Skin;
		item.MarkDirty();
		BaseEntity heldEntity = item.GetHeldEntity();
		if ((Object)(object)heldEntity != (Object)null)
		{
			heldEntity.skinID = Skin;
			heldEntity.SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RepairItem(RPCMessage msg)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot != null)
		{
			BasePlayer player = msg.player;
			float conditionLost = maxConditionLostOnRepair;
			ItemModRepair component = ((Component)slot.info).GetComponent<ItemModRepair>();
			if ((Object)(object)component != (Object)null)
			{
				conditionLost = component.conditionLost;
			}
			RepairAnItem(slot, player, this, conditionLost, mustKnowBlueprint: true);
		}
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		return 0;
	}

	public static void RepairAnItem(Item itemToRepair, BasePlayer player, BaseEntity repairBenchEntity, float maxConditionLostOnRepair, bool mustKnowBlueprint)
	{
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		if (itemToRepair == null)
		{
			return;
		}
		ItemDefinition info = itemToRepair.info;
		ItemBlueprint blueprint = info.Blueprint;
		if (!Object.op_Implicit((Object)(object)blueprint))
		{
			return;
		}
		ItemModRepair component = ((Component)itemToRepair.info).GetComponent<ItemModRepair>();
		if (!info.condition.repairable || itemToRepair.condition == itemToRepair.maxCondition)
		{
			return;
		}
		if (mustKnowBlueprint)
		{
			ItemDefinition itemDefinition = (((Object)(object)info.isRedirectOf != (Object)null) ? info.isRedirectOf : info);
			bool flag = player.blueprints.HasUnlocked(itemDefinition) || ((Object)(object)itemDefinition.Blueprint != (Object)null && !itemDefinition.Blueprint.isResearchable);
			if (!flag && (Object)(object)BaseGameMode.svActiveGameMode != (Object)null && BaseGameMode.svActiveGameMode.canRepairIfCraftingBanned && !itemDefinition.IsAllowed((EraRestriction)4))
			{
				flag = true;
			}
			if (!flag)
			{
				return;
			}
		}
		if (Interface.CallHook("OnItemRepair", player, itemToRepair) != null)
		{
			return;
		}
		float num = RepairCostFraction(itemToRepair);
		bool flag2 = false;
		List<ItemAmount> list = Pool.Get<List<ItemAmount>>();
		GetRepairCostList(blueprint, list);
		foreach (ItemAmount item in list)
		{
			if (item.itemDef.category != ItemCategory.Component)
			{
				int amount = player.inventory.GetAmount(item.itemDef.itemid);
				if (Mathf.CeilToInt(item.amount * num) > amount)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2)
		{
			Pool.FreeUnmanaged<ItemAmount>(ref list);
			return;
		}
		foreach (ItemAmount item2 in list)
		{
			if (item2.itemDef.category != ItemCategory.Component)
			{
				int amount2 = Mathf.CeilToInt(item2.amount * num);
				player.inventory.Take(null, item2.itemid, amount2);
				Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "repair", item2.itemDef.shortname, amount2, repairBenchEntity, null, safezone: false, null, 0uL, null, itemToRepair, null, 0uL);
			}
		}
		Pool.FreeUnmanaged<ItemAmount>(ref list);
		float conditionNormalized = itemToRepair.conditionNormalized;
		float maxConditionNormalized = itemToRepair.maxConditionNormalized;
		itemToRepair.DoRepair(maxConditionLostOnRepair);
		Facepunch.Rust.Analytics.Azure.OnItemRepaired(player, repairBenchEntity, itemToRepair, conditionNormalized, maxConditionNormalized);
		if (Global.developer > 0)
		{
			Debug.Log((object)("Item repaired! condition : " + itemToRepair.condition + "/" + itemToRepair.maxCondition));
		}
		string strName = "assets/bundled/prefabs/fx/repairbench/itemrepair.prefab";
		if ((Object)(object)component != (Object)null && (Object)(object)component.successEffect?.Get() != (Object)null)
		{
			strName = component.successEffect.resourcePath;
		}
		Effect.server.Run(strName, repairBenchEntity, 0u, Vector3.zero, Vector3.zero);
	}
}
