using Oxide.Core.Libraries;

namespace Oxide.Game.Rust.Libraries;

public class Item : Library
{
	public static Item GetItem(int itemId)
	{
		return ItemManager.CreateByItemID(itemId, 1, 0uL, 0uL);
	}
}
