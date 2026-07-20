using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using API.Logger;
using Carbon.Extensions;
using UnityEngine;

namespace Carbon;

public sealed class Logger : ILogger
{
	public static FileLogger CoreLog { get; set; }

	public static Action<string, Exception, int> OnErrorCallback { get; set; }

	public static Action<string, int> OnWarningCallback { get; set; }

	public static Action<string, int> OnNoticeCallback { get; set; }

	public static Action<string, int> OnDebugCallback { get; set; }

	public static void InitTaskExceptions()
	{
		TaskScheduler.UnobservedTaskException += delegate(object _, UnobservedTaskExceptionEventArgs args)
		{
			args.SetObserved();
			Error("Unobserved task exception [GC]", args.Exception.InnerException);
		};
	}

	public static string GetDate()
	{
		return DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
	}

	public static void Write(Severity severity, object message, Exception ex = null, int verbosity = 1, bool nativeLog = true)
	{
		if (CoreLog == null)
		{
			CoreLog = new FileLogger("Carbon.Core");
		}
		CoreLog.Init(archive: false, backup: true);
		if (severity != Severity.Debug)
		{
			Severity severity2 = Community.Runtime?.Config?.Logging.LogSeverity ?? Severity.Notice;
			if (severity > severity2)
			{
				return;
			}
		}
		string text = message?.ToString();
		if (!ThreadEx.IsOnMainThread())
		{
			Thread currentThread = Thread.CurrentThread;
			text += $" [{currentThread.Name}|{currentThread.ManagedThreadId}]";
		}
		switch (severity)
		{
		case Severity.Error:
		{
			Exception ex2 = ((ex != null) ? ExceptionExtensions.Demystify<Exception>(ex) : null) ?? ex;
			if (ex2 != null)
			{
				string text2 = "(" + ex2?.Message + ")\n" + ex2.GetFullStackTrace(mainMessage: false);
				CoreLog.QueueLog("[ERRO] " + text + " " + text2);
				if (nativeLog)
				{
					PrintLog(text + " " + text2, severity);
				}
			}
			else
			{
				CoreLog.QueueLog("[ERRO] " + text);
				if (nativeLog)
				{
					PrintLog(text, severity);
				}
			}
			OnErrorCallback?.Invoke(text, ex2, verbosity);
			break;
		}
		case Severity.Warning:
			CoreLog.QueueLog("[WARN] " + text);
			if (nativeLog)
			{
				PrintLog(text, severity);
			}
			OnWarningCallback?.Invoke(text, verbosity);
			break;
		case Severity.Notice:
			CoreLog.QueueLog("[INFO] " + text);
			if (nativeLog)
			{
				PrintLog(text, severity);
			}
			OnNoticeCallback?.Invoke(text, verbosity);
			break;
		case Severity.Debug:
		{
			int num = Community.Runtime?.Config?.Logging.LogVerbosity ?? (-1);
			if (verbosity <= num)
			{
				CoreLog.QueueLog("[INFO] " + text);
				if (nativeLog)
				{
					PrintLog(text, severity);
				}
				OnDebugCallback?.Invoke(text, verbosity);
			}
			break;
		}
		default:
			throw new Exception($"Severity {severity} not implemented.");
		}
	}

	private static void PrintLog(string text, Severity severity)
	{
		if (!ThreadEx.IsOnMainThread())
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			switch (severity)
			{
			case Severity.Error:
				foregroundColor = ConsoleColor.Red;
				break;
			case Severity.Warning:
				foregroundColor = ConsoleColor.Yellow;
				break;
			case Severity.Notice:
			case Severity.Debug:
				foregroundColor = ConsoleColor.Gray;
				break;
			}
			ConsoleColor foregroundColor2 = Console.ForegroundColor;
			Console.ForegroundColor = foregroundColor;
			Console.WriteLine(text);
			Console.ForegroundColor = foregroundColor2;
		}
		switch (severity)
		{
		case Severity.Error:
			Debug.LogError((object)text);
			break;
		case Severity.Warning:
			Debug.LogWarning((object)text);
			break;
		case Severity.Notice:
		case Severity.Debug:
			Debug.Log((object)text);
			break;
		}
	}

	public static void Dispose()
	{
		CoreLog.Dispose();
		CoreLog = null;
	}

	public static void Debug(object header, object message, int layer)
	{
		Write(Severity.Debug, $"[CRBN.{header}] {message}", null, layer);
	}

	public static void Debug(object message, int layer)
	{
		Write(Severity.Debug, $"[CRBN] {message}", null, layer);
	}

	public static void Debug(object header, object message)
	{
		Write(Severity.Debug, $"[CRBN.{header}] {message}");
	}

	public static void Debug(object message)
	{
		Write(Severity.Debug, $"[CRBN] {message}");
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

	void ILogger.Console(string message, Severity severity, Exception exception)
	{
		switch (severity)
		{
		case Severity.Error:
			Error(message, exception);
			break;
		case Severity.Warning:
			Warn(message);
			break;
		case Severity.Debug:
			Debug(message);
			break;
		default:
			Log(message);
			break;
		}
	}
}
