using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;
using Carbon.Compat.Lib;
using HarmonyLib;

namespace Carbon.Compat.Patches.Harmony;

public class HarmonyILSwitch : BaseHarmonyPatch
{
	public override void Apply(ModuleDefinition asm, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		if (!ReferencesHarmony1(asm))
		{
			return;
		}
		IMethodDescriptor val = importer.ImportMethod((MethodBase)AccessTools.Method(typeof(HarmonyCompat), "PatchProcessorCompat", (Type[])null, (Type[])null));
		foreach (TypeDefinition allType in asm.GetAllTypes())
		{
			foreach (MethodDefinition method in allType.Methods)
			{
				MethodBody methodBody = method.MethodBody;
				CilMethodBody val2 = (CilMethodBody)(object)((methodBody is CilMethodBody) ? methodBody : null);
				if (val2 == null)
				{
					continue;
				}
				for (int i = 0; i < val2.Instructions.Count; i++)
				{
					CilInstruction val3 = val2.Instructions[i];
					if (val3.OpCode == CilOpCodes.Call)
					{
						object operand = val3.Operand;
						MemberReference val4 = (MemberReference)((operand is MemberReference) ? operand : null);
						if (val4 != null && val4.FullName == "HarmonyLib.Harmony HarmonyLib.Harmony::Create(System.String)")
						{
							val3.OpCode = CilOpCodes.Newobj;
							val3.Operand = importer.ImportMethod((MethodBase)AccessTools.Constructor(typeof(Harmony), new Type[1] { typeof(string) }, false));
						}
					}
					if (val3.OpCode == CilOpCodes.Newobj)
					{
						object operand2 = val3.Operand;
						MemberReference val5 = (MemberReference)((operand2 is MemberReference) ? operand2 : null);
						if (val5 != null && ((AssemblyDescriptor)((ITypeDescriptor)(object)val5.DeclaringType).DefinitionAssembly()).Name == "0Harmony" && val5.DeclaringType.Name == "PatchProcessor" && val5.Name == ".ctor")
						{
							val3.OpCode = CilOpCodes.Call;
							val3.Operand = val;
							continue;
						}
					}
					if (val3.OpCode == CilOpCodes.Callvirt)
					{
						object operand3 = val3.Operand;
						MemberReference val6 = (MemberReference)((operand3 is MemberReference) ? operand3 : null);
						if (val6 != null && ((AssemblyDescriptor)((ITypeDescriptor)(object)val6.DeclaringType).DefinitionAssembly()).Name == "0Harmony" && val6.DeclaringType.Name == "PatchProcessor" && val6.Name == "Patch" && i != 0)
						{
							CilInstruction val7 = val2.Instructions[i - 1];
							if (val7.OpCode == CilOpCodes.Call && val7.Operand == val)
							{
								val2.Instructions.RemoveAt(i);
								CilInstruction val8 = val2.Instructions[i];
								if (val8.OpCode == CilOpCodes.Pop)
								{
									val2.Instructions.RemoveAt(i);
								}
							}
						}
					}
					if (val3.OpCode == CilOpCodes.Callvirt || val3.OpCode == CilOpCodes.Call)
					{
						object operand4 = val3.Operand;
						MemberReference val9 = (MemberReference)((operand4 is MemberReference) ? operand4 : null);
						if (val9 != null && ((AssemblyDescriptor)((ITypeDescriptor)(object)val9.DeclaringType).DefinitionAssembly()).Name == "0Harmony" && val9.Name == "Patch")
						{
							val3.Operand = importer.ImportMethod((MethodBase)AccessTools.Method(typeof(HarmonyCompat), "InstancePatchCompat", (Type[])null, (Type[])null));
							val3.OpCode = CilOpCodes.Call;
						}
					}
				}
			}
		}
	}

	private static bool ReferencesHarmony1(ModuleDefinition asm)
	{
		IList<AssemblyReference> assemblyReferences = asm.AssemblyReferences;
		for (int i = 0; i < assemblyReferences.Count; i++)
		{
			AssemblyReference val = assemblyReferences[i];
			if (((AssemblyDescriptor)val).Name == "0Harmony" && ((AssemblyDescriptor)val).Version.Major < 2)
			{
				return true;
			}
		}
		return false;
	}
}
