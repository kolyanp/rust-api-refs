using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
internal class Meter : IDisposable
{
	private static readonly List<Meter> s_allMeters = new List<Meter>();

	private List<Instrument> _instruments = new List<Instrument>();

	internal bool Disposed { get; private set; }

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)]
	public string Name
	{
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
		get;
	}

	public string Version { get; }

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public Meter(string name)
		: this(name, null)
	{
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public Meter(string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string version)
	{
		Name = name ?? throw new ArgumentNullException("name");
		Version = version;
		lock (Instrument.SyncObject)
		{
			s_allMeters.Add(this);
		}
		GC.KeepAlive(MetricsEventSource.Log);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public Counter<T> CreateCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, string unit = null, string description = null) where T : struct
	{
		return new Counter<T>(this, name, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public Histogram<T> CreateHistogram<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, string unit = null, string description = null) where T : struct
	{
		return new Histogram<T>(this, name, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public UpDownCounter<T> CreateUpDownCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, string unit = null, string description = null) where T : struct
	{
		return new UpDownCounter<T>(this, name, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableUpDownCounter<T> CreateObservableUpDownCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })] Func<T> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableUpDownCounter<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableUpDownCounter<T> CreateObservableUpDownCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 0 })] Func<Measurement<T>> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableUpDownCounter<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableUpDownCounter<T> CreateObservableUpDownCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 1, 0, 0 })] Func<IEnumerable<Measurement<T>>> observeValues, string unit = null, string description = null) where T : struct
	{
		return new ObservableUpDownCounter<T>(this, name, observeValues, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableCounter<T> CreateObservableCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })] Func<T> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableCounter<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableCounter<T> CreateObservableCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 0 })] Func<Measurement<T>> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableCounter<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableCounter<T> CreateObservableCounter<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 1, 0, 0 })] Func<IEnumerable<Measurement<T>>> observeValues, string unit = null, string description = null) where T : struct
	{
		return new ObservableCounter<T>(this, name, observeValues, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableGauge<T> CreateObservableGauge<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })] Func<T> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableGauge<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableGauge<T> CreateObservableGauge<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 0 })] Func<Measurement<T>> observeValue, string unit = null, string description = null) where T : struct
	{
		return new ObservableGauge<T>(this, name, observeValue, unit, description);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0 })]
	public ObservableGauge<T> CreateObservableGauge<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)] T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 1, 0, 0 })] Func<IEnumerable<Measurement<T>>> observeValues, string unit = null, string description = null) where T : struct
	{
		return new ObservableGauge<T>(this, name, observeValues, unit, description);
	}

	public void Dispose()
	{
		List<Instrument> list = null;
		lock (Instrument.SyncObject)
		{
			if (Disposed)
			{
				return;
			}
			Disposed = true;
			s_allMeters.Remove(this);
			list = _instruments;
			_instruments = new List<Instrument>();
		}
		if (list == null)
		{
			return;
		}
		foreach (Instrument item in list)
		{
			item.NotifyForUnpublishedInstrument();
		}
	}

	internal bool AddInstrument(Instrument instrument)
	{
		if (!_instruments.Contains(instrument))
		{
			_instruments.Add(instrument);
			return true;
		}
		return false;
	}

	internal static List<Instrument> GetPublishedInstruments()
	{
		List<Instrument> list = null;
		if (s_allMeters.Count > 0)
		{
			list = new List<Instrument>();
			foreach (Meter s_allMeter in s_allMeters)
			{
				foreach (Instrument instrument in s_allMeter._instruments)
				{
					list.Add(instrument);
				}
			}
		}
		return list;
	}
}
