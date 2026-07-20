using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using API.Events;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Carbon.Compat.Converters;
using HarmonyLib;

namespace Carbon.Compat.Patches.Harmony;

public class HarmonyEntrypoint : BaseHarmonyPatch
{
	public override void Apply(ModuleDefinition asm, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		if (context.NoEntrypoint)
		{
			return;
		}
		Guid guid = Guid.NewGuid();
		IEnumerable<TypeDefinition> enumerable = from x in asm.GetAllTypes()
			where x.Interfaces.Any(delegate(InterfaceImplementation y)
			{
				ITypeDefOrRef obj = y.Interface;
				return ((obj != null) ? ((IFullNameProvider)obj).FullName : null) == "IHarmonyModHooks";
			})
			select x;
		CodeGenHelpers.GenerateEntrypoint(asm, importer, "Harmony", guid, out var load, out var unload, out var typeDef);
		load.CilMethodBody = new CilMethodBody(load);
		unload.CilMethodBody = new CilMethodBody(unload);
		unload.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		MethodDefinition val = new MethodDefinition(Utf8String.op_Implicit("postHookLoad"), (MethodAttributes)0, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { importer.ImportTypeSignature(typeof(EventArgs)) }));
		val.CilMethodBody = new CilMethodBody(val);
		FieldDefinition val2 = new FieldDefinition(Utf8String.op_Implicit("loaded"), (FieldAttributes)0, new FieldSignature((TypeSignature)(object)asm.CorLibTypeFactory.Boolean));
		int index = 0;
		CodeGenHelpers.GenerateCarbonEventCall(load.CilMethodBody, importer, ref index, CarbonEvent.HookValidatorRefreshed, val, new CilInstruction(CilOpCodes.Ldarg_0));
		load.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
		CilInstruction val3 = new CilInstruction(CilOpCodes.Ret);
		val.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)(object)new CilInstruction[9]
		{
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldfld, (object)val2),
			new CilInstruction(CilOpCodes.Brtrue_S, (object)val3.CreateLabel()),
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldc_I4_1),
			new CilInstruction(CilOpCodes.Stfld, (object)val2),
			new CilInstruction(CilOpCodes.Ldstr, (object)$"__CCL:{((AssemblyDescriptor)asm.Assembly).Name}:{guid:N}"),
			new CilInstruction(CilOpCodes.Newobj, (object)importer.ImportMethod((MethodBase)AccessTools.Constructor(typeof(Harmony), new Type[1] { typeof(string) }, false))),
			new CilInstruction(CilOpCodes.Callvirt, (object)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(Harmony), "PatchAll", (Type[])null, (Type[])null)))
		});
		val.CilMethodBody.Instructions.Add(val3);
		typeDef.Methods.Add(val);
		typeDef.Fields.Add(val2);
	}
}
