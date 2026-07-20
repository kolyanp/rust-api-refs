using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Events;
using Carbon.Compat.Legacy.EventCompat;
using Carbon.Core;
using JetBrains.Annotations;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;

namespace Carbon.Compat.Lib;

[UsedImplicitly(/*Could not decode attribute arguments.*/)]
public static class OxideCompat
{
	public class PluginManagerEvent : OxideEvents.Event<Plugin, PluginManager>
	{
	}

	internal const string LEGACY_MSG = "Used for oxide backwards compatibility";

	internal static Dictionary<Assembly, ModLoader.Package> modPackages;

	public static void RegisterPluginLoader(ExtensionManager self, PluginLoader loader, Extension oxideExt)
	{
		self.RegisterPluginLoader(loader);
		string name = Assembly.GetCallingAssembly().GetName().Name;
		Assembly assembly = ((oxideExt != null) ? oxideExt.GetType().Assembly : loader.GetType().Assembly);
		string text = ((oxideExt != null) ? oxideExt.Name : assembly.GetName().Name);
		string text2 = ((oxideExt != null) ? oxideExt.Author : "Carbon.Compat");
		if (!modPackages.TryGetValue(assembly, out var value))
		{
			ModLoader.Package package = (modPackages[assembly] = ModLoader.Package.Get(text + " - " + text2 + " (Oxide Extension)", isCoreMod: false));
			ModLoader.RegisterPackage(value = package);
		}
		Type[] corePlugins = loader.CorePlugins;
		foreach (Type type in corePlugins)
		{
			if (type.IsAbstract)
			{
				continue;
			}
			try
			{
				ModLoader.InitializePlugin(type, out var plugin, value, (oxideExt == null) ? null : ((Action<RustPlugin>)delegate(RustPlugin rustPlugin)
				{
					rustPlugin.Version = oxideExt.Version;
					if (rustPlugin.Author == "Carbon.Compat" && !string.IsNullOrWhiteSpace(oxideExt.Author))
					{
						rustPlugin.Author = oxideExt.Author;
					}
				}), precompiled: true);
				plugin.IsCorePlugin = true;
				plugin.IsExtension = true;
			}
			catch (Exception arg)
			{
				Logger.Error($"Failed to load plugin {type.Name} in oxide extension {name}: {arg}");
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetMessage1(Lang lang, string key, Plugin plugin, string player)
	{
		return lang.GetMessage(key, plugin, player);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddConsoleCommand1(Command cmd, string name, Plugin plugin, Func<Arg, bool> callback)
	{
		cmd.AddConsoleCommand(name, plugin, callback);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddChatCommand1(Command cmd, string name, Plugin plugin, Action<BasePlayer, string, string[]> callback)
	{
		cmd.AddChatCommand(name, plugin, callback);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T OxideCallHookGeneric<T>(string hook, params object[] args)
	{
		return (T)Interface.Call<T>(hook, args);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetExtensionDirectory(OxideMod _)
	{
		return Defines.GetExtensionsFolder();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Timer TimerOnce(Timers timers, float delay, Action callback, Plugin owner = null)
	{
		return timers.Once(delay, callback);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Timer TimerRepeat(Timers timers, float delay, int reps, Action callback, Plugin owner = null)
	{
		return timers.Repeat(delay, reps, callback);
	}

	static OxideCompat()
	{
		modPackages = new Dictionary<Assembly, ModLoader.Package>();
		Community.Runtime.Events.Subscribe(CarbonEvent.PluginLoaded, delegate(EventArgs args)
		{
			HandlePluginIO(loaded: true, (CarbonEventArgs)args);
		});
		Community.Runtime.Events.Subscribe(CarbonEvent.PluginUnloaded, delegate(EventArgs args)
		{
			HandlePluginIO(loaded: false, (CarbonEventArgs)args);
		});
	}

	internal static void HandlePluginIO(bool loaded, CarbonEventArgs args)
	{
		RustPlugin rustPlugin = (RustPlugin)args.Payload;
		if ((loaded ? rustPlugin.OnAddedToManager : rustPlugin.OnRemovedFromManager) is PluginManagerEvent pluginManagerEvent)
		{
			pluginManagerEvent.Invoke(rustPlugin, rustPlugin.Manager);
		}
	}

	public static PluginManagerEvent OnAddedToManagerCompat(Plugin plugin)
	{
		if (!(plugin.OnAddedToManager is PluginManagerEvent))
		{
			plugin.OnAddedToManager = new PluginManagerEvent();
		}
		return (PluginManagerEvent)plugin.OnAddedToManager;
	}

	public static PluginManagerEvent OnRemovedFromManagerCompat(Plugin plugin)
	{
		if (!(plugin.OnRemovedFromManager is PluginManagerEvent))
		{
			plugin.OnRemovedFromManager = new PluginManagerEvent();
		}
		return (PluginManagerEvent)plugin.OnRemovedFromManager;
	}
}
