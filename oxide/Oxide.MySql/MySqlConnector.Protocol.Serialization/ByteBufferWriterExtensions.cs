using System;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class ByteBufferWriterExtensions
{
	public static void WriteLengthEncodedInteger(this ByteBufferWriter writer, ulong value)
	{
		if (value < 65536)
		{
			if (value < 251)
			{
				writer.Write((byte)value);
				return;
			}
			writer.Write((byte)252);
			writer.Write((ushort)value);
		}
		else if (value < 16777216)
		{
			writer.Write((uint)((value << 8) | 0xFD));
		}
		else
		{
			writer.Write((byte)254);
			writer.Write(value);
		}
	}

	public static void WriteLengthEncodedString(this ByteBufferWriter writer, string value)
	{
		writer.WriteLengthEncodedString(MemoryExtensions.AsSpan(value));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static void WriteLengthEncodedString([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] this ByteBufferWriter writer, ReadOnlySpan<char> value)
	{
		int byteCount = Utility.GetByteCount(Encoding.UTF8, value);
		writer.WriteLengthEncodedInteger((ulong)byteCount);
		writer.Write(value);
	}

	public static void WriteLengthEncodedAsciiString(this ByteBufferWriter writer, string value)
	{
		writer.WriteLengthEncodedInteger((ulong)value.Length);
		writer.WriteAscii(MemoryExtensions.AsSpan(value));
	}

	public static void WriteNullTerminatedString(this ByteBufferWriter writer, string value)
	{
		writer.Write(value);
		writer.Write((byte)0);
	}
}
