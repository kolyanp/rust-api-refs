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

	private readonly List<string> _sourceChanges = new List<string>(32);

	private readonly HashSet<string> _sourceChangeSet = new HashSet<string>();

	private readonly List<string> _pendingSources = new List<string>(16);

	private readonly List<string> _drainedSources = new List<string>(16);

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
			_sourceChanges.Clear();
			_sourceChangeSet.Clear();
			_pendingSources.Clear();
			_drainedSources.Clear();
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

	public virtual string GetInstanceKey(string sourcePath)
	{
		return Path.GetFileNameWithoutExtension(sourcePath);
	}

	protected virtual string GetSourcePath(string eventPath)
	{
		return eventPath;
	}

	protected virtual bool SourceExists(string sourcePath)
	{
		return OsEx.File.Exists(sourcePath);
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
		ReconcileSourceChanges();
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
			ProcessPendingSources();
			yield return null;
		}
	}

	private bool ProcessRuntimeEntry(string key, IBaseProcessor.IProcess value)
	{
		if (value == null)
		{
			InstanceBuffer.Remove(key);
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
			Prepare(GetInstanceKey(file), file);
		}
	}

	public virtual void Prepare(string id, string file)
	{
		if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(file) && !IgnoreList.Contains(file) && (string.IsNullOrEmpty(Extension) || !OsEx.File.Exists(file) || PathEx.HasExtension(file, Extension)))
		{
			InstallProcess(id, file);
		}
	}

	private void InstallProcess(string id, string file)
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

	public virtual void Remove(string id)
	{
		CancelPendingSource(id);
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
		if (list == null)
		{
			_pendingSources.Clear();
		}
		else
		{
			for (int num = _pendingSources.Count - 1; num >= 0; num--)
			{
				if (!FileMatchesAny(_pendingSources[num], list))
				{
					_pendingSources.RemoveAt(num);
				}
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
		if (EnableWatcher)
		{
			QueueSourceChange(e.Path);
		}
	}

	public virtual void OnChanged(WatchFileEvent e)
	{
		if (EnableWatcher)
		{
			QueueSourceChange(e.Path);
		}
	}

	public virtual void OnRenamed(WatchFileEvent e)
	{
		if (EnableWatcher)
		{
			QueueSourceChange(e.OldPath);
			QueueSourceChange(e.Path);
		}
	}

	public virtual void OnRemoved(WatchFileEvent e)
	{
		if (EnableWatcher)
		{
			QueueSourceChange(e.Path);
		}
	}

	private void QueueSourceChange(string path)
	{
		if (!string.IsNullOrEmpty(path) && !IsBlacklisted(path) && (string.IsNullOrEmpty(Extension) || PathEx.HasExtension(path, Extension)))
		{
			string sourcePath = GetSourcePath(path);
			if (!string.IsNullOrEmpty(sourcePath) && _sourceChangeSet.Add(sourcePath))
			{
				_sourceChanges.Add(sourcePath);
			}
		}
	}

	private void ReconcileSourceChanges()
	{
		for (int i = 0; i < _sourceChanges.Count; i++)
		{
			string text = _sourceChanges[i];
			try
			{
				ReconcileSource(text);
			}
			catch (Exception ex)
			{
				Logger.Error("Watcher source error for '" + text + "'", ex);
			}
		}
		_sourceChanges.Clear();
		_sourceChangeSet.Clear();
	}

	private void ReconcileSource(string sourcePath)
	{
		string instanceKey = GetInstanceKey(sourcePath);
		if (string.IsNullOrEmpty(instanceKey))
		{
			return;
		}
		bool flag = SourceExists(sourcePath);
		if (InstanceBuffer.TryGetValue(instanceKey, out var value) && value != null)
		{
			if (PathEx.Equals(value.File, sourcePath))
			{
				if (flag)
				{
					value.MarkDirty();
				}
				else
				{
					value.MarkDeleted();
				}
			}
			else if (flag)
			{
				if (!SourceExists(value.File))
				{
					value.File = sourcePath;
					value.MarkDirty();
				}
				else
				{
					WarnDuplicateSource(sourcePath, value.File);
				}
			}
		}
		else if (flag)
		{
			_pendingSources.Add(sourcePath);
		}
	}

	private void ProcessPendingSources()
	{
		if (_pendingSources.Count == 0)
		{
			return;
		}
		_drainedSources.AddRange(_pendingSources);
		_pendingSources.Clear();
		for (int i = 0; i < _drainedSources.Count; i++)
		{
			string text = _drainedSources[i];
			if (!SourceExists(text))
			{
				continue;
			}
			try
			{
				string instanceKey = GetInstanceKey(text);
				if (InstanceBuffer.TryGetValue(instanceKey, out var value) && value != null)
				{
					if (!PathEx.Equals(value.File, text))
					{
						WarnDuplicateSource(text, value.File);
					}
				}
				else
				{
					InstallProcess(instanceKey, text);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Processor run error for '" + text + "'", ex);
			}
		}
		_drainedSources.Clear();
	}

	private static void WarnDuplicateSource(string sourcePath, string existingFile)
	{
		Logger.Warn("Skipping '" + sourcePath + "': '" + existingFile + "' is already loaded under the same name.");
	}

	private void CancelPendingSource(string key)
	{
		for (int num = _pendingSources.Count - 1; num >= 0; num--)
		{
			if (GetInstanceKey(_pendingSources[num]) == key)
			{
				_pendingSources.RemoveAt(num);
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
