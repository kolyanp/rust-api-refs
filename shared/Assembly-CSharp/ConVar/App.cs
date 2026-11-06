using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CompanionServer;
using Facepunch;
using Facepunch.Extend;
using Steamworks;
using UnityEngine;

namespace ConVar;

[Factory("app")]
public class App : ConsoleSystem
{
	[ServerVar(Clientside = true, Help = "Base address for the Rust+ companion server API")]
	public static string endpoint = "https://companion-rust.facepunch.com/api";

	[ServerVar(Help = "(Generated) IP address the server listens on for incoming connections; leave empty to bind to all available network interfaces")]
	public static string listenip = "";

	[ServerVar(Help = "(Generated) UDP port number the server listens on; default is 28015; must be open in firewall for players to connect")]
	public static int port;

	[ServerVar(Help = "(Generated) Public IP address advertised to the Steam server browser; leave empty to auto-detect; set explicitly if behind NAT")]
	public static string publicip = "";

	[ServerVar(Help = "Enables sending push notifications")]
	public static bool notifications = true;

	[ReplicatedVar(Default = "")]
	public static string serverid = "";

	[ServerVar(Help = "Cooldown time before alarms can send another notification (in seconds)")]
	public static float alarmcooldown = 30f;

	[ServerVar(Help = "Maximum simultaneous backhaul connections from the Rust+ backend (players share them as channels); -1 for no limit")]
	public static int maxconnections = 500;

	[ServerVar(Help = "Maximum simultaneous Rust+ channels per player, and backhaul connections per IP address; -1 for no limit")]
	public static int maxconnectionsperip = 10;

	[ServerVar(Help = "(Generated) When enabled, server-side C# exceptions are written to the server log file; disabling reduces log noise on servers with known non-critical exceptions")]
	public static bool logexceptions = true;

	[ServerUserVar]
	public static async void pair(Arg arg)
	{
		try
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (!((Object)(object)basePlayer == (Object)null))
			{
				Dictionary<string, string> dictionary = Util.TryGetPlayerPairingData(basePlayer);
				if (dictionary == null)
				{
					arg.ReplyWith("This server is not configured to allow Rust+ connections.");
					return;
				}
				NotificationSendResult notificationSendResult = await Util.SendPairNotification("server", basePlayer, StringExtensions.Truncate(Server.hostname, 128, (string)null), "Tap to pair with this server.", dictionary);
				arg.ReplyWith((notificationSendResult == NotificationSendResult.Sent) ? "Sent pairing notification." : notificationSendResult.ToErrorMessage().english);
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	[ServerUserVar]
	public static void regeneratetoken(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			SingletonComponent<ServerMgr>.Instance.persistance.RegenerateAppToken(basePlayer.userID);
			arg.ReplyWith("Regenerated Rust+ token");
		}
	}

	[ServerVar(Help = "Prints the Rust+ companion server ID, addresses and open backhauls from the backend")]
	public static void info(Arg arg)
	{
		if (!CompanionServer.Server.IsEnabled)
		{
			arg.ReplyWith("Companion server is not enabled");
			return;
		}
		Listener listener = CompanionServer.Server.Listener;
		List<BackhaulConnection> list = Pool.Get<List<BackhaulConnection>>();
		listener.GetBackhauls(list);
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumns(new string[2] { "Address", "Channels" });
			int num = 0;
			foreach (BackhaulConnection item in list)
			{
				int channelCount = item.ChannelCount;
				num += channelCount;
				val.AddRow(new string[2]
				{
					item.Address?.ToString() ?? "unknown",
					channelCount.ToString()
				});
			}
			val.AddRow(new string[2]
			{
				"Total",
				num.ToString()
			});
			Pool.FreeUnmanaged<BackhaulConnection>(ref list);
			arg.ReplyWith(string.Format("Server ID: {0}\nListening on: {1}:{2}\nBackend connects to: {3}:{4}\n\n{5}", new object[6]
			{
				serverid,
				listener.Address,
				listener.Port,
				GetPublicIP() ?? "null",
				port,
				val
			}));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Retry initializing the Rust+ companion server if it previously failed, pass true to restart it even if it's already running; shuts it down if app.port is negative")]
	public static void retry_initialize(Arg arg)
	{
		if (port < 0)
		{
			CompanionServer.Server.Shutdown();
			arg.ReplyWith("Companion server port is negative, shut down the companion server");
			return;
		}
		bool flag = arg.GetBool(0);
		if (CompanionServer.Server.IsEnabled && !flag)
		{
			arg.ReplyWith("Companion server is already initialized! Use 'app.retry_initialize true' to restart it anyway");
			return;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.rustPlus)
		{
			arg.ReplyWith("Companion server is disabled by gamemode, cannot initialize companion server");
			return;
		}
		if (CompanionServer.Server.IsRegistering)
		{
			arg.ReplyWith("Companion server registration is still in progress, try again once it finishes");
			return;
		}
		arg.ReplyWith("Trying to initialize companion server...");
		CompanionServer.Server.Shutdown();
		CompanionServer.Server.Initialize();
	}

	[ServerVar(Help = "(Generated) Bans a player by Steam ID from the server via the app layer, adding them to the banlist and kicking them if connected")]
	public static void appban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: true) ? $"Banned {uLong} from using the companion app" : $"{uLong} is already banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	[ServerVar(Help = "(Generated) Removes a ban for the specified Steam ID from the server banlist, allowing the player to reconnect")]
	public static void appunban(Arg arg)
	{
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			arg.ReplyWith("Usage: app.appunban <steamID64>");
			return;
		}
		string strValue = (SingletonComponent<ServerMgr>.Instance.persistance.SetAppTokenLocked(uLong, locked: false) ? $"Unbanned {uLong}, they can use the companion app again" : $"{uLong} is not banned from using the companion app");
		arg.ReplyWith(strValue);
	}

	public static IPAddress GetListenIP()
	{
		if (!string.IsNullOrWhiteSpace(listenip))
		{
			if (!IPAddress.TryParse(listenip, out IPAddress address) || address.AddressFamily != AddressFamily.InterNetwork)
			{
				Debug.LogError((object)("Invalid app.listenip: " + listenip));
				return IPAddress.Any;
			}
			return address;
		}
		return IPAddress.Any;
	}

	public static async ValueTask<string> GetPublicIPAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		Stopwatch timer = null;
		string publicIP;
		while (true)
		{
			bool num = timer != null && timer.Elapsed.TotalMinutes > 5.0;
			publicIP = GetPublicIP();
			if (num || (!string.IsNullOrWhiteSpace(publicIP) && publicIP != "0.0.0.0"))
			{
				break;
			}
			if (timer == null)
			{
				timer = Stopwatch.StartNew();
			}
			await Task.Delay(10000, cancellationToken);
		}
		return publicIP;
	}

	public static string GetPublicIP()
	{
		if (!string.IsNullOrWhiteSpace(publicip) && IPAddress.TryParse(publicip, out IPAddress address) && address.AddressFamily == AddressFamily.InterNetwork)
		{
			return publicip;
		}
		if (!SteamServer.IsValid)
		{
			return null;
		}
		return SteamServer.PublicIp.ToString();
	}
}
