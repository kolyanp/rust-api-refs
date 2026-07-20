using Oxide.Core.Libraries;
using Oxide.Game.Rust.Libraries.Covalence;

namespace Oxide.Game.Rust.Libraries;

public class Item : Library
{
	internal static readonly RustCovalenceProvider Covalence = RustCovalenceProvider.Instance;

	public static global::Item GetItem(int itemId)
	{
		return ItemManager.CreateByItemID(itemId, 1, 0uL, 0uL);
	}
}
