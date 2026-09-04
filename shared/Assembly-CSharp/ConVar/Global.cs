using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Nexus.Models;
using Network;
using Network.Visibility;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace ConVar;

[Factory("global")]
public class Global : ConsoleSystem
{
	private static int _developer;

	[ClientVar(Help = "(Generated) Maximum number of Unity job system worker threads; controls the background thread pool size for job dispatching")]
	[ServerVar(Help = "(Generated) Maximum number of Unity job system worker threads; controls the background thread pool size for job dispatching")]
	public static int maxthreads = 8;

	[ClientVar(Help = "(Generated) When enabled, asset bundles are unloaded from memory after their assets are extracted, saving memory; disable to keep bundles resident")]
	[ServerVar(Help = "(Generated) When enabled, asset bundles are unloaded from memory after their assets are extracted, saving memory; disable to keep bundles resident")]
	public static bool forceUnloadBundles = true;

	[ServerVar(Help = "(Generated) When true, the server network position is updated to match the debug camera world position while spectating; useful for testing position-dependent server logic from the spectator view")]
	public static bool updateNetworkPositionWithDebugCameraWhileSpectating = false;

	public static readonly string TopOfBaseFlag = "--topofbase";

	public static readonly string UndergroundFlag = "--underground";

	[ClientVar(Saved = true, Help = "(Generated) Controls the on-screen performance overlay detail level; 0 = off, higher values add more metrics such as FPS, ping, entity count, and memory usage")]
	[ServerVar(Saved = true, Help = "(Generated) Controls the on-screen performance overlay detail level; 0 = off, higher values add more metrics such as FPS, ping, entity count, and memory usage")]
	public static int perf = 0;

	[ClientVar(Saved = true, ClientAdmin = true, Help = "Media: This can be used to disable the performance text info when GC gets triggered")]
	public static bool perf_disable_gc_notif = false;

	private static bool _god = false;

	private static bool _forceOffAdminStatusOverlay = false;

	[ClientVar]
	[ServerVar(ClientAdmin = true, ServerAdmin = true, Help = "When enabled a player wearing a gingerbread suit will gib like the gingerbread NPC's")]
	public static bool cinematicGingerbreadCorpses = false;

	private static uint _gingerbreadMaterialID = 0u;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Multiplier applied to SprayDuration if a spray isn't in the sprayers auth (cannot go above 1f)")]
	public static float SprayOutOfAuthMultiplier = 0.5f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Base time (in seconds) that sprays last")]
	public static float SprayDuration = 10800f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "If a player sprays more than this, the oldest spray will be destroyed. 0 will disable")]
	public static int MaxSpraysPerPlayer = 40;

	[ServerVar(Help = "Disables the backpacks that appear after a corpse times out")]
	public static bool disableBagDropping = false;

	[ClientVar(Saved = true, Help = "Disables any emoji animations")]
	public static bool blockEmojiAnimations = false;

	[ClientVar(Saved = true, Help = "Blocks any emoji from appearing")]
	public static bool blockEmoji = false;

	[ClientVar(Saved = true, Help = "Blocks emoji provided by servers from appearing")]
	public static bool blockServerEmoji = false;

	[ClientVar(Saved = true, Help = "Displays any emoji rendering errors in the console")]
	public static bool showEmojiErrors = false;

	[ServerVar(Help = "(Generated) Developer mode level: 0 = off, 1 = developer overlays and convar unlocks, higher values enable increasingly verbose debug logging")]
	[ClientVar(Help = "(Generated) Developer mode level: 0 = off, 1 = developer overlays and convar unlocks, higher values enable increasingly verbose debug logging")]
	public static int developer
	{
		get
		{
			return _developer;
		}
		set
		{
			_developer = value;
			Array.Fill(RustLog.Levels, _developer);
		}
	}

	[ClientVar(Help = "(Generated) Number of Unity job worker threads; 0 or -1 sets the default (auto); higher values improve parallel job throughput on many-core CPUs")]
	[ServerVar(Help = "(Generated) Number of Unity job worker threads; 0 or -1 sets the default (auto); higher values improve parallel job throughput on many-core CPUs")]
	public static int job_system_threads
	{
		get
		{
			return JobsUtility.JobWorkerCount;
		}
		set
		{
			if (value < 1)
			{
				JobsUtility.ResetJobWorkerCount();
				return;
			}
			value = Mathf.Clamp(value, 1, JobsUtility.JobWorkerMaximumCount);
			JobsUtility.JobWorkerCount = value;
		}
	}

	[ClientVar(ClientInfo = true, Saved = true, Help = "If you're an admin this will enable god mode")]
	public static bool god
	{
		get
		{
			return _god;
		}
		set
		{
			_god = value;
		}
	}

	[ClientVar(ClientInfo = true, Saved = true, Help = "Media: Forcefully disables all status overlays (god, creative, invis)")]
	public static bool forceOffAdminStatusOverlay
	{
		get
		{
			return _forceOffAdminStatusOverlay;
		}
		set
		{
			_forceOffAdminStatusOverlay = value;
		}
	}

	[ServerVar(Help = "(Generated) Schedules a server restart; optionally accepts a countdown in seconds and a broadcast message sent to all players before the restart occurs")]
	public static void restart(Arg args)
	{
		ServerMgr.RestartServer(args.GetString(1, string.Empty), args.GetInt(0, 300));
	}

	[ClientVar(Help = "(Generated) Quits the application cleanly with no arguments; rejects calls with arguments to prevent accidental exit; in the editor exits play mode")]
	[ServerVar(Help = "(Generated) Quits the application cleanly with no arguments; rejects calls with arguments to prevent accidental exit; in the editor exits play mode")]
	public static void quit(Arg args)
	{
		if (args != null && args.HasArgs())
		{
			args.ReplyWith("Invalid quit command, quit only works if provided with no arguments.");
			return;
		}
		if (Application.isEditor)
		{
			Debug.LogWarning((object)"Aborting quit because we're in the editor");
			return;
		}
		if ((Object)(object)SingletonComponent<ServerMgr>.Instance != (Object)null)
		{
			SingletonComponent<ServerMgr>.Instance.Shutdown();
		}
		Application.isQuitting = true;
		Net.sv?.Stop("quit");
		Process.GetCurrentProcess().Kill();
		Debug.Log((object)"Quitting");
		Application.Quit();
	}

	[ServerVar(Help = "(Generated) Runs a server performance diagnostic report covering entity counts, memory usage, and active invokes, outputting results to the server console")]
	public static void report(Arg args)
	{
		ServerPerformance.DoReport();
	}

	[ClientVar(Help = "(Generated) Prints all live Unity Object instances sorted by total memory usage, showing type, instance count, and estimated total size in bytes")]
	[ServerVar(Help = "(Generated) Prints all live Unity Object instances sorted by total memory usage, showing type, instance count, and estimated total size in bytes")]
	public static void objects(Arg args)
	{
		Object[] array = Object.FindObjectsByType<Object>((FindObjectsSortMode)0);
		string text = "";
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		Dictionary<Type, long> dictionary2 = new Dictionary<Type, long>();
		Object[] array2 = array;
		foreach (Object val in array2)
		{
			int runtimeMemorySize = Profiler.GetRuntimeMemorySize(val);
			if (dictionary.ContainsKey(((object)val).GetType()))
			{
				dictionary[((object)val).GetType()]++;
			}
			else
			{
				dictionary.Add(((object)val).GetType(), 1);
			}
			if (dictionary2.ContainsKey(((object)val).GetType()))
			{
				dictionary2[((object)val).GetType()] += runtimeMemorySize;
			}
			else
			{
				dictionary2.Add(((object)val).GetType(), runtimeMemorySize);
			}
		}
		foreach (KeyValuePair<Type, long> item in dictionary2.OrderByDescending(delegate(KeyValuePair<Type, long> x)
		{
			KeyValuePair<Type, long> keyValuePair = x;
			return keyValuePair.Value;
		}))
		{
			text = text + dictionary[item.Key].ToString().PadLeft(10) + " " + NumberExtensions.FormatBytes<long>(item.Value, false).PadLeft(15) + "\t" + item.Key?.ToString() + "\n";
		}
		args.ReplyWith(text);
	}

	[ClientVar(Help = "(Generated) Prints a list of all live Texture objects with their name and estimated runtime memory size")]
	[ServerVar(Help = "(Generated) Prints a list of all live Texture objects with their name and estimated runtime memory size")]
	public static void textures(Arg args)
	{
		Texture[] array = Object.FindObjectsByType<Texture>((FindObjectsSortMode)0);
		string text = "";
		Texture[] array2 = array;
		foreach (Texture val in array2)
		{
			string text2 = NumberExtensions.FormatBytes<int>(Profiler.GetRuntimeMemorySize((Object)(object)val), false);
			text = text + ((object)val).ToString().PadRight(30) + ((Object)val).name.PadRight(30) + text2 + "\n";
		}
		args.ReplyWith(text);
	}

	[ClientVar(Help = "(Generated) Prints the count of enabled versus disabled Collider components currently in the scene")]
	[ServerVar(Help = "(Generated) Prints the count of enabled versus disabled Collider components currently in the scene")]
	public static void colliders(Arg args)
	{
		int num = (from x in Object.FindObjectsByType<Collider>((FindObjectsSortMode)0)
			where x.enabled
			select x).Count();
		int num2 = (from x in Object.FindObjectsByType<Collider>((FindObjectsSortMode)0)
			where !x.enabled
			select x).Count();
		string strValue = num + " colliders enabled, " + num2 + " disabled";
		args.ReplyWith(strValue);
	}

	[ServerVar(Help = "(Generated) Prints the current state of server-side stability check and surroundings update queues; reports nothing useful on client")]
	[ClientVar(Help = "(Generated) Prints the current state of server-side stability check and surroundings update queues; reports nothing useful on client")]
	public static void queue(Arg args)
	{
		string text = "";
		text = text + "stabilityCheckQueue:        " + ((ObjectWorkQueue<StabilityEntity>)StabilityEntity.stabilityCheckQueue).Info() + "\n";
		text = text + "updateSurroundingsQueue:    " + ((ObjectWorkQueue<Bounds>)StabilityEntity.updateSurroundingsQueue).Info() + "\n";
		args.ReplyWith(text);
	}

	[ServerUserVar]
	public static void setinfo(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			string text = args.GetString(0, null);
			string text2 = args.GetString(1, null);
			if (text != null && text2 != null)
			{
				basePlayer.SetInfo(text, text2);
			}
		}
	}

	[ServerVar(Help = "(Generated) Puts the calling player into the sleeping state, disconnecting their control and making them a sleeping entity on the server")]
	public static void sleep(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && !basePlayer.IsSleeping() && !basePlayer.IsSpectating() && !basePlayer.IsDead())
		{
			basePlayer.StartSleeping();
		}
	}

	[ServerVar(Help = "(Generated) Puts the player that the calling admin is looking at into the sleeping state; useful for testing sleeping player interactions")]
	public static void sleeptarget(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			BasePlayer lookingAtPlayer = RelationshipManager.GetLookingAtPlayer(basePlayer);
			if (!((Object)(object)lookingAtPlayer == (Object)null))
			{
				lookingAtPlayer.StartSleeping();
			}
		}
	}

	[ServerUserVar]
	public static void kill(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer) || basePlayer.IsSpectating() || basePlayer.IsDead())
		{
			return;
		}
		if (basePlayer.IsRestrained)
		{
			Handcuffs handcuffs = basePlayer.Belt?.GetRestraintItem();
			if ((Object)(object)handcuffs != (Object)null && handcuffs.BlockSuicide)
			{
				return;
			}
		}
		if (basePlayer.CanSuicide())
		{
			basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
			if (basePlayer.IsDead())
			{
				basePlayer.MarkSuicide();
			}
		}
		else
		{
			basePlayer.ConsoleMessage("You can't suicide again so quickly, wait a while");
		}
	}

	[ServerUserVar]
	public static void respawn(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			return;
		}
		if (!basePlayer.IsDead() && !basePlayer.IsSpectating())
		{
			if (developer > 0)
			{
				Debug.LogWarning((object)(((object)basePlayer)?.ToString() + " wanted to respawn but isn't dead or spectating"));
			}
			basePlayer.SendNetworkUpdate();
		}
		else if (basePlayer.CanRespawn())
		{
			basePlayer.MarkRespawn();
			basePlayer.Respawn();
		}
		else
		{
			basePlayer.ConsoleMessage("You can't respawn again so quickly, wait a while");
		}
	}

	[ServerVar(Help = "(Generated) Puts the calling player or a named target into the wounded/downed state, simulating the critical injury bleed-out state")]
	public static void injure(Arg args)
	{
		InjurePlayer(ArgEx.Player(args));
	}

	public static void InjurePlayer(BasePlayer ply)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ply == (Object)null || ply.IsDead())
		{
			return;
		}
		HitInfo hitInfo = Pool.Get<HitInfo>();
		hitInfo.Init(ply, ply, DamageType.Suicide, 1000f, ((Component)ply).transform.position);
		hitInfo.UseProtection = false;
		if (Server.woundingenabled && !ply.IsIncapacitated() && !ply.IsSleeping() && !ply.isMounted)
		{
			if (ply.IsCrawling())
			{
				ply.GoToIncapacitated(hitInfo);
			}
			else
			{
				ply.BecomeWounded(hitInfo);
			}
		}
		else
		{
			ply.ConsoleMessage("Can't go to wounded state right now.");
		}
	}

	[ServerVar(Help = "(Generated) Revives the calling player or a named target from the wounded state, restoring them to standing with a small amount of health")]
	public static void recover(Arg args)
	{
		RecoverPlayer(ArgEx.Player(args));
	}

	public static void RecoverPlayer(BasePlayer ply)
	{
		if (!((Object)(object)ply == (Object)null) && !ply.IsDead())
		{
			ply.StopWounded();
		}
	}

	[ServerVar(Help = "(Generated) Enters spectator mode; optionally accepts a player name or Steam ID to spectate that specific player from a third-person camera")]
	public static void spectate(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer.wantsSpectate = true;
			if (!basePlayer.IsDead())
			{
				basePlayer.DieInstantly();
			}
			string strName = args.GetString(0);
			if (basePlayer.IsDead())
			{
				basePlayer.StartSpectating();
				basePlayer.UpdateSpectateTarget(strName);
			}
			basePlayer.wantsSpectate = false;
		}
	}

	[ServerVar(Help = "(Generated) Toggles display of the team info overlay (health, location, vitals) for the spectated player while in spectator mode")]
	public static void toggleSpectateTeamInfo(Arg args)
	{
		bool flag = args.GetBool(0);
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer != (Object)null)
		{
			basePlayer.SetSpectateTeamInfo(flag);
			args.ReplyWith($"ToggleSpectateTeamInfo is now {flag}");
		}
		else
		{
			args.ReplyWith("Invalid player or player is not spectating");
		}
	}

	[ServerVar(Help = "(Generated) Enters spectator mode targeting the entity or player with the given network entity ID")]
	public static void spectateid(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer.wantsSpectate = true;
			if (!basePlayer.IsDead())
			{
				basePlayer.DieInstantly();
			}
			ulong uLong = args.GetULong(0, 0uL);
			if (basePlayer.IsDead())
			{
				basePlayer.StartSpectating();
				basePlayer.UpdateSpectateTarget(uLong);
			}
			basePlayer.wantsSpectate = false;
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag(Arg args)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer) || !basePlayer.IsDead())
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!((NetworkableId)(ref entityID)).IsValid)
		{
			args.ReplyWith("Missing sleeping bag ID");
			return;
		}
		string text = args.GetString(1);
		string errorMessage;
		if (NexusServer.Started && !string.IsNullOrWhiteSpace(text))
		{
			if (!ZoneController.Instance.CanRespawnAcrossZones(basePlayer))
			{
				args.ReplyWith("You cannot respawn to a different zone");
				return;
			}
			NexusZoneDetails val = NexusServer.FindZone(text);
			if (val == null)
			{
				args.ReplyWith("Zone was not found");
			}
			else if (!basePlayer.CanRespawn())
			{
				args.ReplyWith("You can't respawn again so quickly, wait a while");
			}
			else
			{
				NexusRespawn(basePlayer, val, entityID);
			}
		}
		else if (!SleepingBag.TrySpawnPlayer(basePlayer, entityID, out errorMessage))
		{
			args.ReplyWith(errorMessage);
		}
		static async void NexusRespawn(BasePlayer player, NexusZoneDetails toZone, NetworkableId sleepingBag)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			_ = 1;
			try
			{
				player.nextRespawnTime = float.PositiveInfinity;
				Request val2 = Pool.Get<Request>();
				val2.respawnAtBag = Pool.Get<SleepingBagRespawnRequest>();
				val2.respawnAtBag.userId = player.userID;
				val2.respawnAtBag.sleepingBagId = sleepingBag;
				val2.respawnAtBag.secondaryData = player.SaveSecondaryData();
				Response val3 = await NexusServer.ZoneRpc(toZone.Key, val2);
				try
				{
					if (!val3.status.success)
					{
						if (player.IsConnected)
						{
							player.ConsoleMessage("RespawnAtBag failed: " + val3.status.errorMessage);
						}
						return;
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				await NexusServer.ZoneClient.Assign((ulong)player.userID, toZone.Key);
				if (player.IsConnected)
				{
					ConsoleNetwork.SendClientCommandImmediate(player.net.connection, "nexus.redirect", toZone.IpAddress, toZone.GamePort, NexusUtil.ConnectionProtocol(toZone));
					player.Kick("Redirecting to another zone...");
				}
			}
			catch (Exception ex)
			{
				if (player.IsConnected)
				{
					player.ConsoleMessage(ex.ToString());
				}
			}
			finally
			{
				player.MarkRespawn();
			}
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag_remove(Arg args)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!((NetworkableId)(ref entityID)).IsValid)
		{
			args.ReplyWith("Missing sleeping bag ID");
			return;
		}
		string text = args.GetString(1);
		if (NexusServer.Started && !string.IsNullOrWhiteSpace(text))
		{
			NexusZoneDetails val = NexusServer.FindZone(text);
			if (val == null)
			{
				args.ReplyWith("Zone was not found");
			}
			else if (ZoneController.Instance.CanRespawnAcrossZones(basePlayer))
			{
				NexusRemoveBag(basePlayer, val.Key, entityID);
			}
		}
		else
		{
			SleepingBag.DestroyBag(basePlayer.userID, entityID);
		}
		static async void NexusRemoveBag(BasePlayer player, string zoneKey, NetworkableId sleepingBag)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Request val2 = Pool.Get<Request>();
				val2.destroyBag = Pool.Get<SleepingBagDestroyRequest>();
				val2.destroyBag.userId = player.userID;
				val2.destroyBag.sleepingBagId = sleepingBag;
				(await NexusServer.ZoneRpc(zoneKey, val2)).Dispose();
			}
			catch (Exception ex)
			{
				if (player.IsConnected)
				{
					player.ConsoleMessage(ex.ToString());
				}
			}
		}
	}

	[ServerUserVar]
	public static void respawn_sleepingbag_favourite(Arg args)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			NetworkableId entityID = ArgEx.GetEntityID(args, 0);
			if (!((NetworkableId)(ref entityID)).IsValid)
			{
				args.ReplyWith("Missing sleeping bag ID");
				return;
			}
			if (!basePlayer.IsDead())
			{
				args.ReplyWith("Can only modify while dead");
				return;
			}
			bool favourite = args.GetInt(1) != 0;
			SleepingBag.SetBagFavourite(basePlayer.userID, entityID, favourite);
		}
	}

	[ServerUserVar]
	public static void status_sv(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			args.ReplyWith(basePlayer.GetDebugStatus());
		}
	}

	[ClientVar(Help = "(Generated) Prints client-side connection status information including connected state, ping, player entity ID, and current map to the client console")]
	public static void status_cl(Arg args)
	{
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to a player by name or partial name match; if two arguments are given, moves the first-named player to the second")]
	public static void teleport(Arg args)
	{
		if (args.HasArgs(2))
		{
			BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if (Object.op_Implicit((Object)(object)playerOrSleeperOrBot) && playerOrSleeperOrBot.IsAlive())
			{
				BasePlayer playerOrSleeperOrBot2 = ArgEx.GetPlayerOrSleeperOrBot(args, 1);
				if (Object.op_Implicit((Object)(object)playerOrSleeperOrBot2) && playerOrSleeperOrBot2.IsAlive())
				{
					playerOrSleeperOrBot.Teleport(playerOrSleeperOrBot2);
				}
			}
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			BasePlayer playerOrSleeperOrBot3 = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
			if (Object.op_Implicit((Object)(object)playerOrSleeperOrBot3) && playerOrSleeperOrBot3.IsAlive())
			{
				basePlayer.Teleport(playerOrSleeperOrBot3);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the named player to the calling admin current position")]
	public static void teleport2me(Arg args)
	{
		BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(args, 0);
		if ((Object)(object)playerOrSleeperOrBot == (Object)null)
		{
			args.ReplyWith("Player or bot not found");
			return;
		}
		if (!playerOrSleeperOrBot.IsAlive())
		{
			args.ReplyWith("Target is not alive");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			playerOrSleeperOrBot.Teleport(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all connected players to the calling admin current position")]
	public static void teleporteveryone2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all sleeping player entities to the calling admin current position")]
	public static void teleportsleepers2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: false, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all connected active (non-sleeping) players to the calling admin current position")]
	public static void teleportnonsleepers2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			TeleportPlayersToMe(basePlayer, includeSleepers: false, includeNonSleepers: true, 0uL);
		}
	}

	[ServerVar(Help = "(Generated) Teleports all members of the calling player team to the calling admin current position")]
	public static void teleportteam2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			if (basePlayer.Team == null)
			{
				args.ReplyWith("Player is not in a team");
			}
			else
			{
				TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, basePlayer.Team.teamID);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports all members of the named player team to the calling admin current position")]
	public static void teleporttargetteam2me(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			if (basePlayer.Team == null)
			{
				args.ReplyWith("Player is not in a team");
				return;
			}
			ulong uLong = args.GetULong(0, 0uL);
			TeleportPlayersToMe(basePlayer, includeSleepers: true, includeNonSleepers: true, uLong);
		}
	}

	private static void TeleportPlayersToMe(BasePlayer player, bool includeSleepers, bool includeNonSleepers, ulong filterByTeam = 0uL)
	{
		if ((Object)(object)player == (Object)null || !Object.op_Implicit((Object)(object)player) || !player.IsAlive())
		{
			return;
		}
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			if (allPlayer.IsAlive() && !((Object)(object)allPlayer == (Object)(object)player) && (!allPlayer.IsSleeping() || includeSleepers) && (allPlayer.IsSleeping() || includeNonSleepers) && (filterByTeam == 0L || (allPlayer.Team != null && allPlayer.Team.teamID == filterByTeam)))
			{
				allPlayer.Teleport(player);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the nearest entity matching the given prefab short name, with an optional radius filter")]
	public static void teleportany(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			basePlayer.Teleport(args.GetString(0), playersOnly: false);
		}
	}

	[ServerVar]
	[Help("Teleport to the current closest entity matching the first argument name. Add second int argument to teleport to the nth closest entity (teleport2nearest horse 2 will teleport to the 3rd closest horse)")]
	public static void teleport2nearest(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			string entityName = args.GetString(0);
			int index = args.GetInt(1);
			basePlayer.TeleportToNearestTargetEntity(entityName, index);
		}
	}

	[ServerVar(Help = "Teleport to the entity with the specified network ID")]
	public static void teleport2entityid(Arg args)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			NetworkableId entityID = ArgEx.GetEntityID(args, 0);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if ((Object)(object)baseNetworkable == (Object)null)
			{
				args.ReplyWith($"No entity found with id {entityID}");
				return;
			}
			args.ReplyWith($"Teleporting to {baseNetworkable.ShortPrefabName} at {((Component)baseNetworkable).transform.position}");
			basePlayer.Teleport(((Component)baseNetworkable).transform.position);
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin or a named player to exact world coordinates specified as X Y Z arguments")]
	public static void teleportpos(Arg args)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			bool num = args.HasArg(TopOfBaseFlag);
			bool flag = args.HasArg(UndergroundFlag);
			StringView val = ((StringView)(ref args.FullString)).Replace(StringView.op_Implicit(", "), StringView.op_Implicit(","));
			val = ((StringView)(ref val)).Replace(StringView.op_Implicit(TopOfBaseFlag), StringView.op_Implicit(""));
			val = ((StringView)(ref val)).Replace(StringView.op_Implicit(UndergroundFlag), StringView.op_Implicit(""));
			StringView str = ((StringView)(ref val)).Trim('"');
			if (num)
			{
				TeleportToTopOfBase(basePlayer, str.ToVector3());
			}
			else if (flag)
			{
				TeleportToUnderground(basePlayer, str.ToVector3());
			}
			else
			{
				basePlayer.Teleport(str.ToVector3());
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the point in the world that their line of sight is currently hitting")]
	public static void teleportlos(Arg args)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer) && basePlayer.IsAlive())
		{
			Ray val = basePlayer.eyes.HeadRay();
			int num = args.GetInt(0, 1000);
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(val, ref val2, (float)num, 1218652417))
			{
				basePlayer.Teleport(((RaycastHit)(ref val2)).point);
			}
			else
			{
				basePlayer.Teleport(((Ray)(ref val)).origin + ((Ray)(ref val)).direction * (float)num);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to an entity owned by a specified player, identified by Steam ID or name")]
	public static void teleport2owneditem(Arg arg)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if ((Object)(object)playerOrSleeper != (Object)null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string strFilter = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsOwnedBy(result, strFilter);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {((Component)array[num]).transform.position}");
		basePlayer.Teleport(((Component)array[num]).transform.position);
	}

	[ServerVar(Help = "<steamID/name> <optional: filter> - Teleport to a random entity the player is authed on")]
	public static void teleport2autheditem(Arg arg)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong result;
		if ((Object)(object)playerOrSleeper != (Object)null)
		{
			result = playerOrSleeper.userID;
		}
		else if (!ulong.TryParse(arg.GetString(0), out result))
		{
			arg.ReplyWith("No player with that id found");
			return;
		}
		string strFilter = arg.GetString(1);
		BaseEntity[] array = BaseEntity.Util.FindTargetsAuthedTo(result, strFilter);
		if (array.Length == 0)
		{
			arg.ReplyWith("No targets found");
			return;
		}
		int num = Random.Range(0, array.Length);
		arg.ReplyWith($"Teleporting to {array[num].ShortPrefabName} at {((Component)array[num]).transform.position}");
		basePlayer.Teleport(((Component)array[num]).transform.position);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the map marker they have placed on their in-game map")]
	public static void teleport2marker(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		if (basePlayer.State.pointsOfInterest == null || basePlayer.State.pointsOfInterest.Count == 0)
		{
			arg.ReplyWith("You don't have a marker set");
			return;
		}
		string text = arg.GetString(0);
		if (arg.HasArgs() && text != "True")
		{
			int num = arg.GetInt(0);
			if (num == -1)
			{
				num = basePlayer.State.pointsOfInterest.Count - 1;
			}
			if (num >= 0 && num < basePlayer.State.pointsOfInterest.Count)
			{
				TeleportToMarker(basePlayer.State.pointsOfInterest[num], basePlayer);
				return;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			foreach (MapNote item in basePlayer.State.pointsOfInterest)
			{
				if (!string.IsNullOrEmpty(item.label) && string.Equals(item.label, text, StringComparison.InvariantCultureIgnoreCase))
				{
					TeleportToMarker(item, basePlayer);
					return;
				}
			}
		}
		int debugMapMarkerIndex = basePlayer.DebugMapMarkerIndex;
		debugMapMarkerIndex++;
		if (debugMapMarkerIndex >= basePlayer.State.pointsOfInterest.Count)
		{
			debugMapMarkerIndex = 0;
		}
		TeleportToMarker(basePlayer.State.pointsOfInterest[debugMapMarkerIndex], basePlayer);
		basePlayer.DebugMapMarkerIndex = debugMapMarkerIndex;
	}

	private static void TeleportToMarker(MapNote marker, BasePlayer player)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		TeleportToTopOfBase(player, marker.worldPosition);
	}

	private static void TeleportToTopOfBase(BasePlayer player, Vector3 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true);
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(new Ray(position + Vector3.up * 100f, Vector3.down), ref val, 110f, 1218652417))
		{
			position.y = ((RaycastHit)(ref val)).point.y + 0.5f;
		}
		player.Teleport(position);
	}

	private static void TeleportToUnderground(BasePlayer player, Vector3 position)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true) - 10f;
		BufferList<RaycastHit> val = Pool.Get<BufferList<RaycastHit>>();
		val.Resize(10);
		int num = Physics.RaycastNonAlloc(new Ray(position, Vector3.down), val.Buffer, 200f, 1210263809);
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			RaycastHit val2 = val[i];
			float y = ((RaycastHit)(ref val2)).transform.position.y;
			if (y < num2)
			{
				position.y = y + 2f;
				if (!global::AntiHack.TestInsideTerrain(position))
				{
					flag = true;
					num2 = y;
				}
			}
		}
		if (flag)
		{
			position.y = num2 + 2f;
			player.Teleport(position);
		}
		else
		{
			TeleportToTopOfBase(player, position);
		}
		Pool.FreeUnmanaged<RaycastHit>(ref val);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the centre of the named map grid square (e.g. A1, B3)")]
	public static void teleport2grid(Arg arg)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			Vector3? val = MapHelper.StringToPosition(arg.GetString(0));
			if (!val.HasValue)
			{
				arg.ReplyWith("Invalid grid reference, should look like 'A1'");
			}
			else
			{
				TeleportToTopOfBase(basePlayer, val.Value);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to their own most recent death location")]
	public static void teleport2death(Arg arg)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		if (basePlayer.State.deathMarker == null)
		{
			arg.ReplyWith("No death marker found");
			return;
		}
		Vector3 worldPosition = basePlayer.ServerCurrentDeathNote.worldPosition;
		basePlayer.Teleport(worldPosition);
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the currently locked-in satellite crash site. Does nothing if no satellite is descending.")]
	public static void teleport2satellitecrashsite(Arg arg)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			SatelliteControlComputer activeDescending = SatelliteControlComputer.ActiveDescending;
			if ((Object)(object)activeDescending == (Object)null || activeDescending.IsDestroyed)
			{
				arg.ReplyWith("No locked-in satellite crash site");
			}
			else
			{
				TeleportToTopOfBase(basePlayer, activeDescending.LockedCrashPosition);
			}
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin to the target location of their currently active mission objective")]
	public static void teleport2mission(Arg arg)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
			if (objectiveStatus.started && !objectiveStatus.completed && !objectiveStatus.failed && !(objectiveStatus.worldLocation == default(Vector3)))
			{
				TeleportToTopOfBase(basePlayer, objectiveStatus.worldLocation);
				break;
			}
		}
	}

	private static PlayerBoat GetPlayerBoat(BasePlayer player)
	{
		return PlayerBoat.GetParentPlayerBoat(player);
	}

	private static bool TeleportBoatToWater(PlayerBoat boat, Vector3 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		position.y = WaterLevel.GetWaterOrTerrainSurface(position, waves: true, volumes: true);
		if (!WaterLevel.Test(position, waves: true, volumes: true))
		{
			return false;
		}
		boat.Teleport(position);
		return true;
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to the map marker they have placed on their in-game map")]
	public static void teleportboat2marker(Arg arg)
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if ((Object)(object)playerBoat == (Object)null)
		{
			arg.ReplyWith("You are not on a player boat");
			return;
		}
		if (basePlayer.State.pointsOfInterest == null || basePlayer.State.pointsOfInterest.Count == 0)
		{
			arg.ReplyWith("You don't have a marker set");
			return;
		}
		string text = arg.GetString(0);
		if (arg.HasArgs() && text != "True")
		{
			int num = arg.GetInt(0);
			if (num == -1)
			{
				num = basePlayer.State.pointsOfInterest.Count - 1;
			}
			if (num >= 0 && num < basePlayer.State.pointsOfInterest.Count)
			{
				if (!TeleportBoatToWater(playerBoat, basePlayer.State.pointsOfInterest[num].worldPosition))
				{
					arg.ReplyWith("Target position is not in water");
				}
				return;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			foreach (MapNote item in basePlayer.State.pointsOfInterest)
			{
				if (!string.IsNullOrEmpty(item.label) && string.Equals(item.label, text, StringComparison.InvariantCultureIgnoreCase))
				{
					if (!TeleportBoatToWater(playerBoat, item.worldPosition))
					{
						arg.ReplyWith("Target position is not in water");
					}
					return;
				}
			}
		}
		int debugMapMarkerIndex = basePlayer.DebugMapMarkerIndex;
		debugMapMarkerIndex++;
		if (debugMapMarkerIndex >= basePlayer.State.pointsOfInterest.Count)
		{
			debugMapMarkerIndex = 0;
		}
		if (!TeleportBoatToWater(playerBoat, basePlayer.State.pointsOfInterest[debugMapMarkerIndex].worldPosition))
		{
			arg.ReplyWith("Target position is not in water");
		}
		basePlayer.DebugMapMarkerIndex = debugMapMarkerIndex;
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to exact world coordinates specified as X Y Z arguments")]
	public static void teleportboatpos(Arg args)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer) || !basePlayer.IsAlive())
		{
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if ((Object)(object)playerBoat == (Object)null)
		{
			args.ReplyWith("You are not on a player boat");
			return;
		}
		StringView val = ((StringView)(ref args.FullString)).Replace(StringView.op_Implicit(", "), StringView.op_Implicit(","));
		string str = ((object)((StringView)(ref val)).Trim('"')/*cast due to constrained. prefix*/).ToString();
		if (!TeleportBoatToWater(playerBoat, str.ToVector3()))
		{
			args.ReplyWith("Target position is not in water");
		}
	}

	[ServerVar(Help = "(Generated) Teleports the calling admin and their player boat to the centre of the named map grid square (e.g. A1, B3)")]
	public static void teleportboat2grid(Arg arg)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		PlayerBoat playerBoat = GetPlayerBoat(basePlayer);
		if ((Object)(object)playerBoat == (Object)null)
		{
			arg.ReplyWith("You are not on a player boat");
			return;
		}
		Vector3? val = MapHelper.StringToPosition(arg.GetString(0));
		if (!val.HasValue)
		{
			arg.ReplyWith("Invalid grid reference, should look like 'A1'");
		}
		else if (!TeleportBoatToWater(playerBoat, val.Value))
		{
			arg.ReplyWith("Target position is not in water");
		}
	}

	[ClientVar(Help = "(Generated) Clears prefab pools and releases pooled objects; delegates to pool.clear_prefabs; admin/developer only")]
	[ServerVar(Help = "(Generated) Clears prefab pools and releases pooled objects; delegates to pool.clear_prefabs; admin/developer only")]
	public static void free(Arg args)
	{
		Pool.clear_prefabs(args);
		Pool.clear_assets(args);
		Pool.clear_memory(args);
		GC.collect();
		GC.unload();
	}

	[ClientVar(Help = "(Generated) Prints the current game version string to the console, including build number and branch")]
	[ServerVar(ServerUser = true, Help = "(Generated) Prints the current game version string to the console, including build number and branch")]
	public static void version(Arg arg)
	{
		arg.ReplyWith(string.Format("Protocol: {0}\nBuild Date: {1}\nUnity Version: {2}\nChangeset: {3}\nBranch: {4}", new object[5]
		{
			Protocol.printable,
			BuildInfo.Current.BuildDate,
			Application.unityVersion,
			BuildInfo.Current.Scm.ChangeId,
			BuildInfo.Current.Scm.Branch
		}));
	}

	[ServerVar(Help = "(Generated) Prints a summary of the current machine hardware and OS info including CPU, GPU, RAM, and platform")]
	[ClientVar(Help = "(Generated) Prints a summary of the current machine hardware and OS info including CPU, GPU, RAM, and platform")]
	public static void sysinfo(Arg arg)
	{
		arg.ReplyWith(SystemInfoGeneralText.currentInfo);
	}

	[ClientVar(Help = "(Generated) Prints the unique device identifier for the current machine as reported by Unity SystemInfo.deviceUniqueIdentifier")]
	[ServerVar(Help = "(Generated) Prints the unique device identifier for the current machine as reported by Unity SystemInfo.deviceUniqueIdentifier")]
	public static void sysuid(Arg arg)
	{
		arg.ReplyWith(SystemInfo.deviceUniqueIdentifier);
	}

	[ServerVar(Help = "(Generated) Reduces the condition of all items in the calling player inventory whose short name matches the given string to zero, breaking them")]
	public static void breakitem(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			Item activeItem = basePlayer.GetActiveItem();
			activeItem?.LoseCondition(activeItem.condition);
		}
	}

	[ServerVar(Help = "(Generated) Breaks all equipped clothing items currently worn by the calling player, reducing their condition to zero")]
	public static void breakclothing(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			return;
		}
		foreach (Item item in basePlayer.inventory.containerWear.itemList)
		{
			item?.LoseCondition(item.condition);
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of active network group subscriptions for the calling player, showing realm and group ID; supports --json flag")]
	[ClientVar(Help = "(Generated) Prints a table of active network group subscriptions for the calling player, showing realm and group ID; supports --json flag")]
	public static void subscriptions(Arg arg)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("realm");
			val.AddColumn("group");
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (Object.op_Implicit((Object)(object)basePlayer))
			{
				Enumerator<Group> enumerator = basePlayer.net.subscriber.subscribed.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Group current = enumerator.Current;
						val.AddRow(new string[2]
						{
							"sv",
							current.ID.ToString()
						});
					}
				}
				finally
				{
					((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
				}
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static uint GingerbreadMaterialID()
	{
		if (_gingerbreadMaterialID == 0)
		{
			_gingerbreadMaterialID = StringPool.Get("Gingerbread");
		}
		return _gingerbreadMaterialID;
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities from the server world; useful for cleaning up excessive player spray art")]
	public static void ClearAllSprays()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		List<SprayCanSpray> list = Pool.Get<List<SprayCanSpray>>();
		Enumerator<SprayCanSpray> enumerator = SprayCanSpray.AllSprays.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SprayCanSpray current = enumerator.Current;
				list.Add(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (SprayCanSpray item in list)
		{
			item.Kill();
		}
		Pool.FreeUnmanaged<SprayCanSpray>(ref list);
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities created by a specific player, identified by Steam ID or name")]
	public static void ClearAllSpraysByPlayer(Arg arg)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!arg.HasArgs())
		{
			return;
		}
		ulong uLong = arg.GetULong(0, 0uL);
		List<SprayCanSpray> list = Pool.Get<List<SprayCanSpray>>();
		Enumerator<SprayCanSpray> enumerator = SprayCanSpray.AllSprays.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SprayCanSpray current = enumerator.Current;
				if (current.sprayedByPlayer == uLong)
				{
					list.Add(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (SprayCanSpray item in list)
		{
			item.Kill();
		}
		int count = list.Count;
		Pool.FreeUnmanaged<SprayCanSpray>(ref list);
		arg.ReplyWith($"Deleted {count} sprays by {uLong}");
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities within the given radius of the calling admin current position")]
	public static void ClearSpraysInRadius(Arg arg)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			float num = arg.GetFloat(0, 16f);
			int num2 = ClearSpraysInRadius(((Component)basePlayer).transform.position, num);
			arg.ReplyWith($"Deleted {num2} sprays within {num} of {basePlayer.displayName}");
		}
	}

	private static int ClearSpraysInRadius(Vector3 position, float radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		List<SprayCanSpray> list = Pool.Get<List<SprayCanSpray>>();
		Enumerator<SprayCanSpray> enumerator = SprayCanSpray.AllSprays.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SprayCanSpray current = enumerator.Current;
				if (current.Distance(position) <= radius)
				{
					list.Add(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (SprayCanSpray item in list)
		{
			item.Kill();
		}
		int count = list.Count;
		Pool.FreeUnmanaged<SprayCanSpray>(ref list);
		return count;
	}

	[ServerVar(Help = "(Generated) Removes all spray paint entities within a given radius of the specified world position (X Y Z)")]
	public static void ClearSpraysAtPositionInRadius(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = arg.GetVector3(0);
		float num = arg.GetFloat(1);
		if (num != 0f)
		{
			int num2 = ClearSpraysInRadius(vector, num);
			arg.ReplyWith($"Deleted {num2} sprays within {num} of {vector}");
		}
	}

	[ServerVar(Help = "(Generated) Removes all dropped item entities from the server, cleaning up every piece of loot on the ground")]
	public static void ClearDroppedItems()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		List<DroppedItem> list = Pool.Get<List<DroppedItem>>();
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is DroppedItem item)
				{
					list.Add(item);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		foreach (DroppedItem item2 in list)
		{
			item2.Kill();
		}
		Pool.FreeUnmanaged<DroppedItem>(ref list);
	}

	[ClientVar(Help = "(Generated) Prints all scenes registered in the build settings with their build index and asset path")]
	[ServerVar(Help = "(Generated) Prints all scenes registered in the build settings with their build index and asset path")]
	public static string printAllScenesInBuild(Arg args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int sceneCountInBuildSettings = SceneManager.sceneCountInBuildSettings;
		stringBuilder.AppendLine($"Scenes: {sceneCountInBuildSettings}");
		for (int i = 0; i < sceneCountInBuildSettings; i++)
		{
			stringBuilder.AppendLine(SceneUtility.GetScenePathByBuildIndex(i));
		}
		return stringBuilder.ToString();
	}

	[ServerVar(Clientside = true, Help = "Immediately update the manifest")]
	public static void UpdateManifest(Arg args)
	{
		Manifest.UpdateManifest();
	}
}
