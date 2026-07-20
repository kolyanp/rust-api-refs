namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, Inherited = false, AllowMultiple = false)]
[ExcludeFromCodeCoverage]
[Conditional("MULTI_TARGETING_SUPPORT_ATTRIBUTES")]
public sealed class RequiresAssemblyFilesAttribute : Attribute
{
	public string? Message { get; }

	public string? Url { get; set; }

	public RequiresAssemblyFilesAttribute()
	{
	}

	public RequiresAssemblyFilesAttribute(string message)
	{
		Message = message;
	}
}
