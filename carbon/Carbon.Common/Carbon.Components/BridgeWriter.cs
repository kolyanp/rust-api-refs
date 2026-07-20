using System;

namespace Carbon.Components;

public struct BridgeWriter : IDisposable
{
	public BridgeWrite write;

	public static BridgeWriter Begin()
	{
		return new BridgeWriter
		{
			write = BridgeWrite.Rent()
		};
	}

	public void Dispose()
	{
		if (write != null)
		{
			BridgeWrite.Return(ref write);
		}
	}
}
