using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Mailbox : StorageContainer
{
	public string ownerPanel;

	public GameObjectRef mailDropSound;

	public bool autoSubmitWhenClosed;

	public bool shouldMarkAsFull;

	public int InputSlotCount = 1;

	[NonSerialized]
	public ItemContainer InputContainer;

	public int mailInputSlot => 0;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Mailbox.OnRpcMessage"))
		{
			if (rpc == 131727457 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Submit"));
				}
				using (TimeWarning.New("RPC_Submit"))
				{
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
							RPC_Submit(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Submit");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool PlayerIsOwner(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseMailbox", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return player.CanBuild();
	}

	public bool IsFull()
	{
		if (shouldMarkAsFull)
		{
			return HasFlag(Flags.Reserved1);
		}
		return false;
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		return base.PlayerOpenLoot(player, PlayerIsOwner(player) ? ownerPanel : panelToOpen);
	}

	public override void AddContainers(PlayerLoot loot)
	{
		if (PlayerIsOwner(loot.GetCastedEntity()))
		{
			loot.AddContainer(base.inventory);
		}
		else
		{
			loot.AddContainer(InputContainer);
		}
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (panelName == ownerPanel)
		{
			if (PlayerIsOwner(player))
			{
				return base.CanOpenLootPanel(player, panelName);
			}
			return false;
		}
		if (!HasFreeSpace())
		{
			return !shouldMarkAsFull;
		}
		return true;
	}

	private bool HasFreeSpace()
	{
		if (base.inventory != null)
		{
			return !base.inventory.IsFull();
		}
		return false;
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (autoSubmitWhenClosed)
		{
			SubmitInputItems(player);
		}
		if (IsFull())
		{
			base.inventory.GetSlot(mailInputSlot)?.Drop(GetDropPosition(), GetDropVelocity());
		}
		base.PlayerStoppedLooting(player);
		if (PlayerIsOwner(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
	}

	[RPC_Server]
	public void RPC_Submit(RPCMessage msg)
	{
		if (!IsFull())
		{
			BasePlayer player = msg.player;
			SubmitInputItems(player);
		}
	}

	public void SubmitInputItems(BasePlayer fromPlayer)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < InputContainer.capacity; i++)
		{
			Item slot = InputContainer.GetSlot(i);
			if (slot == null || Interface.CallHook("OnItemSubmit", slot, this, fromPlayer) != null || !slot.MoveToContainer(base.inventory))
			{
				continue;
			}
			Effect.server.Run(mailDropSound.resourcePath, GetDropPosition());
			if ((Object)(object)fromPlayer != (Object)null && !PlayerIsOwner(fromPlayer))
			{
				using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.On, b: true);
			}
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		MarkFull(!HasFreeSpace());
		base.OnItemAddedOrRemoved(item, added);
	}

	public void MarkFull(bool full)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, shouldMarkAsFull & full);
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (allowedItems == null || allowedItems.Length == 0)
		{
			return base.ItemFilter(item, targetSlot);
		}
		ItemDefinition[] array = allowedItems;
		foreach (ItemDefinition itemDefinition in array)
		{
			if ((Object)(object)item.info == (Object)(object)itemDefinition)
			{
				return true;
			}
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		Mailbox val = Pool.Get<Mailbox>();
		val.inventory = InputContainer.Save();
		info.msg.mailbox = val;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.msg.mailbox != null && info.msg.mailbox.inventory != null)
		{
			InputContainer.Load(info.msg.mailbox.inventory);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (InputContainer == null)
		{
			InputContainer = Pool.Get<ItemContainer>();
			InputContainer.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
			InputContainer.SetOnlyAllowedItem(allowedItem);
			InputContainer.entityOwner = this;
			InputContainer.maxStackSize = maxStackSize;
			InputContainer.ServerInitialize(null, InputSlotCount);
			InputContainer.GiveUID();
			InputContainer.onDirty += OnInventoryDirty;
			InputContainer.onItemAddedRemoved = OnItemAddedOrRemoved;
			ItemContainer inputContainer = InputContainer;
			inputContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(inputContainer.canAcceptItem, new Func<Item, int, bool>(ItemFilter));
			OnInventoryFirstCreated(InputContainer);
		}
	}

	public override void GetAllInventories(List<ItemContainer> list)
	{
		base.GetAllInventories(list);
		list.Add(InputContainer);
	}
}
