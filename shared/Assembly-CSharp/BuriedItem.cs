using System;
using System.Runtime.CompilerServices;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class BuriedItem : IPooled
{
	[CompilerGenerated]
	private Vector2 _003CLocation_003Ek__BackingField;

	public int? ItemId { get; private set; }

	public ulong UID { get; private set; }

	public ItemOwnershipShare? OwnershipShare { get; private set; }

	public ulong? SkinId { get; private set; }

	public long ExpiryTime { get; set; }

	public Vector2 Location
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CLocation_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CLocation_003Ek__BackingField = value;
		}
	}

	public float? Condition { get; private set; }

	public static BuriedItem Create(Item item, Vector3 worldPosition, long expiryTime)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)item.info == (Object)null || item.info == null)
		{
			Debug.LogError((object)$"Tried to create a buried item with an item that has no ItemDefinition! UID: {item.uid}, ItemId: {item.info?.itemid}");
			return null;
		}
		BuriedItem buriedItem = Pool.Get<BuriedItem>();
		buriedItem.ItemId = item.info.itemid;
		buriedItem.ExpiryTime = expiryTime;
		buriedItem.Location = new Vector2(worldPosition.x, worldPosition.z);
		buriedItem.Condition = (item.hasCondition ? new float?(item.condition) : ((float?)null));
		buriedItem.UID = item.uid.Value;
		if (item.ownershipShares != null && item.ownershipShares.Count > 0)
		{
			buriedItem.OwnershipShare = item.ownershipShares[0];
		}
		if (item.skin != 0L)
		{
			buriedItem.SkinId = item.skin;
		}
		return buriedItem;
	}

	public static BuriedItem Create(StoredBuriedItem storedBuriedItem)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BuriedItem buriedItem = Pool.Get<BuriedItem>();
		buriedItem.ItemId = storedBuriedItem.itemId;
		buriedItem.SkinId = storedBuriedItem.skinId;
		buriedItem.Location = storedBuriedItem.location;
		buriedItem.ExpiryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + storedBuriedItem.expiryTimeDiff;
		buriedItem.Condition = ((storedBuriedItem.condition < 0f) ? ((float?)null) : new float?(storedBuriedItem.condition));
		buriedItem.UID = storedBuriedItem.uid;
		if (storedBuriedItem.ownership != null)
		{
			buriedItem.OwnershipShare = new ItemOwnershipShare
			{
				amount = storedBuriedItem.ownership.amount,
				reason = storedBuriedItem.ownership.reason,
				username = storedBuriedItem.ownership.username
			};
		}
		return buriedItem;
	}

	public void EnterPool()
	{
		ItemId = null;
		OwnershipShare = null;
		SkinId = null;
	}

	public void LeavePool()
	{
	}
}
