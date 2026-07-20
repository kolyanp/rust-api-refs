using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
internal struct ActivityContext(ActivityTraceId traceId, ActivitySpanId spanId, ActivityTraceFlags traceFlags, string traceState = null, bool isRemote = false) : IEquatable<ActivityContext>
{
	public ActivityTraceId TraceId { get; } = traceId;

	public ActivitySpanId SpanId { get; } = spanId;

	public ActivityTraceFlags TraceFlags { get; } = traceFlags;

	public string TraceState { get; } = traceState;

	public bool IsRemote { get; } = isRemote;

	public static bool TryParse(string traceParent, string traceState, bool isRemote, out ActivityContext context)
	{
		if (traceParent == null)
		{
			context = default(ActivityContext);
			return false;
		}
		return Activity.TryConvertIdToContext(traceParent, traceState, isRemote, out context);
	}

	public static bool TryParse(string traceParent, string traceState, out ActivityContext context)
	{
		return TryParse(traceParent, traceState, isRemote: false, out context);
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
	public static ActivityContext Parse(string traceParent, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] string traceState)
	{
		if (traceParent == null)
		{
			throw new ArgumentNullException("traceParent");
		}
		if (!Activity.TryConvertIdToContext(traceParent, traceState, isRemote: false, out var context))
		{
			throw new ArgumentException(_003C98aa27ce_002Da3b7_002D4f67_002D9b18_002D59078b0717c9_003ESR.InvalidTraceParent);
		}
		return context;
	}

	public bool Equals(ActivityContext value)
	{
		if (SpanId.Equals(value.SpanId) && TraceId.Equals(value.TraceId) && TraceFlags == value.TraceFlags && TraceState == value.TraceState)
		{
			return IsRemote == value.IsRemote;
		}
		return false;
	}

	public override bool Equals([_003C8138f099_002Ddb66_002D4e8d_002Da0f5_002D5476a41f5864_003ENotNullWhen(true)] object obj)
	{
		if (!(obj is ActivityContext value))
		{
			return false;
		}
		return Equals(value);
	}

	public static bool operator ==(ActivityContext left, ActivityContext right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ActivityContext left, ActivityContext right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		if (this == default(ActivityContext))
		{
			return 0;
		}
		int num = 5381;
		num = (num << 5) + num + TraceId.GetHashCode();
		num = (num << 5) + num + SpanId.GetHashCode();
		num = (int)((num << 5) + num + TraceFlags);
		return (num << 5) + num + ((TraceState != null) ? TraceState.GetHashCode() : 0);
	}
}
