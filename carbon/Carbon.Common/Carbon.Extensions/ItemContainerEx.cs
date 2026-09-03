namespace Carbon.Extensions;

public static class ItemContainerEx
{
	public static int TakeSkinned(this ItemContainer container, int itemid, ulong skinId)
	{
		int num = 0;
		for (int i = 0; i < container.itemList.Count; i++)
		{
			Item val = container.itemList[i];
			if (val.info.itemid == itemid && val.skin == skinId)
			{
				num += val.amount;
			}
		}
		return num;
	}
}
