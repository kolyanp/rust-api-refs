using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Unity;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using UnityEngine;

namespace ConVar;

[ResetStaticFields]
[Factory("debug")]
public class Debugging : ConsoleSystem
{
	private const string NO_RECOVER_ARG = "--no-recover";

	[ServerVar(Help = "(Generated) When enabled, validates trigger collider configurations each physics update to catch incorrectly parented or sized trigger volumes")]
	[ClientVar(Help = "(Generated) When enabled, validates trigger collider configurations each physics update to catch incorrectly parented or sized trigger volumes")]
	public static bool checktriggers = false;

	[ServerVar(Help = "(Generated) When enabled, validates that trigger colliders are correctly parented to their entities during physics updates; helps catch mis-parenting bugs")]
	public static bool checkparentingtriggers = true;

	[ServerVar]
	[ClientVar(Saved = false, Help = "Shows some debug info for dismount attempts.")]
	public static bool DebugDismounts = false;

	[ClientVar(ClientAdmin = true, Saved = false, Help = "Duration in seconds to keep ddraw for dismount attempts visible")]
	public static float DebugDismountDuration = 30f;

	[ServerVar(Help = "Shows debug info for what objects are causing clipping checks to fail.")]
	public static bool DebugClippingChecks = false;

	[ServerVar(Help = "Do not damage any items")]
	public static bool disablecondition = false;

	[ServerVar(Help = "(Generated) Minimum seconds that must pass after a tutorial ends before another one can start; prevents back-to-back tutorial spam")]
	public static int tutorial_start_cooldown = 60;

	[ServerVar(Help = "(Generated) When enabled, logs mission NPC speech info (speaker, line, trigger) to the console as mission dialogue events fire")]
	public static bool printMissionSpeakInfo = false;

	[ServerVar(Help = "(Generated) Multiplier applied to all puzzle reset timers; values below 1.0 make puzzles reset faster, above 1.0 slower")]
	public static float puzzleResetTimeMultiplier = 1f;

	[ServerVar(Help = "Whether to parent players immediately on spawning to a boat if the bag is on a boat")]
	public static bool bag_respawn_parenting = true;

	[ServerVar(Help = "(Generated) When true, nav mesh obstacle components on loot containers are disabled in the deep sea zone to improve performance in underwater areas")]
	public static bool disableLootNavObstaclesInDeepSea = true;

	[ServerVar(Help = "(Generated) When enabled, logs debug information about object callback invocations to the console; useful for tracing event callback chains")]
	[ClientVar(Help = "(Generated) When enabled, logs debug information about object callback invocations to the console; useful for tracing event callback chains")]
	public static bool callbacks = false;

	[ClientVar(Help = "(Generated) When enabled, Unity Debug.Log output is written to disk; disabling first logs a final message before suppressing further output")]
	[ServerVar(Help = "(Generated) When enabled, Unity Debug.Log output is written to disk; disabling first logs a final message before suppressing further output")]
	public static bool log
	{
		get
		{
			return Debug.unityLogger.logEnabled;
		}
		set
		{
			if (!value)
			{
				Debug.Log((object)"Logging disabled");
			}
			Debug.unityLogger.logEnabled = value;
			if (value)
			{
				Debug.Log((object)"Logging enabled");
			}
		}
	}

	[ClientVar(ClientAdmin = true)]
	[ServerVar(Help = "(Generated) Generates and logs a render info report showing draw calls, batch counts, triangle counts, and shadow caster counts for the current frame")]
	public static void renderinfo(Arg arg)
	{
		RenderInfo.GenerateReport();
	}

	[ServerVar(Help = "(Generated) Sends a client RPC to the target player enabling or disabling their movement controls; admin only; useful for testing freeze/lock mechanics")]
	public static void enable_player_movement(Arg arg)
	{
		if (arg.IsAdmin)
		{
			bool flag = arg.GetBool(0, def: true);
			BasePlayer basePlayer = ArgEx.Player(arg);
			if ((Object)(object)basePlayer == (Object)null)
			{
				arg.ReplyWith("Must be called from client with player model");
				return;
			}
			basePlayer.ClientRPC(RpcTarget.Player("TogglePlayerMovement", basePlayer), flag);
			arg.ReplyWith((flag ? "enabled" : "disabled") + " player movement");
		}
	}

	[ServerVar(Help = "(Generated) Logs a configurable number of test messages of a given length; used to stress-test console/logging performance and measure output speed")]
	public static void console_spam(Arg arg)
	{
		int num = Mathf.Clamp(arg.GetInt(0, 100), 1, 100000);
		int count = arg.GetInt(1, 50);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < num; i++)
		{
			Debug.Log((object)new string((char)(97 + i % 26), count));
		}
		stopwatch.Stop();
		Debug.Log((object)$"Took {stopwatch.ElapsedMilliseconds}ms to log {num} lines");
	}

	[ServerVar(Help = "(Generated) Prints a message to the server console using the specified ConsoleColor index; useful for testing coloured console output")]
	public static void console_print_color(Arg arg)
	{
		string text = arg.GetString(0, "This is a test colored message");
		int color = arg.GetInt(1, 2);
		ServerConsole.PrintColoured(text, (ConsoleColor)color);
	}

	[ClientVar(Help = "(Generated) Stalls the main thread for the given duration in seconds (clamped 0-1); admin-only; used to test timeout handling and watchdog systems")]
	[ServerVar(Help = "(Generated) Stalls the main thread for the given duration in seconds (clamped 0-1); admin-only; used to test timeout handling and watchdog systems")]
	public static void stall(Arg arg)
	{
		float num = Mathf.Clamp(arg.GetFloat(0), 0f, 1f);
		arg.ReplyWith("Stalling for " + num + " seconds...");
		Thread.Sleep(Mathf.RoundToInt(num * 1000f));
	}

	[ServerVar(Help = "Repair all items in inventory")]
	public static void repair_inventory(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			return;
		}
		List<Item> list = Pool.Get<List<Item>>();
		basePlayer.inventory.GetAllItems(list);
		foreach (Item item in list)
		{
			if (item != null)
			{
				item.maxCondition = item.info.condition.max;
				item.condition = item.maxCondition;
				item.MarkDirty();
			}
			if (item.contents == null)
			{
				continue;
			}
			foreach (Item item2 in item.contents.itemList)
			{
				item2.maxCondition = item2.info.condition.max;
				item2.condition = item2.maxCondition;
				item2.MarkDirty();
			}
		}
		Pool.Free<Item>(ref list, false);
	}

	[ServerVar(Help = "(Generated) Spawns a clone of the calling player at a configurable height with a parachute deployed and their belt and wear inventories copied")]
	public static void spawnParachuteTester(Arg arg)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		float num = arg.GetFloat(0, 50f);
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer basePlayer2 = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", ((Component)basePlayer).transform.position + Vector3.up * num, Quaternion.LookRotation(basePlayer.eyes.BodyForward())) as BasePlayer;
		basePlayer2.Spawn();
		basePlayer2.eyes.rotation = basePlayer.eyes.rotation;
		basePlayer2.SendNetworkUpdate();
		Inventory.copyTo(basePlayer, basePlayer2);
		if (!basePlayer2.HasValidParachuteEquipped())
		{
			basePlayer2.inventory.containerWear.GiveItem(ItemManager.CreateByName("parachute", 1, 0uL));
		}
		basePlayer2.RequestParachuteDeploy();
	}

	[ServerVar(Help = "(Generated) Triggers the tutorial island ending cinematic for the calling player; spawns a kayak at the designated mount point and mounts the player to it")]
	public static string testTutorialCinematic(Arg arg)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsInTutorial)
		{
			return "Requires a player";
		}
		TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
		if ((Object)(object)currentTutorialIsland == (Object)null)
		{
			return "Invalid island";
		}
		Transform val = ((Component)currentTutorialIsland).transform.FindChildRecursive("KayakMissionPoint");
		if ((Object)(object)val == (Object)null)
		{
			return "Can't find KayakMissionPoint on island";
		}
		Kayak obj = GameManager.server.CreateEntity("assets/content/vehicles/boats/kayak/kayak.prefab", val.position, val.rotation) as Kayak;
		obj.Spawn();
		obj.WantsMount(basePlayer);
		currentTutorialIsland.StartEndingCinematic(basePlayer);
		return "Playing cinematic";
	}

	[ServerVar(Help = "If a player ends up stuck on a tutorial for any reason this will clear the island and reset the player (will also kill player)")]
	public static void clearTutorialForPlayer(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if ((Object)(object)player == (Object)null)
		{
			arg.ReplyWith("Please provide a player");
		}
		else if (player.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = player.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				currentTutorialIsland.Return();
			}
			player.ClearTutorial();
			player.Hurt(99999f);
			player.ClearTutorial_PostDeath();
		}
	}

	[ServerVar(Help = "<shortname> (optional: <radius>) - Delete entities with the given short prefab name")]
	public static void deleteEntitiesByShortname(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0).ToLower();
		float num = arg.GetFloat(1);
		BasePlayer basePlayer = ArgEx.Player(arg);
		PooledList<BaseNetworkable> val = Pool.Get<PooledList<BaseNetworkable>>();
		try
		{
			Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BaseNetworkable current = enumerator.Current;
					if (current.ShortPrefabName == text && (num == 0f || ((Object)(object)basePlayer != (Object)null && basePlayer.Distance(current as BaseEntity) <= num)))
					{
						((List<BaseNetworkable>)(object)val).Add(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			if (CollectionEx.IsEmpty((ICollection<BaseNetworkable>)val))
			{
				arg.ReplyWith("Did not find any " + text);
				return;
			}
			arg.ReplyWith($"Deleting {((List<BaseNetworkable>)(object)val).Count} {text}");
			foreach (BaseNetworkable item in (List<BaseNetworkable>)(object)val)
			{
				item.Kill();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Delete entities by id. Supports multiple arguments")]
	public static void deleteEntityById(Arg arg)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < arg.Args.Length; i++)
		{
			NetworkableId entityID = ArgEx.GetEntityID(arg, i);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if ((Object)(object)baseNetworkable != (Object)null)
			{
				stringBuilder.AppendLine($"Deleting {baseNetworkable}");
				baseNetworkable.Kill();
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Logs all server entity network group IDs and prefab names to the console; useful for debugging network visibility and group assignment")]
	public static void printgroups(Arg arg)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"Server");
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				Debug.Log((object)string.Format("{0}:{1}{2}", current.PrefabName, current.net.group.ID, current.net.group.restricted ? "/Restricted" : string.Empty));
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Takes you in and out of your current network group, causing you to delete and then download all entities in your PVS again")]
	public static void flushgroup(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			basePlayer.net.SwitchGroup(BaseNetworkable.LimboNetworkGroup);
			basePlayer.UpdateNetworkGroup();
		}
	}

	[ServerVar(Help = "Break the current held object")]
	public static void breakheld(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		activeItem?.LoseCondition(activeItem.condition * 2f);
	}

	[ServerVar(Help = "Breaks the currently held shield")]
	public static void breakshield(Arg arg)
	{
		if (ArgEx.Player(arg).TryGetActiveShield(out var foundShield) && foundShield.GetItem() != null)
		{
			foundShield.GetItem().LoseCondition(999f);
		}
	}

	[ServerVar(Help = "Almost break the current held object")]
	public static void breakheld_almost(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		if (activeItem != null && activeItem.hasCondition)
		{
			activeItem.condition = 1f;
		}
	}

	[ServerVar(Help = "Reset all puzzles. Optionally provide a number to only reset puzzles within a radius.")]
	public static void puzzlereset(Arg arg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		Enumerator<PuzzleReset> enumerator = PuzzleReset.AllResets.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PuzzleReset current = enumerator.Current;
				BasePlayer basePlayer = ArgEx.Player(arg);
				if ((Object)(object)basePlayer != (Object)null)
				{
					StringView[] args = arg.Args;
					if (args != null && args.Length != 0)
					{
						int num = arg.GetInt(0, int.MaxValue);
						if (Vector3.Distance(((Component)current).transform.position, ((Component)basePlayer).transform.position) > (float)num)
						{
							continue;
						}
					}
				}
				stringBuilder.AppendLine($"Resetting puzzle at: {((Component)current).transform.position}");
				current.DoReset();
				current.ResetTimer();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(EditorOnly = true, Help = "respawn all puzzles from their prefabs")]
	public static void puzzleprefabrespawn(Arg arg)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		foreach (BaseNetworkable item in BaseNetworkable.serverEntities.Where((BaseNetworkable x) => x is IOEntity && PrefabAttribute.server.Find<Construction>(x.prefabID) == null).ToList())
		{
			item.Kill();
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			GameObject val = GameManager.server.FindPrefab(((Object)((Component)monument).gameObject).name);
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Dictionary<IOEntity, IOEntity> dictionary = new Dictionary<IOEntity, IOEntity>();
			IOEntity[] componentsInChildren = val.GetComponentsInChildren<IOEntity>(true);
			foreach (IOEntity iOEntity in componentsInChildren)
			{
				Quaternion rot = ((Component)monument).transform.rotation * ((Component)iOEntity).transform.rotation;
				Vector3 pos = ((Component)monument).transform.TransformPoint(((Component)iOEntity).transform.position);
				BaseEntity newEntity = GameManager.server.CreateEntity(iOEntity.PrefabName, pos, rot);
				IOEntity iOEntity2 = newEntity as IOEntity;
				if (!((Object)(object)iOEntity2 != (Object)null))
				{
					continue;
				}
				dictionary.Add(iOEntity, iOEntity2);
				DoorManipulator doorManipulator = newEntity as DoorManipulator;
				if ((Object)(object)doorManipulator != (Object)null)
				{
					List<Door> list = Pool.Get<List<Door>>();
					global::Vis.Entities(((Component)newEntity).transform.position, 10f, list, -1, (QueryTriggerInteraction)2);
					Door door = list.OrderBy((Door x) => x.Distance(((Component)newEntity).transform.position)).FirstOrDefault();
					if ((Object)(object)door != (Object)null)
					{
						doorManipulator.targetDoor = door;
					}
					Pool.FreeUnmanaged<Door>(ref list);
				}
				CardReader cardReader = newEntity as CardReader;
				if ((Object)(object)cardReader != (Object)null)
				{
					CardReader cardReader2 = iOEntity as CardReader;
					if ((Object)(object)cardReader2 != (Object)null)
					{
						cardReader.accessLevel = cardReader2.accessLevel;
						cardReader.accessDuration = cardReader2.accessDuration;
					}
				}
				TimerSwitch timerSwitch = newEntity as TimerSwitch;
				if ((Object)(object)timerSwitch != (Object)null)
				{
					TimerSwitch timerSwitch2 = iOEntity as TimerSwitch;
					if ((Object)(object)timerSwitch2 != (Object)null)
					{
						timerSwitch.timerLength = timerSwitch2.timerLength;
					}
				}
			}
			foreach (KeyValuePair<IOEntity, IOEntity> item2 in dictionary)
			{
				IOEntity key = item2.Key;
				IOEntity value = item2.Value;
				for (int num2 = 0; num2 < key.outputs.Length; num2++)
				{
					if (!((Object)(object)key.outputs[num2].connectedTo.ioEnt == (Object)null))
					{
						value.outputs[num2].connectedTo.ioEnt = dictionary[key.outputs[num2].connectedTo.ioEnt];
						value.outputs[num2].connectedToSlot = key.outputs[num2].connectedToSlot;
					}
				}
			}
			foreach (IOEntity value2 in dictionary.Values)
			{
				value2.Spawn();
			}
		}
	}

	[ServerVar(Help = "Break all the items in your inventory whose name match the passed string")]
	public static void breakitem(Arg arg)
	{
		string text = arg.GetString(0);
		foreach (Item item in ArgEx.Player(arg).inventory.containerMain.itemList)
		{
			if (StringEx.Contains(item.info.shortname, text, CompareOptions.IgnoreCase) && item.hasCondition)
			{
				item.LoseCondition(item.condition * 2f);
			}
		}
	}

	[ServerVar(ClientAdmin = true, Help = "Refills the vital of a target player. eg. debug.refillsvital jim - leave blank to target yourself, can take multiple players at once. Will revive players if they are injured. To disable this, pass in --no-recover as the first argument.")]
	public static void refillvitals(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		int num = 0;
		bool shouldPlayerRecover = true;
		if (arg.GetString(0) == "--no-recover")
		{
			shouldPlayerRecover = false;
			num++;
		}
		arg.TryRemoveKeyBindEventArgs();
		if (arg.Args == null || num >= arg.Args.Length)
		{
			RefillPlayerVitals(basePlayer, shouldPlayerRecover);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num; i < arg.Args.Length; i++)
		{
			string text = arg.GetString(i);
			BasePlayer basePlayer2 = ((!(text == basePlayer.displayName)) ? (string.IsNullOrEmpty(text) ? null : ArgEx.GetPlayerOrSleeperOrBot(arg, i)) : basePlayer);
			if ((Object)(object)basePlayer2 == (Object)null)
			{
				stringBuilder.AppendLine("Could not find player '" + text + "'");
				continue;
			}
			RefillPlayerVitals(basePlayer2, shouldPlayerRecover);
			stringBuilder.AppendLine("Refilled '" + text + "' vitals");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(ClientAdmin = true, Help = "Refills the vitals of all active players on the server. Will revive players if they are injured. To disable this, pass in --no-recover as the first argument.")]
	public static void refillvitalsall(Arg arg)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		bool shouldPlayerRecover = arg.GetString(0) != "--no-recover";
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null))
				{
					RefillPlayerVitals(current, shouldPlayerRecover);
					stringBuilder.AppendLine("Refilled player '" + current.displayName + "' vitals");
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		enumerator = BasePlayer.bots.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current2 = enumerator.Current;
				if (!((Object)(object)current2 == (Object)null))
				{
					RefillPlayerVitals(current2, shouldPlayerRecover);
					stringBuilder.AppendLine("Refilled bot '" + current2.displayName + "' vitals");
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static void RefillPlayerVitals(BasePlayer player, bool shouldPlayerRecover)
	{
		if (shouldPlayerRecover && player.IsWounded())
		{
			player.StopWounded();
		}
		AdjustHealth(player, 1000f);
		AdjustCalories(player, 1000f);
		AdjustHydration(player, 1000f);
		AdjustRadiation(player, -10000f);
		AdjustBleeding(player, -10000f);
	}

	[ServerVar(Help = "To disable revival if player is downed, pass in --no-recover as the first argument.")]
	public static void heal(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		bool flag = true;
		int num = 0;
		if (arg.GetString(0) == "--no-recover")
		{
			flag = false;
			num++;
		}
		if (flag && basePlayer.IsWounded())
		{
			basePlayer.StopWounded();
		}
		AdjustHealth(basePlayer, arg.GetInt(num, 1));
	}

	[ServerVar(Help = "(Generated) Deals a specified amount of bullet damage to the calling player; optionally targets a named bone to test per-bone hit reactions")]
	public static void hurt(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		int num = arg.GetInt(0, 1);
		string text = arg.GetString(1, string.Empty);
		HitInfo hitInfo = new HitInfo(basePlayer, basePlayer, DamageType.Bullet, num);
		if (!string.IsNullOrEmpty(text))
		{
			hitInfo.HitBone = StringPool.Get(text);
		}
		basePlayer.OnAttacked(hitInfo);
	}

	[ServerVar(Help = "(Generated) Adds a specified amount of calories to the calling player at a configurable rate; useful for quickly testing hunger-related mechanics")]
	public static void eat(Arg arg)
	{
		AdjustCalories(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	[ServerVar(Help = "(Generated) Adds a specified amount of hydration to the calling player at a configurable rate; useful for quickly testing thirst-related mechanics")]
	public static void drink(Arg arg)
	{
		AdjustHydration(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player health to the specified value; useful for testing low-health or death scenarios")]
	public static void sethealth(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		float num = arg.GetFloat(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if ((Object)(object)usePlayer == (Object)null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		usePlayer.SetHealth(num);
		arg.ReplyWith($"Set health to {num}");
	}

	[ServerVar(Help = "(Generated) Overrides the maximum health of the calling player or a named target; pass 0 to reset to the default value")]
	public static void setmaxhealth(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int num = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if ((Object)(object)usePlayer == (Object)null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		usePlayer.OverrideMaxHealth(num);
		if (num <= 0)
		{
			arg.ReplyWith("Reset max health");
		}
		else
		{
			arg.ReplyWith($"Set max health to {num}");
		}
	}

	[ServerVar(Help = "(Generated) Deals enough bullet damage to bring the calling player or a named target to the specified health value")]
	public static void setdamage(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int num = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if (Object.op_Implicit((Object)(object)usePlayer))
		{
			float damageAmount = usePlayer.health - (float)num;
			HitInfo info = new HitInfo(basePlayer, basePlayer, DamageType.Bullet, damageAmount);
			usePlayer.OnAttacked(info);
		}
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player calorie level to the specified value directly")]
	public static void setfood(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Calories);
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player hydration level to the specified value directly")]
	public static void setwater(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Hydration);
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player radiation level to the specified value directly")]
	public static void setradiation(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Radiation);
	}

	private static void AdjustHealth(BasePlayer player, float amount, string bone = null)
	{
		player.health += amount;
	}

	private static void AdjustCalories(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Calories, amount, time);
	}

	private static void AdjustHydration(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Hydration, amount, time);
	}

	private static void AdjustRadiation(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.SetAttribute(MetabolismAttribute.Type.Radiation, amount);
	}

	private static void AdjustBleeding(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.SetAttribute(MetabolismAttribute.Type.Bleeding, amount);
	}

	private static void setattribute(Arg arg, MetabolismAttribute.Type type)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int num = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if (Object.op_Implicit((Object)(object)usePlayer))
		{
			usePlayer.metabolism.SetAttribute(type, num);
		}
	}

	private static BasePlayer GetUsePlayer(Arg arg, int playerArgument)
	{
		BasePlayer basePlayer = null;
		if (arg.HasArgs(playerArgument + 1))
		{
			BasePlayer player = ArgEx.GetPlayer(arg, playerArgument);
			if (!Object.op_Implicit((Object)(object)player))
			{
				return null;
			}
			return player;
		}
		return ArgEx.Player(arg);
	}

	[ServerVar(Help = "(Generated) Resets all sleeping bag respawn cooldown timers for the calling player, allowing immediate re-use of all their bags")]
	public static void ResetSleepingBagTimers(Arg arg)
	{
		SleepingBag.ResetTimersForPlayer(ArgEx.Player(arg));
	}

	[ServerVar(Help = "Deducts the given number of hours from all spoilable food on the server")]
	public static void FoodSpoilingDeductTimeHours(Arg arg)
	{
		ItemModFoodSpoiling.DeductTimeFromAll(TimeSpan.FromHours(arg.GetFloat(0)));
	}

	[ServerVar(Help = "Spoils all food on the server")]
	public static void FoodSpoilingSpoilAll()
	{
		ItemModFoodSpoiling.DeductTimeFromAll(TimeSpan.MaxValue);
	}

	[ServerVar(Help = "Applies the given number of hours to all food in the players inventory")]
	public static void FoodSpoilingInventoryHours(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		int num = arg.GetInt(0);
		PooledList<Item> spoilList = Pool.Get<PooledList<Item>>();
		try
		{
			FindSpoilableItems(basePlayer.inventory.containerMain.itemList);
			FindSpoilableItems(basePlayer.inventory.containerBelt.itemList);
			foreach (Item item in (List<Item>)(object)spoilList)
			{
				ItemModFoodSpoiling.FoodSpoilingWorkQueue.DeductTimeFromFoodItem(item, (float)num * 60f * 60f, setDirty: true);
			}
		}
		finally
		{
			if (spoilList != null)
			{
				((IDisposable)spoilList).Dispose();
			}
		}
		void FindSpoilableItems(List<Item> items)
		{
			ItemModFoodSpoiling itemModFoodSpoiling = default(ItemModFoodSpoiling);
			foreach (Item item2 in items)
			{
				if (((Component)item2.info).TryGetComponent<ItemModFoodSpoiling>(ref itemModFoodSpoiling))
				{
					((List<Item>)(object)spoilList).Add(item2);
				}
			}
		}
	}

	[ServerVar(Help = "(Generated) Forces all chickens within a given radius of the calling player to immediately spawn an egg; useful for testing egg drop and collection logic")]
	public static void ForceChickensSpawnEgg(Arg arg)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float radius = arg.GetFloat(0, 50f);
		if ((Object)(object)ArgEx.Player(arg) == (Object)null)
		{
			return;
		}
		PooledList<Chicken> val = Pool.Get<PooledList<Chicken>>();
		try
		{
			global::Vis.Entities(((Component)ArgEx.Player(arg)).transform.position, radius, (List<Chicken>)(object)val, 2048, (QueryTriggerInteraction)2);
			foreach (Chicken item in (List<Chicken>)(object)val)
			{
				if (item.isServer)
				{
					item.SpawnEgg();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Drops a specified number of the given item short name as world entities from just in front of the calling player; useful for item physics testing")]
	public static void dropWorldItems(Arg arg)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		int num = arg.GetInt(0);
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(arg.GetString(1));
		Ray val = basePlayer.eyes.HeadRay();
		Vector3 val2 = ((Ray)(ref val)).GetPoint(1f);
		if (!((Object)(object)itemDefinition == (Object)null))
		{
			for (int i = 0; i < num; i++)
			{
				ItemManager.Create(itemDefinition, 1, 0uL, isServerSide: true, 0uL).Drop(val2, Vector3.zero, Quaternion.identity);
				val2 += Vector3.up * 0.3f;
			}
		}
	}

	[ServerVar(Help = "Spawns one of every deployable in a grid")]
	public static void spawn_all_deployables(Arg arg)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper))
		{
			arg.ReplyWith("Must be called by admin player");
			return;
		}
		arg.ReplyWith("Spawning all deployables");
		bool stability = Server.stability;
		Server.stability = false;
		try
		{
			Vector3 position = ((Component)basePlayer).transform.position;
			List<ItemModDeployable> list = (from x in ItemManager.itemList
				select ((Component)x).GetComponent<ItemModDeployable>() into x
				where (Object)(object)x != (Object)null
				select x).ToList();
			int num = 12;
			float num2 = Mathf.Ceil(Mathf.Sqrt((float)list.Count));
			float num3 = num2 * (float)num / 2f;
			Vector3 pos = default(Vector3);
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				((Vector3)(ref pos))._002Ector(position.x - num3 + (float)num * ((float)num4 % num2), position.y, position.z - num3 + (float)num * Mathf.Floor((float)num4 / num2));
				GameManager.server.CreateEntity(list[num4].entityPrefab.resourcePath, pos)?.Spawn();
			}
		}
		finally
		{
			Server.stability = stability;
		}
	}

	[ServerVar(Help = "(Generated) Scans all static respawn areas and kills any whose centre is within 1 metre of another, eliminating duplicate spawn points")]
	public static void removeOverlappingStaticSpawnPoints(Arg arg)
	{
		PooledList<StaticRespawnArea> val = Pool.Get<PooledList<StaticRespawnArea>>();
		try
		{
			foreach (StaticRespawnArea staticRespawnArea2 in StaticRespawnArea.staticRespawnAreas)
			{
				((List<StaticRespawnArea>)(object)val).Add(staticRespawnArea2);
			}
			int num = 0;
			for (int i = 0; i < ((List<StaticRespawnArea>)(object)val).Count; i++)
			{
				StaticRespawnArea staticRespawnArea = ((List<StaticRespawnArea>)(object)val)[i];
				bool flag = false;
				foreach (StaticRespawnArea item in (List<StaticRespawnArea>)(object)val)
				{
					if ((Object)(object)item != (Object)(object)staticRespawnArea && item.Distance((BaseEntity)staticRespawnArea) < 1f)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					((List<StaticRespawnArea>)(object)val).RemoveAt(i);
					i--;
					num++;
					staticRespawnArea.Kill();
				}
			}
			arg.ReplyWith($"Destroyed {num} overlapping static spawn points");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Sets the ore fill percentage on all unloadable train cars within 3 metres of the calling player; updates both inventory amounts and visual ore level")]
	public static void setUnloadableCarFillPercent(Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)basePlayer).transform.position;
		PooledList<TrainCarUnloadable> val = Pool.Get<PooledList<TrainCarUnloadable>>();
		try
		{
			global::Vis.Entities(position, 3f, (List<TrainCarUnloadable>)(object)val, 8192, (QueryTriggerInteraction)2);
			float num = Mathf.Clamp01(arg.GetFloat(0));
			foreach (TrainCarUnloadable item in (List<TrainCarUnloadable>)(object)val)
			{
				if (!item.isServer)
				{
					continue;
				}
				foreach (Item item2 in item.GetStorageContainer().inventory.itemList)
				{
					item2.amount = Mathf.Max(Mathf.RoundToInt(num), 1);
				}
				item.SetLootPercentage(num);
				item.SetVisualOreLevel(num);
				item.SendNetworkUpdate();
				arg.ReplyWith($"Set ore level to {num} on {item.PrefabName}");
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "fillTankerModule <item> - Fills the tanker module(s) of the modular car you're looking at with the given liquid (e.g. water, water.salt, crude.oil)")]
	public static void fillTankerModule(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player.");
			return;
		}
		string text = arg.GetString(0);
		if (string.IsNullOrEmpty(text))
		{
			arg.ReplyWith("Please provide a liquid item shortname (e.g. water, water.salt, crude.oil).");
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(text);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			arg.ReplyWith("Could not find an item with shortname '" + text + "'.");
			return;
		}
		BaseModularVehicle lookedAtModularCar = GetLookedAtModularCar(basePlayer);
		if ((Object)(object)lookedAtModularCar == (Object)null)
		{
			arg.ReplyWith("Not looking at a modular car.");
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BaseVehicleModule attachedModuleEntity in lookedAtModularCar.AttachedModuleEntities)
		{
			if (!(attachedModuleEntity is VehicleModuleStorage vehicleModuleStorage) || !(vehicleModuleStorage.GetContainer() is LiquidContainer { inventory: { } inventory } liquidContainer))
			{
				continue;
			}
			int amount = ((inventory.maxStackSize > 0) ? inventory.maxStackSize : itemDefinition.stackable);
			Item item = liquidContainer.GetLiquidItem();
			if (item != null && (Object)(object)item.info != (Object)(object)itemDefinition)
			{
				item.Remove();
				item = null;
			}
			if (item != null)
			{
				item.amount = amount;
				item.MarkDirty();
			}
			else
			{
				inventory.AddItem(itemDefinition, amount, 0uL, ItemContainer.LimitStack.All);
			}
			Item liquidItem = liquidContainer.GetLiquidItem();
			if (liquidItem != null && (Object)(object)liquidItem.info == (Object)(object)itemDefinition)
			{
				if ((Object)(object)itemDefinition == (Object)(object)VehicleModuleStorage.CrudeItem)
				{
					liquidItem.LockUnlock(bNewState: true);
				}
				num++;
			}
			else
			{
				num2++;
			}
		}
		if (num == 0 && num2 == 0)
		{
			arg.ReplyWith("That car (" + lookedAtModularCar.ShortPrefabName + ") has no tanker (liquid storage) module.");
		}
		else if (num == 0)
		{
			arg.ReplyWith("The tanker module(s) on " + lookedAtModularCar.ShortPrefabName + " would not accept '" + itemDefinition.shortname + "'.");
		}
		else
		{
			string text2 = $"Filled {num} tanker module(s) on {lookedAtModularCar.ShortPrefabName} with {itemDefinition.shortname}.";
			if (num2 > 0)
			{
				text2 += $" ({num2} module(s) rejected the item.)";
			}
			arg.ReplyWith(text2);
		}
	}

	[ServerVar(Help = "emptyTankerModule - Clears the contents of the tanker module(s) of the modular car you're looking at")]
	public static void emptyTankerModule(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player.");
			return;
		}
		BaseModularVehicle lookedAtModularCar = GetLookedAtModularCar(basePlayer);
		if ((Object)(object)lookedAtModularCar == (Object)null)
		{
			arg.ReplyWith("Not looking at a modular car.");
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BaseVehicleModule attachedModuleEntity in lookedAtModularCar.AttachedModuleEntities)
		{
			if (attachedModuleEntity is VehicleModuleStorage vehicleModuleStorage && vehicleModuleStorage.GetContainer() is LiquidContainer liquidContainer)
			{
				num2++;
				Item liquidItem = liquidContainer.GetLiquidItem();
				if (liquidItem != null)
				{
					liquidItem.LockUnlock(bNewState: false);
					liquidItem.Remove();
					num++;
				}
			}
		}
		if (num2 == 0)
		{
			arg.ReplyWith("That car (" + lookedAtModularCar.ShortPrefabName + ") has no tanker (liquid storage) module.");
		}
		else
		{
			arg.ReplyWith((num > 0) ? $"Emptied {num} tanker module(s) on {lookedAtModularCar.ShortPrefabName}." : ("The tanker module(s) on " + lookedAtModularCar.ShortPrefabName + " were already empty."));
		}
	}

	private static BaseModularVehicle GetLookedAtModularCar(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, player.eyes.HeadRay(), 0f, 12f, 1218652417, (QueryTriggerInteraction)0);
		while ((Object)(object)baseNetworkable != (Object)null)
		{
			if (baseNetworkable is BaseModularVehicle result)
			{
				return result;
			}
			if (baseNetworkable is BaseVehicleModule baseVehicleModule && (Object)(object)baseVehicleModule.Vehicle != (Object)null)
			{
				return baseVehicleModule.Vehicle;
			}
			baseNetworkable = baseNetworkable.GetParentEntity();
		}
		return null;
	}

	[ServerVar(Help = "fillmounts <radius> - Spawns and mounts a player on every mount point in radius")]
	public static void fillmounts(Arg arg)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		int num = Mathf.Clamp(arg.GetInt(0), 0, 100);
		if (num == 0)
		{
			arg.ReplyWith("Must supply a radius > 0!");
			return;
		}
		Vector3 position = ((Component)basePlayer).transform.position;
		int layerMask = 1218521345;
		PooledList<BaseMountable> val = Pool.Get<PooledList<BaseMountable>>();
		try
		{
			StringBuilder stringBuilder = Pool.Get<StringBuilder>();
			global::Vis.Entities(position, num, (List<BaseMountable>)(object)val, layerMask, (QueryTriggerInteraction)2);
			foreach (BaseMountable item in (List<BaseMountable>)(object)val)
			{
				if (item.isClient)
				{
					continue;
				}
				if (item is RidableHorse ridableHorse)
				{
					if (ridableHorse.HasSingleSaddle)
					{
						TrySpawnAndMountPlayer(ridableHorse.mountPoints[0].mountable, stringBuilder);
					}
					else if (ridableHorse.HasDoubleSaddle)
					{
						TrySpawnAndMountPlayer(ridableHorse.mountPoints[1].mountable, stringBuilder);
						TrySpawnAndMountPlayer(ridableHorse.mountPoints[2].mountable, stringBuilder);
					}
				}
				else if (item is BaseVehicle { allMountPoints: var allMountPoints })
				{
					foreach (BaseVehicle.MountPointInfo item2 in allMountPoints)
					{
						if (item2 != null && (Object)(object)item2.mountable != (Object)null)
						{
							TrySpawnAndMountPlayer(item2.mountable, stringBuilder);
						}
					}
				}
				else
				{
					TrySpawnAndMountPlayer(item, stringBuilder);
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			string text = stringBuilder.ToString();
			Pool.FreeUnmanaged(ref stringBuilder);
			arg.ReplyWith((text.Length > 0) ? text : "Didn't find any eligible/unoccupied mount points in this radius.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void TrySpawnAndMountPlayer(BaseMountable mountable, StringBuilder sb)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!mountable.AnyMounted())
		{
			BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab") as BasePlayer;
			basePlayer.Spawn();
			mountable.AttemptMount(basePlayer, doMountChecks: false);
			if (!basePlayer.isMounted)
			{
				sb.AppendLine("Failed to mount a player to: " + mountable.ShortPrefabName);
				basePlayer.Kill();
			}
			else
			{
				basePlayer.UpdateNetworkGroup();
				sb.AppendLine("Mounted a player to: " + mountable.ShortPrefabName);
			}
		}
	}

	[ServerVar(Help = "Spawn lots of IO entities to lag the server")]
	public static void bench_io(Arg arg)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsAdmin)
		{
			return;
		}
		int num = arg.GetInt(0, 50);
		string name = arg.GetString(1, "water_catcher_small");
		List<IOEntity> list = new List<IOEntity>();
		WaterCatcher waterCatcher = null;
		Vector3 position = ((Component)ArgEx.Player(arg)).transform.position;
		string[] array = (from x in GameManifest.Current.entities
			where StringEx.Contains(Path.GetFileNameWithoutExtension(x), name, CompareOptions.IgnoreCase)
			select x.ToLower()).ToArray();
		if (array.Length == 0)
		{
			arg.ReplyWith("Couldn't find io prefab \"" + array[0] + "\"");
			return;
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				Debug.Log((object)$"{arg} failed to find io entity \"{name}\"");
				arg.ReplyWith("Unknown entity - could be:\n\n" + string.Join("\n", array.Select(Path.GetFileNameWithoutExtension).ToArray()));
				return;
			}
			array[0] = text;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			Vector3 pos = position + new Vector3((float)(num2 * 5), 0f, 0f);
			Quaternion identity = Quaternion.identity;
			BaseEntity baseEntity = GameManager.server.CreateEntity(array[0], pos, identity);
			if (!Object.op_Implicit((Object)(object)baseEntity))
			{
				continue;
			}
			baseEntity.Spawn();
			WaterCatcher component = ((Component)baseEntity).GetComponent<WaterCatcher>();
			if (Object.op_Implicit((Object)(object)component))
			{
				list.Add(component);
				if ((Object)(object)waterCatcher != (Object)null)
				{
					Connect(waterCatcher, component);
				}
				if (num2 == num - 1)
				{
					Connect(component, list.First());
				}
				waterCatcher = component;
			}
		}
		static void Connect(IOEntity InputIOEnt, IOEntity OutputIOEnt)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			int num3 = 0;
			int num4 = 0;
			WireTool.WireColour wireColour = WireTool.WireColour.Gray;
			IOEntity.IOSlot iOSlot = InputIOEnt.inputs[num3];
			IOEntity.IOSlot obj = OutputIOEnt.outputs[num4];
			iOSlot.connectedTo.Set(OutputIOEnt);
			iOSlot.connectedToSlot = num4;
			iOSlot.wireColour = wireColour;
			iOSlot.connectedTo.Init();
			obj.connectedTo.Set(InputIOEnt);
			obj.connectedToSlot = num3;
			obj.wireColour = wireColour;
			obj.connectedTo.Init();
			obj.linePoints = (Vector3[])(object)new Vector3[2]
			{
				Vector3.zero,
				((Component)OutputIOEnt).transform.InverseTransformPoint(((Component)InputIOEnt).transform.TransformPoint(iOSlot.handlePosition))
			};
			OutputIOEnt.MarkDirtyForceUpdateOutputs();
			OutputIOEnt.SendNetworkUpdate();
			InputIOEnt.SendNetworkUpdate();
			OutputIOEnt.SendChangedToRoot(forceUpdate: true);
		}
	}

	[Help("Arg0: mission stage (int), Arg1: block objective resetting (bool, default false)")]
	[ServerVar]
	public static void completeMissionStage(Arg arg)
	{
		int num = arg.GetInt(0, -1);
		bool flag = arg.GetBool(1);
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer != (Object)null) || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
			if (!objectiveStatus.completed && (i == num || (num == -1 && !objectiveStatus.completed)))
			{
				MissionObjective missionObjective = instance.GetMission().objectives[i].Get();
				missionObjective.ServerObjectiveStarted(basePlayer, i, instance);
				missionObjective.CompleteObjective(i, instance, basePlayer);
				if (flag)
				{
					objectiveStatus.blockReset = true;
				}
				break;
			}
		}
	}

	[ServerVar(Help = "(Generated) Completes all incomplete objectives in the calling player active mission, triggering the mission completion flow")]
	public static void completeMission(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer != (Object)null) || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			if (!instance.objectiveStatuses[i].completed)
			{
				instance.GetMission().objectives[i].objective.CompleteObjective(i, instance, basePlayer);
			}
		}
	}

	[ServerVar(Help = "Prints out the topologies at your position")]
	public static void print_topologies(Arg arg)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected I4, but got Unknown
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		StringBuilder stringBuilder = Pool.Get<StringBuilder>();
		int topology = TerrainMeta.TopologyMap.GetTopology(((Component)basePlayer).transform.position);
		foreach (Enum value in Enum.GetValues(typeof(Enum)))
		{
			int num = (int)value;
			if ((topology & num) == num)
			{
				stringBuilder.AppendLine(((object)value/*cast due to constrained. prefix*/).ToString());
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
		Pool.FreeUnmanaged(ref stringBuilder);
	}

	[ServerUserVar]
	public static void startTutorial(Arg arg)
	{
		if (!Server.tutorialEnabled)
		{
			arg.ReplyWith("Tutorial is not enabled on this server");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer != (Object)null && !basePlayer.IsInTutorial)
		{
			basePlayer.StartTutorial(triggerAnalytics: false);
		}
	}

	[ServerVar(Help = "(Generated) Immediately completes the calling player tutorial by triggering the island completion callback; bypasses normal progression")]
	public static void completeTutorial(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer != (Object)null && basePlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				currentTutorialIsland.OnPlayerCompletedTutorial(basePlayer, isQuit: false, triggerAnalytics: false);
			}
		}
	}

	[ServerUserVar(ServerAdmin = false)]
	public static void quitTutorial(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer != (Object)null && basePlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
			if ((Object)(object)currentTutorialIsland != (Object)null)
			{
				currentTutorialIsland.OnPlayerCompletedTutorial(basePlayer, isQuit: true, triggerAnalytics: true);
			}
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all active tutorial islands showing index, network group ID, assigned player name, duration, and connection state")]
	public static void tutorialStatus(Arg arg)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		ListHashSet<TutorialIsland> tutorialList = TutorialIsland.GetTutorialList(isServer: true);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[5] { "Index", "ID", "Player Name", "Duration", "IsConnected" });
			int num = 0;
			Enumerator<TutorialIsland> enumerator = tutorialList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TutorialIsland current = enumerator.Current;
					BasePlayer basePlayer = current.ForPlayer.Get(serverside: true);
					val.AddRow(new string[5]
					{
						num++.ToString(),
						(current.net.group.ID - 1).ToString(),
						((Object)(object)basePlayer != (Object)null) ? basePlayer.displayName : "NULL",
						TimeSpanEx.ToShortString(current.TutorialDuration),
						((Object)(object)basePlayer != (Object)null) ? basePlayer.IsConnected.ToString() : "NULL"
					});
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Tutorial islands in use: {num}/{TutorialIsland.MaxTutorialIslandCount}");
			stringBuilder.AppendLine(((object)val).ToString());
			arg.ReplyWith(stringBuilder.ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Make admin invisible")]
	public static void invis(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		bool flag = arg.GetBool(0, !basePlayer.isInvisible);
		if (flag && !basePlayer.isInvisible)
		{
			if (Interface.CallHook("OnPlayerVanish", basePlayer) != null)
			{
				return;
			}
			foreach (Connection subscriber in basePlayer.net.group.subscribers)
			{
				BasePlayer basePlayer2 = subscriber.player as BasePlayer;
				if (subscriber != basePlayer.net.connection && basePlayer.ShouldNetworkTo(basePlayer2) && !basePlayer2.IsSpectating())
				{
					basePlayer.DestroyOnClient(subscriber);
				}
			}
			if (ServerOcclusion.OcclusionEnabled)
			{
				basePlayer.OcclusionMakeSubscribersForget();
			}
			basePlayer.isInvisible = true;
			BasePlayer.invisPlayers.Add(basePlayer);
			basePlayer.DisablePlayerCollider();
			SimpleAIMemory.AddIgnorePlayer(basePlayer);
			BaseEntity.Query.Server.RemovePlayer(basePlayer);
			Interface.CallHook("OnPlayerVanished", basePlayer);
		}
		else if (!flag && basePlayer.isInvisible)
		{
			if (Interface.CallHook("OnPlayerUnvanish", basePlayer) != null)
			{
				return;
			}
			basePlayer.isInvisible = false;
			BasePlayer.invisPlayers.Remove(basePlayer);
			basePlayer.EnablePlayerCollider();
			if (!ServerOcclusion.OcclusionEnabled)
			{
				foreach (Connection subscriber2 in basePlayer.net.group.subscribers)
				{
					BasePlayer player = subscriber2.player as BasePlayer;
					if (basePlayer.ShouldNetworkTo(player))
					{
						basePlayer.SendAsSnapshotWithChildren(player);
					}
				}
			}
			SimpleAIMemory.RemoveIgnorePlayer(basePlayer);
			BaseEntity.Query.Server.RemovePlayer(basePlayer);
			BaseEntity.Query.Server.AddPlayer(basePlayer);
			Interface.CallHook("OnPlayerUnvanished", basePlayer);
		}
		arg.ReplyWith("Invis: " + basePlayer.isInvisible);
		basePlayer.Command("debug.setinvis_ui", basePlayer.isInvisible);
	}

	[ServerVar(Help = "(Generated) Removes all active modifiers (buffs/debuffs) from the calling player; useful for resetting modifier state during testing")]
	public static void clearPlayerModifiers(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			int count = basePlayer.modifiers.All.Count;
			basePlayer.modifiers.RemoveAll();
			arg.ReplyWith($"Removed {count} modifiers");
		}
	}

	[ServerVar(Help = "(Generated) Sets the visual variant index on the building block the calling player is looking at; useful for testing block randomisation visuals")]
	public static void applyBuildingBlockRandomisation(Arg arg)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		int variant = arg.GetInt(0);
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && GamePhysics.Trace(basePlayer.eyes.HeadRay(), 0f, out var hitInfo, 3f, 2097408, (QueryTriggerInteraction)0) && RaycastHitEx.GetEntity(hitInfo) is SimpleBuildingBlock simpleBuildingBlock)
		{
			simpleBuildingBlock.SetVariant(variant);
		}
	}

	[ServerVar(Help = "(Generated) Prints a summary table of all VineSwingingTree and VineMountable entities on the server, including average destination count per mountable")]
	public static void vineSwingingReport(Arg arg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (current is VineSwingingTree)
				{
					num++;
				}
				if (current is VineMountable vineMountable)
				{
					num2++;
					num3 += vineMountable.DestinationCount;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[2] { "Entity", "Count" });
			val.AddRow(new string[2]
			{
				"VineTrees",
				num.ToString()
			});
			val.AddRow(new string[2]
			{
				"VineMountables",
				num2.ToString()
			});
			val.AddRow(new string[2]
			{
				"VineMountableDirections",
				((float)num3 / (float)num2).ToString()
			});
			arg.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Sends a highlight RPC to every VineMountable on the server targeting the calling player; used for visually debugging vine placement")]
	public static void vineSwingingHighlight(Arg arg)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is VineMountable vineMountable)
				{
					vineMountable.Highlight(ArgEx.Player(arg));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Respawns vine trees from their stumps within a given radius of the calling player; reports how many were respawned versus blocked by players")]
	public static void respawnVineTreesInRadius(Arg arg)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		float num = arg.GetFloat(0);
		PooledList<Collider> val = Pool.Get<PooledList<Collider>>();
		try
		{
			GamePhysics.OverlapSphere(((Component)basePlayer).transform.position, num, (List<Collider>)(object)val, 1073741824, (QueryTriggerInteraction)1);
			int num2 = 0;
			int num3 = 0;
			PooledList<VineSwingingTreeStump> val2 = Pool.Get<PooledList<VineSwingingTreeStump>>();
			try
			{
				foreach (Collider item in (List<Collider>)(object)val)
				{
					VineSwingingTreeStump vineSwingingTreeStump = GameObjectEx.ToBaseEntity(item) as VineSwingingTreeStump;
					if ((Object)(object)vineSwingingTreeStump != (Object)null && vineSwingingTreeStump.isServer && !((List<VineSwingingTreeStump>)(object)val2).Contains(vineSwingingTreeStump))
					{
						((List<VineSwingingTreeStump>)(object)val2).Add(vineSwingingTreeStump);
						if (vineSwingingTreeStump.RespawnTree())
						{
							num2++;
						}
						else
						{
							num3++;
						}
					}
				}
				arg.ReplyWith($"Respawned {num2} trees in {num}m, {num3} were blocked by players");
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

	[ServerVar(Help = "(Generated) Prints the world position of every industrial conveyor running in strict mode; helps locate conveyors that are blocking item flow")]
	public static void conveyorStrictModeReport(Arg arg)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		IndustrialConveyor[] array = BaseEntity.Util.FindAll<IndustrialConveyor>();
		foreach (IndustrialConveyor industrialConveyor in array)
		{
			if (industrialConveyor.strictMode)
			{
				stringBuilder.AppendLine($"{((Component)industrialConveyor).transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Sends a configurable number of test custom vital entries to the calling player client for a given duration; used to verify custom vitals UI rendering")]
	public static void test_custom_vitals(Arg arg)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		int num = Mathf.Clamp(arg.GetInt(0, 1), 0, 100);
		int timeLeft = arg.GetInt(1, 60);
		string text = arg.GetString(2, "ss");
		CustomVitals val = new CustomVitals
		{
			vitals = new List<CustomVitalInfo>()
		};
		for (int i = 0; i < num; i++)
		{
			val.vitals.Add(new CustomVitalInfo
			{
				active = true,
				backgroundColor = Color.red,
				iconColor = Color.green,
				leftTextColor = Color.blue,
				rightTextColor = Color.yellow,
				leftText = "Left",
				rightText = "Right {timeleft:" + text + "}",
				timeLeft = timeLeft
			});
		}
		CommunityEntity.ServerInstance.SendCustomVitals(ArgEx.Player(arg), val);
	}

	[ServerVar(Help = "0 = can't throw, 1 = can throw & melee, 2 = only throwable")]
	public static void setthrowable(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("This can only be ran by players");
			return;
		}
		BaseMelee baseMelee = basePlayer.GetHeldEntity() as BaseMelee;
		if ((Object)(object)baseMelee == (Object)null)
		{
			arg.ReplyWith("You must be holding a melee weapon");
			return;
		}
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'setthrowable {0-2}");
			return;
		}
		int num = arg.GetInt(0);
		switch (num)
		{
		case 0:
			baseMelee.canThrowAsProjectile = false;
			baseMelee.onlyThrowAsProjectile = false;
			break;
		case 1:
			baseMelee.canThrowAsProjectile = true;
			baseMelee.onlyThrowAsProjectile = false;
			break;
		case 2:
			baseMelee.canThrowAsProjectile = true;
			baseMelee.onlyThrowAsProjectile = true;
			break;
		default:
			arg.ReplyWith($"Invalid throwable value {num}, must be 0 (not throwable), 1 (throwable) or 2 (only throwable)");
			return;
		}
		baseMelee.SendNetworkUpdate();
		arg.ReplyWith($"Set canThrowAsProjectile to {num} on {baseMelee.ShortPrefabName}");
	}

	[ServerVar(Help = "(Generated) Applies a debug reset time in seconds to all PuzzleReset objects in the scene, shortening their timers for rapid testing")]
	public static void applyPuzzleResetTime(Arg arg)
	{
		float time = arg.GetFloat(0);
		PuzzleReset[] array = Object.FindObjectsByType<PuzzleReset>((FindObjectsSortMode)0);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DebugApplyPuzzleResetTime(time);
		}
	}

	[ServerVar(Help = "(Generated) Prints detailed debug info about the PuzzleReset the calling player is currently inside, including timer state and dependency status")]
	public static void puzzleResetInfo(Arg arg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer bp = ArgEx.Player(arg);
		Enumerator<PuzzleReset> enumerator = PuzzleReset.AllResets.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				PuzzleReset current = enumerator.Current;
				if (!current.IsPlayerInRange(bp))
				{
					continue;
				}
				List<string> list = new List<string>();
				current.GetDebugInfo(list);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(UnityEngine.TransformEx.GetRecursiveName(((Component)current).transform));
				foreach (string item in list)
				{
					stringBuilder.AppendLine(item);
				}
				arg.ReplyWith(stringBuilder.ToString());
				return;
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith("Player is not inside any PuzzleResets");
	}

	[ServerVar(Help = "Find how large of a gap there is. <maxDistance> <stepsize> <maxSize> <layer>")]
	public static void findgap(Arg arg)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper))
		{
			return;
		}
		float num = arg.GetFloat(0, 3f);
		float num2 = arg.GetFloat(1, 0.01f);
		float num3 = arg.GetFloat(2, 0.5f);
		int layerMask = arg.GetInt(3, 2162688);
		Ray ray = basePlayer.eyes.BodyRay();
		if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0.01f, out var _, num, layerMask, (QueryTriggerInteraction)1))
		{
			arg.ReplyWith($"Reduce max distance: hit before {num}m");
			return;
		}
		basePlayer.SendConsoleCommand(DDrawCommand.Line(((Ray)(ref ray)).origin, ((Ray)(ref ray)).origin + ((Ray)(ref ray)).direction * num, 5f, Color.red));
		for (float num4 = num2; num4 <= num3; num4 += num2)
		{
			if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, num4, out var _, num - num4, layerMask, (QueryTriggerInteraction)1))
			{
				arg.ReplyWith($"Gap size: {num4}m");
				return;
			}
		}
		arg.ReplyWith($"Gap larger than {num3}m (or something went wrong!)");
	}

	[ServerVar(Help = "(Generated) Spawns a 100x10 grid of lit furnaces loaded with wood and metal ore near the calling player; used to stress-test the oven cooking system")]
	public static void spawnOvenStressTest(Arg arg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)ArgEx.Player(arg)).transform.position;
		for (int i = 0; i < 100; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				Vector3 pos = position + new Vector3((float)i * 1f, 0f, (float)j * 1f);
				BaseOven baseOven = GameManager.server.CreateEntity("Assets/Prefabs/Deployable/Furnace/furnace.prefab", pos, Quaternion.identity) as BaseOven;
				baseOven.Spawn();
				ItemManager.CreateByName("wood", 1000, 0uL).MoveToContainer(baseOven.inventory, 0);
				ItemManager.CreateByName("metal.ore", 1000, 0uL).MoveToContainer(baseOven.inventory, baseOven.fuelSlots);
				baseOven.StartCooking();
			}
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all ObjectWorkQueue instances showing name, total items processed, current queue length, and cumulative execution time")]
	[ClientVar(ClientAdmin = true)]
	public static void printqueues(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.ResizeColumns(5);
			val.AddColumn("Name");
			val.AddColumn("Processed");
			val.AddColumn("Size");
			val.AddColumn("Capacity");
			val.AddColumn("Execution Time");
			val.ResizeRows(ObjectWorkQueue.All.Count);
			foreach (ObjectWorkQueue item in ObjectWorkQueue.All.OrderBy((ObjectWorkQueue x) => x.Name))
			{
				TimeSpan totalExecutionTime = item.TotalExecutionTime;
				string text = ((totalExecutionTime.TotalMilliseconds < 1000.0) ? $"{Math.Floor(totalExecutionTime.TotalMilliseconds)}ms" : $"{Math.Round(totalExecutionTime.TotalSeconds, 2)}s");
				val.AddValue(item.Name);
				val.AddValue(item.TotalProcessedCount);
				val.AddValue(item.QueueLength);
				val.AddValue(item.Capacity);
				val.AddValue(text);
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Logs a test error and exception for testing error display.")]
	[ClientVar(Help = "Logs a test error and exception for testing error display.")]
	public static void testerror(Arg arg)
	{
		Debug.LogError((object)"Test error message");
		Debug.LogException((Exception)new NullReferenceException("Test NullReferenceException"));
	}

	[ServerVar(Help = "(Generated) Prints the network visibility layer (overworld, tunnel, underwater, etc.) at the calling player position; helps debug layer-based network group assignment")]
	public static void printgrouplayer(Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			Vector3 position = ((Component)basePlayer).transform.position;
			int? num = Net.sv?.visibility?.PositionToLayer(position.x, position.y, position.z, basePlayer.networkRange);
			string text;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				text = ((valueOrDefault >= 10) ? $"Dynamic Dungeons ({num.Value})" : (valueOrDefault switch
				{
					0 => "Overworld (Small)", 
					1 => "Overworld (Medium)", 
					2 => "Overworld (Large)", 
					3 => "Caves", 
					4 => "Tunnels", 
					5 => "Deep Sea", 
					_ => $"Unknown ({num.Value})", 
				}));
			}
			else
			{
				text = "<null>";
			}
			string text2 = text;
			string strValue = (TerrainMeta.IsPointWithinTutorialBounds(position) ? (text2 + " (but you're in the tutorial bounds)") : text2);
			arg.ReplyWith(strValue);
		}
	}
}
