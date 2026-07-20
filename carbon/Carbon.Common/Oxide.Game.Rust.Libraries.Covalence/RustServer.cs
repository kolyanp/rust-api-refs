using System;
using System.Globalization;
using System.IO;
using System.Net;
using Carbon;
using ConVar;
using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Rust;

namespace Oxide.Game.Rust.Libraries.Covalence;

public struct RustServer : IServer
{
	internal readonly Server Server = new Server();

	private static IPAddress address;

	private static IPAddress localAddress;

	public string Name
	{
		get
		{
			return Server.hostname;
		}
		set
		{
			Server.hostname = value;
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
					if (Utility.ValidateIPv4(Server.ip) && !Utility.IsLocalIP(Server.ip))
					{
						IPAddress.TryParse(Server.ip, out address);
						Interface.Oxide.LogInfo($"IP address from command-line: {address}");
					}
					else
					{
						IPAddress.TryParse(new WebClient().DownloadString("http://api.ipify.org"), out address);
						Interface.Oxide.LogInfo($"IP address from external API: {address}");
					}
				}
				return address;
			}
			catch (Exception ex)
			{
				Logger.Error("Couldn't get server's public IP address", ex);
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
				IPAddress result;
				if ((result = localAddress) == null)
				{
					result = (localAddress = Utility.GetLocalIP());
				}
				return result;
			}
			catch (Exception ex)
			{
				Logger.Error("Couldn't get server's local IP address", ex);
				return IPAddress.Any;
			}
		}
	}

	public ushort Port => (ushort)Server.port;

	public string Version => BuildInfo.Current.Build.Number;

	public string Protocol => Protocol.printable;

	public CultureInfo Language => CultureInfo.InstalledUICulture;

	public int Players => BasePlayer.activePlayerList.Count;

	public int MaxPlayers
	{
		get
		{
			return Server.maxplayers;
		}
		set
		{
			Server.maxplayers = value;
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

	public RustServer()
	{
	}

	public void Ban(string id, string reason, TimeSpan duration = default(TimeSpan))
	{
		if (!IsBanned(id))
		{
			long num = -1L;
			if (duration != TimeSpan.Zero)
			{
				DateTime dateTime = DateTime.UtcNow.Add(duration);
				num = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
			}
			ServerUsers.Set(ulong.Parse(id), (UserGroup)3, Name, reason, num);
			ServerUsers.Save();
		}
	}

	public TimeSpan BanTimeRemaining(string id)
	{
		if (!IsBanned(id))
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.MaxValue;
	}

	public bool IsBanned(string id)
	{
		return ServerUsers.Is(ulong.Parse(id), (UserGroup)3);
	}

	public void Save()
	{
		Server.save((Arg)null);
		File.WriteAllText(Server.GetServerFolder("cfg") + "/serverauto.cfg", ConsoleSystem.SaveToConfigString(true));
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
