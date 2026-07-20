using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Facepunch.Rust.Profiling;
using Fleck;
using Oxide.Core;
using UnityEngine;

namespace Facepunch.Rcon;

[ConsoleSystem.Factory("rcon")]
public class Listener
{
	public struct IPNetwork : IEquatable<IPNetwork>
	{
		public uint Address;

		public uint Mask;

		public byte PrefixLength;

		public IPNetwork(string networkAddress)
		{
			if (networkAddress.Contains("/"))
			{
				string[] array = networkAddress.Split('/');
				Address = BitConverter.ToUInt32(IPAddress.Parse(array[0]).GetAddressBytes(), 0);
				PrefixLength = byte.Parse(array[1]);
				if (PrefixLength > 32)
				{
					throw new Exception($"Invalid prefix length {PrefixLength}");
				}
			}
			else
			{
				Address = BitConverter.ToUInt32(IPAddress.Parse(networkAddress).GetAddressBytes(), 0);
				PrefixLength = 32;
			}
			Mask = ((PrefixLength > 0) ? (uint.MaxValue >> 32 - PrefixLength) : 0u);
		}

		public static bool operator ==(IPNetwork a, IPNetwork b)
		{
			if (a.Address == b.Address)
			{
				return a.Mask == b.Mask;
			}
			return false;
		}

		public static bool operator !=(IPNetwork a, IPNetwork b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return (Address, Mask).GetHashCode();
		}

		public bool Equals(IPNetwork other)
		{
			if (Address == other.Address)
			{
				return Mask == other.Mask;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is IPNetwork other)
			{
				return Equals(other);
			}
			return false;
		}

		public IPAddress NetworkAddress()
		{
			return new IPAddress(Address);
		}

		public IPAddress Netmask()
		{
			return new IPAddress(Mask);
		}

		public bool Contains(IPNetwork network)
		{
			return Contains(network.NetworkAddress());
		}

		public bool Contains(IPAddress ip)
		{
			uint num = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
			return (Address & Mask) == (num & Mask);
		}

		public override string ToString()
		{
			return $"{NetworkAddress()}/{PrefixLength}";
		}
	}

	public class FailedIPData
	{
		public bool IsBanned;

		public DateTime BanTime;

		public int Attempts;
	}

	[ServerVar(Help = "How many password failures before banning an RCON client's IP (default: 5)")]
	public static int MaxPasswordFailures = 5;

	[ServerVar(Help = "How long in seconds to ban an IP that has exceeded the maximum password failures (default: 300 seconds)")]
	public static float BanDuration = 300f;

	[ServerVar(Help = "Permanently ban IPs that trigger too many failed attempts (default: false)")]
	public static bool PermanentBanFailedIPs = false;

	[ServerVar(Help = "Log failed attempts and attempts from banned IP addresses (default: true)")]
	public static bool LogFailedAttempts = true;

	[ServerVar(Help = "How long (in seconds) before we allow another rcon connection from the same address (default: 1 second)")]
	public static float ConnectionCooldown = 1f;

	public string Password;

	public int Port;

	public string Address;

	public string SslCertificate;

	public string SslCertificatePassword;

	public string BansFile;

	public Action<IPAddress, int, string> OnMessage;

	private int nextClientId;

	private readonly Dictionary<int, RconConnection> clients = new Dictionary<int, RconConnection>();

	private readonly List<int> deadClients = new List<int>();

	private WebSocketServer server;

	public readonly Dictionary<IPAddress, FailedIPData> FailedIPs = new Dictionary<IPAddress, FailedIPData>();

	public readonly List<IPNetwork> BannedNetworks = new List<IPNetwork>();

	public bool BanIP(string addressNetwork)
	{
		IPNetwork item;
		lock (BannedNetworks)
		{
			try
			{
				item = new IPNetwork(addressNetwork);
			}
			catch
			{
				return false;
			}
			BannedNetworks.Add(item);
		}
		lock (clients)
		{
			foreach (KeyValuePair<int, RconConnection> client in clients)
			{
				if (item.Contains(client.Value.Socket.ConnectionInfo.ClientIpAddress))
				{
					Debug.Log((object)$"RCON: Banned IP {client.Value.Socket.ConnectionInfo.ClientIpAddress} connected and was kicked.");
					client.Value.Socket.Close();
				}
			}
		}
		SaveBans();
		return true;
	}

	public bool UnbanIP(string addressNetwork)
	{
		bool flag = false;
		IPNetwork item = new IPNetwork(addressNetwork);
		lock (FailedIPs)
		{
			foreach (KeyValuePair<IPAddress, FailedIPData> failedIP in FailedIPs)
			{
				if (!(failedIP.Key.ToString() != addressNetwork))
				{
					flag = FailedIPs.Remove(failedIP.Key);
					break;
				}
			}
		}
		lock (BannedNetworks)
		{
			flag = BannedNetworks.Remove(item) || flag;
		}
		if (flag)
		{
			SaveBans();
		}
		return flag;
	}

	public void ClearFailedIPData()
	{
		lock (FailedIPs)
		{
			FailedIPs.Clear();
		}
	}

	public bool IsBannedIP(IPAddress IP)
	{
		lock (BannedNetworks)
		{
			foreach (IPNetwork bannedNetwork in BannedNetworks)
			{
				if (bannedNetwork.Contains(IP))
				{
					return true;
				}
			}
		}
		lock (FailedIPs)
		{
			FailedIPData failedIPData = GetFailedIPData(IP);
			if (failedIPData.IsBanned && DateTime.UtcNow < failedIPData.BanTime)
			{
				return true;
			}
			if (failedIPData.IsBanned)
			{
				failedIPData.IsBanned = false;
				failedIPData.Attempts = 0;
			}
		}
		return false;
	}

	public FailedIPData GetFailedIPData(IPAddress IP)
	{
		lock (FailedIPs)
		{
			if (FailedIPs.TryGetValue(IP, out var value))
			{
				return value;
			}
			FailedIPs[IP] = new FailedIPData();
			return FailedIPs[IP];
		}
	}

	public void SaveBans()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IPNetwork bannedNetwork in BannedNetworks)
		{
			stringBuilder.Append(bannedNetwork.ToString()).Append("\n");
		}
		File.WriteAllText(BansFile, stringBuilder.ToString());
	}

	public void LoadBans()
	{
		if (!File.Exists(BansFile))
		{
			return;
		}
		string[] array = File.ReadAllLines(BansFile);
		foreach (string networkAddress in array)
		{
			try
			{
				IPNetwork item = new IPNetwork(networkAddress);
				BannedNetworks.Add(item);
			}
			catch (Exception ex)
			{
				Debug.Log((object)("RCON: Failed to load ban, skipping: " + ex.Message));
			}
		}
	}

	public void Start(int maxConnections, int maxConnectionsPerIP)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		Shutdown();
		LoadBans();
		bool num = !string.IsNullOrEmpty(SslCertificate) && !string.IsNullOrEmpty(SslCertificatePassword);
		IPAddress address = IPAddress.Any;
		string arg = (num ? "wss" : "ws");
		if (Address != null && !IPAddress.TryParse(Address, out address))
		{
			address = IPAddress.Any;
		}
		string text = $"{arg}://{address}:{Port}";
		server = new WebSocketServer(text, true, 100, maxConnections, maxConnectionsPerIP);
		server.ListenerSocket.NoDelay = true;
		server.RestartAfterListenError = true;
		if (num)
		{
			X509Certificate2 certificate = new X509Certificate2(SslCertificate, SslCertificatePassword);
			server.Certificate = certificate;
		}
		string requiredPath = "/" + Password;
		server.Start((Action<IWebSocketConnection>)delegate(IWebSocketConnection socket)
		{
			IWebSocketConnection socket2 = socket;
			IPAddress address2 = socket2.ConnectionInfo.ClientIpAddress;
			if (IsBannedIP(address2))
			{
				if (LogFailedAttempts)
				{
					Debug.Log((object)$"RCON: Banned IP {address2} attempted to connect.");
				}
				if (socket2.ConnectionInfo.Path == requiredPath && LogFailedAttempts)
				{
					Debug.Log((object)$"RCON: CRITICAL - Banned IP {address2} supplied the correct password. Access was still denied.");
				}
				socket2.Close();
			}
			else
			{
				if (Interface.CallHook("OnRconConnection", socket.ConnectionInfo.ClientIpAddress) == null)
				{
					if (!(socket2.ConnectionInfo.Path != requiredPath))
					{
						int id = Interlocked.Increment(ref nextClientId);
						string ipString = address2.ToString();
						int port = socket2.ConnectionInfo.ClientPort;
						RconProfiler.OnNewConnection(socket2, id);
						socket2.OnOpen = delegate
						{
							lock (clients)
							{
								clients.Add(id, new RconConnection(socket2, id));
								RconProfiler.UpdateClientCount(clients.Count);
							}
						};
						socket2.OnClose = delegate
						{
							lock (clients)
							{
								try
								{
									RconProfiler.UpdateClientCount(clients.Count);
									RconProfiler.OnDisconnect(ipString, port, id);
								}
								finally
								{
									clients.Remove(id);
								}
							}
						};
						socket2.OnMessage = delegate(string s)
						{
							if (Interface.CallHook("IOnRconMessage", socket2.ConnectionInfo.ClientIpAddress, s) == null)
							{
								lock (clients)
								{
									if (clients.TryGetValue(id, out var value))
									{
										value.Stats.RecievedMessages++;
									}
								}
								RconProfiler.OnRconMessage(ipString, port, id, s);
								OnMessage?.Invoke(address2, id, s);
							}
						};
						socket2.OnError = delegate(Exception e)
						{
							RconProfiler.OnError(socket2);
							Debug.LogException(e);
						};
						return;
					}
					if (LogFailedAttempts)
					{
						Debug.Log((object)$"RCON: IP {address2} attempted to connect with incorrect password.");
					}
				}
				RconProfiler.OnFailedConnection(socket2, socket2.ConnectionInfo.Path);
				socket2.Close();
				FailedIPData failedIPData = GetFailedIPData(address2);
				if (++failedIPData.Attempts >= MaxPasswordFailures)
				{
					failedIPData.IsBanned = true;
					failedIPData.BanTime = DateTime.UtcNow.AddSeconds(BanDuration);
					Debug.Log((object)$"RCON: IP {address2} banned for {BanDuration} seconds due to reaching max password failure attempts ({MaxPasswordFailures})");
					if (PermanentBanFailedIPs)
					{
						lock (BannedNetworks)
						{
							BannedNetworks.Add(new IPNetwork(address2.ToString()));
						}
						Debug.Log((object)$"RCON: IP {address2} permanently banned due to reaching max password failure attempts ({MaxPasswordFailures})");
					}
				}
			}
		});
	}

	public void Shutdown()
	{
		if (server != null)
		{
			server.Dispose();
			server = null;
		}
	}

	public void BroadcastMessage(string str)
	{
		if (server == null)
		{
			return;
		}
		lock (clients)
		{
			deadClients.Clear();
			foreach (KeyValuePair<int, RconConnection> client in clients)
			{
				if (client.Value.Socket.IsAvailable)
				{
					client.Value.Socket.Send(str);
					client.Value.Stats.BroadcastedMessages++;
				}
				else
				{
					deadClients.Add(client.Key);
				}
			}
			foreach (int deadClient in deadClients)
			{
				if (clients.TryGetValue(deadClient, out var value))
				{
					value.Socket.Close();
					clients.Remove(deadClient);
				}
			}
			RconProfiler.UpdateClientCount(clients.Count);
		}
	}

	public void SendMessage(int target, string str)
	{
		if (server == null)
		{
			return;
		}
		lock (clients)
		{
			if (clients.TryGetValue(target, out var value))
			{
				if (!value.Socket.IsAvailable)
				{
					value.Socket.Close();
					clients.Remove(target);
				}
				else
				{
					value.Socket.Send(str);
					value.Stats.SentMessages++;
					RconProfiler.UpdateClientCount(clients.Count);
				}
			}
		}
	}

	public IList<IPNetwork> GetBannedNetworks()
	{
		lock (BannedNetworks)
		{
			return BannedNetworks.ToList();
		}
	}

	public Dictionary<IPAddress, FailedIPData> GetFailedIPs()
	{
		lock (FailedIPs)
		{
			return FailedIPs.ToDictionary((KeyValuePair<IPAddress, FailedIPData> e) => e.Key, (KeyValuePair<IPAddress, FailedIPData> e) => e.Value);
		}
	}

	public IList<RconClientStats> GetClientStats()
	{
		List<RconClientStats> list = new List<RconClientStats>();
		lock (clients)
		{
			foreach (KeyValuePair<int, RconConnection> client in clients)
			{
				list.Add(client.Value.Stats);
			}
			return list;
		}
	}
}
