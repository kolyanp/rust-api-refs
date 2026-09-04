using Facepunch.Rust;
using UnityEngine;

public class ItemModCookable : ItemMod
{
	[ItemSelector]
	public ItemDefinition becomeOnCooked;

	public float cookTime = 30f;

	public float amountOfBecome = 1f;

	public int lowTemp;

	public int highTemp;

	public bool setCookingFlag;

	public void OnValidate()
	{
		if (amountOfBecome < 1f)
		{
			amountOfBecome = 1f;
		}
		if ((Object)(object)becomeOnCooked == (Object)null)
		{
			Debug.LogWarning((object)("[ItemModCookable] becomeOnCooked is unset! [" + ((Object)this).name + "]"), (Object)(object)((Component)this).gameObject);
		}
	}

	public static void CycleCooking(Item item, float delta)
	{
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		CookableItemInfo cookableItemInfo = item.info.ItemModCookable;
		if (item.GetEntityOwner() is Composter)
		{
			cookableItemInfo = item.info.ItemModCompostable;
		}
		if (cookableItemInfo == null)
		{
			return;
		}
		using (TimeWarning.New("ItemModCookable:CycleCooking"))
		{
			if (!cookableItemInfo.CanBeCookedByAtTemperature(item.temperature) || item.cookTimeLeft < 0f)
			{
				if (cookableItemInfo.setCookingFlag && item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
				return;
			}
			if (cookableItemInfo.setCookingFlag && !item.HasFlag(Item.Flag.Cooking))
			{
				item.SetFlag(Item.Flag.Cooking, b: true);
				item.MarkDirty();
			}
			int num = Mathf.FloorToInt(item.cookTimeLeft / 5f);
			item.cookTimeLeft -= delta;
			if (item.cookTimeLeft > 0f)
			{
				int num2 = Mathf.FloorToInt(item.cookTimeLeft / 5f);
				if (num != num2)
				{
					item.MarkDirty();
				}
				return;
			}
			float num3 = item.cookTimeLeft * -1f;
			int num4 = 1 + Mathf.FloorToInt(num3 / cookableItemInfo.cookTime);
			item.cookTimeLeft = cookableItemInfo.cookTime - num3 % cookableItemInfo.cookTime;
			BaseOven baseOven = item.GetEntityOwner() as BaseOven;
			num4 = Mathf.Min(num4, item.amount);
			if (item.amount > num4)
			{
				item.amount -= num4;
				item.MarkDirty();
			}
			else
			{
				item.Remove();
			}
			Analytics.Azure.AddPendingItems(baseOven, item.info.shortname, num4, "smelt");
			if ((Object)(object)cookableItemInfo.becomeOnCooked == (Object)null)
			{
				return;
			}
			float num5 = cookableItemInfo.amountOfBecome * (float)num4;
			int num6 = Mathf.FloorToInt(num5);
			if (num5 != (float)num6)
			{
				float num7 = num5 - (float)num6;
				if (Random.value < num7)
				{
					num6++;
				}
			}
			if (num6 == 0)
			{
				return;
			}
			bool flag = false;
			foreach (Item item3 in item.parent.itemList)
			{
				if ((Object)(object)cookableItemInfo.becomeOnCooked == (Object)(object)item3.info && item3.amount + num6 < cookableItemInfo.becomeOnCooked.stackable)
				{
					item3.amount += num6;
					item3.MarkDirty();
					flag = true;
					break;
				}
			}
			Analytics.Azure.AddPendingItems(baseOven, cookableItemInfo.becomeOnCooked.shortname, num6, "smelt", consumed: false);
			if ((Object)(object)item.parent.entityOwner != (Object)null && item.parent.entityOwner.net.group.restricted)
			{
				TutorialIsland closestTutorialIsland = TutorialIsland.GetClosestTutorialIsland(((Component)item.parent.entityOwner).transform.position, 50f);
				if ((Object)(object)closestTutorialIsland != (Object)null)
				{
					BasePlayer basePlayer = closestTutorialIsland.ForPlayer.Get(serverside: true);
					if ((Object)(object)basePlayer != (Object)null)
					{
						basePlayer.ProcessMissionEvent(BaseMission.MissionEventType.COOK, new BaseMission.MissionEventPayload
						{
							IntIdentifier = cookableItemInfo.becomeOnCooked.itemid,
							WorldPosition = ((Component)item.parent.entityOwner).transform.position,
							NetworkIdentifier = item.parent.entityOwner.net.ID
						}, num6);
					}
				}
			}
			if (flag)
			{
				return;
			}
			Item item2 = ItemManager.Create(cookableItemInfo.becomeOnCooked, num6, 0uL, isServerSide: true, 0uL);
			if (item2 != null && !item2.MoveToContainer(item.parent) && !item2.MoveToContainer(item.parent))
			{
				item2.Drop(item.parent.dropPosition, item.parent.dropVelocity);
				if (Object.op_Implicit((Object)(object)item.parent.entityOwner) && (Object)(object)baseOven != (Object)null)
				{
					baseOven.OvenFull();
				}
			}
		}
	}

	public override void OnItemCreated(Item itemcreated)
	{
		itemcreated.cookTimeLeft = cookTime;
		SubscribeCycleCooking(itemcreated);
	}

	public static void SubscribeCycleCooking(Item item)
	{
		item.onCycle -= CycleCooking;
		item.onCycle += CycleCooking;
	}
}
