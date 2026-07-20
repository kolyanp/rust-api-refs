namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class FeatureSwitchDefinitionAttribute : Attribute
{
	public string SwitchName { get; }

	public FeatureSwitchDefinitionAttribute(string switchName)
	{
		SwitchName = switchName;
	}
}
