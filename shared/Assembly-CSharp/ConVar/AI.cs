using System;
using System.Collections.Generic;
using System.Text;
using Facepunch;
using Rust.Ai;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.AI;

namespace ConVar;

[Factory("ai")]
public class AI : ConsoleSystem
{
	[ServerVar(Help = "How long (in seconds) a value that has just moved stays coloured in the ai.showState read-out; 0 stops it colouring at all", ShowInAdminUI = true)]
	public static float showstateflashduration = 1.5f;

	[ServerVar(Help = "Run RustNav movement, steering and corridor queries in the transform job.")]
	public static bool batch_navmesh_pathfollowing = true;

	[ServerVar(Help = "Batch RustNav agent transform updates using jobs. Disable to apply each update immediately.")]
	public static bool batch_navmesh_transforms = true;

	[ReplicatedVar(Saved = true)]
	public static bool designingEnabled = false;

	[ReplicatedVar]
	public static bool npcBarksEnabled = false;

	public const float showCommandsRefreshInterval = 0.1f;

	public const float animFadeDuration = 0.25f;

	[ServerVar(Help = "(Generated) When enabled, AI entities run their brain Think() logic each tick; disable to freeze all AI decision-making while leaving entities in place")]
	public static bool think = true;

	[ServerVar(Help = "(Generated) When enabled, AI entities update their NavMesh agent destinations each tick; disable to freeze AI movement while keeping brain logic running")]
	public static bool navthink = true;

	[ServerVar(Help = "(Generated) When enabled, AI entities ignore player presence and will not target or react to players; useful for building/testing without NPC interference")]
	public static bool ignoreplayers = false;

	[ServerVar(Help = "(Generated) When enabled, AI weapons deal real damage when fired; disable to make NPC weapons harmless for testing AI behaviour safely")]
	public static bool effectaiweapons = false;

	[ServerVar(Help = "(Generated) When enabled, AI group logic is active allowing NPCs to coordinate as squads; disable to make all NPCs act as independent individuals")]
	public static bool groups = true;

	[ServerVar(Help = "(Generated) When enabled, AI updates are spliced across multiple frames to spread CPU cost; disable to run all AI updates synchronously every tick")]
	public static bool spliceupdates = true;

	[ServerVar(Help = "(Generated) When enabled, NavMesh destinations are sampled to the nearest valid NavMesh position before being set; prevents NPCs getting stuck off-mesh")]
	public static bool setdestinationsamplenavmesh = true;

	[ServerVar(Help = "(Generated) When enabled, NavMesh path calculation uses the full CalculatePath API; disable to use the simpler SetDestination fallback only")]
	public static bool usecalculatepath = true;

	[ServerVar(Help = "(Generated) When enabled, falls back to SetDestination if CalculatePath fails to find a valid path to the target")]
	public static bool usesetdestinationfallback = true;

	[ServerVar(Help = "(Generated) When enabled, NPCs can enter and swim in water; disable to prevent all NPCs from entering water bodies")]
	public static bool npcswimming = true;

	[ServerVar(Help = "(Generated) When enabled, vision distance checks use per-bone raycasts for accuracy; disable to use a single origin ray for performance")]
	public static bool accuratevisiondistance = true;

	[ServerVar(Help = "(Generated) When enabled, AI entities move toward their NavMesh destinations; disable to freeze NPC movement while keeping brain logic running")]
	public static bool move = true;

	[ServerVar(Help = "(Generated) When enabled, AI uses the spatial grid for entity queries; disable to fall back to brute-force entity iteration for debugging spatial query issues")]
	public static bool usegrid = true;

	[ServerVar(Help = "(Generated) When enabled, AI information zones can sleep inactive NPCs and wake them when players approach; disable to keep all NPCs awake at all times")]
	public static bool sleepwake = true;

	[ServerVar(Help = "(Generated) How frequently in seconds the NPC sensory system updates its awareness of nearby entities; higher values reduce CPU cost but make NPCs slower to react")]
	public static float sensetime = 1f;

	[ServerVar(Help = "(Generated) Target frame time budget in seconds for AI tick processing; default is 5s in production (1s in editor) to cap CPU usage per AI frame")]
	public static float frametime = 5f;

	[ServerVar(Help = "Target frame time in seconds for the default FSMComponent budget")]
	public static float fsm_frametime = 1f;

	[ServerVar(Help = "Target frame time in seconds for the Critters budget")]
	public static float critters_frametime = 0.2f;

	[ServerVar(Help = "Target frame time in seconds for the Ocean Critters swimming movement budget")]
	public static float ocean_critters_movement_frametime = 0.2f;

	[ServerVar(Help = "How close can a player get before a critter gets startled and runs away")]
	public static float critters_startle_distance = 0.5f;

	[ServerVar(Help = "(Generated) Maximum number of path-finding iterations used when calculating NPC ocean patrol routes; higher values produce better paths at more CPU cost")]
	public static int ocean_patrol_path_iterations = 100000;

	[ServerVar(Help = "If npc_enable is set to false then npcs won't spawn. (default: true)")]
	public static bool npc_enable = true;

	[ServerVar(Help = "npc_max_population_military_tunnels defines the size of the npc population at military tunnels. (default: 3)")]
	public static int npc_max_population_military_tunnels = 3;

	[ServerVar(Help = "npc_spawn_per_tick_max_military_tunnels defines how many can maximum spawn at once at military tunnels. (default: 1)")]
	public static int npc_spawn_per_tick_max_military_tunnels = 1;

	[ServerVar(Help = "npc_spawn_per_tick_min_military_tunnels defineshow many will minimum spawn at once at military tunnels. (default: 1)")]
	public static int npc_spawn_per_tick_min_military_tunnels = 1;

	[ServerVar(Help = "npc_respawn_delay_max_military_tunnels defines the maximum delay between spawn ticks at military tunnels. (default: 1920)")]
	public static float npc_respawn_delay_max_military_tunnels = 1920f;

	[ServerVar(Help = "npc_respawn_delay_min_military_tunnels defines the minimum delay between spawn ticks at military tunnels. (default: 480)")]
	public static float npc_respawn_delay_min_military_tunnels = 480f;

	[ServerVar(Help = "npc_valid_aim_cone defines how close their aim needs to be on target in order to fire. (default: 0.8)")]
	public static float npc_valid_aim_cone = 0.8f;

	[ServerVar(Help = "npc_valid_mounted_aim_cone defines how close their aim needs to be on target in order to fire while mounted. (default: 0.92)")]
	public static float npc_valid_mounted_aim_cone = 0.92f;

	[ServerVar(Help = "npc_cover_compromised_cooldown defines how long a cover point is marked as compromised before it's cleared again for selection. (default: 10)")]
	public static float npc_cover_compromised_cooldown = 10f;

	[ServerVar(Help = "If npc_cover_use_path_distance is set to true then npcs will look at the distance between the cover point and their target using the path between the two, rather than the straight-line distance.")]
	public static bool npc_cover_use_path_distance = true;

	[ServerVar(Help = "npc_cover_path_vs_straight_dist_max_diff defines what the maximum difference between straight-line distance and path distance can be when evaluating cover points. (default: 2)")]
	public static float npc_cover_path_vs_straight_dist_max_diff = 2f;

	[ServerVar(Help = "npc_door_trigger_size defines the size of the trigger box on doors that opens the door as npcs walk close to it (default: 1.5)")]
	public static float npc_door_trigger_size = 1.5f;

	[ServerVar(Help = "npc_patrol_point_cooldown defines the cooldown time on a patrol point until it's available again (default: 5)")]
	public static float npc_patrol_point_cooldown = 5f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the walk state, and should be a number between 0 and 1. (Default: 0.18)")]
	public static float npc_speed_walk = 0.18f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the run state, and should be a number between 0 and 1. (Default: 0.4)")]
	public static float npc_speed_run = 0.4f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the sprint state, and should be a number between 0 and 1. (Default: 1.0)")]
	public static float npc_speed_sprint = 1f;

	[ServerVar(Help = "npc_speed_walk define the speed of an npc when in the crouched walk state, and should be a number between 0 and 1. (Default: 0.1)")]
	public static float npc_speed_crouch_walk = 0.1f;

	[ServerVar(Help = "npc_speed_crouch_run define the speed of an npc when in the crouched run state, and should be a number between 0 and 1. (Default: 0.25)")]
	public static float npc_speed_crouch_run = 0.25f;

	[ServerVar(Help = "npc_alertness_drain_rate define the rate at which we drain the alertness level of an NPC when there are no enemies in sight. (Default: 0.01)")]
	public static float npc_alertness_drain_rate = 0.01f;

	[ServerVar(Help = "npc_alertness_zero_detection_mod define the threshold of visibility required to detect an enemy when alertness is zero. (Default: 0.5)")]
	public static float npc_alertness_zero_detection_mod = 0.5f;

	[ServerVar(Help = "defines the chance for scientists to spawn at NPC junkpiles. (Default: 0.1)")]
	public static float npc_junkpilespawn_chance = 0.07f;

	[ServerVar(Help = "npc_junkpile_dist_aggro_gate define at what range (or closer) a junkpile scientist will get aggressive. (Default: 8)")]
	public static float npc_junkpile_dist_aggro_gate = 8f;

	[ServerVar(Help = "npc_max_junkpile_count define how many npcs can spawn into the world at junkpiles at the same time (does not include monuments) (Default: 30)")]
	public static int npc_max_junkpile_count = 30;

	[ServerVar(Help = "If npc_families_no_hurt is true, npcs of the same family won't be able to hurt each other. (default: true)")]
	public static bool npc_families_no_hurt = true;

	[ServerVar(Help = "If npc_ignore_chairs is true, npcs won't care about seeking out and sitting in chairs. (default: true)")]
	public static bool npc_ignore_chairs = true;

	[ServerVar(Help = "The rate at which we tick the sensory system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 5)")]
	public static float npc_sensory_system_tick_rate_multiplier = 5f;

	[ServerVar(Help = "The rate at which we gather information about available cover points. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 20)")]
	public static float npc_cover_info_tick_rate_multiplier = 20f;

	[ServerVar(Help = "The rate at which we tick the reasoning system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 1)")]
	public static float npc_reasoning_system_tick_rate_multiplier = 1f;

	[ServerVar(Help = "(Generated) When enabled, NPC spawn points are validated to ensure they are on a valid NavMesh position before spawning; prevents NPCs from spawning in unreachable locations")]
	public static bool npc_check_spawner_is_on_navmesh = true;

	[ServerVar(Help = "If animal_ignore_food is true, animals will not sense food sources or interact with them (server optimization). (default: true)")]
	public static bool animal_ignore_food = true;

	[ServerVar(Help = "The modifier by which a silencer reduce the noise that a gun makes when shot. (Default: 0.15)")]
	public static float npc_gun_noise_silencer_modifier = 0.15f;

	[ServerVar(Help = "If nav_carve_use_building_optimization is true, we attempt to reduce the amount of navmesh carves for a building. (default: false)")]
	public static bool nav_carve_use_building_optimization = false;

	[ServerVar(Help = "The minimum number of building blocks a building needs to consist of for this optimization to be applied. (default: 25)")]
	public static int nav_carve_min_building_blocks_to_apply_optimization = 25;

	[ServerVar(Help = "The minimum size we allow a carving volume to be. (default: 2)")]
	public static float nav_carve_min_base_size = 2f;

	[ServerVar(Help = "The size multiplier applied to the size of the carve volume. The smaller the value, the tighter the skirt around foundation edges, but too small and animals can attack through walls. (default: 4)")]
	public static float nav_carve_size_multiplier = 4f;

	[ServerVar(Help = "The height of the carve volume. (default: 2)")]
	public static float nav_carve_height = 2f;

	[ServerVar(Help = "If npc_only_hurt_active_target_in_safezone is true, npcs won't any player other than their actively targeted player when in a safe zone. (default: true)")]
	public static bool npc_only_hurt_active_target_in_safezone = true;

	[ServerVar(Help = "If npc_use_new_aim_system is true, npcs will miss on purpose on occasion, where the old system would randomize aim cone. (default: true)")]
	public static bool npc_use_new_aim_system = true;

	[ServerVar(Help = "If npc_use_thrown_weapons is true, npcs will throw grenades, etc. This is an experimental feature. (default: true)")]
	public static bool npc_use_thrown_weapons = true;

	[ServerVar(Help = "This is multiplied with the max roam range stat of an NPC to determine how far from its spawn point the NPC is allowed to roam. (default: 3)")]
	public static float npc_max_roam_multiplier = 3f;

	[ServerVar(Help = "This is multiplied with the current alertness (0-10) to decide how long it will take for the NPC to deliberately miss again. (default: 0.33)")]
	public static float npc_alertness_to_aim_modifier = 0.5f;

	[ServerVar(Help = "The time it takes for the NPC to deliberately miss to the time the NPC tries to hit its target. (default: 1.5)")]
	public static float npc_deliberate_miss_to_hit_alignment_time = 1.5f;

	[ServerVar(Help = "The offset with which the NPC will maximum miss the target. (default: 1.25)")]
	public static float npc_deliberate_miss_offset_multiplier = 1.25f;

	[ServerVar(Help = "The percentage away from a maximum miss the randomizer is allowed to travel when shooting to deliberately hit the target (we don't want perfect hits with every shot). (default: 0.85f)")]
	public static float npc_deliberate_hit_randomizer = 0.85f;

	[ServerVar(Help = "Baseline damage modifier for the new HTN Player NPCs to nerf their damage compared to the old NPCs. (default: 1.15f)")]
	public static float npc_htn_player_base_damage_modifier = 1.15f;

	[ServerVar(Help = "Spawn NPCs on the Cargo Ship. (default: true)")]
	public static bool npc_spawn_on_cargo_ship = true;

	[ServerVar(Help = "Spawn NPCs on junkpiles (default: true)")]
	public static bool npc_spawn_on_junkpile = true;

	[ServerVar(Help = "Spawn NPCs on deep sea islands (default: false)")]
	public static bool npc_spawn_on_deep_sea_islands = true;

	[ServerVar(Help = "Do any kind of scientists spawn on the map (default: true)")]
	public static bool scientist_spawners_enabled = true;

	[ServerVar(Help = "npc_htn_player_frustration_threshold defines where the frustration threshold for NPCs go, where they have the opportunity to change to a more aggressive tactic. (default: 3)")]
	public static int npc_htn_player_frustration_threshold = 3;

	[ServerVar(Help = "(Generated) When enabled, logs AI-related warnings and issues to the server console; useful for diagnosing pathfinding and brain errors")]
	public static bool logIssues = false;

	[ServerVar(Help = "(Generated) Number of AI think ticks per second; default is 5 (every 200ms); higher values make AI more responsive at higher CPU cost")]
	public static float tickrate = 5f;

	[ServerVar(Help = "The angle under which the AI will think it's being watched by another entity")]
	public static float watchedAngle = 50f;

	[ServerVar(Help = "The angle under which the AI will think it's being aimed at at by a player")]
	public static float aimedAtAngle = 10f;

	private const float npcDefaultReactionTime = 0.65f;

	public static float npcReactionTime = 0.65f;

	[ServerVar(Help = "(Generated) Global health multiplier applied to all NPCs; 1.0 = normal, 2.0 = double health; useful for difficulty scaling without modifying individual NPC prefabs")]
	public static float npcHealthMultiplier = 1f;

	public const float minDepthToBeConsideredInWater = 0.3f;

	[ServerVar(Help = "(Generated) Default network interpolation delay in seconds applied to NPC entity movement; lower values reduce visual lag at the cost of jitter on unstable connections")]
	public static float defaultInterpolationDelay = 0.1f;

	[ServerVar(Help = "(Generated) Radius in metres within which a thrown smoke grenade suppresses NPC vision and targeting")]
	public static float smokeGrenadeNpcRadius = 4f;

	public static bool useUnityNavmesh = false;

	public static bool checkTileValid = false;

	public static float debugMoveRange = 200f;

	public static float debugMoveSelectRadius = 0.4f;

	[ServerVar(Saved = true, Help = "Gait ai.movenpc uses when called without one - set it to walk to keep commanded NPCs at walking speed. One of: sneak, walk, jog, run, sprint, fullsprint")]
	public static string debugMoveGait = "run";

	public const float debugMoveSelectRange = 30f;

	[ServerVar(Saved = true, Help = "Gap in metres between the destinations ai.movenpc hands out when more than one NPC is selected, so they don't all path to the same point.")]
	public static float debugMoveSpacing = 2f;

	[ServerVar(Saved = true, Help = "Whether ai.selectnpc and ai.movenpc draw their DDraw markers - turn it off to keep the view clean while commanding NPCs.")]
	public static bool debugMoveDraw = true;

	private const float debugMoveDrawDuration = 5f;

	private const float debugMoveGroundSnap = 5f;

	[ServerVar]
	public static void showState(Arg arg)
	{
		ArgEx.Player(arg)?.ToggleShowFSMStateDebugInfo();
	}

	[ServerVar(Help = "(Generated) Prints statistics about AI sleeping zones: how many zones are sleepable, how many are sleeping, and the total count of sleeping entities")]
	public static void sleepwakestats(Arg args)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			if (!((Object)(object)zone == (Object)null) && zone.ShouldSleepAI)
			{
				num++;
				if (zone.Sleeping)
				{
					num2++;
					num3 += zone.SleepingCount;
				}
			}
		}
		args.ReplyWith("Sleeping AIZs: " + num2 + " / " + num + ". Total sleeping ents: " + num3);
	}

	[ServerVar(Help = "(Generated) Wakes all currently sleeping AI information zones, forcing all sleeping NPCs within them to become active; reports zones and entity counts woken")]
	public static void wakesleepingai(Arg args)
	{
		int num = 0;
		int num2 = 0;
		foreach (AIInformationZone zone in AIInformationZone.zones)
		{
			if (!((Object)(object)zone == (Object)null) && zone.ShouldSleepAI && zone.Sleeping)
			{
				num++;
				num2 += zone.SleepingCount;
				zone.WakeAI();
			}
		}
		args.ReplyWith("Woke " + num + " sleeping AIZs containing " + num2 + " sleeping entities.");
	}

	[ServerVar(Help = "(Generated) Prints the current count of active animal, scientist, pet, and new NPC2 brain instances on the server")]
	public static void brainstats(Arg args)
	{
		int num = BaseEntity.Util.FindAll<BaseNPC2>().Length;
		int num2 = AnimalBrain.Count + ScientistBrain.Count + PetBrain.Count;
		args.ReplyWith(string.Format("Animal: {0}. Scientist: {1}. Pet: {2}. NPC2:{3}. Total: {4}", new object[5]
		{
			AnimalBrain.Count,
			ScientistBrain.Count,
			PetBrain.Count,
			num,
			num2
		}));
	}

	[ServerVar(Help = "(Generated) Prints the total count of registered AIInformationZone instances on the server")]
	public static void aizonestats(Arg args)
	{
		args.ReplyWith("AIInformationZone count: " + AIInformationZone.zones.Count);
	}

	[ServerVar(Help = "(Generated) Kills all scientist NPCs, tunnel dwellers, and non-animal NPC2 entities currently on the server")]
	public static void killscientists(Arg args)
	{
		ScientistNPC[] array = BaseEntity.Util.FindAll<ScientistNPC>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
		TunnelDweller[] array2 = BaseEntity.Util.FindAll<TunnelDweller>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Kill();
		}
		BaseNPC2[] array3 = BaseEntity.Util.FindAll<BaseNPC2>();
		foreach (BaseNPC2 baseNPC in array3)
		{
			if (!baseNPC.IsAnimal)
			{
				baseNPC.Kill();
			}
		}
	}

	[ServerVar(Help = "(Generated) Kills all animal NPCs and animal NPC2 entities currently on the server")]
	public static void killanimals(Arg args)
	{
		BaseAnimalNPC[] array = BaseEntity.Util.FindAll<BaseAnimalNPC>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
		BaseNPC2[] array2 = BaseEntity.Util.FindAll<BaseNPC2>();
		foreach (BaseNPC2 baseNPC in array2)
		{
			if (baseNPC.IsAnimal)
			{
				baseNPC.Kill();
			}
		}
	}

	[ServerVar(Help = "Add a player (or command user if no player is specified) to the AIs ignore list.")]
	public static void addignoreplayer(Arg args)
	{
		BasePlayer basePlayer = null;
		basePlayer = (args.HasArgs() ? ArgEx.GetPlayerOrSleeper(args, 0) : ArgEx.Player(args));
		if ((Object)(object)basePlayer == (Object)null || basePlayer.net == null || basePlayer.net.connection == null)
		{
			args.ReplyWith("Player not found.");
		}
		else
		{
			SimpleAIMemory.AddIgnorePlayer(basePlayer);
		}
	}

	[ServerVar(Help = "Remove a player (or command user if no player is specified) from the AIs ignore list.")]
	public static void removeignoreplayer(Arg args)
	{
		BasePlayer basePlayer = null;
		basePlayer = (args.HasArgs() ? ArgEx.GetPlayerOrSleeper(args, 0) : ArgEx.Player(args));
		if ((Object)(object)basePlayer == (Object)null || basePlayer.net == null || basePlayer.net.connection == null)
		{
			args.ReplyWith("Player not found.");
		}
		else
		{
			SimpleAIMemory.RemoveIgnorePlayer(basePlayer);
		}
	}

	[ServerVar(Help = "Remove all players from the AIs ignore list.")]
	public static void clearignoredplayers(Arg args)
	{
		SimpleAIMemory.ClearIgnoredPlayers();
	}

	[ServerVar(Help = "Print a lost of all the players in the AI ignore list.")]
	public static void printignoredplayers(Arg args)
	{
		args.ReplyWith(SimpleAIMemory.GetIgnoredPlayers());
	}

	private static void testIfEntityOnNav(BaseEntity entity, List<BaseEntity> offNavEntities)
	{
		NavMeshAgent val = default;
		if (((Component)entity).TryGetComponent<NavMeshAgent>(ref val) && !val.isOnNavMesh)
		{
			offNavEntities.Add(entity);
		}
	}

	[ServerVar(Help = "Print a list of all scientists that are off the navmesh. Optionally kill them off by passing true as the first argument.")]
	public static void printOrKillOffNavScientists(Arg args)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			int num = 0;
			ScientistNPC[] array = BaseEntity.Util.FindAll<ScientistNPC>();
			for (int i = 0; i < array.Length; i++)
			{
				testIfEntityOnNav(array[i], (List<BaseEntity>)(object)val);
				num++;
			}
			TunnelDweller[] array2 = BaseEntity.Util.FindAll<TunnelDweller>();
			for (int i = 0; i < array2.Length; i++)
			{
				testIfEntityOnNav(array2[i], (List<BaseEntity>)(object)val);
				num++;
			}
			BaseNPC2[] array3 = BaseEntity.Util.FindAll<BaseNPC2>();
			foreach (BaseNPC2 baseNPC in array3)
			{
				if (!baseNPC.IsAnimal)
				{
					testIfEntityOnNav(baseNPC, (List<BaseEntity>)(object)val);
					num++;
				}
			}
			if (args.GetBool(0))
			{
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					item.Kill();
				}
				args.ReplyWith($"Killed {((List<BaseEntity>)(object)val).Count} off navmesh entities.");
				return;
			}
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			stringBuilder.AppendLine($"Found {((List<BaseEntity>)(object)val).Count} off navmesh entities among {num}:");
			foreach (BaseEntity item2 in (List<BaseEntity>)(object)val)
			{
				stringBuilder.AppendLine($" * {item2} at {((Component)item2).transform.position} in {MapHelper.PositionToString(((Component)item2).transform.position)}");
			}
			args.ReplyWith(stringBuilder.ToString());
			Pool.FreeUnmanaged(ref stringBuilder);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "The time it takes for a NPC to fully notice a player standing right in front of them, in seconds.")]
	public static void SetNpcReactionTime(Arg args)
	{
		npcReactionTime = Mathf.Clamp(args.GetFloat(0, 0.65f), 0.001f, 5f);
		args.ReplyWith($"NPC reaction time set to {npcReactionTime} seconds.");
	}

	public static float TickDelta()
	{
		return 1f / tickrate;
	}

	[ServerVar(Help = "(Generated) Editor-only: finds the NPC entity with the given network ID on the server and selects its game object in the Unity editor hierarchy")]
	public static void selectNPCLookatServer(Arg args)
	{
	}

	[ServerVar(Help = "(Generated) Editor-only: teleports the caller to a top-down view, fills NPC populations, then draws DDraw labels and lines showing each NPC category and closest neighbour")]
	public static void showDistributions(Arg args)
	{
	}

	[ServerVar(Help = "Adds the NPC you are looking at to your selection so ai.movenpc can order it around.")]
	public static void selectnpc(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		BaseEntity baseEntity = TraceForNpc(basePlayer);
		if ((Object)(object)baseEntity == (Object)null)
		{
			args.ReplyWith($"No NPC found under your crosshair within {debugMoveRange}m.");
			return;
		}
		PruneSelection(basePlayer);
		if (IsSelected(basePlayer, baseEntity))
		{
			args.ReplyWith(DescribeNpc(baseEntity) + " is already selected - use ai.deselectnpc to drop it.");
			return;
		}
		if (!SupportsDebugMove(baseEntity, out var error))
		{
			args.ReplyWith(error);
			return;
		}
		AddToSelection(basePlayer, baseEntity);
		DrawSelection(basePlayer, baseEntity);
		args.ReplyWith($"Selected {DescribeNpc(baseEntity)} ({SelectionCount(basePlayer)} selected). Look at a position and run ai.movenpc.");
	}

	[ServerVar(Help = "Adds every commandable NPC around you to your selection. Optional radius in metres - defaults to ai.debugmoveselectrange.")]
	public static void selectnpcsinrange(Arg args)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 30f);
		if (num <= 0f)
		{
			args.ReplyWith("Radius has to be greater than zero.");
			return;
		}
		PruneSelection(basePlayer);
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			global::Vis.Entities(((Component)basePlayer).transform.position, num, (List<BaseEntity>)(object)val, 133120, (QueryTriggerInteraction)2);
			int num2 = 0;
			int num3 = 0;
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				BaseEntity baseEntity = ResolveNpc(item);
				if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.isClient && !((Object)(object)baseEntity == (Object)(object)basePlayer) && !IsSelected(basePlayer, baseEntity) && (!(baseEntity is BaseCombatEntity baseCombatEntity) || !baseCombatEntity.IsDead()))
				{
					if (!SupportsDebugMove(baseEntity, out var _))
					{
						num3++;
						continue;
					}
					AddToSelection(basePlayer, baseEntity);
					DrawSelection(basePlayer, baseEntity);
					num2++;
				}
			}
			string text = ((num3 > 0) ? $" ({num3} nearby NPCs can't be commanded)" : string.Empty);
			args.ReplyWith(string.Format("Added {0} NPCs within {1}m, {2} selected{3}.", new object[4]
			{
				num2,
				num,
				SelectionCount(basePlayer),
				text
			}));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Removes the NPC you are looking at from your selection and hands it back to its normal AI.")]
	public static void deselectnpc(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		BaseEntity baseEntity = TraceForNpc(basePlayer);
		if ((Object)(object)baseEntity == (Object)null)
		{
			args.ReplyWith($"No NPC found under your crosshair within {debugMoveRange}m.");
			return;
		}
		PruneSelection(basePlayer);
		if (!RemoveFromSelection(basePlayer, baseEntity))
		{
			args.ReplyWith(DescribeNpc(baseEntity) + " is not in your selection.");
		}
		else
		{
			args.ReplyWith($"Deselected {DescribeNpc(baseEntity)} ({SelectionCount(basePlayer)} still selected).");
		}
	}

	[ServerVar(Help = "Orders every NPC selected with ai.selectnpc to path to the position you are looking at. Optional gait: sneak, walk, jog, run, sprint, fullsprint - defaults to ai.debugmovegait.")]
	public static void movenpc(Arg args)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			GatherSelection(basePlayer, (List<BaseEntity>)(object)val);
			if (((List<BaseEntity>)(object)val).Count == 0)
			{
				selectnpc(args);
				return;
			}
			if (!TraceForPosition(basePlayer, out var position))
			{
				args.ReplyWith($"Nothing solid under your crosshair within {debugMoveRange}m.");
				return;
			}
			string text = args.GetString(0, null);
			if (string.IsNullOrEmpty(text))
			{
				text = debugMoveGait;
			}
			if (!TryParseGait(text, out var gait))
			{
				args.ReplyWith("Unknown gait '" + text + "'. Valid: " + string.Join(", ", Enum.GetNames(typeof(RustNavMeshAgent.Speeds))).ToLower() + ".");
				return;
			}
			PooledList<Vector3> val2 = Pool.Get<PooledList<Vector3>>();
			try
			{
				BuildFormation(position, ((List<BaseEntity>)(object)val).Count, (List<Vector3>)(object)val2);
				AssignFormationSlots((List<BaseEntity>)(object)val, position, (List<Vector3>)(object)val2);
				int num = 0;
				string text2 = null;
				for (int i = 0; i < ((List<BaseEntity>)(object)val).Count; i++)
				{
					BaseEntity npc = ((List<BaseEntity>)(object)val)[i];
					Vector3 destination = ((((List<Vector3>)(object)val2)[i] == position) ? position : ProjectToGround(((List<Vector3>)(object)val2)[i]));
					if (!OrderNpc(npc, destination, gait, out var error))
					{
						if (!OrderNpc(npc, position, gait, out error))
						{
							text2 = error;
							continue;
						}
						destination = position;
					}
					DrawMoveOrder(basePlayer, npc, destination);
					num++;
				}
				if (num == 0)
				{
					args.ReplyWith(text2 ?? "Nothing in your selection could be ordered.");
					return;
				}
				if (((List<BaseEntity>)(object)val).Count == 1)
				{
					args.ReplyWith($"{DescribeNpc(((List<BaseEntity>)(object)val)[0])} is pathing to {position} at {gait}.");
					return;
				}
				string text3 = ((num < ((List<BaseEntity>)(object)val).Count) ? $" ({((List<BaseEntity>)(object)val).Count - num} refused: {text2})" : string.Empty);
				args.ReplyWith(string.Format("{0} NPCs are pathing to {1} at {2}, spaced {3}m apart{4}.", new object[5] { num, position, gait, debugMoveSpacing, text3 }));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Releases every NPC selected with ai.selectnpc and hands them back to their normal AI.")]
	public static void releasenpc(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		PruneSelection(basePlayer);
		int num = SelectionCount(basePlayer);
		string text;
		switch (num)
		{
		case 0:
			args.ReplyWith("No NPCs selected.");
			return;
		default:
			text = $"{num} NPCs";
			break;
		case 1:
			text = DescribeNpc(basePlayer.debugCommandedNpcs[0].Get(serverside: true));
			break;
		}
		string text2 = text;
		ReleaseSelection(basePlayer);
		args.ReplyWith("Released " + text2 + ".");
	}

	private static int SelectionCount(BasePlayer player)
	{
		return player.debugCommandedNpcs.Count;
	}

	private static bool IsSelected(BasePlayer player, BaseEntity npc)
	{
		List<EntityRef<BaseEntity>> debugCommandedNpcs = player.debugCommandedNpcs;
		for (int i = 0; i < debugCommandedNpcs.Count; i++)
		{
			if ((Object)(object)debugCommandedNpcs[i].Get(serverside: true) == (Object)(object)npc)
			{
				return true;
			}
		}
		return false;
	}

	private static void AddToSelection(BasePlayer player, BaseEntity npc)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		player.debugCommandedNpcs.Add(new EntityRef<BaseEntity>(npc.net.ID));
	}

	private static bool RemoveFromSelection(BasePlayer player, BaseEntity npc)
	{
		List<EntityRef<BaseEntity>> debugCommandedNpcs = player.debugCommandedNpcs;
		for (int i = 0; i < debugCommandedNpcs.Count; i++)
		{
			if (!((Object)(object)debugCommandedNpcs[i].Get(serverside: true) != (Object)(object)npc))
			{
				debugCommandedNpcs.RemoveAt(i);
				ReleaseNpc(npc);
				return true;
			}
		}
		return false;
	}

	private static void PruneSelection(BasePlayer player)
	{
		List<EntityRef<BaseEntity>> debugCommandedNpcs = player.debugCommandedNpcs;
		for (int num = debugCommandedNpcs.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = debugCommandedNpcs[num].Get(serverside: true);
			if ((Object)(object)baseEntity == (Object)null || baseEntity.IsDestroyed || (baseEntity is BaseCombatEntity baseCombatEntity && baseCombatEntity.IsDead()))
			{
				debugCommandedNpcs.RemoveAt(num);
			}
		}
	}

	private static void GatherSelection(BasePlayer player, List<BaseEntity> into)
	{
		PruneSelection(player);
		List<EntityRef<BaseEntity>> debugCommandedNpcs = player.debugCommandedNpcs;
		for (int i = 0; i < debugCommandedNpcs.Count; i++)
		{
			into.Add(debugCommandedNpcs[i].Get(serverside: true));
		}
	}

	private static void ReleaseSelection(BasePlayer player)
	{
		List<EntityRef<BaseEntity>> debugCommandedNpcs = player.debugCommandedNpcs;
		for (int i = 0; i < debugCommandedNpcs.Count; i++)
		{
			ReleaseNpc(debugCommandedNpcs[i].Get(serverside: true));
		}
		debugCommandedNpcs.Clear();
	}

	private static void ReleaseNpc(BaseEntity npc)
	{
		if (!((Object)(object)npc == (Object)null) && !npc.IsDestroyed)
		{
			FSMComponent fSMComponent = default;
			BaseAIBrain baseAIBrain = default;
			if (((Component)npc).TryGetComponent<FSMComponent>(ref fSMComponent))
			{
				fSMComponent.DebugRelease();
			}
			else if (((Component)npc).TryGetComponent<BaseAIBrain>(ref baseAIBrain))
			{
				baseAIBrain.DebugRelease();
			}
		}
	}

	private static bool OrderNpc(BaseEntity npc, Vector3 destination, RustNavMeshAgent.Speeds gait, out string error)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		FSMComponent fSMComponent = default;
		if (((Component)npc).TryGetComponent<FSMComponent>(ref fSMComponent))
		{
			return fSMComponent.DebugMoveTo(destination, gait, out error);
		}
		BaseAIBrain baseAIBrain = default;
		if (((Component)npc).TryGetComponent<BaseAIBrain>(ref baseAIBrain))
		{
			return baseAIBrain.DebugMoveTo(destination, ToNavigatorSpeed(gait), out error);
		}
		error = DescribeNpc(npc) + " has neither an FSM nor a brain.";
		return false;
	}

	private static void BuildFormation(Vector3 centre, int count, List<Vector3> slots)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (count <= 0)
		{
			return;
		}
		slots.Add(centre);
		float num = Mathf.Max(debugMoveSpacing, 0.1f);
		int num2 = 1;
		while (slots.Count < count)
		{
			int num3 = num2 * 6;
			float num4 = (float)num2 * num;
			for (int i = 0; i < num3; i++)
			{
				if (slots.Count >= count)
				{
					break;
				}
				float num5 = (float)i / (float)num3 * MathF.PI * 2f;
				slots.Add(centre + new Vector3(Mathf.Sin(num5), 0f, Mathf.Cos(num5)) * num4);
			}
			num2++;
		}
	}

	private static void AssignFormationSlots(List<BaseEntity> npcs, Vector3 centre, List<Vector3> slots)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		npcs.Sort((BaseEntity a, BaseEntity b) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val3 = ((Component)a).transform.position - centre;
			float sqrMagnitude2 = val3.sqrMagnitude;
			val3 = ((Component)b).transform.position - centre;
			return sqrMagnitude2.CompareTo(val3.sqrMagnitude);
		});
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			((List<Vector3>)(object)val).AddRange((IEnumerable<Vector3>)slots);
			for (int num = 0; num < npcs.Count; num++)
			{
				Vector3 position = ((Component)npcs[num]).transform.position;
				int index = 0;
				float num2 = float.MaxValue;
				for (int num3 = 0; num3 < ((List<Vector3>)(object)val).Count; num3++)
				{
					Vector3 val2 = ((List<Vector3>)(object)val)[num3] - position;
					float sqrMagnitude = val2.sqrMagnitude;
					if (!(sqrMagnitude >= num2))
					{
						num2 = sqrMagnitude;
						index = num3;
					}
				}
				slots[num] = ((List<Vector3>)(object)val)[index];
				((List<Vector3>)(object)val).RemoveAt(index);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static Vector3 ProjectToGround(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (TransformUtil.GetGroundInfo(position + Vector3.up * 5f, out var hitOut, 10f, LayerMask.op_Implicit(1084293377)))
		{
			return hitOut.point;
		}
		return position;
	}

	private static bool SupportsDebugMove(BaseEntity npc, out string error)
	{
		error = null;
		FSMComponent fSMComponent = default;
		if (((Component)npc).TryGetComponent<FSMComponent>(ref fSMComponent))
		{
			if (fSMComponent.SupportsDebugMove)
			{
				return true;
			}
			error = DescribeNpc(npc) + "'s FSM has no debug move state registered - add a RegisterDebugMoveTo call to its InitShared.";
			return false;
		}
		BaseAIBrain baseAIBrain = default;
		if (((Component)npc).TryGetComponent<BaseAIBrain>(ref baseAIBrain))
		{
			if (baseAIBrain.SupportsDebugMove)
			{
				return true;
			}
			error = DescribeNpc(npc) + "'s brain has no move-to-point design in its Designs list.";
			return false;
		}
		error = DescribeNpc(npc) + " has neither an FSM nor a brain.";
		return false;
	}

	private static BaseEntity TraceForNpc(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ResolveNpc(GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, player.eyes.HeadRay(), debugMoveSelectRadius, debugMoveRange, 133120, (QueryTriggerInteraction)0, player) as BaseEntity);
	}

	private static BaseEntity ResolveNpc(BaseEntity hit)
	{
		BaseEntity baseEntity = hit;
		while ((Object)(object)baseEntity != (Object)null)
		{
			if (ComponentEx.HasComponent<FSMComponent>((Component)(object)baseEntity) || ComponentEx.HasComponent<BaseAIBrain>((Component)(object)baseEntity))
			{
				return baseEntity;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		return null;
	}

	private static bool TraceForPosition(BasePlayer player, out Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (GamePhysics.Trace(player.eyes.HeadRay(), 0f, out var hitInfo, debugMoveRange, 1084293377, (QueryTriggerInteraction)0, player))
		{
			position = hitInfo.point;
			return true;
		}
		position = default;
		return false;
	}

	private static bool TryParseGait(string gaitName, out RustNavMeshAgent.Speeds gait)
	{
		gait = RustNavMeshAgent.Speeds.Run;
		if (string.IsNullOrEmpty(gaitName))
		{
			return true;
		}
		if (!Enum.TryParse<RustNavMeshAgent.Speeds>(gaitName, ignoreCase: true, out gait) || !Enum.IsDefined(typeof(RustNavMeshAgent.Speeds), gait))
		{
			gait = RustNavMeshAgent.Speeds.Run;
			return false;
		}
		return true;
	}

	private static BaseNavigator.NavigationSpeed ToNavigatorSpeed(RustNavMeshAgent.Speeds gait)
	{
		switch (gait)
		{
		case RustNavMeshAgent.Speeds.Sneak:
		case RustNavMeshAgent.Speeds.Walk:
			return BaseNavigator.NavigationSpeed.Slowest;
		case RustNavMeshAgent.Speeds.Jog:
			return BaseNavigator.NavigationSpeed.Slow;
		case RustNavMeshAgent.Speeds.Sprint:
		case RustNavMeshAgent.Speeds.FullSprint:
			return BaseNavigator.NavigationSpeed.Fast;
		default:
			return BaseNavigator.NavigationSpeed.Normal;
		}
	}

	private static string DescribeNpc(BaseEntity npc)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (npc.net == null)
		{
			return npc.ShortPrefabName;
		}
		return $"{npc.ShortPrefabName} ({npc.net.ID})";
	}

	private static void DrawSelection(BasePlayer player, BaseEntity npc)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (debugMoveDraw)
		{
			player.SendConsoleCommand("ddraw.sphere", 5f, Color.cyan, npc.CenterPoint(), 1f);
		}
	}

	private static void DrawMoveOrder(BasePlayer player, BaseEntity npc, Vector3 destination)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (debugMoveDraw)
		{
			player.SendConsoleCommand("ddraw.arrow", 5f, Color.cyan, npc.CenterPoint(), destination, 0.5f);
			player.SendConsoleCommand("ddraw.sphere", 5f, Color.cyan, destination, 0.5f);
		}
	}
}
