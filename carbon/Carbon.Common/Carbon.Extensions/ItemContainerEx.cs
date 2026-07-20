namespace Carbon.Extensions;

public static class ItemContainerEx
{
	public static int TakeSkinned(this ItemContainer container, int itemid, ulong skinId, bool onlyUsableAmounts)
	{
		int num = 0;
		foreach (Item item in container.itemList)
		{
			if (item.info.itemid == itemid && item.skin == skinId && (!onlyUsableAmounts || !item.IsBusy()))
			{
				num += item.amount;
			}
		}
		return num;
	}
}
