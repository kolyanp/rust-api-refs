using System;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

internal readonly struct EofPayload
{
	public const byte Signature = 254;

	public int WarningCount { get; }

	public ServerStatus ServerStatus { get; }

	public static EofPayload Create(ReadOnlySpan<byte> span)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		byteArrayReader.ReadByte(254);
		if (span.Length > 5)
		{
			throw new FormatException("Not an EOF packet");
		}
		ushort warningCount = byteArrayReader.ReadUInt16();
		ServerStatus status = (ServerStatus)byteArrayReader.ReadUInt16();
		if (byteArrayReader.BytesRemaining != 0)
		{
			throw new FormatException("Extra bytes at end of payload.");
		}
		return new EofPayload(warningCount, status);
	}

	public static bool IsEof(PayloadData payload)
	{
		int length = payload.Span.Length;
		if (length > 0 && length < 9)
		{
			return payload.HeaderByte == 254;
		}
		return false;
	}

	private EofPayload(int warningCount, ServerStatus status)
	{
		WarningCount = warningCount;
		ServerStatus = status;
	}
}
