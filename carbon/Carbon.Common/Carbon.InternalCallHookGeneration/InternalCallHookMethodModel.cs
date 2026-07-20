using System.Collections.Generic;

namespace Carbon.InternalCallHookGeneration;

public sealed class InternalCallHookMethodModel
{
	public string MethodName { get; set; } = string.Empty;

	public string HookName { get; set; } = string.Empty;

	public uint HookId { get; set; }

	public bool ReturnsVoid { get; set; }

	public int Score { get; set; }

	public string ConditionalSymbol { get; set; } = string.Empty;

	public List<InternalCallHookParameterModel> Parameters { get; } = new List<InternalCallHookParameterModel>();
}
