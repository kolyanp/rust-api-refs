using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;
using Carbon.Compat.Lib;
using HarmonyLib;
using Oxide.Plugins;

namespace Carbon.Compat.Patches.Oxide;

public class OxideILSwitch : BaseOxidePatch
{
	private static MethodInfo pluginLoaderMethod = AccessTools.Method(typeof(OxideCompat), "RegisterPluginLoader", (Type[])null, (Type[])null);

	private static MethodInfo consoleCommand1 = AccessTools.Method(typeof(OxideCompat), "AddConsoleCommand1", (Type[])null, (Type[])null);

	private static MethodInfo chatCommand1 = AccessTools.Method(typeof(OxideCompat), "AddChatCommand1", (Type[])null, (Type[])null);

	private static MethodInfo getExtensionDirectory = AccessTools.Method(typeof(OxideCompat), "GetExtensionDirectory", (Type[])null, (Type[])null);

	private static MethodInfo onAddedToManagerCompat = AccessTools.Method(typeof(OxideCompat), "OnAddedToManagerCompat", (Type[])null, (Type[])null);

	private static MethodInfo onRemovedFromManagerCompat = AccessTools.Method(typeof(OxideCompat), "OnRemovedFromManagerCompat", (Type[])null, (Type[])null);

	private static FieldInfo rustPluginTimer = AccessTools.Field(typeof(RustPlugin), "timer");

	private static MethodInfo pluginTimersLibrary = AccessTools.PropertyGetter(typeof(PluginTimers), "Library");

	public override void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Invalid comparison between Unknown and I4
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Expected O, but got Unknown
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected O, but got Unknown
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Invalid comparison between Unknown and I4
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0795: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0654: Unknown result type (might be due to invalid IL or missing references)
		//IL_065a: Expected O, but got Unknown
		//IL_065c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Expected O, but got Unknown
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_067f: Expected O, but got Unknown
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_0697: Expected O, but got Unknown
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			bool flag = allType.IsBaseType((ITypeDefOrRef x) => x.Name == "RustPlugin" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x).DefinitionAssembly()).Name == "Carbon.Common");
			bool flag2 = allType.IsBaseType((ITypeDefOrRef x) => ((IFullNameProvider)x).FullName == "Oxide.Core.Extensions.Extension" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x).DefinitionAssembly()).Name == "Carbon.Common");
			foreach (MethodDefinition method2 in allType.Methods)
			{
				bool flag3 = !method2.IsStatic & flag;
				MethodBody methodBody = method2.MethodBody;
				CilMethodBody val = (CilMethodBody)(object)((methodBody is CilMethodBody) ? methodBody : null);
				if (val == null)
				{
					continue;
				}
				for (int num = 0; num < val.Instructions.Count; num++)
				{
					CilInstruction val2 = val.Instructions[num];
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand = val2.Operand;
						MemberReference val3 = (MemberReference)((operand is MemberReference) ? operand : null);
						if (val3 != null && val3.Name == "RegisterPluginLoader")
						{
							IMemberRefParent parent = val3.Parent;
							TypeReference val4 = (TypeReference)(object)((parent is TypeReference) ? parent : null);
							if (val4 != null && ((AssemblyDescriptor)((ITypeDescriptor)(object)val4).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
							{
								val2.OpCode = CilOpCodes.Call;
								val2.Operand = importer.ImportMethod((MethodBase)pluginLoaderMethod);
								val.Instructions.Insert(num++, new CilInstruction((!method2.IsStatic & flag2) ? CilOpCodes.Ldarg_0 : CilOpCodes.Ldnull));
								continue;
							}
						}
					}
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand2 = val2.Operand;
						MemberReference val5 = (MemberReference)((operand2 is MemberReference) ? operand2 : null);
						if (val5 != null && val5.Name == "AddConsoleCommand")
						{
							IMemberRefParent parent2 = val5.Parent;
							TypeReference val6 = (TypeReference)(object)((parent2 is TypeReference) ? parent2 : null);
							if (val6 != null && val6.FullName == "Oxide.Game.Rust.Libraries.Command")
							{
								CallingConventionSignature signature = val5.Signature;
								MethodSignature val7 = (MethodSignature)(object)((signature is MethodSignature) ? signature : null);
								if (val7 != null && ((MethodSignatureBase)val7).ParameterTypes.Count == 3 && (int)((MethodSignatureBase)val7).ParameterTypes[0].ElementType == 14 && ((MethodSignatureBase)val7).ParameterTypes[1].FullName == "Oxide.Core.Plugins.Plugin" && ((MethodSignatureBase)val7).ParameterTypes[2].FullName == "System.Func`2<ConsoleSystem+Arg, System.Boolean>" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val6).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
								{
									val2.OpCode = CilOpCodes.Call;
									val2.Operand = importer.ImportMethod((MethodBase)consoleCommand1);
									continue;
								}
							}
						}
					}
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand3 = val2.Operand;
						MemberReference val8 = (MemberReference)((operand3 is MemberReference) ? operand3 : null);
						if (val8 != null && val8.Name == "AddChatCommand")
						{
							IMemberRefParent parent3 = val8.Parent;
							TypeReference val9 = (TypeReference)(object)((parent3 is TypeReference) ? parent3 : null);
							if (val9 != null && val9.FullName == "Oxide.Game.Rust.Libraries.Command")
							{
								CallingConventionSignature signature2 = val8.Signature;
								MethodSignature val10 = (MethodSignature)(object)((signature2 is MethodSignature) ? signature2 : null);
								if (val10 != null && ((MethodSignatureBase)val10).ParameterTypes.Count == 3 && (int)((MethodSignatureBase)val10).ParameterTypes[0].ElementType == 14 && ((MethodSignatureBase)val10).ParameterTypes[1].FullName == "Oxide.Core.Plugins.Plugin" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val9).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
								{
									string fullName = ((MethodSignatureBase)val10).ParameterTypes[2].FullName;
									if (fullName == "System.Action`3<BasePlayer, System.String, System.String[]>")
									{
										val2.Operand = importer.ImportMethod((MethodBase)chatCommand1);
										val2.OpCode = CilOpCodes.Call;
									}
									continue;
								}
							}
						}
					}
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand4 = val2.Operand;
						MemberReference val11 = (MemberReference)((operand4 is MemberReference) ? operand4 : null);
						if (val11 != null && val11.Name == "RegisterLibrary")
						{
							IMemberRefParent parent4 = val11.Parent;
							TypeReference val12 = (TypeReference)(object)((parent4 is TypeReference) ? parent4 : null);
							if (val12 != null && val12.FullName == "Oxide.Core.Extensions.ExtensionManager" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val12).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
							{
								val2.OpCode = CilOpCodes.Pop;
								val2.Operand = null;
								val.Instructions.InsertRange(num, (IEnumerable<CilInstruction>)(object)new CilInstruction[2]
								{
									new CilInstruction(CilOpCodes.Pop),
									new CilInstruction(CilOpCodes.Pop)
								});
								num += 2;
								continue;
							}
						}
					}
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand5 = val2.Operand;
						MemberReference val13 = (MemberReference)((operand5 is MemberReference) ? operand5 : null);
						if (val13 != null && val13.Name == "get_ExtensionDirectory")
						{
							IMemberRefParent parent5 = val13.Parent;
							TypeReference val14 = (TypeReference)(object)((parent5 is TypeReference) ? parent5 : null);
							if (val14 != null && val14.FullName == "Oxide.Core.OxideMod" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val14).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
							{
								val2.OpCode = CilOpCodes.Call;
								val2.Operand = importer.ImportMethod((MethodBase)getExtensionDirectory);
								continue;
							}
						}
					}
					if (flag3 && val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand6 = val2.Operand;
						MethodSpecification val15 = (MethodSpecification)((operand6 is MethodSpecification) ? operand6 : null);
						if (val15 != null)
						{
							IMethodDefOrRef method = val15.Method;
							MemberReference val16 = (MemberReference)(object)((method is MemberReference) ? method : null);
							if (val16 != null)
							{
								IMemberRefParent parent6 = val16.Parent;
								TypeReference val17 = (TypeReference)(object)((parent6 is TypeReference) ? parent6 : null);
								if (val17 != null && val16.Name == "GetLibrary" && val17.FullName == "Oxide.Core.OxideMod" && val15.Signature.TypeArguments.Count == 1 && val15.Signature.TypeArguments[0].FullName == "Oxide.Core.Libraries.Timer" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val17).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
								{
									val2.OpCode = CilOpCodes.Pop;
									val2.Operand = null;
									val.Instructions.InsertRange(++num, (IEnumerable<CilInstruction>)(object)new CilInstruction[4]
									{
										new CilInstruction(CilOpCodes.Pop),
										new CilInstruction(CilOpCodes.Ldarg_0),
										new CilInstruction(CilOpCodes.Ldfld, (object)importer.ImportField(rustPluginTimer)),
										new CilInstruction(CilOpCodes.Callvirt, (object)importer.ImportMethod((MethodBase)pluginTimersLibrary))
									});
									continue;
								}
							}
						}
					}
					if (!(val2.OpCode == CilOpCodes.Ldfld))
					{
						continue;
					}
					object operand7 = val2.Operand;
					MemberReference val18 = (MemberReference)((operand7 is MemberReference) ? operand7 : null);
					if (val18 == null)
					{
						continue;
					}
					CallingConventionSignature signature3 = val18.Signature;
					FieldSignature val19 = (FieldSignature)(object)((signature3 is FieldSignature) ? signature3 : null);
					if (val19 == null)
					{
						continue;
					}
					IMemberRefParent parent7 = val18.Parent;
					TypeReference val20 = (TypeReference)(object)((parent7 is TypeReference) ? parent7 : null);
					if (val20 == null || !(val20.FullName == "Oxide.Core.Plugins.Plugin") || !(val19.FieldType.FullName == "Oxide.Core.Plugins.PluginManagerEvent") || !(((AssemblyDescriptor)((ITypeDescriptor)(object)val20).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name))
					{
						continue;
					}
					string text = ((object)val18.Name).ToString();
					if (!(text == "OnAddedToManager"))
					{
						if (!(text == "OnRemovedFromManager"))
						{
							continue;
						}
						val2.Operand = importer.ImportMethod((MethodBase)onRemovedFromManagerCompat);
					}
					else
					{
						val2.Operand = importer.ImportMethod((MethodBase)onAddedToManagerCompat);
					}
					val2.OpCode = CilOpCodes.Call;
				}
			}
		}
	}
}
