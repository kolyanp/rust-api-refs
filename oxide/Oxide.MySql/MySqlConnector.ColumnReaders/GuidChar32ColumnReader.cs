using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Utilities;

namespace MySqlConnector.ColumnReaders;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class GuidChar32ColumnReader : ColumnReader
{
	public static GuidChar32ColumnReader Instance { get; } = new GuidChar32ColumnReader();

	public override object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		if (!Utf8Parser.TryParse(data, out Guid value, out int bytesConsumed, 'N') || bytesConsumed != 32)
		{
			throw new FormatException("Could not parse CHAR(32) value as Guid: " + Utility.GetString(Encoding.UTF8, data));
		}
		return value;
	}
}
