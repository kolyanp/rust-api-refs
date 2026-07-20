namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class _003C8138f099_002Ddb66_002D4e8d_002Da0f5_002D5476a41f5864_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003C8138f099_002Ddb66_002D4e8d_002Da0f5_002D5476a41f5864_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
