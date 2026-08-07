using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class VendingMachine : ContainerIOEntity, IUGCBrowserEntity, IFoodSpoilModifier, IIndustrialStorageCallbackReceiver, PlayerInventory.ICanMoveFrom
{
	private enum HistoryCategory
	{
		History,
		BestSold,
		MostRevenue
	}

	[Serializable]
	public class PurchaseDetails
	{
		public int itemId;

		public int amount;

		public int priceId;

		public int price;

		public int timestamp;

		public bool itemIsBp;

		public bool priceIsBp;
	}

	public static class VendingMachineFlags
	{
		public const Flags EmptyInv = Flags.Reserved1;

		public const Flags IsVending = Flags.Reserved2;

		public const Flags DroneAccessible = Flags.Reserved3;

		public const Flags Broadcasting = Flags.Reserved4;

		public const Flags OutOfStock = Flags.Reserved5;

		public const Flags NoDirectAccess = Flags.Reserved6;

		public const Flags SkinMode = Flags.Reserved9;
	}

	[ServerVar]
	public static int max_returned = 100;

	[ServerVar]
	public static int max_processed = 10000;

	[ServerVar]
	public static int max_history = 10000;

	private List<PurchaseDetails> purchaseHistory = new List<PurchaseDetails>();

	private Dictionary<ulong, int> uniqueCustomers = new Dictionary<ulong, int>();

	[Header("VendingMachine")]
	public static readonly Phrase WaitForVendingMessage = new Phrase("vendingmachine.wait", "Please wait...");

	public GameObjectRef adminMenuPrefab;

	public string customerPanel = "";

	public SellOrderContainer sellOrders;

	public SoundPlayer buySound;

	public string shopName = "A Shop";

	public int maxCurrencyVolume = 1;

	public Vector3 localDropPosition = Vector3.zero;

	public GameObjectRef mapMarkerPrefab;

	public bool IsLocalized;

	[Range(0f, 1f)]
	public float PoweredFoodSpoilageRateMultiplier = 0.1f;

	public int PowerConsumption = 5;

	public bool IsInDeepSeaCached;

	[Header("Drone Prediction")]
	public DeliveryDroneConfig predictionConfig;

	private HashSet<BasePlayer> purchasingPlayers = new HashSet<BasePlayer>();

	private Action fullUpdateCached;

	private ulong nameLastEditedBy;

	private bool droneAccessible;

	protected BasePlayer vend_Player;

	private int vend_sellOrderID;

	private int vend_numberOfTransactions;

	public bool transactionActive;

	private VendingMachineMapMarker myMarker;

	private bool industrialItemIncoming;

	private static readonly Phrase NotAdministratingError = new Phrase("error.notadministrating", "Cannot move item: Not administrating!");

	public static readonly Phrase TooManySellOrders = new Phrase("error_toomanysellorders", "Too many sell orders");

	private int __sync_PendingItemId;

	[Sync(Pack = false)]
	public int PendingItemId
	{
		[CompilerGenerated]
		get
		{
			return __sync_PendingItemId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PendingItemId, value))
			{
				__sync_PendingItemId = value;
				byte nameID = __GetWeaverID("PendingItemId");
				SV_SyncVarSend(nameID);
			}
		}
	}

	public ItemDefinition blueprintBaseDef => ItemManager.blueprintBaseDef;

	public uint[] GetContentCRCs => null;

	public UGCType ContentType => UGCType.VendingMachine;

	public List<ulong> EditingHistory => new List<ulong> { nameLastEditedBy };

	public BaseNetworkable UgcEntity
	{
		get
		{
			if (!(this is NPCVendingMachine))
			{
				return this;
			}
			return null;
		}
	}

	public string ContentString => shopName;

	public virtual EraRestriction CurrentEraRestriction => (EraRestriction)7;

	public virtual bool ShouldRecordStats => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VendingMachine.OnRpcMessage"))
		{
			if (rpc == 3011053703u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BuyItem"));
				}
				using (TimeWarning.New("BuyItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3011053703u, "BuyItem", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3011053703u, "BuyItem", this, player, 3f))
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
							BuyItem(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BuyItem");
					}
				}
				return true;
			}
			if (rpc == 491261180 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BuyItemRentableShop"));
				}
				using (TimeWarning.New("BuyItemRentableShop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(491261180u, "BuyItemRentableShop", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(491261180u, "BuyItemRentableShop", this, player, 9f))
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
							BuyItemRentableShop(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in BuyItemRentableShop");
					}
				}
				return true;
			}
			if (rpc == 1626480840 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_AddSellOrder"));
				}
				using (TimeWarning.New("RPC_AddSellOrder"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1626480840u, "RPC_AddSellOrder", this, player, 3f))
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
							RPC_AddSellOrder(msg2);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_AddSellOrder");
					}
				}
				return true;
			}
			if (rpc == 169239598 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Broadcast"));
				}
				using (TimeWarning.New("RPC_Broadcast"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(169239598u, "RPC_Broadcast", this, player, 3f))
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
							RPC_Broadcast(msg3);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_Broadcast");
					}
				}
				return true;
			}
			if (rpc == 330108049 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DeleteAllSellOrders"));
				}
				using (TimeWarning.New("RPC_DeleteAllSellOrders"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(330108049u, "RPC_DeleteAllSellOrders", this, player, 3f))
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
							RPC_DeleteAllSellOrders(msg4);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_DeleteAllSellOrders");
					}
				}
				return true;
			}
			if (rpc == 3680901137u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_DeleteSellOrder"));
				}
				using (TimeWarning.New("RPC_DeleteSellOrder"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3680901137u, "RPC_DeleteSellOrder", this, player, 3f))
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
							RPC_DeleteSellOrder(msg5);
						}
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
						player.Kick("RPC Error in RPC_DeleteSellOrder");
					}
				}
				return true;
			}
			if (rpc == 1788835019 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_MoveSellOrder"));
				}
				using (TimeWarning.New("RPC_MoveSellOrder"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1788835019u, "RPC_MoveSellOrder", this, player, 3f))
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
							RPC_MoveSellOrder(msg6);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
						player.Kick("RPC Error in RPC_MoveSellOrder");
					}
				}
				return true;
			}
			if (rpc == 2555993359u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenAdmin"));
				}
				using (TimeWarning.New("RPC_OpenAdmin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2555993359u, "RPC_OpenAdmin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg7 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenAdmin(msg7);
						}
					}
					catch (Exception ex8)
					{
						Debug.LogException(ex8);
						player.Kick("RPC Error in RPC_OpenAdmin");
					}
				}
				return true;
			}
			if (rpc == 36164441 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenShop"));
				}
				using (TimeWarning.New("RPC_OpenShop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(36164441u, "RPC_OpenShop", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg8 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenShop(msg8);
						}
					}
					catch (Exception ex9)
					{
						Debug.LogException(ex9);
						player.Kick("RPC Error in RPC_OpenShop");
					}
				}
				return true;
			}
			if (rpc == 2947824655u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenShopNoLOS"));
				}
				using (TimeWarning.New("RPC_OpenShopNoLOS"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2947824655u, "RPC_OpenShopNoLOS", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg9 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_OpenShopNoLOS(msg9);
						}
					}
					catch (Exception ex10)
					{
						Debug.LogException(ex10);
						player.Kick("RPC Error in RPC_OpenShopNoLOS");
					}
				}
				return true;
			}
			if (rpc == 3346513099u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RotateVM"));
				}
				using (TimeWarning.New("RPC_RotateVM"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3346513099u, "RPC_RotateVM", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg10 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RotateVM(msg10);
						}
					}
					catch (Exception ex11)
					{
						Debug.LogException(ex11);
						player.Kick("RPC Error in RPC_RotateVM");
					}
				}
				return true;
			}
			if (rpc == 2892597292u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetSkinMode"));
				}
				using (TimeWarning.New("RPC_SetSkinMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2892597292u, "RPC_SetSkinMode", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg11 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_SetSkinMode(msg11);
						}
					}
					catch (Exception ex12)
					{
						Debug.LogException(ex12);
						player.Kick("RPC Error in RPC_SetSkinMode");
					}
				}
				return true;
			}
			if (rpc == 1012779214 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UpdateShopName"));
				}
				using (TimeWarning.New("RPC_UpdateShopName"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1012779214u, "RPC_UpdateShopName", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg12 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UpdateShopName(msg12);
						}
					}
					catch (Exception ex13)
					{
						Debug.LogException(ex13);
						player.Kick("RPC Error in RPC_UpdateShopName");
					}
				}
				return true;
			}
			if (rpc == 1147600716 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_RequestLongTermData"));
				}
				using (TimeWarning.New("SV_RequestLongTermData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1147600716u, "SV_RequestLongTermData", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1147600716u, "SV_RequestLongTermData", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg13 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_RequestLongTermData(msg13);
						}
					}
					catch (Exception ex14)
					{
						Debug.LogException(ex14);
						player.Kick("RPC Error in SV_RequestLongTermData");
					}
				}
				return true;
			}
			if (rpc == 3957849636u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_RequestPurchaseData"));
				}
				using (TimeWarning.New("SV_RequestPurchaseData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3957849636u, "SV_RequestPurchaseData", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3957849636u, "SV_RequestPurchaseData", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg14 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SV_RequestPurchaseData(msg14);
						}
					}
					catch (Exception ex15)
					{
						Debug.LogException(ex15);
						player.Kick("RPC Error in SV_RequestPurchaseData");
					}
				}
				return true;
			}
			if (rpc == 3559014831u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TransactionStart"));
				}
				using (TimeWarning.New("TransactionStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3559014831u, "TransactionStart", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							TransactionStart(rpc4);
						}
					}
					catch (Exception ex16)
					{
						Debug.LogException(ex16);
						player.Kick("RPC Error in TransactionStart");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[ServerVar(Help = "Wipe the backend stats data on all vending machines. Slow operation.")]
	public static void ClearAllVendingHistory()
	{
		VendingMachine[] array = Object.FindObjectsByType<VendingMachine>((FindObjectsSortMode)0);
		foreach (VendingMachine vendingMachine in array)
		{
			if (!vendingMachine.isClient)
			{
				vendingMachine.ClearPurchaseHistory();
			}
		}
	}

	[ServerVar(Help = "Wipe the backend customer stats data on all vending machines. Slow operation.")]
	public static void ClearAllVendingCustomerHistory()
	{
		VendingMachine[] array = Object.FindObjectsByType<VendingMachine>((FindObjectsSortMode)0);
		foreach (VendingMachine vendingMachine in array)
		{
			if (!vendingMachine.isClient)
			{
				vendingMachine.ClearCustomerHistory();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void SV_RequestLongTermData(RPCMessage msg)
	{
		if (CanPlayerAdmin(msg.player))
		{
			int seconds = 86400;
			VendingMachineLongTermStats val = Pool.Get<VendingMachineLongTermStats>();
			val.numberOfPurchases = purchaseHistory.Count;
			val.bestSalesHour = GetPeakSaleHourTimestamp(seconds);
			val.uniqueCustomers = GetUniqueCustomers();
			val.repeatCustomers = GetRepeatCustomers();
			val.bestCustomer = GetBestCustomer();
			ClientRPC(RpcTarget.Player("CL_ReceiveLongTermData", msg.player), val);
			val.Dispose();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void SV_RequestPurchaseData(RPCMessage msg)
	{
		if (CanPlayerAdmin(msg.player))
		{
			HistoryCategory historyCategory = (HistoryCategory)msg.read.Int32();
			int minutes = msg.read.Int32();
			VendingMachinePurchaseHistoryMessage proto = GetProto(historyCategory, minutes);
			ClientRPC(RpcTarget.Player("CL_ReceivePurchaseData", msg.player), (int)historyCategory, proto);
			proto.Dispose();
		}
	}

	public void AddPurchaseHistory(int itemId, int amount, int priceId, int price, bool itemIsBp, bool priceIsBp)
	{
		if (purchaseHistory.Count > max_history)
		{
			purchaseHistory.RemoveAt(0);
		}
		purchaseHistory.Add(new PurchaseDetails
		{
			itemId = itemId,
			amount = amount,
			priceId = priceId,
			price = price,
			timestamp = Epoch.Current,
			itemIsBp = itemIsBp,
			priceIsBp = priceIsBp
		});
	}

	public void RegisterCustomer(ulong userId)
	{
		if (uniqueCustomers.ContainsKey(userId))
		{
			uniqueCustomers[userId]++;
		}
		else
		{
			uniqueCustomers.Add(userId, 1);
		}
	}

	public void RemovePurchaseHistory(int index)
	{
		purchaseHistory.RemoveAt(index);
	}

	public void ClearPurchaseHistory()
	{
		purchaseHistory.Clear();
	}

	public void ClearCustomerHistory()
	{
		uniqueCustomers.Clear();
	}

	private VendingMachinePurchaseHistoryMessage GetProto(HistoryCategory category, int minutes)
	{
		if (minutes == 0)
		{
			minutes = 999999;
		}
		VendingMachinePurchaseHistoryMessage val = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		switch (category)
		{
		case HistoryCategory.History:
			val.transactions = GetEntriesProto(GetRecentPurchases(minutes * 60));
			break;
		case HistoryCategory.BestSold:
			val.smallTransactions = GetEntriesProtoSmall(GetBestSoldItems(minutes * 60));
			break;
		case HistoryCategory.MostRevenue:
			val.smallTransactions = GetEntriesProtoSmall(GetMostRevenueGeneratingItems(minutes * 60));
			break;
		}
		return val;
	}

	private List<VendingMachinePurchaseHistoryEntryMessage> GetEntriesProto(List<PurchaseDetails> details)
	{
		List<VendingMachinePurchaseHistoryEntryMessage> list = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
		foreach (PurchaseDetails detail in details)
		{
			list.Add(GetEntryProto(detail));
		}
		return list;
	}

	private List<PurchaseDetails> GetListFromProto(List<VendingMachinePurchaseHistoryEntryMessage> details)
	{
		List<PurchaseDetails> list = new List<PurchaseDetails>();
		foreach (VendingMachinePurchaseHistoryEntryMessage detail in details)
		{
			list.Add(new PurchaseDetails
			{
				itemId = detail.itemID,
				amount = detail.amount,
				priceId = detail.priceID,
				price = detail.price,
				timestamp = detail.dateTime,
				itemIsBp = detail.itemIsBp,
				priceIsBp = detail.priceIsBp
			});
		}
		return list;
	}

	private List<VendingMachinePurchaseHistoryEntrySmallMessage> GetEntriesProtoSmall(List<PurchaseDetails> details)
	{
		List<VendingMachinePurchaseHistoryEntrySmallMessage> list = Pool.Get<List<VendingMachinePurchaseHistoryEntrySmallMessage>>();
		foreach (PurchaseDetails detail in details)
		{
			list.Add(GetEntryProtoSmall(detail));
		}
		return list;
	}

	private VendingMachinePurchaseHistoryEntryMessage GetEntryProto(PurchaseDetails details)
	{
		VendingMachinePurchaseHistoryEntryMessage obj = Pool.Get<VendingMachinePurchaseHistoryEntryMessage>();
		obj.itemID = details.itemId;
		obj.amount = details.amount;
		obj.priceID = details.priceId;
		obj.price = details.price;
		obj.dateTime = details.timestamp;
		obj.priceIsBp = details.priceIsBp;
		obj.itemIsBp = details.itemIsBp;
		return obj;
	}

	private VendingMachinePurchaseHistoryEntrySmallMessage GetEntryProtoSmall(PurchaseDetails details)
	{
		VendingMachinePurchaseHistoryEntrySmallMessage obj = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		obj.itemID = details.itemId;
		obj.amount = details.amount;
		obj.priceID = details.priceId;
		obj.price = details.price;
		obj.priceIsBp = details.priceIsBp;
		obj.itemIsBp = details.itemIsBp;
		return obj;
	}

	public List<PurchaseDetails> GetRecentPurchases(int seconds)
	{
		int currentTime = Epoch.Current;
		return (from p in purchaseHistory
			where currentTime - p.timestamp <= seconds
			orderby p.timestamp descending
			select p).Take(max_returned).ToList();
	}

	public List<PurchaseDetails> GetBestSoldItems(int seconds)
	{
		int currentTime = Epoch.Current;
		return (from p in (from p in purchaseHistory
				where currentTime - p.timestamp <= seconds
				orderby p.timestamp descending
				select p).Take(max_processed)
			group p by new { p.itemId, p.itemIsBp, p.priceIsBp } into @group
			select new PurchaseDetails
			{
				itemId = @group.Key.itemId,
				amount = @group.Sum((PurchaseDetails p) => p.amount),
				priceId = 0,
				price = 0,
				timestamp = 0,
				itemIsBp = @group.Key.itemIsBp,
				priceIsBp = @group.Key.priceIsBp
			} into p
			orderby p.amount descending
			select p).Take(max_returned).ToList();
	}

	public List<PurchaseDetails> GetMostRevenueGeneratingItems(int seconds)
	{
		int currentTime = Epoch.Current;
		return (from p in (from p in purchaseHistory
				where currentTime - p.timestamp <= seconds
				orderby p.timestamp descending
				select p).Take(max_processed)
			group p by new { p.itemId, p.priceId, p.itemIsBp, p.priceIsBp } into @group
			select new PurchaseDetails
			{
				itemId = @group.Key.itemId,
				amount = @group.Sum((PurchaseDetails p) => p.amount),
				priceId = @group.Key.priceId,
				price = @group.Sum((PurchaseDetails p) => p.price),
				timestamp = 0,
				itemIsBp = @group.Key.itemIsBp,
				priceIsBp = @group.Key.priceIsBp
			} into p
			orderby p.price descending
			select p).Take(max_returned).ToList();
	}

	public long GetPeakSaleHourTimestamp(int seconds)
	{
		int currentTime = Epoch.Current;
		return (from p in (from p in purchaseHistory
				where currentTime - p.timestamp <= seconds
				orderby p.timestamp descending
				select p).Take(max_processed)
			group p by p.timestamp into @group
			select new
			{
				Timestamp = @group.Key,
				TotalSales = @group.Sum((PurchaseDetails p) => p.amount)
			} into s
			orderby s.TotalSales descending
			select s).FirstOrDefault()?.Timestamp ?? (-1);
	}

	public int GetUniqueCustomers()
	{
		return uniqueCustomers.Count;
	}

	public int GetRepeatCustomers()
	{
		return uniqueCustomers.Count((KeyValuePair<ulong, int> c) => c.Value > 1);
	}

	public int GetBestCustomer()
	{
		if (uniqueCustomers.Count == 0)
		{
			return 0;
		}
		return uniqueCustomers.Values.Max();
	}

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}

	public float GetSpoilMultiplier(Item arg)
	{
		if (IsPowered())
		{
			return PoweredFoodSpoilageRateMultiplier;
		}
		return 1f;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.vendingMachineStats != null)
		{
			purchaseHistory = GetListFromProto(info.msg.vendingMachineStats.purchaseHistory);
			for (int i = 0; i < info.msg.vendingMachineStats.customers.Count; i++)
			{
				uniqueCustomers.Add(info.msg.vendingMachineStats.customers[i], info.msg.vendingMachineStats.customersVisits[i]);
			}
		}
		if (info.msg.vendingMachine != null)
		{
			if (!IsLocalized)
			{
				shopName = info.msg.vendingMachine.shopName;
			}
			if (info.msg.vendingMachine.sellOrderContainer != null)
			{
				sellOrders = info.msg.vendingMachine.sellOrderContainer;
				sellOrders.ShouldPool = false;
			}
			if (info.fromDisk && base.isServer)
			{
				nameLastEditedBy = info.msg.vendingMachine.nameLastEditedBy;
				RefreshSellOrderStockLevel();
			}
		}
	}

	public static int GetTotalReceivedMerchandiseForOrder(SellOrder order)
	{
		return GetTotalReceivedMerchandiseForOrder(order.itemToSellAmount, order.receivedQuantityMultiplier);
	}

	public static int GetTotalReceivedMerchandiseForOrder(int merchAmountPerOrder, float multiplier)
	{
		float num = ((multiplier != 0f) ? multiplier : 1f);
		return Mathf.Max(Mathf.RoundToInt((float)merchAmountPerOrder * num), 1);
	}

	public static int GetTotalPriceForOrder(SellOrder order)
	{
		return GetTotalPriceForOrder(order.currencyAmountPerItem, order.priceMultiplier);
	}

	public static int GetTotalPriceForOrder(int currencyAmountPerItem, float multiplier)
	{
		float num = ((multiplier != 0f) ? multiplier : 1f);
		return Mathf.Max(Mathf.RoundToInt((float)currencyAmountPerItem * num), 1);
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (predictionConfig != null)
		{
			UpdateDronePrediction();
		}
	}

	protected void UpdateDronePrediction(bool checkForUpdate = true)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (predictionConfig == null)
		{
			droneAccessible = false;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
			return;
		}
		using (TimeWarning.New("VendingMachine.UpdateDronePrediction"))
		{
			bool flag = predictionConfig.IsVendingMachineAccessible(this, predictionConfig.vendingMachineOffset, out var hitInfo);
			flag |= predictionConfig.IsVendingMachineAccessible(this, predictionConfig.vendingMachineOffset + Vector3.forward * predictionConfig.maxDistanceFromVendingMachine, out hitInfo);
			bool flag2 = droneAccessible;
			bool flag3 = flag;
			droneAccessible = flag;
			using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope2.Set(Flags.Reserved3, flag);
			}
			if (checkForUpdate && flag2 != flag3)
			{
				CancelInvoke(fullUpdateCached);
				Invoke(fullUpdateCached, 0.2f);
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public bool IsDroneAccessible()
	{
		return droneAccessible;
	}

	public override void Save(SaveInfo info)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		base.Save(info);
		info.msg.vendingMachine = new VendingMachine();
		info.msg.vendingMachine.ShouldPool = false;
		info.msg.vendingMachine.shopName = shopName;
		if (info.forDisk)
		{
			info.msg.vendingMachine.nameLastEditedBy = nameLastEditedBy;
			if (ShouldRecordStats)
			{
				info.msg.vendingMachineStats = Pool.Get<VendingMachineStats>();
				info.msg.vendingMachineStats.purchaseHistory = GetEntriesProto(purchaseHistory);
				info.msg.vendingMachineStats.customers = uniqueCustomers.Keys.ToList();
				info.msg.vendingMachineStats.customersVisits = uniqueCustomers.Values.ToList();
			}
		}
		if (this is NPCVendingMachine)
		{
			info.msg.vendingMachine.translationToken = GetTranslationToken();
			info.msg.vendingMachine.inDeepSea = IsInDeepSeaCached;
		}
		if (sellOrders == null)
		{
			return;
		}
		info.msg.vendingMachine.sellOrderContainer = new SellOrderContainer();
		info.msg.vendingMachine.sellOrderContainer.ShouldPool = false;
		info.msg.vendingMachine.sellOrderContainer.sellOrders = new List<SellOrder>();
		foreach (SellOrder sellOrder in sellOrders.sellOrders)
		{
			if (CanSellOrBuyItem(sellOrder.itemToSellID, sellOrder.currencyID))
			{
				SellOrder val = new SellOrder();
				val.ShouldPool = false;
				sellOrder.CopyTo(val);
				info.msg.vendingMachine.sellOrderContainer.sellOrders.Add(val);
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.isServer)
		{
			fullUpdateCached = FullUpdate;
			UpdateDronePrediction(checkForUpdate: false);
			InstallDefaultSellOrders();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
			}
			base.inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
			RefreshSellOrderStockLevel();
			ItemContainer itemContainer = base.inventory;
			itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
			UpdateMapMarker();
		}
	}

	public override void DestroyShared()
	{
		if (Object.op_Implicit((Object)(object)myMarker))
		{
			myMarker.Kill();
			myMarker = null;
		}
		base.DestroyShared();
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return true;
	}

	public void FullUpdate()
	{
		if (base.inventory != null)
		{
			RefreshSellOrderStockLevel();
			UpdateMapMarker();
			SendNetworkUpdate();
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		CancelInvoke(fullUpdateCached);
		Invoke(fullUpdateCached, 0.2f);
	}

	public void RefreshSellOrderStockLevel(ItemDefinition itemDef = null)
	{
		int num = 0;
		foreach (SellOrder sellOrder in sellOrders.sellOrders)
		{
			if (!((Object)(object)itemDef == (Object)null) && itemDef.itemid != sellOrder.itemToSellID)
			{
				continue;
			}
			List<Item> list = Pool.Get<List<Item>>();
			GetItemsToSell(sellOrder, list);
			int num2 = sellOrder.itemToSellAmount;
			if ((Object)(object)ItemManager.FindItemDefinition(sellOrder.itemToSellID) == (Object)(object)NPCVendingMachine.ScrapItem && sellOrder.receivedQuantityMultiplier != 1f)
			{
				num2 = GetTotalPriceForOrder(num2, sellOrder.receivedQuantityMultiplier);
			}
			int inStock;
			if (list.Count < 0)
			{
				inStock = 0;
			}
			else
			{
				List<Item> source = list;
				Func<Item, int> selector = (Item x) => x.amount;
				Interface.CallHook("OnRefreshVendingStock", this, itemDef);
				inStock = source.Sum(selector) / num2;
			}
			sellOrder.inStock = inStock;
			float itemCondition = 0f;
			float itemConditionMax = 0f;
			int instanceData = 0;
			List<int> list2 = Pool.Get<List<int>>();
			int totalAttachmentSlots = 0;
			int ammoType = 0;
			int ammoCount = 0;
			if (list.Count > 0)
			{
				if (list[0].hasCondition)
				{
					itemCondition = list[0].condition;
					itemConditionMax = list[0].maxCondition;
				}
				if ((Object)(object)list[0].info != (Object)null && (list[0].info.amountType == ItemDefinition.AmountType.Genetics || list[0].info.amountType == ItemDefinition.AmountType.NucleusGrades) && list[0].instanceData != null)
				{
					instanceData = list[0].instanceData.dataInt;
					sellOrder.inStock = list[0].amount;
				}
				if (list[0].contents != null && list[0].contents.capacity > 0 && list[0].contents.HasFlag(ItemContainer.Flag.ShowSlotsOnIcon))
				{
					foreach (Item item in list[0].contents.itemList)
					{
						list2.Add(item.info.itemid);
					}
					totalAttachmentSlots = list[0].contents.capacity;
				}
				if (list[0].GetHeldEntity() is BaseProjectile { primaryMagazine: not null } baseProjectile)
				{
					ammoCount = baseProjectile.primaryMagazine.contents;
					if ((Object)(object)baseProjectile.primaryMagazine.ammoType != (Object)null)
					{
						ammoType = baseProjectile.primaryMagazine.ammoType.itemid;
					}
				}
			}
			sellOrder.ammoType = ammoType;
			sellOrder.ammoCount = ammoCount;
			sellOrder.itemCondition = itemCondition;
			sellOrder.itemConditionMax = itemConditionMax;
			sellOrder.instanceData = instanceData;
			if (sellOrder.attachmentsList != null)
			{
				Pool.FreeUnmanaged<int>(ref sellOrder.attachmentsList);
			}
			sellOrder.attachmentsList = list2;
			sellOrder.totalAttachmentSlots = totalAttachmentSlots;
			sellOrder.priceMultiplier = GetDiscountForSlot(num, sellOrder);
			sellOrder.receivedQuantityMultiplier = GetReceivedQuantityMultiplier(num, sellOrder);
			num++;
			Pool.Free<Item>(ref list, false);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, OutOfStock());
	}

	protected virtual float GetDiscountForSlot(int sellOrderSlot, SellOrder forOrder)
	{
		return 1f;
	}

	protected virtual float GetReceivedQuantityMultiplier(int sellOrderSlot, SellOrder forOrder)
	{
		return 1f;
	}

	public bool OutOfStock()
	{
		foreach (SellOrder sellOrder in sellOrders.sellOrders)
		{
			if (sellOrder.inStock > 0)
			{
				return true;
			}
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		RefreshSellOrderStockLevel();
		UpdateMapMarker();
	}

	public void UpdateEmptyFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, base.inventory.itemList.Count == 0);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		UpdateEmptyFlag();
		if ((Object)(object)vend_Player != (Object)null && (Object)(object)vend_Player == (Object)(object)player)
		{
			ClearPendingOrder();
		}
		purchasingPlayers.Remove(player);
	}

	public virtual void InstallDefaultSellOrders()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		sellOrders = new SellOrderContainer();
		sellOrders.ShouldPool = false;
		sellOrders.sellOrders = new List<SellOrder>();
	}

	public virtual bool HasVendingSounds()
	{
		return true;
	}

	public virtual float GetBuyDuration()
	{
		return 2.5f;
	}

	public void SetPendingOrder(BasePlayer buyer, int sellOrderId, int numberOfTransactions)
	{
		ClearPendingOrder();
		PendingItemId = sellOrders.sellOrders[sellOrderId].itemToSellID;
		vend_Player = buyer;
		vend_sellOrderID = sellOrderId;
		vend_numberOfTransactions = numberOfTransactions;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
		if (HasVendingSounds())
		{
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartVendingSounds"), sellOrderId);
		}
	}

	public void ClearPendingOrder()
	{
		CancelInvoke(CompletePendingOrder);
		vend_Player = null;
		vend_sellOrderID = -1;
		vend_numberOfTransactions = -1;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		PendingItemId = 0;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_CancelVendingSounds"));
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(9f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void BuyItemRentableShop(RPCMessage rpc)
	{
		if (GetParentEntity() is RentableShop)
		{
			BuyItem(rpc);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void BuyItem(RPCMessage rpc)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!OccupiedCheck(rpc.player))
		{
			return;
		}
		int num = rpc.read.Int32();
		int num2 = rpc.read.Int32();
		if (IsVending())
		{
			rpc.player.ShowToast(GameTip.Styles.Red_Normal, WaitForVendingMessage, false);
		}
		else
		{
			if (Interface.CallHook("OnBuyVendingItem", this, rpc.player, num, num2) != null)
			{
				return;
			}
			int num3 = 0;
			for (int i = 0; i < sellOrders.sellOrders.Count; i++)
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellOrders.sellOrders[i].itemToSellID);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(sellOrders.sellOrders[i].currencyID);
				if (itemDefinition.IsAllowed(CurrentEraRestriction) && itemDefinition2.IsAllowed(CurrentEraRestriction))
				{
					if (num3 == num)
					{
						num = i;
						break;
					}
					num3++;
				}
			}
			SetPendingOrder(rpc.player, num, num2);
			Invoke(CompletePendingOrder, GetBuyDuration());
		}
	}

	public virtual void CompletePendingOrder()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		DoTransaction(vend_Player, vend_sellOrderID, vend_numberOfTransactions);
		ClearPendingOrder();
		Decay.RadialDecayTouch(((Component)this).transform.position, 40f, 2097408);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void TransactionStart(RPCMessage rpc)
	{
	}

	private void GetItemsToSell(SellOrder sellOrder, List<Item> items)
	{
		if (sellOrder.itemToSellIsBP)
		{
			foreach (Item item in base.inventory.itemList)
			{
				if (item.info.itemid == blueprintBaseDef.itemid && item.blueprintTarget == sellOrder.itemToSellID)
				{
					items.Add(item);
				}
			}
			return;
		}
		foreach (Item item2 in base.inventory.itemList)
		{
			if (item2.info.itemid == sellOrder.itemToSellID && (sellOrder.sellSkinId == 0L || item2.skin == sellOrder.sellSkinId))
			{
				items.Add(item2);
			}
		}
	}

	public int? GetSlotsRequiredForTransaction(int sellOrderId, int numberOfTransactions)
	{
		SellOrder val = sellOrders.sellOrders[sellOrderId];
		List<Item> items = Pool.Get<List<Item>>();
		GetItemsToSell(val, items);
		int num = val.itemToSellAmount * numberOfTransactions;
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(val.itemToSellID);
		if (!Object.op_Implicit((Object)(object)itemDefinition))
		{
			return null;
		}
		if (itemDefinition.stackable <= 1)
		{
			return num;
		}
		return num / itemDefinition.stackable;
	}

	public bool DoTransaction(BasePlayer buyer, int sellOrderId, int numberOfTransactions = 1, ItemContainer targetContainer = null, Action<BasePlayer, Item> onCurrencyRemoved = null, Action<BasePlayer, Item> onItemPurchased = null, MarketTerminal droneMarketTerminal = null)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		if (sellOrderId < 0 || sellOrderId >= sellOrders.sellOrders.Count)
		{
			return false;
		}
		if (targetContainer == null && Vector3.Distance(((Component)buyer).transform.position, ((Component)this).transform.position) > 4f)
		{
			return false;
		}
		object obj = Interface.CallHook("OnVendingTransaction", this, buyer, sellOrderId, numberOfTransactions, targetContainer);
		if (obj is bool)
		{
			return (bool)obj;
		}
		SellOrder val = sellOrders.sellOrders[sellOrderId];
		List<Item> list = Pool.Get<List<Item>>();
		GetItemsToSell(val, list);
		if (list == null || list.Count == 0)
		{
			Pool.FreeUnmanaged<Item>(ref list);
			return false;
		}
		numberOfTransactions = Mathf.Clamp(numberOfTransactions, 1, list[0].hasCondition ? 1 : 1000000);
		int num = val.itemToSellAmount * numberOfTransactions;
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(val.itemToSellID);
		ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(val.currencyID);
		if (!itemDefinition.IsAllowed(CurrentEraRestriction) || !itemDefinition2.IsAllowed(CurrentEraRestriction))
		{
			return false;
		}
		if ((Object)(object)itemDefinition == (Object)(object)NPCVendingMachine.ScrapItem && val.receivedQuantityMultiplier != 1f)
		{
			num = GetTotalReceivedMerchandiseForOrder(val.itemToSellAmount, val.receivedQuantityMultiplier) * numberOfTransactions;
		}
		int num2 = list.Sum((Item x) => x.amount);
		if (num > num2)
		{
			Pool.FreeUnmanaged<Item>(ref list);
			return false;
		}
		int num3 = 0;
		PooledList<Item> val2 = Pool.Get<PooledList<Item>>();
		try
		{
			PooledList<Item> val3 = Pool.Get<PooledList<Item>>();
			try
			{
				buyer.inventory.FindItemsByItemID((List<Item>)(object)val3, val.currencyIsBP ? blueprintBaseDef.itemid : val.currencyID);
				for (int num4 = 0; num4 < ((List<Item>)(object)val3).Count; num4++)
				{
					Item item = ((List<Item>)(object)val3)[num4];
					if ((!val.currencyIsBP || item.blueprintTarget == val.currencyID) && (val.costSkinId == 0L || item.skin == val.costSkinId) && (!item.hasCondition || (item.conditionNormalized >= 0.5f && item.maxConditionNormalized > 0.5f)) && item.GetItemVolume() <= maxCurrencyVolume)
					{
						((List<Item>)(object)val2).Add(item);
						num3 += item.amount;
					}
				}
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			if (((List<Item>)(object)val2).Count == 0)
			{
				Pool.FreeUnmanaged<Item>(ref list);
				return false;
			}
			int num5 = GetTotalPriceForOrder(val) * numberOfTransactions;
			if (num3 < num5)
			{
				Pool.FreeUnmanaged<Item>(ref list);
				return false;
			}
			transactionActive = true;
			int num6 = 0;
			for (int num7 = 0; num7 < ((List<Item>)(object)val2).Count; num7++)
			{
				Item item2 = ((List<Item>)(object)val2)[num7];
				int num8 = Mathf.Min(num5 - num6, item2.amount);
				Item item3 = ((item2.amount > num8) ? item2.SplitItem(num8) : item2);
				TakeCurrencyItem(item3);
				onCurrencyRemoved?.Invoke(buyer, item3);
				num6 += num8;
				if (num6 >= num5)
				{
					break;
				}
			}
			int num9 = 0;
			foreach (Item item5 in list)
			{
				int num10 = num - num9;
				Item item4 = ((item5.amount > num10) ? item5.SplitItem(num10) : item5);
				if (item4 == null)
				{
					Debug.LogError((object)"Vending machine error, contact developers!");
				}
				else
				{
					num9 += item4.amount;
					object obj2 = Interface.CallHook("CanPurchaseItem", buyer, item4, onItemPurchased, this, targetContainer);
					if (obj2 != null)
					{
						if (!(obj2 is bool))
						{
							return false;
						}
						return (bool)obj2;
					}
					RecordSaleAnalytics(item4, sellOrderId, val.currencyAmountPerItem);
					if (targetContainer == null)
					{
						GiveSoldItem(item4, buyer);
					}
					else if (!item4.MoveToContainer(targetContainer))
					{
						item4.Drop(targetContainer.dropPosition, targetContainer.dropVelocity);
					}
					if (ShouldRecordStats)
					{
						RegisterCustomer(buyer.userID);
					}
					onItemPurchased?.Invoke(buyer, item4);
				}
				if (num9 >= num)
				{
					break;
				}
			}
			Facepunch.Rust.Analytics.Azure.OnBuyFromVendingMachine(buyer, this, val.itemToSellID, num, val.itemToSellIsBP, val.currencyID, num6, val.currencyIsBP, numberOfTransactions, val.priceMultiplier, droneMarketTerminal);
			if (ShouldRecordStats)
			{
				AddPurchaseHistory(val.itemToSellID, num, val.currencyID, num6, val.itemToSellIsBP, val.currencyIsBP);
			}
			Pool.FreeUnmanaged<Item>(ref list);
			UpdateEmptyFlag();
			transactionActive = false;
			return true;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	protected virtual void RecordSaleAnalytics(Item itemSold, int orderId, int currencyUsed)
	{
	}

	public virtual void TakeCurrencyItem(Item takenCurrencyItem)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnTakeCurrencyItem", this, takenCurrencyItem) == null && !takenCurrencyItem.MoveToContainer(base.inventory))
		{
			takenCurrencyItem.Drop(base.inventory.dropPosition, Vector3.zero);
		}
	}

	public virtual void GiveSoldItem(Item soldItem, BasePlayer buyer)
	{
		if (Interface.CallHook("OnGiveSoldItem", this, soldItem, buyer) == null)
		{
			while (soldItem.amount > soldItem.MaxStackable())
			{
				Item item = soldItem.SplitItem(soldItem.MaxStackable());
				buyer.GiveItem(item, GiveItemReason.PickedUp);
			}
			buyer.GiveItem(soldItem, GiveItemReason.PickedUp);
		}
	}

	public void SendSellOrders(BasePlayer player = null)
	{
		if (Object.op_Implicit((Object)(object)player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_ReceiveSellOrders", player), sellOrders);
		}
		else
		{
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_ReceiveSellOrders"), sellOrders);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public virtual void RPC_Broadcast(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		bool b = msg.read.Bit();
		if (CanPlayerAdmin(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved4, b);
			}
			Interface.CallHook("OnToggleVendingBroadcast", this, player);
			UpdateMapMarker();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_SetSkinMode(RPCMessage msg)
	{
		if (CanPlayerAdmin(msg.player))
		{
			bool b = msg.read.Bit();
			SetFlagLocal(Flags.Reserved9, b);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public virtual void RPC_UpdateShopName(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		string obj = msg.read.String(32);
		if (CanPlayerAdmin(player))
		{
			if (Interface.CallHook("OnVendingShopRename", this, obj, player) != null)
			{
				return;
			}
			shopName = obj;
			nameLastEditedBy = player.userID.Get();
			UpdateMapMarker();
		}
		SendNetworkUpdate();
	}

	public void UpdateMapMarkerPosition()
	{
		if (!((Object)(object)myMarker == (Object)null))
		{
			myMarker.TryUpdatePosition();
		}
	}

	public void UpdateMapMarker(bool updatePosition = false)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!mapMarkerPrefab.isValid)
		{
			return;
		}
		if (IsBroadcasting() && !IsInUnopenedDeepSea())
		{
			bool flag = false;
			if ((Object)(object)myMarker == (Object)null)
			{
				myMarker = GameManager.server.CreateEntity(mapMarkerPrefab.resourcePath, ((Component)this).transform.position, Quaternion.identity) as VendingMachineMapMarker;
				flag = true;
			}
			using (FlagsUpdateScope flagsUpdateScope = myMarker.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, OutOfStock());
			}
			myMarker.SetVendingMachine(this, shopName);
			if (flag)
			{
				myMarker.Spawn();
			}
			else
			{
				myMarker.SendNetworkUpdate();
			}
		}
		else if (Object.op_Implicit((Object)(object)myMarker))
		{
			myMarker.Kill();
			myMarker = null;
		}
	}

	private bool IsInUnopenedDeepSea()
	{
		if (!IsInDeepSeaCached)
		{
			return false;
		}
		DeepSeaManager deepSeaManager = DeepSeaManager.Get(server: true);
		if ((Object)(object)deepSeaManager != (Object)null)
		{
			return !deepSeaManager.IsOpen();
		}
		return false;
	}

	public void OpenShop(BasePlayer ply, string panelName)
	{
		SendSellOrders(ply);
		PlayerOpenLoot(ply, panelName);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenShopNoLOS(RPCMessage msg)
	{
		if (OccupiedCheck(msg.player))
		{
			OpenShop(msg.player, customerPanel);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenShop(RPCMessage msg)
	{
		if (OccupiedCheck(msg.player) && Interface.CallHook("OnVendingShopOpen", this, msg.player) == null)
		{
			OpenShop(msg.player, customerPanel);
			Interface.CallHook("OnVendingShopOpened", this, msg.player);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenAdmin(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPlayerAdmin(player))
		{
			OpenShop(player, "notCustomerPanel");
			ClientRPC(RpcTarget.Player("CLIENT_OpenAdminMenu", player));
			Interface.CallHook("OnOpenVendingAdmin", this, player);
		}
	}

	public void OnIndustrialItemTransferBegins()
	{
		industrialItemIncoming = true;
	}

	public void OnIndustrialItemTransferEnd()
	{
		industrialItemIncoming = false;
	}

	public bool CanAcceptItem(Item item, int targetSlot)
	{
		object obj = Interface.CallHook("CanVendingAcceptItem", this, item, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		BasePlayer basePlayer = item.GetRootContainer()?.GetOwnerPlayer();
		if (transactionActive || industrialItemIncoming)
		{
			return true;
		}
		if (purchasingPlayers.Contains(basePlayer))
		{
			return false;
		}
		if (item.parent == null)
		{
			return true;
		}
		if (base.inventory.itemList.Contains(item))
		{
			return true;
		}
		if ((Object)(object)basePlayer == (Object)null)
		{
			return item.GetEntityOwner() is ContainerCorpse;
		}
		return CanPlayerAdmin(basePlayer);
	}

	public virtual PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		return new PlayerInventory.CanMoveFromResponse(CanPlayerAdmin(player), NotAdministratingError);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (panelToOpen == customerPanel)
		{
			purchasingPlayers.Add(player);
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		object obj = Interface.CallHook("CanUseVending", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (panelName == customerPanel)
		{
			return true;
		}
		if (base.CanOpenLootPanel(player, panelName))
		{
			return CanPlayerAdmin(player);
		}
		return false;
	}

	public override Vector3 GetDropPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.TransformPoint(localDropPosition);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_DeleteSellOrder(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPlayerAdmin(player))
		{
			int num = msg.read.Int32();
			Interface.CallHook("OnDeleteVendingOffer", this, num);
			if (num >= 0 && num < sellOrders.sellOrders.Count)
			{
				SellOrder val = sellOrders.sellOrders[num];
				Facepunch.Rust.Analytics.Azure.OnVendingMachineOrderChanged(msg.player, this, val.itemToSellID, val.itemToSellAmount, val.itemToSellIsBP, val.currencyID, val.currencyAmountPerItem, val.currencyIsBP, added: false);
				sellOrders.sellOrders.RemoveAt(num);
			}
			ClearPendingOrder();
			RefreshSellOrderStockLevel();
			UpdateMapMarker();
			SendSellOrders(player);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_DeleteAllSellOrders(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerAdmin(player))
		{
			return;
		}
		foreach (SellOrder sellOrder in sellOrders.sellOrders)
		{
			Facepunch.Rust.Analytics.Azure.OnVendingMachineOrderChanged(msg.player, this, sellOrder.itemToSellID, sellOrder.itemToSellAmount, sellOrder.itemToSellIsBP, sellOrder.currencyID, sellOrder.currencyAmountPerItem, sellOrder.currencyIsBP, added: false);
		}
		sellOrders.sellOrders.Clear();
		ClearPendingOrder();
		RefreshSellOrderStockLevel();
		UpdateMapMarker();
		SendSellOrders(player);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_MoveSellOrder(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanPlayerAdmin(player))
		{
			int num = msg.read.Int32();
			int num2 = msg.read.Int32();
			if (num >= 0 && num < sellOrders.sellOrders.Count && num2 >= 0 && num2 < sellOrders.sellOrders.Count && num != num2)
			{
				SellOrder item = sellOrders.sellOrders[num];
				sellOrders.sellOrders.RemoveAt(num);
				sellOrders.sellOrders.Insert(num2, item);
				ClearPendingOrder();
				SendSellOrders(player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_RotateVM(RPCMessage msg)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnRotateVendingMachine", this, msg.player) == null && CanRotate())
		{
			UpdateEmptyFlag();
			if (msg.player.CanBuild() && IsInventoryEmpty())
			{
				((Component)this).transform.rotation = Quaternion.LookRotation(-((Component)this).transform.forward, ((Component)this).transform.up);
				SendNetworkUpdate();
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_AddSellOrder(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!CanPlayerAdmin(player))
		{
			return;
		}
		if (sellOrders.sellOrders.Count >= 7)
		{
			player.ShowToast(GameTip.Styles.Error, TooManySellOrders, true);
			return;
		}
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		int num3 = msg.read.Int32();
		int num4 = msg.read.Int32();
		byte b = msg.read.UInt8();
		ulong sellSkinId = msg.read.UInt64();
		ulong costSkinId = msg.read.UInt64();
		if (!HasFlag(Flags.Reserved9))
		{
			sellSkinId = 0uL;
			costSkinId = 0uL;
		}
		AddSellOrder(num, num2, num3, num4, b, sellSkinId, costSkinId);
		Facepunch.Rust.Analytics.Azure.OnVendingMachineOrderChanged(msg.player, this, num, num2, b == 2 || b == 3, num3, num4, b == 1 || b == 3, added: true);
	}

	public void AddSellOrder(int itemToSellID, int itemToSellAmount, int currencyToUseID, int currencyAmount, byte bpState, ulong sellSkinId = 0uL, ulong costSkinId = 0uL)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemToSellID);
		ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(currencyToUseID);
		if (!((Object)(object)itemDefinition == (Object)null) && !((Object)(object)itemDefinition2 == (Object)null))
		{
			currencyAmount = Mathf.Clamp(currencyAmount, 1, 10000);
			itemToSellAmount = Mathf.Clamp(itemToSellAmount, 1, itemDefinition.stackable);
			SellOrder val = new SellOrder();
			val.ShouldPool = false;
			val.itemToSellID = itemToSellID;
			val.itemToSellAmount = itemToSellAmount;
			val.currencyID = currencyToUseID;
			val.currencyAmountPerItem = currencyAmount;
			val.currencyIsBP = bpState == 3 || bpState == 2;
			val.itemToSellIsBP = bpState == 3 || bpState == 1;
			val.sellSkinId = sellSkinId;
			val.costSkinId = costSkinId;
			Interface.CallHook("OnAddVendingOffer", this, val);
			sellOrders.sellOrders.Add(val);
			ClearPendingOrder();
			RefreshSellOrderStockLevel(itemDefinition);
			UpdateMapMarker();
			SendNetworkUpdate();
		}
	}

	public void RefreshAndSendNetworkUpdate()
	{
		RefreshSellOrderStockLevel();
		SendNetworkUpdate();
	}

	public void UpdateOrCreateSalesSheet()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("note");
		PooledList<Item> val = Pool.Get<PooledList<Item>>();
		try
		{
			base.inventory.FindItemsByItemID((List<Item>)(object)val, itemDefinition.itemid);
			Item item = null;
			foreach (Item item4 in (List<Item>)(object)val)
			{
				if (item4.text.Length == 0)
				{
					item = item4;
					break;
				}
			}
			if (item == null)
			{
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition("paper");
				Item item2 = base.inventory.FindItemByItemID(itemDefinition2.itemid);
				if (item2 != null)
				{
					item = ItemManager.CreateByItemID(itemDefinition.itemid, 1, 0uL, 0uL);
					if (!item.MoveToContainer(base.inventory))
					{
						item.Drop(GetDropPosition(), GetDropVelocity());
					}
					item2.UseItem();
				}
			}
			if (item == null)
			{
				return;
			}
			foreach (SellOrder sellOrder in sellOrders.sellOrders)
			{
				ItemDefinition itemDefinition3 = ItemManager.FindItemDefinition(sellOrder.itemToSellID);
				Item item3 = item;
				item3.text = item3.text + itemDefinition3.displayName.translated + "\n";
			}
			item.MarkDirty();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void ClearContent()
	{
		if (!(this is NPCVendingMachine))
		{
			shopName = "A Shop";
			nameLastEditedBy = 0uL;
			SendNetworkUpdate();
			UpdateMapMarker();
		}
	}

	protected virtual bool CanShop(BasePlayer bp)
	{
		return true;
	}

	protected virtual bool CanRotate()
	{
		IndustrialStorageAdaptor adaptor;
		if (!HasAttachedStorageMonitor())
		{
			return !HasAttachedStorageAdaptor(out adaptor);
		}
		return false;
	}

	public bool IsBroadcasting()
	{
		return HasFlag(Flags.Reserved4);
	}

	public bool IsInventoryEmpty()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool IsVending()
	{
		return HasFlag(Flags.Reserved2);
	}

	public virtual bool PlayerBehind(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		return Vector3.Dot(forward, ((Vector3)(ref val)).normalized) <= -0.7f;
	}

	public bool PlayerInfront(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		return Vector3.Dot(forward, ((Vector3)(ref val)).normalized) >= 0.7f;
	}

	public virtual bool CanPlayerAdmin(BasePlayer player)
	{
		object obj = Interface.CallHook("CanAdministerVending", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (PlayerBehind(player))
		{
			return OccupiedCheck(player);
		}
		return false;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public virtual string GetTranslationToken()
	{
		return "";
	}

	public bool CanSellOrBuyItem(int itemToSellID, int currencyID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemToSellID);
		if (ItemManager.FindItemDefinition(currencyID).IsAllowed(CurrentEraRestriction))
		{
			return itemDefinition.IsAllowed(CurrentEraRestriction);
		}
		return false;
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: PendingItemId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_PendingItemId);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_PendingItemId;
				int _sync_PendingItemId = reader.Int32();
				__sync_PendingItemId = _sync_PendingItemId;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "PendingItemId")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_PendingItemId = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
