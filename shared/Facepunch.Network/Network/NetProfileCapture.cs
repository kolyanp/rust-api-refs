using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Network;

public static class NetProfileCapture
{
	public static NetProfileSession Session;

	public static int EventCapacity = 262144;

	public static int BucketCapacity = 6000;

	public static bool SampleTransportStats = true;

	public static Func<ulong, bool, uint> PrefabResolver;

	public static Func<uint, string> NameResolver;

	private static int lastTickFrame = -1;

	private static double lastClientSampleAt = double.MinValue;

	private static double lastServerSampleAt = double.MinValue;

	public static bool IsCapturing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			NetProfileSession session = Session;
			if (session != null)
			{
				return session.Active;
			}
			return false;
		}
	}

	public static void Start(float expectedDurationSeconds = 600f)
	{
		int bucketCapacity = Mathf.Clamp(Math.Max(BucketCapacity, (int)((double)expectedDurationSeconds / 0.1) + 100), 600, 12000);
		int eventCapacity = Mathf.Clamp(EventCapacity, 4096, 4194304);
		lastTickFrame = -1;
		lastClientSampleAt = double.MinValue;
		lastServerSampleAt = double.MinValue;
		NetProfileSession netProfileSession = new NetProfileSession(bucketCapacity, eventCapacity);
		netProfileSession.Active = true;
		Volatile.Write(ref Session, netProfileSession);
	}

	public static void Stop()
	{
		NetProfileSession session = Session;
		if (session != null)
		{
			session.StoppedAt = session.NowSeconds;
			session.Active = false;
		}
	}

	public static NetProfileSnapshot CreateSnapshot()
	{
		return NetProfileSnapshot.FromSession(Session, NameResolver);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnSend(BaseNetwork peer, NetWrite write, bool immediate = false)
	{
		NetProfileSession session = Session;
		if (session != null && session.Active)
		{
			CaptureSend(session, peer, write, immediate);
		}
	}

	private static void CaptureSend(NetProfileSession s, BaseNetwork peer, NetWrite write, bool immediate)
	{
		int num = write.PeekPacketID() - 140;
		if (num < 0 || num >= 29)
		{
			Interlocked.Increment(ref s.EventsSkipped);
			return;
		}
		bool flag = peer is Server;
		int bytes = (int)write.Length;
		int count = write.connections.Count;
		double nowSeconds = s.NowSeconds;
		NetProfileEvent evt = new NetProfileEvent
		{
			Time = nowSeconds,
			Bytes = bytes,
			Fanout = (ushort)Math.Min(count, 65535),
			Type = (byte)num,
			Flags = (NetProfileEventFlags)(1 | (flag ? 2 : 0) | (immediate ? 8 : 0))
		};
		if (count == 1)
		{
			Connection connection = write.connections[0];
			evt.ConnectionId = ((connection.userid != 0L) ? connection.userid : connection.guid);
		}
		if (write.profilerEntityId != 0L || write.profilerPrefabId != 0 || write.profilerInfoId != 0)
		{
			evt.EntityId = write.profilerEntityId;
			evt.PrefabId = write.profilerPrefabId;
			evt.Flags |= NetProfileEventFlags.Annotated;
			if (write.profilerInfoId != 0)
			{
				evt.Aux = write.profilerInfoId;
				evt.Flags |= NetProfileEventFlags.AuxIsInfoId;
			}
			else if (write.profilerAux != 0)
			{
				evt.Aux = write.profilerAux;
				if (write.profilerAuxIsStringId)
				{
					evt.Flags |= NetProfileEventFlags.AuxIsStringId;
				}
			}
		}
		else
		{
			var (buffer, length) = write.GetBuffer();
			ParseHeader(buffer, length, (Message.Type)num, ref evt);
		}
		write.profilerEntityId = 0uL;
		write.profilerPrefabId = 0u;
		write.profilerAux = 0u;
		write.profilerInfoId = 0;
		write.profilerAuxIsStringId = false;
		ResolvePrefab(s, ref evt, flag);
		Commit(s, ref evt, nowSeconds, flag, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void OnReceive(BaseNetwork peer, Message.Type type, NetRead read)
	{
		NetProfileSession session = Session;
		if (session != null && session.Active)
		{
			CaptureReceive(session, peer, type, read);
		}
	}

	private static void CaptureReceive(NetProfileSession s, BaseNetwork peer, Message.Type type, NetRead read)
	{
		if ((int)type <= 0 || (int)type >= 29)
		{
			Interlocked.Increment(ref s.EventsSkipped);
			return;
		}
		bool flag = peer is Server;
		double nowSeconds = s.NowSeconds;
		NetProfileEvent evt = new NetProfileEvent
		{
			Time = nowSeconds,
			Bytes = (int)read.Length,
			Fanout = 1,
			Type = (byte)type,
			Flags = (NetProfileEventFlags)((flag ? 2 : 0) | ((peer is DemoClient) ? 32 : 0)),
			ConnectionId = ((read.connection != null && read.connection.userid != 0L) ? read.connection.userid : read.guid)
		};
		var (buffer, length) = read.GetBuffer();
		ParseHeader(buffer, length, type, ref evt);
		ResolvePrefab(s, ref evt, flag);
		long lastInboundEventIndex = Commit(s, ref evt, nowSeconds, flag, (int)type);
		if (Environment.CurrentManagedThreadId == s.MainThreadId)
		{
			s.LastInboundEventIndex = lastInboundEventIndex;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Tick(BaseNetwork peer)
	{
		NetProfileSession session = Session;
		if (session != null && session.Active)
		{
			CaptureTick(session, peer);
		}
	}

	private static void CaptureTick(NetProfileSession s, BaseNetwork peer)
	{
		double nowSeconds = s.NowSeconds;
		if (Time.frameCount != lastTickFrame)
		{
			lastTickFrame = Time.frameCount;
			long num = (long)(nowSeconds / 0.1) + 64;
			long num2 = Volatile.Read(in s.ClearedThroughBucket);
			if (num > num2)
			{
				for (long num3 = Math.Max(num2 + 1, num - s.BucketCapacity + 1); num3 <= num; num3++)
				{
					Array.Clear(s.Buckets, (int)(num3 % s.BucketCapacity) * 290, 290);
				}
				Volatile.Write(ref s.ClearedThroughBucket, num);
			}
			if (s.FrameMarkers.Count < 65536)
			{
				s.FrameMarkers.Add(nowSeconds);
			}
		}
		bool flag = peer is Server;
		ref double reference = ref flag ? ref lastServerSampleAt : ref lastClientSampleAt;
		if (!(nowSeconds - reference < 1.0))
		{
			reference = nowSeconds;
			SampleConnections(s, peer, flag, nowSeconds);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Annotate(NetWrite write, ulong entityId, uint prefabId, uint aux = 0u, bool auxIsStringId = false)
	{
		if (IsCapturing)
		{
			write.profilerEntityId = entityId;
			write.profilerPrefabId = prefabId;
			write.profilerAux = aux;
			write.profilerAuxIsStringId = auxIsStringId;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Annotate(NetWrite write, string info)
	{
		NetProfileSession session = Session;
		if (session != null && session.Active)
		{
			write.profilerInfoId = session.Info.Intern(info);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AnnotateLastReceive(ulong entityId, uint prefabId, string info = null)
	{
		NetProfileSession session = Session;
		if (session != null && session.Active)
		{
			CaptureLastReceiveAnnotation(session, entityId, prefabId, info);
		}
	}

	private static void CaptureLastReceiveAnnotation(NetProfileSession s, ulong entityId, uint prefabId, string info)
	{
		long lastInboundEventIndex = s.LastInboundEventIndex;
		if (lastInboundEventIndex < 0)
		{
			return;
		}
		ushort num = (ushort)((info != null) ? s.Info.Intern(info) : 0);
		lock (s.EventLock)
		{
			if (lastInboundEventIndex >= s.EventWriteIndex - s.Events.Length)
			{
				ref NetProfileEvent reference = ref s.Events[lastInboundEventIndex % s.Events.Length];
				reference.EntityId = entityId;
				if (prefabId != 0)
				{
					reference.PrefabId = prefabId;
				}
				reference.Flags |= NetProfileEventFlags.Annotated;
				if (num != 0 && reference.Aux == 0)
				{
					reference.Aux = num;
					reference.Flags |= NetProfileEventFlags.AuxIsInfoId;
				}
			}
		}
	}

	private static void ParseHeader(byte[] buffer, int length, Message.Type type, ref NetProfileEvent evt)
	{
		switch (type)
		{
		case Message.Type.EntityDestroy:
		case Message.Type.EntityPosition:
		case Message.Type.VoiceData:
		case Message.Type.EntityFlags:
			if (length >= 9)
			{
				evt.EntityId = BitConverter.ToUInt64(buffer, 1);
			}
			break;
		case Message.Type.RPCMessage:
			if (length >= 13)
			{
				evt.EntityId = BitConverter.ToUInt64(buffer, 1);
				evt.RpcId = BitConverter.ToUInt32(buffer, 9);
			}
			break;
		case Message.Type.GroupChange:
			if (length >= 13)
			{
				evt.EntityId = BitConverter.ToUInt64(buffer, 1);
				evt.Aux = BitConverter.ToUInt32(buffer, 9);
			}
			break;
		case Message.Type.GroupDestroy:
		case Message.Type.GroupEnter:
		case Message.Type.GroupLeave:
			if (length >= 5)
			{
				evt.Aux = BitConverter.ToUInt32(buffer, 1);
			}
			break;
		case Message.Type.SyncVar:
			if (length >= 10)
			{
				evt.EntityId = BitConverter.ToUInt64(buffer, 1);
				evt.Aux = buffer[9];
			}
			break;
		case Message.Type.PackedSyncVar:
			if (length >= 13)
			{
				evt.EntityId = BitConverter.ToUInt64(buffer, 1);
				evt.Aux = BitConverter.ToUInt32(buffer, 9);
			}
			break;
		}
	}

	private static void ResolvePrefab(NetProfileSession s, ref NetProfileEvent evt, bool server)
	{
		if (evt.PrefabId == 0 && evt.EntityId != 0L)
		{
			Func<ulong, bool, uint> prefabResolver = PrefabResolver;
			if (prefabResolver != null && Environment.CurrentManagedThreadId == s.MainThreadId)
			{
				evt.PrefabId = prefabResolver(evt.EntityId, server);
			}
		}
	}

	private static long Commit(NetProfileSession s, ref NetProfileEvent evt, double time, bool server, int type)
	{
		long num = (long)(time / 0.1);
		if (num <= Volatile.Read(in s.ClearedThroughBucket))
		{
			int num2 = NetProfileSession.BucketOffset(num, s.BucketCapacity, server, type, 0);
			if (evt.IsOutbound)
			{
				Interlocked.Increment(ref s.Buckets[num2 + 2]);
				Interlocked.Add(ref s.Buckets[num2 + 3], evt.Bytes);
				Interlocked.Add(ref s.Buckets[num2 + 4], evt.Bytes * evt.Fanout);
			}
			else
			{
				Interlocked.Increment(ref s.Buckets[num2]);
				Interlocked.Add(ref s.Buckets[num2 + 1], evt.Bytes);
			}
		}
		else
		{
			Interlocked.Increment(ref s.BucketAddsDropped);
		}
		int num3 = NetProfileSession.CellOffset(server, type, 0);
		if (evt.IsOutbound)
		{
			Interlocked.Increment(ref s.Totals[num3 + 2]);
			Interlocked.Add(ref s.Totals[num3 + 3], evt.Bytes);
			Interlocked.Add(ref s.Totals[num3 + 4], (long)evt.Bytes * (long)evt.Fanout);
		}
		else
		{
			Interlocked.Increment(ref s.Totals[num3]);
			Interlocked.Add(ref s.Totals[num3 + 1], evt.Bytes);
		}
		lock (s.EventLock)
		{
			s.RealmFlags |= ((!server) ? 1 : 2);
			long num4 = s.EventWriteIndex++;
			s.Events[num4 % s.Events.Length] = evt;
			return num4;
		}
	}

	private static void SampleConnections(NetProfileSession s, BaseNetwork peer, bool server, double now)
	{
		long num = 0L;
		long num2 = 0L;
		bool flag = false;
		if (peer is Server { connections: var connections })
		{
			for (int i = 0; i < connections.Count; i++)
			{
				Connection connection = connections[i];
				bool flag2 = false;
				for (int j = 0; j < s.Connections.Count; j++)
				{
					if (s.Connections[j].UserId == connection.userid && s.Connections[j].Guid == connection.guid)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					s.Connections.Add(new NetProfileConnection
					{
						UserId = connection.userid,
						Guid = connection.guid,
						Username = connection.username
					});
				}
				if (SampleTransportStats)
				{
					num += (long)peer.GetStat(connection, BaseNetwork.StatTypeLong.BytesReceived);
					num2 += (long)peer.GetStat(connection, BaseNetwork.StatTypeLong.BytesSent);
					flag = true;
				}
			}
		}
		else if (peer is Client { Connection: not null } client && SampleTransportStats)
		{
			num += (long)peer.GetStat(client.Connection, BaseNetwork.StatTypeLong.BytesReceived);
			num2 += (long)peer.GetStat(client.Connection, BaseNetwork.StatTypeLong.BytesSent);
			flag = true;
		}
		if (flag)
		{
			s.TransportSamples.Add(new NetProfileTransportSample
			{
				Time = now,
				Realm = (byte)(server ? 1 : 0),
				BytesIn = num,
				BytesOut = num2
			});
		}
	}
}
