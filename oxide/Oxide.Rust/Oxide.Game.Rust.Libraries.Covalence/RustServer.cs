using System;
using System.Globalization;
using System.IO;
using System.Net;
using ConVar;
using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Rust;

namespace Oxide.Game.Rust.Libraries.Covalence;

public class RustServer : IServer
{
	internal readonly Server Server = new Server();

	private static IPAddress address;

	private static IPAddress localAddress;

	public string Name
	{
		get
		{
			return ConVar.Server.hostname;
		}
		set
		{
			ConVar.Server.hostname = value;
		}
	}

	public IPAddress Address
	{
		get
		{
			try
			{
				if (address == null || !Utility.ValidateIPv4(address.ToString()))
				{
					if (Utility.ValidateIPv4(ConVar.Server.ip) && !Utility.IsLocalIP(ConVar.Server.ip))
					{
						IPAddress.TryParse(ConVar.Server.ip, out address);
						Interface.Oxide.LogInfo($"IP address from command-line: {address}");
					}
					else
					{
						WebClient webClient = new WebClient();
						IPAddress.TryParse(webClient.DownloadString("http://api.ipify.org"), out address);
						Interface.Oxide.LogInfo($"IP address from external API: {address}");
					}
				}
				return address;
			}
			catch (Exception exception)
			{
				RemoteLogger.Exception("Couldn't get server's public IP address", exception);
				return IPAddress.Any;
			}
		}
	}

	public IPAddress LocalAddress
	{
		get
		{
			try
			{
				return localAddress ?? (localAddress = Utility.GetLocalIP());
			}
			catch (Exception exception)
			{
				RemoteLogger.Exception("Couldn't get server's local IP address", exception);
				return IPAddress.Any;
			}
		}
	}

	public ushort Port => (ushort)ConVar.Server.port;

	public string Version => BuildInfo.Current.Build.Number;

	public string Protocol => Protocol.printable;

	public CultureInfo Language => CultureInfo.InstalledUICulture;

	public int Players => BasePlayer.activePlayerList.Count;

	public int MaxPlayers
	{
		get
		{
			return ConVar.Server.maxplayers;
		}
		set
		{
			ConVar.Server.maxplayers = value;
		}
	}

	public DateTime Time
	{
		get
		{
			return TOD_Sky.Instance.Cycle.DateTime;
		}
		set
		{
			TOD_Sky.Instance.Cycle.DateTime = value;
		}
	}

	public SaveInfo SaveInfo { get; } = SaveInfo.Create(World.SaveFileName);

	public void Ban(string id, string reason, TimeSpan duration = default(TimeSpan))
	{
		if (!IsBanned(id))
		{
			long expiry = -1L;
			if (duration != TimeSpan.Zero)
			{
				DateTime dateTime = DateTime.UtcNow.Add(duration);
				expiry = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
			}
			ServerUsers.Set(ulong.Parse(id), ServerUsers.UserGroup.Banned, Name, reason, expiry);
			ServerUsers.Save();
		}
	}

	public TimeSpan BanTimeRemaining(string id)
	{
		return IsBanned(id) ? TimeSpan.MaxValue : TimeSpan.Zero;
	}

	public bool IsBanned(string id)
	{
		return ServerUsers.Is(ulong.Parse(id), ServerUsers.UserGroup.Banned);
	}

	public void Save()
	{
		ConVar.Server.save(null);
		File.WriteAllText(ConVar.Server.GetServerFolder("cfg") + "/serverauto.cfg", ConsoleSystem.SaveToConfigString(bServer: true));
		ServerUsers.Save();
	}

	public void Unban(string id)
	{
		if (IsBanned(id))
		{
			ServerUsers.Remove(ulong.Parse(id));
			ServerUsers.Save();
		}
	}

	public void Broadcast(string message, string prefix, params object[] args)
	{
		Server.Broadcast(message, prefix, 0uL, args);
	}

	public void Broadcast(string message)
	{
		Broadcast(message, null);
	}

	public void Command(string command, params object[] args)
	{
		Server.Command(command, args);
	}
}
