using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Carbon.Core;
using Carbon.Extensions;
using UnityEngine;

namespace Carbon;

public class FileLogger : IDisposable
{
	private readonly object _sync = new object();

	private int _isFlushing;

	private readonly List<string> _buffer = new List<string>();

	private StreamWriter _file;

	public string Name { get; set; } = "default";

	public int SplitSize { get; set; } = 5000000;

	public bool HasInit { get; private set; }

	public int PendingCount
	{
		get
		{
			lock (_sync)
			{
				return _buffer.Count;
			}
		}
	}

	public FileLogger()
	{
	}

	public FileLogger(string name)
	{
		Name = name;
	}

	public virtual void Init(bool archive = false, bool backup = false)
	{
		lock (_sync)
		{
			if (HasInit && !archive)
			{
				return;
			}
			string text = Path.Combine(Defines.GetLogsFolder(), Name + ".log");
			string text2 = Path.Combine(Defines.GetLogsFolder(), "archive");
			bool flag = false;
			OsEx.Folder.Create(text2);
			if (backup && OsEx.File.Exists(text))
			{
				try
				{
					string text3 = Path.Combine(text2, $"{Name}.backup.{DateTime.Now:yyyy.MM.dd}.log");
					string text4 = OsEx.File.ReadText(text);
					if (OsEx.File.Exists(text3))
					{
						File.AppendAllText(text3, text4);
					}
					else
					{
						OsEx.File.Create(text3, text4);
					}
				}
				catch (Exception ex)
				{
					flag = true;
					Debug.LogError((object)("Failed backing up the current log file. Most likely because it's in use. (" + ex.Message + ")\n" + ex.StackTrace));
				}
			}
			if (archive && !flag && OsEx.File.Exists(text))
			{
				OsEx.File.Move(text, Path.Combine(text2, $"{Name}.{DateTime.Now:yyyy.MM.dd.HHmmss}.log"));
			}
			if (!flag)
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
			else
			{
				text = Path.Combine(Defines.GetLogsFolder(), Name + "_locked.log");
			}
			HasInit = true;
			_file = new StreamWriter(text, append: true);
		}
	}

	public virtual void Dispose()
	{
		while (Interlocked.CompareExchange(ref _isFlushing, 1, 0) != 0)
		{
			Thread.Yield();
		}
		try
		{
			lock (_sync)
			{
				if (_file != null)
				{
					_file.Flush();
					_file.Close();
					_file.Dispose();
					_file = null;
				}
				HasInit = false;
			}
		}
		finally
		{
			Interlocked.Exchange(ref _isFlushing, 0);
		}
	}

	public virtual void Flush()
	{
		if (Interlocked.CompareExchange(ref _isFlushing, 1, 0) != 0)
		{
			return;
		}
		try
		{
			while (true)
			{
				lock (_sync)
				{
					if (_file == null || _buffer.Count == 0)
					{
						break;
					}
					int count = _buffer.Count;
					for (int i = 0; i < count; i++)
					{
						_file.WriteLine(_buffer[i]);
					}
					_file.Flush();
					if (_buffer.Count == count)
					{
						_buffer.Clear();
					}
					else
					{
						_buffer.RemoveRange(0, count);
					}
					if (_file.BaseStream.Length > SplitSize)
					{
						_file.Flush();
						_file.Close();
						_file.Dispose();
						_file = null;
						HasInit = false;
						Init(archive: true);
					}
				}
				if (!Community.IsConfigReady || Community.Runtime.Config.Logging.LogFileMode != 2)
				{
					break;
				}
				Thread.Yield();
			}
		}
		finally
		{
			Interlocked.Exchange(ref _isFlushing, 0);
		}
	}

	public virtual void QueueLog(object message)
	{
		if (Community.IsConfigReady && Community.Runtime.Config.Logging.LogFileMode == 0)
		{
			return;
		}
		bool flag = false;
		lock (_sync)
		{
			_buffer.Add($"[{Logger.GetDate()}] {message}");
			if (Community.IsConfigReady && Community.Runtime.Config.Logging.LogFileMode == 2)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Flush();
		}
	}
}
