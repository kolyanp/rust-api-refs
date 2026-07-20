using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

internal sealed class ByteBufferWriter : IBufferWriter<byte>
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Encoder m_encoder;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private byte[] m_buffer;

	private Memory<byte> m_output;

	public int Position => m_buffer.Length - m_output.Length;

	public ArraySegment<byte> ArraySegment => new ArraySegment<byte>(m_buffer, 0, Position);

	public ByteBufferWriter(int capacity = 0)
	{
		m_buffer = ArrayPool<byte>.Shared.Rent(Math.Max(capacity, 128));
		m_output = m_buffer;
	}

	public PayloadData ToPayloadData()
	{
		return new PayloadData(ArraySegment, isPooled: true);
	}

	public Memory<byte> GetMemory(int sizeHint = 0)
	{
		if (sizeHint > m_output.Length)
		{
			Reallocate(sizeHint);
		}
		return m_output;
	}

	public Span<byte> GetSpan(int sizeHint = 0)
	{
		if (sizeHint > m_output.Length)
		{
			Reallocate(sizeHint);
		}
		return m_output.Span;
	}

	public void Advance(int count)
	{
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(count, output.Length - count);
	}

	public void TrimEnd(int byteCount)
	{
		m_output = MemoryExtensions.AsMemory(m_buffer, Position - byteCount);
	}

	public void Write(byte value)
	{
		if (m_output.Length < 1)
		{
			Reallocate();
		}
		m_output.Span[0] = value;
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(1, output.Length - 1);
	}

	public void Write(ushort value)
	{
		if (m_output.Length < 2)
		{
			Reallocate(2);
		}
		BinaryPrimitives.WriteUInt16LittleEndian(m_output.Span, value);
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(2, output.Length - 2);
	}

	public void Write(int value)
	{
		if (m_output.Length < 4)
		{
			Reallocate(4);
		}
		BinaryPrimitives.WriteInt32LittleEndian(m_output.Span, value);
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(4, output.Length - 4);
	}

	public void Write(uint value)
	{
		if (m_output.Length < 4)
		{
			Reallocate(4);
		}
		BinaryPrimitives.WriteUInt32LittleEndian(m_output.Span, value);
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(4, output.Length - 4);
	}

	public void Write(ulong value)
	{
		if (m_output.Length < 8)
		{
			Reallocate(8);
		}
		BinaryPrimitives.WriteUInt64LittleEndian(m_output.Span, value);
		ref Memory<byte> output = ref m_output;
		m_output = output.Slice(8, output.Length - 8);
	}

	public void Write(ArraySegment<byte> arraySegment)
	{
		Write(MemoryExtensions.AsSpan(arraySegment));
	}

	public void Write(ReadOnlySpan<byte> span)
	{
		if (m_output.Length < span.Length)
		{
			Reallocate(span.Length);
		}
		span.CopyTo(m_output.Span);
		ref Memory<byte> output = ref m_output;
		int length = span.Length;
		m_output = output.Slice(length, output.Length - length);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public void Write(string value)
	{
		Write(MemoryExtensions.AsSpan(value));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public void WriteAscii(string value)
	{
		WriteAscii(MemoryExtensions.AsSpan(value));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public void Write(string value, int offset, int length)
	{
		Write(MemoryExtensions.AsSpan(value, offset, length));
	}

	public void Write(ReadOnlySpan<char> chars)
	{
		if (m_output.Length < chars.Length * 3)
		{
			int byteCount = Utility.GetByteCount(Encoding.UTF8, chars);
			if (m_output.Length < byteCount)
			{
				Reallocate(byteCount);
			}
		}
		ref Memory<byte> output = ref m_output;
		int bytes = Encoding.UTF8.GetBytes(chars, m_output.Span);
		m_output = output.Slice(bytes, output.Length - bytes);
	}

	public void Write(ReadOnlySpan<char> chars, bool flush)
	{
		if (m_encoder == null)
		{
			m_encoder = Encoding.UTF8.GetEncoder();
		}
		int num;
		while (chars.Length > 0)
		{
			if (m_output.Length < 4)
			{
				Reallocate();
			}
			m_encoder.Convert(chars, m_output.Span, flush: false, out var charsUsed, out var bytesUsed, out var completed);
			num = charsUsed;
			chars = chars.Slice(num, chars.Length - num);
			ref Memory<byte> output = ref m_output;
			num = bytesUsed;
			m_output = output.Slice(num, output.Length - num);
			if (!completed)
			{
				Reallocate();
			}
		}
		if (flush && m_encoder != null)
		{
			if (m_output.Length < 4)
			{
				Reallocate();
			}
			m_encoder.Convert(MemoryExtensions.AsSpan(""), m_output.Span, flush: true, out num, out var bytesUsed2, out var _);
			ref Memory<byte> output = ref m_output;
			num = bytesUsed2;
			m_output = output.Slice(num, output.Length - num);
		}
	}

	public void WriteAscii(ReadOnlySpan<char> chars)
	{
		if (m_output.Length < chars.Length)
		{
			Reallocate(chars.Length);
		}
		ref Memory<byte> output = ref m_output;
		int bytes = Encoding.ASCII.GetBytes(chars, m_output.Span);
		m_output = output.Slice(bytes, output.Length - bytes);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public void WriteLengthEncodedString(StringBuilder stringBuilder)
	{
		this.WriteLengthEncodedString(stringBuilder.ToString());
	}

	public void WriteString(short value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(6), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	public void WriteString(ushort value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(5), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	public void WriteString(int value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(11), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	public void WriteString(uint value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(10), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	public void WriteString(long value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(20), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	public void WriteString(ulong value)
	{
		Utf8Formatter.TryFormat(value, GetSpan(20), out var bytesWritten);
		ref Memory<byte> output = ref m_output;
		int num = bytesWritten;
		m_output = output.Slice(num, output.Length - num);
	}

	private void Reallocate(int additional = 0)
	{
		int position = Position;
		byte[] array = ArrayPool<byte>.Shared.Rent(Math.Max(position + additional, m_buffer.Length * 2));
		MemoryExtensions.AsSpan(m_buffer, 0, position).CopyTo(array);
		ArrayPool<byte>.Shared.Return(m_buffer);
		m_buffer = array;
		m_output = new Memory<byte>(m_buffer, position, m_buffer.Length - position);
	}
}
