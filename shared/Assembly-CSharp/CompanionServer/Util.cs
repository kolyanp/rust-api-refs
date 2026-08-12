using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public static class Util
{
	public const int OceanMargin = 500;

	public static readonly Phrase NotificationEmpty;

	public static readonly Phrase NotificationDisabled;

	public static readonly Phrase NotificationRateLimit;

	public static readonly Phrase NotificationServerError;

	public static readonly Phrase NotificationNoTargets;

	public static readonly Phrase NotificationTooManySubscribers;

	public static readonly Phrase NotificationUnknown;

	public static Vector2 WorldToMap(Vector3 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(worldPos.x - TerrainMeta.Position.x, worldPos.z - TerrainMeta.Position.z);
	}

	public static void SendSignedInNotification(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && player.currentTeam != 0L)
		{
			Dictionary<string, string> dictionary = TryGetServerPairingData();
			if (dictionary != null)
			{
				dictionary.Add("type", "login");
				dictionary.Add("targetId", player.UserIDString);
				dictionary.Add("targetName", StringExtensions.Truncate(player.displayName, 128, (string)null));
				RelationshipManager.ServerInstance.FindTeam(player.currentTeam)?.SendNotification(NotificationChannel.PlayerLoggedIn, player.displayName + " is now online", ConVar.Server.hostname, dictionary, player.userID);
			}
		}
	}

	public static void SendDeathNotification(BasePlayer player, BaseEntity killer)
	{
		string value;
		string text;
		if (killer is BasePlayer basePlayer && ((object)basePlayer).GetType() == typeof(BasePlayer))
		{
			value = basePlayer.UserIDString;
			text = basePlayer.displayName;
		}
		else
		{
			if (PrefabAttribute.server.Find(killer.prefabID, out PrefabInformation result))
			{
				Phrase title = result.title;
				if (!string.IsNullOrEmpty((title != null) ? title.english : null))
				{
					value = "";
					text = result.title.english;
					goto IL_0081;
				}
			}
			value = "";
			text = killer.ShortPrefabName;
		}
		goto IL_0081;
		IL_0081:
		if (!((Object)(object)player == (Object)null) && !string.IsNullOrEmpty(text))
		{
			Dictionary<string, string> dictionary = TryGetServerPairingData();
			if (dictionary != null)
			{
				dictionary.Add("type", "death");
				dictionary.Add("targetId", value);
				dictionary.Add("targetName", StringExtensions.Truncate(text, 128, (string)null));
				NotificationList.SendNotificationTo(player.userID, NotificationChannel.PlayerDied, "You were killed by " + text, ConVar.Server.hostname, dictionary);
			}
		}
	}

	public static Task<NotificationSendResult> SendPairNotification(string type, BasePlayer player, string title, string message, Dictionary<string, string> data)
	{
		if (!Server.IsEnabled)
		{
			return Task.FromResult(NotificationSendResult.Disabled);
		}
		if (!Server.CanSendPairingNotification(player.userID))
		{
			return Task.FromResult(NotificationSendResult.RateLimited);
		}
		if (data == null)
		{
			data = TryGetPlayerPairingData(player);
			if (data == null)
			{
				return Task.FromResult(NotificationSendResult.Failed);
			}
		}
		data.Add("type", type);
		return NotificationList.SendNotificationTo(player.userID, NotificationChannel.Pairing, title, message, data);
	}

	public static Dictionary<string, string> TryGetServerPairingData()
	{
		string value = App.GetPublicIP() ?? "";
		if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(App.serverid))
		{
			return null;
		}
		Dictionary<string, string> dictionary = Pool.Get<Dictionary<string, string>>();
		dictionary.Clear();
		dictionary.Add("id", App.serverid);
		dictionary.Add("name", StringExtensions.Truncate(ConVar.Server.hostname, 128, (string)null));
		dictionary.Add("desc", StringExtensions.Truncate(ConVar.Server.description, 512, (string)null));
		dictionary.Add("img", StringExtensions.Truncate(ConVar.Server.headerimage, 128, (string)null));
		dictionary.Add("logo", StringExtensions.Truncate(ConVar.Server.logoimage, 128, (string)null));
		dictionary.Add("url", StringExtensions.Truncate(ConVar.Server.url, 128, (string)null));
		dictionary.Add("ip", value);
		dictionary.Add("port", App.port.ToString("G", CultureInfo.InvariantCulture));
		if (NexusServer.Started)
		{
			int? nexusId = NexusServer.NexusId;
			string zoneKey = NexusServer.ZoneKey;
			if (nexusId.HasValue && zoneKey != null)
			{
				dictionary.Add("nexus", Nexus.endpoint);
				dictionary.Add("nexusId", nexusId.Value.ToString("G"));
				dictionary.Add("nexusZone", zoneKey);
			}
		}
		return dictionary;
	}

	public static Dictionary<string, string> TryGetPlayerPairingData(BasePlayer player)
	{
		Dictionary<string, string> dictionary = TryGetServerPairingData();
		if (dictionary == null)
		{
			return null;
		}
		int orGenerateAppToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(player.userID, out var _);
		dictionary.Add("playerId", player.UserIDString);
		dictionary.Add("playerToken", orGenerateAppToken.ToString("G", CultureInfo.InvariantCulture));
		return dictionary;
	}

	public static void BroadcastAppTeamRemoval(this BasePlayer player)
	{
		AppBroadcast val = Pool.Get<AppBroadcast>();
		try
		{
			val.teamChanged = Pool.Get<AppTeamChanged>();
			val.teamChanged.playerId = player.userID;
			val.teamChanged.teamInfo = player.GetAppTeamInfo(player.userID);
			Server.Broadcast(new PlayerTarget(player.userID), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void BroadcastAppTeamUpdate(this RelationshipManager.PlayerTeam team)
	{
		AppBroadcast val = Pool.Get<AppBroadcast>();
		val.teamChanged = Pool.Get<AppTeamChanged>();
		val.ShouldPool = false;
		foreach (ulong member in team.members)
		{
			val.teamChanged.playerId = member;
			val.teamChanged.teamInfo = team.GetAppTeamInfo(member);
			Server.Broadcast(new PlayerTarget(member), val);
		}
		val.ShouldPool = true;
		val.Dispose();
	}

	public static void BroadcastTeamChat(this RelationshipManager.PlayerTeam team, ulong steamId, string name, string message, string color)
	{
		uint current = (uint)Epoch.Current;
		Server.TeamChat.Record(team.teamID, steamId, name, message, color, current);
		AppBroadcast val = Pool.Get<AppBroadcast>();
		val.teamMessage = Pool.Get<AppNewTeamMessage>();
		val.teamMessage.message = Pool.Get<AppTeamMessage>();
		val.ShouldPool = false;
		AppTeamMessage message2 = val.teamMessage.message;
		message2.steamId = steamId;
		message2.name = name;
		message2.message = message;
		message2.color = color;
		message2.time = current;
		foreach (ulong member in team.members)
		{
			Server.Broadcast(new PlayerTarget(member), val);
		}
		val.ShouldPool = true;
		val.Dispose();
	}

	public static async void SendNotification(this RelationshipManager.PlayerTeam team, NotificationChannel channel, string title, string body, Dictionary<string, string> data, ulong ignorePlayer = 0uL)
	{
		List<ulong> steamIds = Pool.Get<List<ulong>>();
		foreach (ulong member in team.members)
		{
			if (member != ignorePlayer)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if ((Object)(object)basePlayer == (Object)null || basePlayer.net?.connection == null)
				{
					steamIds.Add(member);
				}
			}
		}
		await NotificationList.SendNotificationTo(steamIds, channel, title, body, data);
		Pool.FreeUnmanaged<ulong>(ref steamIds);
	}

	public static string ToErrorCode(this ValidationResult result)
	{
		return result switch
		{
			ValidationResult.NotFound => "not_found", 
			ValidationResult.RateLimit => "rate_limit", 
			ValidationResult.Banned => "banned", 
			_ => "unknown", 
		};
	}

	public static Phrase ToErrorMessage(this NotificationSendResult result)
	{
		return (Phrase)(result switch
		{
			NotificationSendResult.Sent => null, 
			NotificationSendResult.Empty => NotificationEmpty, 
			NotificationSendResult.Disabled => NotificationDisabled, 
			NotificationSendResult.RateLimited => NotificationRateLimit, 
			NotificationSendResult.ServerError => NotificationServerError, 
			NotificationSendResult.NoTargetsFound => NotificationNoTargets, 
			NotificationSendResult.TooManySubscribers => NotificationTooManySubscribers, 
			_ => NotificationUnknown, 
		});
	}

	static Util()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		NotificationEmpty = new Phrase("app.error.empty", "Notification was not sent because it was missing some content.");
		NotificationDisabled = new Phrase("app.error.disabled", "Rust+ features are disabled on this server.");
		NotificationRateLimit = new Phrase("app.error.ratelimit", "You are sending too many notifications at a time. Please wait and then try again.");
		NotificationServerError = new Phrase("app.error.servererror", "The companion server failed to send the notification.");
		NotificationNoTargets = new Phrase("app.error.notargets", "Open the Rust+ menu in-game to pair your phone with this server.");
		NotificationTooManySubscribers = new Phrase("app.error.toomanysubs", "There are too many players subscribed to these notifications.");
		NotificationUnknown = new Phrase("app.error.unknown", "An unknown error occurred sending the notification.");
	}
}
