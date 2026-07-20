namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Struct, Inherited = false)]
internal sealed class InlineArrayAttribute : Attribute
{
	public int Length { get; }

	public InlineArrayAttribute(int length)
	{
		Length = length;
	}
}
