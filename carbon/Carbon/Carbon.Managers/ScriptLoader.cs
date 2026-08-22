using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using API.Events;
using Carbon.Base;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Jobs;
using Facepunch;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Carbon.Managers;

public class ScriptLoader : IScriptLoader, IDisposable
{
	[Serializable]
	public class Script : IDisposable, IScript
	{
		public Assembly Assembly { get; set; }

		public Type Type { get; set; }

		public string Name { get; set; }

		public string Author { get; set; }

		public VersionNumber Version { get; set; }

		public string Description { get; set; }

		public IScriptLoader Loader { get; set; }

		public RustPlugin Instance { get; set; }

		public bool IsCore { get; set; }

		public static Script Create(Assembly assembly, Type type)
		{
			return new Script
			{
				Assembly = assembly,
				Type = type,
				Name = null,
				Author = null,
				Version = new VersionNumber(1, 0, 0),
				Description = null
			};
		}

		public void Dispose()
		{
			Assembly = null;
			Type = null;
			Name = null;
			Author = null;
			Version = default(VersionNumber);
			Description = null;
			Loader = null;
			Instance = null;
			IsCore = false;
		}

		public override string ToString()
		{
			return $"{Name} v{Version}";
		}
	}

	public const int BusyFileAttempts = 10;

	private IEnumerator _compileRoutine;

	public ISource InitialSource
	{
		get
		{
			List<ISource> sources = Sources;
			if (sources == null || sources.Count <= 0)
			{
				return null;
			}
			return Sources[0];
		}
	}

	public bool BypassFileNameChecks { get; set; }

	public List<IScript> Scripts { get; set; } = new List<IScript>();

	public List<ISource> Sources { get; set; } = new List<ISource>();

	public bool IsCore { get; set; }

	public bool IsExtension { get; set; }

	public bool HasFinished { get; set; }

	public bool HasRequires { get; set; }

	public IBaseProcessor.IProcess Process { get; set; }

	public ModLoader.Package Mod { get; set; }

	public IBaseProcessor.IParser Parser { get; set; }

	public ScriptCompilationThread AsyncLoader { get; set; } = new ScriptCompilationThread();

	public void Load()
	{
		if (InitialSource == null || string.IsNullOrEmpty(InitialSource.FilePath))
		{
			Clear();
			return;
		}
		try
		{
			string directoryName = Path.GetDirectoryName(InitialSource.FilePath);
			IsExtension = directoryName.EndsWith("extensions");
			_compileRoutine = Compile();
			Community.Runtime.ScriptProcessor.StartCoroutine(_compileRoutine);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed loading script '" + InitialSource.FilePath + "':", ex);
		}
	}

	public static void LoadAll(IEnumerable<string> except = null)
	{
		Config config = Community.Runtime.Config;
		string[] filesWithExtension = OsEx.Folder.GetFilesWithExtension(Defines.GetExtensionsFolder(), "cs");
		string[] filesWithExtension2 = OsEx.Folder.GetFilesWithExtension(Defines.GetScriptsFolder(), "cs", config.Watchers.ScriptWatcherOption);
		string[] filesWithExtension3 = OsEx.Folder.GetFilesWithExtension(Defines.GetScriptsFolder(), "cszip", config.Watchers.ScriptWatcherOption);
		int count = 0;
		ExecuteProcess(Community.Runtime.ScriptProcessor, folderMode: false, except, ref count, new string[2][] { filesWithExtension, filesWithExtension2 });
		ExecuteProcess(Community.Runtime.ZipScriptProcessor, folderMode: false, except, ref count, new string[1][] { filesWithExtension3 });
		if (count == 0)
		{
			ModLoader.IsBatchComplete = true;
			Community.Runtime.Events.Trigger(CarbonEvent.AllPluginsLoaded, EventArgs.Empty);
			Community.Runtime.Events.Trigger(CarbonEvent.AllPluginsInitialized, EventArgs.Empty);
		}
		static void ExecuteProcess(IScriptProcessor processor, bool folderMode, IEnumerable<string> enumerable, ref int reference, params string[][] folders)
		{
			processor.Clear();
			string[][] array = folders;
			foreach (string[] array2 in array)
			{
				string[] array3 = array2;
				foreach (string file in array3)
				{
					if (!processor.IsBlacklisted(file) && (enumerable == null || !enumerable.Any((string x) => file.Contains(x))))
					{
						string text = (folderMode ? file : Path.GetDirectoryName(file));
						string key = (folderMode ? text : Path.GetFileNameWithoutExtension(file));
						if (!processor.InstanceBuffer.ContainsKey(key))
						{
							ScriptProcessor.Script value = new ScriptProcessor.Script
							{
								File = file
							};
							processor.InstanceBuffer.Add(key, value);
							reference++;
						}
					}
				}
			}
			foreach (KeyValuePair<string, IBaseProcessor.IProcess> item in processor.InstanceBuffer)
			{
				item.Value.MarkDirty();
			}
			Array.Clear(folders, 0, folders.Length);
			folders = null;
		}
	}

	public void Clear()
	{
		if (Scripts != null)
		{
			for (int i = 0; i < Scripts.Count; i++)
			{
				IScript plugin = Scripts[i];
				if (!plugin.IsCore && plugin.Instance != null)
				{
					plugin.Instance.Package.Plugins?.RemoveAll((RustPlugin x) => x == plugin.Instance);
					if (plugin.Instance.IsExtension)
					{
						ScriptCompilationThread._clearExtensionPlugin(plugin.Instance.FilePath);
					}
					try
					{
						ModLoader.UninitializePlugin(plugin.Instance);
					}
					catch (Exception ex)
					{
						Logger.Error($"Failed unloading '{plugin.Instance}'", ex);
					}
				}
			}
			if (Scripts.Count > 0)
			{
				Scripts.RemoveAll((IScript x) => !x.IsCore);
			}
		}
		Dispose();
	}

	private IEnumerator ReadFileAsync(string filePath, Action<string> onRead)
	{
		Task<string> task = Task.Run<string>(async delegate
		{
			FileInfo fileInfo = new FileInfo(filePath);
			bool inUse = true;
			bool success = true;
			int attempts = 0;
			while (inUse)
			{
				inUse = !RunFileUseChecks();
				if (!inUse)
				{
					break;
				}
				attempts++;
				await AsyncEx.WaitForSeconds(0.2f);
				if (attempts >= 10)
				{
					inUse = false;
					success = false;
					Logger.Warn("Failed compiling '" + InitialSource.ContextFileName + "' due to it being in use.");
				}
			}
			if (success && !inUse)
			{
				using (StreamReader reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true))
				{
					return await reader.ReadToEndAsync();
				}
			}
			return (string)null;
			bool RunFileUseChecks()
			{
				try
				{
					using FileStream fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
					fileStream.Close();
					return true;
				}
				catch (IOException)
				{
					return false;
				}
			}
		});
		while (!task.IsCompleted)
		{
			yield return null;
		}
		onRead?.Invoke(task.Result);
	}

	public IEnumerator Compile()
	{
		if (string.IsNullOrEmpty(InitialSource.Content) && !string.IsNullOrEmpty(InitialSource.FilePath) && OsEx.File.Exists(InitialSource.FilePath))
		{
			yield return ReadFileAsync(InitialSource.FilePath, delegate(string content)
			{
				if (InitialSource != null && !string.IsNullOrEmpty(content))
				{
					InitialSource.Content = content;
				}
			});
		}
		if (Parser != null && Sources != null)
		{
			for (int num = 0; num < Sources.Count; num++)
			{
				ISource source = Sources[num];
				Parser.Process(source.FilePath, source.Content, out var output);
				if (Sources != null && !string.IsNullOrEmpty(output))
				{
					Sources[num] = new BaseSource
					{
						ContextFilePath = source.ContextFilePath,
						ContextFileName = source.ContextFileName,
						FilePath = source.FilePath,
						FileName = source.FileName,
						Content = output
					};
				}
			}
		}
		if (Sources == null || Sources.Count == 0)
		{
			HasFinished = true;
			yield break;
		}
		IEnumerable<string> enumerable = Sources.Where((ISource x) => !string.IsNullOrEmpty(x.Content)).SelectMany((ISource x) => x.Content.Split('\n'));
		List<string> resultReferences = Pool.Get<List<string>>();
		List<string> resultRequires = Pool.Get<List<string>>();
		if (enumerable != null)
		{
			foreach (string item3 in enumerable)
			{
				try
				{
					if (item3.StartsWith("// Reference:") || item3.StartsWith("//Reference:"))
					{
						string item = (item3.Replace("// Reference:", "").Replace("//Reference:", "") ?? "").Trim();
						resultReferences.Add(item);
					}
				}
				catch
				{
				}
				try
				{
					if (item3.StartsWith("// Requires:") || item3.StartsWith("//Requires:"))
					{
						string item2 = (item3.Replace("// Requires:", "").Replace("//Requires:", "") ?? "").Trim();
						resultRequires.Add(item2);
					}
				}
				catch
				{
				}
			}
		}
		yield return null;
		if (AsyncLoader != null)
		{
			AsyncLoader.Sources = new List<ISource>(Sources);
			AsyncLoader.References = resultReferences?.ToArray();
			AsyncLoader.Requires = resultRequires?.ToArray();
			AsyncLoader.IsExtension = IsExtension;
		}
		Pool.FreeUnmanaged<string>(ref resultReferences);
		Pool.FreeUnmanaged<string>(ref resultRequires);
		if (AsyncLoader != null)
		{
			HasRequires = AsyncLoader.Requires.Length != 0;
		}
		yield return null;
		while (HasRequires && !Community.Runtime.ScriptProcessor.AllNonRequiresScriptsComplete() && !IsExtension && !Community.Runtime.ScriptProcessor.AllExtensionsComplete())
		{
			yield return null;
		}
		List<Plugin> requires = Pool.Get<List<Plugin>>();
		List<string> missingRequires = Pool.Get<List<string>>();
		if (AsyncLoader != null)
		{
			string[] requires2 = AsyncLoader.Requires;
			foreach (string text in requires2)
			{
				Plugin plugin = Community.Runtime.Core.plugins.Find(text);
				if (plugin == null)
				{
					missingRequires.Add(text);
				}
				else
				{
					requires.Add(plugin);
				}
			}
		}
		yield return null;
		if (missingRequires.Count > 0)
		{
			foreach (string item4 in missingRequires)
			{
				Logger.Warn(" Couldn't find required plugin '" + item4 + "' for '" + ((!string.IsNullOrEmpty(InitialSource.ContextFilePath)) ? Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath) : "<unknown>") + "', retrying..");
			}
			ModLoader.AddPostBatchFailedRequiree(InitialSource.ContextFilePath);
			HasFinished = true;
			Pool.FreeUnmanaged<Plugin>(ref requires);
			Pool.FreeUnmanaged<string>(ref missingRequires);
			if (Community.AllProcessorsFinalized)
			{
				ModLoader.IsBatchComplete = true;
			}
			yield break;
		}
		Pool.FreeUnmanaged<string>(ref missingRequires);
		yield return null;
		Plugin[] requiresResult = requires.ToArray();
		AsyncLoader?.Start();
		while (AsyncLoader != null && !AsyncLoader.IsDone)
		{
			yield return null;
		}
		if (AsyncLoader == null)
		{
			HasFinished = true;
			Pool.FreeUnmanaged<Plugin>(ref requires);
			yield break;
		}
		yield return null;
		if (AsyncLoader != null && (AsyncLoader.IsCompileTestMode || AsyncLoader.Assembly == null))
		{
			if (AsyncLoader.Exceptions != null && AsyncLoader.Exceptions.Count > 0)
			{
				Logger.Error("Failed compiling '" + AsyncLoader.InitialSource.ContextFileName + "':");
				for (int num3 = 0; num3 < AsyncLoader.Exceptions.Count; num3++)
				{
					ScriptCompilationThread.CompilerException ex = AsyncLoader.Exceptions[num3];
					string arg = string.Format("{0} [{1}]\n     ({2} {3} line {4})", new object[5]
					{
						ex.Error.ErrorText,
						ex.Error.ErrorNumber,
						ex.Error.FileName,
						ex.Error.Column,
						ex.Error.Line
					});
					Logger.Error($"  {num3 + 1:n0}. {arg}");
				}
				ModLoader.CompilationResult compilationResult = ModLoader.GetCompilationResult(InitialSource.ContextFilePath);
				compilationResult.Clear();
				compilationResult.RollbackType = ModLoader.GetRegisteredType(InitialSource.ContextFilePath);
				compilationResult.AppendErrors(AsyncLoader.Exceptions.Select((ScriptCompilationThread.CompilerException x) => new ModLoader.Trace
				{
					Message = x.Error.ErrorText,
					Number = x.Error.ErrorNumber,
					Column = x.Error.Column,
					Line = x.Error.Line
				}));
				HookCaller.CallStaticHook(2719094727u, InitialSource.ContextFilePath, compilationResult);
				if (Community.Runtime.Config.Compiler.UnloadOnFailure)
				{
					string rollbackTypeName = compilationResult.GetRollbackTypeName();
					if (!string.IsNullOrEmpty(rollbackTypeName))
					{
						RustPlugin rustPlugin = ModLoader.FindPlugin(rollbackTypeName);
						if (rustPlugin != null)
						{
							ModLoader.UninitializePlugin(rustPlugin);
						}
					}
				}
			}
			else if (AsyncLoader.IsCompileTestMode)
			{
				string text2 = AsyncLoader.InitialSource?.ContextFileName ?? "<unknown>";
				if (AsyncLoader.IsCompileSuccess)
				{
					if (AsyncLoader.Warnings != null)
					{
						_ = AsyncLoader.Warnings.Count;
					}
					Logger.Log($"Compilation of '{text2}' complete [{AsyncLoader.CompileTime.TotalMilliseconds:0}ms] (compile-test mode)");
				}
				else
				{
					Logger.Error("Compilation of '" + text2 + "' failed (compile-test mode)");
				}
			}
			AsyncLoader.Exceptions?.Clear();
			AsyncLoader.Warnings?.Clear();
			AsyncLoader.Exceptions = (AsyncLoader.Warnings = null);
			HasFinished = true;
			Pool.FreeUnmanaged<Plugin>(ref requires);
			if (Community.AllProcessorsFinalized)
			{
				ModLoader.OnPluginProcessFinished();
			}
			yield break;
		}
		if (AsyncLoader == null)
		{
			Pool.FreeUnmanaged<Plugin>(ref requires);
			yield break;
		}
		Assembly assembly = AsyncLoader.Assembly;
		bool firstPlugin = true;
		yield return null;
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			try
			{
				if (string.IsNullOrEmpty(type.Namespace) || (!type.Namespace.Equals("Oxide.Plugins") && !type.Namespace.Equals("Carbon.Plugins")))
				{
					continue;
				}
				Attribute customAttribute = type.GetCustomAttribute(typeof(InfoAttribute), inherit: true);
				InfoAttribute info = customAttribute as InfoAttribute;
				if (info == null)
				{
					continue;
				}
				if ((!IsExtension & firstPlugin) && !BypassFileNameChecks)
				{
					string text3 = Path.GetFileNameWithoutExtension(InitialSource.FilePath).ToLower().Replace(" ", "")
						.Replace(".", "")
						.Replace("-", "");
					if (type.Name.ToLower().Replace(" ", "").Replace(".", "")
						.Replace("-", "") != text3)
					{
						Logger.Warn("Plugin '" + type.Name + "' does not match with its file-name '" + text3 + "'.");
						break;
					}
				}
				firstPlugin = false;
				if (requires.Any((Plugin x) => x.Name == info.Title))
				{
					continue;
				}
				DescriptionAttribute descriptionAttribute = type.GetCustomAttribute(typeof(DescriptionAttribute), inherit: true) as DescriptionAttribute;
				Script plugin2 = Script.Create(assembly, type);
				plugin2.Name = info.Title;
				plugin2.Author = info.Author;
				plugin2.Version = info.Version;
				plugin2.Description = descriptionAttribute?.Description;
				if (ModLoader.InitializePlugin(type, out var plugin3, Mod, delegate(RustPlugin p)
				{
					Scripts.Add(plugin2);
					p.HasConditionals = Sources.Any((ISource x) => x.Content.Contains("#if "));
					p.IsExtension = IsExtension;
					plugin2.IsCore = IsCore;
					p.Hooks = AsyncLoader.Hooks[type];
					p.HookMethods = AsyncLoader.HookMethods[type];
					p.PluginReferences = AsyncLoader.PluginReferences[type];
					p.Requires = requiresResult;
					p.SetProcessor(Community.Runtime.ScriptProcessor, Process);
					p.CompileTime = AsyncLoader.CompileTime;
					p.InternalCallHookGenTime = AsyncLoader.InternalCallHookGenTime;
					p.InternalCallHookSource = AsyncLoader.InternalCallHookSource;
					p.FilePath = AsyncLoader.InitialSource.ContextFilePath;
					p.FileName = AsyncLoader.InitialSource.ContextFileName;
				}))
				{
					plugin2.Instance = plugin3;
					CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
					e.Init(plugin3);
					Community.Runtime.Events.Trigger(CarbonEvent.PluginPreload, e);
					Pool.Free<CarbonEventArgs>(ref e);
					ModLoader.RegisterType(AsyncLoader.InitialSource.ContextFilePath, type);
					Plugin.InternalApplyAllPluginReferences();
					HookCaller.CallStaticHook(3051933177u, plugin3);
				}
				goto IL_0d08;
			}
			catch (Exception ex2)
			{
				HasFinished = true;
				if (InitialSource != null)
				{
					HookCaller.CallStaticHook(1298319061u, (!string.IsNullOrEmpty(InitialSource.ContextFilePath)) ? Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath) : "<unknown>", ex2);
					Logger.Error("Failed to compile '" + ((!string.IsNullOrEmpty(InitialSource.ContextFilePath)) ? Path.GetFileNameWithoutExtension(InitialSource.ContextFilePath) : "<unknown>") + "': ", ex2);
				}
				goto IL_0d08;
			}
			IL_0d08:
			yield return null;
		}
		if (firstPlugin)
		{
			Logger.Error("Invalid plugin format in '" + AsyncLoader.InitialSource.ContextFileName + "'. Namespace must be Carbon|Oxide.Plugins and inherited class must be Carbon|Rust|CovalencePlugin.");
		}
		AsyncLoader?.Dispose();
		HasFinished = true;
		if (Community.AllProcessorsFinalized)
		{
			ModLoader.OnPluginProcessFinished();
		}
		Pool.FreeUnmanaged<Plugin>(ref requires);
		yield return null;
	}

	public void Dispose()
	{
		if (_compileRoutine != null)
		{
			Community.Runtime.ScriptProcessor.StopCoroutine(_compileRoutine);
			_compileRoutine = null;
		}
		HasFinished = true;
		AsyncLoader?.Abort();
		AsyncLoader = null;
		if (Scripts != null)
		{
			foreach (IScript script in Scripts)
			{
				script.Dispose();
			}
		}
		Scripts?.Clear();
		Sources = null;
		Scripts = null;
	}
}
