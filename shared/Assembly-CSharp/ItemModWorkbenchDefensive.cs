using UnityEngine;

public class ItemModWorkbenchDefensive : ItemModWorkbenchUpgrade
{
	[Tooltip("Number of bonus armor insert slots added to the normal roll.")]
	[Header("Defensive")]
	public int bonusSlots = 1;

	public override void ApplyToCraftedItem(Workbench workbench, BasePlayer crafter, ItemCraftTask task, Item craftedItem, Item upgradeItem)
	{
		ItemModContainerArmorSlot component = ((Component)craftedItem.info).GetComponent<ItemModContainerArmorSlot>();
		if (!((Object)(object)component == (Object)null))
		{
			int num = craftedItem.contents?.capacity ?? 0;
			int num2 = Mathf.Min(num + bonusSlots, component.MaxSlots);
			if (num2 > num)
			{
				component.SetSlotAmount(craftedItem, num2);
			}
		}
	}
}
