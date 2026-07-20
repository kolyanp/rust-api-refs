namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
internal sealed class TargetPlatformAttribute : Attribute
{
	public string PlatformName { get; }

	public TargetPlatformAttribute(string platformName)
	{
		PlatformName = platformName;
	}
}
