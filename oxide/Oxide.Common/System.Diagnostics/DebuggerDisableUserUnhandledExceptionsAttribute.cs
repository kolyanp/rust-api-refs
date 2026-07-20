using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics;

[AttributeUsage(AttributeTargets.Method)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class DebuggerDisableUserUnhandledExceptionsAttribute : Attribute
{
}
