using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Fleck;
using UnityEngine;

namespace CompanionServer;

public sealed class BackhaulConnection
{
	private readonly Listener _listener;

	private readonly IWebSocketConnection _connection;

	private readonly SynchronizationContext _syncContext;

	private readonly ConcurrentDictionary<uint, Connection> _channels = new ConcurrentDictionary<uint, Connection>();

	public IPAddress Address { get; }

	public int ChannelCount => _channels.Count;

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
		Address = connection.ConnectionInfo.ClientIpAddress;
	}

	public void OnMessage(Span<byte> data)
	{
		if (data.Length >= 5)
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
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (payload.Length < 8)
		{
			SendFramed(2, channelId, default);
			return;
		}
		ulong num = BinaryPrimitives.ReadUInt64LittleEndian(payload.Slice(0, 8));
		if (_channels.ContainsKey(channelId))
		{
			return;
		}
		if (num == 0L || !_listener.TryAddPlayerChannel(num))
		{
			SendFramed(2, channelId, default);
			return;
		}
		long connectionId = _listener.NextConnectionId();
		_channels[channelId] = new Connection(connectionId, _listener, this, channelId, num);
		if (!IsAvailable && TryRemoveChannel(channelId, out var connection))
		{
			PostClose(connection);
		}
	}

	private void HandleData(uint channelId, Span<byte> payload)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (payload.Length <= 1048576 && _channels.TryGetValue(channelId, out var value))
		{
			MemoryBuffer val = new MemoryBuffer(payload.Length);
			payload.CopyTo(MemoryBuffer.op_Implicit(val));
			_listener.Enqueue(value, val.Slice(payload.Length));
		}
	}

	private void HandleClose(uint channelId)
	{
		if (TryRemoveChannel(channelId, out var connection))
		{
			PostClose(connection);
		}
	}

	public void OnBackhaulClosed()
	{
		foreach (KeyValuePair<uint, Connection> channel in _channels)
		{
			if (TryRemoveChannel(channel.Key, out var connection))
			{
				connection.OnClose();
			}
		}
	}

	private bool TryRemoveChannel(uint channelId, out Connection connection)
	{
		if (!_channels.TryRemove(channelId, out connection))
		{
			return false;
		}
		_listener.RemovePlayerChannel(connection.SteamId);
		return true;
	}

	internal void SendData(uint channelId, MemoryBuffer payload)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		SendFramed(0, channelId, payload);
	}

	internal void CloseChannelLocal(uint channelId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (TryRemoveChannel(channelId, out var connection))
		{
			SendFramed(2, channelId, default);
			PostClose(connection);
		}
	}

	public void Close()
	{
		IWebSocketConnection connection = _connection;
		if (connection != null)
		{
			connection.Close();
		}
	}

	private void SendFramed(byte opcode, uint channelId, MemoryBuffer payload)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (_connection == null || !_connection.IsAvailable)
		{
			payload.Dispose();
			return;
		}
		if (payload.Length > 2097147)
		{
			Debug.LogError((object)$"Rust+ message is too large to send ({payload.Length} bytes), dropping it");
			payload.Dispose();
			return;
		}
		Span<byte> span = stackalloc byte[5];
		span[0] = opcode;
		BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(1, 4), channelId);
		_connection.Send((ReadOnlySpan<byte>)span, payload);
	}

	private void PostClose(Connection connection)
	{
		_syncContext.Post((object c) =>
		{
			((Connection)c).OnClose();
		}, connection);
	}
}
