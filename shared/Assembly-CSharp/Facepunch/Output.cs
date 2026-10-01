using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Facepunch.Math;
using UnityEngine;

namespace Facepunch;

public static class Output
{
	public struct Entry
	{
		public string Message;

		public string Stacktrace;

		public string Type;

		public int Time;
	}

	private struct ThreadedEntry
	{
		public string Message;

		public string Stacktrace;

		public LogType Type;

		public bool BackgroundThread;
	}

	private static readonly ConcurrentQueue<ThreadedEntry> threadedLogs = new ConcurrentQueue<ThreadedEntry>();

	private const int MaxQueuedThreadedLogs = 10000;

	private static int MainThreadId;

	private static int logHandlerDepth;

	public static Action<string, string, LogType> OnBackgroundThreadMessage = ExceptionReporter.ReportBackgroundThread;

	public static bool installed = false;

	public static Queue<Entry> HistoryOutput = new Queue<Entry>();

	private static readonly Memoized<string, LogType> LogTypeToString = new Memoized<string, LogType>((Func<LogType, string>)((LogType type) => ((object)type/*cast due to constrained. prefix*/).ToString()));

	public static event Action<string, string, LogType> OnMessage;

	public static event Action<string, string, LogType> OnPostMessage;

	public static void Install()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected Obj, but got Unknown
		if (!installed)
		{
			MainThreadId = Thread.CurrentThread.ManagedThreadId;
			Application.logMessageReceived += LogHandler;
			Application.logMessageReceivedThreaded += ThreadedLogHandler;
			PreUpdateHook.OnUpdate = (Action)Delegate.Combine(PreUpdateHook.OnUpdate, new Action(ProcessThreadedLogs));
			TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;
			installed = true;
		}
	}

	private static void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
	{
		if (e == null)
		{
			return;
		}
		try
		{
			foreach (Exception innerException in e.Exception.Flatten().InnerExceptions)
			{
				for (Exception ex = innerException; ex != null; ex = ex.InnerException)
				{
					Debug.LogException(ex);
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			e.SetObserved();
		}
	}

	private static void ThreadedLogHandler(string log, string stacktrace, LogType type)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (Thread.CurrentThread.ManagedThreadId != MainThreadId)
		{
			PushToQueue(log, stacktrace, type, backgroundThread: true);
		}
	}

	private static void PushToQueue(string log, string stacktrace, LogType type, bool backgroundThread)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (threadedLogs.Count < 10000)
		{
			threadedLogs.Enqueue(new ThreadedEntry
			{
				Message = log,
				Stacktrace = stacktrace,
				Type = type,
				BackgroundThread = backgroundThread
			});
		}
	}

	public static void ProcessThreadedLogs()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		int count = threadedLogs.Count;
		for (int i = 0; i < count; i++)
		{
			if (!threadedLogs.TryDequeue(out var result))
			{
				break;
			}
			LogType type = result.Type;
			if (result.BackgroundThread)
			{
				OnBackgroundThreadMessage?.Invoke(result.Message, result.Stacktrace, type);
			}
			LogHandler(result.Message, result.Stacktrace, type);
		}
	}

	public static void LogHandler(string log, string stacktrace, LogType type)
	{
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if ((OnMessage == null && OnPostMessage == null) || log.StartsWith("Kinematic body only supports Speculative Continuous collision detection") || log.StartsWith("Non-convex MeshCollider with non-kinematic Rigidbody is no longer supported") || log.StartsWith("Too many layers used to exclude objects from lighting") || log.StartsWith("[CrashSessionMarker]") || log.StartsWith("Skipped frame because GfxDevice") || log.StartsWith("Your current multi-scene setup has inconsistent Lighting") || log.Contains("HandleD3DDeviceLost") || log.Contains("ResetD3DDevice") || log.Contains("dev->Reset") || log.Contains("D3Dwindow device not lost anymore") || log.Contains("D3D device reset") || log.Contains("group < 0xfff") || log.Contains("Mesh can not have more than 65000 vert") || log.Contains("Trying to add (Layout Rebuilder for)") || log.Contains("Coroutine continue failure") || log.Contains("No texture data available to upload") || log.Contains("Trying to reload asset from disk that is not") || log.Contains("Unable to find shaders used for the terrain engine.") || log.Contains("Canvas element contains more than 65535 vertices") || log.Contains("RectTransform.set_anchorMin") || log.Contains("FMOD failed to initialize the output device") || log.Contains("Cannot create FMOD::Sound") || log.Contains("invalid utf-16 sequence") || log.Contains("missing surrogate tail") || log.Contains("Failed to create agent because it is not close enough to the Nav") || log.Contains("user-provided triangle mesh descriptor is invalid") || log.Contains("Releasing render texture that is set as") || log.Contains("AsyncResourceUpload failed.") || log.StartsWith("Missing shader") || log.StartsWith("ERROR: Shader") || log.StartsWith("Warning: Shader") || log.StartsWith("Shader '") || log.StartsWith("The shader Hidden") || log.StartsWith("HDR Render Texture not supported"))
		{
			return;
		}
		if (logHandlerDepth > 0)
		{
			PushToQueue(log, stacktrace, type, backgroundThread: false);
			return;
		}
		logHandlerDepth++;
		try
		{
			using (TimeWarning.New("Facepunch.Output.LogHandler"))
			{
				try
				{
					OnMessage?.Invoke(log, stacktrace, type);
				}
				catch (Exception)
				{
				}
				try
				{
					OnPostMessage?.Invoke(log, stacktrace, type);
				}
				catch (Exception)
				{
				}
			}
		}
		finally
		{
			logHandlerDepth--;
		}
		Entry item = new Entry
		{
			Message = log,
			Stacktrace = stacktrace,
			Type = LogTypeToString.Get(type),
			Time = Epoch.Current
		};
		HistoryOutput.Enqueue(item);
		while (HistoryOutput.Count > 1024)
		{
			HistoryOutput.Dequeue();
		}
	}
}
