using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

namespace ConVar;

[Factory("clan")]
public class Clan : ConsoleSystem
{
	[ReplicatedVar(Help = "If enabled then players will need to be near a Clan Table to make changes to clans", Default = "true")]
	public static bool editsRequireClanTable = true;

	[ServerVar(Help = "Enables the clan system if set to true (must be set at boot, requires restart)")]
	public static bool enabled = true;

	[ServerVar(Help = "Maximum number of members each clan can have (local backend only!)")]
	public static int maxMemberCount = 100;

	public static int scoreForKillingPlayerInOtherClan = 0;

	public static int scoreForKilledByPlayerInOtherClan = 0;

	public static int scoreForKillingUnarmedPlayer = 0;

	public static int scoreForDestroyingToolCupboards = 0;

	[ServerVar(Help = "How much score players earn for hacking crates")]
	public static int scoreForHackingCrates = 5;

	[ServerVar(Help = "How much score players earn for opening hacked crates")]
	public static int scoreForOpeningHackedCrates = 10;

	[ServerVar(Help = "How much score players earn for destroying bradley")]
	public static int scoreForDestroyingBradley = 50;

	[ServerVar(Help = "How much score players earn for running the excavator, per diesel fuel consumed")]
	public static int scoreForRunningExcavator = 3;

	[ServerVar(Help = "How much score players earn for reaching cargo ship")]
	public static int scoreForReachingCargoShip = 10;

	[ServerVar(Help = "How much score players earn for looting an elite crate")]
	public static int scoreForLootingEliteCrate = 5;

	[ServerVar(Help = "How much score players earn for destroying patrol heli")]
	public static int scoreForDestroyingPatrolHeli = 50;

	[ServerVar(Help = "How much score players earn for swiping a red keycard")]
	public static int scoreForSwipingRedKeycard = 5;

	[ServerVar(Help = "How much score players earn for inserting a heavy fuse into powerplant")]
	public static int scoreForInsertHeavyFuseInPowerPlant = 5;

	[ServerVar(Help = "How much score players earn for looting a crashed satellite's crates")]
	public static int scoreForLootingSatellite = 10;

	[ServerVar(Help = "How much score players earn for running the water treatment plant, per consumed item")]
	public static int scoreForEnablingWaterTreatmentPlant = 3;

	[ServerVar(Help = "How much score players earn for launching a satellite")]
	public static int scoreForLaunchingSatellite = 25;

	[ServerVar(Help = "How much score players earn for starting the oil rig fuel switch")]
	public static int scoreForStartingOilRigFuelSwitch = 5;

	[ServerVar(Help = "Prints info about a clan given its ID")]
	public static void Info(Arg arg)
	{
		if ((Object)(object)ClanManager.ServerInstance == (Object)null)
		{
			arg.ReplyWith("ClanManager is null!");
			return;
		}
		long clanId = arg.GetLong(0, 0L);
		if (clanId == 0L)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if ((Object)(object)basePlayer == (Object)null)
			{
				arg.ReplyWith("Usage: clan.info <clanID>");
			}
			else
			{
				SendClanInfoPlayer(basePlayer);
			}
		}
		else
		{
			SendClanInfoConsole(clanId);
		}
		static string FormatClan(IClan clan)
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Clan ID: {clan.ClanId}");
			stringBuilder.AppendLine("Name: " + clan.Name);
			stringBuilder.AppendLine("MoTD: " + clan.Motd);
			stringBuilder.AppendLine("Members:");
			TextTable val = Pool.Get<TextTable>();
			try
			{
				val.AddColumns(new string[4] { "steamID", "username", "online", "role" });
				foreach (ClanMember member in clan.Members)
				{
					ClanRole? val2 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)clan.Roles, (Func<ClanRole, int>)((ClanRole r) => r.RoleId), member.RoleId, (IEqualityComparer<int>)null);
					string text = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member.SteamId) ?? "[unknown]";
					bool flag = (NexusServer.Started ? NexusServer.IsOnline(member.SteamId) : ServerPlayers.IsOnline(member.SteamId));
					string[] array = new string[4];
					ulong steamId = member.SteamId;
					array[0] = steamId.ToString();
					array[1] = text;
					array[2] = (flag ? "x" : "");
					array[3] = val2?.Name ?? "[null]";
					val.AddRow(array);
				}
				stringBuilder.Append(val);
				return stringBuilder.ToString();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		static async void SendClanInfoConsole(long id)
		{
			try
			{
				IClan val = await GetClanByID(id);
				if (val != null)
				{
					Debug.Log((object)FormatClan(val));
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		async void SendClanInfoPlayer(BasePlayer player)
		{
			_ = 1;
			try
			{
				IClan val = ((clanId != 0L) ? (await GetClanByID(clanId)) : (await GetPlayerClan(player)));
				IClan val2 = val;
				if (val2 != null)
				{
					string msg = FormatClan(val2);
					player.ConsoleMessage(msg);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	private static async ValueTask<IClan> GetPlayerClan(BasePlayer player)
	{
		ClanValueResult<IClan> val = await ClanManager.ServerInstance.Backend.GetByMember((ulong)player.userID);
		if (!val.IsSuccess)
		{
			string msg = (((int)val.Result == 3) ? "You're not in a clan!" : "Failed to find your clan!");
			player.ConsoleMessage(msg);
			return null;
		}
		return val.Value;
	}

	private static async ValueTask<IClan> GetClanByID(long clanId, BasePlayer player = null)
	{
		ClanValueResult<IClan> val = await ClanManager.ServerInstance.Backend.Get(clanId);
		if (!val.IsSuccess)
		{
			string text = (((int)val.Result == 4) ? $"Clan with ID {clanId} was not found!" : $"Failed to get the clan with ID {clanId} ({val.Result})!");
			if ((Object)(object)player != (Object)null)
			{
				player.ConsoleMessage(text);
			}
			else
			{
				Debug.Log((object)text);
			}
			return null;
		}
		return val.Value;
	}

	public static int GetScoreForEvent(ClanScoreEventType eventType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected I4, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		return (eventType - -1) switch
		{
			1 => 1, 
			2 => scoreForKillingPlayerInOtherClan, 
			3 => scoreForKilledByPlayerInOtherClan, 
			4 => scoreForKillingUnarmedPlayer, 
			5 => scoreForDestroyingToolCupboards, 
			6 => scoreForHackingCrates, 
			7 => scoreForOpeningHackedCrates, 
			8 => scoreForDestroyingBradley, 
			9 => scoreForRunningExcavator, 
			10 => scoreForReachingCargoShip, 
			11 => scoreForLootingEliteCrate, 
			12 => scoreForDestroyingPatrolHeli, 
			13 => scoreForSwipingRedKeycard, 
			14 => scoreForInsertHeavyFuseInPowerPlant, 
			15 => scoreForLootingSatellite, 
			16 => scoreForEnablingWaterTreatmentPlant, 
			17 => scoreForLaunchingSatellite, 
			18 => scoreForStartingOilRigFuelSwitch, 
			0 => 0, 
			_ => Unknown(eventType), 
		};
		static int Unknown(ClanScoreEventType type)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Debug.LogError((object)$"Unhandled score event type: {type}");
			return 0;
		}
	}

	[ServerVar(Help = "Disbands your current clan")]
	public static void Disband(Arg arg)
	{
		BasePlayer player = ArgEx.Player(arg);
		if ((Object)(object)player == (Object)null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if ((Object)(object)ClanManager.ServerInstance == (Object)null)
		{
			arg.ReplyWith("ClanManager is null!");
		}
		else
		{
			DisbandImpl();
		}
		async void DisbandImpl()
		{
			_ = 1;
			try
			{
				IClan clan = await GetPlayerClan(player);
				if (clan != null)
				{
					ClanResult val = await clan.Disband((ulong)player.userID);
					if ((int)val != 1)
					{
						player.ConsoleMessage($"Failed to disband clan: {val}");
					}
					else
					{
						player.ConsoleMessage($"Disbanded clan ID {clan.ClanId}");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	[ServerVar(Help = "Adds a player by SteamID64 to your current clan")]
	public static void AddToClan(Arg arg)
	{
		ulong steamId = arg.GetUInt64(0, 0uL);
		if (steamId == 0L)
		{
			arg.ReplyWith("Usage: clan.addtoclan <steamID>");
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		if ((Object)(object)player == (Object)null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if ((Object)(object)ClanManager.ServerInstance == (Object)null)
		{
			arg.ReplyWith("ClanManager is null!");
		}
		else
		{
			AddToClanImpl();
		}
		async void AddToClanImpl()
		{
			_ = 2;
			try
			{
				IClan clan = await GetPlayerClan(player);
				if (clan != null)
				{
					if (clan.Invites.All((ClanInvite i) => i.SteamId != steamId))
					{
						ClanResult val = await clan.Invite(steamId, (ulong)player.userID);
						if ((int)val != 1)
						{
							player.ConsoleMessage($"Failed to invite {steamId} to your clan: {val}");
							return;
						}
					}
					ClanResult val2 = await clan.AcceptInvite(steamId);
					if ((int)val2 != 1)
					{
						player.ConsoleMessage($"Failed to accept invite for {steamId} to join your clan: {val2}");
					}
					else
					{
						player.ConsoleMessage($"Added {steamId} to your clan.");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				player.ConsoleMessage(ex.ToString());
			}
		}
	}

	[ServerVar(Help = "Adds a generic score event to your clan")]
	public static void ScoreTest(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Can only be used by a player!");
		}
		else if ((Object)(object)ClanManager.ServerInstance == (Object)null)
		{
			arg.ReplyWith("CLanManager is null!");
		}
		else if (basePlayer.serverClan == null)
		{
			arg.ReplyWith("Player's clan is null!");
		}
		else
		{
			basePlayer.AddClanScore((ClanScoreEventType)0);
		}
	}
}
