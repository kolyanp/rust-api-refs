using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Carbon;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.OxideRefs;
using Carbon.Pooling;
using Newtonsoft.Json;
using Oxide.Core.Configuration;
using Oxide.Core.Extensions;
using Oxide.Core.Libraries;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;
using UnityEngine;

namespace Oxide.Core;

public class OxideMod
{
	private ExtensionManager extensionManager = new ExtensionManager();

	internal static readonly Version _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

	internal List<Extension> _extensions = new List<Extension>();

	internal static ConcurrentDictionary<string, object> _libraryCache = new ConcurrentDictionary<string, object>();

	public static readonly VersionNumber Version = new VersionNumber(_assemblyVersion.Major, _assemblyVersion.Minor, _assemblyVersion.Build);

	public DataFileSystem DataFileSystem { get; private set; } = new DataFileSystem(Defines.GetDataFolder());

	public PluginManager RootPluginManager { get; private set; }

	public Permission Permission { get; set; }

	public string RootDirectory { get; private set; }

	public string InstanceDirectory { get; private set; }

	public string PluginDirectory { get; private set; }

	public string ConfigDirectory { get; private set; }

	public string DataDirectory { get; private set; }

	public string LangDirectory { get; private set; }

	public string LogDirectory { get; private set; }

	public string TempDirectory { get; private set; }

	public string ExtensionDirectory { get; private set; }

	public bool IsShuttingDown { get; private set; }

	public OxideConfig Config { get; private set; } = new OxideConfig(Path.Combine(Defines.GetRootFolder(), "oxide.config.json"));

	public float Now => Time.realtimeSinceStartup;

	public CompoundLogger RootLogger { get; set; } = new CompoundLogger();

	public IEnumerable<PluginLoader> GetPluginLoaders()
	{
		return extensionManager.GetPluginLoaders();
	}

	public void Load()
	{
		InstanceDirectory = Defines.GetRootFolder();
		RootDirectory = Environment.CurrentDirectory;
		if (RootDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
		{
			RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
		}
		JsonConvert.DefaultSettings = delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			return new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				ReferenceLoopHandling = (ReferenceLoopHandling)1
			};
		};
		ConfigDirectory = Defines.GetConfigsFolder();
		DataDirectory = Defines.GetDataFolder();
		LangDirectory = Defines.GetLangFolder();
		LogDirectory = Defines.GetLogsFolder();
		PluginDirectory = Defines.GetScriptsFolder();
		TempDirectory = Defines.GetTempFolder();
		ExtensionDirectory = Defines.GetExtensionsFolder();
		DataFileSystem = new DataFileSystem(DataDirectory);
		RootPluginManager = new PluginManager();
		Permission = Community.Runtime.Config.Permissions.PermissionSerialization switch
		{
			Permission.SerializationMode.Storeless => new PermissionStoreless(), 
			Permission.SerializationMode.Protobuf => new Permission(), 
			Permission.SerializationMode.SQL => new PermissionSql(), 
			_ => Permission, 
		};
		_extensions.Add(new Extension
		{
			Name = "Rust",
			Author = "Carbon Community LTD",
			Branch = "none",
			Filename = "Carbon.dll",
			Version = new VersionNumber(1, 0, 0)
		});
		CovalencePlugin.PlayerManager.RefreshDatabase(Permission.userdata);
	}

	public void NextTick(Action callback)
	{
		ICarbonProcessor carbonProcessor = Community.Runtime.CarbonProcessor;
		lock (carbonProcessor.CurrentFrameLock)
		{
			carbonProcessor.CurrentFrameQueue.Add(callback);
		}
	}

	public void NextFrame(Action callback)
	{
		ICarbonProcessor carbonProcessor = Community.Runtime.CarbonProcessor;
		lock (carbonProcessor.CurrentFrameLock)
		{
			carbonProcessor.CurrentFrameQueue.Add(callback);
		}
	}

	public bool LoadPlugin(string name)
	{
		CorePlugin.ProcessableFilesLookup();
		CorePlugin.ProcessableFile pluginFile = CorePlugin.GetPluginFile(name);
		if (string.IsNullOrEmpty(pluginFile.Id))
		{
			return false;
		}
		switch (pluginFile.Type)
		{
		case CorePlugin.ProcessableFile.Types.Script:
			Community.Runtime.ScriptProcessor.Prepare(pluginFile.Id, pluginFile.Path);
			return true;
		case CorePlugin.ProcessableFile.Types.CSZIP:
			Community.Runtime.ZipScriptProcessor.Prepare(pluginFile.Id, pluginFile.Path);
			return true;
		default:
			return false;
		}
	}

	public bool ReloadPlugin(string name)
	{
		return LoadPlugin(name);
	}

	public bool UnloadPlugin(string name)
	{
		CorePlugin.ProcessableFile pluginFile = CorePlugin.GetPluginFile(name);
		if (string.IsNullOrEmpty(pluginFile.Id))
		{
			return false;
		}
		RustPlugin rustPlugin = ModLoader.FindPlugin(name);
		if (rustPlugin != null && !rustPlugin.IsCorePlugin)
		{
			ModLoader.UninitializePlugin(rustPlugin);
		}
		switch (pluginFile.Type)
		{
		case CorePlugin.ProcessableFile.Types.Script:
			Community.Runtime.ScriptProcessor.Remove(pluginFile.Id);
			return true;
		case CorePlugin.ProcessableFile.Types.CSZIP:
			Community.Runtime.ZipScriptProcessor.Remove(pluginFile.Id);
			return true;
		default:
			return false;
		}
	}

	public void ReloadAllPlugins(IList<string> skip = null)
	{
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> plugin in Community.Runtime.ScriptProcessor.InstanceBuffer)
		{
			if (skip == null || !skip.Any((string x) => plugin.Key.Contains(x)))
			{
				plugin.Value?.MarkDirty();
			}
		}
		foreach (KeyValuePair<string, IBaseProcessor.IProcess> plugin2 in Community.Runtime.ZipScriptProcessor.InstanceBuffer)
		{
			if (skip == null || !skip.Any((string x) => plugin2.Key.Contains(x)))
			{
				plugin2.Value?.MarkDirty();
			}
		}
	}

	public void UnloadAllPlugins(IList<string> skip = null)
	{
		Community.Runtime.ScriptProcessor.Clear(skip);
		Community.Runtime.ZipScriptProcessor.Clear(skip);
	}

	public void OnSave()
	{
	}

	public void OnShutdown()
	{
		if (!IsShuttingDown)
		{
			Permission?.SaveData();
			Permission?.Dispose();
			IsShuttingDown = true;
		}
	}

	public IEnumerable<Extension> GetAllExtensions()
	{
		return _extensions;
	}

	public object CallHook(string hookName)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd);
	}

	public object CallHook(string hookName, object arg1)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1);
	}

	public object CallHook(string hookName, object arg1, object arg2)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
	}

	public object CallHook(string hookName, params object[] args)
	{
		uint orAdd = HookStringPool.GetOrAdd(hookName);
		return HookCaller.CallStaticHook(orAdd, args);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, params object[] args)
	{
		uint orAdd = HookStringPool.GetOrAdd(oldHook);
		uint orAdd2 = HookStringPool.GetOrAdd(newHook);
		return HookCaller.CallStaticDeprecatedHook(orAdd, orAdd2, expireDate, args);
	}

	public T GetLibrary<T>(string name = null) where T : Library
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(Permission))
		{
			return Community.Runtime.Core.permission as T;
		}
		if (typeFromHandle == typeof(Lang))
		{
			return Community.Runtime.Core.lang as T;
		}
		if (typeFromHandle == typeof(Command))
		{
			return Community.Runtime.Core.cmd as T;
		}
		if (typeFromHandle == typeof(Rust))
		{
			return Community.Runtime.Core.rust as T;
		}
		if (typeFromHandle == typeof(WebRequests))
		{
			return Community.Runtime.Core.webrequest as T;
		}
		if (typeFromHandle == typeof(Oxide.Core.Libraries.Timer))
		{
			return Community.Runtime.Core.timer?.Library as T;
		}
		if (name == null)
		{
			name = typeFromHandle.Name;
		}
		if (!_libraryCache.TryGetValue(name, out var value))
		{
			try
			{
				value = Activator.CreateInstance<T>();
			}
			catch
			{
				try
				{
					value = FormatterServices.GetUninitializedObject(typeof(T)) as T;
				}
				catch
				{
				}
			}
			_libraryCache.TryAdd(name, value);
		}
		return value as T;
	}

	public Extension GetExtension(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (Extension item in ExtensionManager.extensionCache)
		{
			if (item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				return item;
			}
		}
		return null;
	}

	public void LoadExtension(string name)
	{
		string text = Path.Combine(Defines.GetExtensionsFolder(), name + ".dll");
		Carbon.Logger.Log("Loading extension: " + text);
		Community.Runtime.AssemblyEx.Extensions.Load(text, "OxideMod.LoadExtension");
	}

	public void LoadAllPlugins(bool init = false)
	{
	}

	public void LogDebug(string message, params object[] args)
	{
		Carbon.Logger.Debug((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}

	public void LogError(string message, params object[] args)
	{
		Carbon.Logger.Error((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}

	public void LogException(string message, Exception ex)
	{
		Carbon.Logger.Error(message, ex);
	}

	public void LogInfo(string message, params object[] args)
	{
		Carbon.Logger.Log((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}

	public void LogWarning(string message, params object[] args)
	{
		Carbon.Logger.Warn((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}

	public void PrintWarning(string message, params object[] args)
	{
		Carbon.Logger.Warn((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}

	public void PrintError(string message, params object[] args)
	{
		Carbon.Logger.Error((args != null && args.Length != 0) ? string.Format(message, args) : message);
	}
}
