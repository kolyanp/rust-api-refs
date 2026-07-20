using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MySqlConnector.Utilities;

internal static class SocketExtensions
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
	{
		awaitable.Reset();
		if (!socket.ReceiveAsync(awaitable.EventArgs))
		{
			awaitable.WasCompleted = true;
		}
		return awaitable;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static SocketAwaitable SendAsync(this Socket socket, SocketAwaitable awaitable)
	{
		awaitable.Reset();
		if (!socket.SendAsync(awaitable.EventArgs))
		{
			awaitable.WasCompleted = true;
		}
		return awaitable;
	}

	public static void SetBuffer([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this SocketAsyncEventArgs args, Memory<byte> buffer)
	{
		MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment);
		args.SetBuffer(segment.Array, segment.Offset, segment.Count);
	}

	public static int Send([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this Socket socket, ReadOnlyMemory<byte> data, SocketFlags flags)
	{
		MemoryMarshal.TryGetArray(data, out var segment);
		return socket.Send(segment.Array, segment.Offset, segment.Count, flags);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static void SetKeepAlive(this Socket socket, uint keepAliveTimeSeconds)
	{
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, optionValue: true);
		if (keepAliveTimeSeconds != 0 && Utility.IsWindows())
		{
			uint num = ((keepAliveTimeSeconds > 4294967) ? uint.MaxValue : (keepAliveTimeSeconds * 1000));
			socket.IOControl(IOControlCode.KeepAliveValues, new byte[12]
			{
				1,
				0,
				0,
				0,
				(byte)(num & 0xFF),
				(byte)((num >> 8) & 0xFF),
				(byte)((num >> 16) & 0xFF),
				(byte)((num >> 24) & 0xFF),
				232,
				3,
				0,
				0
			}, null);
		}
	}
}
