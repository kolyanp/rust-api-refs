using System;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Payloads;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal readonly struct ErrorPayload
{
	public const byte Signature = byte.MaxValue;

	public int ErrorCode { get; }

	public string State { get; }

	public string Message { get; }

	public MySqlException ToException()
	{
		return new MySqlException((MySqlErrorCode)ErrorCode, State, Message);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public static ErrorPayload Create(ReadOnlySpan<byte> span)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byteArrayReader.ReadByte(byte.MaxValue);
		ushort errorCode = byteArrayReader.ReadUInt16();
		string text = Utility.GetString(Encoding.ASCII, byteArrayReader.ReadByteString(1));
		string state;
		string message;
		if (text == "#")
		{
			state = Utility.GetString(Encoding.ASCII, byteArrayReader.ReadByteString(5));
			message = Utility.GetString(Encoding.UTF8, byteArrayReader.ReadByteString(span.Length - 9));
		}
		else
		{
			state = "HY000";
			message = text + Utility.GetString(Encoding.UTF8, byteArrayReader.ReadByteString(span.Length - 4));
		}
		return new ErrorPayload(errorCode, state, message);
	}

	private ErrorPayload(int errorCode, string state, string message)
	{
		ErrorCode = errorCode;
		State = state;
		Message = message;
	}
}
