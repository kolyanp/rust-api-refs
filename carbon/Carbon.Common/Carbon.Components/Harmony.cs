using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Facepunch;
using HarmonyLib;

namespace Carbon.Components;

public class Harmony
{
	public struct PatchInfoEntry
	{
		public string ParentAssemblyName;

		public string AssemblyName;

		public string TypeName;

		public string MethodName;

		public string Reason;

		public Harmony Harmony;

		public MethodBase runtime_method;

		public PatchInfoEntry(string parentAssemblyName, string assemblyName, string methodName, string typeName, string reason, Harmony harmony)
		{
			runtime_method = null;
			ParentAssemblyName = parentAssemblyName;
			AssemblyName = assemblyName;
			MethodName = methodName;
			TypeName = typeName;
			Reason = reason;
			Harmony = harmony;
		}

		public PatchInfoEntry(string parentAssemblyName, MethodBase method, Harmony harmony)
		{
			AssemblyName = null;
			TypeName = null;
			MethodName = null;
			Reason = null;
			ParentAssemblyName = parentAssemblyName;
			Harmony = harmony;
			runtime_method = method;
		}

		public int Unpatch()
		{
			int num = 0;
			if (Harmony == null)
			{
				return num;
			}
			try
			{
				if (Harmony != null)
				{
					foreach (MethodBase patchedMethod in Harmony.GetPatchedMethods())
					{
						Logger.Warn("[" + Harmony.Id + "] Unpatched '" + patchedMethod.Name + "' method. (" + patchedMethod.DeclaringType.Name + ")");
						num++;
					}
				}
				Harmony.UnpatchAll(Harmony.Id);
				Harmony = null;
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to unpatch '" + MethodName + "' (" + TypeName + ")", ex);
			}
			return num;
		}
	}

	public static Dictionary<Assembly, List<IHarmonyModHooks>> ModHooks = new Dictionary<Assembly, List<IHarmonyModHooks>>();

	public static List<PatchInfoEntry> CurrentPatches = new List<PatchInfoEntry>();

	public static int PatchAll(Assembly assembly)
	{
		return PatchAll(assembly, null);
	}

	public static int PatchAll(Assembly assembly, string fileName)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		int num = 0;
		string name = assembly.GetName().Name;
		Harmony val = new Harmony("com.compat-harmony." + (string.IsNullOrEmpty(fileName) ? name : fileName));
		foreach (Type item in assembly.GetTypes().Where(delegate(Type x)
		{
			IEnumerable<HarmonyPatch> customAttributes = ((MemberInfo)x).GetCustomAttributes<HarmonyPatch>();
			return customAttributes != null && customAttributes.Count() > 0;
		}))
		{
			try
			{
				List<MethodInfo> list = val.CreateClassProcessor(item).Patch();
				if (list == null || list.Count == 0)
				{
					continue;
				}
				foreach (MethodInfo item2 in list)
				{
					Logger.Warn("[" + val.Id + "] Patched '" + item2.Name + "' method. (" + item.Name + ")");
					num++;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("[" + val.Id + "] Failed to patch '" + item.Name + "'", ex);
			}
		}
		CurrentPatches.Add(new PatchInfoEntry(name + ".dll", name, null, null, null, val));
		return num;
	}

	public static int UnpatchAll(string assembly)
	{
		assembly += ".dll";
		List<PatchInfoEntry> list = Pool.Get<List<PatchInfoEntry>>();
		list.AddRange(CurrentPatches.Where((PatchInfoEntry x) => x.ParentAssemblyName.Equals(assembly)));
		int result = list.Sum((PatchInfoEntry a) => a.Unpatch());
		CurrentPatches.RemoveAll((PatchInfoEntry x) => x.ParentAssemblyName.Equals(assembly));
		Pool.FreeUnmanaged<PatchInfoEntry>(ref list);
		return result;
	}
}
