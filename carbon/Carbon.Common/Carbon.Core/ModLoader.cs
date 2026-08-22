using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using API.Commands;
using API.Events;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Generator;
using Carbon.Pooling;
using Carbon.Profiler;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;

namespace Carbon.Core;

public static class ModLoader
{
	[JsonObject(/*Could not decode attribute arguments.*/)]
	public struct CompilationResult
	{
		[JsonProperty]
		public string File;

		[JsonProperty]
		public List<Trace> Errors;

		[JsonProperty]
		public List<Trace> Warnings;

		public Type RollbackType;

		public static CompilationResult Create(string file)
		{
			return new CompilationResult
			{
				File = file,
				Errors = new List<Trace>(),
				Warnings = new List<Trace>()
			};
		}

		public void AppendError(Trace trace)
		{
			Errors.Add(trace);
		}

		public void AppendWarning(Trace trace)
		{
			Warnings.Add(trace);
		}

		public void AppendErrors(IEnumerable<Trace> traces)
		{
			Errors.AddRange(traces);
		}

		public void AppendWarnings(IEnumerable<Trace> traces)
		{
			Warnings.AddRange(traces);
		}

		public void SetRollbackType(Type type)
		{
			RollbackType = type;
		}

		public void LoadRollbackType()
		{
			if (RollbackType == null)
			{
				return;
			}
			RustPlugin rustPlugin = FindPlugin(GetRollbackTypeName());
			if (rustPlugin == null)
			{
				InitializePlugin(RollbackType, out var plugin, Community.Runtime.Plugins, delegate(RustPlugin rustPlugin2)
				{
					Logger.Warn("Rollback for plugin '" + rustPlugin2.ToPrettyString() + "' due to compilation failure");
				}, precompiled: true);
				plugin.InternalCallHookOverriden = true;
				plugin.IsPrecompiled = false;
			}
		}

		public string GetRollbackTypeName()
		{
			if (!(RollbackType == null))
			{
				return RollbackType.GetCustomAttribute<InfoAttribute>()?.Title?.Replace(" ", string.Empty);
			}
			return string.Empty;
		}

		public bool HasFailed()
		{
			List<Trace> errors = Errors;
			if (errors != null)
			{
				return errors.Count > 0;
			}
			return false;
		}

		public void Clear()
		{
			Errors?.Clear();
			Warnings?.Clear();
		}
	}

	[JsonObject(/*Could not decode attribute arguments.*/)]
	public struct Package
	{
		public Assembly Assembly;

		public Type[] AllTypes;

		[JsonProperty]
		public string Name;

		[JsonProperty]
		public string File;

		[JsonProperty]
		public bool IsCoreMod;

		[JsonProperty]
		public List<RustPlugin> Plugins;

		public Dictionary<string, RustPlugin> Index;

		public bool IsValid { get; internal set; }

		public readonly int PluginCount
		{
			get
			{
				if (!IsValid)
				{
					return 0;
				}
				return Plugins.Count;
			}
		}

		public Package AddPlugin(RustPlugin plugin)
		{
			if (!IsValid || Plugins == null || Plugins.Contains(plugin))
			{
				return this;
			}
			Plugins.Add(plugin);
			if (Index != null && plugin.Name != null)
			{
				Index[plugin.Name] = plugin;
			}
			return this;
		}

		public Package RemovePlugin(RustPlugin plugin)
		{
			if (!IsValid || Plugins == null || !Plugins.Contains(plugin))
			{
				return this;
			}
			Plugins.Remove(plugin);
			if (Index != null && plugin.Name != null && Index.TryGetValue(plugin.Name, out var value) && value == plugin)
			{
				Index.Remove(plugin.Name);
			}
			return this;
		}

		public RustPlugin FindPlugin(string name)
		{
			if (string.IsNullOrEmpty(name) || Index == null)
			{
				return null;
			}
			if (!Index.TryGetValue(name, out var value))
			{
				return null;
			}
			return value;
		}

		public static Package Get(string name, bool isCoreMod, string file = null)
		{
			return new Package
			{
				Name = name,
				File = file,
				IsCoreMod = isCoreMod,
				Plugins = new List<RustPlugin>(),
				Index = new Dictionary<string, RustPlugin>(StringComparer.OrdinalIgnoreCase),
				IsValid = true
			};
		}
	}

	public class PackageBank : List<Package>
	{
		public Package FindPackage(string name)
		{
			return this.FirstOrDefault((Package x) => x.Name.Equals(name, StringComparison.InvariantCulture));
		}

		public int FindPackageIndex(string name)
		{
			return FindIndex((Package x) => x.Name.Equals(name, StringComparison.InvariantCulture));
		}

		public bool RemovePackage(string name)
		{
			int num = FindPackageIndex(name);
			if (num == -1)
			{
				return false;
			}
			RemoveAt(num);
			return true;
		}

		public RustPlugin FindPlugin(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			for (int i = 0; i < base.Count; i++)
			{
				RustPlugin rustPlugin = base[i].FindPlugin(name);
				if (rustPlugin != null)
				{
					return rustPlugin;
				}
			}
			return null;
		}

		public void GetAllHookables(List<RustPlugin> plugins, bool ignoreCore = false)
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				foreach (RustPlugin plugin in enumerator.Current.Plugins)
				{
					if (!(plugin.IsCorePlugin & ignoreCore))
					{
						plugins.Add(plugin);
					}
				}
			}
		}
	}

	[JsonObject(/*Could not decode attribute arguments.*/)]
	public struct Trace
	{
		[JsonProperty]
		public string Number;

		[JsonProperty]
		public string Message;

		[JsonProperty]
		public int Column;

		[JsonProperty]
		public int Line;
	}

	public static bool IsBatchComplete;

	public static PackageBank Packages;

	public static Dictionary<string, CompilationResult> FailedCompilations;

	internal const string CARBON_PLUGIN = "CarbonPlugin";

	internal const string RUST_PLUGIN = "RustPlugin";

	internal const string COVALENCE_PLUGIN = "CovalencePlugin";

	internal static Dictionary<string, Type> TypeDictionaryCache { get; }

	internal static Dictionary<string, List<string>> PendingRequirees { get; }

	internal static List<string> PostBatchFailedRequirees { get; }

	internal static bool FirstLoadSinceStartup { get; set; }

	public static CompilationResult GetCompilationResult(string file, bool clear = false)
	{
		if (!FailedCompilations.TryGetValue(file, out var value))
		{
			value = (FailedCompilations[file] = CompilationResult.Create(file));
		}
		if (clear)
		{
			value.Clear();
		}
		return value;
	}

	public static void RegisterPackage(Package package)
	{
		if (!Packages.Contains(package))
		{
			Packages.Add(package);
		}
	}

	public static Package GetPackage(string name)
	{
		foreach (Package package in Packages)
		{
			if (package.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
			{
				return package;
			}
		}
		return default(Package);
	}

	public static RustPlugin FindPlugin(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		for (int i = 0; i < Packages.Count; i++)
		{
			RustPlugin rustPlugin = Packages[i].FindPlugin(name);
			if (rustPlugin != null)
			{
				return rustPlugin;
			}
		}
		return null;
	}

	static ModLoader()
	{
		Packages = new PackageBank();
		FailedCompilations = new Dictionary<string, CompilationResult>();
		TypeDictionaryCache = new Dictionary<string, Type>();
		PendingRequirees = new Dictionary<string, List<string>>();
		PostBatchFailedRequirees = new List<string>();
		FirstLoadSinceStartup = true;
		Community.Runtime.Events.Subscribe(CarbonEvent.OnServerInitialized, delegate
		{
			OnPluginProcessFinished();
		});
	}

	public static List<string> GetRequirees(Plugin initial)
	{
		if (string.IsNullOrEmpty(initial.FilePath))
		{
			return null;
		}
		if (PendingRequirees.TryGetValue(initial.FilePath, out var value))
		{
			return value;
		}
		return null;
	}

	public static void AddPendingRequiree(string initial, string requiree)
	{
		if (!PendingRequirees.TryGetValue(initial, out var value))
		{
			PendingRequirees.Add(initial, value = Pool.Get<List<string>>());
		}
		if (!value.Contains(requiree))
		{
			value.Add(requiree);
		}
	}

	public static void AddPendingRequiree(Plugin initial, Plugin requiree)
	{
		AddPendingRequiree(initial.FilePath, requiree.FilePath);
	}

	public static void AddPostBatchFailedRequiree(string requiree)
	{
		if (!PostBatchFailedRequirees.Contains(requiree))
		{
			PostBatchFailedRequirees.Add(requiree);
		}
	}

	public static void ClearPendingRequirees(Plugin initial)
	{
		if (PendingRequirees.TryGetValue(initial.FilePath, out var value))
		{
			value.Clear();
			PendingRequirees[initial.FilePath] = null;
			PendingRequirees.Remove(initial.FilePath);
		}
	}

	public static void ClearAllRequirees()
	{
		foreach (KeyValuePair<string, List<string>> pendingRequiree in PendingRequirees)
		{
			List<string> value = pendingRequiree.Value;
			Pool.FreeUnmanaged<string>(ref value);
		}
		PendingRequirees.Clear();
	}

	public static void ClearAllErrored()
	{
		foreach (CompilationResult value in FailedCompilations.Values)
		{
			value.Clear();
		}
	}

	public static Type GetRegisteredType(string key)
	{
		if (TypeDictionaryCache.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public static void RegisterType(string key, Type assembly)
	{
		TypeDictionaryCache[key] = assembly;
	}

	public static void UnloadCarbonMods(bool includeCore = false)
	{
		ClearAllRequirees();
		List<Package> list = Pool.Get<List<Package>>();
		list.AddRange(Packages);
		foreach (Package item in list)
		{
			if (includeCore || !item.IsCoreMod)
			{
				UnloadCarbonMod(item.Name);
			}
		}
		Pool.FreeUnmanaged<Package>(ref list);
	}

	public static bool UnloadCarbonMod(string name)
	{
		Package package = GetPackage(name);
		if (!package.IsValid)
		{
			return false;
		}
		UninitializePlugins(package);
		return true;
	}

	public static void UninitializePlugins(Package mod)
	{
		List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
		list.AddRange(mod.Plugins);
		foreach (RustPlugin item in list)
		{
			try
			{
				UninitializePlugin(item);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed unloading '" + mod.Name + "'", ex);
			}
		}
		Pool.FreeUnmanaged<RustPlugin>(ref list);
	}

	public static RustPlugin InitializePlugin(Assembly assembly, Package package = default(Package), Action<RustPlugin> preInit = null, bool precompiled = false)
	{
		Type[] types = assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!(type.BaseType == null) && IsValidPlugin(type.BaseType, recursive: false) && InitializePlugin(type, out var plugin, package, preInit, precompiled))
			{
				return plugin;
			}
		}
		return null;
	}

	public static bool InitializePlugin(Type type, out RustPlugin plugin, Package package = default(Package), Action<RustPlugin> preInit = null, bool precompiled = false)
	{
		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
		object uninitializedObject = FormatterServices.GetUninitializedObject(type);
		plugin = uninitializedObject as RustPlugin;
		InfoAttribute customAttribute = type.GetCustomAttribute<InfoAttribute>();
		DescriptionAttribute customAttribute2 = type.GetCustomAttribute<DescriptionAttribute>();
		if (customAttribute == null)
		{
			Logger.Warn("Failed loading '" + type.Name + "'. The plugin doesn't have the Info attribute.");
			return false;
		}
		string title = customAttribute.Title;
		string author = customAttribute.Author;
		VersionNumber version = customAttribute.Version;
		string description = ((customAttribute2 == null) ? string.Empty : customAttribute2.Description);
		RustPlugin rustPlugin = FindPlugin(title) ?? FindPlugin(type.Name);
		if (rustPlugin != null)
		{
			UninitializePlugin(rustPlugin);
		}
		plugin.SetProcessor(Community.Runtime.ScriptProcessor, null);
		plugin.SetupMod(package, title, author, version, description);
		plugin.IsPrecompiled = precompiled;
		preInit?.Invoke(plugin);
		try
		{
			constructor?.Invoke(uninitializedObject, null);
		}
		catch (Exception ex)
		{
			Analytics.plugin_constructor_failure(plugin);
			HookCaller.CallStaticHook(2684549964u, plugin, ex);
			Exception innerException = ex.InnerException;
			GetCompilationResult(plugin.FilePath).AppendError(new Trace
			{
				Message = "Constructor threw an exception (" + innerException.Message + ")",
				Number = ".ctor"
			});
			Logger.Error("Failed executing constructor for " + plugin.ToPrettyString() + ". This is fatal!", ex);
			return false;
		}
		if (precompiled)
		{
			ProcessPrecompiledType(plugin);
		}
		if (precompiled || !IsValidPlugin(type.BaseType, recursive: false))
		{
			plugin.InternalCallHookOverriden = false;
		}
		package.AddPlugin(plugin);
		plugin.ILoadConfig();
		plugin.ILoadDefaultMessages();
		if ((!plugin.IInit() || !plugin.ILoad()) && UninitializePlugin(plugin, premature: true))
		{
			package.RemovePlugin(plugin);
			return false;
		}
		if (!plugin.ManualCommands)
		{
			ProcessCommands(type, plugin);
		}
		Interface.Oxide.RootPluginManager.AddPlugin(plugin);
		bool flag = MonoProfiler.IsRecording && Community.Runtime.MonoProfilerConfig.IsWhitelisted(MonoProfilerConfig.ProfileTypes.Plugin, Path.GetFileNameWithoutExtension(plugin.FileName));
		Logger.Log((precompiled ? "Preloaded" : "Loaded") + " plugin " + plugin.ToPrettyString() + (precompiled ? string.Empty : $" [{plugin.CompileTime.TotalMilliseconds:0}ms]") + (flag ? " [PROFILING]" : string.Empty));
		CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
		e.Init(plugin);
		Community.Runtime.Events.Trigger(CarbonEvent.PluginLoaded, e);
		Pool.Free<CarbonEventArgs>(ref e);
		if (Community.IsServerInitialized)
		{
			plugin.HasInitialized = true;
			plugin.CallHook("OnServerInitialized", FirstLoadSinceStartup);
			if (!plugin.ApplyOrderedPatches(AutoPatchAttribute.Orders.AfterOnServerInitialized))
			{
				return UninitializePlugin(plugin);
			}
		}
		return true;
	}

	public static bool UninitializePlugin(RustPlugin plugin, bool premature = false, bool unloadDependantPlugins = true)
	{
		if (!premature && !plugin.IsLoaded)
		{
			return true;
		}
		plugin.UnapplyOrderedPatches(AutoPatchAttribute.Orders.Delayed);
		plugin.UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterOnServerInitialized);
		plugin.UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginLoad);
		plugin.UnapplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginInit);
		if (unloadDependantPlugins)
		{
			plugin.IUnloadDependantPlugins();
		}
		if (!premature)
		{
			plugin.CallHook("Unload");
		}
		CarbonEventArgs e = Pool.Get<CarbonEventArgs>();
		e.Init(plugin);
		Community.Runtime.Events.Trigger(CarbonEvent.PluginUnloaded, e);
		Pool.Free<CarbonEventArgs>(ref e);
		RemoveCommands(plugin);
		plugin.IUnload();
		if (!premature)
		{
			HookCaller.CallStaticHook(1250294368u, plugin);
		}
		plugin.Dispose();
		if (!premature)
		{
			Logger.Log("Unloaded plugin " + plugin.ToPrettyString());
			Interface.Oxide.RootPluginManager.RemovePlugin(plugin);
			Plugin.InternalApplyAllPluginReferences();
		}
		return true;
	}

	public static void ProcessPrecompiledType(RustPlugin plugin)
	{
		try
		{
			Type type = plugin.GetType();
			RustPlugin rustPlugin = plugin;
			List<uint> list = rustPlugin.Hooks ?? (rustPlugin.Hooks = new List<uint>());
			rustPlugin = plugin;
			List<HookMethodAttribute> list2 = rustPlugin.HookMethods ?? (rustPlugin.HookMethods = new List<HookMethodAttribute>());
			rustPlugin = plugin;
			List<PluginReferenceAttribute> list3 = rustPlugin.PluginReferences ?? (rustPlugin.PluginReferences = new List<PluginReferenceAttribute>());
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (InternalCallHook.HasRefLikeSignature(methodInfo))
				{
					continue;
				}
				uint orAdd = HookStringPool.GetOrAdd(methodInfo.Name);
				if (Community.Runtime.HookManager.IsHook(methodInfo.Name))
				{
					if (!list.Contains(orAdd))
					{
						list.Add(orAdd);
					}
					continue;
				}
				HookMethodAttribute customAttribute = methodInfo.GetCustomAttribute<HookMethodAttribute>();
				if (customAttribute != null)
				{
					customAttribute.Method = methodInfo;
					list2.Add(customAttribute);
				}
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				PluginReferenceAttribute customAttribute2 = fieldInfo.GetCustomAttribute<PluginReferenceAttribute>();
				if (customAttribute2 != null)
				{
					customAttribute2.Field = fieldInfo;
					list3.Add(customAttribute2);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed ProcessPrecompiledType for plugin '" + plugin.ToPrettyString() + "'", ex);
		}
	}

	public static bool IsValidPlugin(Type type, bool recursive)
	{
		if (type == null)
		{
			return false;
		}
		bool flag;
		switch (type.Name)
		{
		case "CarbonPlugin":
		case "RustPlugin":
		case "CovalencePlugin":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return true;
		}
		if (recursive)
		{
			return IsValidPlugin(type.BaseType, recursive);
		}
		return false;
	}

	public static void ProcessCommands(Type type, BaseHookable hookable = null, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic, string prefix = null, bool hidden = false)
	{
		MethodInfo[] methods = type.GetMethods(flags);
		FieldInfo[] fields = type.GetFields(flags | BindingFlags.Public);
		PropertyInfo[] properties = type.GetProperties(flags | BindingFlags.Public);
		bool flag = !string.IsNullOrEmpty(prefix);
		MethodInfo[] array = methods;
		foreach (MethodInfo method in array)
		{
			object[] customAttributes = method.GetCustomAttributes(inherit: false);
			if (customAttributes.Length == 0)
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			int authLevel = -1;
			int cooldown = 0;
			bool doCooldownPenalty = false;
			bool flag2 = false;
			object[] array2 = customAttributes;
			foreach (object obj in array2)
			{
				if (!(obj is PermissionAttribute))
				{
					if (!(obj is GroupAttribute))
					{
						if (!(obj is AuthLevelAttribute authLevelAttribute))
						{
							if (!(obj is CooldownAttribute cooldownAttribute))
							{
								if (obj is ChatCommandAttribute || obj is ConsoleCommandAttribute || obj is RConCommandAttribute || obj is ProtectedCommandAttribute || obj is CommandAttribute)
								{
									flag2 = true;
								}
							}
							else
							{
								cooldown = cooldownAttribute.Miliseconds;
								doCooldownPenalty = cooldownAttribute.DoCooldownPenalty;
							}
						}
						else
						{
							authLevel = authLevelAttribute.AuthLevel;
						}
					}
					else
					{
						num2++;
					}
				}
				else
				{
					num++;
				}
			}
			if (!flag2)
			{
				continue;
			}
			string[] array3 = null;
			if (num > 0)
			{
				array3 = new string[num];
				int num3 = 0;
				object[] array4 = customAttributes;
				foreach (object obj2 in array4)
				{
					if (obj2 is PermissionAttribute permissionAttribute)
					{
						array3[num3++] = permissionAttribute.Name;
					}
				}
			}
			string[] array5 = null;
			if (num2 > 0)
			{
				array5 = new string[num2];
				int num4 = 0;
				object[] array6 = customAttributes;
				foreach (object obj3 in array6)
				{
					if (obj3 is GroupAttribute groupAttribute)
					{
						array5[num4++] = groupAttribute.Name;
					}
				}
			}
			int parameterCount = method.GetParameters().Length;
			object[] array7 = customAttributes;
			foreach (object obj4 in array7)
			{
				if (!(obj4 is CommandAttribute { Names: var names }))
				{
					if (!(obj4 is ChatCommandAttribute chatCommandAttribute))
					{
						if (!(obj4 is ConsoleCommandAttribute consoleCommandAttribute))
						{
							if (!(obj4 is ProtectedCommandAttribute protectedCommandAttribute))
							{
								if (!(obj4 is RConCommandAttribute rConCommandAttribute))
								{
									continue;
								}
								API.Commands.Command.RCon command = new API.Commands.Command.RCon
								{
									Name = (flag ? (prefix + "." + rConCommandAttribute.Name) : rConCommandAttribute.Name),
									Reference = hookable,
									Callback = delegate(API.Commands.Command.Args arg)
									{
										object[] array21 = HookCaller.Caller.AllocateBuffer(parameterCount);
										if (array21.Length >= 1)
										{
											array21[0] = arg.Token ?? arg;
										}
										try
										{
											object obj11 = method.Invoke(hookable, array21);
											if (obj11 != null && arg.PrintOutput)
											{
												Logger.Log(obj11);
											}
										}
										finally
										{
											HookCaller.Caller.ReturnBuffer(array21);
										}
									},
									Help = rConCommandAttribute.Help,
									Token = rConCommandAttribute,
									CanExecute = (API.Commands.Command _, API.Commands.Command.Args _) => true
								};
								Community.Runtime.CommandManager.RegisterCommand(command, out var _);
								continue;
							}
							Community.Runtime.Core.cmd.AddConsoleCommand(Community.Protect(flag ? (prefix + "." + protectedCommandAttribute.Name) : protectedCommandAttribute.Name), hookable, delegate(Arg arg)
							{
								object[] array21 = HookCaller.Caller.AllocateBuffer(parameterCount);
								if (array21.Length >= 1)
								{
									array21[0] = arg;
								}
								try
								{
									object obj11 = method.Invoke(hookable, array21);
									if (obj11 != null && ((Option)(ref arg.Option)).PrintOutput)
									{
										Logger.Log(obj11);
									}
								}
								finally
								{
									HookCaller.Caller.ReturnBuffer(array21);
								}
								return true;
							}, protectedCommandAttribute.Help, method, array3, array5, authLevel, cooldown, isHidden: true, @protected: false, silent: true, doCooldownPenalty);
							continue;
						}
						Community.Runtime.Core.cmd.AddConsoleCommand(flag ? (prefix + "." + consoleCommandAttribute.Name) : consoleCommandAttribute.Name, hookable, delegate(Arg arg)
						{
							object[] array21 = HookCaller.Caller.AllocateBuffer(parameterCount);
							if (array21.Length >= 1)
							{
								array21[0] = arg;
							}
							try
							{
								object obj11 = method.Invoke(hookable, array21);
								if (obj11 != null && ((Option)(ref arg.Option)).PrintOutput)
								{
									Logger.Log(obj11);
								}
							}
							catch (Exception ex)
							{
								Exception innerException = ex.InnerException;
								if (arg.IsRcon)
								{
									arg.ReplyWith("Failed executing command (" + innerException.Message + ")\n" + innerException.StackTrace);
								}
								else
								{
									Logger.Error("Failed executing command", innerException);
								}
							}
							finally
							{
								HookCaller.Caller.ReturnBuffer(array21);
							}
							return true;
						}, consoleCommandAttribute.Help, method, array3, array5, authLevel, cooldown, hidden, @protected: false, silent: true, doCooldownPenalty);
					}
					else
					{
						Community.Runtime.Core.cmd.AddChatCommand(flag ? (prefix + "." + chatCommandAttribute.Name) : chatCommandAttribute.Name, hookable, method, chatCommandAttribute.Help, method, array3, array5, authLevel, cooldown, hidden, @protected: false, silent: true, doCooldownPenalty);
					}
				}
				else
				{
					foreach (string text in names)
					{
						string command2 = (flag ? (prefix + "." + text) : text);
						Community.Runtime.Core.cmd.AddChatCommand(command2, hookable, method, string.Empty, method, array3, array5, authLevel, cooldown, hidden, @protected: false, silent: true, doCooldownPenalty);
						Community.Runtime.Core.cmd.AddConsoleCommand(command2, hookable, method, string.Empty, method, array3, array5, authLevel, cooldown, hidden, @protected: false, silent: true, doCooldownPenalty);
					}
				}
			}
			if (array3 == null || array3.Length == 0)
			{
				continue;
			}
			Permission permission = Interface.Oxide.Permission;
			string[] array8 = array3;
			foreach (string name in array8)
			{
				if (!permission.PermissionExists(name, hookable))
				{
					permission.RegisterPermission(name, hookable);
				}
			}
		}
		FieldInfo[] array9 = fields;
		foreach (FieldInfo field in array9)
		{
			object[] customAttributes2 = field.GetCustomAttributes(inherit: false);
			if (customAttributes2.Length == 0)
			{
				continue;
			}
			CommandVarAttribute cmdVar = null;
			int num8 = -1;
			int num9 = 0;
			bool doCooldownPenalty2 = false;
			int num10 = 0;
			int num11 = 0;
			object[] array10 = customAttributes2;
			foreach (object obj5 in array10)
			{
				if (!(obj5 is CommandVarAttribute commandVarAttribute))
				{
					if (!(obj5 is AuthLevelAttribute authLevelAttribute2))
					{
						if (!(obj5 is CooldownAttribute cooldownAttribute2))
						{
							if (!(obj5 is PermissionAttribute))
							{
								if (obj5 is GroupAttribute)
								{
									num11++;
								}
							}
							else
							{
								num10++;
							}
						}
						else
						{
							num9 = cooldownAttribute2.Miliseconds;
							doCooldownPenalty2 = cooldownAttribute2.DoCooldownPenalty;
						}
					}
					else
					{
						num8 = authLevelAttribute2.AuthLevel;
					}
				}
				else
				{
					cmdVar = commandVarAttribute;
				}
			}
			if (cmdVar == null)
			{
				continue;
			}
			string[] array11 = null;
			if (num10 > 0)
			{
				array11 = new string[num10];
				int num13 = 0;
				object[] array12 = customAttributes2;
				foreach (object obj6 in array12)
				{
					if (obj6 is PermissionAttribute permissionAttribute2)
					{
						array11[num13++] = permissionAttribute2.Name;
					}
				}
			}
			string[] array13 = null;
			if (num11 > 0)
			{
				array13 = new string[num11];
				int num15 = 0;
				object[] array14 = customAttributes2;
				foreach (object obj7 in array14)
				{
					if (obj7 is GroupAttribute groupAttribute2)
					{
						array13[num15++] = groupAttribute2.Name;
					}
				}
			}
			Oxide.Game.Rust.Libraries.Command cmd = Community.Runtime.Core.cmd;
			string command3 = (flag ? (prefix + "." + cmdVar.Name) : cmdVar.Name);
			BaseHookable plugin = hookable;
			Func<Arg, bool> callback = delegate(Arg args)
			{
				object value = field.GetValue(hookable);
				if (args != null && args.HasArgs(1))
				{
					try
					{
						if (field.FieldType == typeof(string))
						{
							value = args.GetString(0, "");
						}
						else if (field.FieldType == typeof(bool))
						{
							value = args.GetBool(0, false);
						}
						if (field.FieldType == typeof(int))
						{
							value = args.GetInt(0, 0);
						}
						if (field.FieldType == typeof(uint))
						{
							value = args.GetUInt(0, 0u);
						}
						else if (field.FieldType == typeof(float))
						{
							value = args.GetFloat(0, 0f);
						}
						else if (field.FieldType == typeof(long))
						{
							value = args.GetLong(0, 0L);
						}
						else if (field.FieldType == typeof(ulong))
						{
							value = args.GetULong(0, 0uL);
						}
						field.SetValue(hookable, value);
					}
					catch
					{
					}
				}
				value = field.GetValue(hookable);
				if (value != null && cmdVar.Protected)
				{
					value = new string('*', value.ToString().Length);
				}
				args.ReplyWith($"{args.cmd.FullName}: \"{value}\"");
				return true;
			};
			string help = cmdVar.Help;
			FieldInfo reference = field;
			string[] permissions = array11;
			string[] groups = array13;
			int authLevel2 = num8;
			int cooldown2 = num9;
			bool flag3 = cmdVar.Protected;
			cmd.AddConsoleCommand(command3, plugin, callback, help, reference, permissions, groups, authLevel2, cooldown2, hidden, flag3, silent: true, doCooldownPenalty2);
		}
		PropertyInfo[] array15 = properties;
		foreach (PropertyInfo property in array15)
		{
			object[] customAttributes3 = property.GetCustomAttributes(inherit: false);
			if (customAttributes3.Length == 0)
			{
				continue;
			}
			CommandVarAttribute cmdVar2 = null;
			int num18 = -1;
			int num19 = 0;
			bool doCooldownPenalty3 = false;
			int num20 = 0;
			int num21 = 0;
			object[] array16 = customAttributes3;
			foreach (object obj8 in array16)
			{
				if (!(obj8 is CommandVarAttribute commandVarAttribute2))
				{
					if (!(obj8 is AuthLevelAttribute authLevelAttribute3))
					{
						if (!(obj8 is CooldownAttribute cooldownAttribute3))
						{
							if (!(obj8 is PermissionAttribute))
							{
								if (obj8 is GroupAttribute)
								{
									num21++;
								}
							}
							else
							{
								num20++;
							}
						}
						else
						{
							num19 = cooldownAttribute3.Miliseconds;
							doCooldownPenalty3 = cooldownAttribute3.DoCooldownPenalty;
						}
					}
					else
					{
						num18 = authLevelAttribute3.AuthLevel;
					}
				}
				else
				{
					cmdVar2 = commandVarAttribute2;
				}
			}
			if (cmdVar2 == null)
			{
				continue;
			}
			string[] array17 = null;
			if (num20 > 0)
			{
				array17 = new string[num20];
				int num23 = 0;
				object[] array18 = customAttributes3;
				foreach (object obj9 in array18)
				{
					if (obj9 is PermissionAttribute permissionAttribute3)
					{
						array17[num23++] = permissionAttribute3.Name;
					}
				}
			}
			string[] array19 = null;
			if (num21 > 0)
			{
				array19 = new string[num21];
				int num25 = 0;
				object[] array20 = customAttributes3;
				foreach (object obj10 in array20)
				{
					if (obj10 is GroupAttribute groupAttribute3)
					{
						array19[num25++] = groupAttribute3.Name;
					}
				}
			}
			Oxide.Game.Rust.Libraries.Command cmd2 = Community.Runtime.Core.cmd;
			string command4 = (flag ? (prefix + "." + cmdVar2.Name) : cmdVar2.Name);
			BaseHookable plugin2 = hookable;
			Func<Arg, bool> callback2 = delegate(Arg args)
			{
				object value = property.GetValue(hookable);
				if (args != null && args.HasArgs(1))
				{
					try
					{
						Type propertyType = property.PropertyType;
						if (propertyType == typeof(string))
						{
							value = args.GetString(0, "");
						}
						else if (propertyType == typeof(bool))
						{
							value = args.GetBool(0, false);
						}
						else if (propertyType == typeof(int))
						{
							value = args.GetInt(0, 0);
						}
						else if (propertyType == typeof(uint))
						{
							value = args.GetUInt(0, 0u);
						}
						else if (propertyType == typeof(float))
						{
							value = args.GetFloat(0, 0f);
						}
						else if (propertyType == typeof(long))
						{
							value = args.GetLong(0, 0L);
						}
						else if (propertyType == typeof(ulong))
						{
							value = args.GetULong(0, 0uL);
						}
						property.SetValue(hookable, value);
					}
					catch
					{
					}
				}
				value = property.GetValue(hookable);
				if (value != null && cmdVar2.Protected)
				{
					value = new string('*', value.ToString().Length);
				}
				args.ReplyWith($"{args.cmd.FullName}: \"{value}\"");
				return true;
			};
			string help2 = cmdVar2.Help;
			PropertyInfo reference2 = property;
			string[] permissions2 = array17;
			string[] groups2 = array19;
			int authLevel3 = num18;
			int cooldown3 = num19;
			bool flag3 = cmdVar2.Protected;
			cmd2.AddConsoleCommand(command4, plugin2, callback2, help2, reference2, permissions2, groups2, authLevel3, cooldown3, hidden, flag3, silent: true, doCooldownPenalty3);
		}
	}

	public static void RemoveCommands(BaseHookable hookable)
	{
		if (hookable != null)
		{
			Community.Runtime.CommandManager.ClearCommands((API.Commands.Command command) => command.Reference == hookable);
		}
	}

	public static void OnPluginProcessFinished()
	{
		List<string> list = Pool.Get<List<string>>();
		list.AddRange(PostBatchFailedRequirees);
		foreach (string item in list)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
			Community.Runtime.ScriptProcessor.ClearIgnore(fileNameWithoutExtension);
			Community.Runtime.ScriptProcessor.Prepare(fileNameWithoutExtension, item);
			Community.Runtime.ZipScriptProcessor.ClearIgnore(fileNameWithoutExtension);
			Community.Runtime.ZipScriptProcessor.Prepare(fileNameWithoutExtension, item);
		}
		PostBatchFailedRequirees.Clear();
		if (list.Count == 0)
		{
			IsBatchComplete = true;
		}
		list.Clear();
		Pool.FreeUnmanaged<string>(ref list);
		Community.Runtime.Events.Trigger(CarbonEvent.AllPluginsLoaded, EventArgs.Empty);
		if (!Community.IsServerInitialized)
		{
			return;
		}
		int num = 0;
		List<RustPlugin> list2 = Pool.Get<List<RustPlugin>>();
		list2.AddRange(Packages.SelectMany((Package mod) => mod.Plugins));
		foreach (RustPlugin item2 in list2)
		{
			try
			{
				item2.InternalApplyPluginReferences();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed applying PluginReferences for '" + item2.ToPrettyString() + "'", ex);
			}
			if (!item2.HasInitialized)
			{
				num++;
				item2.HasInitialized = true;
				item2.CallHook("OnServerInitialized", FirstLoadSinceStartup);
				if (!item2.ApplyOrderedPatches(AutoPatchAttribute.Orders.AfterOnServerInitialized))
				{
					UninitializePlugin(item2);
				}
			}
		}
		FirstLoadSinceStartup = false;
		Pool.FreeUnmanaged<RustPlugin>(ref list2);
		if (num > 1)
		{
			Analytics.batch_plugin_types();
			Logger.Log(string.Format(" Batch completed! OSI on {0:n0} {1}.", num, num.Plural("plugin", "plugins")));
		}
		Community.Runtime.Events.Trigger(CarbonEvent.AllPluginsInitialized, EventArgs.Empty);
	}
}
