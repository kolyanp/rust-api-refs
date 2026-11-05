using ConVar;
using Oxide.Core;
using UnityEngine;

public class DropUtil
{
	public static void DropItems(ItemContainer container, Vector3 position, bool scatter = true)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		if (!Server.dropitems || container == null || container.itemList == null || Interface.CallHook("OnContainerDropItems", container) != null)
		{
			return;
		}
		float num = (scatter ? 0.25f : 0f);
		float num2 = (scatter ? 1f : 0f);
		Item[] array = container.itemList.ToArray();
		foreach (Item item in array)
		{
			float num3 = (scatter ? Random.Range(0f, 2f) : 0f);
			item.RemoveFromContainer();
			BaseEntity baseEntity = item.CreateWorldObject(position + new Vector3(Random.Range(0f - num, num), num2, Random.Range(0f - num, num)));
			if ((Object)(object)baseEntity == (Object)null)
			{
				item.Remove();
				continue;
			}
			if (baseEntity is DroppedItem droppedItem && container.entityOwner is LootContainer)
			{
				droppedItem.DropReason = DroppedItem.DropReasonEnum.Loot;
			}
			if (num3 > 0f)
			{
				baseEntity.SetVelocity(new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f)) * num3);
				baseEntity.SetAngularVelocity(new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)) * num3);
			}
		}
	}
}
