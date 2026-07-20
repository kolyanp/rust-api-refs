namespace Carbon.InternalCallHookGeneration;

public sealed class InternalCallHookParameterModel
{
	public string TypeName { get; set; } = string.Empty;

	public bool IsOut { get; set; }

	public bool IsRef { get; set; }

	public bool UseInlineDefaultExpression { get; set; }

	public bool RequiresGuard { get; set; }
}
