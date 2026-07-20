using System;
using System.IO;
using Carbon.Core;
using Carbon.Extensions;
using ConVar;
using UnityEngine;

namespace Carbon;

public class ServerLog : ILogHandler, IDisposable
{
	internal FileStream _stream;

	internal StreamWriter _writer;

	internal ILogHandler _default = Debug.unityLogger.logHandler;

	public ServerLog()
	{
		_stream = new FileStream("-logfile".GetArgumentResult(Path.Combine(Defines.GetLogsFolder(), "Server." + Server.identity + ".txt")), FileMode.OpenOrCreate, FileAccess.ReadWrite);
		_writer = new StreamWriter(_stream);
		Debug.unityLogger.logHandler = (ILogHandler)(object)this;
	}

	public void LogFormat(LogType logType, Object context, string format, params object[] args)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		_writer.WriteLine(string.Format(format, args));
		_writer.Flush();
		_default.LogFormat(logType, context, format, args);
	}

	public void LogException(Exception exception, Object context)
	{
		_default.LogException(exception, context);
	}

	public void Dispose()
	{
		Debug.unityLogger.logHandler = _default;
	}
}
