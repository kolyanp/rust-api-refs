using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
internal sealed class ObservableGauge<T> : ObservableInstrument<T> where T : struct
{
	private object _callback;

	internal ObservableGauge(Meter meter, string name, Func<T> observeValue, string unit, string description)
		: base(meter, name, unit, description)
	{
		_callback = observeValue ?? throw new ArgumentNullException("observeValue");
		Publish();
	}

	internal ObservableGauge(Meter meter, string name, Func<Measurement<T>> observeValue, string unit, string description)
		: base(meter, name, unit, description)
	{
		_callback = observeValue ?? throw new ArgumentNullException("observeValue");
		Publish();
	}

	internal ObservableGauge(Meter meter, string name, Func<IEnumerable<Measurement<T>>> observeValues, string unit, string description)
		: base(meter, name, unit, description)
	{
		_callback = observeValues ?? throw new ArgumentNullException("observeValues");
		Publish();
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 0 })]
	protected override IEnumerable<Measurement<T>> Observe()
	{
		return ObservableInstrument<T>.Observe(_callback);
	}
}
