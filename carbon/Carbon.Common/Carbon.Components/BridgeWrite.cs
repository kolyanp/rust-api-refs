using Facepunch;
using Fleck;
using Network;

namespace Carbon.Components;

public sealed class BridgeWrite : NetWrite
{
	public static BridgeWrite Rent()
	{
		BridgeWrite bridgeWrite = Pool.Get<BridgeWrite>();
		((NetWrite)bridgeWrite).Start((BaseNetwork)(object)Net.sv);
		return bridgeWrite;
	}

	public static void Return(ref BridgeWrite write)
	{
		Pool.Free<BridgeWrite>(ref write);
	}

	public MemoryBuffer GetMemory(bool fromPool = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		(byte[], int) buffer = ((NetWrite)this).GetBuffer();
		return new MemoryBuffer(buffer.Item1, buffer.Item2, fromPool);
	}

	public void BridgeMessage(BridgeMessages.Channels message)
	{
		((NetWrite)this).Int32((int)message);
	}
}
