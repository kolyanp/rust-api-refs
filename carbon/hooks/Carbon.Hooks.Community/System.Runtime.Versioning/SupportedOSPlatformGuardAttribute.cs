namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
internal sealed class SupportedOSPlatformGuardAttribute : Attribute
{
	public string PlatformName { get; }

	public SupportedOSPlatformGuardAttribute(string platformName)
	{
		PlatformName = platformName;
	}
}
