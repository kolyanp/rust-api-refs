using ConVar;
using ProtoBuf;
using UnityEngine;

public class ItemModConditionRefrigeratedTime : ItemMod
{
	private static float RequiredSeconds => Server.creamSeparationHours * 60f * 60f;

	public override bool Passes(Item item)
	{
		return IsReady(item);
	}

	public static void AddRefrigeratedTime(Item item, float seconds)
	{
		if (item.instanceData == null || seconds <= 0f)
		{
			return;
		}
		bool flag = IsReady(item);
		InstanceData instanceData = item.instanceData;
		instanceData.refrigeratedSeconds += seconds;
		if (!flag && IsReady(item))
		{
			item.MarkDirty();
			if ((Object)(object)item.GetEntityOwner() != (Object)null)
			{
				item.GetEntityOwner().SendNetworkUpdate();
			}
		}
	}

	private static bool IsReady(Item item)
	{
		if (item.instanceData == null)
		{
			return false;
		}
		return item.instanceData.refrigeratedSeconds >= RequiredSeconds;
	}

	private void OnValidate()
	{
		ItemDefinition componentInParent = ((Component)this).GetComponentInParent<ItemDefinition>(true);
		if (!((Object)(object)componentInParent == (Object)null) && (Object)(object)((Component)componentInParent).GetComponentInChildren<ItemModFoodSpoiling>(true) == (Object)null)
		{
			Debug.LogWarning((object)"ItemModConditionRefrigeratedTime needs ItemModFoodSpoiling on the item to accumulate time!", (Object)(object)((Component)this).gameObject);
		}
	}
}
