using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using API.Hooks;
using Carbon.Base;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Plugins;
using Facepunch;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Plugins;
using Rust;

namespace Carbon.Components;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Analytics
{
	public static readonly Dictionary<string, object> Metrics = new Dictionary<string, object>();

	public static Analytics Singleton = default(Analytics);

	public static bool Enabled => Community.Runtime.Analytics.Enabled;

	public Analytics Include(string key, object value)
	{
		Metrics[key] = value;
		return this;
	}

	public Analytics Submit(string eventName)
	{
		Community.Runtime.Analytics.LogEvents(eventName);
		Dispose();
		return this;
	}

	public static void Dispose()
	{
		Metrics.Clear();
	}

	public static void on_server_startup()
	{
		if (Enabled)
		{
			Singleton.Include("carbon", Community.Runtime.Analytics.Version + "/" + Community.Runtime.Analytics.Platform + "/" + Community.Runtime.Analytics.Protocol).Include("carbon_informational", Community.Runtime.Analytics.InformationalVersion).Include("carbon_build", Build.Git.Author + " on " + Build.Git.Branch + " [" + Build.Git.HashLong + "]")
				.Include("rust", BuildInfo.Current.Build.Number + "/" + Protocol.printable)
				.Submit("on_server_startup");
		}
	}

	public static void on_server_initialized()
	{
		if (!Enabled)
		{
			return;
		}
		Singleton.Include("plugin_count", ModLoader.Packages.Sum((ModLoader.Package x) => x.Plugins.Count)).Include("plugins_totalmemoryused", ModLoader.Packages.Sum((ModLoader.Package x) => x.Plugins.Sum((RustPlugin y) => y.TotalMemoryUsed)).Format(ByteEx.ByteTypes.Auto, shortName: true, "0", "{0}{1}").ToLower() ?? "").Include("plugins_totalhooktime", $"{ModLoader.Packages.Sum((ModLoader.Package x) => x.Plugins.Sum((RustPlugin y) => y.TotalHookTime.TotalMilliseconds)).RoundUpToNearestCount(100.0):0}ms")
			.Include("extension_count", Community.Runtime.AssemblyEx.Extensions.Loaded.Count)
			.Include("module_count", Community.Runtime.AssemblyEx.Modules.Loaded.Count)
			.Include("hook_count", Community.Runtime.HookManager.LoadedDynamicHooks.Count((IHook x) => x.IsInstalled) + Community.Runtime.HookManager.LoadedStaticHooks.Count((IHook x) => x.IsInstalled))
			.Submit("on_server_initialized");
	}

	public static void plugin_constructor_failure(RustPlugin plugin)
	{
		if (Enabled)
		{
			Singleton.Include("plugin", $"{plugin.Name} v{plugin.Version} by {plugin.Author}").Submit("plugin_constructor_failure");
		}
	}

	public static void batch_plugin_types()
	{
		if (!Enabled)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (RustPlugin item in ModLoader.Packages.SelectMany((ModLoader.Package package) => package.Plugins))
		{
			if (item.HookableType.BaseType == typeof(CovalencePlugin))
			{
				num2++;
			}
			else if (item.HookableType.BaseType == typeof(RustPlugin))
			{
				num++;
			}
			else if (item.HookableType.BaseType == typeof(CarbonPlugin))
			{
				num3++;
			}
		}
		Singleton.Include("rustplugin", $"{num:n0}").Include("covalenceplugin", $"{num2:n0}").Include("carbonplugin", $"{num3:n0}")
			.Submit("batch_plugin_types");
	}

	public static void plugin_time_warn(string readableHook, Plugin basePlugin, double afterHookTime, double totalMemory, bool lagSpike, BaseHookable.CachedHook cachedHook, BaseHookable hookable)
	{
		if (Enabled)
		{
			Singleton.Include("name", readableHook + " (" + basePlugin.ToPrettyString() + ")").Include("time", $"{afterHookTime.RoundUpToNearestCount(50.0)}ms").Include("memory", totalMemory.Format().ToLower() ?? "")
				.Include("fires", $"{cachedHook?.TimesFired}")
				.Include("hasgc", hookable.HasGCCollected)
				.Include("lagspike", lagSpike)
				.Submit("plugin_time_warn");
		}
	}

	public static void plugin_native_compile_fail(ISource initialSource, Exception ex)
	{
		if (Enabled && initialSource != null)
		{
			Singleton.Include("file", initialSource.ContextFilePath).Include("stacktrace", "(" + ex.Message + ") " + ex.StackTrace).Submit("plugin_native_compile_fail");
		}
	}

	public static void admin_module_greet_continue()
	{
		if (Enabled)
		{
			Singleton.Submit("admin_module_greet_continue");
		}
	}

	public static void profiler_started(MonoProfiler.ProfilerArgs args, bool timed)
	{
		if (Enabled)
		{
			Singleton.Include("settings", $"{Community.Runtime.MonoProfilerConfig.TrackCalls}tc " + $"{Community.Runtime.MonoProfilerConfig.Assemblies.Count}a " + $"{Community.Runtime.MonoProfilerConfig.Plugins.Count}p " + $"{Community.Runtime.MonoProfilerConfig.Modules.Count}m " + $"{Community.Runtime.MonoProfilerConfig.Extensions.Count}e " + $"{Community.Runtime.MonoProfilerConfig.Harmony.Count}h").Include("args", $"{args} {timed}t").Submit("profiler_started");
		}
	}

	public static void profiler_ended(MonoProfiler.ProfilerArgs args, double duration, bool timed)
	{
		if (Enabled)
		{
			Singleton.Include("settings", $"{Community.Runtime.MonoProfilerConfig.TrackCalls}tc " + $"{Community.Runtime.MonoProfilerConfig.Assemblies.Count}a " + $"{Community.Runtime.MonoProfilerConfig.Plugins.Count}p " + $"{Community.Runtime.MonoProfilerConfig.Modules.Count}m " + $"{Community.Runtime.MonoProfilerConfig.Extensions.Count}e " + $"{Community.Runtime.MonoProfilerConfig.Harmony.Count}h").Include("args", $"{args} {timed}t").Include("duration", TimeEx.Format(duration).ToLower() ?? "")
				.Submit("profiler_started");
		}
	}

	public static void profiler_tl_started(MonoProfiler.ProfilerArgs args)
	{
		if (Enabled)
		{
			Singleton.Include("settings", $"{Community.Runtime.MonoProfilerConfig.TrackCalls}tc " + $"{Community.Runtime.MonoProfilerConfig.Assemblies.Count}a " + $"{Community.Runtime.MonoProfilerConfig.Plugins.Count}p " + $"{Community.Runtime.MonoProfilerConfig.Modules.Count}m " + $"{Community.Runtime.MonoProfilerConfig.Extensions.Count}e " + $"{Community.Runtime.MonoProfilerConfig.Harmony.Count}h").Include("args", $"{args}").Submit("profiler_tl_started");
		}
	}

	public static void profiler_tl_ended(MonoProfiler.ProfilerArgs args, double duration, MonoProfiler.TimelineRecording.StatusTypes status)
	{
		if (Enabled)
		{
			Singleton.Include("settings", $"{Community.Runtime.MonoProfilerConfig.TrackCalls}tc " + $"{Community.Runtime.MonoProfilerConfig.Assemblies.Count}a " + $"{Community.Runtime.MonoProfilerConfig.Plugins.Count}p " + $"{Community.Runtime.MonoProfilerConfig.Modules.Count}m " + $"{Community.Runtime.MonoProfilerConfig.Extensions.Count}e " + $"{Community.Runtime.MonoProfilerConfig.Harmony.Count}h" + $"{(int)status}st").Include("args", $"{args}").Include("duration", TimeEx.Format(duration).ToLower() ?? "")
				.Submit("profiler_tl_ended");
		}
	}

	public static void codefling_login()
	{
		if (Enabled)
		{
			Singleton.Submit("codefling_login");
		}
	}

	public static void perms_migration(Permission.SerializationMode mode, int groups, int users)
	{
		if (Enabled)
		{
			Singleton.Include("mode", mode.ToString()).Include("groups", groups.ToString("n0")).Include("users", users.ToString("n0"))
				.Submit("perms_migration");
		}
	}

	public static void webcontrolpanel_serverconnect()
	{
		if (Enabled)
		{
			Singleton.Submit("webcontrolpanel_serverconnect");
		}
	}

	public static void webcontrolpanel_clientconnect()
	{
		if (Enabled)
		{
			Singleton.Submit("webcontrolpanel_clientconnect");
		}
	}
}
