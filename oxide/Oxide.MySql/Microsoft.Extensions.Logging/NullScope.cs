using System;

namespace Microsoft.Extensions.Logging;

internal sealed class NullScope : IDisposable
{
	public static NullScope Instance { get; } = new NullScope();

	private NullScope()
	{
	}

	public void Dispose()
	{
	}
}
