using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using ProtoBuf;
using UnityEngine;

public class ApartmentRoom : BaseEntity
{
	[Serializable]
	private class FurnitureSpawn
	{
		public string Prefab;

		public Vector3 LocalPosition;

		public Quaternion LocalRotation;

		public Vector3 LocalScale;
	}

	public enum ApartmentAuth
	{
		Default,
		Loot,
		EnterApartment
	}

	public NetworkableId UpkeepTerminalId;

	private ItemDefinition scrapItemDef;

	private ApartmentUpkeepTerminal apartmentUpkeepTerminal;

	private Action _delayedUpdateCallback;

	private float outstandingRent;

	private float timeRentOverdue;

	private float lastRentPaymentTime;

	private static float UpkeepTick = 1800f;

	public static readonly Flags Flag_IsRented = Flags.Reserved1;

	public static readonly Flags Flag_BreakIn = Flags.Reserved2;

	public GameObject[] FurnitureObjects;

	private HashSet<ulong> owners = new HashSet<ulong>();

	public ApartmentDoor FrontDoor;

	public ApartmentSize Size;

	public string RoomNumber;

	public Transform TeleportAnchor;

	public int PurchaseCost;

	public Transform CameraAnchor;

	public int MinimumRent;

	public ApartmentOccludeeComponent OcludeeOverride;

	public TriggerInvisibleDoorToggle invisibleBarrier;

	public Transform invisibleBarrierEjectLocation;

	[NonSerialized]
	public int UpkeepSeconds;

	[NonSerialized]
	public bool ClientIsAuthed;

	[NonSerialized]
	private int cachedStorageCapacity = -1;

	[NonSerialized]
	public List<BaseEntity> Furniture = new List<BaseEntity>();

	[SerializeField]
	[HideInInspector]
	private List<FurnitureSpawn> furnitureSpawns = new List<FurnitureSpawn>();

	private TriggerSafeZoneOverride safeZoneOverrideTrigger;

	private EntityRef<ApartmentBuilding> __sync_Building;

	public float CachedDailyUpkeep { get; set; }

	public ApartmentUpkeepTerminal UpkeepTerminal => apartmentUpkeepTerminal;

	[Sync(Autosave = true)]
	public EntityRef<ApartmentBuilding> Building
	{
		[CompilerGenerated]
		get
		{
			return __sync_Building;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Building, value))
			{
				__sync_Building = value;
				byte nameID = __GetWeaverID("Building");
				QueueSyncVar(nameID);
			}
		}
	}

	public IReadOnlyCollection<ulong> Owners => owners;

	public override void Save(SaveInfo info)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		ApartmentRoom val = Pool.Get<ApartmentRoom>();
		info.msg.apartmentRoom = val;
		val.roomNumber = RoomNumber;
		bool flag = info.forConnection != null && IsAuthed(info.forConnection.userid);
		if (flag)
		{
			val.upkeepSeconds = GetUpkeepSecondsServer(cached: false);
			val.dailyUpkeepCost = CachedDailyUpkeep;
		}
		if ((info.forDisk | flag) || CanBypassInvisibleBarrier(info.forConnection.userid))
		{
			val.owners = Pool.Get<List<ulong>>();
			val.owners.AddRange(owners);
		}
		if (!info.forDisk)
		{
			return;
		}
		val.upkeepTerminalId = (NetworkableId)(((Object)(object)apartmentUpkeepTerminal != (Object)null) ? apartmentUpkeepTerminal.net.ID : default(NetworkableId));
		val.timeRentOverdue = timeRentOverdue;
		val.outstandingRent = outstandingRent;
		val.furnitureIds = Pool.Get<List<NetworkableId>>();
		foreach (BaseEntity item in Furniture)
		{
			if (!item.IsDestroyed)
			{
				val.furnitureIds.Add(item.net.ID);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.apartmentRoom != null)
		{
			RoomNumber = info.msg.apartmentRoom.roomNumber;
		}
		if (base.isServer)
		{
			SetStaticFurnitureVisible(HasFlag(Flag_IsRented));
		}
		owners.Clear();
		if (info.msg.apartmentRoom.owners != null)
		{
			foreach (ulong owner in info.msg.apartmentRoom.owners)
			{
				owners.Add(owner);
			}
		}
		if (!base.isServer)
		{
			return;
		}
		if (info.msg.apartmentRoom.furnitureIds != null)
		{
			foreach (NetworkableId furnitureId in info.msg.apartmentRoom.furnitureIds)
			{
				BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(furnitureId) as BaseEntity;
				if ((Object)(object)baseEntity != (Object)null)
				{
					Furniture.Add(baseEntity);
				}
				else
				{
					Debug.LogError((object)$"Apartment {this} couldn't find furniture with ID '{furnitureId}' when loading");
				}
			}
		}
		SetFlagLocal(Flag_BreakIn, b: false);
		timeRentOverdue = info.msg.apartmentRoom.timeRentOverdue;
		outstandingRent = info.msg.apartmentRoom.outstandingRent;
		UpkeepTerminalId = info.msg.apartmentRoom.upkeepTerminalId;
	}

	public override void InitShared()
	{
		base.InitShared();
		scrapItemDef = ItemManager.FindItemDefinition("scrap");
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		if (CanBypassInvisibleBarrier(connection.userid))
		{
			return false;
		}
		return !IsAuthed(connection.userid);
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		OnLoaded(loadingSave: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		OnLoaded(loadingSave: true);
	}

	private void OnLoaded(bool loadingSave)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseEntity item in Furniture)
		{
			PostFurnitureSpawnedOrLoaded(item);
		}
		SetupFrontDoor(loadingSave);
		apartmentUpkeepTerminal = BaseNetworkable.serverEntities.Find(UpkeepTerminalId) as ApartmentUpkeepTerminal;
		if ((Object)(object)apartmentUpkeepTerminal != (Object)null)
		{
			apartmentUpkeepTerminal.Apartment = this;
		}
		else if (UpkeepTerminalId != default(NetworkableId))
		{
			Debug.LogError((object)$"Apartment {this} couldn't find its upkeep terminal with ID '{UpkeepTerminalId}' when loading");
		}
	}

	private void PostFurnitureSpawnedOrLoaded(BaseEntity entity)
	{
		if (entity is StorageContainer container)
		{
			SubscribeToUpkeepUpdates(container);
		}
		MakeEntityStatic(entity);
		if (entity is Fridge fridge)
		{
			fridge.SetFlagLocal(Flags.Reserved8, b: true);
		}
		if (entity is Telephone telephone)
		{
			telephone.SetFlagLocal(Flags.Reserved8, b: true);
		}
	}

	private void AddDelayedUpdate()
	{
		if (_delayedUpdateCallback == null)
		{
			_delayedUpdateCallback = DelayedUpdate;
		}
		if (IsInvoking(_delayedUpdateCallback))
		{
			CancelInvoke(_delayedUpdateCallback);
		}
		Invoke(_delayedUpdateCallback, 1f);
	}

	private void DelayedUpdate()
	{
		UpdateUpkeep();
		SendNetworkUpdate();
	}

	private void CancelDelayedUpdate()
	{
		if (_delayedUpdateCallback == null)
		{
			_delayedUpdateCallback = DelayedUpdate;
		}
		if (IsInvoking(_delayedUpdateCallback))
		{
			CancelInvoke(_delayedUpdateCallback);
		}
	}

	private void SubscribeToUpkeepUpdates(StorageContainer container)
	{
		ItemContainer inventory = container.inventory;
		inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Remove(inventory.onItemAddedRemoved, new Action<Item, bool>(OnFurnitureInventoryAddedRemoved));
		ItemContainer inventory2 = container.inventory;
		inventory2.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory2.onItemAddedRemoved, new Action<Item, bool>(OnFurnitureInventoryAddedRemoved));
		container.inventory.onDirty -= OnFurnitureInventoryDirty;
		container.inventory.onDirty += OnFurnitureInventoryDirty;
	}

	private int GetUpkeepSecondsServer(bool cached)
	{
		if (!cached)
		{
			UpdateUpkeep();
		}
		return UpkeepSeconds;
	}

	private void UpdateUpkeep()
	{
		UpkeepSeconds = CalculateUpkeepSeconds();
	}

	private int CalculateUpkeepSeconds()
	{
		int scrapForUpkeep = GetScrapForUpkeep();
		CachedDailyUpkeep = GetDailyUpkeepCost();
		return Mathf.FloorToInt((float)scrapForUpkeep / CachedDailyUpkeep * 86400f);
	}

	private int GetScrapForUpkeep()
	{
		if ((Object)(object)apartmentUpkeepTerminal == (Object)null || apartmentUpkeepTerminal.IsDestroyed)
		{
			Debug.LogError((object)$"Apartment {this} is missing it's upkeep terminal");
			return 0;
		}
		return apartmentUpkeepTerminal.inventory.GetAmount(scrapItemDef.itemid, onlyUsableAmounts: false);
	}

	public float GetDailyUpkeepCost()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		foreach (BaseEntity item in Furniture)
		{
			if (!(item is StorageContainer { IsDestroyed: false } storageContainer))
			{
				continue;
			}
			foreach (Item item2 in storageContainer.inventory.itemList)
			{
				if (item2.info.ApartmentTaxPerStack > 0f)
				{
					num += item2.info.ApartmentTaxPerStack * ((float)item2.amount / (float)item2.MaxStackable());
				}
			}
		}
		ApartmentBuilding apartmentBuilding = Building.Get(serverside: true);
		Vector3 val = (BaseNetworkable.UseParallelSaves ? Facepunch.Extend.TransformEx.Unsafe.GetPosMT(apartmentBuilding.TransformHandle) : ((Component)apartmentBuilding).transform.position);
		foreach (ulong owner in owners)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(owner);
			if ((Object)(object)basePlayer == (Object)null || basePlayer.IsDestroyed || Vector3.Distance(BaseNetworkable.UseParallelSaves ? Facepunch.Extend.TransformEx.Unsafe.GetPosMT(basePlayer.TransformHandle) : ((Component)basePlayer).transform.position, val) > 200f)
			{
				continue;
			}
			List<Item> list = Pool.Get<List<Item>>();
			basePlayer.inventory.GetAllItems(list);
			foreach (Item item3 in list)
			{
				if (item3.info.ApartmentTaxPerStack > 0f)
				{
					num += item3.info.ApartmentTaxPerStack * ((float)item3.amount / (float)item3.MaxStackable());
				}
			}
			Pool.Free<Item>(ref list, false);
		}
		return Mathf.Max((float)MinimumRent, num * ApartmentCommands.rentscaling);
	}

	private void OnFurnitureInventoryAddedRemoved(Item item, bool added)
	{
		AddDelayedUpdate();
	}

	private void OnFurnitureInventoryDirty()
	{
		AddDelayedUpdate();
	}

	private void ResetRentState()
	{
		lastRentPaymentTime = Time.time;
		outstandingRent = 0f;
		timeRentOverdue = 0f;
	}

	private void RunUpkeepPayment()
	{
		if (IsCurrentlyRented())
		{
			float dailyUpkeepCost = GetDailyUpkeepCost();
			float time = Time.time;
			float num = time - lastRentPaymentTime;
			float num2 = dailyUpkeepCost * (num / 86400f);
			outstandingRent += num2;
			int iAmount = Mathf.CeilToInt(outstandingRent);
			int num3 = apartmentUpkeepTerminal.inventory.Take(null, scrapItemDef.itemid, iAmount);
			outstandingRent -= num3;
			if (outstandingRent > 0f)
			{
				timeRentOverdue += num;
			}
			else
			{
				timeRentOverdue = 0f;
			}
			lastRentPaymentTime = time;
			if (timeRentOverdue > ApartmentCommands.apartmentevictiondelay)
			{
				Building.Get(serverside: true).Checkout(this);
			}
			SendNetworkUpdate();
		}
	}

	public bool IsCurrentlyRented()
	{
		return HasFlag(Flag_IsRented);
	}

	private void SetStaticFurnitureVisible(bool state)
	{
		if (FurnitureObjects == null)
		{
			return;
		}
		GameObject[] furnitureObjects = FurnitureObjects;
		foreach (GameObject val in furnitureObjects)
		{
			if ((Object)(object)val != (Object)null && val.activeSelf != state)
			{
				val.SetActive(state);
			}
		}
	}

	public bool IsAuthed(ulong user, ApartmentAuth auth = ApartmentAuth.Default)
	{
		if (owners == null)
		{
			return false;
		}
		if (owners.Contains(user))
		{
			return true;
		}
		if (base.isServer && ApartmentCommands.adminapartmentbypass && BasePlayer.TryFindByID(user, out var basePlayer) && basePlayer.IsAdmin)
		{
			return true;
		}
		if (auth == ApartmentAuth.EnterApartment && base.isServer)
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(user);
			if (playerTeam != null)
			{
				foreach (ulong owner in Owners)
				{
					if (playerTeam.members.Contains(owner))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsBreakInActive()
	{
		return HasFlag(Flag_BreakIn);
	}

	public bool IsInsideRoom(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		OBB val = WorldSpaceBounds();
		return ((OBB)(ref val)).Contains(((Component)player).transform.position);
	}

	public override void AdminKill()
	{
	}

	public void StartBreakIn()
	{
		if (!IsBreakInActive())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flag_BreakIn, b: true);
			}
			CancelInvoke(CancelBreakIn);
			Invoke(CancelBreakIn, ApartmentCommands.intruderauthseconds);
		}
	}

	public void CancelBreakIn()
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flag_BreakIn, b: false);
		if ((Object)(object)safeZoneOverrideTrigger == (Object)null)
		{
			safeZoneOverrideTrigger = ((Component)this).GetComponentInChildren<TriggerSafeZoneOverride>();
		}
		if (!((Object)(object)safeZoneOverrideTrigger != (Object)null) || safeZoneOverrideTrigger.entityContents == null || !((Object)(object)invisibleBarrierEjectLocation != (Object)null))
		{
			return;
		}
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		try
		{
			foreach (BaseEntity entityContent in safeZoneOverrideTrigger.entityContents)
			{
				if (entityContent is BasePlayer basePlayer && !CanBypassInvisibleBarrier(basePlayer))
				{
					((List<BasePlayer>)(object)val).Add(basePlayer);
				}
			}
			foreach (BasePlayer item in (List<BasePlayer>)(object)val)
			{
				item.EnsureDismounted();
				item.EndLooting();
				item.Teleport(invisibleBarrierEjectLocation.position);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void SetEntityFurniture(bool state)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		SetStaticFurnitureVisible(state);
		foreach (BaseEntity item in Furniture)
		{
			if ((Object)(object)item != (Object)null && !item.IsDestroyed)
			{
				item.Kill();
			}
		}
		Furniture.Clear();
		if ((Object)(object)UpkeepTerminal != (Object)null)
		{
			UpkeepTerminal.Kill();
		}
		if (!state)
		{
			return;
		}
		foreach (FurnitureSpawn furnitureSpawn in furnitureSpawns)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(furnitureSpawn.Prefab, ((Component)this).transform.TransformPoint(furnitureSpawn.LocalPosition), ((Component)this).transform.rotation * furnitureSpawn.LocalRotation);
			if ((Object)(object)baseEntity != (Object)null)
			{
				baseEntity.Spawn();
				if (baseEntity is ApartmentUpkeepTerminal apartmentUpkeepTerminal)
				{
					this.apartmentUpkeepTerminal = apartmentUpkeepTerminal;
					this.apartmentUpkeepTerminal.Apartment = this;
				}
				else
				{
					Furniture.Add(baseEntity);
				}
				if (baseEntity is Telephone telephone)
				{
					telephone.Controller.UpdatePhoneName("Apartment " + RoomNumber);
					telephone.SetFlagLocal(Flags.Reserved11, b: true);
					foreach (PhoneController allTelephone in TelephoneManager.AllTelephones)
					{
						if (allTelephone.GetBaseEntity() is Telephone telephone2 && telephone2.HasFlag(Flags.Reserved11) && (Object)(object)telephone2 != (Object)(object)telephone)
						{
							telephone2.Controller.AddSavedNumber(telephone.Controller.PhoneNumber, telephone.Controller.PhoneName);
						}
					}
				}
				PostFurnitureSpawnedOrLoaded(baseEntity);
				AddApartmentLockToEntity(baseEntity);
			}
			else
			{
				Debug.LogError((object)$"Apartment {this} failed to spawn furniture with prefab '{furnitureSpawn.Prefab}'");
			}
		}
	}

	private void AddInitialScrapToUpkeepTerminal()
	{
		if ((Object)(object)UpkeepTerminal != (Object)null)
		{
			ItemManager.Create(ItemManager.Items.Scrap, Mathf.Max(Mathf.CeilToInt((float)MinimumRent / 24f * ApartmentCommands.apartmentfreerenthours), 1), 0uL, isServerSide: true, 0uL).MoveToContainer(UpkeepTerminal.inventory);
		}
	}

	private void SetupFrontDoor(bool loadingSave)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)FrontDoor != (Object)null)
		{
			FrontDoor.SetFlagLocal(Flags.Open, b: false);
			FrontDoor.RoomNumber = RoomNumber;
			FrontDoor.ApartmentId = net.ID;
			if (!loadingSave)
			{
				AddApartmentLockToEntity(FrontDoor);
			}
		}
	}

	private void AddApartmentLockToEntity(BaseEntity entity)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!(entity is Door) || !((Object)(object)FrontDoor != (Object)(object)entity))
		{
			ApartmentLock apartmentLock = entity.GetSlot(Slot.Lock) as ApartmentLock;
			if ((Object)(object)apartmentLock == (Object)null)
			{
				apartmentLock = GameManager.server.CreateEntity("assets/prefabs/apartment/apartment_lock.prefab") as ApartmentLock;
				apartmentLock.SetParent(entity, entity.GetSlotAnchorName(Slot.Lock));
				apartmentLock.Spawn();
				apartmentLock.SetFlagLocal(Flags.Locked, b: true);
				apartmentLock.Room = this;
				entity.SetSlot(Slot.Lock, apartmentLock);
			}
		}
	}

	public void Checkout()
	{
		ClearRoom();
		CancelDelayedUpdate();
		SetEntityFurniture(state: false);
		SetFlagLocal(Flag_IsRented, b: false);
		if ((Object)(object)FrontDoor == (Object)null)
		{
			foreach (BaseEntity child in children)
			{
				if (child is ApartmentDoor frontDoor)
				{
					FrontDoor = frontDoor;
					break;
				}
			}
		}
		if ((Object)(object)FrontDoor != (Object)null)
		{
			FrontDoor.SetOpen(open: false);
		}
		RemoveAllPlayers();
		SendNetworkUpdate();
	}

	private void RemoveAllPlayers()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)invisibleBarrierEjectLocation == (Object)null)
		{
			Debug.LogError((object)$"Unable to eject players from apartment {this} because no invisible barrier eject location was provided!");
			return;
		}
		foreach (BasePlayer item in ((IEnumerable<BasePlayer>)BasePlayer.activePlayerList).Concat((IEnumerable<BasePlayer>)BasePlayer.sleepingPlayerList))
		{
			if (IsInsideRoom(item))
			{
				item.Teleport(invisibleBarrierEjectLocation.position);
			}
		}
	}

	private void ClearRoom()
	{
		owners.Clear();
	}

	public void AddUser(ulong user)
	{
		owners.Add(user);
	}

	public void RemoveUser(ulong user)
	{
		owners.Remove(user);
	}

	public void StartRentingRoom(BasePlayer player)
	{
		if (!IsCurrentlyRented())
		{
			AddUser(player.userID);
			ResetRentState();
			SetEntityFurniture(state: true);
			UpdateBedAssignments();
			AddInitialScrapToUpkeepTerminal();
			SetFlagLocal(Flag_IsRented, b: true);
			SendNetworkUpdate();
		}
	}

	public bool TryTeleportPlayer(BasePlayer player)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TeleportAnchor == (Object)null)
		{
			Debug.LogError((object)$"Unable to safely teleport players into their room {this} because no teleport anchor was provided!");
			return false;
		}
		player.Teleport(((Component)TeleportAnchor).transform.position);
		if (player.IsSleeping())
		{
			player.SetServerFall(wantsOn: true);
		}
		return true;
	}

	private void MakeEntityStatic(BaseEntity entity)
	{
		DestroyOnGroundMissing[] componentsInChildren = ((Component)entity).GetComponentsInChildren<DestroyOnGroundMissing>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren[i]);
		}
		GroundWatch[] componentsInChildren2 = ((Component)entity).GetComponentsInChildren<GroundWatch>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren2[i]);
		}
		if (entity is BaseCombatEntity baseCombatEntity)
		{
			baseCombatEntity.baseProtection = ProtectionProperties.immortalProtection;
			baseCombatEntity.markAttackerHostile = false;
			baseCombatEntity.ShowHealthInfo = false;
			baseCombatEntity.pickup.enabled = false;
		}
		entity.networkRange = EntityNetworkRange.Small;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(RunUpkeepPayment, Random.Range(0f, 1f) * UpkeepTick, UpkeepTick, UpkeepTick * 0.05f);
	}

	private void UpdateBedAssignments()
	{
		ulong user = ((Owners.Count == 0) ? 0 : Owners.First());
		foreach (BaseEntity item in Furniture)
		{
			if (item is SleepingBag sleepingBag)
			{
				sleepingBag.AssignToUser(user);
			}
		}
	}

	public int GetTotalStorageCapacity()
	{
		if (cachedStorageCapacity >= 0)
		{
			return cachedStorageCapacity;
		}
		int num = 0;
		StorageContainer storageContainer = default(StorageContainer);
		foreach (FurnitureSpawn furnitureSpawn in furnitureSpawns)
		{
			GameObject val = GameManager.server.FindPrefab(furnitureSpawn.Prefab);
			if ((Object)(object)val != (Object)null && val.TryGetComponent<StorageContainer>(ref storageContainer) && !(storageContainer is ApartmentUpkeepTerminal))
			{
				num += storageContainer.inventorySlots;
			}
		}
		cachedStorageCapacity = num;
		return num;
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (!serverside)
		{
			return;
		}
		Furniture.Clear();
		if (FurnitureObjects == null)
		{
			return;
		}
		GameObject[] furnitureObjects = FurnitureObjects;
		foreach (GameObject val in furnitureObjects)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			BaseEntity[] componentsInChildren = val.GetComponentsInChildren<BaseEntity>(true);
			foreach (BaseEntity baseEntity in componentsInChildren)
			{
				if (!((Object)(object)baseEntity == (Object)(object)this))
				{
					furnitureSpawns.Add(new FurnitureSpawn
					{
						Prefab = baseEntity.PrefabName,
						LocalPosition = rootObj.transform.InverseTransformPoint(((Component)baseEntity).transform.position),
						LocalRotation = Quaternion.Inverse(rootObj.transform.rotation) * ((Component)baseEntity).transform.rotation,
						LocalScale = ((Component)baseEntity).transform.localScale
					});
					preProcess.DeleteGameObject(((Component)baseEntity).gameObject);
				}
			}
			val.SetActive(false);
		}
	}

	public static bool ArePlayersInsideSameHostileRoom(BasePlayer attacker, BasePlayer victim)
	{
		if ((Object)(object)attacker == (Object)null || (Object)(object)victim == (Object)null)
		{
			return false;
		}
		if ((Object)(object)attacker == (Object)(object)victim)
		{
			return true;
		}
		TriggerSafeZoneOverride triggerSafeZoneOverride = attacker.FindActiveCombatTrigger();
		TriggerSafeZoneOverride triggerSafeZoneOverride2 = victim.FindActiveCombatTrigger();
		if ((Object)(object)triggerSafeZoneOverride != (Object)null && (Object)(object)triggerSafeZoneOverride2 != (Object)null && (Object)(object)triggerSafeZoneOverride == (Object)(object)triggerSafeZoneOverride2)
		{
			if ((Object)(object)triggerSafeZoneOverride.Apartment != (Object)null && ApartmentCommands.apartmentinvisibleblocker && !triggerSafeZoneOverride.Apartment.IsBreakInActive() && !triggerSafeZoneOverride.Apartment.CanBypassInvisibleBarrier(attacker))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void OnPlayerEnterCombatZone(BasePlayer player)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (ApartmentCommands.apartmentinvisibleblocker && !CanBypassInvisibleBarrier(player) && IsCurrentlyRented() && !IsBreakInActive() && (!ApartmentCommands.adminapartmentnoclip || !player.IsAdmin || !player.IsFlying))
		{
			if ((Object)(object)invisibleBarrierEjectLocation != (Object)null)
			{
				player.Teleport(invisibleBarrierEjectLocation.position);
			}
			else
			{
				Debug.LogError((object)"Invisible barrier eject location is not set.");
			}
		}
	}

	public bool CanBypassInvisibleBarrier(BasePlayer player)
	{
		return IsAuthed(player.userID, ApartmentAuth.EnterApartment);
	}

	public bool CanBypassInvisibleBarrier(ulong player)
	{
		return IsAuthed(player, ApartmentAuth.EnterApartment);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
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
				Debug.Log((object)("SyncVar Writing: Building for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_Building);
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
				_ = __sync_Building;
				EntityRef<ApartmentBuilding> _sync_Building = NetworkReadEx.EntityRef<ApartmentBuilding>(reader);
				__sync_Building = _sync_Building;
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
		if (propertyName == "Building")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
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
		__sync_Building = default(EntityRef<ApartmentBuilding>);
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
