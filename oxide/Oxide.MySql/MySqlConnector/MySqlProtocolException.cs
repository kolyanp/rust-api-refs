using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MySqlConnector;

[Serializable]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlProtocolException : InvalidOperationException
{
	internal static MySqlProtocolException CreateForPacketOutOfOrder(int expectedSequenceNumber, int packetSequenceNumber)
	{
		return new MySqlProtocolException($"Packet received out-of-order. Expected {expectedSequenceNumber:d}; got {packetSequenceNumber:d}.");
	}

	private MySqlProtocolException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private MySqlProtocolException(string message)
		: base(message)
	{
	}
}
