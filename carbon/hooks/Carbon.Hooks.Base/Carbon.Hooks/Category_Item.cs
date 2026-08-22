using API.Hooks;
using Carbon.Core;

namespace Carbon.Hooks;

public class Category_Item
{
	public class Item_OnLoseCondition
	{
		[Patch("OnLoseCondition", "OnLoseCondition", typeof(CorePlugin), "IOnLoseCondition", null)]
		[Options(/*Could not decode attribute arguments.*/)]
		[Info("Called right before the condition of the item is modified.")]
		[Parameter("item", typeof(Item), false)]
		[Parameter("amount", typeof(float), false, ByRef = true)]
		[Return(typeof(void), Discarded = true)]
		[OxideCompatible]
		public class OnLoseCondition : Patch
		{
		}
	}
}
