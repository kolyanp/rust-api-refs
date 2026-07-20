using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Carbon.Compat.Patches.Harmony;
using HarmonyLib;
using JetBrains.Annotations;

namespace Carbon.Compat.Lib;

[UsedImplicitly(/*Could not decode attribute arguments.*/)]
public static class HarmonyCompat
{
	internal static HashSet<Type> typeCache = new HashSet<Type>();

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static DynamicMethod InstancePatchCompat(Harmony instance, MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
	{
		MethodBase method = new StackTrace().GetFrame(1).GetMethod();
		HarmonyPatchProcessor.RegisterPatch(original, $"{method.DeclaringType.Assembly.GetName().Name} - {method}", instance);
		HookProcessor.HookReload();
		instance.Patch(original, prefix, postfix, transpiler, (HarmonyMethod)null);
		return null;
	}

	public static void PatchProcessorCompat(Harmony instance, Type type, HarmonyMethod attributes)
	{
		if (typeCache.Contains(type))
		{
			return;
		}
		typeCache.Add(type);
		MethodInfo[] methods = type.GetMethods();
		MethodInfo postfix = methods.FirstOrDefault((MethodInfo x) => x.GetCustomAttributes(typeof(HarmonyPostfix), inherit: false).Length != 0);
		MethodInfo prefix = methods.FirstOrDefault((MethodInfo x) => x.GetCustomAttributes(typeof(HarmonyPrefix), inherit: false).Length != 0);
		MethodInfo transpiler = methods.FirstOrDefault((MethodInfo x) => x.GetCustomAttributes(typeof(HarmonyTranspiler), inherit: false).Length != 0);
		MethodInfo methodInfo = methods.FirstOrDefault((MethodInfo x) => x.GetCustomAttributes(typeof(HarmonyTargetMethods), inherit: false).Length != 0 || x.GetCustomAttributes(typeof(HarmonyTargetMethod), inherit: false).Length != 0) ?? throw new NullReferenceException("Failed to find target method in " + type.FullName);
		IEnumerable<MethodBase> enumerable = null;
		MethodBase methodBase = null;
		if (methodInfo.ReturnType == typeof(IEnumerable<MethodBase>))
		{
			enumerable = (IEnumerable<MethodBase>)methodInfo.Invoke(null, (methodInfo.GetParameters().Length != 0) ? new object[1] : Array.Empty<object>());
		}
		else
		{
			if (!(methodInfo.ReturnType == typeof(MethodBase)))
			{
				return;
			}
			methodBase = (MethodBase)methodInfo.Invoke(null, (methodInfo.GetParameters().Length != 0) ? new object[1] : Array.Empty<object>());
		}
		bool flag = true;
		while (true)
		{
			if (enumerable != null)
			{
				foreach (MethodBase item in enumerable)
				{
					ProcessType(item, flag);
				}
			}
			else if (methodBase != null)
			{
				ProcessType(methodBase, flag);
			}
			if (flag)
			{
				flag = false;
				HookProcessor.HookReload();
				continue;
			}
			break;
		}
		void ProcessType(MethodBase original, bool pregen)
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			if (pregen)
			{
				string name = type.Assembly.GetName().Name;
				HarmonyPatchProcessor.RegisterPatch(name + ".dll", original.DeclaringType.Assembly.GetName().Name, original.Name, original.DeclaringType.FullName, name + " - " + type.FullName, instance);
				return;
			}
			try
			{
				if (!AccessTools.IsDeclaredMember<MethodBase>(original))
				{
					original = AccessTools.GetDeclaredMember<MethodBase>(original);
				}
				PatchProcessor val = new PatchProcessor(instance, original);
				if (postfix != null)
				{
					val.AddPostfix(postfix);
				}
				if (prefix != null)
				{
					val.AddPrefix(prefix);
				}
				if (transpiler != null)
				{
					val.AddTranspiler(transpiler);
				}
				val.Patch();
			}
			catch (Exception ex)
			{
				Logger.Error("[HarmonyCompat] Failed to patch '" + original.Name + "'", ex);
			}
		}
	}
}
