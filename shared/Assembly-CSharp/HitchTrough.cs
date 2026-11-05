using System;
using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

public class HitchTrough : StorageContainer
{
	[Serializable]
	public class HitchSpot
	{
		public HitchTrough owner;

		public Transform tr;

		public EntityRef hitchableEntRef;

		public IHitchable GetHitchable(bool isServer = true)
		{
			return hitchableEntRef.Get(isServer) as IHitchable;
		}

		public bool IsOccupied(bool isServer = true)
		{
			return hitchableEntRef.IsValid(isServer);
		}

		public void SetOccupiedBy(IHitchable hitchable)
		{
			if (hitchable == null)
			{
				hitchableEntRef.Set(null);
				return;
			}
			BaseEntity baseEntity = hitchable as BaseEntity;
			if ((Object)(object)baseEntity != (Object)null)
			{
				hitchableEntRef.Set(baseEntity);
			}
		}
	}

	public interface IHitchable
	{
		void SetHitch(HitchTrough hitch, HitchSpot spot);
	}

	public HitchSpot[] hitchSpots;

	public float caloriesToDecaySeconds = 36f;

	[Header("Water")]
	public GameObjectRef waterStoragePrefab;

	[Tooltip("Where the water container sits, in the trough's local space.")]
	public Transform waterStorageAnchor;

	private const float SupplyRefreshSeconds = 60f;

	private const float SupplyResendShare = 0.05f;

	private LiquidContainer waterStorage;

	private float supplyFoodMinutes;

	private TimeSince sinceSupplyMeasured;

	private int supplyHeads;

	private Action refreshSupplyWhileOpen;

	public LiquidContainer WaterStorage => waterStorage;

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		foreach (BaseEntity child in children)
		{
			if (!(child is LiquidContainer))
			{
				pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
				return false;
			}
		}
		return base.CanCompletePickup(player);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		bool flag = false;
		if (!Application.isLoadingSave && !flag)
		{
			SpawnWaterStorage(load: false);
		}
	}

	public override void PostServerLoad()
	{
		SpawnWaterStorage(load: true);
		HitchSpot[] array = hitchSpots;
		foreach (HitchSpot hitchSpot in array)
		{
			AttemptToHitch(hitchSpot.GetHitchable(), hitchSpot);
		}
	}

	private void SpawnWaterStorage(bool load)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (load)
		{
			foreach (BaseEntity child in children)
			{
				if (child is LiquidContainer liquidContainer)
				{
					waterStorage = liquidContainer;
					return;
				}
			}
		}
		if (!((Object)(object)waterStorage != (Object)null))
		{
			waterStorage = GameManager.server.CreateEntity(waterStoragePrefab.resourcePath, waterStorageAnchor.localPosition, waterStorageAnchor.localRotation) as LiquidContainer;
			if ((Object)(object)waterStorage == (Object)null)
			{
				Debug.LogError((object)$"Could not create the water container for {this}", (Object)(object)((Component)this).gameObject);
				return;
			}
			waterStorage.SetParent(this);
			waterStorage.Spawn();
		}
	}

	public Item GetWaterItem()
	{
		if (!((Object)(object)waterStorage != (Object)null))
		{
			return null;
		}
		return waterStorage.GetLiquidItem();
	}

	public bool HasWater()
	{
		return GetWaterItem() != null;
	}

	public bool TryConsumeWater(int amount)
	{
		Item waterItem = GetWaterItem();
		if (waterItem == null || waterItem.amount < amount)
		{
			return false;
		}
		waterItem.UseItem(amount);
		return true;
	}

	public override void DestroyShared()
	{
		if (isServer)
		{
			UnHitchAll();
		}
		base.DestroyShared();
	}

	public Item GetFoodItem()
	{
		foreach (Item item in inventory.itemList)
		{
			if (item.info.category == ItemCategory.Food && Object.op_Implicit((Object)(object)((Component)item.info).GetComponent<ItemModConsumable>()))
			{
				return item;
			}
		}
		return null;
	}

	public Item GetLivestockFoodItem()
	{
		foreach (Item item in inventory.itemList)
		{
			if (LivestockAnimal.IsLivestockFood(item.info))
			{
				return item;
			}
		}
		return null;
	}

	public bool HasLivestockFood()
	{
		return GetLivestockFoodItem() != null;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added, BasePlayer sourcePlayer)
	{
		base.OnItemAddedOrRemoved(item, added, sourcePlayer);
		OnFoodChanged();
	}

	public override void OnItemAddedToStack(Item item, int amount, BasePlayer sourcePlayer)
	{
		base.OnItemAddedToStack(item, amount, sourcePlayer);
		OnFoodChanged();
	}

	public override void OnItemRemovedFromStack(Item item, int amount, BasePlayer sourcePlayer)
	{
		base.OnItemRemovedFromStack(item, amount, sourcePlayer);
		OnFoodChanged();
	}

	private void OnFoodChanged()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal.MarkSupplyDirtyNear(((Component)this).transform.position);
		if (refreshSupplyWhileOpen != null && IsInvoking(refreshSupplyWhileOpen))
		{
			RefreshSupplyEstimate();
			SendNetworkUpdate();
		}
	}

	public LivestockFloor FloorInReach()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			Query.Server.GetBrainsInSphere(((Component)this).transform.position, 27f, (List<LivestockAnimal>)(object)val);
			LivestockFloor livestockFloor = null;
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if ((Object)(object)item == (Object)null || item.IsDead())
				{
					continue;
				}
				LivestockFloor floor = item.Floor;
				if (floor != null && floor != livestockFloor)
				{
					if (floor.Reaches(this))
					{
						return floor;
					}
					livestockFloor = floor;
				}
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void RefreshSupplyEstimate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		LivestockFloor.SupplyEstimate supplyEstimate = FloorInReach()?.Supply() ?? default(LivestockFloor.SupplyEstimate);
		supplyFoodMinutes = supplyEstimate.FoodMinutes;
		supplyHeads = supplyEstimate.Heads;
		sinceSupplyMeasured = TimeSince.op_Implicit(0f);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		RefreshSupplyEstimate();
		if (!base.PlayerOpenLoot(player, panelToOpen, doPositionChecks))
		{
			return false;
		}
		if (refreshSupplyWhileOpen == null)
		{
			refreshSupplyWhileOpen = RefreshSupplyWhileOpen;
		}
		if (!IsInvoking(refreshSupplyWhileOpen))
		{
			InvokeRepeating(refreshSupplyWhileOpen, 60f, 60f);
		}
		return true;
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		if (refreshSupplyWhileOpen != null && !IsLootedByAnyoneBut(player))
		{
			CancelInvoke(refreshSupplyWhileOpen);
		}
	}

	private bool IsLootedByAnyoneBut(BasePlayer player)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if ((Object)(object)current != (Object)(object)player && (Object)(object)current.inventory.loot.entitySource == (Object)(object)this)
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

	private void RefreshSupplyWhileOpen()
	{
		int num = supplyHeads;
		float num2 = supplyFoodMinutes;
		RefreshSupplyEstimate();
		if (num != supplyHeads || Mathf.Abs(supplyFoodMinutes - num2) > num2 * 0.05f)
		{
			SendNetworkUpdate();
		}
	}

	public bool HasSpace()
	{
		HitchSpot[] array = hitchSpots;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsOccupied())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValidHitchPosition(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetClosestSpot(pos, includeOccupied: false, 1f) != null;
	}

	public bool AttemptToHitch(IHitchable hitchable, HitchSpot spot = null)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (hitchable == null)
		{
			return false;
		}
		if (spot == null)
		{
			BaseEntity baseEntity = hitchable as BaseEntity;
			if ((Object)(object)baseEntity != (Object)null)
			{
				spot = GetClosestSpot(((Component)baseEntity).transform.position);
			}
		}
		if (spot != null)
		{
			object obj = Interface.CallHook("OnHorseHitch", hitchable, spot);
			if (obj is bool)
			{
				return (bool)obj;
			}
			spot.SetOccupiedBy(hitchable);
			hitchable.SetHitch(this, spot);
			return true;
		}
		return false;
	}

	public HitchSpot GetClosestSpot(Vector3 testPos, bool includeOccupied = false, float maxRadius = -1f)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = 10000f;
		HitchSpot result = null;
		for (int i = 0; i < hitchSpots.Length; i++)
		{
			float num2 = Vector3.Distance(testPos, hitchSpots[i].tr.position);
			if (num2 < num && (maxRadius == -1f || num2 <= maxRadius) && (includeOccupied || !hitchSpots[i].IsOccupied()))
			{
				num = num2;
				result = hitchSpots[i];
			}
		}
		return result;
	}

	public void UnHitch(IHitchable hitchable)
	{
		HitchSpot[] array = hitchSpots;
		foreach (HitchSpot hitchSpot in array)
		{
			if (hitchSpot.GetHitchable(isServer) == hitchable)
			{
				if (Interface.CallHook("OnHorseUnhitch", hitchable, hitchSpot) != null)
				{
					break;
				}
				hitchSpot.SetOccupiedBy(null);
				hitchable.SetHitch(null, null);
			}
		}
	}

	public void UnHitchAll()
	{
		HitchSpot[] array = hitchSpots;
		for (int i = 0; i < array.Length; i++)
		{
			IHitchable hitchable = array[i].GetHitchable();
			if (hitchable != null)
			{
				UnHitch(hitchable);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.ioEntity = Pool.Get<IOEntity>();
		info.msg.ioEntity.genericEntRef1 = hitchSpots[0].hitchableEntRef.uid;
		info.msg.ioEntity.genericEntRef2 = hitchSpots[1].hitchableEntRef.uid;
		if (!info.forDisk)
		{
			info.msg.ioEntity.genericFloat1 = Mathf.Max(0f, supplyFoodMinutes - sinceSupplyMeasured.PassedSince(info.cachedTime.Time) / 60f);
			info.msg.ioEntity.genericInt1 = supplyHeads;
			info.msg.ioEntity.genericInt2 = (HasAttachedStorageAdaptor(out var _) ? 1 : 0);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			hitchSpots[0].hitchableEntRef.uid = info.msg.ioEntity.genericEntRef1;
			hitchSpots[1].hitchableEntRef.uid = info.msg.ioEntity.genericEntRef2;
		}
	}
}
