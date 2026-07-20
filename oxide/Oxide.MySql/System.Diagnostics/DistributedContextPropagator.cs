using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Diagnostics;

[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
internal abstract class DistributedContextPropagator
{
	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(0)]
	public delegate void PropagatorGetterCallback(object carrier, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string fieldName, out string fieldValue, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })] out IEnumerable<string> fieldValues);

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(0)]
	public delegate void PropagatorSetterCallback([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object carrier, string fieldName, string fieldValue);

	private static DistributedContextPropagator s_current = CreateDefaultPropagator();

	internal const string TraceParent = "traceparent";

	internal const string RequestId = "Request-Id";

	internal const string TraceState = "tracestate";

	internal const string Baggage = "baggage";

	internal const string CorrelationContext = "Correlation-Context";

	internal const char Space = ' ';

	internal const char Tab = '\t';

	internal const char Comma = ',';

	internal const char Semicolon = ';';

	internal const string CommaWithSpace = ", ";

	internal static readonly char[] s_trimmingSpaceCharacters = new char[2] { ' ', '\t' };

	public abstract IReadOnlyCollection<string> Fields { get; }

	public static DistributedContextPropagator Current
	{
		get
		{
			return s_current;
		}
		set
		{
			s_current = value ?? throw new ArgumentNullException("value");
		}
	}

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	public abstract void Inject(Activity activity, object carrier, PropagatorSetterCallback setter);

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	public abstract void ExtractTraceIdAndState(object carrier, PropagatorGetterCallback getter, out string traceId, out string traceState);

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 0, 1, 2 })]
	public abstract IEnumerable<KeyValuePair<string, string>> ExtractBaggage(object carrier, PropagatorGetterCallback getter);

	public static DistributedContextPropagator CreateDefaultPropagator()
	{
		return LegacyPropagator.Instance;
	}

	public static DistributedContextPropagator CreatePassThroughPropagator()
	{
		return PassThroughPropagator.Instance;
	}

	public static DistributedContextPropagator CreateNoOutputPropagator()
	{
		return NoOutputPropagator.Instance;
	}

	internal static void InjectBaggage(object carrier, IEnumerable<KeyValuePair<string, string>> baggage, PropagatorSetterCallback setter)
	{
		using IEnumerator<KeyValuePair<string, string>> enumerator = baggage.GetEnumerator();
		if (enumerator.MoveNext())
		{
			StringBuilder stringBuilder = new StringBuilder();
			do
			{
				KeyValuePair<string, string> current = enumerator.Current;
				stringBuilder.Append(WebUtility.UrlEncode(current.Key)).Append('=').Append(WebUtility.UrlEncode(current.Value))
					.Append(", ");
			}
			while (enumerator.MoveNext());
			setter(carrier, "Correlation-Context", stringBuilder.ToString(0, stringBuilder.Length - 2));
		}
	}
}
