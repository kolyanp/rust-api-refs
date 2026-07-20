using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
internal struct ActivityEvent
{
	private static readonly IEnumerable<KeyValuePair<string, object>> s_emptyTags = Array.Empty<KeyValuePair<string, object>>();

	private readonly Activity.TagsLinkedList _tags;

	public string Name { get; }

	public DateTimeOffset Timestamp { get; }

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })]
	public IEnumerable<KeyValuePair<string, object>> Tags
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 1, 0, 1, 2 })]
		get
		{
			IEnumerable<KeyValuePair<string, object>> tags = _tags;
			return tags ?? s_emptyTags;
		}
	}

	public ActivityEvent(string name)
		: this(name, DateTimeOffset.UtcNow)
	{
	}

	public ActivityEvent(string name, DateTimeOffset timestamp = default(DateTimeOffset), [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] ActivityTagsCollection tags = null)
	{
		Name = name ?? string.Empty;
		Timestamp = ((timestamp != default(DateTimeOffset)) ? timestamp : DateTimeOffset.UtcNow);
		_tags = ((tags != null && tags.Count > 0) ? new Activity.TagsLinkedList(tags) : null);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public Activity.Enumerator<KeyValuePair<string, object>> EnumerateTagObjects()
	{
		return new Activity.Enumerator<KeyValuePair<string, object>>(_tags?.First);
	}
}
