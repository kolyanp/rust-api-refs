using System;
using System.Runtime.CompilerServices;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal readonly struct AuthenticationMoreDataPayload
{
	public const byte Signature = 1;

	public byte[] Data { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static AuthenticationMoreDataPayload Create(ReadOnlySpan<byte> span)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byteArrayReader.ReadByte(1);
		return new AuthenticationMoreDataPayload(byteArrayReader.ReadByteString(byteArrayReader.BytesRemaining).ToArray());
	}

	private AuthenticationMoreDataPayload(byte[] data)
	{
		Data = data;
	}
}
