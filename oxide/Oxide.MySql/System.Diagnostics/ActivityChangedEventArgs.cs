using System.Runtime.CompilerServices;
using System.Security;

namespace System.Diagnostics;

[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
[SecuritySafeCritical]
internal struct ActivityChangedEventArgs
{
	public Activity Previous { get; init; }

	public Activity Current { get; init; }

	internal ActivityChangedEventArgs(Activity previous, Activity current)
	{
		Previous = previous;
		Current = current;
	}
}
