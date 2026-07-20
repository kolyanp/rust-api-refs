using Facepunch.Rust;
using UnityEngine;

public class ItemModCookable : ItemMod
{
	[ItemSelector]
	public ItemDefinition becomeOnCooked;

	public float cookTime = 30f;

	public int amountOfBecome = 1;

	public int lowTemp;

	public int highTemp;

	public bool setCookingFlag;

	public void OnValidate()
	{
		if (amountOfBecome < 1)
		{
			amountOfBecome = 1;
		}
		if ((Object)(object)becomeOnCooked == (Object)null)
		{
			Debug.LogWarning((object)("[ItemModCookable] becomeOnCooked is unset! [" + ((Object)this).name + "]"), (Object)(object)((Component)this).gameObject);
		}
	}

	public bool CanBeCookedByAtTemperature(float temperature)
	{
		if (temperature > (float)lowTemp)
		{
			return temperature < (float)highTemp;
		}
		return false;
	}

	private void CycleCooking(Item item, float delta)
	{
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("ItemModCookable:CycleCooking"))
		{
			if (!CanBeCookedByAtTemperature(item.temperature) || item.cookTimeLeft < 0f)
			{
				if (setCookingFlag && item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
				return;
			}
			if (setCookingFlag && !item.HasFlag(Item.Flag.Cooking))
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
			int num4 = 1 + Mathf.FloorToInt(num3 / cookTime);
			item.cookTimeLeft = cookTime - num3 % cookTime;
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
			if (!((Object)(object)becomeOnCooked != (Object)null))
			{
				return;
			}
			int num5 = amountOfBecome * num4;
			bool flag = false;
			foreach (Item item3 in item.parent.itemList)
			{
				if ((Object)(object)becomeOnCooked == (Object)(object)item3.info && item3.amount + num5 < becomeOnCooked.stackable)
				{
					item3.amount += num5;
					item3.MarkDirty();
					flag = true;
					break;
				}
			}
			Analytics.Azure.AddPendingItems(baseOven, becomeOnCooked.shortname, num5, "smelt", consumed: false);
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
							IntIdentifier = becomeOnCooked.itemid,
							WorldPosition = ((Component)item.parent.entityOwner).transform.position,
							NetworkIdentifier = item.parent.entityOwner.net.ID
						}, num5);
					}
				}
			}
			if (flag)
			{
				return;
			}
			Item item2 = ItemManager.Create(becomeOnCooked, num5, 0uL, isServerSide: true, 0uL);
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
		itemcreated.onCycle += CycleCooking;
	}
}
