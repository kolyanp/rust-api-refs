using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
internal sealed class MeterListener : IDisposable
{
	private static List<MeterListener> s_allStartedListeners = new List<MeterListener>();

	private DiagLinkedList<Instrument> _enabledMeasurementInstruments = new DiagLinkedList<Instrument>();

	private bool _disposed;

	private MeasurementCallback<byte> _byteMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<short> _shortMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<int> _intMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<long> _longMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<float> _floatMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<double> _doubleMeasurementCallback = delegate
	{
	};

	private MeasurementCallback<decimal> _decimalMeasurementCallback = delegate
	{
	};

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 1 })]
	public Action<Instrument, MeterListener> InstrumentPublished
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 1 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 1 })]
		set;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 2 })]
	public Action<Instrument, object> MeasurementsCompleted
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 2 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1, 2 })]
		set;
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public void EnableMeasurementEvents(Instrument instrument, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object state = null)
	{
		bool oldStateStored = false;
		bool flag = false;
		object arg = null;
		lock (Instrument.SyncObject)
		{
			if (instrument != null && !_disposed && !instrument.Meter.Disposed)
			{
				_enabledMeasurementInstruments.AddIfNotExist(instrument, object.ReferenceEquals);
				arg = instrument.EnableMeasurement(new ListenerSubscription(this, state), out oldStateStored);
				flag = true;
			}
		}
		if (flag)
		{
			if (oldStateStored && MeasurementsCompleted != null)
			{
				MeasurementsCompleted?.Invoke(instrument, arg);
			}
		}
		else
		{
			MeasurementsCompleted?.Invoke(instrument, state);
		}
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public object DisableMeasurementEvents(Instrument instrument)
	{
		object obj = null;
		lock (Instrument.SyncObject)
		{
			if (instrument == null || _enabledMeasurementInstruments.Remove(instrument, object.ReferenceEquals) == null)
			{
				return null;
			}
			obj = instrument.DisableMeasurements(this);
		}
		MeasurementsCompleted?.Invoke(instrument, obj);
		return obj;
	}

	public void SetMeasurementEventCallback<T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0 })] MeasurementCallback<T> measurementCallback) where T : struct
	{
		if (measurementCallback == null)
		{
			measurementCallback = delegate
			{
			};
		}
		if (typeof(T) == typeof(byte))
		{
			_byteMeasurementCallback = (MeasurementCallback<byte>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(int))
		{
			_intMeasurementCallback = (MeasurementCallback<int>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(float))
		{
			_floatMeasurementCallback = (MeasurementCallback<float>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(double))
		{
			_doubleMeasurementCallback = (MeasurementCallback<double>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(decimal))
		{
			_decimalMeasurementCallback = (MeasurementCallback<decimal>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(short))
		{
			_shortMeasurementCallback = (MeasurementCallback<short>)(object)measurementCallback;
			return;
		}
		if (typeof(T) == typeof(long))
		{
			_longMeasurementCallback = (MeasurementCallback<long>)(object)measurementCallback;
			return;
		}
		throw new InvalidOperationException(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.Format(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.UnsupportedType, typeof(T)));
	}

	public void Start()
	{
		List<Instrument> list = null;
		lock (Instrument.SyncObject)
		{
			if (_disposed)
			{
				return;
			}
			if (!s_allStartedListeners.Contains(this))
			{
				s_allStartedListeners.Add(this);
				list = Meter.GetPublishedInstruments();
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (Instrument item in list)
		{
			InstrumentPublished?.Invoke(item, this);
		}
	}

	public void RecordObservableInstruments()
	{
		List<Exception> list = null;
		for (DiagNode<Instrument> diagNode = _enabledMeasurementInstruments.First; diagNode != null; diagNode = diagNode.Next)
		{
			if (diagNode.Value.IsObservable)
			{
				try
				{
					diagNode.Value.Observe(this);
				}
				catch (Exception item)
				{
					if (list == null)
					{
						list = new List<Exception>();
					}
					list.Add(item);
				}
			}
		}
		if (list != null)
		{
			throw new AggregateException(list);
		}
	}

	public void Dispose()
	{
		Dictionary<Instrument, object> dictionary = null;
		Action<Instrument, object> measurementsCompleted = MeasurementsCompleted;
		lock (Instrument.SyncObject)
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			s_allStartedListeners.Remove(this);
			DiagNode<Instrument> diagNode = _enabledMeasurementInstruments.First;
			if (diagNode != null && measurementsCompleted != null)
			{
				dictionary = new Dictionary<Instrument, object>();
				do
				{
					object value = diagNode.Value.DisableMeasurements(this);
					dictionary.Add(diagNode.Value, value);
					diagNode = diagNode.Next;
				}
				while (diagNode != null);
				_enabledMeasurementInstruments.Clear();
			}
		}
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<Instrument, object> item in dictionary)
		{
			measurementsCompleted?.Invoke(item.Key, item.Value);
		}
	}

	internal static List<MeterListener> GetAllListeners()
	{
		if (s_allStartedListeners.Count != 0)
		{
			return new List<MeterListener>(s_allStartedListeners);
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void NotifyMeasurement<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object>> tags, object state) where T : struct
	{
		if (typeof(T) == typeof(byte))
		{
			_byteMeasurementCallback(instrument, (byte)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(short))
		{
			_shortMeasurementCallback(instrument, (short)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(int))
		{
			_intMeasurementCallback(instrument, (int)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(long))
		{
			_longMeasurementCallback(instrument, (long)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(float))
		{
			_floatMeasurementCallback(instrument, (float)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(double))
		{
			_doubleMeasurementCallback(instrument, (double)(object)measurement, tags, state);
		}
		if (typeof(T) == typeof(decimal))
		{
			_decimalMeasurementCallback(instrument, (decimal)(object)measurement, tags, state);
		}
	}
}
