using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using API.Abstracts;
using API.Assembly;
using Carbon.Extensions;
using UnityEngine;
using Utility;

namespace Components;

internal sealed class FileWatcherManager : CarbonBehaviour, IFileWatcherManager, IDisposable
{
	private sealed class WatchEntry
	{
		public WatchFolder Config;

		public FileSystemWatcher Handler;
	}

	private readonly ConcurrentDictionary<FileSystemWatcher, WatchEntry> _byHandler = new ConcurrentDictionary<FileSystemWatcher, WatchEntry>();

	private bool _disposing;

	internal void Awake()
	{
		((Behaviour)this).enabled = false;
	}

	internal void OnEnable()
	{
		foreach (WatchEntry value in _byHandler.Values)
		{
			DispatchInitialEvents(value);
			value.Handler.EnableRaisingEvents = true;
		}
	}

	internal void OnDisable()
	{
		foreach (WatchEntry value in _byHandler.Values)
		{
			value.Handler.EnableRaisingEvents = false;
		}
	}

	internal void OnDestroy()
	{
		Dispose();
	}

	private void DispatchInitialEvents(WatchEntry entry)
	{
		Action<WatchFileEvent> onEvent = entry.Config.OnEvent;
		if (onEvent == null)
		{
			return;
		}
		foreach (string item in Directory.EnumerateFiles(entry.Handler.Path, entry.Handler.Filter))
		{
			try
			{
				onEvent(new WatchFileEvent(WatcherChangeTypes.Created, item, null, isInitial: true));
			}
			catch (Exception ex)
			{
				Logger.Error("Initial event dispatch failed for '" + item + "'", ex);
			}
		}
	}

	internal void FileSystemEvent(object sender, FileSystemEventArgs e)
	{
		if (!(sender is FileSystemWatcher key) || !_byHandler.TryGetValue(key, out var value))
		{
			return;
		}
		Action<WatchFileEvent> onEvent = value.Config.OnEvent;
		if (onEvent == null)
		{
			return;
		}
		try
		{
			string oldPath = ((e is RenamedEventArgs e2) ? e2.OldFullPath : null);
			onEvent(new WatchFileEvent(e.ChangeType, e.FullPath, oldPath, isInitial: false));
		}
		catch (Exception ex)
		{
			Logger.Error("Watcher dispatch failed for '" + e.FullPath + "'", ex);
		}
	}

	public void Watch(WatchFolder item)
	{
		FileSystemWatcher fileSystemWatcher;
		try
		{
			if (string.IsNullOrEmpty(item.Filter))
			{
				throw new ArgumentException("No filter defined");
			}
			if (string.IsNullOrEmpty(item.Directory) || !Directory.Exists(item.Directory))
			{
				throw new Exception("Unable to watch '" + item.Directory + "'");
			}
			string b = PathEx.NormalizePath(item.Directory);
			foreach (WatchEntry value in _byHandler.Values)
			{
				if (PathEx.Equals(PathEx.NormalizePath(value.Config.Directory), b) && PathEx.Equals(value.Config.Filter, item.Filter))
				{
					throw new InvalidOperationException("Already watching '" + item.Directory + "' with filter '" + item.Filter + "'");
				}
			}
			fileSystemWatcher = new FileSystemWatcher(item.Directory)
			{
				Filter = item.Filter,
				NotifyFilter = (NotifyFilters.FileName | NotifyFilters.LastWrite),
				IncludeSubdirectories = item.IncludeSubFolders,
				InternalBufferSize = 65536,
				EnableRaisingEvents = false
			};
			fileSystemWatcher.Changed += FileSystemEvent;
			fileSystemWatcher.Created += FileSystemEvent;
			fileSystemWatcher.Renamed += FileSystemEvent;
			fileSystemWatcher.Deleted += FileSystemEvent;
			fileSystemWatcher.Error += OnWatcherError;
		}
		catch (Exception ex)
		{
			Logger.Error("Unable to instantiate a new folder watcher", ex);
			throw;
		}
		_byHandler[fileSystemWatcher] = new WatchEntry
		{
			Config = item,
			Handler = fileSystemWatcher
		};
	}

	public void Unwatch(WatchFolder item)
	{
		if (item == null)
		{
			return;
		}
		FileSystemWatcher fileSystemWatcher = null;
		foreach (KeyValuePair<FileSystemWatcher, WatchEntry> item2 in _byHandler)
		{
			if (item2.Value.Config == item)
			{
				fileSystemWatcher = item2.Key;
				break;
			}
		}
		if (fileSystemWatcher != null)
		{
			UnwatchInternal(fileSystemWatcher);
			_byHandler.TryRemove(fileSystemWatcher, out var _);
		}
	}

	public void Unwatch(string directory)
	{
		if (string.IsNullOrEmpty(directory))
		{
			return;
		}
		string b = PathEx.NormalizePath(directory);
		FileSystemWatcher fileSystemWatcher = null;
		foreach (KeyValuePair<FileSystemWatcher, WatchEntry> item in _byHandler)
		{
			if (PathEx.Equals(PathEx.NormalizePath(item.Value.Config.Directory), b))
			{
				fileSystemWatcher = item.Key;
				break;
			}
		}
		if (fileSystemWatcher != null)
		{
			UnwatchInternal(fileSystemWatcher);
			_byHandler.TryRemove(fileSystemWatcher, out var _);
		}
	}

	private void UnwatchInternal(FileSystemWatcher handler)
	{
		if (handler != null)
		{
			handler.Changed -= FileSystemEvent;
			handler.Created -= FileSystemEvent;
			handler.Renamed -= FileSystemEvent;
			handler.Deleted -= FileSystemEvent;
			handler.Error -= OnWatcherError;
			handler.EnableRaisingEvents = false;
			handler.Dispose();
		}
	}

	private void OnWatcherError(object sender, ErrorEventArgs e)
	{
		string text = ((sender is FileSystemWatcher fileSystemWatcher) ? fileSystemWatcher.Path : "?");
		Exception exception = e.GetException();
		Logger.Error("FileSystemWatcher error in '" + text + "': " + exception?.Message, exception);
	}

	private void Dispose(bool disposing)
	{
		if (_disposing)
		{
			return;
		}
		if (disposing)
		{
			foreach (FileSystemWatcher key in _byHandler.Keys)
			{
				UnwatchInternal(key);
			}
			_byHandler.Clear();
		}
		_disposing = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
