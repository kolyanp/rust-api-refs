using System;
using System.Runtime.CompilerServices;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

internal static class ChangeUserPayload
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public static PayloadData Create([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string user, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> authResponse, string schemaName, CharacterSet characterSet, byte[] connectionAttributes)
	{
		ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
		byteBufferWriter.Write((byte)17);
		byteBufferWriter.WriteNullTerminatedString(user);
		byteBufferWriter.Write(checked((byte)authResponse.Length));
		byteBufferWriter.Write(authResponse);
		byteBufferWriter.WriteNullTerminatedString(schemaName ?? "");
		byteBufferWriter.Write((byte)characterSet);
		byteBufferWriter.Write((byte)0);
		byteBufferWriter.Write("mysql_native_password\0"u8);
		if (connectionAttributes != null)
		{
			byteBufferWriter.Write(connectionAttributes);
		}
		return byteBufferWriter.ToPayloadData();
	}
}
