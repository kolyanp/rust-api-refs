using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerInventory : EntityComponent<BasePlayer>, IAmmoContainer
{
	public enum Type
	{
		Main,
		Belt,
		Wear,
		BackpackContents
	}

	public readonly struct CanMoveFromResponse
	{
		public readonly bool allowed;

		public readonly Phrase reasonForFailure;

		public static CanMoveFromResponse Success()
		{
			return new CanMoveFromResponse(allowed: true, null);
		}

		public static CanMoveFromResponse Failure(Phrase reasonForFailure)
		{
			return new CanMoveFromResponse(allowed: false, reasonForFailure);
		}

		public CanMoveFromResponse(bool allowed, Phrase reasonForFailure)
		{
			this.allowed = allowed;
			this.reasonForFailure = reasonForFailure;
		}
	}

	public interface ICanMoveFrom
	{
		CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item);
	}

	public enum NetworkInventoryMode
	{
		LocalPlayer,
		Everyone,
		EveryoneButLocal
	}

	private struct WearCheckResult
	{
		public bool Result;

		public List<Item> ChangedItem;
	}

	public ItemContainer containerMain;

	public ItemContainer containerBelt;

	public ItemContainer containerWear;

	public ItemCrafter crafting;

	public PlayerLoot loot;

	public static Phrase BackpackGroundedError = new Phrase("error.backpackGrounded", "You must be on a solid surface to equip a backpack");

	public float inventoryRadioactivity;

	public bool containsRadioactiveItems;

	private static Comparison<HeldEntity> hostileComparer = null;

	private Action _updatedVisibleHolsteredItemsCallback;

	private Action _deferredServerUpdateAction;

	private static BufferList<Item> multiContainerBuffer = new BufferList<Item>(128);

	private List<Item> returnItems;

	[ServerVar(Help = "(Generated) When enabled, forces the birthday event state to true regardless of the actual date; overrides IsBirthday() calendar check for testing")]
	public static bool forceBirthday = false;

	[ServerVar(Help = "(Generated) When enabled, players can directionally drop items by looking in the desired direction; disable to revert to gravity-only drops")]
	public static bool directionalDropEnabled = true;

	private static float nextCheckTime = 0f;

	private static bool wasBirthday = false;

	private Action DeferredServerUpdateAction => DeferredServerUpdate;

	public event Action<float, bool> onRadioactivityChanged;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerInventory.OnRpcMessage"))
		{
			if (rpc == 3482449460u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ItemCmd"));
				}
				using (TimeWarning.New("ItemCmd"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.FromOwner.Test(3482449460u, "ItemCmd", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg2 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ItemCmd(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ItemCmd");
					}
				}
				return true;
			}
			if (rpc == 3041092525u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - MoveItem"));
				}
				using (TimeWarning.New("MoveItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.FromOwner.Test(3041092525u, "MoveItem", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg3 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							MoveItem(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in MoveItem");
					}
				}
				return true;
			}
			if (rpc == 2137592151 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdateAccessoryOnItem"));
				}
				using (TimeWarning.New("UpdateAccessoryOnItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.CallsPerSecond.Test(2137592151u, "UpdateAccessoryOnItem", GetBaseEntity(), player, 2uL))
						{
							return true;
						}
						if (!BaseEntity.RPC_Server.FromOwner.Test(2137592151u, "UpdateAccessoryOnItem", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg4 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UpdateAccessoryOnItem(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in UpdateAccessoryOnItem");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected void Initialize(BasePlayer owner)
	{
		Debug.Assert(containerMain == null, "Double init of main container!");
		containerMain = Pool.Get<ItemContainer>();
		containerMain.SetFlag(ItemContainer.Flag.IsPlayer, b: true);
		Debug.Assert(containerBelt == null, "Double init of belt container!");
		containerBelt = Pool.Get<ItemContainer>();
		containerBelt.SetFlag(ItemContainer.Flag.IsPlayer, b: true);
		containerBelt.SetFlag(ItemContainer.Flag.Belt, b: true);
		Debug.Assert(containerWear == null, "Double init of wear container!");
		containerWear = Pool.Get<ItemContainer>();
		containerWear.SetFlag(ItemContainer.Flag.IsPlayer, b: true);
		containerWear.SetFlag(ItemContainer.Flag.Clothing, b: true);
		containerWear.containerVolume = 2;
		crafting = ((Component)this).GetComponent<ItemCrafter>();
		if ((Object)(object)crafting != (Object)null)
		{
			crafting.owner = owner;
			crafting.AddContainer(containerMain);
			crafting.AddContainer(containerBelt);
		}
		loot = ((Component)this).GetComponent<PlayerLoot>();
		if (!Object.op_Implicit((Object)(object)loot))
		{
			loot = base.baseEntity.AddComponent<PlayerLoot>();
		}
	}

	public void DoDestroy()
	{
		if (containerMain != null)
		{
			Pool.Free<ItemContainer>(ref containerMain);
		}
		if (containerBelt != null)
		{
			Pool.Free<ItemContainer>(ref containerBelt);
		}
		if (containerWear != null)
		{
			Pool.Free<ItemContainer>(ref containerWear);
		}
	}

	public void SetLockedByRestraint(bool flag)
	{
		containerMain.SetLocked(flag, lockSubItems: true);
		containerWear.SetLocked(flag, lockSubItems: true);
		containerBelt.SetLocked(flag, lockSubItems: true);
		GetContainer(Type.BackpackContents)?.SetLocked(flag, lockSubItems: true);
	}

	public void ServerInit(BasePlayer owner)
	{
		Initialize(owner);
		containerMain.ServerInitialize(null, 24);
		if (!((ItemContainerId)(ref containerMain.uid)).IsValid)
		{
			containerMain.GiveUID();
		}
		containerBelt.ServerInitialize(null, 6);
		if (!((ItemContainerId)(ref containerBelt.uid)).IsValid)
		{
			containerBelt.GiveUID();
		}
		containerWear.ServerInitialize(null, 8);
		if (!((ItemContainerId)(ref containerWear.uid)).IsValid)
		{
			containerWear.GiveUID();
		}
		containerMain.playerOwner = owner;
		containerBelt.playerOwner = owner;
		containerWear.playerOwner = owner;
		containerWear.onItemContentsChanged = OnClothingItemContentsChanged;
		containerWear.onItemAddedRemoved = OnClothingChanged;
		containerWear.canAcceptItem = CanWearItem;
		containerBelt.canAcceptItem = CanEquipItem;
		containerMain.canAcceptItem = CanStoreInInventory;
		containerMain.onPreItemRemove = OnItemRemoved;
		containerWear.onPreItemRemove = OnItemRemoved;
		containerBelt.onPreItemRemove = OnItemRemoved;
		containerMain.onDirty += OnContentsDirty;
		containerBelt.onDirty += OnContentsDirty;
		containerWear.onDirty += OnContentsDirty;
		containerBelt.onItemAddedRemoved = OnItemAddedOrRemoved;
		containerMain.onItemAddedRemoved = OnItemAddedOrRemoved;
		ItemContainer itemContainer = containerWear;
		itemContainer.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(itemContainer.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedOrRemoved));
		containerWear.onItemRadiationChanged = OnItemRadiationChanged;
		containerBelt.onItemRadiationChanged = OnItemRadiationChanged;
		containerMain.onItemRadiationChanged = OnItemRadiationChanged;
		onRadioactivityChanged += owner.PlayerInventoryRadioactivityChange;
		CalculateInventoryRadioactivity();
	}

	public void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		if (item != null && (item.radioactivity > 0f || item.contents != null))
		{
			CalculateInventoryRadioactivity();
		}
		if (item.info.isHoldable)
		{
			if (_updatedVisibleHolsteredItemsCallback == null)
			{
				_updatedVisibleHolsteredItemsCallback = UpdatedVisibleHolsteredItems;
			}
			Invoke(_updatedVisibleHolsteredItemsCallback, 0.1f);
		}
		if (item.parent == containerBelt)
		{
			OnBeltItemAddedOrRemoved(item, bAdded);
		}
		if (bAdded)
		{
			BasePlayer basePlayer = base.baseEntity;
			if (!basePlayer.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) && basePlayer.IsHostileItem(item))
			{
				base.baseEntity.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, b: true);
			}
			if (bAdded)
			{
				basePlayer.ProcessMissionEvent(BaseMission.MissionEventType.ACQUIRE_ITEM, item.info.itemid, item.amount);
			}
		}
	}

	private void OnBeltItemAddedOrRemoved(Item item, bool added)
	{
		if (!added)
		{
			return;
		}
		ItemModForceWearFromBelt component = ((Component)item.info).GetComponent<ItemModForceWearFromBelt>();
		if (!((Object)(object)component == (Object)null) && (!component.IfPlayerRestrained || base.baseEntity.IsRestrained))
		{
			bool num = containerWear.IsLocked();
			if (num)
			{
				containerWear.SetLocked(isLocked: false);
			}
			if (!item.MoveToContainer(containerWear))
			{
				item.MoveToContainer(containerWear, 0, allowStack: false);
			}
			if (num)
			{
				containerWear.SetLocked(isLocked: true);
			}
		}
	}

	private static int CompareHostility(HeldEntity a, HeldEntity b)
	{
		if ((Object)(object)a == (Object)null || (Object)(object)b == (Object)null)
		{
			return 0;
		}
		if (a.hostileScore < b.hostileScore)
		{
			return 1;
		}
		if (a.hostileScore > b.hostileScore)
		{
			return -1;
		}
		return 0;
	}

	public void UpdatedVisibleHolsteredItems()
	{
		List<HeldEntity> list = Pool.Get<List<HeldEntity>>();
		List<Item> list2 = Pool.Get<List<Item>>();
		GetAllItems(list2);
		AddBackpackContentsToList(list2);
		foreach (Item item in list2)
		{
			if (item.info.isHoldable && !((Object)(object)item.GetHeldEntity() == (Object)null))
			{
				HeldEntity component = ((Component)item.GetHeldEntity()).GetComponent<HeldEntity>();
				if (!((Object)(object)component == (Object)null) && (!component.IsShield || containerWear.itemList.Contains(item)))
				{
					list.Add(component);
				}
			}
		}
		Pool.Free<Item>(ref list2, false);
		using (TimeWarning.New("Sort"))
		{
			if (hostileComparer == null)
			{
				hostileComparer = CompareHostility;
			}
			list.Sort(hostileComparer);
		}
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		foreach (HeldEntity item2 in list)
		{
			if (!((Object)(object)item2 == (Object)null) && item2.holsterInfo.displayWhenHolstered)
			{
				if (flag4 && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.BACK_SHIELD)
				{
					item2.SetVisibleWhileHolstered(visible: true);
					flag4 = false;
				}
				else if (flag3 && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.BACK)
				{
					item2.SetVisibleWhileHolstered(visible: true);
					flag3 = false;
				}
				else if (flag2 && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.RIGHT_THIGH)
				{
					item2.SetVisibleWhileHolstered(visible: true);
					flag2 = false;
				}
				else if (flag && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.LEFT_THIGH)
				{
					item2.SetVisibleWhileHolstered(visible: true);
					flag = false;
				}
				else
				{
					item2.SetVisibleWhileHolstered(visible: false);
				}
			}
		}
		Pool.FreeUnmanaged<HeldEntity>(ref list);
	}

	public void AddBackpackContentsToList(List<Item> items)
	{
		Item backpackWithInventory = GetBackpackWithInventory();
		if (backpackWithInventory != null && backpackWithInventory.contents != null)
		{
			items.AddRange(backpackWithInventory.contents.itemList);
		}
	}

	public void OnContentsDirty()
	{
		if ((Object)(object)base.baseEntity != (Object)null)
		{
			base.baseEntity.InvalidateNetworkCache();
		}
	}

	public CanMoveFromResponse CanMoveItemsFrom(BaseEntity entity, Item item)
	{
		if (entity is ICanMoveFrom canMoveFrom)
		{
			CanMoveFromResponse result = canMoveFrom.CanMoveFrom(base.baseEntity, item);
			if (!result.allowed)
			{
				return result;
			}
		}
		if (Object.op_Implicit((Object)(object)BaseGameMode.GetActiveGameMode(serverside: true)))
		{
			return BaseGameMode.GetActiveGameMode(serverside: true).CanMoveItemsFrom(this, entity, item);
		}
		return CanMoveFromResponse.Success();
	}

	[BaseEntity.RPC_Server.FromOwner]
	[BaseEntity.RPC_Server]
	private void ItemCmd(BaseEntity.RPCMessage msg)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)(object)msg.player != (Object)null && msg.player.IsWounded()) || base.baseEntity.IsTransferring())
		{
			return;
		}
		ItemId id = msg.read.ItemID();
		string text = msg.read.String();
		Item item = FindItemByUID(id);
		if (item == null || Interface.CallHook("OnItemAction", item, text, msg.player) != null)
		{
			return;
		}
		BaseEntity entityOwner = item.GetEntityOwner();
		if (((Object)(object)entityOwner != (Object)null && (Object)(object)entityOwner == (Object)(object)msg.player && msg.player.IsRestrainedOrSurrendering) || item.IsLocked() || (item.parent != null && item.parent.IsLocked()) || !CanMoveItemsFrom(item.GetEntityOwner(), item).allowed)
		{
			return;
		}
		if (text == "drop")
		{
			int num = item.amount;
			if (msg.read.Unread >= 4)
			{
				num = msg.read.Int32();
			}
			if (!msg.player.isMounted && !msg.player.HasParent() && !GamePhysics.LineOfSight(((Component)msg.player).transform.position, msg.player.eyes.position, 1218519041))
			{
				return;
			}
			base.baseEntity.stats.Add("item_drop", 1, (Stats)5);
			if (num < item.amount)
			{
				Item item2 = item.SplitItem(num);
				ItemContainer parent = item.parent;
				if (item2 != null)
				{
					Vector3 dropVelocity = GetDropVelocity(msg);
					DroppedItem droppedItem = item2.Drop(base.baseEntity.GetDropPosition(), dropVelocity) as DroppedItem;
					if ((Object)(object)droppedItem != (Object)null)
					{
						droppedItem.DropReason = DroppedItem.DropReasonEnum.Player;
						droppedItem.DroppedBy = base.baseEntity.userID;
						droppedItem.DroppedTime = DateTime.UtcNow;
						Facepunch.Rust.Analytics.Azure.OnItemDropped(base.baseEntity, droppedItem, DroppedItem.DropReasonEnum.Player);
					}
				}
				parent?.onItemRemovedFromStack?.Invoke(item, num);
			}
			else
			{
				Vector3 dropVelocity2 = GetDropVelocity(msg);
				ItemContainer parent2 = item.parent;
				DroppedItem droppedItem2 = item.Drop(base.baseEntity.GetDropPosition(), dropVelocity2) as DroppedItem;
				if ((Object)(object)droppedItem2 != (Object)null)
				{
					droppedItem2.DropReason = DroppedItem.DropReasonEnum.Player;
					droppedItem2.DroppedBy = base.baseEntity.userID;
					droppedItem2.DroppedTime = DateTime.UtcNow;
					Facepunch.Rust.Analytics.Azure.OnItemDropped(base.baseEntity, droppedItem2, DroppedItem.DropReasonEnum.Player);
				}
				parent2?.onItemAddedRemoved?.Invoke(item, arg2: false);
			}
			base.baseEntity.SignalBroadcast(BaseEntity.Signal.Gesture, "drop_item");
		}
		else
		{
			item.ServerCommand(text, base.baseEntity);
			ItemManager.DoRemoves();
			ServerUpdate(0f);
		}
	}

	[BaseEntity.RPC_Server.FromOwner]
	[BaseEntity.RPC_Server]
	[BaseEntity.RPC_Server.CallsPerSecond(2uL)]
	private void UpdateAccessoryOnItem(BaseEntity.RPCMessage msg)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)(object)msg.player != (Object)null && msg.player.IsWounded()) || base.baseEntity.IsTransferring())
		{
			return;
		}
		ItemId id = msg.read.ItemID();
		int num = msg.read.Int32();
		if ((num != 0 && !base.baseEntity.blueprints.CheckSkinOwnership(num, base.baseEntity)) || (num != 0 && !(ItemSkinDirectory.FindByInventoryDefinitionId(num).invItem is AccessoryItem)))
		{
			return;
		}
		Item item = FindItemByUID(id);
		if (item != null && item.info.supportsAccessories)
		{
			item.attachment = (ulong)num;
			item.MarkDirty();
			BaseEntity heldEntity = item.GetHeldEntity();
			if ((Object)(object)heldEntity != (Object)null)
			{
				heldEntity.attachmentID = item.attachment;
				heldEntity.SendNetworkUpdate();
			}
		}
	}

	private Vector3 GetDropVelocity(BaseEntity.RPCMessage msg)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (msg.read.Unread >= 4)
		{
			num = msg.read.Float();
		}
		if (!directionalDropEnabled)
		{
			num = 0f;
		}
		Vector3 inheritedDropVelocity = base.baseEntity.GetInheritedDropVelocity();
		Vector3 val = base.baseEntity.eyes.BodyForward();
		Vector3 val2 = Quaternion.AngleAxis(num, Vector3.up) * new Vector3(val.x, 0f, val.z);
		val2.y = val.y;
		return inheritedDropVelocity + val2 * 4f + Vector3Ex.Range(-0.5f, 0.5f);
	}

	[BaseEntity.RPC_Server]
	[BaseEntity.RPC_Server.FromOwner]
	public unsafe void MoveItem(BaseEntity.RPCMessage msg)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Invalid comparison between Unknown and I4
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		if (base.baseEntity.IsTransferring())
		{
			return;
		}
		ItemId id = msg.read.ItemID();
		ItemContainerId val = msg.read.ItemContainerID();
		int num = msg.read.Int8();
		int num2 = (int)msg.read.UInt32();
		ItemMoveModifier val2 = (ItemMoveModifier)msg.read.Int32();
		Item item = FindItemByUID(id);
		if (item == null)
		{
			msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.InvalidItem, false);
			ConstructionErrors.Log(msg.player, ((object)(*(ItemId*)(&id))/*cast due to constrained. prefix*/).ToString());
		}
		else
		{
			if (Interface.CallHook("CanMoveItem", item, this, val, num, num2, val2) != null)
			{
				return;
			}
			BaseEntity entityOwner = item.GetEntityOwner();
			if ((Object)(object)entityOwner != (Object)null && (Object)(object)entityOwner == (Object)(object)msg.player && msg.player.IsRestrainedOrSurrendering)
			{
				return;
			}
			CanMoveFromResponse canMoveFromResponse = CanMoveItemsFrom(entityOwner, item);
			if (!canMoveFromResponse.allowed)
			{
				msg.player.ShowToast(GameTip.Styles.Error, canMoveFromResponse.reasonForFailure ?? PlayerInventoryErrors.CannotMoveItem, canMoveFromResponse.reasonForFailure != null);
				return;
			}
			if (num2 <= 0)
			{
				num2 = item.amount;
			}
			num2 = Mathf.Clamp(num2, 1, item.MaxStackable());
			if (msg.player.GetActiveItem() == item)
			{
				msg.player.UpdateActiveItem(default(ItemId));
			}
			if (!((ItemContainerId)(ref val)).IsValid)
			{
				BaseEntity baseEntity = entityOwner;
				if (loot.containers.Count > 0)
				{
					if ((Object)(object)entityOwner == (Object)(object)base.baseEntity)
					{
						if ((val2 & 2) != 2)
						{
							baseEntity = loot.entitySource;
						}
					}
					else
					{
						baseEntity = base.baseEntity;
					}
				}
				if (baseEntity is IIdealSlotEntity idealSlotEntity)
				{
					val = idealSlotEntity.GetIdealContainer(base.baseEntity, item, val2);
					if (val == ItemContainerId.Invalid)
					{
						return;
					}
				}
				ItemContainer parent = item.parent;
				if (parent != null && parent.IsLocked())
				{
					msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
					return;
				}
				if (!((ItemContainerId)(ref val)).IsValid)
				{
					if ((Object)(object)baseEntity == (Object)(object)loot.entitySource)
					{
						foreach (ItemContainer container in loot.containers)
						{
							if (!container.PlayerItemInputBlocked() && !container.IsLocked() && item.MoveToContainer(container, -1, allowStack: true, ignoreStackLimit: false, base.baseEntity))
							{
								break;
							}
						}
						return;
					}
					if (!GiveItem(item, val2))
					{
						msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.GiveItemFailedError, false);
					}
					return;
				}
			}
			ItemContainer itemContainer = FindContainer(val);
			if (itemContainer == null)
			{
				msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.InvalidContainer, false);
				ConstructionErrors.Log(msg.player, ((object)(*(ItemContainerId*)(&val))/*cast due to constrained. prefix*/).ToString());
				return;
			}
			if (itemContainer.IsLocked())
			{
				msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
				return;
			}
			if (itemContainer.PlayerItemInputBlocked())
			{
				msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.DoesntAcceptPlayerItems, false);
				return;
			}
			if (itemContainer.maxStackSize > 0)
			{
				num2 = Mathf.Clamp(num2, 1, itemContainer.maxStackSize);
			}
			bool allowSwap = !itemContainer.PlayerItemInputBlocked() && item.parent != null && !item.parent.PlayerItemInputBlocked();
			using (TimeWarning.New("Split"))
			{
				if (item.amount > num2)
				{
					int split_Amount = num2;
					Item item2 = item.SplitItem(split_Amount);
					Item slot = itemContainer.GetSlot(num);
					if (slot != null && !item.CanStack(slot) && item.parent != null && !item2.MoveToContainer(item.parent, -1, allowStack: false, ignoreStackLimit: false, base.baseEntity, allowSwap: false))
					{
						item.amount += item2.amount;
						item2.Remove();
						ItemManager.DoRemoves();
						ServerUpdate(0f);
						return;
					}
					if (!item2.MoveToContainer(itemContainer, num, allowStack: true, ignoreStackLimit: false, base.baseEntity, allowSwap))
					{
						item.amount += item2.amount;
						item2.Remove();
					}
					else
					{
						item.parent.onItemRemovedFromStack?.Invoke(item, num2);
					}
					ItemManager.DoRemoves();
					ServerUpdate(0f);
					return;
				}
			}
			if (item.MoveToContainer(itemContainer, num, allowStack: true, ignoreStackLimit: false, base.baseEntity, allowSwap))
			{
				ItemManager.DoRemoves();
				ServerUpdate(0f);
			}
		}
	}

	private void OnClothingItemContentsChanged(Item item, bool bAdded)
	{
		OnClothingChanged(item, bAdded);
	}

	public void OnClothingChanged(Item item, bool bAdded)
	{
		base.baseEntity.SV_ClothingChanged();
		if (ItemManager.EnablePooling)
		{
			if (!IsInvoking(DeferredServerUpdateAction))
			{
				Invoke(DeferredServerUpdateAction, 0f);
			}
		}
		else
		{
			ItemManager.DoRemoves();
			ServerUpdate(0f);
		}
		if (item.position == 7)
		{
			item.RecalulateParentEntity(children: true);
			if (_updatedVisibleHolsteredItemsCallback == null)
			{
				_updatedVisibleHolsteredItemsCallback = UpdatedVisibleHolsteredItems;
			}
			Invoke(_updatedVisibleHolsteredItemsCallback, 0.1f);
			if ((Object)(object)base.baseEntity.GetHeldEntity() != (Object)null)
			{
				base.baseEntity.GetHeldEntity().UpdateShieldState(bHeld: true);
			}
			item?.contents?.onItemAddedRemoved?.Invoke(item, bAdded);
		}
		base.baseEntity.ProcessMissionEvent(BaseMission.MissionEventType.CLOTHINGCHANGED, 0, 0f);
		Interface.CallHook("OnClothingItemChanged", this, item, bAdded);
	}

	private void DeferredServerUpdate()
	{
		ServerUpdate(0f);
	}

	public void OnItemRemoved(Item item)
	{
		base.baseEntity.InvalidateNetworkCache();
	}

	public bool CanStoreInInventory(Item item, int targetSlot)
	{
		return true;
	}

	public bool CanEquipItem(Item item, int targetSlot)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("CanEquipItem", this, item, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((item.info.flags & ItemDefinition.Flag.NotAllowedInBelt) != 0)
		{
			return false;
		}
		if ((Object)(object)base.baseEntity != (Object)null && base.baseEntity.IsRestrained)
		{
			Handcuffs restraintItem = base.baseEntity.Belt.GetRestraintItem();
			if ((Object)(object)restraintItem != (Object)null && restraintItem.GetItem().position == targetSlot)
			{
				return false;
			}
		}
		ItemModContainerRestriction component = ((Component)item.info).GetComponent<ItemModContainerRestriction>();
		if ((Object)(object)component == (Object)null)
		{
			return true;
		}
		BufferList<Item> val = Pool.Get<BufferList<Item>>();
		val.CopyFrom(containerBelt.itemList);
		Enumerator<Item> enumerator = val.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Item current = enumerator.Current;
				if (current != item)
				{
					ItemModContainerRestriction component2 = ((Component)current.info).GetComponent<ItemModContainerRestriction>();
					if (!((Object)(object)component2 == (Object)null) && !component.CanExistWith(component2) && !current.MoveToContainer(containerMain))
					{
						current.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.Free<Item>(ref val, false);
		return true;
	}

	public bool CanWearItem(Item item, int targetSlot)
	{
		object obj = Interface.CallHook("CanWearItem", this, item, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return CanWearItem(item, canAdjustClothing: true, targetSlot);
	}

	public bool CanWearItem(Item item, bool canAdjustClothing, int targetSlot)
	{
		return WearItemCheck(item, canAdjustClothing, targetSlot).Result;
	}

	public bool CanReplaceBackpack(Item itemToWear)
	{
		Item slot = containerWear.GetSlot(7);
		if (slot == null)
		{
			return true;
		}
		ItemContainer contents = slot.contents;
		if (contents != null && contents.itemList?.Count > 0)
		{
			if (base.baseEntity.InSafeZone())
			{
				return false;
			}
			if (!itemToWear.IsDroppedInWorld(serverside: true))
			{
				return false;
			}
		}
		return true;
	}

	public void ServerUpdate(float delta)
	{
		loot.Check();
		if (delta > 0f && !base.baseEntity.IsSleeping() && !base.baseEntity.IsTransferring())
		{
			crafting.ServerUpdate(delta);
		}
		float currentTemperature = base.baseEntity.currentTemperature;
		UpdateContainer(delta, Type.Main, containerMain, bSendInventoryToEveryone: false, currentTemperature);
		UpdateContainer(delta, Type.Belt, containerBelt, bSendInventoryToEveryone: true, currentTemperature);
		UpdateContainer(delta, Type.Wear, containerWear, bSendInventoryToEveryone: true, currentTemperature);
	}

	public void UpdateContainer(float delta, Type type, ItemContainer container, bool bSendInventoryToEveryone, float temperature)
	{
		if (container != null)
		{
			container.temperature = temperature;
			if (delta > 0f)
			{
				container.OnCycle(delta);
			}
			if (container.dirty)
			{
				SendUpdatedInventory(type, container, bSendInventoryToEveryone);
				base.baseEntity.InvalidateNetworkCache();
			}
		}
	}

	public void SendSnapshot()
	{
		using (TimeWarning.New("PlayerInventory.SendSnapshot"))
		{
			SendUpdatedInventory(Type.Main, containerMain);
			SendUpdatedInventory(Type.Belt, containerBelt, bSendInventoryToEveryone: true);
			SendUpdatedInventory(Type.Wear, containerWear, bSendInventoryToEveryone: true);
		}
	}

	public void SendUpdatedInventory(Type type, ItemContainer container, bool bSendInventoryToEveryone = false)
	{
		if (type == Type.Wear)
		{
			if (bSendInventoryToEveryone)
			{
				SendUpdatedInventoryInternal(type, container, NetworkInventoryMode.LocalPlayer);
				SendUpdatedInventoryInternal(type, container, NetworkInventoryMode.EveryoneButLocal);
			}
			else
			{
				SendUpdatedInventoryInternal(type, container, NetworkInventoryMode.LocalPlayer);
			}
		}
		else
		{
			SendUpdatedInventoryInternal(type, container, bSendInventoryToEveryone ? NetworkInventoryMode.Everyone : NetworkInventoryMode.LocalPlayer);
		}
	}

	public void SendUpdatedInventoryInternal(Type type, ItemContainer container, NetworkInventoryMode mode)
	{
		UpdateItemContainer val = Pool.Get<UpdateItemContainer>();
		try
		{
			val.type = (int)type;
			if (base.baseEntity.IsSpectating())
			{
				mode = NetworkInventoryMode.LocalPlayer;
			}
			if (container != null)
			{
				container.dirty = false;
				val.container = Pool.Get<List<ItemContainer>>();
				bool bIncludeContainer = type != Type.Wear || mode == NetworkInventoryMode.LocalPlayer;
				val.container.Add(container.Save(bIncludeContainer));
			}
			if (Interface.CallHook("OnInventoryNetworkUpdate", this, container, val, type, mode) != null)
			{
				return;
			}
			switch (mode)
			{
			case NetworkInventoryMode.Everyone:
				base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("UpdatedItemContainer"), val);
				break;
			case NetworkInventoryMode.LocalPlayer:
				base.baseEntity.ClientRPC(RpcTarget.Player("UpdatedItemContainer", base.baseEntity), val);
				break;
			case NetworkInventoryMode.EveryoneButLocal:
				if (base.baseEntity.net?.group?.subscribers == null)
				{
					break;
				}
				{
					foreach (Connection subscriber in base.baseEntity.net.group.subscribers)
					{
						if (subscriber.player is BasePlayer basePlayer && (Object)(object)basePlayer != (Object)(object)base.baseEntity)
						{
							base.baseEntity.ClientRPC(RpcTarget.Player("UpdatedItemContainer", basePlayer), val);
						}
					}
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void MoveItemsIntoContainer(ItemDefinition itemToMove, int amount, ItemContainer targetContainer)
	{
		MoveFromContainer(containerMain);
		MoveFromContainer(containerBelt);
		MoveFromContainer(containerWear);
		void MoveFromContainer(ItemContainer container)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (amount <= 0)
			{
				return;
			}
			multiContainerBuffer.Clear();
			container.FindItemsByItemID(itemToMove.itemid, multiContainerBuffer);
			Enumerator<Item> enumerator = multiContainerBuffer.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Item current = enumerator.Current;
					if (amount <= 0)
					{
						break;
					}
					int num = Mathf.Min(current.amount, amount);
					if (num < current.amount)
					{
						Item item = current.SplitItem(amount);
						if (item.MoveToContainer(targetContainer))
						{
							amount -= num;
						}
						else
						{
							current.amount += item.amount;
							item.Remove();
						}
					}
					else if (current.MoveToContainer(targetContainer))
					{
						amount -= num;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private WearCheckResult WearItemCheck(Item item, bool canAdjustClothing, int targetSlot, bool dontMove = false)
	{
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		ItemModWearable component = ((Component)item.info).GetComponent<ItemModWearable>();
		if ((Object)(object)component == (Object)null)
		{
			return new WearCheckResult
			{
				Result = false,
				ChangedItem = null
			};
		}
		if (component.npcOnly && !Inventory.disableAttireLimitations)
		{
			BasePlayer basePlayer = base.baseEntity;
			if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsNpc)
			{
				return new WearCheckResult
				{
					Result = false,
					ChangedItem = null
				};
			}
		}
		bool flag = item.IsBackpack();
		if (flag)
		{
			if (targetSlot != 7)
			{
				return new WearCheckResult
				{
					Result = false,
					ChangedItem = null
				};
			}
			if (!CanReplaceBackpack(item))
			{
				return new WearCheckResult
				{
					Result = false,
					ChangedItem = null
				};
			}
		}
		else if (!flag && targetSlot == 7)
		{
			return new WearCheckResult
			{
				Result = false,
				ChangedItem = null
			};
		}
		if ((Object)(object)((Component)item.info).GetComponent<ItemModParachute>() != (Object)null && !CanEquipParachute())
		{
			base.baseEntity.ShowToast(GameTip.Styles.Red_Normal, BackpackGroundedError, false);
			return new WearCheckResult
			{
				Result = false,
				ChangedItem = null
			};
		}
		if (component.preventsMounting && base.baseEntity.isMounted)
		{
			return new WearCheckResult
			{
				Result = false,
				ChangedItem = null
			};
		}
		BufferList<Item> val = Pool.Get<BufferList<Item>>();
		val.CopyFrom(containerWear.itemList);
		Enumerator<Item> enumerator = val.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Item clothingItem = enumerator.Current;
				if (clothingItem == item)
				{
					continue;
				}
				ItemModWearable component2 = ((Component)clothingItem.info).GetComponent<ItemModWearable>();
				if (!((Object)(object)component2 == (Object)null) && !Inventory.disableAttireLimitations && !component.CanExistWith(component2))
				{
					if (!canAdjustClothing)
					{
						Pool.Free<Item>(ref val, false);
						return new WearCheckResult
						{
							Result = false,
							ChangedItem = null
						};
					}
					if (!dontMove && (targetSlot != clothingItem.position || targetSlot == 7) && !DirectSwap(containerMain) && !DirectSwap(containerBelt) && !clothingItem.MoveToContainer(containerMain) && !clothingItem.MoveToContainer(containerBelt))
					{
						clothingItem.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
					}
				}
				bool DirectSwap(ItemContainer container)
				{
					if (container.itemList.Count == container.capacity && container.itemList.Contains(item))
					{
						if (!clothingItem.MoveToContainer(container))
						{
							return false;
						}
						item.RemoveFromContainer();
						return true;
					}
					return false;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.Free<Item>(ref val, false);
		return new WearCheckResult
		{
			Result = true,
			ChangedItem = returnItems
		};
	}

	public Item FindItemByUID(ItemId id)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!((ItemId)(ref id)).IsValid)
		{
			return null;
		}
		if (containerMain != null)
		{
			Item item = containerMain.FindItemByUID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindItemByUID(id);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindItemByUID(id);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return loot.FindItem(id);
	}

	public Item FindItemByItemID(string itemName)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemName);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return null;
		}
		return FindItemByItemID(itemDefinition.itemid);
	}

	public Item FindItemByItemID(int id)
	{
		object obj = Interface.CallHook("OnInventoryItemFind", this, id);
		if (obj is Item)
		{
			return (Item)obj;
		}
		if (containerMain != null)
		{
			Item item = containerMain.FindItemByItemID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindItemByItemID(id);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindItemByItemID(id);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return null;
	}

	public Item FindItemByItemName(string name)
	{
		if (containerMain != null)
		{
			Item item = containerMain.FindItemByItemName(name);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindItemByItemName(name);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindItemByItemName(name);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return null;
	}

	public Item FindBySubEntityID(NetworkableId subEntityID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (containerMain != null)
		{
			Item item = containerMain.FindBySubEntityID(subEntityID);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindBySubEntityID(subEntityID);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindBySubEntityID(subEntityID);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return null;
	}

	[PoolAnalyzerNonCaching]
	public void FindItemsByItemID(List<Item> list, int id)
	{
		if (Interface.CallHook("OnInventoryItemsFind", this, id, list) == null)
		{
			if (containerMain != null)
			{
				containerMain.FindItemsByItemID(list, id);
			}
			if (containerBelt != null)
			{
				containerBelt.FindItemsByItemID(list, id);
			}
			if (containerWear != null)
			{
				containerWear.FindItemsByItemID(list, id);
			}
		}
	}

	public ItemContainer FindContainer(ItemContainerId id)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FindContainer"))
		{
			ItemContainer itemContainer = containerMain.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			itemContainer = containerBelt.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			itemContainer = containerWear.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			return loot.FindContainer(id);
		}
	}

	public ItemContainer GetContainer(Type id)
	{
		if (id == Type.Main)
		{
			return containerMain;
		}
		if (Type.Belt == id)
		{
			return containerBelt;
		}
		if (Type.Wear == id)
		{
			return containerWear;
		}
		if (Type.BackpackContents == id)
		{
			return GetBackpackWithInventory()?.contents;
		}
		return null;
	}

	public Item GetAnyBackpack()
	{
		return containerWear?.GetSlot(7);
	}

	public bool HasBackpackItem()
	{
		return GetAnyBackpack() != null;
	}

	public Item GetBackpackWithInventory()
	{
		Item anyBackpack = GetAnyBackpack();
		if (anyBackpack == null || anyBackpack.contents == null)
		{
			return null;
		}
		return anyBackpack;
	}

	public void DropBackpackOnDeath(bool wounded)
	{
		if (base.baseEntity.InSafeZone())
		{
			return;
		}
		if (wounded)
		{
			if (!Player.dropbackpackondowned)
			{
				return;
			}
		}
		else if (!Player.dropbackpackondeath)
		{
			return;
		}
		Item anyBackpack = GetAnyBackpack();
		if (anyBackpack != null)
		{
			ItemModBackpack component = ((Component)anyBackpack.info).GetComponent<ItemModBackpack>();
			if (!((Object)(object)component == (Object)null) && component.DropWhenDowned)
			{
				TryDropBackpack();
			}
		}
	}

	public Item GetEquippedPrisonerHoodItem()
	{
		return containerWear.FindItemByItemID(Handcuffs.PrisonerHoodItemID);
	}

	public Item GetUsableHoodItem()
	{
		return FindItemByItemID(Handcuffs.PrisonerHoodItemID);
	}

	public bool GiveItem(Item item, ItemContainer container = null, GiveItemOptions options = GiveItemOptions.None)
	{
		return GiveItem(item, (ItemMoveModifier)0, container, options);
	}

	public bool GiveItem(Item item, ItemMoveModifier modifiers, ItemContainer container = null, GiveItemOptions options = GiveItemOptions.None)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		bool tryWearClothing = (modifiers & 2) == 2;
		bool flag = (modifiers & 0x10) == 16;
		if (item == null)
		{
			return false;
		}
		if (container == null)
		{
			container = GetIdealPickupContainer(item, tryWearClothing);
		}
		if (container != null && item.MoveToContainer(container))
		{
			return true;
		}
		if (item.MoveToContainer(containerMain))
		{
			return true;
		}
		if (flag || (options & GiveItemOptions.BackpackOverflow) == GiveItemOptions.BackpackOverflow)
		{
			Item backpackWithInventory = GetBackpackWithInventory();
			if (backpackWithInventory != null && item.MoveToContainer(backpackWithInventory.contents))
			{
				return true;
			}
		}
		if (item.MoveToContainer(containerBelt))
		{
			return true;
		}
		return false;
	}

	public ItemContainer GetIdealPickupContainer(Item item, bool tryWearClothing)
	{
		if (item.MaxStackable() > 1)
		{
			if (containerBelt != null && containerBelt.FindItemByItemID(item.info.itemid) != null)
			{
				return containerBelt;
			}
			if (containerMain != null && containerMain.FindItemByItemID(item.info.itemid) != null)
			{
				return containerMain;
			}
		}
		if (item.info.isWearable && item.info.ItemModWearable.equipOnPickup && item.IsDroppedInWorld(serverside: true))
		{
			Item anyBackpack = GetAnyBackpack();
			if ((Object)(object)((Component)item.info).GetComponent<ItemModShield>() != (Object)null && anyBackpack != null && (Object)(object)((Component)anyBackpack.info).GetComponent<ItemModShield>() != (Object)null)
			{
				if (!containerMain.IsFull())
				{
					return containerMain;
				}
				return containerBelt;
			}
			if (anyBackpack != null && anyBackpack.GetItemVolume() > containerMain.containerVolume && item.GetItemVolume() <= containerMain.containerVolume)
			{
				if (!containerMain.IsFull())
				{
					return containerMain;
				}
				return containerBelt;
			}
			return containerWear;
		}
		if (tryWearClothing && item.info.isWearable && CanWearItem(item, canAdjustClothing: false, item.IsBackpack() ? 7 : (-1)))
		{
			return containerWear;
		}
		if ((item.info.isUsable || item.info.HasFlag(ItemDefinition.Flag.PrioritizeBelt)) && !item.info.HasFlag(ItemDefinition.Flag.NotStraightToBelt))
		{
			return containerBelt;
		}
		return null;
	}

	public void Strip()
	{
		containerMain.Clear();
		containerBelt.Clear();
		containerWear.Clear();
		ItemManager.DoRemoves();
	}

	public static bool IsBirthday()
	{
		if (forceBirthday)
		{
			return true;
		}
		if (Time.time < nextCheckTime)
		{
			return wasBirthday;
		}
		nextCheckTime = Time.time + 60f;
		DateTime now = DateTime.Now;
		wasBirthday = now.Month == 12 && now.Day >= 7 && now.Day <= 16;
		return wasBirthday;
	}

	public static bool IsChristmas()
	{
		return XMas.enabled;
	}

	public void GiveDefaultItems()
	{
		if (Interface.CallHook("OnDefaultItemsReceive", this) != null)
		{
			return;
		}
		Strip();
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && activeGameMode.HasLoadouts())
		{
			BaseGameMode.GetActiveGameMode(serverside: true).LoadoutPlayer(base.baseEntity);
			return;
		}
		GiveDefaultItemWithSkin("client.rockskin", "rock");
		GiveDefaultItemWithSkin("client.torchskin", "torch");
		if (IsBirthday() && !base.baseEntity.IsInTutorial)
		{
			TryGiveItem("cakefiveyear", 0uL, containerBelt);
			TryGiveItem("partyhat", 0uL, containerWear);
		}
		if (IsChristmas() && !base.baseEntity.IsInTutorial)
		{
			TryGiveItem("snowball", 0uL, containerBelt);
			TryGiveItem("snowball", 0uL, containerBelt);
			TryGiveItem("snowball", 0uL, containerBelt);
		}
		Interface.CallHook("OnDefaultItemsReceived", this);
		void GiveDefaultItemWithSkin(string convarSkinName, string itemShortName)
		{
			ulong num = 0uL;
			int infoInt = base.baseEntity.GetInfoInt(convarSkinName, 0);
			bool flag = false;
			if (infoInt > 0 && base.baseEntity.blueprints.CheckSkinOwnership(infoInt, base.baseEntity))
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemShortName);
				if ((Object)(object)itemDefinition != (Object)null && ItemDefinition.FindSkin(itemDefinition.itemid, infoInt) != 0L)
				{
					IPlayerItemDefinition itemDefinition2 = PlatformService.Instance.GetItemDefinition(infoInt);
					if (itemDefinition2 != null)
					{
						num = itemDefinition2.WorkshopDownload;
					}
					if (num == 0L && itemDefinition.skins != null)
					{
						ItemSkinDirectory.Skin[] skins = itemDefinition.skins;
						for (int i = 0; i < skins.Length; i++)
						{
							ItemSkinDirectory.Skin skin = skins[i];
							if (skin.id == infoInt && (Object)(object)skin.invItem != (Object)null && skin.invItem is ItemSkin itemSkin && (Object)(object)itemSkin.Redirect != (Object)null)
							{
								TryGiveItem(itemSkin.Redirect.shortname, 0uL, containerBelt);
								flag = true;
								break;
							}
						}
					}
				}
			}
			if (!flag)
			{
				TryGiveItem(itemShortName, num, containerBelt);
			}
		}
		bool TryGiveItem(string itemShortName, ulong skin, ItemContainer container)
		{
			Item item = ItemManager.CreateByName(itemShortName, 1, skin);
			if (item != null)
			{
				item.SetItemOwnership(base.baseEntity, ItemOwnershipPhrases.BornPhrase);
				GiveItem(item, container);
				return true;
			}
			Debug.LogError((object)$"Failed to spawn {itemShortName} with {skin}!");
			return false;
		}
	}

	public bool CanEquipParachute()
	{
		if (ConVar.Server.canEquipBackpacksInAir || Parachute.BypassRepack)
		{
			return true;
		}
		if (base.baseEntity.WaterFactor() > 0.5f)
		{
			return true;
		}
		if (!base.baseEntity.IsOnGround())
		{
			return false;
		}
		if (base.baseEntity.isMounted && Object.op_Implicit((Object)(object)base.baseEntity.GetMounted()) && base.baseEntity.GetMounted().VehicleParent() is Parachute)
		{
			return false;
		}
		return true;
	}

	public PlayerInventory Save(bool bForDisk)
	{
		PlayerInventory val = Pool.Get<PlayerInventory>();
		if (bForDisk)
		{
			val.invMain = containerMain.Save();
		}
		val.invBelt = containerBelt.Save();
		val.invWear = containerWear.Save();
		return val;
	}

	public void Load(PlayerInventory msg)
	{
		if (msg.invMain != null)
		{
			containerMain.Load(msg.invMain);
		}
		if (msg.invBelt != null)
		{
			containerBelt.Load(msg.invBelt);
		}
		if (msg.invWear != null)
		{
			containerWear.Load(msg.invWear);
		}
		if (Object.op_Implicit((Object)(object)base.baseEntity) && base.baseEntity.isServer && containerWear.capacity == 7)
		{
			containerWear.capacity = 8;
		}
	}

	public void TryDropBackpack()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Item anyBackpack = GetAnyBackpack();
		if (anyBackpack != null && base.baseEntity.isServer && Interface.CallHook("OnBackpackDrop", anyBackpack, this) == null)
		{
			anyBackpack.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
		}
	}

	[PoolAnalyzerNonCaching]
	public int Take(List<Item> collect, int itemid, int amount)
	{
		object obj = Interface.CallHook("OnInventoryItemsTake", this, collect, itemid, amount);
		if (obj is int)
		{
			return (int)obj;
		}
		int num = 0;
		if (containerMain != null)
		{
			int num2 = containerMain.Take(collect, itemid, amount);
			num += num2;
			amount -= num2;
		}
		if (amount <= 0)
		{
			return num;
		}
		if (containerBelt != null)
		{
			int num3 = containerBelt.Take(collect, itemid, amount);
			num += num3;
			amount -= num3;
		}
		if (amount <= 0)
		{
			return num;
		}
		if (containerWear != null)
		{
			int num4 = containerWear.Take(collect, itemid, amount);
			num += num4;
			amount -= num4;
		}
		return num;
	}

	public bool HasEmptySlotInBeltOrMain()
	{
		if (containerMain != null && containerMain.capacity > containerMain.itemList.Count)
		{
			return true;
		}
		if (containerBelt != null && containerBelt.capacity > containerBelt.itemList.Count)
		{
			return true;
		}
		return false;
	}

	public bool HasEmptySlots(int requiredSlots)
	{
		int num = 0;
		if (containerMain != null)
		{
			num += containerMain.capacity - containerMain.itemList.Count;
		}
		if (containerBelt != null)
		{
			num += containerBelt.capacity - containerBelt.itemList.Count;
		}
		return num >= requiredSlots;
	}

	public int GetAmount(ItemDefinition definition)
	{
		if (!((Object)(object)definition != (Object)null))
		{
			return 0;
		}
		return GetAmount(definition.itemid);
	}

	public int GetAmount(int itemid, bool includeBackpack = false)
	{
		if (itemid == 0)
		{
			return 0;
		}
		object obj = Interface.CallHook("OnInventoryItemsCount", this, itemid, includeBackpack);
		if (obj is int)
		{
			return (int)obj;
		}
		int num = 0;
		if (containerMain != null)
		{
			num += containerMain.GetAmount(itemid, onlyUsableAmounts: true);
		}
		if (containerBelt != null)
		{
			num += containerBelt.GetAmount(itemid, onlyUsableAmounts: true);
		}
		if (containerWear != null)
		{
			num += containerWear.GetAmount(itemid, onlyUsableAmounts: true);
		}
		if (includeBackpack)
		{
			Item backpackWithInventory = GetBackpackWithInventory();
			if (backpackWithInventory != null && backpackWithInventory.contents != null)
			{
				num += backpackWithInventory.contents.GetAmount(itemid, onlyUsableAmounts: true);
			}
		}
		return num;
	}

	public int GetOkConditionAmount(int itemid)
	{
		if (itemid == 0)
		{
			return 0;
		}
		int num = 0;
		if (containerMain != null)
		{
			num += containerMain.GetOkConditionAmount(itemid, onlyUsableAmounts: true);
		}
		if (containerBelt != null)
		{
			num += containerBelt.GetOkConditionAmount(itemid, onlyUsableAmounts: true);
		}
		if (containerWear != null)
		{
			num += containerWear.GetOkConditionAmount(itemid, onlyUsableAmounts: true);
		}
		return num;
	}

	public bool Contains(Item item)
	{
		bool flag = containerMain?.itemList.Contains(item) ?? false;
		if (!flag)
		{
			flag = containerBelt?.itemList.Contains(item) ?? false;
		}
		if (!flag)
		{
			flag = containerWear?.itemList.Contains(item) ?? false;
		}
		return flag;
	}

	[PoolAnalyzerNonCaching]
	public int GetAllItems(List<Item> items)
	{
		items.Clear();
		if (containerMain != null)
		{
			items.AddRange(containerMain.itemList);
		}
		if (containerBelt != null)
		{
			items.AddRange(containerBelt.itemList);
		}
		if (containerWear != null)
		{
			items.AddRange(containerWear.itemList);
		}
		return items.Count;
	}

	public Item FindAmmo(AmmoTypes ammoType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnInventoryAmmoItemFind", this, ammoType);
		if (obj is Item)
		{
			return (Item)obj;
		}
		Item item = containerMain?.FindAmmo(ammoType);
		if (item == null)
		{
			item = containerBelt?.FindAmmo(ammoType);
		}
		return item;
	}

	[PoolAnalyzerNonCaching]
	public void FindAmmo(List<Item> list, AmmoTypes ammoType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnInventoryAmmoFind", this, list, ammoType) == null)
		{
			containerMain?.FindAmmo(list, ammoType);
			containerBelt?.FindAmmo(list, ammoType);
		}
	}

	public bool HasAmmo(AmmoTypes ammoType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!containerMain.HasAmmo(ammoType))
		{
			return containerBelt.HasAmmo(ammoType);
		}
		return true;
	}

	private void OnItemRadiationChanged(Item item, float rads)
	{
		CalculateInventoryRadioactivity();
	}

	private void CalculateInventoryRadioactivity()
	{
		float num = 0f;
		if (containerMain != null)
		{
			num += containerMain.GetRadioactiveMaterialInContainer();
		}
		if (containerBelt != null)
		{
			num += containerBelt.GetRadioactiveMaterialInContainer();
		}
		if (containerWear != null)
		{
			num += containerWear.GetRadioactiveMaterialInContainer();
		}
		inventoryRadioactivity = num;
		bool arg = (containsRadioactiveItems = num > 0f);
		this.onRadioactivityChanged?.Invoke(num, arg);
	}

	private void CalculateInventoryRadioactivityCheckFast()
	{
		if (containsRadioactiveItems)
		{
			CalculateInventoryRadioactivity();
		}
	}
}
