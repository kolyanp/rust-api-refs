using System;
using System.IO;
using System.Reflection;
using Carbon.Components;
using Carbon.Profiler;
using HarmonyLib;
using UnityEngine;

namespace Carbon;

public class Patches
{
	[HarmonyPatch(typeof(Bootstrap), "Init_Tier0")]
	public static class Bootstrap_Init_Tier0
	{
		public static void Postfix()
		{
			if (!HarmonyProfiler.IsCarbonInstalled)
			{
				HarmonyProfiler.InstallCommands();
			}
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	public static class OxideMod_PluginLoaded
	{
		public static bool Prepare()
		{
			return HarmonyProfiler.IsOxideInstalled;
		}

		public static void Prefix(object plugin)
		{
			Type type = plugin.GetType();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension((string)type.GetProperty("Filename").GetValue(plugin));
			if (!string.IsNullOrEmpty(fileNameWithoutExtension))
			{
				MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Plugin, type.Assembly, fileNameWithoutExtension, incremental: true);
				Debug.Log((object)("MonoProfiler.TryStartProfileFor Plugin: " + fileNameWithoutExtension));
			}
		}
	}

	[HarmonyPatch]
	public static class Extension_Constructor
	{
		public static bool Prepare()
		{
			return HarmonyProfiler.IsOxideInstalled;
		}

		private static MethodBase TargetMethod()
		{
			return AccessTools.TypeByName("Oxide.Core.Extensions.Extension").GetConstructors()[0];
		}

		public static void Prefix(object manager, object __instance)
		{
			Type type = __instance.GetType();
			HarmonyProfiler.Runner.Invoke((Action)delegate
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension((string)type.GetProperty("Filename").GetValue(__instance));
				if (!string.IsNullOrEmpty(fileNameWithoutExtension))
				{
					MonoProfiler.TryStartProfileFor(MonoProfilerConfig.ProfileTypes.Extension, type.Assembly, fileNameWithoutExtension);
					Debug.Log((object)("MonoProfiler.TryStartProfileFor Extension: " + fileNameWithoutExtension));
				}
			}, 0.1f);
		}
	}
}
