using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Spatial;
using UnityEngine;

public class BuriedItems : PointEntity
{
	[ServerVar(Help = "Time in seconds before an item expires.")]
	public static float expiryTime = 86400f;

	private const int CellSize = 128;

	private const float WorldSize = 8096f;

	private const float QuerySize = 64f;

	[ServerVar]
	public static int maxBuriedItems = 32;

	[ServerVar(Help = "Metal detector loot weight is 100.")]
	public static int buriedItemWeight = 100;

	[ServerVar(Help = "[0.0 to 1.0]")]
	public static float buryItemChance = 0.85f;

	private Grid<BuriedItem> grid = new Grid<BuriedItem>(128, 8096f);

	private readonly SortedList<long, BuriedItem> itemExpiryTracking = new SortedList<long, BuriedItem>(128);

	private readonly Dictionary<ulong, BuriedItem> uidItemMapping = new Dictionary<ulong, BuriedItem>(128);

	private static readonly Random Random = new Random();

	private (long lastExpiryTime, long modifiedExpiryTime)? lastExpiryTime;

	public static BuriedItems Instance { get; private set; }

	public override void ServerInit()
	{
		base.ServerInit();
		Clear();
		Instance = this;
	}

	public void Register(Item item, Vector3 worldPosition)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (item != null && ((ItemId)(ref item.uid)).IsValid && item.info.allowBurying && Random.NextDouble() <= (double)buryItemChance && itemExpiryTracking.Count < maxBuriedItems)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(expiryTime);
			long num = DateTimeOffset.UtcNow.Add(timeSpan).ToUnixTimeMilliseconds();
			BuriedItem buriedItem = BuriedItem.Create(item, worldPosition, num);
			if (buriedItem != null)
			{
				Add(buriedItem);
			}
		}
	}

	private void Add(BuriedItem buriedItem)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		HandleDuplicateExpiryTimes(buriedItem);
		if (itemExpiryTracking.TryAdd(buriedItem.ExpiryTime, buriedItem))
		{
			grid.Add(buriedItem, buriedItem.Location.x, buriedItem.Location.y);
			uidItemMapping.Add(buriedItem.UID, buriedItem);
		}
		else
		{
			Debug.LogError((object)$"Failed to add buried item with expiry time {buriedItem.ExpiryTime} to the expiry tracking list, retrying.");
			Add(buriedItem);
		}
	}

	private void HandleDuplicateExpiryTimes(BuriedItem buriedItem)
	{
		long num = buriedItem.ExpiryTime;
		if (lastExpiryTime.HasValue && lastExpiryTime.Value.lastExpiryTime == num)
		{
			long num2;
			for (num2 = lastExpiryTime.Value.modifiedExpiryTime + 1; itemExpiryTracking.ContainsKey(num2); num2++)
			{
			}
			lastExpiryTime = (num, num2);
			buriedItem.ExpiryTime = num2;
		}
		else
		{
			lastExpiryTime = (num, num);
		}
	}

	private void PruneExpiredItems()
	{
		if (itemExpiryTracking.Count == 0)
		{
			return;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		if (itemExpiryTracking.Values[0].ExpiryTime > num)
		{
			return;
		}
		PooledList<BuriedItem> val = Pool.Get<PooledList<BuriedItem>>();
		try
		{
			foreach (var (num3, item) in itemExpiryTracking)
			{
				if (num3 <= num)
				{
					((List<BuriedItem>)(object)val).Add(item);
					continue;
				}
				break;
			}
			if (((List<BuriedItem>)(object)val).Count <= 0)
			{
				return;
			}
			if (((List<BuriedItem>)(object)val).Count == itemExpiryTracking.Count)
			{
				itemExpiryTracking.Clear();
			}
			foreach (BuriedItem item2 in (List<BuriedItem>)(object)val)
			{
				UnregisterItem(item2);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void UnregisterItem(BuriedItem buriedItem)
	{
		grid.Remove(buriedItem);
		itemExpiryTracking.Remove(buriedItem.ExpiryTime);
		uidItemMapping.Remove(buriedItem.UID);
		Pool.Free<BuriedItem>(ref buriedItem);
	}

	public void UnregisterItem(ulong itemUid)
	{
		if (itemUid == 0L || !uidItemMapping.TryGetValue(itemUid, out var value))
		{
			Debug.LogError((object)$"Couldn't find buried item with ID {itemUid}");
		}
		else
		{
			UnregisterItem(value);
		}
	}

	public void Clear()
	{
		uidItemMapping.Clear();
		foreach (BuriedItem value in itemExpiryTracking.Values)
		{
			BuriedItem current = value;
			Pool.Free<BuriedItem>(ref current);
		}
		itemExpiryTracking.Clear();
		grid = new Grid<BuriedItem>(128, 8096f);
		lastExpiryTime = null;
	}

	public void DoUpdate()
	{
		PruneExpiredItems();
	}

	public void AddItems(List<DiggableEntityLoot.ItemEntry> items, Vector3 digWorldPos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		List<BuriedItem> list = Pool.Get<List<BuriedItem>>();
		grid.Query(digWorldPos.x, digWorldPos.z, 64f, list);
		foreach (BuriedItem item in list)
		{
			if (item.ItemId.HasValue)
			{
				items.Add(new DiggableEntityLoot.ItemEntry
				{
					Item = ItemManager.FindItemDefinition(item.ItemId.Value),
					Skin = item.SkinId.GetValueOrDefault(),
					Min = 1,
					Max = 1,
					Weight = buriedItemWeight,
					Condition = item.Condition,
					UID = item.UID,
					Owner = item.OwnershipShare
				});
			}
		}
		Pool.Free<BuriedItem>(ref list, false);
	}

	public override void Save(SaveInfo info)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!info.forDisk)
		{
			return;
		}
		BuriedItems val = Pool.Get<BuriedItems>();
		val.buriedItems = Pool.Get<List<StoredBuriedItem>>();
		foreach (BuriedItem value in itemExpiryTracking.Values)
		{
			if (value.ItemId.HasValue)
			{
				StoredBuriedItem val2 = Pool.Get<StoredBuriedItem>();
				val2.itemId = value.ItemId.Value;
				val2.skinId = value.SkinId.GetValueOrDefault();
				val2.expiryTimeDiff = value.ExpiryTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				val2.location = value.Location;
				val2.condition = (value.Condition.HasValue ? value.Condition.Value : (-1f));
				val2.uid = value.UID;
				if (value.OwnershipShare.HasValue)
				{
					ItemOwnershipAmount val3 = Pool.Get<ItemOwnershipAmount>();
					val3.amount = value.OwnershipShare.Value.amount;
					val3.username = value.OwnershipShare.Value.username;
					val3.reason = value.OwnershipShare.Value.reason;
					val2.ownership = val3;
				}
				val.buriedItems.Add(val2);
			}
		}
		info.msg.buriedItemStorage = val;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.buriedItemStorage == null)
		{
			return;
		}
		Clear();
		foreach (StoredBuriedItem buriedItem2 in info.msg.buriedItemStorage.buriedItems)
		{
			if (buriedItem2.uid != 0L)
			{
				BuriedItem buriedItem = BuriedItem.Create(buriedItem2);
				Add(buriedItem);
			}
		}
	}
}
