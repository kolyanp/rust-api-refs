namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class _003Cdf54965f_002Df0e6_002D4e00_002D9532_002D08eb057096c1_003EMaybeNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003Cdf54965f_002Df0e6_002D4e00_002D9532_002D08eb057096c1_003EMaybeNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
