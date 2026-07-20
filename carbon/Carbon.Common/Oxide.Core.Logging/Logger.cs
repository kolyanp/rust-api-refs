using System;
using Carbon;

namespace Oxide.Core.Logging;

public abstract class Logger
{
	public struct LogMessage
	{
		public LogType Type;

		public string ConsoleMessage;

		public string LogfileMessage;
	}

	protected LogMessage CreateLogMessage(LogType type, string format, object[] args)
	{
		LogMessage result = new LogMessage
		{
			Type = type,
			ConsoleMessage = $"[Carbon] {DateTime.Now.ToShortTimeString()} [{type}] {format}",
			LogfileMessage = $"{DateTime.Now.ToShortTimeString()} [{type}] {format}"
		};
		if (Interface.Oxide.Config.Console.MinimalistMode)
		{
			result.ConsoleMessage = format;
		}
		if (args.Length == 0)
		{
			return result;
		}
		result.ConsoleMessage = string.Format(result.ConsoleMessage, args);
		result.LogfileMessage = string.Format(result.LogfileMessage, args);
		return result;
	}

	public virtual void Write(LogType type, string format, params object[] args)
	{
		Write(CreateLogMessage(type, format, args));
	}

	internal virtual void Write(LogMessage message)
	{
		switch (message.Type)
		{
		case LogType.Info:
		case LogType.Debug:
			Carbon.Logger.Log(message.ConsoleMessage);
			break;
		case LogType.Warning:
			Carbon.Logger.Warn(message.ConsoleMessage);
			break;
		case LogType.Error:
			Carbon.Logger.Error(message.ConsoleMessage);
			break;
		case LogType.Chat:
			break;
		}
	}
}
