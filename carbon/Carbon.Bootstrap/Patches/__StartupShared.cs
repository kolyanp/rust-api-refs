using System;
using API.Events;
using Carbon;
using HarmonyLib;

namespace Patches;

[HarmonyPatch(typeof(Bootstrap), "StartupShared")]
internal static class __StartupShared
{
	public static void Prefix()
	{
		Bootstrap.Events.Trigger(CarbonEvent.StartupShared, EventArgs.Empty);
	}

	public static void Postfix()
	{
		Bootstrap.Events.Trigger(CarbonEvent.StartupSharedComplete, EventArgs.Empty);
	}
}
