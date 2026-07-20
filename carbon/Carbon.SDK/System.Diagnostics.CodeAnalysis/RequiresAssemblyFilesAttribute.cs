namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
internal sealed class RequiresAssemblyFilesAttribute : Attribute
{
	public string Message { get; }

	public string? Url { get; set; }

	public RequiresAssemblyFilesAttribute(string message)
	{
		Message = message;
	}
}
