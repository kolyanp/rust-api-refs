using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
internal struct ActivityLink(ActivityContext context, ActivityTagsCollection tags = null) : IEquatable<ActivityLink>
{
	private readonly Activity.TagsLinkedList _tags = ((tags != null && tags.Count > 0) ? new Activity.TagsLinkedList(tags) : null);

	public ActivityContext Context { get; } = context;

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })]
	public IEnumerable<KeyValuePair<string, object>> Tags
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })]
		get
		{
			return _tags;
		}
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	public override bool Equals([_003C8138f099_002Ddb66_002D4e8d_002Da0f5_002D5476a41f5864_003ENotNullWhen(true)] object obj)
	{
		if (obj is ActivityLink value)
		{
			return Equals(value);
		}
		return false;
	}

	public bool Equals(ActivityLink value)
	{
		if (Context == value.Context)
		{
			return value.Tags == Tags;
		}
		return false;
	}

	public static bool operator ==(ActivityLink left, ActivityLink right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ActivityLink left, ActivityLink right)
	{
		return !left.Equals(right);
	}

	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public Activity.Enumerator<KeyValuePair<string, object>> EnumerateTagObjects()
	{
		return new Activity.Enumerator<KeyValuePair<string, object>>(_tags?.First);
	}

	public override int GetHashCode()
	{
		if (this == default(ActivityLink))
		{
			return 0;
		}
		int num = 5381;
		num = (num << 5) + num + Context.GetHashCode();
		if (Tags != null)
		{
			foreach (KeyValuePair<string, object> tag in Tags)
			{
				num = (num << 5) + num + tag.Key.GetHashCode();
				if (tag.Value != null)
				{
					num = (num << 5) + num + tag.Value.GetHashCode();
				}
			}
		}
		return num;
	}
}
