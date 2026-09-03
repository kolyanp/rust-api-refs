using System;
using API.Hooks;
using Carbon.Core;
using UnityEngine;

namespace Carbon.Hooks;

public class Category_Entity
{
	public class Entity_PatrolHelicopterAI
	{
		[Patch("CanPatrolHeliSeePlayer", "CanPatrolHeliSeePlayer", typeof(PatrolHelicopterAI), "PlayerVisible", new Type[] { typeof(BasePlayer) })]
		[Info("Can the Patrol Helicopter see the player.")]
		[Parameter("heli", typeof(PatrolHelicopterAI), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Return(typeof(bool))]
		public class CanPatrolHeliSeePlayer : Patch
		{
			public static bool Prefix(BasePlayer ply, ref PatrolHelicopterAI __instance, out bool __result)
			{
				if (HookCaller.CallStaticHook(2827558490u, (object)__instance, (object)ply) is bool flag)
				{
					__result = flag;
					return false;
				}
				__result = false;
				return true;
			}
		}
	}

	public class WeaponRack_Entity
	{
		[Patch("CanPickupAllFromRack", "CanPickupAllFromRack", typeof(WeaponRack), "GivePlayerAllWeapons", new Type[]
		{
			typeof(BasePlayer),
			typeof(int)
		})]
		[Info("Return false to prevent all weapons from being picked up from the rack.")]
		[Parameter("rack", typeof(WeaponRack), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("mountSlotIndex", typeof(int), false)]
		public class CanPickupAllFromRack : Patch
		{
			public static bool Prefix(BasePlayer player, int mountSlotIndex, WeaponRack __instance)
			{
				if ((Object)(object)player == (Object)null)
				{
					return false;
				}
				object obj = HookCaller.CallStaticHook(2434163304u, (object)__instance, (object)player, (object)mountSlotIndex);
				if (obj is bool)
				{
					return (bool)obj;
				}
				return true;
			}
		}

		[Patch("CanPickupFromRack", "CanPickupFromRack", typeof(WeaponRack), "GivePlayerWeapon", new Type[]
		{
			typeof(BasePlayer),
			typeof(int),
			typeof(int),
			typeof(bool),
			typeof(bool)
		})]
		[Info("Return false to prevent the weapon from being picked up.")]
		[Parameter("rack", typeof(WeaponRack), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("item", typeof(Item), false)]
		[Parameter("mountSlotIndex", typeof(int), false)]
		[Parameter("playerBeltIndex", typeof(int), false)]
		[Parameter("tryHold", typeof(bool), false)]
		public class CanPickupFromRack : Patch
		{
			internal static bool _hasPickedUp;

			internal static Item _currentItem;

			public static bool Prefix(BasePlayer player, int mountSlotIndex, int playerBeltIndex, bool tryHold, bool sendUpdate, WeaponRack __instance)
			{
				if ((Object)(object)player == (Object)null)
				{
					return false;
				}
				WeaponRackSlot weaponAtIndex = __instance.GetWeaponAtIndex(mountSlotIndex);
				if (weaponAtIndex == null)
				{
					return false;
				}
				Item slot = ((StorageContainer)__instance).inventory.GetSlot(weaponAtIndex.InventoryIndex);
				if (slot == null)
				{
					return false;
				}
				_currentItem = slot;
				if (HookCaller.CallStaticHook(2780342367u, (object)__instance, (object)player, (object)_currentItem, (object)mountSlotIndex, (object)playerBeltIndex, (object)tryHold) is bool flag)
				{
					_hasPickedUp = flag;
					return flag;
				}
				_hasPickedUp = true;
				return true;
			}

			public static void Postfix(BasePlayer player, int mountSlotIndex, int playerBeltIndex, bool tryHold, bool sendUpdate, WeaponRack __instance)
			{
				if (_hasPickedUp)
				{
					HookCaller.CallStaticHook(3671231874u, (object)__instance, (object)player, (object)_currentItem, (object)mountSlotIndex, (object)playerBeltIndex, (object)tryHold);
				}
			}
		}

		[Patch("CanPlaceOnRack", "CanPlaceOnRack", typeof(WeaponRack), "MountWeapon", new Type[]
		{
			typeof(Item),
			typeof(BasePlayer),
			typeof(int),
			typeof(int),
			typeof(bool)
		})]
		[Info("Return a bool to prevent the weapon from being placed.")]
		[Parameter("rack", typeof(WeaponRack), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("item", typeof(Item), false)]
		[Parameter("gridCellIndex", typeof(int), false)]
		[Parameter("rotation", typeof(int), false)]
		[Return(typeof(bool))]
		public class CanPlaceOnRack : Patch
		{
			public static bool Prefix(Item item, BasePlayer player, int gridCellIndex, int rotation, bool sendUpdate, ref bool __result, WeaponRack __instance)
			{
				if (HookCaller.CallStaticHook(1860967996u, (object)__instance, (object)player, (object)item, (object)gridCellIndex, (object)rotation) is bool flag)
				{
					__result = flag;
					return false;
				}
				return true;
			}
		}

		[Patch("OnPickupFromRack", "OnPickupFromRack", typeof(WeaponRack), "GivePlayerWeapon", new Type[]
		{
			typeof(BasePlayer),
			typeof(int),
			typeof(int),
			typeof(bool),
			typeof(bool)
		})]
		[Dependencies(new string[] { "CanPickupFromRack" })]
		[Info("Whenever a weapon was picked up from the rack.")]
		[Parameter("rack", typeof(WeaponRack), false)]
		[Parameter("player", typeof(BasePlayer), false)]
		[Parameter("item", typeof(Item), false)]
		[Parameter("mountSlotIndex", typeof(int), false)]
		[Parameter("playerBeltIndex", typeof(int), false)]
		[Parameter("tryHold", typeof(bool), false)]
		[Return(typeof(void), Discarded = true)]
		public class OnPickupFromRack
		{
		}
	}

	public class BasePlayer_Entity
	{
		[Patch("OnChairComfort", "OnChairComfort", typeof(BaseChair), "GetComfort", new Type[] { })]
		[Info("Overrides the amount of comfort chairs give to players.")]
		[Parameter("chair", typeof(BaseChair), false)]
		[Return(typeof(float))]
		public class OnChairComfort : Patch
		{
			public static bool Prefix(ref BaseChair __instance, ref float __result)
			{
				if (HookCaller.CallStaticHook(3306666476u, (object)__instance) is float num)
				{
					__result = num;
					return false;
				}
				return true;
			}
		}
	}

	public class Entity_BaseNetworkable
	{
		[Patch("OnEntitySpawn", "OnEntitySpawn", typeof(BaseNetworkable), "Spawn", new Type[] { })]
		[Info("Called before any networked entity has spawned (including trees).")]
		[Parameter("networkable", typeof(BaseNetworkable), false)]
		[Return(typeof(void), Discarded = true)]
		public class OnEntitySpawn : Patch
		{
			public static void Prefix(ref BaseNetworkable __instance)
			{
				HookCaller.CallStaticHook(4136550545u, (object)__instance);
			}
		}
	}

	public class Entity_Hooks
	{
		[Patch("OnEntityTakeDamage", "OnEntityTakeDamage", typeof(CorePlugin), "IOnBasePlayerAttacked")]
		[Options(/*Could not decode attribute arguments.*/)]
		[Category("Entity")]
		[Info("Called when a player gets attacked.")]
		[Parameter("entity", typeof(BaseCombatEntity), false)]
		[Parameter("info", typeof(HitInfo), false)]
		[Return(typeof(void))]
		[Assembly("Carbon.Common.dll")]
		[OxideCompatible]
		public class OnEntityTakeDamage : Patch
		{
		}
	}
}
