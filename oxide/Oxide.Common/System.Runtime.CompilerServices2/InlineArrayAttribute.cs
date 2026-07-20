using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices2;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class InlineArrayAttribute : Attribute
{
	public int Length { get; }

	public InlineArrayAttribute(int length)
	{
		Length = length;
	}
}
