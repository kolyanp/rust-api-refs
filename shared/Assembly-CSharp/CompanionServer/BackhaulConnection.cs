using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using ConVar;
using Fleck;

namespace CompanionServer;

public sealed class BackhaulConnection
{
	private readonly Listener _listener;

	private readonly IWebSocketConnection _connection;

	private readonly SynchronizationContext _syncContext;

	private readonly IPAddress _address;

	private readonly ConcurrentDictionary<uint, Connection> _channels = new ConcurrentDictionary<uint, Connection>();

	public bool IsAvailable
	{
		get
		{
			if (_connection != null)
			{
				return _connection.IsAvailable;
			}
			return false;
		}
	}

	public BackhaulConnection(Listener listener, IWebSocketConnection connection, SynchronizationContext syncContext)
	{
		_listener = listener;
		_connection = connection;
		_syncContext = syncContext;
		_address = connection.ConnectionInfo.ClientIpAddress;
	}

	public void OnMessage(Span<byte> data)
	{
		if (App.update && App.queuelimit > 0 && data.Length >= 5)
		{
			byte b = data[0];
			uint channelId = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(1, 4));
			Span<byte> payload = data.Slice(5);
			switch (b)
			{
			case 1:
				HandleOpen(channelId, payload);
				break;
			case 0:
				HandleData(channelId, payload);
				break;
			case 2:
				HandleClose(channelId);
				break;
			}
		}
	}

	private void HandleOpen(uint channelId, Span<byte> payload)
	{
		if (payload.Length >= 8)
		{
			ulong channelSteamId = BinaryPrimitives.ReadUInt64LittleEndian(payload.Slice(0, 8));
			Connection value = new Connection(_listener.NextConnectionId(), _listener, new ChannelTransport(this, channelId, _address), channelSteamId);
			_channels[channelId] = value;
		}
	}

	private void HandleData(uint channelId, Span<byte> payload)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (payload.Length <= App.maxmessagesize && _channels.TryGetValue(channelId, out var value))
		{
			MemoryBuffer val = default(MemoryBuffer);
			((MemoryBuffer)(ref val))._002Ector(payload.Length);
			payload.CopyTo(MemoryBuffer.op_Implicit(val));
			_listener.Enqueue(value, ((MemoryBuffer)(ref val)).Slice(payload.Length));
		}
	}

	private void HandleClose(uint channelId)
	{
		if (_channels.TryRemove(channelId, out var value))
		{
			PostClose(value);
		}
	}

	public void OnBackhaulClosed()
	{
		foreach (KeyValuePair<uint, Connection> channel in _channels)
		{
			if (_channels.TryRemove(channel.Key, out var value))
			{
				value.OnClose();
			}
		}
	}

	internal void SendData(uint channelId, MemoryBuffer payload)
	{
		try
		{
			SendFramed(0, channelId, ((MemoryBuffer)(ref payload)).Data, ((MemoryBuffer)(ref payload)).Length);
		}
		finally
		{
			((MemoryBuffer)(ref payload)).Dispose();
		}
	}

	internal void CloseChannelLocal(uint channelId)
	{
		if (_channels.TryRemove(channelId, out var value))
		{
			SendFramed(2, channelId, null, 0);
			PostClose(value);
		}
	}

	private void SendFramed(byte opcode, uint channelId, byte[] payload, int payloadLength)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (_connection != null && _connection.IsAvailable)
		{
			int num = 5 + payloadLength;
			MemoryBuffer val = default(MemoryBuffer);
			((MemoryBuffer)(ref val))._002Ector(num);
			((MemoryBuffer)(ref val)).Data[0] = opcode;
			BinaryPrimitives.WriteUInt32LittleEndian(((MemoryBuffer)(ref val)).Data.AsSpan(1, 4), channelId);
			if (payloadLength > 0)
			{
				Array.Copy(payload, 0, ((MemoryBuffer)(ref val)).Data, 5, payloadLength);
			}
			_connection.Send(((MemoryBuffer)(ref val)).Slice(num));
		}
	}

	private void PostClose(Connection connection)
	{
		_syncContext.Post(delegate(object c)
		{
			((Connection)c).OnClose();
		}, connection);
	}
}
