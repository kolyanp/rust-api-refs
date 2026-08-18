using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("entity")]
public class Entity : ConsoleSystem
{
	private struct EntityInfo
	{
		public BaseNetworkable entity;

		public NetworkableId entityID;

		public uint groupID;

		public NetworkableId parentID;

		public string status;

		public EntityInfo(BaseNetworkable src)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			entity = src;
			BaseEntity baseEntity = entity as BaseEntity;
			BaseEntity baseEntity2 = (((Object)(object)baseEntity != (Object)null) ? baseEntity.GetParentEntity() : null);
			NetworkableId val;
			NetworkableId val2;
			if (!((Object)(object)entity != (Object)null) || entity.net == null)
			{
				val = default(NetworkableId);
				val2 = val;
			}
			else
			{
				val2 = entity.net.ID;
			}
			entityID = val2;
			groupID = (((Object)(object)entity != (Object)null && entity.net != null && entity.net.group != null) ? entity.net.group.ID : 0u);
			NetworkableId val3;
			if (!((Object)(object)baseEntity != (Object)null))
			{
				val = default(NetworkableId);
				val3 = val;
			}
			else
			{
				val3 = baseEntity.parentEntity.uid;
			}
			parentID = val3;
			if ((Object)(object)baseEntity != (Object)null)
			{
				val = baseEntity.parentEntity.uid;
				if (((NetworkableId)(ref val)).IsValid)
				{
					if ((Object)(object)baseEntity2 == (Object)null)
					{
						status = "orphan";
					}
					else
					{
						status = "child";
					}
					return;
				}
			}
			status = string.Empty;
		}
	}

	public struct EntitySpawnRequest
	{
		public string PrefabName;

		public string Error;

		public bool Valid => string.IsNullOrEmpty(Error);
	}

	private struct VendorDefinition
	{
		public string VendingMachinePrefab;

		public string ShopKeeperPrefab;
	}

	private static readonly Dictionary<string, VendorDefinition> VendorDefinitions = new Dictionary<string, VendorDefinition>(StringComparer.OrdinalIgnoreCase) { ["waterwell"] = new VendorDefinition
	{
		VendingMachinePrefab = "assets/prefabs/deployable/vendingmachine/npcvendingmachines/shopkeeper_vm_invis_waterwell.prefab",
		ShopKeeperPrefab = "assets/prefabs/npc/waterwell/waterwell_shopkeeper.prefab"
	} };

	private static void GetEntityTable(TextTable table, Func<EntityInfo, bool> filter)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		table.AddColumn("realm");
		table.AddColumn("entity");
		table.AddColumn("group");
		table.AddColumn("parent");
		table.AddColumn("name");
		table.AddColumn("position");
		table.AddColumn("local");
		table.AddColumn("rotation");
		table.AddColumn("local");
		table.AddColumn("status");
		table.AddColumn("invokes");
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (!((Object)(object)current == (Object)null))
				{
					EntityInfo arg = new EntityInfo(current);
					if (filter(arg))
					{
						string[] obj = new string[11]
						{
							"sv",
							arg.entityID.Value.ToString(),
							arg.groupID.ToString(),
							arg.parentID.Value.ToString(),
							arg.entity.ShortPrefabName,
							((object)((Component)arg.entity).transform.position/*cast due to constrained. prefix*/).ToString(),
							((object)((Component)arg.entity).transform.localPosition/*cast due to constrained. prefix*/).ToString(),
							null,
							null,
							null,
							null
						};
						Quaternion val = ((Component)arg.entity).transform.rotation;
						obj[7] = ((object)((Quaternion)(ref val)).eulerAngles/*cast due to constrained. prefix*/).ToString();
						val = ((Component)arg.entity).transform.localRotation;
						obj[8] = ((object)((Quaternion)(ref val)).eulerAngles/*cast due to constrained. prefix*/).ToString();
						obj[9] = arg.status;
						obj[10] = arg.entity.InvokeString();
						table.AddRow(obj);
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Lists all networked entities whose prefab path contains the given filter string in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities whose prefab path contains the given filter string in a formatted table; admin-only on client")]
	public static void find_entity(Arg args)
	{
		string filter = args.GetString(0);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, (EntityInfo info) => string.IsNullOrEmpty(filter) || info.entity.PrefabName.Contains(filter));
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Lists the networked entity with the given network entity ID in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists the networked entity with the given network entity ID in a formatted table; admin-only on client")]
	public static void find_id(Arg args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId filter = ArgEx.GetEntityID(args, 0);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, delegate(EntityInfo info)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return info.entityID == filter;
			});
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(Help = "(Generated) Lists all networked entities belonging to the given network group ID in a formatted table; admin-only on client")]
	[ServerVar(Help = "(Generated) Lists all networked entities belonging to the given network group ID in a formatted table; admin-only on client")]
	public static void find_group(Arg args)
	{
		uint filter = args.GetUInt(0);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, (EntityInfo info) => info.groupID == filter);
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(Help = "(Generated) Lists all networked entities that have the given network entity ID as their parent in a formatted table; admin-only on client")]
	[ServerVar(Help = "(Generated) Lists all networked entities that have the given network entity ID as their parent in a formatted table; admin-only on client")]
	public static void find_parent(Arg args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId filter = ArgEx.GetEntityID(args, 0);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, delegate(EntityInfo info)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return info.parentID == filter;
			});
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(Help = "(Generated) Lists all networked entities whose status string contains the given filter text in a formatted table; admin-only on client")]
	[ServerVar(Help = "(Generated) Lists all networked entities whose status string contains the given filter text in a formatted table; admin-only on client")]
	public static void find_status(Arg args)
	{
		string filter = args.GetString(0);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, (EntityInfo info) => string.IsNullOrEmpty(filter) || info.status.Contains(filter));
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Lists all networked entities within the given radius in metres of the calling player in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities within the given radius in metres of the calling player in a formatted table; admin-only on client")]
	public static void find_radius(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		uint filter = args.GetUInt(0, 10u);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, delegate(EntityInfo info)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				return Vector3.Distance(((Component)info.entity).transform.position, ((Component)player).transform.position) <= (float)filter;
			});
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ClientVar(Help = "(Generated) Lists all networked entities owned by the calling player (matched by network ID) in a formatted table; admin-only on client")]
	[ServerVar(Help = "(Generated) Lists all networked entities owned by the calling player (matched by network ID) in a formatted table; admin-only on client")]
	public static void find_self(Arg args)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null || basePlayer.net == null)
		{
			return;
		}
		NetworkableId filter = basePlayer.net.ID;
		TextTable val = Pool.Get<TextTable>();
		try
		{
			GetEntityTable(val, delegate(EntityInfo info)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return info.entityID == filter;
			});
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Toggles the debug info overlay for an entity by net ID, showing position, velocity, health, and network state in the world")]
	public unsafe static void debug_toggle(Arg args)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!((NetworkableId)(ref entityID)).IsValid)
		{
			return;
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityID) as BaseEntity;
		if (!((Object)(object)baseEntity == (Object)null))
		{
			using (BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(BaseEntity.Flags.Debugging, !baseEntity.IsDebugging());
			}
			if (baseEntity.IsDebugging())
			{
				baseEntity.OnDebugStart();
			}
			NetworkableId iD = baseEntity.net.ID;
			args.ReplyWith("Debugging for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString() + " " + (baseEntity.IsDebugging() ? "enabled" : "disabled"));
		}
	}

	[ServerVar(Help = "(Generated) Applies a small positional nudge to an entity by net ID, useful for unsticking entities that are clipping into geometry")]
	public static void nudge(Arg args)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (((NetworkableId)(ref entityID)).IsValid)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityID) as BaseEntity;
			if (!((Object)(object)baseEntity == (Object)null))
			{
				((Component)baseEntity).BroadcastMessage("DebugNudge", (SendMessageOptions)1);
			}
		}
	}

	public static EntitySpawnRequest GetSpawnEntityFromName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return new EntitySpawnRequest
			{
				Error = "No entity name provided"
			};
		}
		string[] array = (from x in GameManifest.Current.entities
			where StringEx.Contains(Path.GetFileNameWithoutExtension(x), name, CompareOptions.IgnoreCase)
			select x.ToLower()).ToArray();
		if (array.Length == 0)
		{
			return new EntitySpawnRequest
			{
				Error = "Entity type not found"
			};
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				return new EntitySpawnRequest
				{
					Error = "Unknown entity - could be:\n\n" + string.Join("\n", array.Select(Path.GetFileNameWithoutExtension).ToArray())
				};
			}
			array[0] = text;
		}
		return new EntitySpawnRequest
		{
			PrefabName = array[0]
		};
	}

	[ServerVar(Name = "spawn", Help = "(Generated) Spawns a server entity by prefab name at a given world position and direction; returns the spawned entity net ID")]
	public unsafe static string svspawn(string name, Vector3 pos, Vector3 dir, int forceUp = 1)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer arg = ArgEx.Player(ConsoleSystem.CurrentArgs);
		EntitySpawnRequest spawnEntityFromName = GetSpawnEntityFromName(name);
		if (!spawnEntityFromName.Valid)
		{
			return spawnEntityFromName.Error;
		}
		bool flag = forceUp == 1;
		BaseEntity baseEntity = GameManager.server.CreateEntity(spawnEntityFromName.PrefabName, pos, flag ? Quaternion.LookRotation(dir, Vector3.up) : Quaternion.Euler(dir));
		if ((Object)(object)baseEntity == (Object)null)
		{
			Debug.Log((object)$"{arg} failed to spawn \"{spawnEntityFromName.PrefabName}\" (tried to spawn \"{name}\")");
			return "Couldn't spawn " + name;
		}
		BasePlayer basePlayer = baseEntity as BasePlayer;
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (flag)
			{
				Quaternion val = Quaternion.LookRotation(dir, Vector3.up);
				basePlayer.OverrideViewAngles(((Quaternion)(ref val)).eulerAngles);
			}
			else
			{
				basePlayer.OverrideViewAngles(dir);
			}
		}
		baseEntity.Spawn();
		EntityParentSettings entityParentSettings = default(EntityParentSettings);
		if (((Component)baseEntity).TryGetComponent<EntityParentSettings>(ref entityParentSettings))
		{
			entityParentSettings.TryDetachChildren(baseEntity);
		}
		baseEntity.UpdateNetworkGroup();
		Debug.Log((object)$"{arg} spawned \"{baseEntity}\" at {pos}");
		return "spawned " + ((object)baseEntity)?.ToString() + " at " + ((object)(*(Vector3*)(&pos))/*cast due to constrained. prefix*/).ToString();
	}

	private static string UnknownVendorMessage(string name)
	{
		return "Unknown vendor \"" + name + "\" - known vendors: " + string.Join(", ", VendorDefinitions.Keys);
	}

	[ServerVar(Name = "spawnvendor", Help = "(Generated) Spawns a complete NPC vendor by vendor name - both the shopkeeper NPC and the invisible vending machine it needs - at the position the calling player is looking at")]
	public static string svspawnvendor(string name)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name) || !VendorDefinitions.TryGetValue(name, out var value))
		{
			return UnknownVendorMessage(name);
		}
		if ((Object)(object)basePlayer == (Object)null)
		{
			return "spawnvendor has to be run by a player - it spawns the vendor wherever you're looking";
		}
		Ray val = basePlayer.eyes.HeadRay();
		RaycastHit val2 = default(RaycastHit);
		if (!Physics.Raycast(val, ref val2, 100f, 1218652417, (QueryTriggerInteraction)1))
		{
			return "Nothing to place the vendor on - look at the ground and try again";
		}
		Vector3 point = ((RaycastHit)(ref val2)).point;
		Vector3 val3 = -Vector3Ex.XZ3D(((Ray)(ref val)).direction);
		Quaternion rot = Quaternion.LookRotation((((Vector3)(ref val3)).sqrMagnitude > 0.001f) ? val3 : Vector3.forward, Vector3.up);
		InvisibleVendingMachine invisibleVendingMachine = SpawnVendorEntity<InvisibleVendingMachine>(value.VendingMachinePrefab, point, rot);
		if ((Object)(object)invisibleVendingMachine == (Object)null)
		{
			Debug.Log((object)$"{basePlayer} failed to spawn \"{value.VendingMachinePrefab}\" for the \"{name}\" vendor");
			return "Couldn't spawn the vending machine for the \"" + name + "\" vendor";
		}
		NPCShopKeeper nPCShopKeeper = SpawnVendorEntity<NPCShopKeeper>(value.ShopKeeperPrefab, point, rot);
		if ((Object)(object)nPCShopKeeper == (Object)null)
		{
			invisibleVendingMachine.Kill();
			Debug.Log((object)$"{basePlayer} failed to spawn \"{value.ShopKeeperPrefab}\" for the \"{name}\" vendor");
			return "Couldn't spawn the shopkeeper for the \"" + name + "\" vendor";
		}
		if ((Object)(object)nPCShopKeeper.GetVendingMachine() != (Object)(object)invisibleVendingMachine)
		{
			Debug.LogWarning((object)("Spawned the \"" + name + "\" vendor but the shopkeeper didn't pair up with its vending machine - the shop won't be interactable"));
		}
		Debug.Log((object)$"{basePlayer} spawned the \"{name}\" vendor at {point}");
		return $"spawned the \"{name}\" vendor at {point}";
	}

	private static T SpawnVendorEntity<T>(string prefabName, Vector3 pos, Quaternion rot) where T : BaseEntity
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabName, pos, rot);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (!(baseEntity is T val))
		{
			Debug.LogError((object)("\"" + prefabName + "\" is not a " + typeof(T).Name + " - the vendor definition is wrong"));
			GameManager.Destroy(((Component)baseEntity).gameObject);
			return null;
		}
		if (val is BasePlayer basePlayer)
		{
			basePlayer.OverrideViewAngles(((Quaternion)(ref rot)).eulerAngles);
		}
		val.Spawn();
		EntityParentSettings entityParentSettings = default(EntityParentSettings);
		if (((Component)val).TryGetComponent<EntityParentSettings>(ref entityParentSettings))
		{
			entityParentSettings.TryDetachChildren(val);
		}
		val.UpdateNetworkGroup();
		return val;
	}

	[ServerVar(Name = "spawnitem", Help = "(Generated) Spawns a dropped item entity server-side by item short name at a given world position")]
	public unsafe static string svspawnitem(string name, Vector3 pos)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name))
		{
			return "No entity name provided";
		}
		string[] array = (from x in ItemManager.itemList
			select x.shortname into x
			where StringEx.Contains(x, name, CompareOptions.IgnoreCase)
			select x).ToArray();
		if (array.Length == 0)
		{
			return "Entity type not found";
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(x, name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				Debug.Log((object)$"{basePlayer} failed to spawn \"{name}\"");
				return "Unknown entity - could be:\n\n" + string.Join("\n", array);
			}
			array[0] = text;
		}
		Item item = ItemManager.CreateByName(array[0], 1, 0uL);
		if (item == null)
		{
			Debug.Log((object)$"{basePlayer} failed to spawn \"{array[0]}\" (tried to spawnitem \"{name}\")");
			return "Couldn't spawn " + name;
		}
		item?.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		BaseEntity arg = item.CreateWorldObject(pos);
		Debug.Log((object)$"{basePlayer} spawned \"{arg}\" at {pos} (via spawnitem)");
		return "spawned " + item?.ToString() + " at " + ((object)(*(Vector3*)(&pos))/*cast due to constrained. prefix*/).ToString();
	}

	[ServerVar(Name = "spawngrid", Help = "(Generated) Spawns a grid of server entities by prefab name centred at a position; useful for stress-testing entity counts")]
	public static string svspawngrid(string name, int width = 5, int height = 5, float spacing = 5f)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		EntitySpawnRequest spawnEntityFromName = GetSpawnEntityFromName(name);
		if (!spawnEntityFromName.Valid)
		{
			return spawnEntityFromName.Error;
		}
		Quaternion rotation = ((Component)basePlayer).transform.rotation;
		((Quaternion)(ref rotation)).eulerAngles = new Vector3(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
		Matrix4x4 val = Matrix4x4.TRS(((Component)basePlayer).transform.position, ((Component)basePlayer).transform.rotation, Vector3.one);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Vector3 pos = ((Matrix4x4)(ref val)).MultiplyPoint(new Vector3((float)i * spacing, 0f, (float)j * spacing));
				BaseEntity baseEntity = GameManager.server.CreateEntity(spawnEntityFromName.PrefabName, pos, rotation);
				if ((Object)(object)baseEntity == (Object)null)
				{
					Debug.Log((object)$"{basePlayer} failed to spawn \"{spawnEntityFromName.PrefabName}\" (tried to spawn \"{name}\")");
					return "Couldn't spawn " + name;
				}
				baseEntity.Spawn();
			}
		}
		Debug.Log((object)($"{basePlayer} spawned ({width * height}) " + spawnEntityFromName.PrefabName));
		return $"spawned ({width * height}) " + spawnEntityFromName.PrefabName;
	}

	[ServerVar(Name = "spawnplants", Help = "Spawn every stage of every plant inside it's own planter, with an optional filter")]
	public static void spawnplants(Arg args)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		string filter = args.GetString(0);
		int height = args.GetInt(0, 1);
		BasePlayer basePlayer = ArgEx.Player(args);
		List<PlanterBox> list = SpawnPlants(((Component)basePlayer).transform.position, basePlayer.ServerRotation, filter, height);
		args.ReplyWith($"Spawned {list.Count} planters");
	}

	public static List<PlanterBox> SpawnPlants(Vector3 position, Quaternion rotation, string filter = "", int height = 1)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		List<PlanterBox> list = new List<PlanterBox>();
		GrowableEntity[] source = (from x in GameManifest.Current.entities
			where x.StartsWith("assets/prefabs/plants/", StringComparison.OrdinalIgnoreCase)
			select GameManager.server.FindPrefab(x) into x
			select x.GetComponent<GrowableEntity>() into x
			where (Object)(object)x != (Object)null
			select x).ToArray();
		int num = 0;
		string strPrefab = "Assets/Prefabs/Deployable/Planters/planter.large.deployed.prefab";
		foreach (GrowableEntity item in source.OrderBy((GrowableEntity x) => x.ShortPrefabName))
		{
			if (!string.IsNullOrEmpty(filter) && !item.ShortPrefabName.Contains(filter))
			{
				continue;
			}
			for (int num2 = 0; num2 <= 7; num2++)
			{
				for (int num3 = 0; num3 < height; num3++)
				{
					Vector3 pos = position + new Vector3((float)num * 3f, 0f, (float)num2 * 3f);
					PlanterBox planterBox = GameManager.server.CreateEntity(strPrefab, pos, rotation) as PlanterBox;
					planterBox.soilSaturation = planterBox.soilSaturationMax;
					planterBox.Spawn();
					list.Add(planterBox);
					Socket_Specific_Female[] array = (from x in PrefabAttribute.server.FindAll<Socket_Base>(planterBox.prefabID).OfType<Socket_Specific_Female>()
						where Enumerable.Contains(x.allowedMaleSockets, "planter_slot")
						select x).ToArray();
					foreach (Socket_Specific_Female socket_Specific_Female in array)
					{
						GrowableEntity obj = GameManager.server.CreateEntity(item.PrefabName, socket_Specific_Female.localPosition, socket_Specific_Female.localRotation) as GrowableEntity;
						obj.ChangeState((PlantProperties.State)num2, resetAge: true, loading: true);
						obj.Spawn();
						obj.SetParent(planterBox);
						obj.SetGrowing(state: false);
					}
				}
			}
			num++;
		}
		return list;
	}

	[ServerVar(Help = "(Generated) Spawns a copy of the loot table from one container prefab into the world at the calling player position")]
	public static void spawnlootfrom(Arg args)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		string text = args.GetString(0, string.Empty);
		int num = args.GetInt(1, 1);
		Vector3 vector = args.GetVector3(1, Object.op_Implicit((Object)(object)basePlayer) ? basePlayer.CenterPoint() : Vector3.zero);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(text, vector);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return;
		}
		baseEntity.Spawn();
		basePlayer.ChatMessage("Contents of " + text + " spawned " + num + " times");
		LootContainer component = ((Component)baseEntity).GetComponent<LootContainer>();
		if ((Object)(object)component != (Object)null)
		{
			for (int i = 0; i < num * component.maxDefinitionsToSpawn; i++)
			{
				component.lootDefinition.SpawnIntoContainer(basePlayer.inventory.containerMain);
			}
		}
		baseEntity.Kill();
	}

	public static int DeleteBy(ulong id)
	{
		List<ulong> list = Pool.Get<List<ulong>>();
		list.Add(id);
		int result = DeleteBy(list);
		Pool.FreeUnmanaged<ulong>(ref list);
		return result;
	}

	[ServerVar(Help = "Destroy all entities created by provided users (separate users by space)")]
	public static int DeleteBy(Arg arg)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!arg.HasArgs())
		{
			return 0;
		}
		List<ulong> list = Pool.Get<List<ulong>>();
		StringView[] args = arg.Args;
		for (int i = 0; i < args.Length; i++)
		{
			if (ulong.TryParse(StringView.op_Implicit(args[i]), out var result))
			{
				list.Add(result);
			}
		}
		int result2 = DeleteBy(list);
		Pool.FreeUnmanaged<ulong>(ref list);
		return result2;
	}

	private static int DeleteBy(List<ulong> ids)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseEntity baseEntity = (BaseEntity)enumerator.Current;
				if ((Object)(object)baseEntity == (Object)null)
				{
					continue;
				}
				bool flag = false;
				foreach (ulong id in ids)
				{
					if (baseEntity.OwnerID == id)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					baseEntity.Invoke(baseEntity.KillMessage, (float)num * 0.2f);
					num++;
				}
			}
			return num;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Destroy all entities created by users in the provided text block (can use with copied results from ent auth)")]
	public static void DeleteByTextBlock(Arg arg)
	{
		if (arg.Args.Length != 1)
		{
			arg.ReplyWith("Invalid arguments, provide a text block surrounded by \" and listing player id's at the start of each line");
			return;
		}
		MatchCollection matchCollection = Regex.Matches(arg.GetString(0), "^\\b\\d{17}", RegexOptions.Multiline);
		List<ulong> list = Pool.Get<List<ulong>>();
		foreach (Match item in matchCollection)
		{
			if (ulong.TryParse(item.Value, out var result))
			{
				list.Add(result);
			}
		}
		int num = DeleteBy(list);
		Pool.FreeUnmanaged<ulong>(ref list);
		arg.ReplyWith($"Destroyed {num} entities");
	}

	[ServerVar(Help = "(Generated) Sets the charge level of an electric battery entity by net ID to the given percentage (0-100)")]
	public static void set_battery_charge(Arg arg)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Usage: set_battery_charge <charge>");
			return;
		}
		float num = arg.GetFloat(0);
		ElectricBattery electricBattery = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 5f, -5, (QueryTriggerInteraction)0) as ElectricBattery;
		if ((Object)(object)electricBattery == (Object)null)
		{
			arg.ReplyWith("Not looking at battery");
			return;
		}
		electricBattery.SetCharge(num);
		arg.ReplyWith($"Set battery charge to {num}");
	}

	[ServerVar(EditorOnly = true, Help = "(Generated) Editor only: stress-tests the entity pool system by rapidly spawning and despawning a named prefab many times")]
	public static void test_pooling(Arg args)
	{
	}
}
