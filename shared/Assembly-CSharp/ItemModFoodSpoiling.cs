using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ItemModFoodSpoiling : ItemMod
{
	public class FoodSpoilingWorkQueue : PersistentObjectWorkQueue<Item>
	{
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
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)container.playerOwner != (Object)null)
			{
				pos = ((Component)container.playerOwner).transform.position;
				return true;
			}
			if (container.parent != null && (Object)(object)container.parent.GetWorldEntity() != (Object)null)
			{
				pos = ((Component)container.parent.GetWorldEntity()).transform.position;
				return true;
			}
			if (container.parent != null && container.parent.parent != null && container.parent.parent != null && depth > 0)
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
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			pos = Vector3.zero;
			if (item.parent != null && CheckItemParent(item.parent, 4, out pos))
			{
				return true;
			}
			if (item.parent != null && (Object)(object)item.parent.entityOwner != (Object)null)
			{
				pos = ((Component)item.parent.entityOwner).transform.position;
				return true;
			}
			if (item.parent != null && (Object)(object)item.parent.playerOwner != (Object)null)
			{
				pos = ((Component)item.parent.playerOwner).transform.position;
				return true;
			}
			return false;
		}

		public static void DeductTimeFromFoodItem(Item foodItem, float timeToApply, bool setDirty)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			if (foodItem.instanceData != null)
			{
				float dataFloat = foodItem.instanceData.dataFloat;
				float num = 1f;
				IFoodSpoilModifier foodSpoilModifier = default(IFoodSpoilModifier);
				if (foodItem.parent != null && (Object)(object)foodItem.parent.entityOwner != (Object)null && ((Component)foodItem.parent.entityOwner).TryGetComponent<IFoodSpoilModifier>(ref foodSpoilModifier))
				{
					num = foodSpoilModifier.GetSpoilMultiplier(foodItem);
				}
				if (num > 0f && GetWorldPositionForItem(foodItem, out var pos) && TerrainMeta.BiomeMap.GetBiome(pos, 8) > 0f)
				{
					num = 0f;
				}
				bool flag = num != 1f;
				if (foodItem.HasFlag(Item.Flag.Refrigerated) != flag)
				{
					foodItem.SetFlag(Item.Flag.Refrigerated, flag);
					foodItem.MarkDirty();
					if ((Object)(object)foodItem.GetEntityOwner() != (Object)null)
					{
						foodItem.GetEntityOwner().SendNetworkUpdate();
					}
				}
				InstanceData instanceData = foodItem.instanceData;
				instanceData.dataFloat -= timeToApply * num;
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
						item.Drop(((Component)parent.entityOwner).transform.position + Vector3.up * ((Bounds)(ref parent.entityOwner.bounds)).size.y, Vector3.zero);
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

	public static void DeductTimeFromAll(TimeSpan span)
	{
		((PersistentObjectWorkQueue<Item>)foodSpoilItems).RunOnAll((Action<Item>)delegate(Item foodItem)
		{
			FoodSpoilingWorkQueue.DeductTimeFromFoodItem(foodItem, (float)span.TotalSeconds, setDirty: true);
		});
	}
}
