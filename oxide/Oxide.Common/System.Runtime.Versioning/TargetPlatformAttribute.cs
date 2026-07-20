using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class TargetPlatformAttribute : Attribute
{
	public TargetPlatformAttribute(string platformName)
	{
	}
}
