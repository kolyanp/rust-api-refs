using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Protocol.Serialization;

internal interface IPayloadHandler : IDisposable
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	IByteHandler ByteHandler
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		set;
	}

	void StartNewConversation();

	void SetNextSequenceNumber(int sequenceNumber);

	ValueTask<ArraySegment<byte>> ReadPayloadAsync([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ArraySegmentHolder<byte> cache, ProtocolErrorBehavior protocolErrorBehavior, IOBehavior ioBehavior);

	ValueTask WritePayloadAsync(ReadOnlyMemory<byte> payload, IOBehavior ioBehavior);
}
