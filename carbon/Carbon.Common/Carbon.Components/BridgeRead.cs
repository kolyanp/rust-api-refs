using Facepunch;
using Network;

namespace Carbon.Components;

public sealed class BridgeRead : NetRead
{
	public BridgeConnection Connection;

	public static BridgeRead Rent(BufferStream stream, BridgeConnection conn = null)
	{
		BridgeRead bridgeRead = Pool.Get<BridgeRead>();
		bridgeRead.Init(stream, conn);
		return bridgeRead;
	}

	public static void Return(ref BridgeRead read)
	{
		if (((NetRead)read).stream != null)
		{
			BufferStream stream = ((NetRead)read).stream;
			Pool.Free<BufferStream>(ref stream);
			((NetRead)read).stream = null;
		}
		Pool.Free<BridgeRead>(ref read);
	}

	public void Init(BufferStream stream, BridgeConnection conn = null)
	{
		base.stream = stream;
		Connection = conn;
	}

	public BridgeMessages.Channels PeekBridgeMessage()
	{
		return ((NetRead)this).Peek<BridgeMessages.Channels>();
	}

	public BridgeMessages.Channels BridgeMessage()
	{
		return (BridgeMessages.Channels)((NetRead)this).Int32();
	}

	public void EnterPool()
	{
		Connection = null;
		((NetRead)this).EnterPool();
	}
}
