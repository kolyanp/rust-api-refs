using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;

namespace MySqlConnector.ColumnReaders;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class BitColumnReader : ColumnReader
{
	public static BitColumnReader Instance { get; } = new BitColumnReader();

	public override object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		return DoReadValue(data, columnDefinition);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public override int? TryReadInt32(ReadOnlySpan<byte> data, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ColumnDefinitionPayload columnDefinition)
	{
		return checked((int)DoReadValue(data, columnDefinition));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	private static ulong DoReadValue(ReadOnlySpan<byte> data, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ColumnDefinitionPayload columnDefinition)
	{
		if ((columnDefinition.ColumnFlags & ColumnFlags.Binary) == 0)
		{
			ulong num = 0uL;
			for (int i = 0; i < data.Length; i++)
			{
				num = num * 256 + data[i];
			}
			return num;
		}
		if (columnDefinition.ColumnLength <= 5 && data.Length == 1 && data[0] < (byte)(1 << (int)columnDefinition.ColumnLength))
		{
			return data[0];
		}
		if (Utf8Parser.TryParse(data, out ulong value, out int bytesConsumed, '\0') && bytesConsumed == data.Length)
		{
			return value;
		}
		throw new FormatException();
	}
}
