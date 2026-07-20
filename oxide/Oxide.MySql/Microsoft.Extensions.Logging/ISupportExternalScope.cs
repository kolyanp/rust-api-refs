namespace Microsoft.Extensions.Logging;

internal interface ISupportExternalScope
{
	void SetScopeProvider(IExternalScopeProvider scopeProvider);
}
