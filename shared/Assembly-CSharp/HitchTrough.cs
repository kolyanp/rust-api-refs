using System;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
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
	[Tooltip("The liquid container spawned into the water end of the trough. It owns the water, the trough owns the food.")]
	public GameObjectRef waterStoragePrefab;

	[Tooltip("Where the water container sits, in the trough's local space.")]
	public Transform waterStorageAnchor;

	private LiquidContainer waterStorage;

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
		ItemModConsumable itemModConsumable = default;
		foreach (Item item in inventory.itemList)
		{
			if (((Component)item.info).TryGetComponent<ItemModConsumable>(ref itemModConsumable) && itemModConsumable.chickenCoopFood)
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
