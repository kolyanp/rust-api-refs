namespace Carbon.Components;

public abstract class BridgeMessages
{
	public enum Channels
	{
		Rpc,
		Command,
		Custom
	}

	public virtual bool ShouldPool => true;

	protected abstract void OnRpc(BridgeRead read);

	protected abstract void OnCommand(BridgeRead read);

	protected abstract void OnCustom(BridgeRead read);

	protected abstract void OnUnhandled(BridgeRead read);

	public void HandleChannelRead(BridgeRead read)
	{
		switch (read.BridgeMessage())
		{
		case Channels.Rpc:
			OnRpc(read);
			break;
		case Channels.Command:
			OnCommand(read);
			break;
		case Channels.Custom:
			OnCustom(read);
			break;
		default:
			OnUnhandled(read);
			break;
		}
	}
}
