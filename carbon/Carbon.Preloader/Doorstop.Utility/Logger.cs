using System;
using System.IO;
using System.Reflection;
using Carbon.Core;

namespace Doorstop.Utility;

public sealed class Logger
{
	internal enum Severity
	{
		Error,
		Warning,
		Notice,
		Debug,
		None
	}

	private static string logFile;

	public static object locker;

	static Logger()
	{
		logFile = Path.Combine(Defines.GetLogsFolder(), Assembly.GetExecutingAssembly().GetName().Name + ".log");
		locker = new object();
		if (!Directory.Exists(Defines.GetLogsFolder()))
		{
			Directory.CreateDirectory(Defines.GetLogsFolder());
		}
	}

	internal static void Write(Severity severity, object message, Exception ex = null)
	{
		string text = severity switch
		{
			Severity.None => $"{message}", 
			Severity.Notice => $"{message}", 
			Severity.Warning => $"[w] {message}", 
			Severity.Error => $"[e] {message}", 
			Severity.Debug => $"[d] {message}", 
			_ => throw new Exception($"Severity {severity} not implemented."), 
		};
		lock (locker)
		{
			if (ex != null)
			{
				text = text + " (" + ex?.Message + ")\n" + ex?.StackTrace;
			}
			if (severity == Severity.Error || severity == Severity.Notice)
			{
				Console.WriteLine(text);
			}
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}" + Environment.NewLine);
		}
	}

	public static void None(object message)
	{
		Write(Severity.None, message);
	}

	public static void Debug(object message)
	{
		Write(Severity.Debug, message);
	}

	public static void Log(object message)
	{
		Write(Severity.Notice, message);
	}

	public static void Warn(object message)
	{
		Write(Severity.Warning, message);
	}

	public static void Error(object message, Exception ex = null)
	{
		Write(Severity.Error, message, ex);
	}
}
