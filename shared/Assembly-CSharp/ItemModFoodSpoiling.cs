using System;
using System.Collections.Generic;
using ConVar;
using Development.Attributes;
using Facepunch;
using ProtoBuf;
using UnityEngine;

[ResetStaticFields]
public class ItemModFoodSpoiling : ItemMod
{
	public class FoodSpoilingWorkQueue : PersistentObjectWorkQueue<Item>
	{
		private const float ArcticRefrigerationThreshold = 0.1f;

		private Dictionary<ItemId, TimeSince> lastUpdated = new Dictionary<ItemId, TimeSince>();

		protected override void RunJob(Item foodItem)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			float timeToApply = 0f;
			if (lastUpdated.TryGetValue(foodItem.uid, out var value))
			{
				timeToApply = TimeSince.op_Implicit(value);
				lastUpdated[foodItem.uid] = TimeSince.op_Implicit(0f);
			}
			else
			{
				lastUpdated.Add(foodItem.uid, TimeSince.op_Implicit(0f));
			}
			DeductTimeFromFoodItem(foodItem, timeToApply, setDirty: false);
		}

		private static bool CheckItemParent(ItemContainer container, int depth, out Vector3 pos)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)container.playerOwner != (Object)null)
			{
				pos = ((Component)container.playerOwner).transform.position;
				return true;
			}
			if ((Object)(object)container.entityOwner != (Object)null)
			{
				pos = ((Component)container.entityOwner).transform.position;
				return true;
			}
			if (container.parent != null && (Object)(object)container.parent.GetWorldEntity() != (Object)null)
			{
				pos = ((Component)container.parent.GetWorldEntity()).transform.position;
				return true;
			}
			if (container.parent != null && container.parent.parent != null && depth > 0)
			{
				return CheckItemParent(container.parent.parent, depth - 1, out pos);
			}
			pos = Vector3.zero;
			return false;
		}

		private static bool GetWorldPositionForItem(Item item, out Vector3 pos)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			pos = Vector3.zero;
			if (item.parent != null)
			{
				return CheckItemParent(item.parent, 4, out pos);
			}
			return false;
		}

		public static void DeductTimeFromFoodItem(Item foodItem, float timeToApply, bool setDirty)
		{
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			if (foodItem.instanceData != null)
			{
				float dataFloat = foodItem.instanceData.dataFloat;
				RefreshRefrigeration(foodItem, out var spoilMultiplier);
				if (spoilMultiplier < 1f)
				{
					ItemModConditionRefrigeratedTime.AddRefrigeratedTime(foodItem, timeToApply);
				}
				if (Server.foodSpoiling)
				{
					InstanceData instanceData = foodItem.instanceData;
					instanceData.dataFloat -= timeToApply * spoilMultiplier;
				}
				if (!(foodItem.instanceData.dataFloat <= 0f) || !(dataFloat > 0f))
				{
					return;
				}
				int amount = foodItem.amount;
				ItemContainer parent = foodItem.parent;
				foodItem.RemoveFromContainer();
				Item item = ItemManager.Create(((Component)foodItem.info).GetComponent<ItemModFoodSpoiling>().SpoilItem, amount, 0uL, isServerSide: true, 0uL);
				if (parent != null && !parent.GiveItem(item))
				{
					if ((Object)(object)parent.entityOwner != (Object)null)
					{
						item.Drop(((Component)parent.entityOwner).transform.position + Vector3.up * parent.entityOwner.bounds.size.y, Vector3.zero);
					}
					else
					{
						item.Remove();
					}
				}
				else if (item.parent == null)
				{
					BaseEntity worldEntity = foodItem.GetWorldEntity();
					if ((Object)(object)worldEntity != (Object)null)
					{
						item.Drop(((Component)worldEntity).transform.position, Vector3.zero, ((Component)worldEntity).transform.rotation);
					}
					else
					{
						item.Remove();
					}
				}
				foodItem.Remove();
				ItemManager.DoRemoves();
			}
			else if (setDirty)
			{
				foodItem.MarkDirty();
				if ((Object)(object)foodItem.GetEntityOwner() != (Object)null)
				{
					foodItem.GetEntityOwner().SendNetworkUpdate();
				}
			}
		}

		private static float GetContainerSpoilMultiplier(Item item)
		{
			if (item.parent == null || (Object)(object)item.parent.entityOwner == (Object)null)
			{
				return 1f;
			}
			IFoodSpoilModifier foodSpoilModifier = default;
			if (!((Component)item.parent.entityOwner).TryGetComponent<IFoodSpoilModifier>(ref foodSpoilModifier))
			{
				return 1f;
			}
			return foodSpoilModifier.GetSpoilMultiplier(item);
		}

		public static void RefreshRefrigeration(Item item, out float spoilMultiplier)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			spoilMultiplier = GetContainerSpoilMultiplier(item);
			if (spoilMultiplier > 0f && GetWorldPositionForItem(item, out var pos) && TerrainMeta.BiomeMap.GetBiome(pos, 8) > 0.1f)
			{
				spoilMultiplier = 0f;
			}
			bool flag = spoilMultiplier != 1f;
			if (item.HasFlag(Item.Flag.Refrigerated) != flag)
			{
				item.SetFlag(Item.Flag.Refrigerated, flag);
				item.MarkDirty();
				if ((Object)(object)item.GetEntityOwner() != (Object)null)
				{
					item.GetEntityOwner().SendNetworkUpdate();
				}
			}
		}
	}

	public float TotalSpoilTimeHours = 12f;

	public ItemDefinition SpoilItem;

	public static FoodSpoilingWorkQueue foodSpoilItems = new FoodSpoilingWorkQueue();

	public override void OnItemCreated(Item item)
	{
		base.OnItemCreated(item);
		if (item.instanceData == null)
		{
			item.instanceData = Pool.Get<InstanceData>();
			item.instanceData.dataFloat = 3600f * TotalSpoilTimeHours;
			item.instanceData.ShouldPool = false;
		}
		((PersistentObjectWorkQueue<Item>)foodSpoilItems).Add(item);
	}

	public override void OnRemove(Item item)
	{
		base.OnRemove(item);
		((PersistentObjectWorkQueue<Item>)foodSpoilItems).Remove(item);
	}

	public override void OnParentChanged(Item item)
	{
		base.OnParentChanged(item);
		if (item.parent != null)
		{
			FoodSpoilingWorkQueue.RefreshRefrigeration(item, out var _);
		}
	}

	public static void DeductTimeFromAll(TimeSpan span)
	{
		((PersistentObjectWorkQueue<Item>)foodSpoilItems).RunOnAll((Action<Item>)((Item foodItem) =>
		{
			FoodSpoilingWorkQueue.DeductTimeFromFoodItem(foodItem, (float)span.TotalSeconds, setDirty: true);
		}));
	}
}
