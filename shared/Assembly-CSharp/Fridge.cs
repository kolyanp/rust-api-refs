using System.Collections.Generic;
using UnityEngine;

public class Fridge : ContainerIOEntity, IFoodSpoilModifier
{
	public ItemCategory OnlyAcceptCategory = ItemCategory.All;

	public List<ItemDefinition> IncludedItems = new List<ItemDefinition>();

	public int PowerConsumption = 5;

	[Range(0f, 1f)]
	public float PoweredFoodSpoilageRateMultiplier = 0.1f;

	public float GetSpoilMultiplier(Item arg)
	{
		if (IsPowered())
		{
			return PoweredFoodSpoilageRateMultiplier;
		}
		return 1f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = CanAcceptItem;
	}

	private bool CanAcceptItem(Item item, int targetSlot)
	{
		if (OnlyAcceptCategory == ItemCategory.All)
		{
			return true;
		}
		if (item.info.category != OnlyAcceptCategory)
		{
			return IsItemInAcceptedList(item);
		}
		return true;
	}

	private bool IsItemInAcceptedList(Item item)
	{
		foreach (ItemDefinition includedItem in IncludedItems)
		{
			if ((Object)(object)item.info == (Object)(object)includedItem)
			{
				return true;
			}
		}
		return false;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	private bool CanOpenLootPanel(BasePlayer player)
	{
		if (!CanBeLooted(player))
		{
			return false;
		}
		BaseLock baseLock = GetLock();
		if ((Object)(object)baseLock != (Object)null && !baseLock.OnTryToOpen(player))
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
			return false;
		}
		return true;
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (!CanOpenLootPanel(player))
		{
			return false;
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if ((Object)(object)GetSlot(Slot.Lock) != (Object)null)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasLock, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}
}
