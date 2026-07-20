using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
public sealed class AsyncMethodBuilderAttribute : Attribute
{
	public Type BuilderType { get; }

	public AsyncMethodBuilderAttribute(Type builderType)
	{
		BuilderType = builderType;
	}
}
