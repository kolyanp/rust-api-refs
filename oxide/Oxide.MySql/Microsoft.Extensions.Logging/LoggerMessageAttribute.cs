using System;

namespace Microsoft.Extensions.Logging;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class LoggerMessageAttribute : Attribute
{
	public int EventId { get; set; } = -1;

	public string? EventName { get; set; }

	public LogLevel Level { get; set; } = LogLevel.None;

	public string Message { get; set; } = "";

	public bool SkipEnabledCheck { get; set; }

	public LoggerMessageAttribute()
	{
	}

	public LoggerMessageAttribute(int eventId, LogLevel level, string message)
	{
		EventId = eventId;
		Level = level;
		Message = message;
	}
}
