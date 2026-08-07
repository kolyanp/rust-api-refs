using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("spawn")]
public class Spawn : ConsoleSystem
{
	[ServerVar(Help = "(Generated) Minimum spawn rate scalar applied to NPC/resource population spawning; lower values slow down respawn ticking when server population is low")]
	public static float min_rate = 0.5f;

	[ServerVar(Help = "(Generated) Maximum spawn rate scalar applied to NPC/resource population spawning; the spawn tick rate scales up to this value as player count increases")]
	public static float max_rate = 1f;

	[ServerVar(Help = "(Generated) Minimum population density scalar; controls the lower bound for how densely spawnable items fill their designated spawn areas at low player counts")]
	public static float min_density = 0.5f;

	[ServerVar(Help = "(Generated) Maximum population density scalar; controls the upper bound for how densely spawnable items fill their designated spawn areas at high player counts")]
	public static float max_density = 1f;

	[ServerVar(Help = "(Generated) Base player count used when computing population spawn rates; below this value player_scale group rates are not yet applied")]
	public static float player_base = 100f;

	[ServerVar(Help = "(Generated) Multiplier applied to group spawn rates based on current player count relative to player_base; higher values cause more group spawns as the server fills up")]
	public static float player_scale = 2f;

	[ServerVar(Help = "(Generated) When enabled, population spawners (animals, NPCs, resources) will respawn entities over time as they are killed or harvested")]
	public static bool respawn_populations = true;

	[ServerVar(Help = "(Generated) When enabled, spawn groups (monument NPCs, timed event spawners) will respawn their entities after they are cleared")]
	public static bool respawn_groups = true;

	[ServerVar(Help = "(Generated) When enabled, individually tracked entities (e.g. specific persistent NPCs) will respawn after a delay when destroyed")]
	public static bool respawn_individuals = true;

	[ServerVar(Help = "(Generated) Interval in seconds between population spawn ticks; lower values cause populations to refill faster but increase server CPU load")]
	public static float tick_populations = 60f;

	[ServerVar(Help = "(Generated) Interval in seconds between individual entity respawn ticks; controls how frequently the server checks for and respawns dead individual entities")]
	public static float tick_individuals = 300f;

	[ServerVar(Help = "When scaling loot respawn rates by population, this will be considered the 'max' population, preventing loot speeding up if player counts are above this")]
	public static int population_cap_rate = 300;

	[ServerVar(Help = "If set the loot spawn system will consider this the player count, not the actual player count. Useful for testing")]
	public static int loot_population_test = 0;

	[ServerVar(Help = "(Generated) Immediately fills all population spawners to their target density; useful after a wipe or server restart to skip the gradual ramp-up period")]
	public static void fill_populations(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			SingletonComponent<SpawnHandler>.Instance.FillPopulations();
		}
	}

	[ServerVar(Help = "(Generated) Deletes all spawned entities belonging to the named population(s); pass one or more population names as arguments to target specific groups")]
	public unsafe static void delete_populations(Arg args)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!args.HasArgs())
		{
			args.ReplyWith("Usage: delete_populations <population_name> ...");
			return;
		}
		StringView[] args2 = args.Args;
		for (int i = 0; i < args2.Length; i++)
		{
			StringView val = args2[i];
			SingletonComponent<SpawnHandler>.Instance?.DeletePopulation(((object)(*(StringView*)(&val))/*cast due to constrained. prefix*/).ToString());
		}
	}

	[ServerVar(Help = "(Generated) Deletes all entities from every active population spawner on the server at once; effectively despawns all wildlife, NPCs, and resource nodes")]
	public static void delete_all_populations(Arg args)
	{
		SingletonComponent<SpawnHandler>.Instance?.DeleteAllPopulations();
	}

	[ServerVar(Help = "<iterations> - Simulates a number of iterations on the closest loot container and sums up the items spawned")]
	public static void simulate_loot(Arg args)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = ArgEx.Player(args);
		if ((Object)(object)player == (Object)null)
		{
			args.ReplyWith("Must be called from player");
			return;
		}
		int num = Mathf.Clamp(args.GetInt(0, 100), 1, 10000);
		List<ILootContainer> list = new List<ILootContainer>();
		global::Vis.Entities(((Component)player).transform.position, 5f, list, -1, (QueryTriggerInteraction)1);
		ILootContainer lootContainer = list.OrderBy((ILootContainer x) => Vector3.Distance(((Component)player).transform.position, ((Component)x.GetEntity()).transform.position)).FirstOrDefault();
		if (lootContainer == null)
		{
			args.ReplyWith("No loot container found");
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int num2 = 0; num2 < num; num2++)
		{
			ItemContainer inventory = lootContainer.GetInventory();
			inventory.Clear();
			ItemManager.DoRemoves();
			lootContainer.PopulateLoot();
			foreach (Item item in inventory.itemList)
			{
				if (item != null)
				{
					dictionary.TryGetValue(item.info.shortname, out var value);
					dictionary[item.info.shortname] = value + item.amount;
				}
			}
		}
		int totalWidth = dictionary.Max((KeyValuePair<string, int> x) => x.Key.Length);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Simulated loot from {num} {lootContainer.GetEntity().ShortPrefabName}:");
		foreach (KeyValuePair<string, int> item2 in dictionary.OrderByDescending((KeyValuePair<string, int> x) => x.Value))
		{
			stringBuilder.AppendLine($"{item2.Key.PadRight(totalWidth)} : {item2.Value}");
		}
		args.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Respawns loot in the loot container currently being looked at")]
	public static void respawnloot_lookingat(Arg arg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 3f, 1218652417, (QueryTriggerInteraction)0);
			if (!(baseNetworkable is ILootContainer lootContainer) || !baseNetworkable.isServer)
			{
				arg.ReplyWith("Not looking at a loot container");
				return;
			}
			lootContainer.SpawnLoot();
			arg.ReplyWith("Respawned loot for " + lootContainer.GetEntity().ShortPrefabName);
		}
	}

	[ServerVar(Help = "Respawns loot in all loot containers within the given radius")]
	public static void respawnloot_radius(Arg arg)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		float num = arg.GetFloat(0, 20f);
		if (num <= 0f)
		{
			arg.ReplyWith("Radius must be greater than zero");
			return;
		}
		int num2 = 0;
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			global::Vis.Entities(((Component)basePlayer).transform.position, num, (List<BaseEntity>)(object)val, -1, (QueryTriggerInteraction)2);
			int i = 0;
			for (int count = ((List<BaseEntity>)(object)val).Count; i < count; i++)
			{
				BaseEntity baseEntity = ((List<BaseEntity>)(object)val)[i];
				if (baseEntity.isServer && baseEntity is ILootContainer lootContainer)
				{
					lootContainer.SpawnLoot();
					num2++;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		arg.ReplyWith($"Respawned loot for {num2} loot containers in radius {num:0.##}m.");
	}

	[ServerVar(Help = "Respawns loot in all loot containers currently on the server")]
	public static void respawnloot_all(Arg arg)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is ILootContainer lootContainer)
				{
					lootContainer.SpawnLoot();
					num++;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Respawned loot for {num} loot containers");
	}

	[ServerVar(Help = "Fills all spawn groups to their maximum count without waiting for the normal tick interval")]
	public static void fill_groups(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			SingletonComponent<SpawnHandler>.Instance.FillGroups();
		}
	}

	[ServerVar(Help = "Clears all spawn groups of already spawned entities, then re-fills them")]
	public static void reset_groups(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			SingletonComponent<SpawnHandler>.Instance.ResetGroups();
		}
	}

	[ServerVar(Help = "(Generated) Immediately respawns all individually tracked entities that are currently missing, bypassing the normal tick_individuals delay")]
	public static void fill_individuals(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			SingletonComponent<SpawnHandler>.Instance.FillIndividuals();
		}
	}

	[ServerVar(Help = "(Generated) Prints a spawn handler report listing all populations, their current count, target count, and fill percentage; pass true for detailed mode or a name filter as a second argument")]
	public static void report(Arg args)
	{
		if (Object.op_Implicit((Object)(object)SingletonComponent<SpawnHandler>.Instance))
		{
			bool detailed = args.GetBool(0);
			string filter = args.GetString(1, null);
			args.ReplyWith(SingletonComponent<SpawnHandler>.Instance.GetReport(detailed, filter));
		}
		else
		{
			args.ReplyWith("No spawn handler found.");
		}
	}

	[ServerVar(Help = "(Generated) Generates a debug spawn map for a named population, simulating up to the given number of spawn attempts and reporting how many would succeed; used to diagnose spawn point coverage")]
	public static void dump_map(Arg args)
	{
		string name = args.GetString(0);
		int simCount = args.GetInt(1, 100);
		SingletonComponent<SpawnHandler>.Instance.GenerateDebugMaps(name, simCount, out var spawned, out var attempts);
		args.ReplyWith($"Would spawn {spawned} prefabs, ran {attempts} attempts");
	}

	[ServerVar(Help = "Renders a PNG of every spawned ore nodes location to <rootFolder>/debug/ore-nodes.png (blue rings = safezones)")]
	public static void ore_map(Arg args)
	{
		if (!((Object)(object)SingletonComponent<SpawnHandler>.Instance == (Object)null))
		{
			int inSafeZone;
			int num = SingletonComponent<SpawnHandler>.Instance.GenerateOreNodeMap(out inSafeZone);
			args.ReplyWith($"Wrote {Server.rootFolder}/debug/ore-nodes.png - {num} ore nodes, {inSafeZone} inside safezones");
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of current spawn scalar values including player fraction, excess, population rate, density, and group rate; pass --json for machine-readable output")]
	public static void scalars(Arg args)
	{
		bool flag = args.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("Type");
			val.AddColumn("Value");
			val.AddRow(new string[2]
			{
				"Player Fraction",
				SpawnHandler.PlayerFraction().ToString()
			});
			val.AddRow(new string[2]
			{
				"Player Excess",
				SpawnHandler.PlayerExcess().ToString()
			});
			val.AddRow(new string[2]
			{
				"Population Rate",
				SpawnHandler.PlayerLerp(min_rate, max_rate).ToString()
			});
			val.AddRow(new string[2]
			{
				"Population Density",
				SpawnHandler.PlayerLerp(min_density, max_density).ToString()
			});
			val.AddRow(new string[2]
			{
				"Group Rate",
				SpawnHandler.PlayerScale(player_scale).ToString()
			});
			args.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Spawns a cargo ship and starts the cargo ship event immediately, bypassing the normal random event scheduler")]
	public static void cargoshipevent(Arg args)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/content/vehicles/boats/cargoship/cargoshiptest.prefab");
		if ((Object)(object)baseEntity != (Object)null)
		{
			((Component)baseEntity).SendMessage("TriggeredEventSpawn", (SendMessageOptions)1);
			baseEntity.Spawn();
			args.ReplyWith("Cargo ship event has been started");
		}
		else
		{
			args.ReplyWith("Couldn't find cargo ship prefab - maybe it has been renamed?");
		}
	}

	[ServerVar(Help = "(Generated) Triggers a CH47 Chinook scientist event targeting the calling player position; optionally pass a start distance in metres (default 300)")]
	public static void ch47event(Arg args)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		if (!CH47LandingZone.HasAnyLandingZones)
		{
			args.ReplyWith("Couldn't find any landing zones for CH47. Not starting the event");
			return;
		}
		int num = args.GetInt(0, 300);
		if (CH47ReinforcementListener.TryCall("assets/Prefabs/NPC/CH47/ch47scientists.entity.prefab", ((Component)basePlayer).transform.position, num))
		{
			args.ReplyWith($"CH47 event has been started at a distance of {num}m");
		}
		else
		{
			args.ReplyWith("Couldn't start CH47 event");
		}
	}

	[ServerVar(Help = "(Generated) Spawns a cargo ship and initiates the harbor docking test sequence at the specified docking path index; used to test cargo ship docking behaviour at harbors")]
	public static void cargoshipdockingtest(Arg args)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (CargoShip.TotalAvailableHarborDockingPaths == 0)
		{
			args.ReplyWith("No valid harbor dock points");
			return;
		}
		int num = args.GetInt(0);
		num = Mathf.Clamp(num, 0, CargoShip.TotalAvailableHarborDockingPaths);
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/content/vehicles/boats/cargoship/cargoshiptest.prefab");
		if ((Object)(object)baseEntity != (Object)null)
		{
			((Component)baseEntity).SendMessage("TriggeredEventSpawnDockingTest", (object)num, (SendMessageOptions)1);
			baseEntity.Spawn();
			args.ReplyWith("Cargo ship event has been started");
		}
		else
		{
			args.ReplyWith("Couldn't find cargo ship prefab - maybe it has been renamed?");
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that spawns a dummy player entity at the given position and direction loaded with the Shields loadout, optionally holstering the shield; triggered by spawn.shielddummy")]
	public static void svShieldDummy(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = arg.GetVector3(0);
		Vector3 vector2 = arg.GetVector3(1);
		bool flag = arg.GetBool(2);
		BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", vector, Quaternion.Euler(vector2)) as BasePlayer;
		basePlayer.Spawn();
		if (Inventory.LoadLoadout("Shields", out var so))
		{
			so.LoadItemsOnTo(basePlayer);
			if (!flag)
			{
				Inventory.EquipItemInSlot(basePlayer, 0);
			}
			else
			{
				Inventory.EquipItemInSlot(basePlayer, -1);
			}
		}
	}
}
