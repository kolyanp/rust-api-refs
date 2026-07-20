using System.Runtime.CompilerServices;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

internal static class InitDatabasePayload
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static PayloadData Create(string databaseName)
	{
		ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
		byteBufferWriter.Write((byte)2);
		byteBufferWriter.Write(databaseName);
		return byteBufferWriter.ToPayloadData();
	}
}
