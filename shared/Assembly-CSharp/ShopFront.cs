using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ShopFront : StorageContainer
{
	public static class ShopFrontFlags
	{
		public const Flags VendorAccepted = Flags.Reserved1;

		public const Flags CustomerAccepted = Flags.Reserved2;

		public const Flags Exchanging = Flags.Reserved3;
	}

	public float maxUseAngle = 27f;

	public BasePlayer vendorPlayer;

	public BasePlayer customerPlayer;

	public GameObjectRef transactionCompleteEffect;

	[NonSerialized]
	public ItemContainer customerInventory;

	private bool swappingItems;

	private static readonly Phrase TradeLockedError;

	private static readonly Phrase TradeUnsuccessfulError;

	private float AngleDotProduct => 1f - maxUseAngle / 90f;

	public ItemContainer vendorInventory => base.inventory;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ShopFront.OnRpcMessage"))
		{
			if (rpc == 1159607245 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AcceptClicked"));
				}
				using (TimeWarning.New("AcceptClicked"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1159607245u, "AcceptClicked", this, player, 3f))
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
							AcceptClicked(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AcceptClicked");
					}
				}
				return true;
			}
			if (rpc == 3168107540u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CancelClicked"));
				}
				using (TimeWarning.New("CancelClicked"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3168107540u, "CancelClicked", this, player, 3f))
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
							CancelClicked(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in CancelClicked");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool TradeLocked()
	{
		return false;
	}

	public bool IsTradingPlayer(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			if (!IsPlayerCustomer(player))
			{
				return IsPlayerVendor(player);
			}
			return true;
		}
		return false;
	}

	public bool IsPlayerCustomer(BasePlayer player)
	{
		return (Object)(object)player == (Object)(object)customerPlayer;
	}

	public bool IsPlayerVendor(BasePlayer player)
	{
		return (Object)(object)player == (Object)(object)vendorPlayer;
	}

	public bool PlayerInVendorPos(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 right = ((Component)this).transform.right;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		return Vector3.Dot(right, ((Vector3)(ref val)).normalized) <= 0f - AngleDotProduct;
	}

	public bool PlayerInCustomerPos(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 right = ((Component)this).transform.right;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		return Vector3.Dot(right, ((Vector3)(ref val)).normalized) >= AngleDotProduct;
	}

	public bool LootEligable(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (PlayerInVendorPos(player) && (Object)(object)vendorPlayer == (Object)null)
		{
			return true;
		}
		if (PlayerInCustomerPos(player) && (Object)(object)customerPlayer == (Object)null)
		{
			return true;
		}
		return false;
	}

	public void ResetTrade()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		vendorInventory.SetLocked(isLocked: false);
		customerInventory.SetLocked(isLocked: false);
		CancelInvoke(CompleteTrade);
	}

	public void CompleteTrade()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)vendorPlayer != (Object)null && (Object)(object)customerPlayer != (Object)null && HasFlag(Flags.Reserved1) && HasFlag(Flags.Reserved2))
		{
			if (Interface.CallHook("OnShopCompleteTrade", this) != null)
			{
				return;
			}
			try
			{
				swappingItems = true;
				for (int num = vendorInventory.capacity - 1; num >= 0; num--)
				{
					Item slot = vendorInventory.GetSlot(num);
					Item slot2 = customerInventory.GetSlot(num);
					if (Object.op_Implicit((Object)(object)customerPlayer) && slot != null)
					{
						customerPlayer.GiveItem(slot);
					}
					if (Object.op_Implicit((Object)(object)vendorPlayer) && slot2 != null)
					{
						vendorPlayer.GiveItem(slot2);
					}
				}
			}
			finally
			{
				swappingItems = false;
			}
			Effect.server.Run(transactionCompleteEffect.resourcePath, this, 0u, new Vector3(0f, 1f, 0f), Vector3.zero);
		}
		ResetTrade();
		SendNetworkUpdate();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void AcceptClicked(RPCMessage msg)
	{
		if (!IsTradingPlayer(msg.player) || (Object)(object)vendorPlayer == (Object)null || (Object)(object)customerPlayer == (Object)null || Interface.CallHook("OnShopAcceptClick", this, msg.player) != null)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (IsPlayerVendor(msg.player))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
			vendorInventory.SetLocked(isLocked: true);
		}
		else if (IsPlayerCustomer(msg.player))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
			customerInventory.SetLocked(isLocked: true);
		}
		if (HasFlag(Flags.Reserved1) && HasFlag(Flags.Reserved2))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
			Invoke(CompleteTrade, 2f);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void CancelClicked(RPCMessage msg)
	{
		if (IsTradingPlayer(msg.player) && Interface.CallHook("OnShopCancelClick", this, msg.player) == null)
		{
			Object.op_Implicit((Object)(object)vendorPlayer);
			Object.op_Implicit((Object)(object)customerPlayer);
			ResetTrade();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = vendorInventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptVendorItem));
		if (customerInventory == null)
		{
			customerInventory = Pool.Get<ItemContainer>();
			customerInventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
			customerInventory.SetOnlyAllowedItem(allowedItem);
			customerInventory.entityOwner = this;
			customerInventory.maxStackSize = maxStackSize;
			customerInventory.ServerInitialize(null, inventorySlots);
			customerInventory.GiveUID();
			customerInventory.onDirty += OnInventoryDirty;
			customerInventory.onItemAddedRemoved = OnItemAddedOrRemoved;
			customerInventory.onItemAddedToStack = OnItemAddedToStack;
			customerInventory.onItemRemovedFromStack = OnItemRemovedFromStack;
			ItemContainer itemContainer2 = customerInventory;
			itemContainer2.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer2.canAcceptItem, new Func<Item, int, bool>(CanAcceptCustomerItem));
			OnInventoryFirstCreated(customerInventory);
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Pool.Free<ItemContainer>(ref customerInventory);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		ResetTrade();
	}

	public override void OnItemAddedToStack(Item item, int amount)
	{
		base.OnItemAddedToStack(item, amount);
		ResetTrade();
	}

	public override void OnItemRemovedFromStack(Item item, int amount)
	{
		base.OnItemRemovedFromStack(item, amount);
		ResetTrade();
	}

	private bool CanAcceptVendorItem(Item item, int targetSlot)
	{
		if (swappingItems || ((Object)(object)vendorPlayer != (Object)null && (Object)(object)item.GetOwnerPlayer() == (Object)(object)vendorPlayer) || vendorInventory.itemList.Contains(item))
		{
			return true;
		}
		return false;
	}

	private bool CanAcceptCustomerItem(Item item, int targetSlot)
	{
		if (swappingItems || ((Object)(object)customerPlayer != (Object)null && (Object)(object)item.GetOwnerPlayer() == (Object)(object)customerPlayer) || customerInventory.itemList.Contains(item))
		{
			return true;
		}
		return false;
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		if (TradeLocked())
		{
			return PlayerInventory.CanMoveFromResponse.Failure(TradeLockedError);
		}
		if (IsTradingPlayer(player))
		{
			if (IsPlayerCustomer(player) && customerInventory.itemList.Contains(item) && !customerInventory.IsLocked())
			{
				return PlayerInventory.CanMoveFromResponse.Success();
			}
			if (IsPlayerVendor(player) && vendorInventory.itemList.Contains(item) && !vendorInventory.IsLocked())
			{
				return PlayerInventory.CanMoveFromResponse.Success();
			}
		}
		return PlayerInventory.CanMoveFromResponse.Failure(TradeUnsuccessfulError);
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (base.CanOpenLootPanel(player, panelName))
		{
			return LootEligable(player);
		}
		return false;
	}

	public void ReturnPlayerItems(BasePlayer player)
	{
		if (!IsTradingPlayer(player))
		{
			return;
		}
		ItemContainer itemContainer = null;
		if (IsPlayerVendor(player))
		{
			itemContainer = vendorInventory;
		}
		else if (IsPlayerCustomer(player))
		{
			itemContainer = customerInventory;
		}
		if (itemContainer != null)
		{
			for (int num = itemContainer.itemList.Count - 1; num >= 0; num--)
			{
				Item item = itemContainer.itemList[num];
				player.GiveItem(item);
			}
		}
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		if (IsTradingPlayer(player))
		{
			ReturnPlayerItems(player);
			if ((Object)(object)player == (Object)(object)vendorPlayer)
			{
				vendorPlayer = null;
			}
			if ((Object)(object)player == (Object)(object)customerPlayer)
			{
				customerPlayer = null;
			}
			UpdatePlayers();
			ResetTrade();
			base.PlayerStoppedLooting(player);
		}
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		bool flag = base.PlayerOpenLoot(player, panelToOpen);
		if (flag)
		{
			player.inventory.loot.AddContainer(customerInventory);
			player.inventory.loot.SendImmediate();
		}
		if (PlayerInVendorPos(player) && (Object)(object)vendorPlayer == (Object)null)
		{
			vendorPlayer = player;
		}
		else
		{
			if (!PlayerInCustomerPos(player) || !((Object)(object)customerPlayer == (Object)null))
			{
				return false;
			}
			customerPlayer = player;
		}
		ResetTrade();
		UpdatePlayers();
		return flag;
	}

	public void UpdatePlayers()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_ReceivePlayers"), (NetworkableId)(((Object)(object)vendorPlayer == (Object)null) ? default(NetworkableId) : vendorPlayer.net.ID), (NetworkableId)(((Object)(object)customerPlayer == (Object)null) ? default(NetworkableId) : customerPlayer.net.ID));
	}

	public override void GetAllInventories(List<ItemContainer> list)
	{
		base.GetAllInventories(list);
		list.Add(customerInventory);
	}

	static ShopFront()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		TradeLockedError = new Phrase("error.tradelocked", "Trade Locked!");
		TradeUnsuccessfulError = new Phrase("error.tradeunsuccessful", "Trade Unsuccessful!");
	}
}
