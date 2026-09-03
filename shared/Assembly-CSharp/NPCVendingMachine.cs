using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConVar;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Safety;
using UnityEngine;

public class NPCVendingMachine : VendingMachine
{
	public class SalesData
	{
		public ulong TotalSales;

		public ulong TotalIntervals;

		public ulong SoldThisInterval;

		public float CurrentMultiplier;

		public bool IsForReceivedCurrency;

		public double GetAverageSalesPerInterval()
		{
			if (TotalSales == 0L || TotalIntervals == 0L)
			{
				return 0.0;
			}
			return (double)TotalSales / (double)TotalIntervals;
		}

		public void RecordSale(int count)
		{
			SoldThisInterval += (ulong)count;
		}

		public void ProcessEndOfInterval()
		{
			double averageSalesPerInterval = GetAverageSalesPerInterval();
			bool flag = TotalIntervals == 0;
			TotalSales += SoldThisInterval;
			TotalIntervals++;
			SoldThisInterval = 0uL;
			float num = 0f;
			num = ((!((GetAverageSalesPerInterval() <= averageSalesPerInterval) | flag)) ? PriceIncreaseAmount : (0f - PriceDecreaseAmount));
			if (IsForReceivedCurrency)
			{
				CurrentMultiplier -= num;
			}
			else
			{
				CurrentMultiplier += num;
			}
			CurrentMultiplier = Mathf.Clamp(CurrentMultiplier, MinimumPriceMultiplier, MaximumPriceMultiplier);
		}
	}

	[ServerVar(Saved = true, Help = "Whether to run the the dynamic pricing system")]
	public static bool DynamicPricingEnabled = true;

	[ServerVar(Saved = true, Help = "How many realtime hours are checked when looking for price increases. Max 72 (10 days), min 0.5 (half an hour)", ShowInAdminUI = true)]
	public static float PriceUpdateFrequencyDefault = 3f;

	[ServerVar(Saved = true, Help = "How many realtime hours are checked when looking for price increases. Max 72 (10 days), min 0.5 (half an hour)", ShowInAdminUI = true)]
	public static float PriceUpdateFrequencyBiWeekly = 2f;

	[ServerVar(Saved = true, Help = "How many realtime hours are checked when looking for price increases. Max 72 (10 days), min 0.5 (half an hour)", ShowInAdminUI = true)]
	public static float PriceUpdateFrequencyWeekly = 1f;

	private static bool hasCachedTags = false;

	private static bool cachedBiWeekly;

	private static bool cachedWeekly;

	[ServerVar(Saved = true, Help = "The maximum point that a price can increase to (2 = 200%)")]
	public static float MaximumPriceMultiplier = 2f;

	[ServerVar(Saved = true, Help = "The Minimum point that the price can drop to (0.5 = 50% off)")]
	public static float MinimumPriceMultiplier = 0.5f;

	[ServerVar(Saved = true, Help = "What discount surcharge should be applied to items when the server starts")]
	public static float StartingPriceMultiplier = 2f;

	[ServerVar(Saved = true, Help = "How much to increase the price by if it is selling a lot (0.05 = 5%)")]
	public static float PriceIncreaseAmount = 0.1f;

	[ServerVar(Saved = true, Help = "How much to decrease the price for if it is underselling (0.05 = 5%)")]
	public static float PriceDecreaseAmount = 0.05f;

	private SalesData[] allSalesData;

	private float timeToNextSalesUpdate;

	private bool preserveSalesData;

	private static ItemDefinition _scrapItem = null;

	private TimeSince lastHourCheck;

	public NPCVendingOrder vendingOrders;

	public Phrase Phrase;

	public NPCVendingOrder[] alternativeVendingOrders;

	public float RefillTime = 1f;

	public int StartingStock = 10;

	public bool BypassDynamicPricing;

	public const int MaxVendingEntries = 7;

	public const int Capacity = 128;

	private static ListHashSet<NPCVendingMachine> allNpcVendingMachines = new ListHashSet<NPCVendingMachine>();

	private float[] refillTimes;

	private static float ScaledByWipeUpdateFrequency
	{
		get
		{
			if (!hasCachedTags)
			{
				cachedBiWeekly = StringEx.Contains(ConVar.Server.tags, "biweekly", CompareOptions.IgnoreCase);
				cachedWeekly = StringEx.Contains(ConVar.Server.tags, "weekly", CompareOptions.IgnoreCase);
				hasCachedTags = true;
			}
			if (cachedBiWeekly)
			{
				return PriceUpdateFrequencyBiWeekly;
			}
			if (cachedWeekly)
			{
				return PriceUpdateFrequencyWeekly;
			}
			return PriceUpdateFrequencyDefault;
		}
	}

	public static float IntervalSeconds => Mathf.Clamp(ScaledByWipeUpdateFrequency, 0.5f, 72f) * 60f * 60f;

	public static ItemDefinition ScrapItem
	{
		get
		{
			if ((Object)(object)_scrapItem == (Object)null)
			{
				_scrapItem = ItemManager.FindItemDefinition("scrap");
			}
			return _scrapItem;
		}
	}

	private bool CanApplyDynamicPricing
	{
		get
		{
			if (!BypassDynamicPricing)
			{
				return DynamicPricingEnabled;
			}
			return false;
		}
	}

	public override EraRestriction CurrentEraRestriction => (EraRestriction)1;

	protected virtual bool BlockOrderRefreshOnLoad => false;

	public override bool ShouldRecordStats => false;

	[ServerVar]
	public static void ResetFrequencyTags(ConsoleSystem.Arg arg)
	{
		hasCachedTags = false;
		arg.ReplyWith($"Reset frequency tags. Scaled frequency is now:{ScaledByWipeUpdateFrequency} hours");
	}

	[ServerVar(Help = "Resets the state of all discounts and surcharges from NPC vending machines")]
	public static void resetDynamicPricing()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<NPCVendingMachine> enumerator = allNpcVendingMachines.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.ResetDynamicPricing();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Print out all current price changes on the server")]
	public static void printAllPriceChanges(ConsoleSystem.Arg arg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		Enumerator<NPCVendingMachine> enumerator = allNpcVendingMachines.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				NPCVendingMachine current = enumerator.Current;
				TextTable val = Pool.Get<TextTable>();
				try
				{
					val.AddColumns(new string[8] { "Item Name", "Original Price", "Discount/Surcharge", "Final Price", "Avg Sales/Interval", "Current Sales/Interval", "Total Sales", "Intervals" });
					int num = 0;
					int num2 = 0;
					foreach (SellOrder sellOrder in current.sellOrders.sellOrders)
					{
						if (sellOrder.priceMultiplier != 1f)
						{
							num++;
							ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellOrder.itemToSellID);
							int totalPriceForOrder = VendingMachine.GetTotalPriceForOrder(sellOrder);
							SalesData salesData = current.allSalesData[num2];
							val.AddRow(new string[8]
							{
								itemDefinition.shortname,
								sellOrder.currencyAmountPerItem.ToString(),
								$"{Mathf.RoundToInt(sellOrder.priceMultiplier * 100f)}%",
								$"{totalPriceForOrder}",
								salesData.GetAverageSalesPerInterval().ToString(),
								salesData.SoldThisInterval.ToString(),
								salesData.TotalSales.ToString(),
								salesData.TotalIntervals.ToString()
							});
						}
						num2++;
					}
					if (num > 0)
					{
						stringBuilder.AppendLine(current.shopName);
						stringBuilder.AppendLine("==============");
						stringBuilder.AppendLine(((object)val).ToString());
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Simulates the provided number of hours passing in the vending machine system")]
	public static void addHours(ConsoleSystem.Arg arg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)arg.GetInt(0) * 60f * 60f;
		Enumerator<NPCVendingMachine> enumerator = allNpcVendingMachines.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				NPCVendingMachine current = enumerator.Current;
				current.lastHourCheck = TimeSince.op_Implicit(TimeSince.op_Implicit(current.lastHourCheck) + num);
				current.HourCheck();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public void RecordSale(int index, int countReceived, int currencyUsed)
	{
		if (CanApplyDynamicPricing)
		{
			CheckSalesDataLength();
			SalesData obj = allSalesData[index];
			obj.RecordSale(obj.IsForReceivedCurrency ? currencyUsed : countReceived);
		}
	}

	private void CheckSalesDataLength(bool reset = false)
	{
		if (reset)
		{
			allSalesData = null;
		}
		int count = sellOrders.sellOrders.Count;
		if (allSalesData == null || allSalesData.Length != count)
		{
			allSalesData = new SalesData[count];
			for (int i = 0; i < count; i++)
			{
				bool flag = sellOrders.sellOrders[i].itemToSellID == ScrapItem.itemid;
				allSalesData[i] = new SalesData
				{
					IsForReceivedCurrency = flag,
					CurrentMultiplier = (flag ? MinimumPriceMultiplier : StartingPriceMultiplier)
				};
			}
		}
	}

	protected override float GetDiscountForSlot(int sellOrderSlot, SellOrder forOrder)
	{
		if (!CanApplyDynamicPricing)
		{
			return 1f;
		}
		if (!preserveSalesData)
		{
			CheckSalesDataLength();
		}
		if (sellOrderSlot < 0 || sellOrderSlot >= allSalesData.Length)
		{
			return 1f;
		}
		if (forOrder.currencyID != ScrapItem.itemid)
		{
			return 1f;
		}
		return allSalesData[sellOrderSlot].CurrentMultiplier;
	}

	protected override float GetReceivedQuantityMultiplier(int sellOrderSlot, SellOrder forOrder)
	{
		if (!CanApplyDynamicPricing)
		{
			return 1f;
		}
		if (!preserveSalesData)
		{
			CheckSalesDataLength();
		}
		if (sellOrderSlot < 0 || sellOrderSlot >= allSalesData.Length)
		{
			return 1f;
		}
		if (forOrder.itemToSellID != ScrapItem.itemid)
		{
			return 1f;
		}
		return allSalesData[sellOrderSlot].CurrentMultiplier;
	}

	private void DynamicPricingServerInit()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		timeToNextSalesUpdate = IntervalSeconds;
		lastHourCheck = TimeSince.op_Implicit(0f);
		if (DeepSeaManager.IsInsideDeepSea((BaseNetworkable)this))
		{
			BypassDynamicPricing = true;
		}
		InvokeRandomized(HourCheck, 1f, 15f, 0.1f);
	}

	private void ResetDynamicPricing()
	{
		CheckSalesDataLength(reset: true);
		timeToNextSalesUpdate = IntervalSeconds;
		HourCheck();
	}

	private void HourCheck()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!CanApplyDynamicPricing)
		{
			return;
		}
		float num = TimeSince.op_Implicit(lastHourCheck);
		lastHourCheck = TimeSince.op_Implicit(0f);
		timeToNextSalesUpdate -= num;
		while (timeToNextSalesUpdate < 0f)
		{
			timeToNextSalesUpdate += IntervalSeconds;
			if (allSalesData != null)
			{
				SalesData[] array = allSalesData;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ProcessEndOfInterval();
				}
			}
			UpdateMapMarker();
			RefreshAndSendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (allSalesData != null && info.forDisk)
		{
			info.msg.vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
			info.msg.vendingDynamicPricing.allSalesData = Pool.Get<List<SalesData>>();
			info.msg.vendingDynamicPricing.timeToNextSalesUpdate = timeToNextSalesUpdate;
			SalesData[] array = allSalesData;
			foreach (SalesData salesData in array)
			{
				SalesData val = Pool.Get<SalesData>();
				val.totalSales = salesData.TotalSales;
				val.totalIntervals = salesData.TotalIntervals;
				val.soldThisInterval = salesData.SoldThisInterval;
				val.currentMultiplier = salesData.CurrentMultiplier;
				val.isForReceivedQuantity = salesData.IsForReceivedCurrency;
				info.msg.vendingDynamicPricing.allSalesData.Add(val);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		if (info.msg.vendingDynamicPricing != null)
		{
			allSalesData = new SalesData[info.msg.vendingDynamicPricing.allSalesData.Count];
			int num = 0;
			timeToNextSalesUpdate = info.msg.vendingDynamicPricing.timeToNextSalesUpdate;
			foreach (SalesData allSalesDatum in info.msg.vendingDynamicPricing.allSalesData)
			{
				SalesData salesData = new SalesData
				{
					TotalSales = allSalesDatum.totalSales,
					TotalIntervals = allSalesDatum.totalIntervals,
					SoldThisInterval = allSalesDatum.soldThisInterval,
					CurrentMultiplier = allSalesDatum.currentMultiplier,
					IsForReceivedCurrency = allSalesDatum.isForReceivedQuantity
				};
				allSalesData[num] = salesData;
				num++;
			}
		}
		base.Load(info);
	}

	public byte GetBPState(bool sellItemAsBP, bool currencyItemAsBP)
	{
		byte result = 0;
		if (sellItemAsBP)
		{
			result = 1;
		}
		if (currencyItemAsBP)
		{
			result = 2;
		}
		if (sellItemAsBP & currencyItemAsBP)
		{
			result = 3;
		}
		return result;
	}

	public override void TakeCurrencyItem(Item takenCurrencyItem)
	{
		if (Interface.CallHook("OnTakeCurrencyItem", this, takenCurrencyItem) == null)
		{
			takenCurrencyItem.MoveToContainer(base.inventory);
			takenCurrencyItem.RemoveFromContainer();
			takenCurrencyItem.Remove();
		}
	}

	public override void GiveSoldItem(Item soldItem, BasePlayer buyer)
	{
		if (Interface.CallHook("OnNpcGiveSoldItem", this, soldItem, buyer) == null)
		{
			soldItem.SetItemOwnership(buyer, ItemOwnershipPhrases.VendorSale);
			base.GiveSoldItem(soldItem, buyer);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!BlockOrderRefreshOnLoad)
		{
			Invoke(InstallFromVendingOrders, 1f);
		}
	}

	public void ChangeRefillTime(float newRefillTime)
	{
		RefillTime = newRefillTime;
		if (IsInvoking(Refill))
		{
			CancelInvoke(Refill);
		}
		InvokeRandomized(Refill, 1f, RefillTime, 0.1f);
	}

	public override void ServerInit()
	{
		IsInDeepSeaCached = this.IsInsideDeepSea();
		base.ServerInit();
		skinID = 861142659uL;
		SendNetworkUpdate();
		if (!BlockOrderRefreshOnLoad || !Application.isLoadingSave)
		{
			Invoke(InstallFromVendingOrders, 1f);
		}
		if (!IsInvoking(Refill))
		{
			InvokeRandomized(Refill, 1f, RefillTime, 0.1f);
		}
		DynamicPricingServerInit();
		allNpcVendingMachines.TryAdd(this);
		UpdateDronePrediction();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		allNpcVendingMachines.Remove(this);
	}

	public virtual void InstallFromVendingOrders()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between I4 and Unknown
		if (alternativeVendingOrders != null && alternativeVendingOrders.Length != 0)
		{
			NPCVendingOrder[] array = alternativeVendingOrders;
			foreach (NPCVendingOrder nPCVendingOrder in array)
			{
				if (nPCVendingOrder.AllowedEras == null || nPCVendingOrder.AllowedEras.Length == 0)
				{
					vendingOrders = nPCVendingOrder;
					break;
				}
				Era[] allowedEras = nPCVendingOrder.AllowedEras;
				for (int j = 0; j < allowedEras.Length; j++)
				{
					if ((int)allowedEras[j] == (int)ConVar.Server.Era)
					{
						vendingOrders = nPCVendingOrder;
						break;
					}
				}
			}
		}
		if ((Object)(object)vendingOrders == (Object)null)
		{
			if (!(parentEntity.Get(base.isServer) is RentableShop))
			{
				Debug.LogError((object)"No vending orders!");
			}
			return;
		}
		int count = sellOrders.sellOrders.Count;
		ClearSellOrders();
		base.inventory.Clear();
		ItemManager.DoRemoves();
		if (numSlots == 0)
		{
			base.inventory.capacity = 128;
		}
		if (vendingOrders.orders.Length <= 7)
		{
			if (count == vendingOrders.orders.Length)
			{
				preserveSalesData = true;
			}
			try
			{
				NPCVendingOrder.Entry[] orders = vendingOrders.orders;
				foreach (NPCVendingOrder.Entry ent in orders)
				{
					SmartAddItemForSale(ent);
				}
				return;
			}
			finally
			{
				preserveSalesData = false;
			}
		}
		List<NPCVendingOrder.Entry> list = Pool.Get<List<NPCVendingOrder.Entry>>();
		vendingOrders.GetRandomEntries(7, list);
		foreach (NPCVendingOrder.Entry item in list)
		{
			SmartAddItemForSale(item);
		}
		Pool.FreeUnmanaged<NPCVendingOrder.Entry>(ref list);
	}

	private void SmartAddItemForSale(NPCVendingOrder.Entry ent)
	{
		int currencyPerTransaction = ent.currencyAmount;
		if (ent.randomDetails.useRandom)
		{
			currencyPerTransaction = ent.randomDetails.GetRandomPrice();
		}
		AddItemForSale(ent.sellItem.itemid, ent.sellItemAmount, ent.currencyItem.itemid, currencyPerTransaction, GetBPState(ent.sellItemAsBP, ent.currencyAsBP), ent.maxStock);
	}

	private int GetMaxStock(int orderMaxStock)
	{
		if (orderMaxStock < 0)
		{
			return StartingStock;
		}
		return orderMaxStock;
	}

	public override void InstallDefaultSellOrders()
	{
		base.InstallDefaultSellOrders();
	}

	public void Refill()
	{
		if ((Object)(object)vendingOrders == (Object)null || vendingOrders.orders == null || base.inventory == null)
		{
			return;
		}
		if (refillTimes == null)
		{
			refillTimes = new float[vendingOrders.orders.Length];
		}
		for (int i = 0; i < vendingOrders.orders.Length; i++)
		{
			NPCVendingOrder.Entry entry = vendingOrders.orders[i];
			if (!(Time.realtimeSinceStartup > refillTimes[i]))
			{
				continue;
			}
			int num = 0;
			num = ((!entry.sellItemAsBP) ? Mathf.FloorToInt((float)(base.inventory.GetAmount(entry.sellItem.itemid) / entry.sellItemAmount)) : Mathf.FloorToInt((float)(base.inventory.GetAmount(base.blueprintBaseDef.itemid, entry.sellItem.itemid) / entry.sellItemAmount)));
			int num2 = Mathf.Min(GetMaxStock(entry.maxStock) - num, entry.refillAmount) * entry.sellItemAmount;
			if (num2 > 0)
			{
				transactionActive = true;
				Item item = null;
				if (entry.sellItemAsBP)
				{
					item = ItemManager.Create(base.blueprintBaseDef, num2, 0uL, isServerSide: true, 0uL);
					item.blueprintTarget = entry.sellItem.itemid;
				}
				else
				{
					item = ItemManager.Create(entry.sellItem, num2, 0uL, isServerSide: true, 0uL);
				}
				if (!item.MoveToContainer(base.inventory))
				{
					item.Remove();
				}
				transactionActive = false;
			}
			refillTimes[i] = Time.realtimeSinceStartup + entry.refillDelay;
		}
	}

	public void ClearSellOrders()
	{
		sellOrders.sellOrders.Clear();
	}

	public void AddItemForSale(int itemID, int amountToSell, int currencyID, int currencyPerTransaction, byte bpState, int maxStock)
	{
		AddSellOrder(itemID, amountToSell, currencyID, currencyPerTransaction, bpState, 0uL, 0uL);
		transactionActive = true;
		int maxStock2 = GetMaxStock(maxStock);
		if (bpState == 1 || bpState == 3)
		{
			for (int i = 0; i < maxStock2; i++)
			{
				Item item = ItemManager.CreateByItemID(base.blueprintBaseDef.itemid, 1, 0uL, 0uL);
				item.blueprintTarget = itemID;
				base.inventory.Insert(item);
			}
		}
		else
		{
			base.inventory.AddItem(ItemManager.FindItemDefinition(itemID), amountToSell * maxStock2, 0uL);
		}
		transactionActive = false;
		RefreshSellOrderStockLevel();
	}

	public void RefreshStock()
	{
	}

	protected override void RecordSaleAnalytics(Item itemSold, int orderId, int currencyUsed)
	{
		RecordSale(orderId, itemSold.amount, currencyUsed);
	}

	public static void RefreshDeepSeaMapMarkers()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<NPCVendingMachine> enumerator = allNpcVendingMachines.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				NPCVendingMachine current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsInDeepSeaCached)
				{
					current.UpdateMapMarker();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override string GetTranslationToken()
	{
		return Phrase.token;
	}

	protected override bool CanRotate()
	{
		return false;
	}

	public override bool CanPlayerAdmin(BasePlayer player)
	{
		object obj = Interface.CallHook("CanAdministerVending", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}
}
