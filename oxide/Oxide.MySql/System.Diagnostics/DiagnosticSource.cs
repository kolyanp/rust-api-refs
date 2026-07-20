using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Diagnostics;

[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(1)]
[_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(0)]
internal abstract class DiagnosticSource
{
	internal const string WriteRequiresUnreferencedCode = "The type of object being written to DiagnosticSource cannot be discovered statically.";

	[RequiresUnreferencedCode("The type of object being written to DiagnosticSource cannot be discovered statically.")]
	public abstract void Write(string name, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object value);

	public abstract bool IsEnabled(string name);

	[_003C6c14b95c_002D4a6b_002D457d_002D94a2_002D080f46b2b478_003ENullableContext(2)]
	public virtual bool IsEnabled([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] string name, object arg1, object arg2 = null)
	{
		return IsEnabled(name);
	}

	[RequiresUnreferencedCode("The type of object being written to DiagnosticSource cannot be discovered statically.")]
	public Activity StartActivity(Activity activity, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object args)
	{
		activity.Start();
		Write(activity.OperationName + ".Start", args);
		return activity;
	}

	[RequiresUnreferencedCode("The type of object being written to DiagnosticSource cannot be discovered statically.")]
	public void StopActivity(Activity activity, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object args)
	{
		if (activity.Duration == TimeSpan.Zero)
		{
			activity.SetEndTime(Activity.GetUtcNow());
		}
		Write(activity.OperationName + ".Stop", args);
		activity.Stop();
	}

	public virtual void OnActivityImport(Activity activity, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object payload)
	{
	}

	public virtual void OnActivityExport(Activity activity, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object payload)
	{
	}
}
