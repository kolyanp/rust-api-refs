using UnityEngine;

public class ChristmasTree : StorageContainer
{
	public GameObject[] decorations;

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if ((Object)(object)((Component)item.info).GetComponent<ItemModXMasTreeDecoration>() == (Object)null)
		{
			return false;
		}
		foreach (Item item2 in base.inventory.itemList)
		{
			if ((Object)(object)item2.info == (Object)(object)item.info)
			{
				return false;
			}
		}
		return base.ItemFilter(player, item, targetSlot);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		ItemModXMasTreeDecoration component = ((Component)item.info).GetComponent<ItemModXMasTreeDecoration>();
		if ((Object)(object)component != (Object)null)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set((Flags)component.flagsToChange, added);
		}
		base.OnItemAddedOrRemoved(item, added);
	}
}
