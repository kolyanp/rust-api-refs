using System;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Payloads;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal readonly struct AuthenticationMethodSwitchRequestPayload
{
	public const byte Signature = 254;

	public string Name { get; }

	public byte[] Data { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static AuthenticationMethodSwitchRequestPayload Create(ReadOnlySpan<byte> span)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byteArrayReader.ReadByte(254);
		string name;
		byte[] data;
		if (span.Length == 1)
		{
			name = "mysql_old_password";
			data = Array.Empty<byte>();
		}
		else
		{
			name = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadNullTerminatedByteString());
			data = byteArrayReader.ReadByteString(byteArrayReader.BytesRemaining).ToArray();
		}
		return new AuthenticationMethodSwitchRequestPayload(name, data);
	}

	private AuthenticationMethodSwitchRequestPayload(string name, byte[] data)
	{
		Name = name;
		Data = data;
	}
}
