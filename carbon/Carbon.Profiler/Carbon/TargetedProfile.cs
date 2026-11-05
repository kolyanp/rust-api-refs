using System;
using System.IO;
using System.Linq;
using Carbon.Components;
using Carbon.Profiler;
using UnityEngine;

namespace Carbon;

public static class TargetedProfile
{
	private const string SelfName = "Carbon.Profiler";

	private const float PluginLoadTimeout = 60f;

	private static string _name;

	private static MonoProfilerConfig.ProfileTypes _type;

	private static float _duration;

	private static MonoProfiler.ProfilerArgs _flags;

	private static bool _modWasLoaded;

	private static Action _pluginLoadTimeout;

	public static bool IsActive => _name != null;

	public static string Name => _name;

	public static string StartMod(string name, float duration, MonoProfiler.ProfilerArgs flags)
	{
		if (name.Equals("Carbon.Profiler", StringComparison.OrdinalIgnoreCase))
		{
			return "Carbon.Profiler can't profile itself";
		}
		if (!File.Exists(Path.Combine(HarmonyLoader.modPath, name + ".dll")))
		{
			return "Couldn't find '" + name + ".dll' in '" + HarmonyLoader.modPath + "'";
		}
		if (!MonoProfiler.Start())
		{
			return "Mono profiler couldn't start. Check the server log.";
		}
		Begin(name, MonoProfilerConfig.ProfileTypes.Harmony, duration, flags);
		_modWasLoaded = HarmonyLoader.GetHarmonyMods().Any((HarmonyModInfo mod) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return mod.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
		});
		MonoProfilerConfig.Instance.TemporaryHarmonyMods.Add(name);
		if (!HarmonyLoader.TryLoadMod(name))
		{
			End();
			return "Couldn't load '" + name + "', see the HarmonyLoader log";
		}
		if (!Record())
		{
			End();
			return "Couldn't start recording. Check the server log.";
		}
		return $"Reloaded '{name}' with profiler hooks, profiling it for {duration}s";
	}

	public static string StartPlugin(string name, float duration, MonoProfiler.ProfilerArgs flags)
	{
		if (!HarmonyProfiler.IsOxideInstalled)
		{
			return "carbon.profile_plugin needs Oxide; use carbon.profile_mod for HarmonyMods";
		}
		string fileName = OxidePlugins.FindFileName(name);
		if (fileName == null)
		{
			return "Couldn't find an Oxide plugin named '" + name + "'";
		}
		if (!MonoProfiler.Start())
		{
			return "Mono profiler couldn't start. Check the server log.";
		}
		Begin(fileName, MonoProfilerConfig.ProfileTypes.Plugin, duration, flags);
		MonoProfilerConfig.Instance.TemporaryPlugins.Add(fileName);
		_pluginLoadTimeout = () =>
		{
			Debug.LogWarning((object)$"'{fileName}' didn't load within {60f}s, cancelled profiling it");
			End();
		};
		HarmonyProfiler.Runner.Invoke(_pluginLoadTimeout, 60f);
		if (!OxidePlugins.Recompile(fileName))
		{
			End();
			return "Oxide couldn't reload '" + fileName + "'";
		}
		return "Recompiling '" + fileName + "' with profiler hooks; profiling starts once it has loaded";
	}

	public static void OnPluginLoaded(string fileName)
	{
		if (_type == MonoProfilerConfig.ProfileTypes.Plugin && !(_name != fileName) && _pluginLoadTimeout != null)
		{
			CancelPluginLoadTimeout();
			if (!Record())
			{
				End();
			}
		}
	}

	public static void End()
	{
		if (_name == null)
		{
			return;
		}
		string name = _name;
		_name = null;
		MonoProfiler.RecordingStopped -= End;
		CancelPluginLoadTimeout();
		if (_type == MonoProfilerConfig.ProfileTypes.Plugin)
		{
			MonoProfilerConfig.Instance.TemporaryPlugins.Remove(name);
			HarmonyProfiler.Runner.Invoke((Action)(() =>
			{
				OxidePlugins.Recompile(name);
			}), 0f);
			return;
		}
		MonoProfilerConfig.Instance.TemporaryHarmonyMods.Remove(name);
		if (_modWasLoaded)
		{
			HarmonyLoader.TryLoadMod(name);
		}
		else
		{
			HarmonyLoader.TryUnloadMod(name);
		}
	}

	private static void Begin(string name, MonoProfilerConfig.ProfileTypes type, float duration, MonoProfiler.ProfilerArgs flags)
	{
		_name = name;
		_type = type;
		_duration = duration;
		_flags = flags | MonoProfiler.ProfilerArgs.Calls;
	}

	private static bool Record()
	{
		if (MonoProfiler.IsRecording)
		{
			Debug.LogWarning((object)("Another recording started while '" + _name + "' was reloading, cancelled profiling it"));
			return false;
		}
		MonoProfiler.ToggleProfilingTimed(_duration, _flags, (MonoProfiler.ProfilerArgs _) =>
		{
			HarmonyProfiler.ProfileSample.Resample();
			MonoProfiler.Clear();
		});
		if (!MonoProfiler.IsRecording)
		{
			return false;
		}
		MonoProfiler.RecordingStopped += End;
		return true;
	}

	private static void CancelPluginLoadTimeout()
	{
		if (_pluginLoadTimeout != null)
		{
			HarmonyProfiler.Runner.CancelInvoke(_pluginLoadTimeout);
			_pluginLoadTimeout = null;
		}
	}
}
