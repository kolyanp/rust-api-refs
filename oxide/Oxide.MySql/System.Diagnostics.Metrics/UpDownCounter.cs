using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
internal sealed class UpDownCounter<T> : Instrument<T> where T : struct
{
	internal UpDownCounter(Meter meter, string name, string unit, string description)
		: base(meter, name, unit, description)
	{
		Publish();
	}

	public void Add(T delta)
	{
		RecordMeasurement(delta);
	}

	public void Add(T delta, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag)
	{
		RecordMeasurement(delta, tag);
	}

	public void Add(T delta, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2)
	{
		RecordMeasurement(delta, tag1, tag2);
	}

	public void Add(T delta, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag3)
	{
		RecordMeasurement(delta, tag1, tag2, tag3);
	}

	public void Add(T delta, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })] ReadOnlySpan<KeyValuePair<string, object>> tags)
	{
		RecordMeasurement(delta, tags);
	}

	public void Add(T delta, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })] params KeyValuePair<string, object>[] tags)
	{
		RecordMeasurement(delta, MemoryExtensions.AsSpan(tags));
	}

	public void Add(T delta, [In][_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly] ref TagList tagList)
	{
		RecordMeasurement(delta, ref tagList);
	}
}
