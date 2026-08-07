using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class ApartmentBuilding : BaseEntity
{
	[Serializable]
	public class ApartmentPurchaseCost
	{
		public ApartmentSize Size;

		public GameObjectRef Prefab;
	}

	public static float MaxRadiusSearch = 300f;

	public ApartmentPurchaseCost[] PurchaseCosts;

	private List<ApartmentRoom> rooms = new List<ApartmentRoom>();

	private Dictionary<ulong, ApartmentRoom> roomLookup = new Dictionary<ulong, ApartmentRoom>();

	private List<ApartmentVendor> vendors = new List<ApartmentVendor>();

	private List<ApartmentMailbox> mailboxes = new List<ApartmentMailbox>();

	private Dictionary<ApartmentSize, int> remainingRooms = new Dictionary<ApartmentSize, int>();

	public NetworkableId ClientApartmentRoomNetId;

	public string ClientRoomNumber;

	public int ClientRent;

	public ApartmentSize ClientRoomSize;

	public static ApartmentBuilding Instance = null;

	public IReadOnlyList<ApartmentRoom> Rooms => rooms;

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		ApartmentBuilding val = Pool.Get<ApartmentBuilding>();
		info.msg.apartmentBuilding = val;
		if (!info.forDisk)
		{
			val.smallRoomsLeft = (remainingRooms.TryGetValue(ApartmentSize.Small, out var value) ? value : 0);
			val.mediumRoomsLeft = (remainingRooms.TryGetValue(ApartmentSize.Medium, out var value2) ? value2 : 0);
			val.largeRoomsLeft = (remainingRooms.TryGetValue(ApartmentSize.Large, out var value3) ? value3 : 0);
		}
		if (info.forConnection != null && roomLookup.TryGetValue(info.forConnection.userid, out var value4))
		{
			val.clientRoomId = value4.net.ID;
			val.clientRoomNumber = value4.RoomNumber;
			val.clientRent = value4.MinimumRent;
			val.clientRoomSize = (int)value4.Size;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		_ = info.msg.apartmentBuilding;
	}

	public override void ServerInit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		Instance = this;
		List<TriggerSafeZone> list = Pool.Get<List<TriggerSafeZone>>();
		GamePhysics.OverlapSphere<TriggerSafeZone>(((Component)this).transform.position, 1f, list, 262144, (QueryTriggerInteraction)2);
		foreach (TriggerSafeZone item in list)
		{
			item.Apartment = this;
		}
		Pool.FreeUnmanaged<TriggerSafeZone>(ref list);
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		GrabAllApartments();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		GrabAllApartments();
	}

	private void GrabAllApartments()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		foreach (ApartmentRoom room in rooms)
		{
			room.Building = default(EntityRef<ApartmentBuilding>);
		}
		foreach (ApartmentVendor vendor in vendors)
		{
			vendor.BuildingRef = default(EntityRef<ApartmentBuilding>);
		}
		rooms.Clear();
		roomLookup.Clear();
		Enumerator<BaseNetworkable> enumerator3 = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				BaseNetworkable current = enumerator3.Current;
				if ((current is ApartmentRoom || current is ApartmentVendor || current is ApartmentMailbox) && !(Vector3.Distance(((Component)current).transform.position, ((Component)this).transform.position) > MaxRadiusSearch))
				{
					if (current is ApartmentRoom item)
					{
						rooms.Add(item);
					}
					if (current is ApartmentVendor item2)
					{
						vendors.Add(item2);
					}
					if (current is ApartmentMailbox item3)
					{
						mailboxes.Add(item3);
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (ApartmentRoom room2 in rooms)
		{
			room2.Building = new EntityRef<ApartmentBuilding>(net.ID);
			foreach (ulong owner in room2.Owners)
			{
				roomLookup[owner] = room2;
			}
		}
		foreach (ApartmentVendor vendor2 in vendors)
		{
			vendor2.BuildingRef = new EntityRef<ApartmentBuilding>(net.ID);
		}
		foreach (ApartmentMailbox mailbox in mailboxes)
		{
			mailbox.Room = FindByRoomNumber(mailbox.RoomNumber);
		}
		UpdateRemainingRooms();
	}

	private void UpdateRemainingRooms()
	{
		remainingRooms.Clear();
		foreach (ApartmentRoom room in rooms)
		{
			if (!room.IsCurrentlyRented())
			{
				remainingRooms.TryGetValue(room.Size, out var value);
				value++;
				remainingRooms[room.Size] = value;
			}
		}
		SendNetworkUpdate();
	}

	public void TryPurchaseRoom(BasePlayer player, ApartmentSize size)
	{
		if (CanPurchaseRoom(size) && CanAffordRoom(player, size))
		{
			PurchaseRoom(player, size);
		}
	}

	public void TryUpgradeRoom(BasePlayer player, ApartmentSize size)
	{
		if (!CanUpgradeRoom(player, size))
		{
			return;
		}
		ApartmentRoom playerApartment = GetPlayerApartment(player);
		if ((Object)(object)playerApartment == (Object)null)
		{
			Debug.LogError((object)("Trying to upgrade room for player " + player.displayName + " but no current room was found!"));
		}
		else
		{
			if (Interface.CallHook("OnApartmentRoomUpgrade", playerApartment, player, size, this) != null)
			{
				return;
			}
			ApartmentRoom apartmentRoom = FetchClosestUnoccupiedRoom(size);
			if ((Object)(object)apartmentRoom == (Object)null)
			{
				Debug.LogError((object)$"Player tried to upgrade to an apartment of size {size} but no unoccupied rooms were found!");
				return;
			}
			int upgradeCost = GetUpgradeCost(playerApartment.Size, apartmentRoom.Size);
			if (upgradeCost > 0)
			{
				player.inventory.Take(null, ItemManager.Items.Scrap.itemid, upgradeCost);
			}
			GiveRoomToPlayer(player, apartmentRoom);
			TransferContentsBetweenRooms(playerApartment, apartmentRoom);
			playerApartment.Checkout();
			NotifyRoomChanged(player.userID);
			if (player.Connection != null)
			{
				player.ChatMessage("You have upgraded to room " + apartmentRoom.RoomNumber);
			}
			Facepunch.Rust.Analytics.Azure.OnApartmentUpgrade(player, playerApartment, apartmentRoom, upgradeCost);
			Interface.CallHook("OnApartmentRoomUpgraded", apartmentRoom, player, size, this);
		}
	}

	private void TransferContentsBetweenRooms(ApartmentRoom oldRoom, ApartmentRoom newRoom)
	{
		Item[] array = oldRoom.UpkeepTerminal.inventory.itemList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].MoveToContainer(newRoom.UpkeepTerminal.inventory);
		}
		IItemContainerEntity[] array2 = oldRoom.Furniture.OfType<IItemContainerEntity>().ToArray();
		IItemContainerEntity[] array3 = newRoom.Furniture.OfType<IItemContainerEntity>().ToArray();
		List<IItemContainerEntity> list = array3.ToList();
		PooledList<Item> val = Pool.Get<PooledList<Item>>();
		try
		{
			IItemContainerEntity[] array4 = array2;
			foreach (IItemContainerEntity oldFurniture in array4)
			{
				IItemContainerEntity itemContainerEntity = list.FirstOrDefault((IItemContainerEntity x) => x.PrefabName == oldFurniture.PrefabName && x.inventory.capacity == oldFurniture.inventory.capacity);
				if (itemContainerEntity == null)
				{
					itemContainerEntity = list.FirstOrDefault((IItemContainerEntity x) => x.PrefabName == oldFurniture.PrefabName);
				}
				if (itemContainerEntity != null)
				{
					list.Remove(itemContainerEntity);
					StorageContainer.MoveAllInventoryItems(oldFurniture.inventory, itemContainerEntity.inventory);
				}
				if (oldFurniture.inventory.itemList.Count > 0)
				{
					((List<Item>)(object)val).AddRange((IEnumerable<Item>)oldFurniture.inventory.itemList);
					for (int num = oldFurniture.inventory.itemList.Count - 1; num >= 0; num--)
					{
						oldFurniture.inventory.itemList[num].RemoveFromContainer();
					}
				}
			}
			int num2 = 0;
			array4 = array3;
			foreach (IItemContainerEntity itemContainerEntity2 in array4)
			{
				if (!itemContainerEntity2.inventory.IsFull())
				{
					for (; num2 < ((List<Item>)(object)val).Count && ((List<Item>)(object)val)[num2].MoveToContainer(itemContainerEntity2.inventory); num2++)
					{
					}
				}
			}
			if (num2 < ((List<Item>)(object)val).Count)
			{
				Debug.LogError((object)$"There were {((List<Item>)(object)val).Count - num2} items that couldn't be moved from apartment {oldRoom.RoomNumber} -> {newRoom.RoomNumber}! This shouldn't happen because each larger apartment should have enough storage");
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool CanUpgradeRoom(BasePlayer player, ApartmentSize size)
	{
		ApartmentSize playerApartmentSize = GetPlayerApartmentSize(player);
		if (playerApartmentSize != ApartmentSize.None && playerApartmentSize < size)
		{
			return HasRemainingRooms(size);
		}
		return false;
	}

	public bool CanBuyRoom(BasePlayer player, ApartmentSize size)
	{
		if (GetPlayerApartmentSize(player) == ApartmentSize.None)
		{
			return HasRemainingRooms(size);
		}
		return false;
	}

	public ApartmentRoom GetPlayerApartment(BasePlayer player)
	{
		roomLookup.TryGetValue(player.userID, out var value);
		return value;
	}

	public ApartmentRoom GetPlayerApartment(ulong player)
	{
		roomLookup.TryGetValue(player, out var value);
		return value;
	}

	public ApartmentSize GetPlayerApartmentSize(BasePlayer player)
	{
		ApartmentRoom playerApartment = GetPlayerApartment(player);
		if ((Object)(object)playerApartment == (Object)null)
		{
			return ApartmentSize.None;
		}
		return playerApartment.Size;
	}

	public bool TryCheckout(BasePlayer player)
	{
		if (GetPlayerApartmentSize(player) == ApartmentSize.None)
		{
			return false;
		}
		ApartmentRoom playerApartment = GetPlayerApartment(player);
		object obj = Interface.CallHook("OnApartmentRoomCheckout", player, playerApartment, this);
		if (obj != null)
		{
			return false;
		}
		Facepunch.Rust.Analytics.Azure.OnApartmentCheckOut(player, playerApartment);
		Checkout(playerApartment);
		Interface.CallHook("OnApartmentRoomCheckedout", player, playerApartment, this);
		return true;
	}

	public void Checkout(ApartmentRoom apartment)
	{
		List<ulong> list = apartment.Owners.ToList();
		foreach (ulong owner in apartment.Owners)
		{
			roomLookup.Remove(owner);
		}
		apartment.Checkout();
		foreach (ulong item in list)
		{
			NotifyRoomChanged(item);
		}
	}

	private bool CanPurchaseRoom(ApartmentSize size)
	{
		if (!HasRemainingRooms(size))
		{
			Debug.Log((object)$"Player tried to purchase an apartment of size {size} but there are no remaining rooms!");
			return false;
		}
		return true;
	}

	private void PurchaseRoom(BasePlayer player, ApartmentSize size)
	{
		ApartmentRoom apartmentRoom = FetchClosestUnoccupiedRoom(size);
		if ((Object)(object)apartmentRoom == (Object)null)
		{
			Debug.LogError((object)$"Player tried to purchase an apartment of size {size} but no unoccupied rooms were found!");
		}
		else if (Interface.CallHook("OnApartmentRoomPurchase", apartmentRoom, player, size, this) == null)
		{
			int purchaseScrapCost = GetPurchaseScrapCost(size);
			player.inventory.Take(null, ItemManager.Items.Scrap.itemid, purchaseScrapCost);
			GiveRoomToPlayer(player, apartmentRoom);
			Facepunch.Rust.Analytics.Azure.OnApartmentCheckIn(player, apartmentRoom, purchaseScrapCost);
			Interface.CallHook("OnApartmentRoomPurchased", apartmentRoom, player, size, this);
		}
	}

	public void GiveRoomToPlayer(BasePlayer player, ApartmentRoom room)
	{
		roomLookup[player.userID] = room;
		room.StartRentingRoom(player);
		UpdateRemainingRooms();
		NotifyRoomChanged(player.userID);
	}

	private void NotifyRoomChanged(ulong player)
	{
		SendNetworkUpdateImmediate();
	}

	public ApartmentRoom FindByRoomNumber(string roomNumber)
	{
		return rooms.FirstOrDefault((ApartmentRoom x) => x.RoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase));
	}

	private ApartmentRoom FetchClosestUnoccupiedRoom(ApartmentSize size)
	{
		return (from x in rooms
			where !x.IsCurrentlyRented() && x.Size == size
			orderby Vector3.Distance(((Component)x).transform.position, ((Component)this).transform.position)
			select x).FirstOrDefault();
	}

	public static void OnTeamMembershipChanged(List<ulong> members, ulong removedPlayer = 0uL)
	{
		if ((Object)(object)Instance == (Object)null)
		{
			return;
		}
		using (TimeWarning.New("ApartmentBuilding.OnTeamMembershipChanged"))
		{
			if (removedPlayer != 0L)
			{
				Instance.SendNetworkUpdateForRoom(removedPlayer);
			}
			Instance.SendNetworkUpdateForRooms(members);
		}
	}

	public void SendNetworkUpdateForRooms(List<ulong> playerList)
	{
		foreach (ulong player in playerList)
		{
			SendNetworkUpdateForRoom(player);
		}
	}

	public void SendNetworkUpdateForRoom(ulong player)
	{
		ApartmentRoom playerApartment = GetPlayerApartment(player);
		if ((Object)(object)playerApartment != (Object)null)
		{
			playerApartment.SendNetworkUpdate();
		}
	}

	public bool HasRemainingRooms(ApartmentSize size)
	{
		if (!remainingRooms.TryGetValue(size, out var value))
		{
			return false;
		}
		return value > 0;
	}

	public int GetRemainingRoomCount(ApartmentSize size)
	{
		if (!remainingRooms.TryGetValue(size, out var value))
		{
			return 0;
		}
		return value;
	}

	public bool CanAffordRoom(BasePlayer player, ApartmentSize size)
	{
		int purchaseScrapCost = GetPurchaseScrapCost(size);
		if (purchaseScrapCost <= 0)
		{
			Debug.LogError((object)$"Trying to purchase an apartment but no cost is found for size {size}!");
			return false;
		}
		if (player.inventory.GetAmount(ItemManager.Items.Scrap.itemid) < purchaseScrapCost)
		{
			return false;
		}
		return true;
	}

	public bool CanAffordUpgrade(BasePlayer player, ApartmentSize currentSize, ApartmentSize targetSize)
	{
		int upgradeCost = GetUpgradeCost(currentSize, targetSize);
		if (upgradeCost <= 0)
		{
			return true;
		}
		if (player.inventory.GetAmount(ItemManager.Items.Scrap.itemid) < upgradeCost)
		{
			return false;
		}
		return true;
	}

	public int GetUpgradeCost(ApartmentSize currentSize, ApartmentSize targetSize)
	{
		int purchaseScrapCost = GetPurchaseScrapCost(currentSize);
		return GetPurchaseScrapCost(targetSize) - purchaseScrapCost;
	}

	private ApartmentRoom GetApartmentPrefab(ApartmentSize size)
	{
		ApartmentPurchaseCost apartmentPurchaseCost = PurchaseCosts.FirstOrDefault((ApartmentPurchaseCost x) => x.Size == size);
		if (apartmentPurchaseCost == null)
		{
			Debug.LogError((object)$"Trying to get purchase cost for apartment size {size} but no cost was found!");
			return null;
		}
		GameObject val = apartmentPurchaseCost.Prefab.Get();
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)$"Trying to get purchase cost for apartment size {size} but Prefab.Get() returned null!");
			return null;
		}
		ApartmentRoom component = val.GetComponent<ApartmentRoom>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)$"Trying to get purchase cost for apartment size {size} but Prefab.Get() returned a prefab without an ApartmentRoom component!");
			return null;
		}
		return component;
	}

	public int GetPurchaseScrapCost(ApartmentSize size)
	{
		ApartmentRoom apartmentPrefab = GetApartmentPrefab(size);
		if ((Object)(object)apartmentPrefab == (Object)null)
		{
			return 0;
		}
		return apartmentPrefab.PurchaseCost;
	}

	public int GetUpkeepCost(ApartmentSize size)
	{
		ApartmentRoom apartmentPrefab = GetApartmentPrefab(size);
		if ((Object)(object)apartmentPrefab == (Object)null)
		{
			return 0;
		}
		return apartmentPrefab.MinimumRent;
	}
}
