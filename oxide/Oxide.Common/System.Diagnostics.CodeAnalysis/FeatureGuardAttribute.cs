namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class FeatureGuardAttribute : Attribute
{
	public Type FeatureType { get; }

	public FeatureGuardAttribute(Type featureType)
	{
		FeatureType = featureType;
	}
}
