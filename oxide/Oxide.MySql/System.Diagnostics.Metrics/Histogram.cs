using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
internal sealed class Histogram<T> : Instrument<T> where T : struct
{
	internal Histogram(Meter meter, string name, string unit, string description)
		: base(meter, name, unit, description)
	{
		Publish();
	}

	public void Record(T value)
	{
		RecordMeasurement(value);
	}

	public void Record(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag)
	{
		RecordMeasurement(value, tag);
	}

	public void Record(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2)
	{
		RecordMeasurement(value, tag1, tag2);
	}

	public void Record(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag1, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag2, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 1, 2 })] KeyValuePair<string, object> tag3)
	{
		RecordMeasurement(value, tag1, tag2, tag3);
	}

	public void Record(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })] ReadOnlySpan<KeyValuePair<string, object>> tags)
	{
		RecordMeasurement(value, tags);
	}

	public void Record(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })] params KeyValuePair<string, object>[] tags)
	{
		RecordMeasurement(value, MemoryExtensions.AsSpan(tags));
	}

	public void Record(T value, [In][_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly] ref TagList tagList)
	{
		RecordMeasurement(value, ref tagList);
	}
}
