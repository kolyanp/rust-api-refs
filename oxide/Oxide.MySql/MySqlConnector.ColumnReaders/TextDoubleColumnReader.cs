using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Utilities;

namespace MySqlConnector.ColumnReaders;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class TextDoubleColumnReader : ColumnReader
{
	public static TextDoubleColumnReader Instance { get; } = new TextDoubleColumnReader();

	public override object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		if (Utf8Parser.TryParse(data, out double value, out int bytesConsumed, '\0') && bytesConsumed == data.Length)
		{
			return value;
		}
		if (data.SequenceEqual("-inf"u8))
		{
			return double.NegativeInfinity;
		}
		if (data.SequenceEqual("inf"u8))
		{
			return double.PositiveInfinity;
		}
		if (data.SequenceEqual("nan"u8))
		{
			return double.NaN;
		}
		throw new FormatException("Couldn't parse value as double: " + Utility.GetString(Encoding.UTF8, data));
	}
}
