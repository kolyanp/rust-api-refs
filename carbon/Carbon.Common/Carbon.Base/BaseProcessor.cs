using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Assembly;
using Carbon.Contracts;
using Carbon.Extensions;
using Facepunch;
using UnityEngine;

namespace Carbon.Base;

public abstract class BaseProcessor : FacepunchBehaviour, IDisposable, IBaseProcessor
{
	public abstract class Process : IBaseProcessor.IProcess, IDisposable
	{
		internal bool _hasChanged;

		internal bool _hasRemoved;

		public IBaseProcessor Processor { get; internal set; }

		public virtual IBaseProcessor.IParser Parser { get; }

		public string File { get; set; }

		public bool HasSucceeded { get; set; }

		public bool IsDirty => _hasChanged;

		public bool IsRemoved => _hasRemoved;

		public abstract void Clear();

		public abstract void Dispose();

		public virtual void Execute(IBaseProcessor processor)
		{
			Processor = processor;
		}

		public void MarkDirty()
		{
			_hasRemoved = false;
			_hasChanged = true;
		}

		public void MarkDeleted()
		{
			_hasRemoved = true;
		}
	}

	public class Parser
	{
		public virtual void Process(string file, string input, out string output)
		{
			output = null;
		}
	}

	[CompilerGenerated]
	private bool _003CIncludeSubdirectories_003Ek__BackingField;

	internal WaitForSeconds _wfsInstance;

	internal readonly Dictionary<string, IBaseProcessor.IProcess> _runtimeCache = new Dictionary<string, IBaseProcessor.IProcess>(128);

	internal string _normalizedFolder;

	private Func<Process> _processFactory;

	private readonly ConcurrentQueue<WatchFileEvent> _events = new ConcurrentQueue<WatchFileEvent>();

	public virtual string Name { get; }

	public Dictionary<string, IBaseProcessor.IProcess> InstanceBuffer { get; set; }

	public List<string> IgnoreList { get; set; }

	public virtual bool EnableWatcher => true;

	public virtual string Folder => string.Empty;

	public virtual string Extension => string.Empty;

	public string[] BlacklistPattern { get; set; }

	public virtual float Rate => 0.2f;

	public virtual Type IndexedType => null;

	public bool IncludeSubdirectories
	{
		[CompilerGenerated]
		get
		{
			return _003CIncludeSubdirectories_003Ek__BackingField;
		}
		set
		{
			_003CIncludeSubdirectories_003Ek__BackingField = value;
			FileSystemWatcher watcher = Watcher;
			if (watcher != null)
			{
				watcher.IncludeSubdirectories = value;
			}
		}
	}

	public FileSystemWatcher Watcher { get; private set; }

	public bool IsInitialized { get; set; }

	public void Awake()
	{
		if (!Community.Runtime.Config.Logging.ReducedLogging)
		{
			Logger.Log("- Installed " + Name);
		}
	}

	public virtual void Start()
	{
		if (!IsInitialized)
		{
			InstanceBuffer = new Dictionary<string, IBaseProcessor.IProcess>();
			IgnoreList = new List<string>();
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			IsInitialized = true;
			RefreshRate();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(Run());
			DisposeWatcher();
			_normalizedFolder = PathEx.NormalizePath(Folder);
			_processFactory = BuildProcessFactory(IndexedType);
			if (!string.IsNullOrEmpty(Extension) && !string.IsNullOrEmpty(Folder))
			{
				Watcher = new FileSystemWatcher(Folder)
				{
					NotifyFilter = (NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.LastAccess),
					Filter = "*" + Extension,
					IncludeSubdirectories = IncludeSubdirectories,
					InternalBufferSize = 65536
				};
				Watcher.Created += OnCreatedRaw;
				Watcher.Changed += OnChangedRaw;
				Watcher.Renamed += OnRenamedRaw;
				Watcher.Deleted += OnDeletedRaw;
				Watcher.Error += OnWatcherError;
				Watcher.EnableRaisingEvents = true;
			}
			if (!Community.Runtime.Config.Logging.ReducedLogging)
			{
				Logger.Log(" Initialized " + (IndexedType?.Name ?? Name) + " processor...");
			}
		}
	}

	public virtual void OnDestroy()
	{
		DisposeWatcher();
		IsInitialized = false;
		Logger.Log(IndexedType?.Name + " processor has been unloaded.");
	}

	private void DisposeWatcher()
	{
		if (Watcher != null)
		{
			Watcher.EnableRaisingEvents = false;
			Watcher.Created -= OnCreatedRaw;
			Watcher.Changed -= OnChangedRaw;
			Watcher.Renamed -= OnRenamedRaw;
			Watcher.Deleted -= OnDeletedRaw;
			Watcher.Error -= OnWatcherError;
			Watcher.Dispose();
			Watcher = null;
		}
	}

	public virtual void Dispose()
	{
		Clear();
	}

	private void OnCreatedRaw(object sender, FileSystemEventArgs e)
	{
		_events.Enqueue(new WatchFileEvent(WatcherChangeTypes.Created, e.FullPath, null, isInitial: false));
	}

	private void OnChangedRaw(object sender, FileSystemEventArgs e)
	{
		_events.Enqueue(new WatchFileEvent(WatcherChangeTypes.Changed, e.FullPath, null, isInitial: false));
	}

	private void OnRenamedRaw(object sender, RenamedEventArgs e)
	{
		_events.Enqueue(new WatchFileEvent(WatcherChangeTypes.Renamed, e.FullPath, e.OldFullPath, isInitial: false));
	}

	private void OnDeletedRaw(object sender, FileSystemEventArgs e)
	{
		_events.Enqueue(new WatchFileEvent(WatcherChangeTypes.Deleted, e.FullPath, null, isInitial: false));
	}

	private void OnWatcherError(object sender, ErrorEventArgs e)
	{
		Exception exception = e.GetException();
		Logger.Error("FileSystemWatcher error in '" + Folder + "': " + exception?.Message, exception);
	}

	private static Func<Process> BuildProcessFactory(Type type)
	{
		if (type == null)
		{
			return null;
		}
		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
		if (constructor == null)
		{
			return null;
		}
		return Expression.Lambda<Func<Process>>(Expression.New(constructor), Array.Empty<ParameterExpression>()).Compile();
	}

	private Process CreateProcess()
	{
		if (_processFactory != null)
		{
			return _processFactory();
		}
		if (IndexedType == null)
		{
			return null;
		}
		return Activator.CreateInstance(IndexedType) as Process;
	}

	protected virtual string GetInstanceKey(string sourcePath)
	{
		return Path.GetFileNameWithoutExtension(sourcePath);
	}

	private void DrainEventQueue()
	{
		WatchFileEvent result;
		while (_events.TryDequeue(out result))
		{
			try
			{
				switch (result.Type)
				{
				case WatcherChangeTypes.Created:
					OnCreated(result);
					break;
				case WatcherChangeTypes.Changed:
					OnChanged(result);
					break;
				case WatcherChangeTypes.Renamed:
					OnRenamed(result);
					break;
				case WatcherChangeTypes.Deleted:
					OnRemoved(result);
					break;
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Watcher dispatch error for '{result.Path}' ({result.Type})", ex);
			}
		}
	}

	public virtual IEnumerator Run()
	{
		while (true)
		{
			yield return _wfsInstance;
			DrainEventQueue();
			foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in InstanceBuffer)
			{
				IBaseProcessor.IProcess value = item.Value;
				if (value == null || value.IsRemoved || value.IsDirty)
				{
					_runtimeCache.Add(item.Key, value);
				}
			}
			if (_runtimeCache.Count == 0)
			{
				yield return null;
				continue;
			}
			foreach (KeyValuePair<string, IBaseProcessor.IProcess> item2 in _runtimeCache)
			{
				bool flag = false;
				try
				{
					flag = ProcessRuntimeEntry(item2.Key, item2.Value);
				}
				catch (Exception ex)
				{
					Logger.Error("Processor run error for '" + item2.Key + "'", ex);
				}
				if (flag)
				{
					yield return null;
				}
			}
			_runtimeCache.Clear();
			yield return null;
		}
	}

	private bool ProcessRuntimeEntry(string key, IBaseProcessor.IProcess value)
	{
		if (value == null)
		{
			Process process = CreateProcess();
			if (process != null)
			{
				process.File = key;
				process.Execute(this);
				string instanceKey = GetInstanceKey(key);
				InstanceBuffer.Remove(key);
				InstanceBuffer[instanceKey] = process;
			}
			return false;
		}
		if (value.IsRemoved)
		{
			Clear(key, value);
			return true;
		}
		if (value.IsDirty)
		{
			Execute(key, value);
			return true;
		}
		return false;
	}

	public virtual bool Exists(string path)
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in InstanceBuffer)
		{
			if (item.Value != null && item.Value.File == path)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void Prepare(string file)
	{
		if (file.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			Prepare(Path.GetFileName(file.Substring(8)), file);
		}
		else if (file.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
		{
			Prepare(Path.GetFileName(file.Substring(7)), file);
		}
		else
		{
			Prepare(Path.GetFileNameWithoutExtension(file), file);
		}
	}

	public virtual void Prepare(string id, string file)
	{
		if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(file) && !IgnoreList.Contains(file) && (string.IsNullOrEmpty(Extension) || !OsEx.File.Exists(file) || PathEx.HasExtension(file, Extension)))
		{
			Remove(id);
			Process process = CreateProcess();
			if (process != null)
			{
				InstanceBuffer.Add(id, process);
				process.File = file;
				process.Execute(this);
			}
		}
	}

	public virtual void Remove(string id)
	{
		if (InstanceBuffer.TryGetValue(id, out var value))
		{
			value?.Clear();
			value?.Dispose();
			InstanceBuffer.Remove(id);
		}
	}

	public virtual void Clear(IEnumerable<string> except = null)
	{
		List<string> list = null;
		if (except != null)
		{
			list = Pool.Get<List<string>>();
			foreach (string item in except)
			{
				list.Add(item);
			}
			if (list.Count == 0)
			{
				Pool.FreeUnmanaged<string>(ref list);
				list = null;
			}
		}
		List<string> list2 = Pool.Get<List<string>>();
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> item2 in InstanceBuffer)
		{
			if (list == null || !FileMatchesAny(item2.Value?.File, list))
			{
				try
				{
					item2.Value?.Clear();
					item2.Value?.Dispose();
				}
				catch (Exception ex)
				{
					Logger.Error(" Processor error: '" + item2.Key + "'", ex);
				}
				list2.Add(item2.Key);
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			InstanceBuffer.Remove(list2[i]);
		}
		Pool.FreeUnmanaged<string>(ref list2);
		if (list != null)
		{
			Pool.FreeUnmanaged<string>(ref list);
		}
	}

	public virtual void Ignore(string file)
	{
		if (!IgnoreList.Contains(file))
		{
			IgnoreList.Add(file);
		}
	}

	public virtual void ClearIgnore(string file)
	{
		IgnoreList.Remove(file);
	}

	public T Get<T>(string id) where T : IBaseProcessor.IProcess
	{
		if (InstanceBuffer.TryGetValue(id, out var value))
		{
			return (T)value;
		}
		return default(T);
	}

	public virtual void Clear(string id, IBaseProcessor.IProcess process)
	{
		process?.Clear();
		process?.Dispose();
		Remove(id);
	}

	public virtual void Execute(string id, IBaseProcessor.IProcess process)
	{
		Prepare(id, process.File);
	}

	public virtual void OnCreated(WatchFileEvent e)
	{
		if (EnableWatcher && !IsBlacklisted(e.Path))
		{
			IBaseProcessor.IProcess value2;
			if (InstanceBuffer.TryGetValue(e.Path, out var value))
			{
				value?.MarkDirty();
			}
			else if (InstanceBuffer.TryGetValue(Path.GetFileNameWithoutExtension(e.Path), out value2))
			{
				value2?.MarkDirty();
			}
			else
			{
				InstanceBuffer.Add(e.Path, null);
			}
		}
	}

	public virtual void OnChanged(WatchFileEvent e)
	{
		if (EnableWatcher && !IsBlacklisted(e.Path))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.Path);
			if (InstanceBuffer.TryGetValue(fileNameWithoutExtension, out var value))
			{
				value.MarkDirty();
			}
		}
	}

	public virtual void OnRenamed(WatchFileEvent e)
	{
		if (!EnableWatcher)
		{
			return;
		}
		if (!string.IsNullOrEmpty(e.OldPath))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.OldPath);
			if (InstanceBuffer.TryGetValue(fileNameWithoutExtension, out var value))
			{
				value?.MarkDeleted();
			}
		}
		if (!IsBlacklisted(e.Path))
		{
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(e.Path);
			if (InstanceBuffer.TryGetValue(fileNameWithoutExtension2, out var value2) && value2 != null)
			{
				value2.MarkDirty();
			}
			else
			{
				InstanceBuffer[fileNameWithoutExtension2] = null;
			}
		}
	}

	public virtual void OnRemoved(WatchFileEvent e)
	{
		if (EnableWatcher && !IsBlacklisted(e.Path))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.Path);
			if (InstanceBuffer.TryGetValue(fileNameWithoutExtension, out var value))
			{
				value.MarkDeleted();
			}
		}
	}

	public void RefreshRate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_wfsInstance = new WaitForSeconds(Rate);
	}

	public bool IsBlacklisted(string path)
	{
		if (!IncludeSubdirectories && !string.IsNullOrEmpty(_normalizedFolder))
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				string a = PathEx.NormalizePath(directoryName);
				if (!PathEx.Equals(a, _normalizedFolder))
				{
					return true;
				}
			}
		}
		if (BlacklistPattern == null)
		{
			return false;
		}
		for (int i = 0; i < BlacklistPattern.Length; i++)
		{
			if (path.Contains(BlacklistPattern[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool FileMatchesAny(string file, List<string> patterns)
	{
		if (file == null)
		{
			return false;
		}
		for (int i = 0; i < patterns.Count; i++)
		{
			if (file.Contains(patterns[i]))
			{
				return true;
			}
		}
		return false;
	}
}
