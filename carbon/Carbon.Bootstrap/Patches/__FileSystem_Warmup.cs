using System;
using System.Threading;
using API.Events;
using Carbon;
using HarmonyLib;

namespace Patches;

internal static class __FileSystem_Warmup
{
	[HarmonyPatch(typeof(FileSystem_Warmup), "Run", new Type[]
	{
		typeof(Action<string>),
		typeof(string),
		typeof(CancellationToken)
	})]
	internal static class __Run
	{
		public static void Prefix()
		{
			Bootstrap.Events.Trigger(CarbonEvent.FileSystemWarmup, EventArgs.Empty);
		}

		public static void Postfix()
		{
			Bootstrap.Events.Trigger(CarbonEvent.FileSystemWarmupComplete, EventArgs.Empty);
		}
	}
}
