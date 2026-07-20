using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MySqlConnector.Protocol;

internal readonly struct PayloadData : IDisposable
{
	private readonly bool m_isPooled;

	public ReadOnlyMemory<byte> Memory { get; }

	public ReadOnlySpan<byte> Span => Memory.Span;

	public byte HeaderByte => Span[0];

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public PayloadData(byte[] data)
	{
		m_isPooled = false;
		Memory = data;
	}

	public PayloadData(ReadOnlyMemory<byte> data, bool isPooled = false)
	{
		Memory = data;
		m_isPooled = isPooled;
	}

	public void Dispose()
	{
		if (m_isPooled && MemoryMarshal.TryGetArray(Memory, out var segment))
		{
			ArrayPool<byte>.Shared.Return(segment.Array);
		}
	}
}
