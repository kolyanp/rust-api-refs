namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
internal sealed class _003C907c15f6_002Ddf07_002D4486_002Da199_002D1944aec67df4_003ENotNullIfNotNullAttribute : Attribute
{
	public string ParameterName { get; }

	public _003C907c15f6_002Ddf07_002D4486_002Da199_002D1944aec67df4_003ENotNullIfNotNullAttribute(string parameterName)
	{
		ParameterName = parameterName;
	}
}
