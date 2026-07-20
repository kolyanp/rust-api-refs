using System.Collections.Generic;
using Facepunch;

namespace Oxide.Core.Plugins;

public static class Extensions
{
	public static void Clear(this ItemContainer cont)
	{
		List<Item> list = Pool.Get<List<Item>>();
		list.AddRange(cont.itemList);
		foreach (Item item in list)
		{
			item.Remove(0.1f);
		}
		ItemManager.DoRemoves(false);
		Pool.FreeUnmanaged<Item>(ref list);
	}
}
