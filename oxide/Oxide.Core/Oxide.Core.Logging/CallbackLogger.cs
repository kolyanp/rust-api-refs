namespace Oxide.Core.Logging;

public class CallbackLogger : Logger
{
	private NativeDebugCallback callback;

	public CallbackLogger(NativeDebugCallback callback)
		: base(processImmediately: true)
	{
		this.callback = callback;
	}

	protected override void ProcessMessage(LogMessage message)
	{
		callback?.Invoke(message.LogfileMessage);
	}
}
