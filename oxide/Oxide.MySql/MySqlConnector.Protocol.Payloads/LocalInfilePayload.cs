using System;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Payloads;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal readonly struct LocalInfilePayload
{
	public const byte Signature = 251;

	public string FileName { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static LocalInfilePayload Create(ReadOnlySpan<byte> span)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byteArrayReader.ReadByte(251);
		return new LocalInfilePayload(Utility.GetString(Encoding.UTF8, byteArrayReader.ReadByteString(byteArrayReader.BytesRemaining)));
	}

	private LocalInfilePayload(string fileName)
	{
		FileName = fileName;
	}
}
