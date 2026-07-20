using System.Linq;
using System.Reflection;
using API.Hooks;
using Carbon.Components;

namespace Carbon.Compat;

internal static class HookProcessor
{
	public static void HookClear()
	{
		foreach (IHook loadedDynamicHook in Community.Runtime.HookManager.LoadedDynamicHooks)
		{
			if (loadedDynamicHook.TargetMethods.Count == 0)
			{
				break;
			}
			MethodBase methodBase = loadedDynamicHook.TargetMethods[0];
			string asmName = methodBase.DeclaringType.Assembly.GetName().Name;
			string typeName = methodBase.DeclaringType.FullName;
			string methodName = methodBase.Name;
			if (Harmony.CurrentPatches.FirstOrDefault((Harmony.PatchInfoEntry x) => x.AssemblyName == asmName && x.TypeName == typeName && x.MethodName == methodName).Harmony != null && (loadedDynamicHook.Options & HookFlags.Patch) != HookFlags.Patch)
			{
				Community.Runtime.HookManager.Unsubscribe(loadedDynamicHook.Identifier, "CCL.Static");
			}
		}
	}

	public static void HookReload()
	{
		foreach (IHook loadedDynamicHook in Community.Runtime.HookManager.LoadedDynamicHooks)
		{
			if (loadedDynamicHook == null || loadedDynamicHook.TargetMethods == null || loadedDynamicHook.TargetMethods.Count == 0)
			{
				return;
			}
			MethodBase methodBase = loadedDynamicHook.TargetMethods[0];
			string asmName = methodBase.DeclaringType.Assembly.GetName().Name;
			string typeName = methodBase.DeclaringType.FullName;
			string methodName = methodBase.Name;
			if (Harmony.CurrentPatches.FirstOrDefault((Harmony.PatchInfoEntry x) => x.AssemblyName == asmName && x.TypeName == typeName && x.MethodName == methodName).Harmony != null && (loadedDynamicHook.Options & HookFlags.Patch) != HookFlags.Patch)
			{
				Community.Runtime.HookManager.Subscribe(loadedDynamicHook.Identifier, "CCL.Static");
			}
		}
		Community.Runtime.HookManager.ForceUpdateHooks();
	}
}
