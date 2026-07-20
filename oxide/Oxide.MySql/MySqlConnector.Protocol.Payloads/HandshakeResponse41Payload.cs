using System;
using System.Runtime.CompilerServices;
using MySqlConnector.Core;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class HandshakeResponse41Payload
{
	private static ByteBufferWriter CreateCapabilitiesPayload(ProtocolCapabilities serverCapabilities, ConnectionSettings cs, bool useCompression, CharacterSet characterSet, ProtocolCapabilities additionalCapabilities = ProtocolCapabilities.None)
	{
		ByteBufferWriter byteBufferWriter = new ByteBufferWriter();
		ProtocolCapabilities protocolCapabilities = (ProtocolCapabilities)((ulong)(ProtocolCapabilities.Protocol41 | (cs.InteractiveSession ? (serverCapabilities & ProtocolCapabilities.Interactive) : ProtocolCapabilities.None) | (serverCapabilities & ProtocolCapabilities.LongPassword) | (serverCapabilities & ProtocolCapabilities.Transactions) | ProtocolCapabilities.SecureConnection | (serverCapabilities & ProtocolCapabilities.PluginAuth) | (serverCapabilities & ProtocolCapabilities.PluginAuthLengthEncodedClientData) | ProtocolCapabilities.MultiStatements | ProtocolCapabilities.MultiResults) | (ulong)(cs.AllowLoadLocalInfile ? 128 : 0) | (ulong)(string.IsNullOrWhiteSpace(cs.Database) ? 0 : 8) | (ulong)(cs.UseAffectedRows ? 0 : 2) | (ulong)(useCompression ? 32 : 0) | (ulong)(serverCapabilities & ProtocolCapabilities.ConnectionAttributes) | (ulong)(serverCapabilities & ProtocolCapabilities.SessionTrack) | (ulong)(serverCapabilities & ProtocolCapabilities.DeprecateEof) | (ulong)(serverCapabilities & ProtocolCapabilities.QueryAttributes) | (ulong)(serverCapabilities & ProtocolCapabilities.MariaDbCacheMetadata) | (ulong)additionalCapabilities);
		byteBufferWriter.Write((int)protocolCapabilities);
		byteBufferWriter.Write(1073741824);
		byteBufferWriter.Write((byte)characterSet);
		ReadOnlySpan<byte> span = new byte[19]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0
		};
		byteBufferWriter.Write(span);
		if ((serverCapabilities & ProtocolCapabilities.LongPassword) == ProtocolCapabilities.None)
		{
			byteBufferWriter.Write((int)((ulong)protocolCapabilities >> 32));
		}
		else
		{
			byteBufferWriter.Write(0u);
		}
		return byteBufferWriter;
	}

	public static PayloadData CreateWithSsl(ProtocolCapabilities serverCapabilities, ConnectionSettings cs, bool useCompression, CharacterSet characterSet)
	{
		return CreateCapabilitiesPayload(serverCapabilities, cs, useCompression, characterSet, ProtocolCapabilities.Ssl).ToPayloadData();
	}

	public static PayloadData Create(InitialHandshakePayload handshake, ConnectionSettings cs, string password, bool useCompression, CharacterSet characterSet, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] byte[] connectionAttributes)
	{
		ByteBufferWriter byteBufferWriter = CreateCapabilitiesPayload(handshake.ProtocolCapabilities, cs, useCompression, characterSet, ProtocolCapabilities.None);
		byteBufferWriter.WriteNullTerminatedString(cs.UserID);
		byte[] array = AuthenticationUtility.CreateAuthenticationResponse(handshake.AuthPluginData, password);
		byteBufferWriter.Write((byte)array.Length);
		byteBufferWriter.Write(array);
		if (!string.IsNullOrWhiteSpace(cs.Database))
		{
			byteBufferWriter.WriteNullTerminatedString(cs.Database);
		}
		if ((handshake.ProtocolCapabilities & ProtocolCapabilities.PluginAuth) != ProtocolCapabilities.None)
		{
			byteBufferWriter.Write("mysql_native_password\0"u8);
		}
		if (connectionAttributes != null)
		{
			byteBufferWriter.Write(connectionAttributes);
		}
		return byteBufferWriter.ToPayloadData();
	}
}
