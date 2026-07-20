using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
internal sealed class ActivityListener : IDisposable
{
	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
	public Action<Activity> ActivityStarted
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
	public Action<Activity> ActivityStopped
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
	public Func<ActivitySource, bool> ShouldListenTo
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
	public SampleActivity<string> SampleUsingParentId
	{
		[return: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	public SampleActivity<ActivityContext> Sample { get; set; }

	public void Dispose()
	{
		ActivitySource.DetachListener(this);
	}
}
