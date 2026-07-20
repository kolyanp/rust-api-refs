using System;
using Facepunch.Rust;
using Oxide.Core;
using UnityEngine;

public class PlayerBelt
{
	public static int ClientAutoSelectSlot = -1;

	public static uint ClientAutoSeletItemUID = 0u;

	public static EncryptedValue<int> SelectedSlot = -1;

	protected BasePlayer player;

	public static int MaxBeltSlots => 6;

	public PlayerBelt(BasePlayer player)
	{
		this.player = player;
	}

	public void DropActive(Vector3 position, Vector3 velocity)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (player.TryGetActiveShield(out var foundShield))
		{
			if (Interface.CallHook("OnPlayerActiveShieldDrop", player, foundShield) != null)
			{
				return;
			}
			DroppedItem droppedItem = foundShield.GetItem().Drop(position, velocity) as DroppedItem;
			if ((Object)(object)droppedItem != (Object)null)
			{
				droppedItem.DropReason = DroppedItem.DropReasonEnum.Death;
				droppedItem.DroppedBy = player.userID;
				droppedItem.DroppedTime = DateTime.UtcNow;
				Facepunch.Rust.Analytics.Azure.OnItemDropped(player, droppedItem, DroppedItem.DropReasonEnum.Death);
			}
		}
		Item activeItem = player.GetActiveItem();
		if (activeItem == null || Interface.CallHook("OnPlayerDropActiveItem", player, activeItem) != null)
		{
			return;
		}
		if (activeItem.GetHeldEntity() is Boomerang { HasThrown: not false })
		{
			activeItem.UseItem();
			if (activeItem.amount == 0)
			{
				activeItem.SetParent(null);
			}
			player.svActiveItemID = default(ItemId);
			player.SendNetworkUpdate();
			return;
		}
		using (TimeWarning.New("PlayerBelt.DropActive"))
		{
			DroppedItem droppedItem2 = activeItem.Drop(position, velocity) as DroppedItem;
			if ((Object)(object)droppedItem2 != (Object)null)
			{
				droppedItem2.DropReason = DroppedItem.DropReasonEnum.Death;
				droppedItem2.DroppedBy = player.userID;
				droppedItem2.DroppedTime = DateTime.UtcNow;
				Facepunch.Rust.Analytics.Azure.OnItemDropped(player, droppedItem2, DroppedItem.DropReasonEnum.Death);
			}
			player.svActiveItemID = default(ItemId);
			player.SendNetworkUpdate();
		}
	}

	public Item GetItemInSlot(int slot)
	{
		if ((Object)(object)player == (Object)null)
		{
			return null;
		}
		if ((Object)(object)player.inventory == (Object)null)
		{
			return null;
		}
		if (player.inventory.containerBelt == null)
		{
			return null;
		}
		return player.inventory.containerBelt.GetSlot(slot);
	}

	public Handcuffs GetRestraintItem()
	{
		if ((Object)(object)player == (Object)null)
		{
			return null;
		}
		if ((Object)(object)player.inventory == (Object)null)
		{
			return null;
		}
		if (player.inventory.containerBelt == null)
		{
			return null;
		}
		foreach (Item item in player.inventory.containerBelt.itemList)
		{
			if (item != null)
			{
				Handcuffs handcuffs = item.GetHeldEntity() as Handcuffs;
				if (!((Object)(object)handcuffs == (Object)null) && handcuffs.Locked)
				{
					return handcuffs;
				}
			}
		}
		return null;
	}

	public bool CanHoldItem()
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (player.IsWounded())
		{
			return false;
		}
		if (player.IsSleeping())
		{
			return false;
		}
		if (player.isMounted && !player.GetMounted().CanHoldItems())
		{
			return false;
		}
		return true;
	}
}
