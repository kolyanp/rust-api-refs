using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
internal abstract class ObservableInstrument<T> : Instrument where T : struct
{
	public override bool IsObservable => true;

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	protected ObservableInstrument(Meter meter, string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string unit, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string description)
		: base(meter, name, unit, description)
	{
		Instrument.ValidateTypeParameter<T>();
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 0 })]
	protected abstract IEnumerable<Measurement<T>> Observe();

	[SecuritySafeCritical]
	internal override void Observe(MeterListener listener)
	{
		object subscriptionState = GetSubscriptionState(listener);
		IEnumerable<Measurement<T>> enumerable = Observe();
		if (enumerable == null)
		{
			return;
		}
		foreach (Measurement<T> item in enumerable)
		{
			listener.NotifyMeasurement(this, item.Value, item.Tags, subscriptionState);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static IEnumerable<Measurement<T>> Observe(object callback)
	{
		if (callback is Func<T> func)
		{
			return new Measurement<T>[1]
			{
				new Measurement<T>(func())
			};
		}
		if (callback is Func<Measurement<T>> func2)
		{
			return new Measurement<T>[1] { func2() };
		}
		if (callback is Func<IEnumerable<Measurement<T>>> func3)
		{
			return func3();
		}
		return null;
	}
}
