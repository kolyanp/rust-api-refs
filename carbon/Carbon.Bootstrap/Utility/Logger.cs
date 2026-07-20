using System;
using System.IO;
using System.Reflection;
using Carbon.Extensions;

namespace Utility;

internal sealed class Logger
{
	internal enum Severity
	{
		Error,
		Warning,
		Notice,
		Debug,
		None
	}

	private static readonly string logFile;

	public static object Lock;

	static Logger()
	{
		logFile = Path.Combine(Context.CarbonLogs, Assembly.GetExecutingAssembly().GetName().Name + ".log");
		Lock = new object();
		if (!Directory.Exists(Context.CarbonLogs))
		{
			Directory.CreateDirectory(Context.CarbonLogs);
		}
	}

	internal static void Write(Severity severity, object message, Exception ex = null)
	{
		string arg = severity switch
		{
			Severity.None => $"{message}", 
			Severity.Notice => $"{message}", 
			Severity.Warning => $"[w] {message}", 
			Severity.Error => string.Format("[e] {0}{1}", message, (ex == null) ? string.Empty : (" (" + ex.Message + ")\n" + ex.GetFullStackTrace(mainMessage: false))), 
			Severity.Debug => $"[d] {message}", 
			_ => throw new Exception($"Severity {severity} not implemented."), 
		};
		lock (Lock)
		{
			File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {arg}" + Environment.NewLine);
		}
	}

	internal static void None(object message)
	{
		Write(Severity.None, message);
	}

	internal static void Log(object message)
	{
		Write(Severity.Notice, message);
	}

	internal static void Warn(object message)
	{
		Write(Severity.Warning, message);
	}

	internal static void Error(object message, Exception ex = null)
	{
		Write(Severity.Error, message, ex);
	}
}
