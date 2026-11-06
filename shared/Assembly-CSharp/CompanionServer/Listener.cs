using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace CompanionServer;

public class Listener : IDisposable, IBroadcastSender<AppBroadcast>
{
	private struct Message(Connection connection, MemoryBuffer buffer)
	{
		public readonly Connection Connection = connection;

		public readonly MemoryBuffer Buffer = buffer;
	}

	private static readonly ByteArrayStream Stream = new ByteArrayStream();

	private const int MaxQueuedMessages = 100;

	private const int MaxQueuedMessagesPerPlayer = 25;

	private const int MaxDroppedRequests = 1000;

	private readonly TokenBucketList<ulong> _playerTokenBuckets;

	private readonly TokenBucketList<ulong> _pairingTokenBuckets;

	private readonly Queue<Message> _messageQueue;

	private readonly WebSocketServer _server;

	private readonly Stopwatch _stopwatch;

	private readonly SynchronizationContext _syncContext;

	private readonly HashSet<BackhaulConnection> _backhauls = new HashSet<BackhaulConnection>();

	private readonly Dictionary<ulong, int> _playerChannelCounts = new Dictionary<ulong, int>();

	private readonly Dictionary<ulong, int> _queuedMessageCounts = new Dictionary<ulong, int>();

	private readonly Queue<(Connection Connection, uint Seq)> _droppedRequests = new Queue<(Connection, uint)>();

	private readonly Dictionary<ulong, int> _droppedRequestCounts = new Dictionary<ulong, int>();

	private RealTimeSince _lastCleanup;

	private long _nextConnectionId;

	private bool _disposed;

	public readonly IPAddress Address;

	public readonly int Port;

	public readonly SubscriberList<PlayerTarget, AppBroadcast> PlayerSubscribers;

	public readonly SubscriberList<EntityTarget, AppBroadcast> EntitySubscribers;

	public readonly SubscriberList<ClanTarget, AppBroadcast> ClanSubscribers;

	public readonly SubscriberList<CameraTarget, AppBroadcast> CameraSubscribers;

	public Listener(IPAddress ipAddress, int port)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected Obj, but got Unknown
		Address = ipAddress;
		Port = port;
		_playerTokenBuckets = new TokenBucketList<ulong>(25.0, 3.0);
		_pairingTokenBuckets = new TokenBucketList<ulong>(5.0, 0.1);
		_messageQueue = new Queue<Message>();
		_syncContext = SynchronizationContext.Current;
		_server = new WebSocketServer($"ws://{Address}:{Port}/", true, 100, FleckLimit(App.maxconnections), FleckLimit(App.maxconnectionsperip));
		_server.RestartAfterListenError = true;
		_server.MaxMessageSize = 2097152;
		_server.Start((Action<IWebSocketConnection>)((IWebSocketConnection socket) =>
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected Obj, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Expected Obj, but got Unknown
			string path = socket.ConnectionInfo.Path;
			if (path == null || !path.StartsWith("/backhaul/", StringComparison.Ordinal) || !IsValidBackhaulSecret(path.Substring("/backhaul/".Length)))
			{
				socket.Close();
			}
			else
			{
				socket.OnPing = (Span<byte> data) =>
				{
					//IL_0009: Unknown result type (might be due to invalid IL or missing references)
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_002a: Unknown result type (might be due to invalid IL or missing references)
					MemoryBuffer val = new MemoryBuffer(data.Length);
					data.CopyTo(MemoryBuffer.op_Implicit(val));
					socket.SendPong(val.Slice(data.Length));
				};
				BackhaulConnection backhaul = new BackhaulConnection(this, socket, _syncContext);
				lock (_backhauls)
				{
					_backhauls.Add(backhaul);
				}
				socket.OnBinary = backhaul.OnMessage;
				socket.OnClose = () =>
				{
					lock (_backhauls)
					{
						_backhauls.Remove(backhaul);
					}
					_syncContext.Post((object c) =>
					{
						((BackhaulConnection)c).OnBackhaulClosed();
					}, backhaul);
				};
				socket.OnError = LogSocketError;
			}
		}));
		_stopwatch = new Stopwatch();
		PlayerSubscribers = new SubscriberList<PlayerTarget, AppBroadcast>(this);
		EntitySubscribers = new SubscriberList<EntityTarget, AppBroadcast>(this);
		ClanSubscribers = new SubscriberList<ClanTarget, AppBroadcast>(this);
		CameraSubscribers = new SubscriberList<CameraTarget, AppBroadcast>(this, 30.0);
	}

	internal long NextConnectionId()
	{
		return Interlocked.Increment(ref _nextConnectionId);
	}

	public void GetBackhauls(List<BackhaulConnection> results)
	{
		lock (_backhauls)
		{
			results.AddRange(_backhauls);
		}
	}

	internal bool TryAddPlayerChannel(ulong steamId)
	{
		lock (_playerChannelCounts)
		{
			int maxconnectionsperip = App.maxconnectionsperip;
			_playerChannelCounts.TryGetValue(steamId, out var value);
			if (maxconnectionsperip >= 0 && value >= maxconnectionsperip)
			{
				return false;
			}
			_playerChannelCounts[steamId] = value + 1;
			return true;
		}
	}

	internal void RemovePlayerChannel(ulong steamId)
	{
		lock (_playerChannelCounts)
		{
			if (_playerChannelCounts.TryGetValue(steamId, out var value))
			{
				if (value <= 1)
				{
					_playerChannelCounts.Remove(steamId);
				}
				else
				{
					_playerChannelCounts[steamId] = value - 1;
				}
			}
		}
	}

	private static int FleckLimit(int limit)
	{
		if (limit >= 0)
		{
			return limit;
		}
		return -1;
	}

	private static bool IsValidBackhaulSecret(string provided)
	{
		string secret = Server.Secret;
		if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(provided))
		{
			return false;
		}
		if (provided.Length != secret.Length)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < provided.Length; i++)
		{
			num |= provided[i] ^ secret[i];
		}
		return num == 0;
	}

	private static void LogSocketError(Exception ex)
	{
		if (App.logexceptions && !(ex is WebSocketException) && !(ex is IOException))
		{
			Debug.LogError((object)ex);
		}
	}

	public void Dispose()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		WebSocketServer server = _server;
		if (server != null)
		{
			server.Dispose();
		}
		List<BackhaulConnection> list = Pool.Get<List<BackhaulConnection>>();
		GetBackhauls(list);
		foreach (BackhaulConnection item in list)
		{
			item.Close();
		}
		Pool.FreeUnmanaged<BackhaulConnection>(ref list);
		lock (_messageQueue)
		{
			_disposed = true;
			while (_messageQueue.Count > 0)
			{
				MemoryBuffer buffer = _messageQueue.Dequeue().Buffer;
				buffer.Dispose();
			}
			_queuedMessageCounts.Clear();
			_droppedRequests.Clear();
			_droppedRequestCounts.Clear();
		}
	}

	internal void Enqueue(Connection connection, MemoryBuffer data)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		lock (_messageQueue)
		{
			if (_disposed)
			{
				data.Dispose();
				return;
			}
			_queuedMessageCounts.TryGetValue(connection.SteamId, out var value);
			if (_messageQueue.Count >= 100 || value >= 25)
			{
				_droppedRequestCounts.TryGetValue(connection.SteamId, out var value2);
				if (value2 < 25 && _droppedRequests.Count < 1000)
				{
					_droppedRequestCounts[connection.SteamId] = value2 + 1;
					_droppedRequests.Enqueue((connection, ReadSeq(MemoryBuffer.op_Implicit(data))));
				}
				data.Dispose();
			}
			else
			{
				_queuedMessageCounts[connection.SteamId] = value + 1;
				Message item = new Message(connection, data);
				_messageQueue.Enqueue(item);
			}
		}
	}

	private static uint ReadSeq(Span<byte> request)
	{
		if (request.Length == 0 || request[0] != 8)
		{
			return 0u;
		}
		uint num = 0u;
		int num2 = 1;
		int num3 = 0;
		while (num2 < request.Length && num3 < 32)
		{
			num |= (uint)((request[num2] & 0x7F) << num3);
			if ((request[num2] & 0x80) == 0)
			{
				return num;
			}
			num2++;
			num3 += 7;
		}
		return 0u;
	}

	private bool TryDequeueMessage(out Message message)
	{
		lock (_messageQueue)
		{
			if (_messageQueue.Count == 0)
			{
				message = default;
				return false;
			}
			message = _messageQueue.Dequeue();
			ulong steamId = message.Connection.SteamId;
			int num = _queuedMessageCounts[steamId];
			if (num <= 1)
			{
				_queuedMessageCounts.Remove(steamId);
			}
			else
			{
				_queuedMessageCounts[steamId] = num - 1;
			}
			return true;
		}
	}

	private bool ReplyToDroppedRequests()
	{
		List<(Connection, uint)> list;
		lock (_messageQueue)
		{
			if (_droppedRequests.Count == 0)
			{
				return false;
			}
			list = Pool.Get<List<(Connection, uint)>>();
			list.AddRange(_droppedRequests);
			_droppedRequests.Clear();
			_droppedRequestCounts.Clear();
		}
		foreach (var (connection, seq) in list)
		{
			SendError(connection, seq, ValidationResult.RateLimit.ToErrorCode());
		}
		Pool.FreeUnmanaged<(Connection, uint)>(ref list);
		return true;
	}

	internal static void SendError(IConnection connection, uint seq, string error)
	{
		AppResponse val = Pool.Get<AppResponse>();
		val.seq = seq;
		val.error = Pool.Get<AppError>();
		val.error.error = error;
		connection.Send(val);
	}

	public bool Update()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		using (TimeWarning.New("CompanionServer.MessageQueue"))
		{
			_stopwatch.Restart();
			Message message;
			while (_stopwatch.Elapsed.TotalMilliseconds < 5.0 && TryDequeueMessage(out message))
			{
				Dispatch(message);
				flag = true;
			}
			flag |= ReplyToDroppedRequests();
		}
		if (RealTimeSince.op_Implicit(_lastCleanup) >= 3f)
		{
			_lastCleanup = RealTimeSince.op_Implicit(0f);
			_playerTokenBuckets.Cleanup();
			_pairingTokenBuckets.Cleanup();
		}
		return flag;
	}

	private void Dispatch(Message message)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		MemoryBuffer buffer = message.Buffer;
		if (message.Connection.IsClosed)
		{
			buffer.Dispose();
			return;
		}
		AppRequest val;
		try
		{
			ByteArrayStream stream = Stream;
			MemoryBuffer buffer2 = message.Buffer;
			byte[] data = buffer2.Data;
			buffer2 = message.Buffer;
			stream.SetData(data, 0, buffer2.Length);
			val = Pool.Get<AppRequest>();
			ProtoStreamExtensions.ReadFromStream((IProto)(object)val, (Stream)(object)Stream, false, 1048576);
		}
		catch
		{
			DebugEx.LogWarning($"Malformed companion packet from connectionID={message.Connection.ConnectionId} steamID={message.Connection.SteamId}", (StackTraceLogType)0);
			message.Connection.Close();
			return;
		}
		finally
		{
			buffer.Dispose();
		}
		if (!Handle<AppEmpty, Info>(val.getInfo, message.Connection, val) && !Handle<AppEmpty, CompanionServer.Handlers.Time>(val.getTime, message.Connection, val) && !Handle<AppEmpty, Map>(val.getMap, message.Connection, val) && !Handle<AppEmpty, TeamInfo>(val.getTeamInfo, message.Connection, val) && !Handle<AppEmpty, TeamChat>(val.getTeamChat, message.Connection, val) && !Handle<AppSendMessage, SendTeamChat>(val.sendTeamMessage, message.Connection, val) && !Handle<AppEmpty, EntityInfo>(val.getEntityInfo, message.Connection, val) && !Handle<AppSetEntityValue, SetEntityValue>(val.setEntityValue, message.Connection, val) && !Handle<AppEmpty, CheckSubscription>(val.checkSubscription, message.Connection, val) && !Handle<AppFlag, SetSubscription>(val.setSubscription, message.Connection, val) && !Handle<AppEmpty, MapMarkers>(val.getMapMarkers, message.Connection, val) && !Handle<AppPromoteToLeader, PromoteToLeader>(val.promoteToLeader, message.Connection, val) && !Handle<AppEmpty, ClanInfo>(val.getClanInfo, message.Connection, val) && !Handle<AppEmpty, ClanChat>(val.getClanChat, message.Connection, val) && !Handle<AppSendMessage, SetClanMotd>(val.setClanMotd, message.Connection, val) && !Handle<AppSendMessage, SendClanChat>(val.sendClanMessage, message.Connection, val) && !Handle<AppGetNexusAuth, NexusAuth>(val.getNexusAuth, message.Connection, val) && !Handle<AppCameraSubscribe, CameraSubscribe>(val.cameraSubscribe, message.Connection, val) && !Handle<AppEmpty, CameraUnsubscribe>(val.cameraUnsubscribe, message.Connection, val) && !Handle<AppCameraInput, CameraInput>(val.cameraInput, message.Connection, val) && !Handle<AppTeamKick, TeamKick>(val.kickFromTeam, message.Connection, val))
		{
			SendError(message.Connection, val.seq, "unhandled");
			val.Dispose();
		}
	}

	private bool Handle<TProto, THandler>(TProto protocol, Connection connection, AppRequest request) where TProto : class, IProto where THandler : BaseHandler<TProto>, new()
	{
		if (protocol == null)
		{
			return false;
		}
		THandler val = Pool.Get<THandler>();
		val.Initialize(_playerTokenBuckets, connection, request, protocol);
		try
		{
			ValidationResult validationResult = val.Validate();
			switch (validationResult)
			{
			case ValidationResult.Rejected:
				connection.Close();
				break;
			default:
				val.SendError(validationResult.ToErrorCode());
				break;
			case ValidationResult.Success:
			{
				ValueTask execution = val.Execute();
				if (!execution.IsCompleted)
				{
					AwaitExecution(execution, val);
					return true;
				}
				execution.GetAwaiter().GetResult();
				break;
			}
			}
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"AppRequest threw an exception: {arg}");
			val.SendError("server_error");
		}
		Pool.Free<THandler>(ref val);
		return true;
	}

	private static async void AwaitExecution<THandler>(ValueTask execution, THandler handler) where THandler : class, IHandler, new()
	{
		try
		{
			await execution;
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"AppRequest threw an exception: {arg}");
			try
			{
				handler.SendError("server_error");
			}
			catch (Exception arg2)
			{
				Debug.LogError((object)$"Failed to send the error response: {arg2}");
			}
		}
		Pool.Free<THandler>(ref handler);
	}

	public void BroadcastTo(List<Connection> targets, AppBroadcast broadcast)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetBroadcastBuffer(broadcast, out var buffer))
		{
			return;
		}
		foreach (Connection target in targets)
		{
			target.SendRaw(buffer.DontDispose());
		}
		buffer.Dispose();
	}

	private static bool TryGetBroadcastBuffer(AppBroadcast broadcast, out MemoryBuffer buffer)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		BufferStream val = Pool.Get<BufferStream>().Initialize();
		try
		{
			AppMessage val2 = Pool.Get<AppMessage>();
			try
			{
				val2.broadcast = broadcast;
				val2.WriteToStream(val);
				if (val.Length > 2097147)
				{
					Debug.LogError((object)$"Rust+ broadcast is too large to send ({val.Length} bytes), dropping it");
					buffer = default;
					return false;
				}
				buffer = new MemoryBuffer(val.Length);
				val.GetBuffer().CopyTo(buffer.Data, 0);
				buffer = buffer.Slice(val.Length);
				return true;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool CanSendPairingNotification(ulong playerId)
	{
		return _pairingTokenBuckets.Get(playerId).TryTake(1.0);
	}

	static Listener()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected Obj, but got Unknown
	}
}
