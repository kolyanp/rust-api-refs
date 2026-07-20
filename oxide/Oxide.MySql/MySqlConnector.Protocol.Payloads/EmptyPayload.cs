using System;

namespace MySqlConnector.Protocol.Payloads;

internal static class EmptyPayload
{
	public static PayloadData Instance { get; } = new PayloadData(Array.Empty<byte>());
}
