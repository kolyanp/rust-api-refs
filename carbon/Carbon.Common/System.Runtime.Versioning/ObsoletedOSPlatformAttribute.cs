namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
internal sealed class ObsoletedOSPlatformAttribute : Attribute
{
	public string PlatformName { get; }

	public string? Message { get; set; }

	public string? Url { get; set; }

	public ObsoletedOSPlatformAttribute(string platformName)
	{
		PlatformName = platformName;
	}
}
