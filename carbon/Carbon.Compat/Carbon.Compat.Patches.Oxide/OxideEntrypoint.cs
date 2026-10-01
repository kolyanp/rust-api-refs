using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using API.Assembly;
using API.Events;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches.Oxide;

public class OxideEntrypoint : BaseOxidePatch
{
	public override void Apply(ModuleDefinition asm, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected Obj, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected Obj, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected Obj, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected Obj, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected Obj, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected Obj, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected Obj, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected Obj, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected Obj, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected Obj, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected Obj, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected Obj, but got Unknown
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected Obj, but got Unknown
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected Obj, but got Unknown
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected Obj, but got Unknown
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected Obj, but got Unknown
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Expected Obj, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Expected Obj, but got Unknown
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected Obj, but got Unknown
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Expected Obj, but got Unknown
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Expected Obj, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Expected Obj, but got Unknown
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		if (context.NoEntrypoint)
		{
			return;
		}
		Guid guid = Guid.NewGuid();
		IEnumerable<TypeDefinition> enumerable = asm.GetAllTypes().Where((TypeDefinition x) =>
		{
			ITypeDefOrRef baseType = x.BaseType;
			return ((baseType != null) ? ((IFullNameProvider)baseType).FullName : null) == "Oxide.Core.Extensions.Extension" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x.BaseType).DefinitionAssembly()).Name == "Carbon.Common";
		});
		if (!enumerable.Any())
		{
			return;
		}
		ref string author = ref context.Author;
		if (author == null)
		{
			PropertyDefinition? val = enumerable.FirstOrDefault().Properties.FirstOrDefault((PropertyDefinition x) =>
			{
				if (x.Name == "Author")
				{
					MethodDefinition getMethod2 = x.GetMethod;
					if (getMethod2 != null)
					{
						return getMethod2.IsVirtual;
					}
					return false;
				}
				return false;
			});
			object obj;
			if (val == null)
			{
				obj = null;
			}
			else
			{
				MethodDefinition getMethod = val.GetMethod;
				if (getMethod == null)
				{
					obj = null;
				}
				else
				{
					CilMethodBody cilMethodBody = getMethod.CilMethodBody;
					if (cilMethodBody == null)
					{
						obj = null;
					}
					else
					{
						CilInstruction? val2 = ((IEnumerable<CilInstruction>)cilMethodBody.Instructions).FirstOrDefault((CilInstruction x) =>
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							return x.OpCode == CilOpCodes.Ldstr;
						});
						obj = ((val2 != null) ? val2.Operand : null);
					}
				}
			}
			author = obj as string;
		}
		CodeGenHelpers.GenerateEntrypoint(asm, importer, "Oxide", guid, out var load, out var unload, out var typeDef);
		typeDef.Interfaces.Add(new InterfaceImplementation(importer.ImportType(typeof(ICarbonExtension))));
		load.CilMethodBody = new CilMethodBody(load);
		unload.CilMethodBody = new CilMethodBody(unload);
		unload.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		MethodDefinition val3 = new MethodDefinition(Utf8String.op_Implicit("serverInit"), (MethodAttributes)0, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, new TypeSignature[1] { importer.ImportTypeSignature(typeof(EventArgs)) }));
		val3.CilMethodBody = new CilMethodBody(val3);
		FieldDefinition val4 = new FieldDefinition(Utf8String.op_Implicit("loaded"), (FieldAttributes)0, new FieldSignature((TypeSignature)(object)asm.CorLibTypeFactory.Boolean));
		int index = 0;
		CodeGenHelpers.GenerateCarbonEventCall(load.CilMethodBody, importer, ref index, CarbonEvent.HookValidatorRefreshed, val3, new CilInstruction(CilOpCodes.Ldarg_0));
		load.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
		CilInstruction val5 = new CilInstruction(CilOpCodes.Ret);
		val3.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)new CilInstruction[6]
		{
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldfld, (object)val4),
			new CilInstruction(CilOpCodes.Brtrue_S, (object)val5.CreateLabel()),
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldc_I4_1),
			new CilInstruction(CilOpCodes.Stfld, (object)val4)
		});
		foreach (TypeDefinition item2 in enumerable)
		{
			MethodDefinition val6 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == "Load" && x.IsVirtual);
			MethodDefinition val7 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == "OnModLoad" && x.IsVirtual && x.Parameters.Count == 0);
			MethodDefinition val8 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == ".ctor" && x.Parameters.Count == 1);
			if (val6 != null || val7 != null)
			{
				CilLocalVariable item = new CilLocalVariable(item2.ToTypeSignature());
				((Collection<CilLocalVariable>)(object)val3.CilMethodBody.LocalVariables).Add(item);
				short num = (short)(((Collection<CilLocalVariable>)(object)val3.CilMethodBody.LocalVariables).Count - 1);
				val3.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)new CilInstruction[3]
				{
					new CilInstruction(CilOpCodes.Ldnull),
					new CilInstruction(CilOpCodes.Newobj, (object)val8),
					new CilInstruction(CilOpCodes.Stloc, (object)num)
				});
				if (val6 != null)
				{
					val3.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldloc, (object)num));
					val3.CilMethodBody.Instructions.Add(CilOpCodes.Callvirt, (IMethodDescriptor)(object)val6);
				}
				if (val7 != null)
				{
					val3.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldloc, (object)num));
					val3.CilMethodBody.Instructions.Add(CilOpCodes.Callvirt, (IMethodDescriptor)(object)val7);
				}
			}
		}
		val3.CilMethodBody.Instructions.Add(val5);
		typeDef.Fields.Add(val4);
		typeDef.Methods.Add(val3);
	}
}
