using System;
using API.Hooks;

namespace Carbon.Hooks;

public class Category_Fun
{
	public class Fun_AnimalBrain
	{
		[Patch("CanAcceptBackpackItem", "CanAcceptBackpackItem", typeof(ItemModBackpack), "CanAcceptItem", new Type[]
		{
			typeof(BasePlayer),
			typeof(Item),
			typeof(Item),
			typeof(int)
		})]
		[Info("Gets called whenever attempting to place an item in a backpack item, overriding returning output.")]
		[Parameter("backpack", typeof(Item), false)]
		[Parameter("item", typeof(Item), false)]
		[Return(typeof(bool))]
		public class CanAcceptBackpackItem : Patch
		{
			public static bool Prefix(BasePlayer player, Item backpack, Item item, int slot, ref bool __result)
			{
				if (!(HookCaller.CallStaticHook(2306141762u, (object)backpack, (object)item) is bool flag))
				{
					return true;
				}
				__result = flag;
				return false;
			}
		}
	}
}
