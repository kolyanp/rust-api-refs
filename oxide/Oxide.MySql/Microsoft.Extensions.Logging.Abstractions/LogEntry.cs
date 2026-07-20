using System;

namespace Microsoft.Extensions.Logging.Abstractions;

internal readonly struct LogEntry<TState>(LogLevel logLevel, string category, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
{
	public LogLevel LogLevel { get; } = logLevel;

	public string Category { get; } = category;

	public EventId EventId { get; } = eventId;

	public TState State { get; } = state;

	public Exception? Exception { get; } = exception;

	public Func<TState, Exception?, string> Formatter { get; } = formatter;
}
