using System;

namespace Microsoft.Extensions.Logging;

internal interface ILoggerProvider : IDisposable
{
	ILogger CreateLogger(string categoryName);
}
