using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using UnityEngine;

public static class ServerFileRequestQueue
{
	public enum RequestKind : byte
	{
		GenericFile,
		EntityImage
	}

	private struct QueuedRequest
	{
		public RequestKind Kind;

		public BaseEntity Entity;

		public string ResponseFunction;

		public uint Crc;

		public FileStorage.Type Type;

		public uint Part;

		public bool RespondIfNotFound;
	}

	private class ConnectionState : IPooled
	{
		public Connection connection;

		public double byteBudget;

		public double lastRefill;

		public readonly Queue<QueuedRequest> queue = new Queue<QueuedRequest>();

		public void EnterPool()
		{
		}

		public void LeavePool()
		{
			connection = null;
			byteBudget = ConVar.Server.filerequestbytesburst;
			lastRefill = Time.realtimeSinceStartupAsDouble;
			queue.Clear();
		}
	}

	private static readonly Dictionary<Connection, ConnectionState> States = new Dictionary<Connection, ConnectionState>();

	public static void Request(Connection connection, BaseEntity entity, RequestKind kind, uint crc, FileStorage.Type type, string responseFunction, uint part = 0u, bool respondIfNotFound = false)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (connection == null || !entity.IsValid())
		{
			return;
		}
		ConnectionState connectionState = FindState(connection);
		if (connectionState == null)
		{
			connectionState = Pool.Get<ConnectionState>();
			connectionState.connection = connection;
			States.Add(connection, connectionState);
		}
		RefillBudget(connectionState);
		if (ConVar.Server.filerequestdebug)
		{
			Debug.Log((object)string.Format("[FileRequest] {0} requested {1} crc {2} part {3} from {4}[{5}] - {6}", new object[7]
			{
				connection,
				type,
				crc,
				part,
				entity.ShortPrefabName,
				entity.net.ID,
				UsageString(connectionState)
			}));
		}
		QueuedRequest request = new QueuedRequest
		{
			Kind = kind,
			Entity = entity,
			ResponseFunction = responseFunction,
			Crc = crc,
			Type = type,
			Part = part,
			RespondIfNotFound = respondIfNotFound
		};
		if (connectionState.queue.Count == 0 && connectionState.byteBudget > 0.0)
		{
			Send(connectionState, in request);
		}
		else if (connectionState.queue.Count < ConVar.Server.filerequestqueuelength)
		{
			connectionState.queue.Enqueue(request);
			if (ConVar.Server.filerequestdebug)
			{
				Debug.Log((object)$"[FileRequest] {connection} over budget, request deferred - {UsageString(connectionState)}");
			}
		}
	}

	public static void Cycle()
	{
		bool filerequestdebug = ConVar.Server.filerequestdebug;
		List<Connection> list = Pool.Get<List<Connection>>();
		foreach (KeyValuePair<Connection, ConnectionState> state in States)
		{
			ConnectionState value = state.Value;
			if (!value.connection.active)
			{
				list.Add(value.connection);
				continue;
			}
			RefillBudget(value);
			while (value.queue.Count > 0 && value.byteBudget > 0.0)
			{
				Send(value, value.queue.Dequeue());
			}
			if (value.queue.Count == 0 && value.byteBudget >= (double)ConVar.Server.filerequestbytesburst)
			{
				list.Add(value.connection);
			}
			else if (filerequestdebug)
			{
				Debug.Log((object)$"[FileRequest] {value.connection} - {UsageString(value)}");
			}
		}
		foreach (Connection item in list)
		{
			ConnectionState connectionState = States[item];
			Pool.Free<ConnectionState>(ref connectionState);
			States.Remove(item);
		}
		Pool.FreeUnmanaged<Connection>(ref list);
	}

	public static void OnDisconnected(Connection connection)
	{
		States.Remove(connection);
	}

	private static ConnectionState FindState(Connection connection)
	{
		States.TryGetValue(connection, out var value);
		return value;
	}

	private static void RefillBudget(ConnectionState state)
	{
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		double num = realtimeSinceStartupAsDouble - state.lastRefill;
		state.lastRefill = realtimeSinceStartupAsDouble;
		state.byteBudget = Math.Min(ConVar.Server.filerequestbytesburst, state.byteBudget + num * (double)ConVar.Server.filerequestbytespersecond);
	}

	private static void Send(ConnectionState state, in QueuedRequest request)
	{
		if (request.Entity.IsValid())
		{
			int num = ((request.Kind != RequestKind.EntityImage) ? request.Entity.SendRequestedFile(state.connection, request.ResponseFunction, request.Crc, request.Type, request.Part, request.RespondIfNotFound) : ((request.Entity is ImageStorageEntity imageStorageEntity) ? imageStorageEntity.SendRequestedImage(state.connection) : 0));
			int num2 = num;
			state.byteBudget -= Math.Max(num2, 32768);
			if (ConVar.Server.filerequestdebug)
			{
				Debug.Log((object)$"[FileRequest] {state.connection} sent {NumberExtensions.FormatBytes<int>(num2, false)} - {UsageString(state)}");
			}
		}
	}

	private static string UsageString(ConnectionState state)
	{
		long num = ConVar.Server.filerequestbytesburst;
		long num2 = num - (long)state.byteBudget;
		return $"used {NumberExtensions.FormatBytes<long>(num2, false)} of {NumberExtensions.FormatBytes<long>(num, false)}, queued {state.queue.Count}";
	}
}
