using System.Runtime.CompilerServices;

namespace System.Diagnostics.Metrics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
internal struct ListenerSubscription
{
	internal MeterListener Listener { get; }

	internal object State { get; }

	internal ListenerSubscription(MeterListener listener, object state = null)
	{
		Listener = listener;
		State = state;
	}
}
