using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using API.Assembly;
using API.Events;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using HarmonyLib;

namespace Carbon.Compat.Patches;

public static class CodeGenHelpers
{
	public static void GenerateEntrypoint(ModuleDefinition asm, ReferenceImporter importer, string name, Guid guid, out MethodDefinition load, out MethodDefinition unload, out TypeDefinition typeDef)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected O, but got Unknown
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		TypeDefinition val = new TypeDefinition(Utf8String.op_Implicit("Carbon.Compat.Generated." + name), Utf8String.op_Implicit("Entrypoint"), (TypeAttributes)1048832, asm.CorLibTypeFactory.Object.Type);
		val.Interfaces.Add(new InterfaceImplementation(importer.ImportType(typeof(ICarbonAddon))));
		val.Interfaces.Add(new InterfaceImplementation(importer.ImportType(typeof(ICarbonExtension))));
		val.AddDefaultCtor(asm, importer);
		TypeSignature val2 = importer.ImportTypeSignature(typeof(EventArgs));
		MethodDefinition val3 = new MethodDefinition(Utf8String.op_Implicit("Awake"), (MethodAttributes)480, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { val2 }));
		val.MethodImplementations.Add(new MethodImplementation((IMethodDefOrRef)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(ICarbonAddon), "Awake", (Type[])null, (Type[])null)), (IMethodDefOrRef)(object)val3));
		val.Methods.Add(val3);
		val3.CilMethodBody = new CilMethodBody(val3);
		val3.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		unload = new MethodDefinition(Utf8String.op_Implicit("OnUnloaded"), (MethodAttributes)480, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { val2 }));
		val.MethodImplementations.Add(new MethodImplementation((IMethodDefOrRef)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(ICarbonAddon), "OnUnloaded", (Type[])null, (Type[])null)), (IMethodDefOrRef)(object)unload));
		val.Methods.Add(unload);
		load = new MethodDefinition(Utf8String.op_Implicit("OnLoaded"), (MethodAttributes)480, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { val2 }));
		val.MethodImplementations.Add(new MethodImplementation((IMethodDefOrRef)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(ICarbonAddon), "OnLoaded", (Type[])null, (Type[])null)), (IMethodDefOrRef)(object)load));
		val.Methods.Add(load);
		asm.TopLevelTypes.Add(val);
		typeDef = val;
	}

	public static void GenerateCarbonEventCall(CilMethodBody body, ReferenceImporter importer, ref int index, CarbonEvent eventId, MethodDefinition method, CilInstruction self = null, string event_method = "Subscribe")
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		if (self == null)
		{
			self = new CilInstruction(CilOpCodes.Ldnull);
		}
		List<CilInstruction> list = new List<CilInstruction>();
		list.Add(new CilInstruction(CilOpCodes.Call, (object)importer.ImportMethod((MethodBase)AccessTools.PropertyGetter(typeof(Community), "Runtime"))));
		list.Add(new CilInstruction(CilOpCodes.Callvirt, (object)importer.ImportMethod((MethodBase)AccessTools.PropertyGetter(typeof(Community), "Events"))));
		list.Add(new CilInstruction(CilOpCodes.Ldc_I4, (object)(int)eventId));
		list.Add(self);
		list.Add(new CilInstruction(CilOpCodes.Ldftn, (object)method));
		list.Add(new CilInstruction(CilOpCodes.Newobj, (object)importer.ImportMethod((MethodBase)AccessTools.Constructor(typeof(Action<EventArgs>), new Type[2]
		{
			typeof(object),
			typeof(IntPtr)
		}, false))));
		list.Add(new CilInstruction(CilOpCodes.Callvirt, (object)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(IEventManager), event_method, (Type[])null, (Type[])null))));
		List<CilInstruction> list2 = list;
		body.Instructions.InsertRange(index, (IEnumerable<CilInstruction>)list2);
		index += list2.Count;
	}

	public static void DoMultiMethodCall(CilMethodBody body, ref int index, List<IMethodDescriptor> staticMethods, IEnumerable<TypeDefinition> instanceTypes, IMethodDescriptor instanceMethod)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		List<CilInstruction> list = new List<CilInstruction>();
		if (staticMethods != null)
		{
			foreach (MethodDefinition staticMethod in staticMethods)
			{
				MethodDefinition val = staticMethod;
				foreach (Parameter parameter in val.Parameters)
				{
					if (!parameter.ParameterType.IsValueType)
					{
						list.Add(new CilInstruction(CilOpCodes.Ldnull));
					}
				}
				list.Add(new CilInstruction(CilOpCodes.Call, (object)val));
				if (((MethodSignatureBase)val.Signature).ReturnsValue)
				{
					list.Add(new CilInstruction(CilOpCodes.Pop));
				}
			}
		}
		if (instanceTypes != null)
		{
			foreach (TypeDefinition instanceType in instanceTypes)
			{
				list.Add(new CilInstruction(CilOpCodes.Newobj, (object)instanceType.Methods.First((MethodDefinition x) => x.Parameters.Count == 0 && x.Name == ".ctor")));
				foreach (TypeSignature parameterType in ((MethodSignatureBase)instanceMethod.Signature).ParameterTypes)
				{
					if (!parameterType.IsValueType)
					{
						list.Add(new CilInstruction(CilOpCodes.Ldnull));
					}
				}
				list.Add(new CilInstruction(CilOpCodes.Callvirt, (object)instanceMethod));
				if (((MethodSignatureBase)instanceMethod.Signature).ReturnsValue)
				{
					list.Add(new CilInstruction(CilOpCodes.Pop));
				}
			}
		}
		body.Instructions.InsertRange(index, (IEnumerable<CilInstruction>)list);
		index += list.Count;
	}
}
