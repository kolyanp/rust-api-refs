using System;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Protocol.Payloads;

internal readonly struct ColumnCountPayload
{
	public int ColumnCount { get; }

	public bool MetadataFollows { get; }

	private ColumnCountPayload(int columnCount, bool metadataFollows)
	{
		ColumnCount = columnCount;
		MetadataFollows = metadataFollows;
	}

	public static ColumnCountPayload Create(ReadOnlySpan<byte> span, bool supportsOptionalMetadata)
	{
		ByteArrayReader byteArrayReader = new ByteArrayReader(span);
		int columnCount = (int)byteArrayReader.ReadLengthEncodedInteger();
		bool metadataFollows = !supportsOptionalMetadata || byteArrayReader.BytesRemaining == 0 || byteArrayReader.ReadByte() == 1;
		return new ColumnCountPayload(columnCount, metadataFollows);
	}
}
