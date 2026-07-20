using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics.Metrics;

[SecuritySafeCritical]
[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
internal struct Measurement<T> where T : struct
{
	private readonly KeyValuePair<string, object>[] _tags;

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public ReadOnlySpan<KeyValuePair<string, object>> Tags
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })]
		get
		{
			return MemoryExtensions.AsSpan(_tags);
		}
	}

	public T Value { get; }

	public Measurement(T value)
	{
		_tags = Instrument.EmptyTags;
		Value = value;
	}

	public Measurement(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })] IEnumerable<KeyValuePair<string, object>> tags)
	{
		_tags = ToArray(tags);
		Value = value;
	}

	public Measurement(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })] params KeyValuePair<string, object>[] tags)
	{
		if (tags != null)
		{
			_tags = new KeyValuePair<string, object>[tags.Length];
			tags.CopyTo(_tags, 0);
		}
		else
		{
			_tags = Instrument.EmptyTags;
		}
		Value = value;
	}

	public Measurement(T value, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })] ReadOnlySpan<KeyValuePair<string, object>> tags)
	{
		_tags = tags.ToArray();
		Value = value;
	}

	private static KeyValuePair<string, object>[] ToArray(IEnumerable<KeyValuePair<string, object>> tags)
	{
		if (tags != null)
		{
			return new List<KeyValuePair<string, object>>(tags).ToArray();
		}
		return Instrument.EmptyTags;
	}
}
