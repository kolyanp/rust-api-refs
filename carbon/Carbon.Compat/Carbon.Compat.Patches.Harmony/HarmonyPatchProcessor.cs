using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;
using Carbon.Components;
using Facepunch;
using HarmonyLib;

namespace Carbon.Compat.Patches.Harmony;

public class HarmonyPatchProcessor : BaseHarmonyPatch
{
	public static class PatchWhitelist
	{
		public static List<string> string_blacklist = new List<string> { "Oxide.Core.OxideMod", "Oxide.Core" };

		public static bool IsPatchAllowed(TypeDefOrRefSignature type, Utf8String method)
		{
			if (((TypeSignature)type).FullName == "ServerMgr")
			{
				IResolutionScope scope = ((TypeSignature)type).Scope;
				SerializedAssemblyReference val = (SerializedAssemblyReference)(object)((scope is SerializedAssemblyReference) ? scope : null);
				if (val != null && ((AssemblyDescriptor)val).Name == "Assembly-CSharp" && method == "UpdateServerInformation")
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsPatchAllowed(TypeDefinition type)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			MethodDefinition val = type.Methods.FirstOrDefault((MethodDefinition x) => x.CustomAttributes.Any(delegate(CustomAttribute y)
			{
				if (((IMethodDefOrRef)y.Constructor).DeclaringType.Name == "HarmonyTargetMethods")
				{
					IResolutionScope scope = ((ITypeDescriptor)((IMethodDefOrRef)y.Constructor).DeclaringType).Scope;
					SerializedAssemblyReference val4 = (SerializedAssemblyReference)(object)((scope is SerializedAssemblyReference) ? scope : null);
					if (val4 != null)
					{
						return ((AssemblyDescriptor)val4).Name == "0Harmony";
					}
				}
				return false;
			}));
			MethodBody obj = ((val != null) ? val.MethodBody : null);
			CilMethodBody val2 = (CilMethodBody)(object)((obj is CilMethodBody) ? obj : null);
			if (val2 != null)
			{
				for (int num = 0; num < val2.Instructions.Count; num++)
				{
					CilInstruction val3 = val2.Instructions[num];
					if (val3.OpCode == CilOpCodes.Ldstr && val3.Operand is string item && string_blacklist.Contains(item))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public override void Apply(ModuleDefinition asm, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		foreach (TypeDefinition allType in asm.GetAllTypes())
		{
			bool flag = false;
			List<CustomAttribute> list = Pool.Get<List<CustomAttribute>>();
			foreach (CustomAttribute customAttribute in allType.CustomAttributes)
			{
				ICustomAttributeType constructor = customAttribute.Constructor;
				ITypeDefOrRef val = ((constructor != null) ? ((IMethodDefOrRef)constructor).DeclaringType : null);
				if (val == null)
				{
					continue;
				}
				CustomAttributeSignature signature = customAttribute.Signature;
				if (signature == null || !(val.Name == "HarmonyPatch"))
				{
					continue;
				}
				IResolutionScope scope = ((ITypeDescriptor)val).Scope;
				SerializedAssemblyReference val2 = (SerializedAssemblyReference)(object)((scope is SerializedAssemblyReference) ? scope : null);
				if (val2 == null || !(((AssemblyDescriptor)val2).Name == "0Harmony"))
				{
					continue;
				}
				if (signature.FixedArguments.Count > 1)
				{
					object element = signature.FixedArguments[0].Element;
					TypeDefOrRefSignature val3 = (TypeDefOrRefSignature)((element is TypeDefOrRefSignature) ? element : null);
					if (val3 != null)
					{
						object element2 = signature.FixedArguments[1].Element;
						Utf8String val4 = (Utf8String)((element2 is Utf8String) ? element2 : null);
						if (val4 != null)
						{
							RegisterPatch(((object)asm.Name).ToString(), Utf8String.op_Implicit(((AssemblyDescriptor)((ITypeDescriptor)(object)val3).DefinitionAssembly()).Name), Utf8String.op_Implicit(val4), ((TypeSignature)val3).FullName, $"{((AssemblyDescriptor)asm.Assembly).Name} - {allType.FullName}", null);
							if (!PatchWhitelist.IsPatchAllowed(val3, val4))
							{
								flag = true;
								list.Add(customAttribute);
								break;
							}
							continue;
						}
					}
				}
				if (!PatchWhitelist.IsPatchAllowed(allType))
				{
					flag = true;
					list.Add(customAttribute);
					break;
				}
			}
			if (flag)
			{
				allType.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)(object)TypeDescriptorExtensions.CreateMemberReference((IMemberRefParent)(object)TypeDescriptorExtensions.CreateTypeReference(asm.CorLibTypeFactory.CorLibScope, "System", "ObsoleteAttribute"), ".ctor", (MemberSignature)(object)MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void)).ImportWith(importer)));
				foreach (CustomAttribute item in list)
				{
					allType.CustomAttributes.Remove(item);
				}
			}
			Pool.FreeUnmanaged<CustomAttribute>(ref list);
		}
	}

	public static void RegisterPatch(string parentAssemblyName, string assemblyName, string methodName, string typeName, string reason, Harmony harmony)
	{
		Carbon.Components.Harmony.CurrentPatches.Add(new Carbon.Components.Harmony.PatchInfoEntry(parentAssemblyName, assemblyName, methodName, typeName, reason, harmony));
	}

	public static void RegisterPatch(MethodBase method, string reason, Harmony harmony)
	{
		if (!(method == null))
		{
			Carbon.Components.Harmony.CurrentPatches.Add(new Carbon.Components.Harmony.PatchInfoEntry(method.DeclaringType.Assembly.GetName().Name + ".dll", method, harmony));
		}
	}
}
