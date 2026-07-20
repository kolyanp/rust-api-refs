using System;

namespace Microsoft.Extensions.Logging;

internal interface ILoggerFactory : IDisposable
{
	ILogger CreateLogger(string categoryName);

	void AddProvider(ILoggerProvider provider);
}
