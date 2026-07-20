using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
[SecuritySafeCritical]
internal abstract class Instrument
{
	internal readonly DiagLinkedList<ListenerSubscription> _subscriptions = new DiagLinkedList<ListenerSubscription>();

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })]
	internal static KeyValuePair<string, object>[] EmptyTags => Array.Empty<KeyValuePair<string, object>>();

	internal static object SyncObject { get; } = new object();

	public Meter Meter { get; }

	public string Name { get; }

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public string Description
	{
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
		get;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public string Unit
	{
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
		get;
	}

	public bool Enabled => _subscriptions.First != null;

	public virtual bool IsObservable => false;

	protected Instrument(Meter meter, string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string unit, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string description)
	{
		Meter = meter ?? throw new ArgumentNullException("meter");
		Name = name ?? throw new ArgumentNullException("name");
		Description = description;
		Unit = unit;
	}

	protected void Publish()
	{
		List<MeterListener> list = null;
		lock (SyncObject)
		{
			if (Meter.Disposed || !Meter.AddInstrument(this))
			{
				return;
			}
			list = MeterListener.GetAllListeners();
		}
		if (list == null)
		{
			return;
		}
		foreach (MeterListener item in list)
		{
			item.InstrumentPublished?.Invoke(this, item);
		}
	}

	internal void NotifyForUnpublishedInstrument()
	{
		for (DiagNode<ListenerSubscription> diagNode = _subscriptions.First; diagNode != null; diagNode = diagNode.Next)
		{
			diagNode.Value.Listener.DisableMeasurementEvents(this);
		}
		_subscriptions.Clear();
	}

	internal static void ValidateTypeParameter<T>()
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle != typeof(byte) && typeFromHandle != typeof(short) && typeFromHandle != typeof(int) && typeFromHandle != typeof(long) && typeFromHandle != typeof(double) && typeFromHandle != typeof(float) && typeFromHandle != typeof(decimal))
		{
			throw new InvalidOperationException(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.Format(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.UnsupportedType, typeFromHandle));
		}
	}

	internal object EnableMeasurement(ListenerSubscription subscription, out bool oldStateStored)
	{
		oldStateStored = false;
		if (!_subscriptions.AddIfNotExist(subscription, (ListenerSubscription s1, ListenerSubscription s2) => s1.Listener == s2.Listener))
		{
			ListenerSubscription listenerSubscription = _subscriptions.Remove(subscription, (ListenerSubscription s1, ListenerSubscription s2) => s1.Listener == s2.Listener);
			_subscriptions.AddIfNotExist(subscription, (ListenerSubscription s1, ListenerSubscription s2) => s1.Listener == s2.Listener);
			oldStateStored = listenerSubscription.Listener == subscription.Listener;
			return listenerSubscription.State;
		}
		return false;
	}

	internal object DisableMeasurements(MeterListener listener)
	{
		return _subscriptions.Remove(new ListenerSubscription(listener), (ListenerSubscription s1, ListenerSubscription s2) => s1.Listener == s2.Listener).State;
	}

	internal virtual void Observe(MeterListener listener)
	{
		throw new InvalidOperationException();
	}

	internal object GetSubscriptionState(MeterListener listener)
	{
		for (DiagNode<ListenerSubscription> diagNode = _subscriptions.First; diagNode != null; diagNode = diagNode.Next)
		{
			if (listener == diagNode.Value.Listener)
			{
				return diagNode.Value.State;
			}
		}
		return null;
	}
}
[SecuritySafeCritical]
internal abstract class Instrument<T> : Instrument where T : struct
{
	[ThreadStatic]
	private static KeyValuePair<string, object>[] ts_tags;

	private const int MaxTagsCount = 8;

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	protected Instrument(Meter meter, string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string unit, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string description)
		: base(meter, name, unit, description)
	{
		Instrument.ValidateTypeParameter<T>();
	}

	protected void RecordMeasurement(T measurement)
	{
		RecordMeasurement(measurement, MemoryExtensions.AsSpan(Instrument.EmptyTags));
	}

	protected void RecordMeasurement(T measurement, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })] ReadOnlySpan<KeyValuePair<string, object>> tags)
	{
		for (DiagNode<ListenerSubscription> diagNode = _subscriptions.First; diagNode != null; diagNode = diagNode.Next)
		{
			diagNode.Value.Listener.NotifyMeasurement(this, measurement, tags, diagNode.Value.State);
		}
	}

	protected void RecordMeasurement(T measurement, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag)
	{
		KeyValuePair<string, object>[] array = ts_tags ?? new KeyValuePair<string, object>[8];
		ts_tags = null;
		array[0] = tag;
		RecordMeasurement(measurement, MemoryExtensions.AsSpan(array).Slice(0, 1));
		ts_tags = array;
	}

	protected void RecordMeasurement(T measurement, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2)
	{
		KeyValuePair<string, object>[] array = ts_tags ?? new KeyValuePair<string, object>[8];
		ts_tags = null;
		array[0] = tag1;
		array[1] = tag2;
		RecordMeasurement(measurement, MemoryExtensions.AsSpan(array).Slice(0, 2));
		ts_tags = array;
	}

	protected void RecordMeasurement(T measurement, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag3)
	{
		KeyValuePair<string, object>[] array = ts_tags ?? new KeyValuePair<string, object>[8];
		ts_tags = null;
		array[0] = tag1;
		array[1] = tag2;
		array[2] = tag3;
		RecordMeasurement(measurement, MemoryExtensions.AsSpan(array).Slice(0, 3));
		ts_tags = array;
	}

	protected void RecordMeasurement(T measurement, [In][_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly] ref TagList tagList)
	{
		KeyValuePair<string, object>[] tags = tagList.Tags;
		if (tags != null)
		{
			RecordMeasurement(measurement, MemoryExtensions.AsSpan(tags).Slice(0, tagList.Count));
			return;
		}
		tags = ts_tags ?? new KeyValuePair<string, object>[8];
		switch (tagList.Count)
		{
		default:
			return;
		case 8:
			tags[7] = tagList.Tag8;
			goto case 7;
		case 7:
			tags[6] = tagList.Tag7;
			goto case 6;
		case 6:
			tags[5] = tagList.Tag6;
			goto case 5;
		case 5:
			tags[4] = tagList.Tag5;
			goto case 4;
		case 4:
			tags[3] = tagList.Tag4;
			goto case 3;
		case 3:
			tags[2] = tagList.Tag3;
			goto case 2;
		case 2:
			tags[1] = tagList.Tag2;
			break;
		case 1:
			break;
		case 0:
			return;
		}
		tags[0] = tagList.Tag1;
		ts_tags = null;
		RecordMeasurement(measurement, MemoryExtensions.AsSpan(tags).Slice(0, tagList.Count));
		ts_tags = tags;
	}
}
