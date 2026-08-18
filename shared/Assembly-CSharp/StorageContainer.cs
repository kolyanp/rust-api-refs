using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class StorageContainer : DecayEntity, IItemContainerEntity, IIdealSlotEntity, ILootableEntity, IInventoryProvider, LootPanel.IHasLootPanel, IContainerSounds, IIndustrialStorageCallbackReceiver, PlayerInventory.ICanMoveFrom
{
	[Header("Storage Container")]
	public static readonly Phrase LockedMessage;

	public static readonly Phrase InUseMessage;

	public int inventorySlots;

	public bool dropsLoot;

	public float dropLootDestroyPercent;

	public bool dropFloats;

	public bool isLootable;

	public bool isLockable;

	public bool isMonitorable;

	public string panelName;

	public Phrase panelTitle;

	public ItemContainer.ContentsType allowedContents;

	public ItemDefinition allowedItem;

	public ItemDefinition allowedItem2;

	public ItemDefinition[] allowedItems;

	public ItemDefinition[] blockedItems;

	public int maxStackSize;

	public bool needsBuildingPrivilegeToUse;

	public bool requireAuthIfNotLocked;

	public bool allowSorting;

	[ServerVar(Help = "(Generated) When enabled, storage containers without a lock can still require tool cupboard auth to access; default false")]
	public static bool canRequireAuthIfNoLock;

	public bool mustBeMountedToUse;

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	[Header("Item Dropping")]
	public Vector3 dropPosition;

	public Vector3 dropVelocity;

	public ItemCategory onlyAcceptCategory;

	public bool onlyOneUser;

	[ServerVar(Help = "(Generated) When enabled, storage containers that die spawn a loot corpse containing their items; when false items are destroyed")]
	public static bool dropCorpseOnDeath;

	[ServerVar(Help = "(Generated) Fraction of items preserved when a storage container spawns a death corpse; 0.5 = 50% of items survive the container death")]
	public static float corpseItemsSavedPercent;

	public ItemContainer _inventory;

	public bool isTransferringIndustrialItem;

	private bool triggerInventoryDirty;

	private IndustrialStorageAdaptor cachedAdaptor;

	public Phrase LootPanelTitle => panelTitle;

	public ItemContainer inventory => _inventory;

	public Transform Transform => ((Component)this).transform;

	public bool DropsLoot => dropsLoot;

	public bool DropFloats => dropFloats;

	public float DestroyLootPercent
	{
		get
		{
			if (!ShouldDropDeployableCorpse(lastAttacker, lastDamage))
			{
				return dropLootDestroyPercent;
			}
			return 0f;
		}
	}

	public ulong LastLootedBy { get; set; }

	public BasePlayer LastLootedByPlayer { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StorageContainer.OnRpcMessage"))
		{
			if (rpc == 331989034 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenLoot"));
				}
				using (TimeWarning.New("RPC_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(331989034u, "RPC_OpenLoot", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenLoot(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(dropPosition, Vector3.one * 0.1f);
		Gizmos.color = Color.white;
		Gizmos.DrawRay(dropPosition, dropVelocity);
	}

	public override bool HasSlot(Slot slot)
	{
		if (isLockable && slot == Slot.Lock)
		{
			return true;
		}
		if (isMonitorable && slot == Slot.StorageMonitor)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public bool MoveAllInventoryItems(ItemContainer from)
	{
		return MoveAllInventoryItems(from, _inventory);
	}

	public static bool MoveAllInventoryItems(ItemContainer source, ItemContainer dest)
	{
		bool flag = true;
		for (int i = 0; i < Mathf.Min(source.capacity, dest.capacity); i++)
		{
			Item slot = source.GetSlot(i);
			if (slot != null)
			{
				bool flag2 = slot.MoveToContainer(dest);
				if (flag && !flag2)
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	public virtual void ReceiveInventoryFromItem(Item item)
	{
		if (item.contents != null)
		{
			MoveAllInventoryItems(item.contents, _inventory);
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		bool num = (Object)(object)GetSlot(Slot.Lock) != (Object)null;
		pickupErrorToFormat.arg0 = pickup.itemTarget.displayName;
		if (num)
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasLock;
			return false;
		}
		if (pickup.requireEmptyInv && _inventory != null && !inventory.IsEmpty())
		{
			pickupErrorToFormat.format = PickupErrors.ItemInventoryMustBeEmpty;
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		if (createdItem != null && createdItem.contents != null)
		{
			MoveAllInventoryItems(_inventory, createdItem.contents);
			return;
		}
		for (int i = 0; i < _inventory.capacity; i++)
		{
			Item slot = _inventory.GetSlot(i);
			if (slot != null)
			{
				slot.RemoveFromContainer();
				player.GiveItem(slot, GiveItemReason.PickedUp);
			}
		}
	}

	public override void ServerInit()
	{
		if (_inventory == null)
		{
			CreateInventory(giveUID: true);
			OnInventoryFirstCreated(_inventory);
		}
		else
		{
			_inventory.SetBlacklist(blockedItems);
		}
		base.ServerInit();
	}

	public virtual void OnInventoryFirstCreated(ItemContainer container)
	{
	}

	public virtual void OnItemAddedOrRemoved(Item item, bool added)
	{
	}

	public virtual void OnItemAddedToStack(Item item, int amount)
	{
	}

	public virtual void OnItemRemovedFromStack(Item item, int amount)
	{
	}

	public virtual void OnItemPositionChanged(Item item, int from, int to)
	{
	}

	public virtual bool ItemFilter(Item item, int targetSlot)
	{
		object obj = Interface.CallHook("OnItemFilter", item, this, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (onlyAcceptCategory == ItemCategory.All)
		{
			return true;
		}
		return item.info.category == onlyAcceptCategory;
	}

	public void CreateInventory(bool giveUID)
	{
		Debug.Assert(_inventory == null, "Double init of inventory!");
		_inventory = Pool.Get<ItemContainer>();
		_inventory.entityOwner = this;
		_inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		_inventory.SetOnlyAllowedItems(allowedItems, allowedItem, allowedItem2);
		_inventory.SetBlacklist(blockedItems);
		_inventory.maxStackSize = maxStackSize;
		_inventory.ServerInitialize(null, inventorySlots);
		if (giveUID)
		{
			_inventory.GiveUID();
		}
		_inventory.onDirty += OnInventoryDirty;
		_inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		_inventory.onItemAddedToStack = OnItemAddedToStack;
		_inventory.onItemRemovedFromStack = OnItemRemovedFromStack;
		_inventory.onItemPositionChanged = OnItemPositionChanged;
		_inventory.canAcceptItem = ItemFilter;
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(giveUID: false);
	}

	public void OnIndustrialItemTransferBegins()
	{
		isTransferringIndustrialItem = true;
		triggerInventoryDirty = false;
	}

	public void OnIndustrialItemTransferEnd()
	{
		isTransferringIndustrialItem = false;
		if (triggerInventoryDirty)
		{
			OnInventoryDirty();
		}
	}

	protected virtual void OnInventoryDirty()
	{
		if (isTransferringIndustrialItem)
		{
			triggerInventoryDirty = true;
			return;
		}
		if (HasAttachedStorageAdaptor(out var adaptor))
		{
			adaptor.RequestSort();
		}
		InvalidateNetworkCache();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (_inventory != null && !((ItemContainerId)(ref _inventory.uid)).IsValid)
		{
			_inventory.GiveUID();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Open, b: false);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (_inventory != null)
		{
			Pool.Free<ItemContainer>(ref _inventory);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (isLootable)
		{
			BasePlayer player = rpc.player;
			if (Object.op_Implicit((Object)(object)player) && player.CanInteract())
			{
				PlayerOpenLoot(player);
			}
		}
	}

	public virtual string GetPanelName()
	{
		return panelName;
	}

	public virtual PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		return new PlayerInventory.CanMoveFromResponse(!_inventory.IsLocked(), PlayerInventoryErrors.ContainerLocked);
	}

	public virtual bool CanOpenLootPanel(BasePlayer player, string panelName)
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

	public override bool CanBeLooted(BasePlayer player)
	{
		BaseEntity baseEntity = GetParentEntity();
		while ((Object)(object)baseEntity != (Object)null)
		{
			if (baseEntity is BaseVehicleModule baseVehicleModule && !baseVehicleModule.CanBeLooted(player))
			{
				return false;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		if ((needsBuildingPrivilegeToUse || ShouldRequireAuthIfNoCodelock(this, requireAuthIfNotLocked)) && !player.CanBuild())
		{
			return false;
		}
		if (mustBeMountedToUse && !player.isMounted)
		{
			return false;
		}
		return base.CanBeLooted(player);
	}

	public virtual void AddContainers(PlayerLoot loot)
	{
		loot.AddContainer(_inventory);
	}

	public virtual bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (Interface.CallHook("CanLootEntity", player, this) != null)
		{
			return false;
		}
		if (IsLocked() || IsTransferring())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, LockedMessage, false);
			return false;
		}
		if (onlyOneUser && IsOpen())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, InUseMessage, false);
			return false;
		}
		if (panelToOpen == "")
		{
			panelToOpen = panelName;
		}
		if (!CanOpenLootPanel(player, panelToOpen))
		{
			return false;
		}
		if (player.inventory.loot.StartLootingEntity(this, doPositionChecks))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Open, b: true);
			}
			AddContainers(player.inventory.loot);
			player.inventory.loot.SendImmediate();
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), panelToOpen);
			SendNetworkUpdate();
			return true;
		}
		return false;
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		SetFlagLocal(Flags.Open, b: false);
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (_inventory != null)
			{
				info.msg.storageBox = Pool.Get<StorageBox>();
				info.msg.storageBox.contents = _inventory.Save();
			}
			else
			{
				Debug.LogWarning((object)("Storage container without inventory: " + ((object)this).ToString()));
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		DropItems(info?.Initiator);
		base.OnDied(info);
	}

	public override void OnDeployableCorpseSpawned(BaseEntity corpse)
	{
		base.OnDeployableCorpseSpawned(corpse);
		if (corpse is ContainerCorpse containerCorpse)
		{
			containerCorpse.TakeFrom(new ItemContainer[1] { inventory }, corpseItemsSavedPercent);
		}
	}

	public virtual void DropItems(BaseEntity initiator = null)
	{
		DropItems(this, initiator);
	}

	public static void DropItems(IItemContainerEntity containerEntity, BaseEntity initiator = null)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		ItemContainer itemContainer = containerEntity.inventory;
		if (itemContainer == null || itemContainer.itemList == null || itemContainer.itemList.Count == 0 || !containerEntity.DropsLoot)
		{
			return;
		}
		if (containerEntity.ShouldDropItemsIndividually() || (itemContainer.itemList.Count == 1 && !containerEntity.DropFloats))
		{
			if ((Object)(object)initiator != (Object)null)
			{
				containerEntity.DropBonusItems(initiator, itemContainer);
			}
			DropUtil.DropItems(itemContainer, containerEntity.GetDropPosition());
		}
		else
		{
			string prefab = (containerEntity.DropFloats ? "assets/prefabs/misc/item drop/item_drop_buoyant.prefab" : "assets/prefabs/misc/item drop/item_drop.prefab");
			_ = (Object)(object)itemContainer.Drop(prefab, containerEntity.GetDropPosition(), containerEntity.Transform.rotation, containerEntity.DestroyLootPercent) != (Object)null;
		}
	}

	public virtual void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
	}

	public override Vector3 GetDropPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint(dropPosition);
	}

	public override Vector3 GetDropVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 inheritedDropVelocity = GetInheritedDropVelocity();
		Matrix4x4 localToWorldMatrix = ((Component)this).transform.localToWorldMatrix;
		return inheritedDropVelocity + ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(dropPosition);
	}

	public virtual bool ShouldDropItemsIndividually()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.storageBox != null)
		{
			if (_inventory != null)
			{
				_inventory.Load(info.msg.storageBox.contents);
				_inventory.capacity = inventorySlots;
			}
			else
			{
				Debug.LogWarning((object)("Storage container without inventory: " + ((object)this).ToString()));
			}
		}
	}

	public virtual int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		return -1;
	}

	public virtual ItemContainerId GetIdealContainer(BasePlayer player, Item item, ItemMoveModifier modifier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return default(ItemContainerId);
	}

	public virtual void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(inventory);
	}

	public static bool ShouldRequireAuthIfNoCodelock(BaseEntity container, bool containerRequiresAuth)
	{
		if (canRequireAuthIfNoLock & containerRequiresAuth)
		{
			return (Object)(object)container.GetLock() == (Object)null;
		}
		return false;
	}

	public bool OccupiedCheck(BasePlayer player = null)
	{
		if ((Object)(object)player != (Object)null && (Object)(object)player.inventory.loot.entitySource == (Object)(object)this)
		{
			return true;
		}
		if (onlyOneUser)
		{
			return !IsOpen();
		}
		return true;
	}

	protected bool HasAttachedStorageAdaptor()
	{
		if (children == null)
		{
			return false;
		}
		foreach (BaseEntity child in children)
		{
			if (child is IndustrialStorageAdaptor)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAttachedStorageAdaptor(out IndustrialStorageAdaptor adaptor)
	{
		using (TimeWarning.New("HasAttachedStorageAdaptor"))
		{
			if ((Object)(object)cachedAdaptor != (Object)null)
			{
				adaptor = cachedAdaptor;
				return true;
			}
			adaptor = null;
			if (children == null || children.Count == 0)
			{
				return false;
			}
			foreach (BaseEntity child in children)
			{
				if (child is IndustrialStorageAdaptor industrialStorageAdaptor)
				{
					adaptor = industrialStorageAdaptor;
					cachedAdaptor = industrialStorageAdaptor;
					return true;
				}
			}
			return false;
		}
	}

	protected bool HasAttachedStorageMonitor()
	{
		if (children == null)
		{
			return false;
		}
		foreach (BaseEntity child in children)
		{
			if (child is StorageMonitor)
			{
				return true;
			}
		}
		return false;
	}

	public StorageContainer()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		inventorySlots = 6;
		dropsLoot = true;
		isLootable = true;
		isLockable = true;
		panelName = "generic";
		panelTitle = new Phrase("loot", "Loot");
		dropVelocity = Vector3.forward;
		onlyAcceptCategory = ItemCategory.All;
		base._002Ector();
	}

	static StorageContainer()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		LockedMessage = new Phrase("storage.locked", "Can't loot right now");
		InUseMessage = new Phrase("storage.in_use", "Already in use");
		canRequireAuthIfNoLock = false;
		corpseItemsSavedPercent = 0.5f;
	}
}
