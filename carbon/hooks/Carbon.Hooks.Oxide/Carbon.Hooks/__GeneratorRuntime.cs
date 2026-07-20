using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace Carbon.Hooks;

internal static class __GeneratorRuntime
{
	private static readonly ConditionalWeakTable<ILGenerator, Dictionary<int, LocalBuilder>> SyntheticLocals = new ConditionalWeakTable<ILGenerator, Dictionary<int, LocalBuilder>>();

	private static int GetOriginalLocalCount(MethodBase method)
	{
		return (method?.GetMethodBody()?.LocalVariables?.Count).GetValueOrDefault();
	}

	private static LocalBuilder GetOrDeclareSyntheticLocal(ILGenerator generator, int requestedSlot, Type localType)
	{
		Dictionary<int, LocalBuilder> orCreateValue = SyntheticLocals.GetOrCreateValue(generator);
		if (orCreateValue.TryGetValue(requestedSlot, out var value))
		{
			return value;
		}
		return orCreateValue[requestedSlot] = generator.DeclareLocal(localType ?? typeof(object));
	}

	private static CodeInstruction CreateOriginalLoad(int requestedSlot)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		return (CodeInstruction)(requestedSlot switch
		{
			0 => (object)new CodeInstruction(OpCodes.Ldloc_0, (object)null), 
			1 => (object)new CodeInstruction(OpCodes.Ldloc_1, (object)null), 
			2 => (object)new CodeInstruction(OpCodes.Ldloc_2, (object)null), 
			3 => (object)new CodeInstruction(OpCodes.Ldloc_3, (object)null), 
			_ => (object)new CodeInstruction(OpCodes.Ldloc, (object)requestedSlot), 
		});
	}

	private static CodeInstruction CreateOriginalStore(int requestedSlot)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		return (CodeInstruction)(requestedSlot switch
		{
			0 => (object)new CodeInstruction(OpCodes.Stloc_0, (object)null), 
			1 => (object)new CodeInstruction(OpCodes.Stloc_1, (object)null), 
			2 => (object)new CodeInstruction(OpCodes.Stloc_2, (object)null), 
			3 => (object)new CodeInstruction(OpCodes.Stloc_3, (object)null), 
			_ => (object)new CodeInstruction(OpCodes.Stloc, (object)requestedSlot), 
		});
	}

	private static CodeInstruction CreateOriginalAddressLoad(int requestedSlot)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		return new CodeInstruction(OpCodes.Ldloca, (object)requestedSlot);
	}

	public static CodeInstruction CreateLoadLocalInstruction(ILGenerator generator, MethodBase method, int requestedSlot, Type syntheticLocalType)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (requestedSlot < GetOriginalLocalCount(method))
		{
			return CreateOriginalLoad(requestedSlot);
		}
		return new CodeInstruction(OpCodes.Ldloc, (object)GetOrDeclareSyntheticLocal(generator, requestedSlot, syntheticLocalType));
	}

	public static CodeInstruction CreateStoreLocalInstruction(ILGenerator generator, MethodBase method, int requestedSlot, Type syntheticLocalType)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (requestedSlot < GetOriginalLocalCount(method))
		{
			return CreateOriginalStore(requestedSlot);
		}
		return new CodeInstruction(OpCodes.Stloc, (object)GetOrDeclareSyntheticLocal(generator, requestedSlot, syntheticLocalType));
	}

	public static CodeInstruction CreateLoadLocalAddressInstruction(ILGenerator generator, MethodBase method, int requestedSlot, Type syntheticLocalType)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		if (requestedSlot < GetOriginalLocalCount(method))
		{
			return CreateOriginalAddressLoad(requestedSlot);
		}
		return new CodeInstruction(OpCodes.Ldloca, (object)GetOrDeclareSyntheticLocal(generator, requestedSlot, syntheticLocalType));
	}
}
