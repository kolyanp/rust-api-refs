using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ReclaimManager : BaseEntity
{
	public class PlayerReclaimEntry : IPooled
	{
		public float timeAlive;

		public int id;

		public ulong victimID;

		public ItemContainer mainInventory;

		public ItemContainer wearInventory;

		public ItemContainer beltInventory;

		public ItemContainer backpackInventory;

		public bool HasItems()
		{
			if ((mainInventory == null || mainInventory.itemList.Count <= 0) && (wearInventory == null || wearInventory.itemList.Count <= 0) && (beltInventory == null || beltInventory.itemList.Count <= 0))
			{
				if (backpackInventory != null)
				{
					return backpackInventory.itemList.Count > 0;
				}
				return false;
			}
			return true;
		}

		private ItemContainer CreateContainer()
		{
			ItemContainer itemContainer = Pool.Get<ItemContainer>();
			itemContainer.entityOwner = instance;
			itemContainer.allowedContents = ItemContainer.ContentsType.Generic;
			itemContainer.SetOnlyAllowedItem(null);
			itemContainer.maxStackSize = 0;
			itemContainer.containerVolume = 10;
			itemContainer.ServerInitialize(null, 40);
			itemContainer.canAcceptItem = null;
			itemContainer.GiveUID();
			return itemContainer;
		}

		void IPooled.LeavePool()
		{
			mainInventory = CreateContainer();
			wearInventory = CreateContainer();
			beltInventory = CreateContainer();
			backpackInventory = CreateContainer();
		}

		void IPooled.EnterPool()
		{
			timeAlive = 0f;
			id = -2;
			Pool.Free<ItemContainer>(ref mainInventory);
			Pool.Free<ItemContainer>(ref wearInventory);
			Pool.Free<ItemContainer>(ref beltInventory);
			Pool.Free<ItemContainer>(ref backpackInventory);
		}

		public void GiveToPlayer(BasePlayer player)
		{
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			wearInventory.MoveAllItems(player.inventory.containerWear);
			beltInventory.MoveAllItems(player.inventory.containerBelt);
			mainInventory.MoveAllItems(player.inventory.containerMain);
			if (backpackInventory.itemList.Count > 0)
			{
				Item backpackWithInventory = player.inventory.GetBackpackWithInventory();
				if (backpackWithInventory != null)
				{
					backpackInventory.MoveAllItems(backpackWithInventory.contents);
				}
				if (backpackInventory.itemList.Count > 0)
				{
					backpackInventory.MoveAllItems(player.inventory.containerMain);
				}
				if (backpackInventory.itemList.Count > 0)
				{
					backpackInventory.MoveAllItems(player.inventory.containerBelt);
				}
			}
			if (wearInventory.itemList.Count + beltInventory.itemList.Count + mainInventory.itemList.Count + backpackInventory.itemList.Count > 0)
			{
				ItemContainer.Drop("assets/prefabs/misc/item drop/item_drop_backpack.prefab", player.GetDropPosition(), default(Quaternion), wearInventory, beltInventory, mainInventory, backpackInventory);
			}
		}
	}

	private const int defaultReclaims = 128;

	private const int reclaimSlotCount = 40;

	public static string ReclaimCorpsePrefab = "assets/prefabs/misc/item drop/item_drop_backpack.prefab";

	private int lastReclaimID;

	[ServerVar(Help = "(Generated) Time in minutes after which an uncollected softcore death reclaim backpack expires and its contents are destroyed")]
	public static float reclaim_expire_minutes = 120f;

	private static ReclaimManager _instance;

	private Dictionary<ulong, PlayerReclaimEntry> entries = new Dictionary<ulong, PlayerReclaimEntry>();

	private float lastTickTime;

	public static ReclaimManager instance => _instance;

	public int AddPlayerReclaim(ulong victimID, List<Item> belt, List<Item> wear, List<Item> main, List<Item> backpack)
	{
		PlayerReclaimEntry orCreateReclaim = GetOrCreateReclaim(victimID);
		if (belt != null)
		{
			foreach (Item item in belt)
			{
				item.MoveToContainer(orCreateReclaim.beltInventory);
			}
		}
		if (wear != null)
		{
			foreach (Item item2 in wear)
			{
				item2.MoveToContainer(orCreateReclaim.wearInventory);
			}
		}
		if (main != null)
		{
			foreach (Item item3 in main)
			{
				item3.MoveToContainer(orCreateReclaim.mainInventory);
			}
		}
		if (backpack != null)
		{
			foreach (Item item4 in backpack)
			{
				item4.MoveToContainer(orCreateReclaim.backpackInventory);
			}
		}
		lastReclaimID++;
		orCreateReclaim.victimID = victimID;
		orCreateReclaim.id = lastReclaimID;
		orCreateReclaim.timeAlive = 0f;
		return orCreateReclaim.id;
	}

	public void DoCleanup()
	{
		List<PlayerReclaimEntry> list = Pool.Get<List<PlayerReclaimEntry>>();
		foreach (PlayerReclaimEntry value in entries.Values)
		{
			if (!value.HasItems() || value.timeAlive / 60f > reclaim_expire_minutes)
			{
				list.Add(value);
			}
		}
		foreach (PlayerReclaimEntry item in list)
		{
			RemoveEntry(item);
		}
		Pool.Free<PlayerReclaimEntry>(ref list, false);
	}

	public void TickEntries()
	{
		float num = Time.realtimeSinceStartup - lastTickTime;
		foreach (PlayerReclaimEntry value in entries.Values)
		{
			value.timeAlive += num;
		}
		lastTickTime = Time.realtimeSinceStartup;
		DoCleanup();
	}

	public bool HasReclaims(ulong playerID)
	{
		return entries.ContainsKey(playerID);
	}

	public PlayerReclaimEntry GetReclaim(ulong victimId)
	{
		entries.TryGetValue(victimId, out var value);
		return value;
	}

	public PlayerReclaimEntry GetOrCreateReclaim(ulong victimId)
	{
		PlayerReclaimEntry playerReclaimEntry = GetReclaim(victimId);
		if (playerReclaimEntry == null)
		{
			playerReclaimEntry = Pool.Get<PlayerReclaimEntry>();
			playerReclaimEntry.victimID = victimId;
			entries.Add(victimId, playerReclaimEntry);
		}
		return playerReclaimEntry;
	}

	public void RemoveEntry(PlayerReclaimEntry entry)
	{
		entries.Remove(entry.victimID);
		Pool.Free<PlayerReclaimEntry>(ref entry);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.reclaimManager == null)
		{
			return;
		}
		lastReclaimID = info.msg.reclaimManager.lastReclaimID;
		foreach (ReclaimInfo reclaimEntry in info.msg.reclaimManager.reclaimEntries)
		{
			PlayerReclaimEntry orCreateReclaim = GetOrCreateReclaim(reclaimEntry.victimID);
			orCreateReclaim.victimID = reclaimEntry.victimID;
			orCreateReclaim.id = reclaimEntry.reclaimId;
			orCreateReclaim.mainInventory.Load(reclaimEntry.mainInventory);
			orCreateReclaim.wearInventory.Load(reclaimEntry.wearInventory);
			orCreateReclaim.beltInventory.Load(reclaimEntry.beltInventory);
			orCreateReclaim.backpackInventory.Load(reclaimEntry.backpackInventory);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			return;
		}
		info.msg.reclaimManager = Pool.Get<ReclaimManager>();
		info.msg.reclaimManager.reclaimEntries = Pool.Get<List<ReclaimInfo>>();
		info.msg.reclaimManager.lastReclaimID = lastReclaimID;
		foreach (PlayerReclaimEntry value in entries.Values)
		{
			ReclaimInfo val = Pool.Get<ReclaimInfo>();
			val.victimID = value.victimID;
			val.reclaimId = value.id;
			val.mainInventory = value.mainInventory.Save();
			val.wearInventory = value.wearInventory.Save();
			val.beltInventory = value.beltInventory.Save();
			val.backpackInventory = value.backpackInventory.Save();
			info.msg.reclaimManager.reclaimEntries.Add(val);
		}
	}

	public override void ServerInit()
	{
		_instance = this;
		base.ServerInit();
		InvokeRepeating(TickEntries, 1f, 60f);
	}

	internal override void DoServerDestroy()
	{
		_instance = null;
		base.DoServerDestroy();
	}
}
