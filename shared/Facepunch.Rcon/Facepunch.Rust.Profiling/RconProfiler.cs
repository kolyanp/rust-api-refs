using System;
using Fleck;

namespace Facepunch.Rust.Profiling;

public static class RconProfiler
{
	public static int mode;

	public static int ClampedMessageLength = 512;

	public static TimeSpan ExecutionTime;

	private static RconProfilerStats currentStats = new RconProfilerStats();

	private static int lastClientCount;

	private static object Lock = new object();

	public static void OnRconMessage(string ip, int port, int connectionId, string message)
	{
		if (mode == 0)
		{
			return;
		}
		lock (Lock)
		{
			currentStats.MessageCount++;
			currentStats.MessageLengthSum += message.Length;
		}
		if (mode < 3)
		{
			return;
		}
		RconMessageStats item = new RconMessageStats
		{
			MessageLength = message.Length,
			Message = ((message.Length <= ClampedMessageLength) ? message : message.Substring(ClampedMessageLength)),
			ConnectionId = connectionId,
			IP = ip,
			Port = port
		};
		lock (Lock)
		{
			currentStats.Messages.Add(item);
		}
	}

	public static void OnNewConnection(IWebSocketConnection socket, int connectionId)
	{
		LogConnection(socket, connectionId, success: true, null);
	}

	public static void OnFailedConnection(IWebSocketConnection socket, string passwordAttempt)
	{
		LogConnection(socket, 0, success: false, passwordAttempt);
	}

	public static void OnDisconnect(string ip, int port, int connectionId)
	{
		if (mode < 2)
		{
			return;
		}
		RconDisconnects item = new RconDisconnects
		{
			ConnectionId = connectionId,
			IP = ip,
			Port = port
		};
		lock (Lock)
		{
			currentStats.Disconnects.Add(item);
		}
	}

	public static void OnError(IWebSocketConnection socket)
	{
		if (mode == 0)
		{
			return;
		}
		lock (Lock)
		{
			currentStats.ErrorCount++;
		}
	}

	public static void UpdateClientCount(int count)
	{
		lock (Lock)
		{
			lastClientCount = count;
		}
	}

	private static void LogConnection(IWebSocketConnection socket, int connectionId, bool success, string passwordAttempt)
	{
		if (mode == 0)
		{
			return;
		}
		lock (Lock)
		{
			if (success)
			{
				currentStats.NewConnectionCount++;
			}
			else
			{
				currentStats.FailedConnectionCount++;
			}
		}
		if (mode < 2)
		{
			return;
		}
		RconConnectionAttempt item = new RconConnectionAttempt
		{
			IP = socket.ConnectionInfo.ClientIpAddress.ToString(),
			Port = socket.ConnectionInfo.ClientPort,
			ConnectionId = connectionId,
			Success = success,
			PasswordAttempt = passwordAttempt
		};
		lock (Lock)
		{
			currentStats.ConnectionAttempts.Add(item);
		}
	}

	public static RconProfilerStats GetCurrentStats(bool reset = true)
	{
		if (mode == 0)
		{
			return RconProfilerStats.Default;
		}
		lock (Lock)
		{
			RconProfilerStats rconProfilerStats = currentStats;
			rconProfilerStats.ConnectionCount = lastClientCount;
			if (reset)
			{
				currentStats = Pool.Get<RconProfilerStats>();
			}
			return rconProfilerStats;
		}
	}

	public static void Reset()
	{
		ExecutionTime = TimeSpan.Zero;
	}
}
