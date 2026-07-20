namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class _003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
