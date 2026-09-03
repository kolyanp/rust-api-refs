using System;
using API.Hooks;
using Network;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Fun
{
	public class Fun_AnimalBrain
	{
		[Patch("OnChickenScared", "OnChickenScared", typeof(FleeState), "StateEnter", new Type[]
		{
			typeof(BaseAIBrain),
			typeof(BaseEntity)
		})]
		[Info("Gets called whenever a chicken gets scared due to the presence of another potential threat.")]
		[Parameter("chicken", typeof(Chicken), false)]
		[Parameter("threat", typeof(BaseEntity), false)]
		[Return(typeof(void), Discarded = true)]
		public class OnChickenScared : Patch
		{
			public static void Postfix(BaseAIBrain brain)
			{
				try
				{
					if ((Object)(object)brain != (Object)null)
					{
						BaseEntity baseEntity = ((EntityComponent<BaseEntity>)(object)brain).baseEntity;
						Chicken val = (Chicken)(object)((baseEntity is Chicken) ? baseEntity : null);
						if (val != null)
						{
							HookCaller.CallStaticHook(1990146717u, (object)val, (object)brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot));
						}
					}
				}
				catch
				{
				}
			}
		}

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

	public class Fun_BasePlayer
	{
		[Patch("OnJackieChan", "OnJackieChan", typeof(BasePlayer), "PlayerInit", new Type[] { typeof(Connection) })]
		[Parameter("player", typeof(BasePlayer), false)]
		[Info("Checks if player that connected is Jackie Chan.")]
		[Return(typeof(void), Discarded = true)]
		public class OnJackieChan : Patch
		{
			public static void Prefix(Connection c)
			{
				try
				{
					MonoBehaviour player = c.player;
					BasePlayer val = (BasePlayer)(object)((player is BasePlayer) ? player : null);
					if (val.displayName == "Jackie Chan")
					{
						HookCaller.CallStaticHook(3530583763u, (object)val);
					}
				}
				catch
				{
				}
			}
		}
	}
}
