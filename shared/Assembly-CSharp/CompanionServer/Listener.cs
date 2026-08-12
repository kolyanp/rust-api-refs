using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
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
	private struct Message
	{
		public readonly Connection Connection;

		public readonly MemoryBuffer Buffer;

		public Message(Connection connection, MemoryBuffer buffer)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Connection = connection;
			Buffer = buffer;
		}
	}

	private static readonly ByteArrayStream Stream;

	private readonly TokenBucketList<IPAddress> _ipTokenBuckets;

	private readonly BanList<IPAddress> _ipBans;

	private readonly TokenBucketList<ulong> _playerTokenBuckets;

	private readonly TokenBucketList<ulong> _pairingTokenBuckets;

	private readonly Queue<Message> _messageQueue;

	private readonly WebSocketServer _server;

	private readonly Stopwatch _stopwatch;

	private RealTimeSince _lastCleanup;

	private long _nextConnectionId;

	public readonly IPAddress Address;

	public readonly int Port;

	public readonly SubscriberList<PlayerTarget, AppBroadcast> PlayerSubscribers;

	public readonly SubscriberList<EntityTarget, AppBroadcast> EntitySubscribers;

	public readonly SubscriberList<ClanTarget, AppBroadcast> ClanSubscribers;

	public readonly SubscriberList<CameraTarget, AppBroadcast> CameraSubscribers;

	public Listener(IPAddress ipAddress, int port)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		base._002Ector();
		Listener listener = this;
		Address = ipAddress;
		Port = port;
		_ipTokenBuckets = new TokenBucketList<IPAddress>(50.0, 15.0);
		_ipBans = new BanList<IPAddress>();
		_playerTokenBuckets = new TokenBucketList<ulong>(25.0, 3.0);
		_pairingTokenBuckets = new TokenBucketList<ulong>(5.0, 0.1);
		_messageQueue = new Queue<Message>();
		SynchronizationContext syncContext = SynchronizationContext.Current;
		_server = new WebSocketServer($"ws://{Address}:{Port}/", true, 100, App.maxconnections, App.maxconnectionsperip);
		_server.Start((Action<IWebSocketConnection>)delegate(IWebSocketConnection socket)
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			IPAddress clientIpAddress = socket.ConnectionInfo.ClientIpAddress;
			if (listener._ipBans.IsBanned(clientIpAddress))
			{
				socket.Close();
			}
			else
			{
				long connectionId = Interlocked.Increment(ref listener._nextConnectionId);
				Connection conn = new Connection(connectionId, listener, socket);
				socket.OnClose = delegate
				{
					syncContext.Post(delegate(object c)
					{
						((Connection)c).OnClose();
					}, conn);
				};
				socket.OnBinary = new BinaryDataHandler(conn.OnMessage);
				socket.OnError = delegate(Exception ex)
				{
					if (App.logexceptions && !(ex is WebSocketException))
					{
						Debug.LogError((object)ex);
					}
				};
			}
		});
		_stopwatch = new Stopwatch();
		PlayerSubscribers = new SubscriberList<PlayerTarget, AppBroadcast>(this);
		EntitySubscribers = new SubscriberList<EntityTarget, AppBroadcast>(this);
		ClanSubscribers = new SubscriberList<ClanTarget, AppBroadcast>(this);
		CameraSubscribers = new SubscriberList<CameraTarget, AppBroadcast>(this, 30.0);
	}

	public void Dispose()
	{
		WebSocketServer server = _server;
		if (server != null)
		{
			server.Dispose();
		}
	}

	internal void Enqueue(Connection connection, MemoryBuffer data)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		lock (_messageQueue)
		{
			if (!App.update || _messageQueue.Count >= App.queuelimit)
			{
				((MemoryBuffer)(ref data)).Dispose();
				return;
			}
			Message item = new Message(connection, data);
			_messageQueue.Enqueue(item);
		}
	}

	public bool Update()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!App.update)
		{
			return false;
		}
		bool result = false;
		using (TimeWarning.New("CompanionServer.MessageQueue"))
		{
			lock (_messageQueue)
			{
				_stopwatch.Restart();
				while (_messageQueue.Count > 0 && _stopwatch.Elapsed.TotalMilliseconds < 5.0)
				{
					Message message = _messageQueue.Dequeue();
					Dispatch(message);
					result = true;
				}
			}
		}
		if (RealTimeSince.op_Implicit(_lastCleanup) >= 3f)
		{
			_lastCleanup = RealTimeSince.op_Implicit(0f);
			_ipTokenBuckets.Cleanup();
			_ipBans.Cleanup();
			_playerTokenBuckets.Cleanup();
			_pairingTokenBuckets.Cleanup();
		}
		return result;
	}

	private void Dispatch(Message message)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MemoryBuffer buffer = message.Buffer;
		AppRequest val;
		try
		{
			ByteArrayStream stream = Stream;
			MemoryBuffer buffer2 = message.Buffer;
			byte[] data = ((MemoryBuffer)(ref buffer2)).Data;
			buffer2 = message.Buffer;
			stream.SetData(data, 0, ((MemoryBuffer)(ref buffer2)).Length);
			val = Pool.Get<AppRequest>();
			ProtoStreamExtensions.ReadFromStream((IProto)(object)val, (Stream)(object)Stream, false, 1048576);
		}
		catch
		{
			DebugEx.LogWarning($"Malformed companion packet from {message.Connection.Address}", (StackTraceLogType)0);
			message.Connection.Close();
			throw;
		}
		finally
		{
			((MemoryBuffer)(ref buffer)).Dispose();
		}
		if (!Handle<AppEmpty, Info>(val.getInfo, message.Connection, val) && !Handle<AppEmpty, CompanionServer.Handlers.Time>(val.getTime, message.Connection, val) && !Handle<AppEmpty, Map>(val.getMap, message.Connection, val) && !Handle<AppEmpty, TeamInfo>(val.getTeamInfo, message.Connection, val) && !Handle<AppEmpty, TeamChat>(val.getTeamChat, message.Connection, val) && !Handle<AppSendMessage, SendTeamChat>(val.sendTeamMessage, message.Connection, val) && !Handle<AppEmpty, EntityInfo>(val.getEntityInfo, message.Connection, val) && !Handle<AppSetEntityValue, SetEntityValue>(val.setEntityValue, message.Connection, val) && !Handle<AppEmpty, CheckSubscription>(val.checkSubscription, message.Connection, val) && !Handle<AppFlag, SetSubscription>(val.setSubscription, message.Connection, val) && !Handle<AppEmpty, MapMarkers>(val.getMapMarkers, message.Connection, val) && !Handle<AppPromoteToLeader, PromoteToLeader>(val.promoteToLeader, message.Connection, val) && !Handle<AppEmpty, ClanInfo>(val.getClanInfo, message.Connection, val) && !Handle<AppEmpty, ClanChat>(val.getClanChat, message.Connection, val) && !Handle<AppSendMessage, SetClanMotd>(val.setClanMotd, message.Connection, val) && !Handle<AppSendMessage, SendClanChat>(val.sendClanMessage, message.Connection, val) && !Handle<AppGetNexusAuth, NexusAuth>(val.getNexusAuth, message.Connection, val) && !Handle<AppCameraSubscribe, CameraSubscribe>(val.cameraSubscribe, message.Connection, val) && !Handle<AppEmpty, CameraUnsubscribe>(val.cameraUnsubscribe, message.Connection, val) && !Handle<AppCameraInput, CameraInput>(val.cameraInput, message.Connection, val))
		{
			AppResponse val2 = Pool.Get<AppResponse>();
			val2.seq = val.seq;
			val2.error = Pool.Get<AppError>();
			val2.error.error = "unhandled";
			message.Connection.Send(val2);
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
				val.Execute();
				break;
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

	public void BroadcastTo(List<Connection> targets, AppBroadcast broadcast)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		MemoryBuffer broadcastBuffer = GetBroadcastBuffer(broadcast);
		foreach (Connection target in targets)
		{
			target.SendRaw(((MemoryBuffer)(ref broadcastBuffer)).DontDispose());
		}
		((MemoryBuffer)(ref broadcastBuffer)).Dispose();
	}

	private static MemoryBuffer GetBroadcastBuffer(AppBroadcast broadcast)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		BufferStream val = Pool.Get<BufferStream>().Initialize();
		try
		{
			AppMessage val2 = Pool.Get<AppMessage>();
			try
			{
				val2.broadcast = broadcast;
				val2.WriteToStream(val);
				MemoryBuffer val3 = new MemoryBuffer(val.Length);
				val.GetBuffer().CopyTo(((MemoryBuffer)(ref val3)).Data, 0);
				return ((MemoryBuffer)(ref val3)).Slice(val.Length);
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
		//IL_000a: Expected O, but got Unknown
		Stream = new ByteArrayStream();
	}
}
