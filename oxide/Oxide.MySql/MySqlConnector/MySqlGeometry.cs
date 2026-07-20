using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

public sealed class MySqlGeometry
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly byte[] m_bytes;

	public int SRID => BinaryPrimitives.ReadInt32LittleEndian(m_bytes);

	public ReadOnlySpan<byte> WKB
	{
		get
		{
			ReadOnlySpan<byte> valueSpan = ValueSpan;
			return valueSpan.Slice(4, valueSpan.Length - 4);
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public byte[] Value
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return ValueSpan.ToArray();
		}
	}

	internal ReadOnlySpan<byte> ValueSpan => m_bytes;

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public static MySqlGeometry FromWkb(int srid, ReadOnlySpan<byte> wkb)
	{
		byte[] array = new byte[wkb.Length + 4];
		BinaryPrimitives.WriteInt32LittleEndian(array, srid);
		Span<byte> span = MemoryExtensions.AsSpan(array);
		wkb.CopyTo(span.Slice(4, span.Length - 4));
		return new MySqlGeometry(array);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public static MySqlGeometry FromMySql(ReadOnlySpan<byte> value)
	{
		return new MySqlGeometry(value.ToArray());
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	internal MySqlGeometry(byte[] bytes)
	{
		m_bytes = bytes;
	}
}
