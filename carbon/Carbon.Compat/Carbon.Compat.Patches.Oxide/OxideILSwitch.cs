using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;
using Carbon.Compat.Lib;
using HarmonyLib;
using Oxide.Core.Libraries;
using Oxide.Plugins;

namespace Carbon.Compat.Patches.Oxide;

public class OxideILSwitch : BaseOxidePatch
{
	private static MethodInfo pluginLoaderMethod;

	private static MethodInfo consoleCommand1;

	private static MethodInfo chatCommand1;

	private static MethodInfo getExtensionDirectory;

	private static MethodInfo timerOnce;

	private static MethodInfo timerRepeat;

	private static MethodInfo onAddedToManagerCompat;

	private static MethodInfo onRemovedFromManagerCompat;

	private static FieldInfo rustPluginTimer;

	private static readonly MethodInfo carbonLangGetMessage;

	private static int carbonLangGetMessageArgLength;

	public static CompatManager Singleton => Community.Runtime.Compat as CompatManager;

	static OxideILSwitch()
	{
		pluginLoaderMethod = AccessTools.Method(typeof(OxideCompat), "RegisterPluginLoader", (Type[])null, (Type[])null);
		consoleCommand1 = AccessTools.Method(typeof(OxideCompat), "AddConsoleCommand1", (Type[])null, (Type[])null);
		chatCommand1 = AccessTools.Method(typeof(OxideCompat), "AddChatCommand1", (Type[])null, (Type[])null);
		getExtensionDirectory = AccessTools.Method(typeof(OxideCompat), "GetExtensionDirectory", (Type[])null, (Type[])null);
		timerOnce = AccessTools.Method(typeof(OxideCompat), "TimerOnce", (Type[])null, (Type[])null);
		timerRepeat = AccessTools.Method(typeof(OxideCompat), "TimerRepeat", (Type[])null, (Type[])null);
		onAddedToManagerCompat = AccessTools.Method(typeof(OxideCompat), "OnAddedToManagerCompat", (Type[])null, (Type[])null);
		onRemovedFromManagerCompat = AccessTools.Method(typeof(OxideCompat), "OnRemovedFromManagerCompat", (Type[])null, (Type[])null);
		rustPluginTimer = AccessTools.Field(typeof(RustPlugin), "timer");
		carbonLangGetMessage = typeof(Lang).GetMethods().First((MethodInfo x) => x.Name == "GetMessage" && x.ReturnType == typeof(string));
		carbonLangGetMessageArgLength = carbonLangGetMessage.GetParameters().Length;
	}

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
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0662: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_073d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0765: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0770: Expected O, but got Unknown
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_0777: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Expected O, but got Unknown
		//IL_077f: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0795: Expected O, but got Unknown
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			bool flag = allType.IsBaseType((ITypeDefOrRef x) => x.Name == "RustPlugin" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x).DefinitionAssembly()).Name == "Carbon.Common");
			bool flag2 = allType.IsBaseType((ITypeDefOrRef x) => ((IFullNameProvider)x).FullName == "Oxide.Core.Extensions.Extension" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x).DefinitionAssembly()).Name == "Carbon.Common");
			foreach (MethodDefinition method2 in allType.Methods)
			{
				bool flag3 = !method2.IsStatic && flag;
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
								val.Instructions.Insert(num++, new CilInstruction((!method2.IsStatic && flag2) ? CilOpCodes.Ldarg_0 : CilOpCodes.Ldnull));
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
					if (val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand6 = val2.Operand;
						MemberReference val15 = (MemberReference)((operand6 is MemberReference) ? operand6 : null);
						if (val15 != null)
						{
							CallingConventionSignature signature3 = val15.Signature;
							MethodSignature val16 = (MethodSignature)(object)((signature3 is MethodSignature) ? signature3 : null);
							if (val16 != null)
							{
								IMemberRefParent parent6 = val15.Parent;
								TypeReference val17 = (TypeReference)(object)((parent6 is TypeReference) ? parent6 : null);
								if (val17 != null && val17.FullName == "Oxide.Core.Libraries.Timer")
								{
									IList<TypeSignature> parameterTypes = ((MethodSignatureBase)val16).ParameterTypes;
									if (parameterTypes[parameterTypes.Count - 1].FullName == "Oxide.Core.Plugins.Plugin" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val17).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
									{
										string text = ((object)val15.Name).ToString();
										if (!(text == "Once"))
										{
											if (!(text == "Repeat"))
											{
												continue;
											}
											val2.Operand = importer.ImportMethod((MethodBase)timerRepeat);
										}
										else
										{
											val2.Operand = importer.ImportMethod((MethodBase)timerOnce);
										}
										val2.OpCode = CilOpCodes.Call;
										continue;
									}
								}
							}
						}
					}
					if (flag3 && val2.OpCode == CilOpCodes.Callvirt)
					{
						object operand7 = val2.Operand;
						MethodSpecification val18 = (MethodSpecification)((operand7 is MethodSpecification) ? operand7 : null);
						if (val18 != null)
						{
							IMethodDefOrRef method = val18.Method;
							MemberReference val19 = (MemberReference)(object)((method is MemberReference) ? method : null);
							if (val19 != null)
							{
								IMemberRefParent parent7 = val19.Parent;
								TypeReference val20 = (TypeReference)(object)((parent7 is TypeReference) ? parent7 : null);
								if (val20 != null && val19.Name == "GetLibrary" && val20.FullName == "Oxide.Core.OxideMod" && val18.Signature.TypeArguments.Count == 1 && val18.Signature.TypeArguments[0].FullName == "Oxide.Core.Libraries.Timer" && ((AssemblyDescriptor)((ITypeDescriptor)(object)val20).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name)
								{
									val2.OpCode = CilOpCodes.Pop;
									val2.Operand = null;
									val.Instructions.InsertRange(++num, (IEnumerable<CilInstruction>)(object)new CilInstruction[3]
									{
										new CilInstruction(CilOpCodes.Pop),
										new CilInstruction(CilOpCodes.Ldarg_0),
										new CilInstruction(CilOpCodes.Ldfld, (object)importer.ImportField(rustPluginTimer))
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
					object operand8 = val2.Operand;
					MemberReference val21 = (MemberReference)((operand8 is MemberReference) ? operand8 : null);
					if (val21 == null)
					{
						continue;
					}
					CallingConventionSignature signature4 = val21.Signature;
					FieldSignature val22 = (FieldSignature)(object)((signature4 is FieldSignature) ? signature4 : null);
					if (val22 == null)
					{
						continue;
					}
					IMemberRefParent parent8 = val21.Parent;
					TypeReference val23 = (TypeReference)(object)((parent8 is TypeReference) ? parent8 : null);
					if (val23 == null || !(val23.FullName == "Oxide.Core.Plugins.Plugin") || !(val22.FieldType.FullName == "Oxide.Core.Plugins.PluginManagerEvent") || !(((AssemblyDescriptor)((ITypeDescriptor)(object)val23).DefinitionAssembly()).Name == ((AssemblyDescriptor)CompatManager.Common).Name))
					{
						continue;
					}
					string text2 = ((object)val21.Name).ToString();
					if (!(text2 == "OnAddedToManager"))
					{
						if (!(text2 == "OnRemovedFromManager"))
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
