using System;
using System.Collections.Generic;
using Facepunch;
using Network;

public struct RpcTarget
{
	[Flags]
	public enum RpcTargetFlags
	{
		Player = 1,
		Spectators = 2,
		ClientDemoRecorders = 4,
		All = -1
	}

	public string Function;

	public SendInfo Connections;

	public bool ToNetworkGroup;

	public bool UsingPooledConnections;

	public static RpcTarget NetworkGroup(string funcName)
	{
		return new RpcTarget
		{
			Function = funcName,
			ToNetworkGroup = true
		};
	}

	public static RpcTarget NetworkGroup(string funcName, BaseNetworkable entity)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(entity.net.group.subscribers)
		};
	}

	public static RpcTarget NetworkGroup(string funcName, BaseNetworkable entity, SendMethod method, Priority priority)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(entity.net.group.subscribers)
			{
				method = method,
				priority = priority
			}
		};
	}

	public static RpcTarget Player(string funcName, BasePlayer target)
	{
		return Player(funcName, target.IsValid() ? target.net.connection : null);
	}

	public static RpcTarget Player(string funcName, Connection connection)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(connection)
		};
	}

	public static RpcTarget Players(string funcName, List<Connection> connections)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(connections)
		};
	}

	public static RpcTarget Players(string funcName, List<Connection> connections, SendMethod method, Priority priority)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(connections)
			{
				method = method,
				priority = priority
			}
		};
	}

	public static RpcTarget SendInfo(string funcName, SendInfo sendInfo)
	{
		return new RpcTarget
		{
			Function = funcName,
			Connections = sendInfo
		};
	}

	public static RpcTarget FromFlags(RpcTargetFlags rpcTargetFlags, string funcName, BasePlayer player)
	{
		if (!player.IsValid() || rpcTargetFlags == (RpcTargetFlags)0)
		{
			return default(RpcTarget);
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		HashSet<Connection> hashSet = Pool.Get<HashSet<Connection>>();
		if ((rpcTargetFlags & RpcTargetFlags.Player) != 0)
		{
			Connection connection = player.net.connection;
			if (connection != null && hashSet.Add(connection))
			{
				list.Add(player.net.connection);
			}
		}
		if ((rpcTargetFlags & RpcTargetFlags.Spectators) != 0 && player.IsBeingSpectated)
		{
			ReadOnlySpan<BasePlayer> spectators = player.GetSpectators();
			for (int i = 0; i < spectators.Length; i++)
			{
				BasePlayer basePlayer = spectators[i];
				Connection connection2 = basePlayer.net.connection;
				if (connection2 != null && hashSet.Add(connection2))
				{
					list.Add(basePlayer.net.connection);
				}
			}
		}
		if ((rpcTargetFlags & RpcTargetFlags.ClientDemoRecorders) != 0)
		{
			for (int j = 0; j < BasePlayer.playersRecordingClientDemos.Count; j++)
			{
				BasePlayer basePlayer2 = BasePlayer.playersRecordingClientDemos[j];
				Connection connection3 = basePlayer2.net.connection;
				if (connection3 != null && !hashSet.Contains(connection3) && player.ShouldNetworkToSkipOcclusion(basePlayer2))
				{
					hashSet.Add(connection3);
					list.Add(basePlayer2.net.connection);
				}
			}
		}
		Pool.FreeUnmanaged<Connection>(ref hashSet);
		return new RpcTarget
		{
			Function = funcName,
			Connections = new SendInfo(list),
			UsingPooledConnections = true
		};
	}

	public static RpcTarget PlayerAndSpectators(string funcName, BasePlayer player)
	{
		return FromFlags(RpcTargetFlags.Player | RpcTargetFlags.Spectators, funcName, player);
	}
}
