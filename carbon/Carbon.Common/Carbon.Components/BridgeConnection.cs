using System;
using Facepunch;
using Fleck;

namespace Carbon.Components;

public sealed class BridgeConnection : IPooled
{
	public int Id;

	public IWebSocketConnection Socket;

	public BridgeMessages Messages;

	public object Reference;

	public BridgeConnection Init(int id, IWebSocketConnection connection, BridgeMessages messages)
	{
		Id = id;
		Socket = connection;
		Messages = messages;
		return this;
	}

	public void Send(BridgeWrite write)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (Socket == null || write == null || !Socket.IsAvailable)
		{
			return;
		}
		try
		{
			Socket.Send(write.GetMemory());
		}
		catch (Exception ex)
		{
			Logger.Error("BridgeConnection.Send failure", ex);
		}
	}

	public void EnterPool()
	{
		Id = 0;
		Messages = null;
		Socket = null;
		Reference = null;
	}

	public void LeavePool()
	{
	}
}
