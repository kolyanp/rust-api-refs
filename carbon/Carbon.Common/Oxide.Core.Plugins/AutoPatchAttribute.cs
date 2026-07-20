using System;

namespace Oxide.Core.Plugins;

[AttributeUsage(AttributeTargets.Class)]
public class AutoPatchAttribute : Attribute
{
	public enum Orders
	{
		AfterPluginInit,
		AfterPluginLoad,
		AfterOnServerInitialized,
		Delayed
	}

	public bool IsRequired;

	public Orders Order;

	public string PatchSuccessCallback;

	public string PatchFailureCallback;

	public bool Silent;
}
