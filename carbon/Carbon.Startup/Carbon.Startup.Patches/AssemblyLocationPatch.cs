using System.Reflection;
using Carbon.Publicizer;
using HarmonyLib;

namespace Carbon.Startup.Patches;

[HarmonyPatchCategory("location")]
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public class AssemblyLocationPatch
{
	public static void Postfix(Assembly __instance, ref string __result)
	{
		if (Patch.PatchMapping.TryGetValue(__instance, out var value))
		{
			__result = value;
		}
	}
}
