using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
internal struct ActivityCreationOptions<[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] T>
{
	private readonly ActivityTagsCollection _samplerTags;

	private readonly ActivityContext _context;

	private readonly string _traceState;

	public ActivitySource Source { get; }

	public string Name { get; }

	public ActivityKind Kind { get; }

	public T Parent { get; }

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })]
	public IEnumerable<KeyValuePair<string, object>> Tags
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })]
		get;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public IEnumerable<ActivityLink> Links
	{
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
		get;
	}

	public ActivityTagsCollection SamplingTags
	{
		[SecuritySafeCritical]
		get
		{
			if (_samplerTags == null)
			{
				Unsafe.AsRef(ref _samplerTags) = new ActivityTagsCollection();
			}
			return _samplerTags;
		}
	}

	public ActivityTraceId TraceId
	{
		[SecuritySafeCritical]
		get
		{
			if (Parent is ActivityContext && IdFormat == ActivityIdFormat.W3C && _context == default(ActivityContext))
			{
				ActivityTraceId traceId = Activity.TraceIdGenerator?.Invoke() ?? ActivityTraceId.CreateRandom();
				Unsafe.AsRef(ref _context) = new ActivityContext(traceId, default(ActivitySpanId), ActivityTraceFlags.None);
			}
			return _context.TraceId;
		}
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)]
	public string TraceState
	{
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
		[SecuritySafeCritical]
		get
		{
			return _traceState;
		}
		[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
		[SecuritySafeCritical]
		init
		{
			_traceState = value;
		}
	}

	internal ActivityIdFormat IdFormat { get; }

	internal ActivityCreationOptions(ActivitySource source, string name, T parent, ActivityKind kind, IEnumerable<KeyValuePair<string, object>> tags, IEnumerable<ActivityLink> links, ActivityIdFormat idFormat)
	{
		Source = source;
		Name = name;
		Kind = kind;
		Parent = parent;
		Tags = tags;
		Links = links;
		IdFormat = idFormat;
		if (IdFormat == ActivityIdFormat.Unknown && Activity.ForceDefaultIdFormat)
		{
			IdFormat = Activity.DefaultIdFormat;
		}
		_samplerTags = null;
		_traceState = null;
		if (parent is ActivityContext activityContext && activityContext != default(ActivityContext))
		{
			_context = activityContext;
			if (IdFormat == ActivityIdFormat.Unknown)
			{
				IdFormat = ActivityIdFormat.W3C;
			}
			_traceState = activityContext.TraceState;
		}
		else if (parent is string text && text != null)
		{
			if (IdFormat != ActivityIdFormat.Hierarchical)
			{
				if (ActivityContext.TryParse(text, null, out _context))
				{
					IdFormat = ActivityIdFormat.W3C;
				}
				if (IdFormat == ActivityIdFormat.Unknown)
				{
					IdFormat = ActivityIdFormat.Hierarchical;
				}
			}
			else
			{
				_context = default(ActivityContext);
			}
		}
		else
		{
			_context = default(ActivityContext);
			if (IdFormat == ActivityIdFormat.Unknown)
			{
				IdFormat = ((Activity.Current != null) ? Activity.Current.IdFormat : Activity.DefaultIdFormat);
			}
		}
	}

	[SecuritySafeCritical]
	internal void SetTraceState(string traceState)
	{
		Unsafe.AsRef(ref _traceState) = traceState;
	}

	internal ActivityTagsCollection GetSamplingTags()
	{
		return _samplerTags;
	}

	internal ActivityContext GetContext()
	{
		return _context;
	}
}
