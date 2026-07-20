using System;
using System.Threading.Tasks;

namespace MySqlConnector.Protocol.Serialization;

internal interface IByteHandler : IDisposable
{
	int RemainingTimeout { get; set; }

	ValueTask<int> ReadBytesAsync(Memory<byte> buffer, IOBehavior ioBehavior);

	ValueTask WriteBytesAsync(ReadOnlyMemory<byte> data, IOBehavior ioBehavior);
}
