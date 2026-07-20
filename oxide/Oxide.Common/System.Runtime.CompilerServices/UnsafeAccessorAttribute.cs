using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class UnsafeAccessorAttribute : Attribute
{
	public UnsafeAccessorKind Kind { get; }

	public string? Name { get; set; }

	public UnsafeAccessorAttribute(UnsafeAccessorKind kind)
	{
		Kind = kind;
	}
}
