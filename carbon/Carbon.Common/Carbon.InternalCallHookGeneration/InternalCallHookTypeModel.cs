using System.Collections.Generic;

namespace Carbon.InternalCallHookGeneration;

public sealed class InternalCallHookTypeModel
{
	public string NamespaceName { get; set; } = string.Empty;

	public string TypeName { get; set; } = string.Empty;

	public string BaseKind { get; set; } = string.Empty;

	public string VersionOwnerExpression { get; set; } = string.Empty;

	public List<string> GlobalUsings { get; } = new List<string>();

	public List<string> NamespaceUsings { get; } = new List<string>();

	public List<InternalCallHookMethodModel> Methods { get; } = new List<InternalCallHookMethodModel>();
}
