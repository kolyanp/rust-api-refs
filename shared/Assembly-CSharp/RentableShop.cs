using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using GameMenu;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class RentableShop : BaseEntity
{
	public class PlayerStoreContents : IPooled
	{
		public ulong ownerID;

		public ItemContainer items;

		public DateTime contentsTimeStamp;

		private ItemContainer CreateContainer(RentableShop owner)
		{
			ItemContainer itemContainer = Pool.Get<ItemContainer>();
			itemContainer.entityOwner = owner;
			itemContainer.allowedContents = ItemContainer.ContentsType.Generic;
			itemContainer.SetOnlyAllowedItem(null);
			itemContainer.maxStackSize = 0;
			itemContainer.ServerInitialize(null, 64);
			itemContainer.canAcceptItem = null;
			itemContainer.GiveUID();
			return itemContainer;
		}

		public void EnterPool()
		{
			ownerID = 0uL;
			contentsTimeStamp = DateTime.MinValue;
			Pool.Free<ItemContainer>(ref items);
		}

		public void LeavePool()
		{
		}

		public void Setup(RentableShop owner, VendingMachine vm, ulong id)
		{
			ownerID = id;
			items = CreateContainer(owner);
			vm.inventory.MoveAllItems(items);
			contentsTimeStamp = CurrentTime;
		}

		public void Load(ulong forId, ItemContainer itemsToLoad, RentableShop owner, long timeStamp)
		{
			ownerID = forId;
			contentsTimeStamp = DateTime.FromBinary(timeStamp);
			items = CreateContainer(owner);
			items.Load(itemsToLoad);
		}

		public void GiveToPlayer(BasePlayer player)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			items.MoveAllItems(player.inventory.containerMain, player);
			if (items.itemList.Count > 0)
			{
				ItemContainer.Drop("assets/prefabs/misc/item drop/item_drop_backpack.prefab", player.GetDropPosition(), default(Quaternion), items);
			}
			items.Clear();
		}
	}

	[SerializeField]
	private GameObjectRef ShopkeeperPrefab;

	[SerializeField]
	private Transform ShopkeeperSpawnPoint;

	[SerializeField]
	private GameObjectRef VendingMachinePrefab;

	[SerializeField]
	private SoundDefinition OpenShopSound;

	[SerializeField]
	private SoundDefinition CloseShopSound;

	[SerializeField]
	private Signage ShopSignEntity;

	[SerializeField]
	private GameObjectRef OpenStoreDialog;

	[SerializeField]
	private Transform FrontInteractPosition;

	[SerializeField]
	private float FrontInteractPositionRadius;

	[SerializeField]
	private int ShopNumber;

	[SerializeField]
	private TextMeshPro ShopNumberLabel;

	public const float InitialRentHoursRequired = 12f;

	[ReplicatedVar]
	public static int ScrapPerHourRent = 10;

	[ReplicatedVar]
	public static int InitialScrapFee = 100;

	[ReplicatedVar]
	public static float ProtectionFromTakeoverHours = 6f;

	[CompilerGenerated]
	private float _003CCurrentRentMultiplier_003Ek__BackingField;

	public static readonly Phrase Phrase_BreakInAlreadyAuthed = new Phrase("vm_breakin_already_authed", "You already have access to this Store");

	public static readonly Phrase Phrase_BreakInUnoccupied = new Phrase("vm_breakin_unoccupied", "This Store is unoccupied");

	private static Phrase Phrase_ShopClosedNotificationPhrase = new Phrase("rentable_shop_closed", "Your rented shop has been closed. Any remaining belongings can be retrieved from the shop.");

	public static readonly Phrase Phrase_BreakInSuccess = new Phrase("vm_breakin_success", "You broke into the Store and have temporary access");

	private static Phrase Phrase_OnlyOneShopAllowedPhrase = new Phrase("rentable_shop_one_shop_allowed", "You may only open one shop at a time.");

	private static ItemDefinition _scrapDef = null;

	private Signage cachedChildSign;

	private float nextRentDue;

	private TimeSince lastRentCheck;

	private const float RENT_INTERVAL = 3600f;

	private static ListHashSet<RentableShop> AllShops = new ListHashSet<RentableShop>();

	private Dictionary<ulong, TimeSince> breakInStarts;

	private Dictionary<ulong, TimeUntil> intruders;

	[ServerVar(ShowInAdminUI = true, Saved = true, Help = "How long stores should store items (after a shop is closed) for before they are destroyed")]
	public static int MaxStoredItemsDurationMinutes = 1440;

	[ServerVar(Help = "When checking the time to see if items need to be deleted, add this many hours to what it thinks the current time is")]
	public static int AdditionalCheckTimeHoursDebug = 0;

	private Dictionary<ulong, PlayerStoreContents> savedContent;

	private EntityRef<NPCShopKeeper> __sync_SpawnedShopkeeperRef;

	private EntityRef<VendingMachine> __sync_SpawnedVendingMachineRef;

	private ulong __sync_ShopOwnerId;

	private float __sync_CurrentRentMultiplier;

	public int ShopNumberId => ShopNumber;

	[Sync(Autosave = true)]
	private EntityRef<NPCShopKeeper> SpawnedShopkeeperRef
	{
		[CompilerGenerated]
		get
		{
			return __sync_SpawnedShopkeeperRef;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_SpawnedShopkeeperRef, value))
			{
				__sync_SpawnedShopkeeperRef = value;
				byte nameID = __GetWeaverID("SpawnedShopkeeperRef");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private EntityRef<VendingMachine> SpawnedVendingMachineRef
	{
		[CompilerGenerated]
		get
		{
			return __sync_SpawnedVendingMachineRef;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_SpawnedVendingMachineRef, value))
			{
				__sync_SpawnedVendingMachineRef = value;
				byte nameID = __GetWeaverID("SpawnedVendingMachineRef");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public ulong ShopOwnerId
	{
		[CompilerGenerated]
		get
		{
			return __sync_ShopOwnerId;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_ShopOwnerId, value))
			{
				__sync_ShopOwnerId = value;
				byte nameID = __GetWeaverID("ShopOwnerId");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private float CurrentRentMultiplier
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentRentMultiplier;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_CurrentRentMultiplier, value))
			{
				__sync_CurrentRentMultiplier = value;
				byte nameID = __GetWeaverID("CurrentRentMultiplier");
				QueueSyncVar(nameID);
			}
		}
	}

	public static ItemDefinition ScrapDef
	{
		get
		{
			if ((Object)(object)_scrapDef == (Object)null)
			{
				_scrapDef = ItemManager.FindItemDefinition("scrap");
			}
			return _scrapDef;
		}
	}

	public TimeSince TimeSinceShopOpened { get; private set; }

	private Signage ShopSign
	{
		get
		{
			if ((Object)(object)cachedChildSign == (Object)null)
			{
				foreach (BaseEntity child in children)
				{
					if (child is Signage signage)
					{
						cachedChildSign = signage;
					}
				}
			}
			return cachedChildSign;
		}
	}

	public bool IsShopRecentlyOpened
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (IsOn())
			{
				return TimeSince.op_Implicit(TimeSinceShopOpened) < ProtectionFromTakeoverHours * 60f * 60f;
			}
			return false;
		}
	}

	public static ListHashSet<RentableShop> AllServerShops => AllShops;

	private static DateTime CurrentTime => DateTime.UtcNow;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RentableShop.OnRpcMessage"))
		{
			if (rpc == 4285583781u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BreakIn"));
				}
				using (TimeWarning.New("BreakIn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4285583781u, "BreakIn", this, player, 3f))
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
							BreakIn(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BreakIn");
					}
				}
				return true;
			}
			if (rpc == 2169074625u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_CloseStore"));
				}
				using (TimeWarning.New("Server_CloseStore"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2169074625u, "Server_CloseStore", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_CloseStore(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_CloseStore");
					}
				}
				return true;
			}
			if (rpc == 2720206421u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_OpenStore"));
				}
				using (TimeWarning.New("Server_OpenStore"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2720206421u, "Server_OpenStore", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2720206421u, "Server_OpenStore", this, player, 3f))
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
							Server_OpenStore(msg2);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in Server_OpenStore");
					}
				}
				return true;
			}
			if (rpc == 1017307015 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_OpenStoreInventory"));
				}
				using (TimeWarning.New("Server_OpenStoreInventory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1017307015u, "Server_OpenStoreInventory", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1017307015u, "Server_OpenStoreInventory", this, player, 3f))
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
							Server_OpenStoreInventory(msg3);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in Server_OpenStoreInventory");
					}
				}
				return true;
			}
			if (rpc == 430079613 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_OpenVendingAdmin"));
				}
				using (TimeWarning.New("Server_OpenVendingAdmin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(430079613u, "Server_OpenVendingAdmin", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(430079613u, "Server_OpenVendingAdmin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_OpenVendingAdmin(msg4);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in Server_OpenVendingAdmin");
					}
				}
				return true;
			}
			if (rpc == 1729350204 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RetrieveAllStoredItems"));
				}
				using (TimeWarning.New("Server_RetrieveAllStoredItems"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1729350204u, "Server_RetrieveAllStoredItems", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1729350204u, "Server_RetrieveAllStoredItems", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_RetrieveAllStoredItems(msg5);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in Server_RetrieveAllStoredItems");
					}
				}
				return true;
			}
			if (rpc == 51939428 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_Shop"));
				}
				using (TimeWarning.New("Server_Shop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(51939428u, "Server_Shop", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(51939428u, "Server_Shop", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_Shop(msg6);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in Server_Shop");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.rentableShop == null)
		{
			return;
		}
		TimeSinceShopOpened = TimeSince.op_Implicit(info.msg.rentableShop.timeSinceShopOpened);
		ShopNumber = info.msg.rentableShop.shopNumber;
		if (!base.isServer)
		{
			return;
		}
		AssignShopNumberIfUnset();
		if (info.msg.rentableShop.storedItemIds.Count == info.msg.rentableShop.storedItems.Count)
		{
			for (int i = 0; i < info.msg.rentableShop.storedItems.Count; i++)
			{
				PlayerStoreContents playerStoreContents = Pool.Get<PlayerStoreContents>();
				long num = 0L;
				playerStoreContents.Load(timeStamp: (info.msg.rentableShop.storedItemTimestamp.Count <= i) ? CurrentTime.ToBinary() : info.msg.rentableShop.storedItemTimestamp[i], forId: info.msg.rentableShop.storedItemIds[i], itemsToLoad: info.msg.rentableShop.storedItems[i], owner: this);
				savedContent.Add(playerStoreContents.ownerID, playerStoreContents);
			}
		}
		if (IsOn())
		{
			lastRentCheck = TimeSince.op_Implicit(0f);
			InvokeRepeating(DeductRent, 60f, 60f);
		}
		nextRentDue = info.msg.rentableShop.nextRentDue;
	}

	public VendingMachine GetServerVendingMachine()
	{
		return SpawnedVendingMachineRef.Get(base.isServer);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			CurrentRentMultiplier = 1f;
		}
		AllShops.Add(this);
		AssignShopNumberIfUnset();
		InvokeRandomized(ClearOutOldItems, 60f, 60f, 10f);
	}

	private void AssignShopNumberIfUnset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (ShopNumber != 0)
		{
			return;
		}
		int num = 0;
		Enumerator<RentableShop> enumerator = AllShops.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RentableShop current = enumerator.Current;
				if (current.ShopNumber > num)
				{
					num = current.ShopNumber;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		ShopNumber = num + 1;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		AllShops.Remove(this);
		foreach (KeyValuePair<ulong, PlayerStoreContents> item in savedContent)
		{
			PlayerStoreContents value = item.Value;
			Pool.Free<PlayerStoreContents>(ref value);
		}
	}

	private void ClearOutOldItems()
	{
		PooledList<ulong> val = Pool.Get<PooledList<ulong>>();
		try
		{
			DateTime dateTime = CurrentTime + TimeSpan.FromHours((double)AdditionalCheckTimeHoursDebug);
			foreach (KeyValuePair<ulong, PlayerStoreContents> item in savedContent)
			{
				if ((dateTime - item.Value.contentsTimeStamp).TotalMinutes > (double)MaxStoredItemsDurationMinutes)
				{
					((List<ulong>)(object)val).Add(item.Key);
				}
			}
			foreach (ulong item2 in (List<ulong>)(object)val)
			{
				PlayerStoreContents playerStoreContents = savedContent[item2];
				savedContent.Remove(item2);
				Pool.Free<PlayerStoreContents>(ref playerStoreContents);
			}
			if (((List<ulong>)(object)val).Count > 0)
			{
				SendNetworkUpdate();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		if (base.CanUseNetworkCache(connection) && !savedContent.ContainsKey(connection.userid))
		{
			return !IsIntruder(connection.userid);
		}
		return false;
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void Server_OpenVendingAdmin(RPCMessage msg)
	{
		VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
		if ((Object)(object)vendingMachine != (Object)null)
		{
			vendingMachine.RPC_OpenAdmin(msg);
		}
	}

	private void DeductRent()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		nextRentDue -= TimeSince.op_Implicit(lastRentCheck);
		lastRentCheck = TimeSince.op_Implicit(0f);
		if (nextRentDue <= 0f)
		{
			nextRentDue += 3600f;
			VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
			int amount = Mathf.RoundToInt((float)ScrapPerHourRent * CurrentRentMultiplier);
			if ((Object)(object)vendingMachine != (Object)null && vendingMachine.inventory.GetAmount(ScrapDef.itemid, onlyUsableAmounts: false) >= amount)
			{
				vendingMachine.inventory.UseAmount(ScrapDef, ref amount);
				return;
			}
			CloseStore();
			CurrentRentMultiplier = 1f;
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.rentableShop = Pool.Get<RentableShop>();
		info.msg.rentableShop.hasStoredItems = false;
		info.msg.rentableShop.nextRentDue = nextRentDue;
		info.msg.rentableShop.timeSinceShopOpened = TimeSince.op_Implicit(TimeSinceShopOpened);
		info.msg.rentableShop.shopNumber = ShopNumber;
		if (info.forDisk)
		{
			info.msg.rentableShop.storedItemIds = (List<ulong>)(object)Pool.Get<PooledList<ulong>>();
			info.msg.rentableShop.storedItems = (List<ItemContainer>)(object)Pool.Get<PooledList<ItemContainer>>();
			info.msg.rentableShop.storedItemTimestamp = (List<long>)(object)Pool.Get<PooledList<long>>();
			{
				foreach (KeyValuePair<ulong, PlayerStoreContents> item in savedContent)
				{
					info.msg.rentableShop.storedItemIds.Add(item.Key);
					info.msg.rentableShop.storedItems.Add(item.Value.items.Save());
					info.msg.rentableShop.storedItemTimestamp.Add(item.Value.contentsTimeStamp.ToBinary());
				}
				return;
			}
		}
		if (info.forConnection != null)
		{
			info.msg.rentableShop.hasStoredItems = savedContent.ContainsKey(info.forConnection.userid);
			info.msg.rentableShop.isLocalPlayerIntruder = IsIntruder(info.forConnection.userid);
		}
	}

	public override void PostInitShared()
	{
		base.PostInitShared();
		if (base.isServer && !Application.isLoadingSave)
		{
			ShopSignEntity.LockSign(null);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_OpenStore(RPCMessage msg)
	{
		if (CanPlayerOpenShop(msg.player) && Interface.CallHook("OnRentableShopOpen", this, msg.player) == null)
		{
			if (IsOn())
			{
				CurrentRentMultiplier++;
				CloseStore();
			}
			SetFlag(Flags.On, b: true);
			OnShopOpened(msg.player);
		}
	}

	public void ResetTimeSinceShopOpened()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		TimeSinceShopOpened = TimeSince.op_Implicit(ProtectionFromTakeoverHours * 60f * 60f);
		SendNetworkUpdate();
	}

	private void CloseStore(bool notify = true)
	{
		if (Interface.CallHook("OnRentableShopClose", this, notify) == null)
		{
			SetFlag(Flags.On, b: false);
			intruders.Clear();
			OnShopClosed(notify);
		}
	}

	public void OnShopOpened(BasePlayer byPlayer)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		TimeSinceShopOpened = TimeSince.op_Implicit(0f);
		ShopOwnerId = byPlayer.userID;
		InvisibleVendingMachine invisibleVendingMachine = base.gameManager.CreateEntity(VendingMachinePrefab.resourcePath, ShopkeeperSpawnPoint.position, ShopkeeperSpawnPoint.rotation) as InvisibleVendingMachine;
		invisibleVendingMachine.SetParent(this, worldPositionStays: true);
		invisibleVendingMachine.Spawn();
		SpawnedVendingMachineRef = new EntityRef<VendingMachine>(invisibleVendingMachine.net.ID);
		NPCShopKeeper nPCShopKeeper = base.gameManager.CreateEntity(ShopkeeperPrefab.resourcePath, ShopkeeperSpawnPoint.position, ShopkeeperSpawnPoint.rotation) as NPCShopKeeper;
		nPCShopKeeper.machine = invisibleVendingMachine;
		nPCShopKeeper.SetParent(this, worldPositionStays: true);
		nPCShopKeeper.Spawn();
		nPCShopKeeper.SetParent(this, worldPositionStays: true);
		SpawnedShopkeeperRef = new EntityRef<NPCShopKeeper>(nPCShopKeeper.net.ID);
		if ((Object)(object)ShopSign != (Object)null)
		{
			ShopSign.LockSign(byPlayer);
			ShopSign.SetFlag(Flags.Reserved3, b: true);
		}
		lastRentCheck = TimeSince.op_Implicit(0f);
		nextRentDue = 3600f;
		InvokeRepeating(DeductRent, 60f, 60f);
		int amount = Mathf.RoundToInt((float)InitialScrapFee * CurrentRentMultiplier);
		byPlayer.inventory.containerMain.UseAmount(ScrapDef, ref amount);
		byPlayer.inventory.containerBelt.UseAmount(ScrapDef, ref amount);
		byPlayer.inventory.containerWear.UseAmount(ScrapDef, ref amount);
		int amount2 = Mathf.RoundToInt((float)ScrapPerHourRent * 12f * CurrentRentMultiplier);
		byPlayer.inventory.MoveItemsIntoContainer(ScrapDef, amount2, invisibleVendingMachine.inventory);
		Interface.CallHook("OnRentableShopOpened", this, byPlayer);
	}

	public void OnShopClosed(bool notify)
	{
		if (SpawnedShopkeeperRef.IsValid(serverside: true))
		{
			SpawnedShopkeeperRef.Get(base.isServer).Kill();
			SpawnedShopkeeperRef.Set(null);
		}
		if (SpawnedVendingMachineRef.IsValid(serverside: true))
		{
			VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
			SaveContentsOfVendingMachineToReclaim(vendingMachine);
			vendingMachine.Kill();
			SpawnedVendingMachineRef.Set(null);
		}
		if ((Object)(object)ShopSign != (Object)null)
		{
			ShopSign.LockSign(null);
			ShopSign.ClearContent();
		}
		if (notify)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(ShopOwnerId);
			if ((Object)(object)basePlayer != (Object)null)
			{
				basePlayer.ShowToast(GameTip.Styles.Error, Phrase_ShopClosedNotificationPhrase, false);
			}
			else
			{
				BasePlayer.RecordToastToPlayOnReconnect(GameTip.Styles.Error, Phrase_ShopClosedNotificationPhrase, ShopOwnerId);
			}
		}
		ShopOwnerId = 0uL;
		CancelInvoke(DeductRent);
		Interface.CallHook("OnRentableShopClosed", this, notify);
	}

	private void SaveContentsOfVendingMachineToReclaim(VendingMachine vm)
	{
		if (vm.inventory.IsEmpty())
		{
			return;
		}
		if (savedContent.TryGetValue(ShopOwnerId, out var value))
		{
			value.items.Clear();
			for (int i = 0; i < vm.inventory.capacity; i++)
			{
				vm.inventory.GetSlot(i)?.MoveToContainer(value.items);
			}
		}
		else
		{
			PlayerStoreContents playerStoreContents = Pool.Get<PlayerStoreContents>();
			playerStoreContents.Setup(this, vm, ShopOwnerId);
			savedContent.Add(ShopOwnerId, playerStoreContents);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_OpenStoreInventory(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && ((ulong)msg.player.userID == ShopOwnerId || IsIntruder(msg.player.userID)))
		{
			VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
			if ((Object)(object)vendingMachine != (Object)null)
			{
				vendingMachine.PlayerOpenLoot(msg.player, "", doPositionChecks: false);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_RetrieveAllStoredItems(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null))
		{
			RetrieveAllStoredItems(player);
		}
	}

	private void RetrieveAllStoredItems(BasePlayer player)
	{
		if (savedContent.TryGetValue(player.userID, out var value))
		{
			value.GiveToPlayer(player);
			savedContent.Remove(player.userID);
			Pool.Free<PlayerStoreContents>(ref value);
		}
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_Shop(RPCMessage msg)
	{
		VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
		if ((Object)(object)vendingMachine != (Object)null)
		{
			vendingMachine.OpenShop(msg.player, vendingMachine.customerPanel);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void BreakIn(RPCMessage rpc)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		RPCProgressBarState rPCProgressBarState = (RPCProgressBarState)rpc.read.Int32();
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && player.CanInteract())
		{
			switch (rPCProgressBarState)
			{
			case RPCProgressBarState.Start:
				breakInStarts[player.userID] = TimeSince.op_Implicit(0f);
				break;
			case RPCProgressBarState.Cancel:
				breakInStarts.Remove(player.userID);
				break;
			case RPCProgressBarState.Complete:
				CompleteBreakIn(player);
				break;
			}
		}
	}

	private void CompleteBreakIn(BasePlayer player)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!breakInStarts.TryGetValue(player.userID, out var value))
		{
			return;
		}
		breakInStarts.Remove(player.userID);
		if (TimeSince.op_Implicit(value) < ApartmentCommands.breakinseconds * 0.9f || Interface.CallHook("OnRentableShopBreakInComplete", this, player) != null)
		{
			return;
		}
		VendingMachine vendingMachine = SpawnedVendingMachineRef.Get(base.isServer);
		if ((Object)(object)vendingMachine == (Object)null)
		{
			return;
		}
		if (vendingMachine.IsInventoryEmpty())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, Phrase_BreakInUnoccupied, false);
			return;
		}
		if (ShopOwnerId == (ulong)player.userID)
		{
			player.ShowToast(GameTip.Styles.Red_Normal, Phrase_BreakInAlreadyAuthed, false);
			return;
		}
		Item activeItem = player.GetActiveItem();
		if (activeItem != null && !((Object)(object)activeItem.info != (Object)(object)ApartmentDoor.MasterKeyDef) && activeItem.amount >= 1)
		{
			activeItem.UseItem();
			AddIntruder(player.userID);
			player.ShowToast(GameTip.Styles.Blue_Long, Phrase_BreakInSuccess, false);
			Interface.CallHook("OnRentableShopBreakInCompleted", this, player);
		}
	}

	public void AddIntruder(ulong user)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		intruders[user] = TimeUntil.op_Implicit(ApartmentCommands.intruderauthseconds);
		SendNetworkUpdate();
	}

	public bool IsIntruder(ulong user)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!intruders.TryGetValue(user, out var value))
		{
			return false;
		}
		if (TimeUntil.op_Implicit(value) <= 0f)
		{
			intruders.Remove(user);
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void Server_CloseStore(RPCMessage rpc)
	{
		if (!((Object)(object)rpc.player == (Object)null) && (ulong)rpc.player.userID == ShopOwnerId)
		{
			CloseStore(notify: false);
			RetrieveAllStoredItems(rpc.player);
			CurrentRentMultiplier = 1f;
		}
	}

	[ServerVar]
	public static void ProcessRentTick()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<RentableShop> enumerator = AllShops.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RentableShop current = enumerator.Current;
				if (current.IsOn())
				{
					current.nextRentDue = 0f;
					current.DeductRent();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Randomise the owner of the nearest shop in 10m. Useful for testing")]
	public static void RandomiseOwnerOfNearestShop(ConsoleSystem.Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		RentableShop rentableShop = FindClosestRentableShop(((Component)basePlayer).transform.position);
		if ((Object)(object)rentableShop != (Object)null)
		{
			rentableShop.ShopOwnerId = (uint)Random.Range(10, 99999);
			Signage shopSign = rentableShop.ShopSign;
			if ((Object)(object)shopSign != (Object)null)
			{
				shopSign.OwnerID = rentableShop.ShopOwnerId;
				shopSign.SendNetworkUpdate();
			}
			rentableShop.SendNetworkUpdate();
		}
	}

	[ServerVar(Help = "Close the nearest shop in 10m. Useful for testing")]
	public static void CloseNearestShop(ConsoleSystem.Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			RentableShop rentableShop = FindClosestRentableShop(((Component)basePlayer).transform.position);
			if ((Object)(object)rentableShop != (Object)null)
			{
				rentableShop.CloseStore();
				rentableShop.CurrentRentMultiplier = 1f;
			}
		}
	}

	[ServerVar(Help = "Reset the takeover protection duration of nearest shop. Useful for testing")]
	public static void ResetTakeoverProtectionOfClosestShop(ConsoleSystem.Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			RentableShop rentableShop = FindClosestRentableShop(((Component)basePlayer).transform.position);
			if ((Object)(object)rentableShop != (Object)null)
			{
				rentableShop.ResetTimeSinceShopOpened();
			}
		}
	}

	private static RentableShop FindClosestRentableShop(Vector3 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		RentableShop result = null;
		float num = float.MaxValue;
		Enumerator<RentableShop> enumerator = AllShops.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RentableShop current = enumerator.Current;
				float num2 = current.Distance2D(pos);
				if (num2 < 10f && num2 < num && current.IsOn())
				{
					num = num2;
					result = current;
				}
			}
			return result;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static bool DoesPlayerOwnRentableShop(BasePlayer player)
	{
		return DoesPlayerOwnRentableShop(player.userID);
	}

	private static bool DoesPlayerOwnRentableShop(ulong id)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<RentableShop> enumerator = AllShops.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				RentableShop current = enumerator.Current;
				if ((Object)(object)current != (Object)null && current.IsOn() && current.ShopOwnerId == id)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}

	public bool CanPlayerOpenShop(BasePlayer player)
	{
		if (IsOwner(player))
		{
			return false;
		}
		if (IsShopRecentlyOpened)
		{
			return false;
		}
		if (base.isServer && DoesPlayerOwnRentableShop(player))
		{
			player.ShowToast(GameTip.Styles.Error, Phrase_OnlyOneShopAllowedPhrase, false);
			return false;
		}
		int amount = player.inventory.GetAmount(ScrapDef);
		CalculateScrapCosts(player, out var _, out var _, out var _, out var total);
		int num = total;
		return amount >= num;
	}

	public void CalculateScrapCosts(BasePlayer forPlayer, out int perHour, out int startingFee, out float multi, out int total)
	{
		perHour = ScrapPerHourRent;
		startingFee = InitialScrapFee;
		multi = CurrentRentMultiplier;
		if (IsOn() && ShopOwnerId != (ulong)forPlayer.userID)
		{
			multi += 1f;
		}
		total = Mathf.RoundToInt(((float)perHour * 12f + (float)startingFee) * multi);
	}

	public bool IsOwner(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		return (ulong)player.userID == ShopOwnerId;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: SpawnedShopkeeperRef for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_SpawnedShopkeeperRef);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: SpawnedVendingMachineRef for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_SpawnedVendingMachineRef);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ShopOwnerId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ShopOwnerId);
			return true;
		case 3:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: CurrentRentMultiplier for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_CurrentRentMultiplier);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_SpawnedShopkeeperRef;
				EntityRef<NPCShopKeeper> _sync_SpawnedShopkeeperRef = NetworkReadEx.EntityRef<NPCShopKeeper>(reader);
				__sync_SpawnedShopkeeperRef = _sync_SpawnedShopkeeperRef;
			}
			catch (Exception ex4)
			{
				Debug.LogException(ex4);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_SpawnedVendingMachineRef;
				EntityRef<VendingMachine> _sync_SpawnedVendingMachineRef = NetworkReadEx.EntityRef<VendingMachine>(reader);
				__sync_SpawnedVendingMachineRef = _sync_SpawnedVendingMachineRef;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_ShopOwnerId;
				ulong _sync_ShopOwnerId = reader.UInt64();
				__sync_ShopOwnerId = _sync_ShopOwnerId;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 3:
			try
			{
				_ = __sync_CurrentRentMultiplier;
				float _sync_CurrentRentMultiplier = reader.Float();
				__sync_CurrentRentMultiplier = _sync_CurrentRentMultiplier;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"SpawnedShopkeeperRef" => 0, 
			"SpawnedVendingMachineRef" => 1, 
			"ShopOwnerId" => 2, 
			"CurrentRentMultiplier" => 3, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
		WriteSyncVar(3, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
		OnSyncVar(3, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_SpawnedShopkeeperRef = default(EntityRef<NPCShopKeeper>);
		__sync_SpawnedVendingMachineRef = default(EntityRef<VendingMachine>);
		__sync_ShopOwnerId = 0uL;
		__sync_CurrentRentMultiplier = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			3 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	public RentableShop()
	{
		CurrentRentMultiplier = 1f;
		breakInStarts = new Dictionary<ulong, TimeSince>();
		intruders = new Dictionary<ulong, TimeUntil>();
		savedContent = new Dictionary<ulong, PlayerStoreContents>();
		base._002Ector();
	}
}
