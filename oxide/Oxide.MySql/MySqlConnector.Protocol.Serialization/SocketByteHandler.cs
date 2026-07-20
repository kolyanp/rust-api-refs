using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

internal sealed class SocketByteHandler : IByteHandler, IDisposable
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly Socket m_socket;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly SocketAwaitable m_socketAwaitable;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly Action m_closeSocket;

	public int RemainingTimeout { get; set; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public SocketByteHandler(Socket socket)
	{
		m_socket = socket;
		m_socketAwaitable = new SocketAwaitable(new SocketAsyncEventArgs());
		m_closeSocket = socket.Dispose;
		RemainingTimeout = int.MaxValue;
	}

	public void Dispose()
	{
		m_socketAwaitable.EventArgs.Dispose();
	}

	public ValueTask<int> ReadBytesAsync(Memory<byte> buffer, IOBehavior ioBehavior)
	{
		if (ioBehavior != IOBehavior.Asynchronous)
		{
			return DoReadBytesSync(buffer);
		}
		return DoReadBytesAsync(buffer);
	}

	private ValueTask<int> DoReadBytesSync(Memory<byte> buffer)
	{
		MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment);
		try
		{
			if (RemainingTimeout == int.MaxValue)
			{
				return new ValueTask<int>(m_socket.Receive(segment.Array, segment.Offset, segment.Count, SocketFlags.None));
			}
			while (RemainingTimeout > 0)
			{
				int tickCount = Environment.TickCount;
				if (m_socket.Poll(Math.Min(2147483, RemainingTimeout) * 1000, SelectMode.SelectRead))
				{
					int result = m_socket.Receive(segment.Array, segment.Offset, segment.Count, SocketFlags.None);
					RemainingTimeout -= Environment.TickCount - tickCount;
					return new ValueTask<int>(result);
				}
				RemainingTimeout -= Environment.TickCount - tickCount;
			}
			return ValueTaskExtensions.FromException<int>(MySqlException.CreateForTimeout());
		}
		catch (Exception exception)
		{
			return ValueTaskExtensions.FromException<int>(exception);
		}
	}

	private async ValueTask<int> DoReadBytesAsync(Memory<byte> buffer)
	{
		int startTime = ((RemainingTimeout != int.MaxValue) ? Environment.TickCount : 0);
		int remainingTimeout = RemainingTimeout;
		if (remainingTimeout <= 0)
		{
			throw MySqlException.CreateForTimeout();
		}
		uint num = ((remainingTimeout != int.MaxValue) ? TimerQueue.Instance.Add(RemainingTimeout, m_closeSocket) : 0u);
		uint timerId = num;
		m_socketAwaitable.EventArgs.SetBuffer(buffer);
		int bytesTransferred;
		try
		{
			await m_socket.ReceiveAsync(m_socketAwaitable);
			bytesTransferred = m_socketAwaitable.EventArgs.BytesTransferred;
		}
		catch (SocketException innerException)
		{
			if (RemainingTimeout != int.MaxValue)
			{
				RemainingTimeout -= Environment.TickCount - startTime;
				if (!TimerQueue.Instance.Remove(timerId))
				{
					throw MySqlException.CreateForTimeout(innerException);
				}
			}
			throw;
		}
		if (RemainingTimeout != int.MaxValue)
		{
			RemainingTimeout -= Environment.TickCount - startTime;
			if (!TimerQueue.Instance.Remove(timerId))
			{
				throw MySqlException.CreateForTimeout();
			}
		}
		return bytesTransferred;
	}

	public ValueTask WriteBytesAsync(ReadOnlyMemory<byte> data, IOBehavior ioBehavior)
	{
		if (ioBehavior == IOBehavior.Asynchronous)
		{
			return DoWriteBytesAsync(data);
		}
		try
		{
			m_socket.Send(data, SocketFlags.None);
			return default(ValueTask);
		}
		catch (Exception exception)
		{
			return ValueTaskExtensions.FromException(exception);
		}
	}

	private async ValueTask DoWriteBytesAsync(ReadOnlyMemory<byte> data)
	{
		m_socketAwaitable.EventArgs.SetBuffer(MemoryMarshal.AsMemory(data));
		await m_socket.SendAsync(m_socketAwaitable);
	}
}
