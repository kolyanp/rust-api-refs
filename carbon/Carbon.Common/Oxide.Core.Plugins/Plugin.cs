using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Carbon;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Pooling;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;

namespace Oxide.Core.Plugins;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class Plugin : BaseHookable, IDisposable
{
	public class Persistence : FacepunchBehaviour
	{
	}

	public Persistence persistence;

	public Command cmd;

	public Permission permission;

	public ModLoader.Package Package;

	public IBaseProcessor Processor;

	public IBaseProcessor.IProcess ProcessorProcess;

	public object OnAddedToManager;

	public object OnRemovedFromManager;

	public PluginManager Manager { get; set; }

	public bool IsCorePlugin { get; set; }

	[JsonProperty]
	public string Title { get; set; }

	[JsonProperty]
	public string Description { get; set; }

	[JsonProperty]
	public string Author { get; set; }

	public int ResourceId { get; set; }

	public bool HasConfig { get; set; }

	public bool HasMessages { get; set; }

	public bool HasConditionals { get; set; }

	[JsonProperty]
	public TimeSpan CompileTime { get; set; }

	[JsonProperty]
	public TimeSpan InternalCallHookGenTime { get; set; }

	[JsonProperty]
	public ModLoader.Trace[] CompileWarnings { get; set; }

	public string InternalCallHookSource { get; set; }

	[JsonProperty]
	public string FilePath { get; set; }

	[JsonProperty]
	public string FileName { get; set; }

	public string Filename => FileName;

	public virtual bool ManualCommands => false;

	public Plugin[] Requires { get; set; }

	public DynamicConfigFile Config { get; internal set; }

	public bool IsLoaded { get; set; }

	public override void TrackStart()
	{
		if (!IsCorePlugin)
		{
			base.TrackStart();
		}
	}

	public override void TrackEnd()
	{
		if (!IsCorePlugin)
		{
			base.TrackEnd();
		}
	}

	public static implicit operator bool(Plugin target)
	{
		return target != null;
	}

	public virtual bool IInit()
	{
		BuildHookCache(BindingFlags.Instance | BindingFlags.NonPublic);
		using (TimeMeasure.New($"Processing PluginReferences on '{this}'"))
		{
			if (!InternalApplyPluginReferences())
			{
				Logger.Warn("Failed vibe check " + ToPrettyString());
				return false;
			}
		}
		if (Hooks != null && !ManualSubscriptions)
		{
			string fileName = FileName ?? $"{this}";
			using (TimeMeasure.New("Processing Hooks on '" + ToPrettyString() + "'"))
			{
				foreach (uint hook in Hooks)
				{
					Community.Runtime.HookManager.Subscribe(HookStringPool.GetOrAdd(hook), fileName);
				}
			}
		}
		CallHook("Init");
		TrackInit();
		if (!ApplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginInit))
		{
			return false;
		}
		return true;
	}

	internal virtual bool ILoad()
	{
		using (TimeMeasure.New("Load on '" + ToPrettyString() + "'"))
		{
			IsLoaded = true;
			CallHook("OnLoaded");
			CallHook("Loaded");
		}
		using (TimeMeasure.New("Load.PendingRequirees on '" + ToPrettyString() + "'"))
		{
			List<string> requirees = ModLoader.GetRequirees(this);
			if (requirees != null)
			{
				foreach (string item in requirees)
				{
					Logger.Warn(" [" + base.Name + "] Loading '" + Path.GetFileNameWithoutExtension(item) + "' to parent's request: '" + ToPrettyString() + "'");
					Community.Runtime.ScriptProcessor.Prepare(item);
					Community.Runtime.ZipScriptProcessor.Prepare(item);
				}
				ModLoader.ClearPendingRequirees(this);
			}
		}
		Load();
		if (!ApplyOrderedPatches(AutoPatchAttribute.Orders.AfterPluginLoad))
		{
			return false;
		}
		return true;
	}

	public virtual void Load()
	{
	}

	public virtual void IUnload()
	{
		try
		{
			using (TimeMeasure.New($"IUnload.UnprocessHooks on '{this}'"))
			{
				if (Hooks != null)
				{
					foreach (uint hook in Hooks)
					{
						Community.Runtime.HookManager.Unsubscribe(HookStringPool.GetOrAdd(hook), FileName);
					}
				}
				MethodInfo[] methods = base.HookableType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					InternalHooks.Handle(methodInfo.Name, subscribed: false);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed calling Plugin.IUnload.UnprocessHooks on {this}", ex);
		}
		base.HasInitialized = false;
	}

	internal bool InternalApplyPluginReferences()
	{
		if (PluginReferences == null)
		{
			return true;
		}
		foreach (PluginReferenceAttribute pluginReference in PluginReferences)
		{
			FieldInfo field = pluginReference.Field;
			string text = (string.IsNullOrEmpty(pluginReference.Name) ? field.Name : pluginReference.Name);
			string initial = Path.Combine(Defines.GetScriptsFolder(), text + ".cs");
			try
			{
				Plugin plugin = null;
				if (field.FieldType.Name != "Plugin" && field.FieldType.Name != "RustPlugin" && field.FieldType.Name != "CovalencePlugin" && field.FieldType.Name != "CarbonPlugin")
				{
					InfoAttribute customAttribute = field.FieldType.GetCustomAttribute<InfoAttribute>();
					if (customAttribute == null)
					{
						Logger.Warn("You're trying to reference a non-plugin instance: " + text + "[" + field.FieldType.Name + "]");
						continue;
					}
					plugin = Community.Runtime.Core.plugins.Find(customAttribute.Title);
					goto IL_014b;
				}
				plugin = Community.Runtime.Core.plugins.Find(text);
				goto IL_014b;
				IL_014b:
				if (plugin != null)
				{
					VersionNumber versionNumber = new VersionNumber(pluginReference.MinVersion);
					if (versionNumber.IsValid() && plugin.Version < versionNumber)
					{
						Logger.Warn(string.Format("Plugin '{0} by {1} v{2}' references a required plugin which is outdated: {3} by {4} v{5} < v{6}", new object[7] { base.Name, Author, Version, plugin.Name, plugin.Author, plugin.Version, versionNumber }));
						return false;
					}
					field.SetValue(this, plugin);
					if (pluginReference.IsRequired)
					{
						ModLoader.AddPendingRequiree(plugin, this);
					}
				}
				else
				{
					field.SetValue(this, null);
					if (pluginReference.IsRequired)
					{
						ModLoader.PostBatchFailedRequirees.Add(FilePath);
						ModLoader.AddPendingRequiree(initial, FilePath);
						Logger.Warn(string.Format("Plugin '{0} by {1} v{2}' references a required plugin which is not loaded: {3}", new object[4] { base.Name, Author, Version, text }));
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Plugin '" + ToPrettyString() + "' failed to assign PluginReference on field " + text + " (" + field.FieldType.Name + ")", ex);
			}
		}
		return true;
	}

	internal void IUnloadDependantPlugins()
	{
		try
		{
			using (TimeMeasure.New("IUnload.UnloadRequirees on '" + ToPrettyString() + "'"))
			{
				List<ModLoader.Package> list = Pool.Get<List<ModLoader.Package>>();
				list.AddRange(ModLoader.Packages);
				List<Plugin> list2 = Pool.Get<List<Plugin>>();
				foreach (ModLoader.Package package in ModLoader.Packages)
				{
					list2.Clear();
					list2.AddRange(package.Plugins);
					foreach (Plugin item in list2.Where((Plugin plugin2) => plugin2.Requires != null && plugin2.Requires.Contains(this)))
					{
						Logger.Warn(" [" + base.Name + "] Unloading '" + item.ToPrettyString() + "' because parent '" + ToPrettyString() + "' has been unloaded.");
						ModLoader.AddPendingRequiree(this, item);
						IBaseProcessor processor = item.Processor;
						if (processor is IScriptProcessor scriptProcessor)
						{
							scriptProcessor.Get<IScriptProcessor.IScript>(item.FileName)?.Dispose();
						}
						if (item is RustPlugin plugin)
						{
							ModLoader.UninitializePlugin(plugin);
						}
					}
				}
				Pool.FreeUnmanaged<ModLoader.Package>(ref list);
				Pool.FreeUnmanaged<Plugin>(ref list2);
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed calling Plugin.IUnload.UnloadRequirees on " + ToPrettyString(), ex);
		}
	}

	public static void InternalApplyAllPluginReferences()
	{
		List<RustPlugin> list = Pool.Get<List<RustPlugin>>();
		foreach (ModLoader.Package package in ModLoader.Packages)
		{
			foreach (RustPlugin plugin in package.Plugins)
			{
				if (!plugin.InternalApplyPluginReferences())
				{
					list.Add(plugin);
				}
			}
		}
		foreach (RustPlugin item in list)
		{
			ModLoader.UninitializePlugin(item);
		}
		Pool.FreeUnmanaged<RustPlugin>(ref list);
	}

	public void SetProcessor(IBaseProcessor processor, IBaseProcessor.IProcess process)
	{
		Processor = processor;
		ProcessorProcess = process;
	}

	public T Call<T>(string hook)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook));
	}

	public T Call<T>(string hook, object arg1)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1);
	}

	public T Call<T>(string hook, object arg1, object arg2)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public T Call<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public T Call<T>(string hook, object[] args)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), args);
	}

	public object Call(string hook)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook));
	}

	public object Call(string hook, object arg1)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1);
	}

	public object Call(string hook, object arg1, object arg2)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2);
	}

	public object Call(string hook, object arg1, object arg2, object arg3)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public object Call(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
	}

	public object Call(string hook, object[] args)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), args);
	}

	public T CallHook<T>(string hook)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook));
	}

	public T CallHook<T>(string hook, object arg1)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1);
	}

	public T CallHook<T>(string hook, object arg1, object arg2)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public T CallHook<T>(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
	}

	public T CallHook<T>(string hook, object[] args)
	{
		return HookCaller.CallHook<T>(this, HookStringPool.GetOrAdd(hook), args);
	}

	public object CallHook(string hook)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook));
	}

	public object CallHook(string hook, object arg1)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1);
	}

	public object CallHook(string hook, object arg1, object arg2)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
	}

	public object CallHook(string hook, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
	}

	public object CallHook(string hook, object[] args)
	{
		return HookCaller.CallHook(this, HookStringPool.GetOrAdd(hook), args);
	}

	public virtual void HandleAddedToManager(PluginManager manager)
	{
	}

	public virtual void HandleRemovedFromManager(PluginManager manager)
	{
	}

	protected void AddCovalenceCommand(string[] commands, string callback, string perm)
	{
		foreach (string command in commands)
		{
			cmd.AddCovalenceCommand(command, this, callback, null, null, new string[1] { perm });
		}
		if (!string.IsNullOrEmpty(perm) && !permission.PermissionExists(perm))
		{
			permission.RegisterPermission(perm, this);
		}
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

	public void QueueWorkerThread(Action<object> callback)
	{
		ThreadPool.QueueUserWorkItem(delegate(object context)
		{
			try
			{
				callback(context);
			}
			catch (Exception ex)
			{
				Logger.Error($"Worker thread callback failed in '{base.Name} v{Version}'", ex);
			}
		});
	}

	protected virtual void LoadConfig()
	{
		Config = new DynamicConfigFile(Path.Combine(Manager.ConfigPath, base.Name + ".json"));
		if (!Config.Exists())
		{
			CallHook("LoadDefaultConfig");
			SaveConfig();
		}
		try
		{
			if (Config.Exists())
			{
				Config.Load();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to load config file (is the config file corrupt?)", ex);
		}
	}

	protected virtual void LoadDefaultConfig()
	{
	}

	protected virtual void SaveConfig()
	{
		if (Config == null)
		{
			return;
		}
		try
		{
			if (Config.Count() > 0)
			{
				Config.Save();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to save config file (does the config have illegal objects in it?) (" + ex.Message + ")", ex);
		}
	}

	protected virtual void LoadDefaultMessages()
	{
	}

	public virtual void Dispose()
	{
		try
		{
			using (TimeMeasure.New($"IUnload.Disposal on '{this}'"))
			{
				IgnoredHooks?.Clear();
				HookPool?.Clear();
				Hooks?.Clear();
				HookMethods?.Clear();
				PluginReferences?.Clear();
				IgnoredHooks = null;
				HookPool = null;
				Hooks = null;
				HookMethods = null;
				PluginReferences = null;
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed calling Plugin.IUnload.Disposal on {this}", ex);
		}
		IsLoaded = false;
	}
}
