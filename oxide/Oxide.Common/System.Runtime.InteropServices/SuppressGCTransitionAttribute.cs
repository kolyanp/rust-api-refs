using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class SuppressGCTransitionAttribute : Attribute
{
}
