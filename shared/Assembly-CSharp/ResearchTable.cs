using System;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ResearchTable : StorageContainer
{
	public enum ResearchType
	{
		ResearchTable,
		TechTree
	}

	[NonSerialized]
	public float researchFinishedTime;

	public float researchCostFraction = 1f;

	public float researchDuration = 10f;

	public int requiredPaper = 10;

	public GameObjectRef researchStartEffect;

	public GameObjectRef researchFailEffect;

	public GameObjectRef researchSuccessEffect;

	public ItemDefinition researchResource;

	public BasePlayer user;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ResearchTable.OnRpcMessage"))
		{
			if (rpc == 3177710095u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoResearch"));
				}
				using (TimeWarning.New("DoResearch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3177710095u, "DoResearch", this, player, 3f))
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
							DoResearch(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoResearch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		researchFinishedTime = 0f;
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (item.info.shortname == "scrap")
		{
			Item slot = base.inventory.GetSlot(1);
			if (slot == null)
			{
				return 1;
			}
			if (slot.amount < item.MaxStackable())
			{
				return 1;
			}
		}
		return base.GetIdealSlot(player, container, item);
	}

	public bool IsResearching()
	{
		return HasFlag(Flags.On);
	}

	public int RarityMultiplier(Rarity rarity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)rarity == 1)
		{
			return 20;
		}
		if ((int)rarity == 2)
		{
			return 15;
		}
		if ((int)rarity == 3)
		{
			return 10;
		}
		return 5;
	}

	public int GetBlueprintStacksize(Item sourceItem)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		int result = RarityMultiplier(sourceItem.info.rarity);
		if (sourceItem.info.category == ItemCategory.Ammunition)
		{
			result = Mathf.FloorToInt((float)sourceItem.MaxStackable() / (float)sourceItem.info.Blueprint.amountToCreate) * 2;
		}
		return result;
	}

	public static int ScrapForResearch(Item item)
	{
		object obj = Interface.CallHook("OnResearchCostDetermine", item);
		if (obj is int)
		{
			return (int)obj;
		}
		return ScrapForResearch(item.info);
	}

	public static int ScrapForResearch(ItemDefinition info)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnResearchCostDetermine", info);
		if (obj is int)
		{
			return (int)obj;
		}
		if ((Object)(object)info.isRedirectOf != (Object)null)
		{
			return ScrapForResearch(info.isRedirectOf);
		}
		int result = 0;
		if ((int)info.rarity == 1)
		{
			result = 15;
		}
		if ((int)info.rarity == 2)
		{
			result = 30;
		}
		if ((int)info.rarity == 3)
		{
			result = 60;
		}
		if ((int)info.rarity == 4 || (int)info.rarity == 0)
		{
			result = 120;
		}
		ItemBlueprint itemBlueprint = ItemManager.FindBlueprint(info);
		if ((Object)(object)itemBlueprint != (Object)null && itemBlueprint.defaultBlueprint)
		{
			return ConVar.Server.defaultBlueprintResearchCost;
		}
		return result;
	}

	public bool IsItemResearchable(Item item)
	{
		ItemDefinition itemDefinition = (((Object)(object)item.info.isRedirectOf != (Object)null) ? item.info.isRedirectOf : item.info);
		if (!itemDefinition.IsAllowed((EraRestriction)4))
		{
			return false;
		}
		ItemBlueprint itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
		if ((Object)(object)itemBlueprint != (Object)null && itemBlueprint.defaultBlueprint)
		{
			return true;
		}
		if ((Object)(object)itemBlueprint == (Object)null || !itemBlueprint.isResearchable)
		{
			return false;
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = ItemFilter;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (targetSlot == 1 && (Object)(object)item.info != (Object)(object)researchResource)
		{
			return false;
		}
		return base.ItemFilter(item, targetSlot);
	}

	public Item GetTargetItem()
	{
		return base.inventory.GetSlot(0);
	}

	public Item GetScrapItem()
	{
		Item slot = base.inventory.GetSlot(1);
		if (slot == null || (Object)(object)slot.info != (Object)(object)researchResource)
		{
			return null;
		}
		return slot;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.On))
		{
			Invoke(ResearchAttemptFinished, researchDuration);
		}
		base.inventory.SetLocked(isLocked: false);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		user = player;
		return base.PlayerOpenLoot(player);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		user = null;
		base.PlayerStoppedLooting(player);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void DoResearch(RPCMessage msg)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (IsResearching())
		{
			return;
		}
		BasePlayer player = msg.player;
		Item targetItem = GetTargetItem();
		if (targetItem == null || Interface.CallHook("CanResearchItem", player, targetItem) != null || targetItem.amount > 1 || !IsItemResearchable(targetItem))
		{
			return;
		}
		Interface.CallHook("OnItemResearch", this, targetItem, player);
		if (!onlyOneUser || !((Object)(object)msg.player.inventory.loot.entitySource != (Object)(object)this))
		{
			targetItem.CollectedForCrafting(player);
			researchFinishedTime = Time.realtimeSinceStartup + researchDuration;
			Invoke(ResearchAttemptFinished, researchDuration);
			base.inventory.SetLocked(isLocked: true);
			int scrapCost = ScrapForResearch(targetItem);
			Facepunch.Rust.Analytics.Azure.OnResearchStarted(player, this, targetItem, scrapCost);
			SetFlagLocal(Flags.On, b: true);
			SendNetworkUpdate();
			player.inventory.loot.SendImmediate();
			if (researchStartEffect.isValid)
			{
				Effect.server.Run(researchStartEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			msg.player.GiveAchievement("RESEARCH_ITEM");
		}
	}

	public void ResearchAttemptFinished()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		Item targetItem = GetTargetItem();
		Item scrapItem = GetScrapItem();
		if (targetItem != null && scrapItem != null)
		{
			int num = ScrapForResearch(targetItem);
			object obj = Interface.CallHook("OnItemResearched", this, num);
			if (obj is int)
			{
				num = (int)obj;
			}
			if (scrapItem.amount >= num)
			{
				if (scrapItem.amount == num)
				{
					base.inventory.Remove(scrapItem);
					scrapItem.RemoveFromContainer();
					scrapItem.Remove();
				}
				else
				{
					scrapItem.UseItem(num);
				}
				base.inventory.Remove(targetItem);
				targetItem.Remove();
				Item item = ItemManager.Create(ItemManager.blueprintBaseDef, 1, 0uL, isServerSide: true, 0uL);
				if ((Object)(object)base.LastLootedByPlayer != (Object)null)
				{
					item.SetItemOwnership(base.LastLootedByPlayer, ItemOwnershipPhrases.ResearchTable);
				}
				item.blueprintTarget = (((Object)(object)targetItem.info.isRedirectOf != (Object)null) ? targetItem.info.isRedirectOf.itemid : targetItem.info.itemid);
				if (!item.MoveToContainer(base.inventory, 0))
				{
					item.Drop(GetDropPosition(), GetDropVelocity());
				}
				if (researchSuccessEffect.isValid)
				{
					Effect.server.Run(researchSuccessEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
				}
			}
		}
		SendNetworkUpdateImmediate();
		if ((Object)(object)user != (Object)null)
		{
			user.inventory.loot.SendImmediate();
		}
		EndResearch();
	}

	public void CancelResearch()
	{
	}

	public void EndResearch()
	{
		base.inventory.SetLocked(isLocked: false);
		SetFlagLocal(Flags.On, b: false);
		researchFinishedTime = 0f;
		SendNetworkUpdate();
		if ((Object)(object)user != (Object)null)
		{
			user.inventory.loot.SendImmediate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.researchTable = Pool.Get<ResearchTable>();
		info.msg.researchTable.researchTimeLeft = researchFinishedTime - info.cachedTime.RealTimeSinceStartup;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.researchTable != null)
		{
			researchFinishedTime = Time.realtimeSinceStartup + info.msg.researchTable.researchTimeLeft;
		}
	}
}
