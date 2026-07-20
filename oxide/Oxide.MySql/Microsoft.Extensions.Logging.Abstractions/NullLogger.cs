using System;

namespace Microsoft.Extensions.Logging.Abstractions;

internal class NullLogger : ILogger
{
	public static NullLogger Instance { get; } = new NullLogger();

	private NullLogger()
	{
	}

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
	{
		return NullScope.Instance;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return false;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
	}
}
internal class NullLogger<T> : ILogger<T>, ILogger
{
	public static readonly NullLogger<T> Instance = new NullLogger<T>();

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
	{
		return NullScope.Instance;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return false;
	}
}
