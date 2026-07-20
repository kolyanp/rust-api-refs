using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MySqlConnector.Utilities;

namespace MySqlConnector.Protocol.Serialization;

internal sealed class StandardPayloadHandler : IPayloadHandler, IDisposable
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private readonly Func<int> m_getNextSequenceNumber;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private IByteHandler m_byteHandler;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private BufferedByteReader m_bufferedByteReader;

	private byte m_sequenceNumber;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public IByteHandler ByteHandler
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			if (m_byteHandler == null)
			{
				throw new ObjectDisposedException("StandardPayloadHandler");
			}
			return m_byteHandler;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		set
		{
			IByteHandler byteHandler = m_byteHandler;
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_byteHandler = value;
			byteHandler?.Dispose();
			m_bufferedByteReader = new BufferedByteReader();
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public StandardPayloadHandler(IByteHandler byteHandler)
	{
		ByteHandler = byteHandler;
		m_getNextSequenceNumber = () => m_sequenceNumber++;
	}

	public void Dispose()
	{
		m_bufferedByteReader = null;
		Utility.Dispose(ref m_byteHandler);
	}

	public void StartNewConversation()
	{
		m_sequenceNumber = 0;
	}

	public void SetNextSequenceNumber(int sequenceNumber)
	{
		m_sequenceNumber = (byte)sequenceNumber;
	}

	public ValueTask<ArraySegment<byte>> ReadPayloadAsync([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ArraySegmentHolder<byte> cache, ProtocolErrorBehavior protocolErrorBehavior, IOBehavior ioBehavior)
	{
		return ProtocolUtility.ReadPayloadAsync(m_bufferedByteReader, m_byteHandler, m_getNextSequenceNumber, cache, protocolErrorBehavior, ioBehavior);
	}

	public ValueTask WritePayloadAsync(ReadOnlyMemory<byte> payload, IOBehavior ioBehavior)
	{
		return ProtocolUtility.WritePayloadAsync(m_byteHandler, m_getNextSequenceNumber, payload, ioBehavior);
	}
}
