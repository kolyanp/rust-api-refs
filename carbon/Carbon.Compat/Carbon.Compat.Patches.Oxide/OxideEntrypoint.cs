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
		//IL_0112: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Expected O, but got Unknown
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Expected O, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Expected O, but got Unknown
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected O, but got Unknown
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Expected O, but got Unknown
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Expected O, but got Unknown
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Expected O, but got Unknown
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		if (context.NoEntrypoint)
		{
			return;
		}
		Guid guid = Guid.NewGuid();
		IEnumerable<TypeDefinition> enumerable = asm.GetAllTypes().Where(delegate(TypeDefinition x)
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
			PropertyDefinition? obj = enumerable.FirstOrDefault().Properties.FirstOrDefault(delegate(PropertyDefinition x)
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
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				MethodDefinition getMethod = obj.GetMethod;
				if (getMethod == null)
				{
					obj2 = null;
				}
				else
				{
					CilMethodBody cilMethodBody = getMethod.CilMethodBody;
					if (cilMethodBody == null)
					{
						obj2 = null;
					}
					else
					{
						CilInstruction? obj3 = ((IEnumerable<CilInstruction>)cilMethodBody.Instructions).FirstOrDefault(delegate(CilInstruction x)
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							return x.OpCode == CilOpCodes.Ldstr;
						});
						obj2 = ((obj3 != null) ? obj3.Operand : null);
					}
				}
			}
			author = obj2 as string;
		}
		CodeGenHelpers.GenerateEntrypoint(asm, importer, "Oxide", guid, out var load, out var unload, out var typeDef);
		typeDef.Interfaces.Add(new InterfaceImplementation(importer.ImportType(typeof(ICarbonExtension))));
		load.CilMethodBody = new CilMethodBody(load);
		unload.CilMethodBody = new CilMethodBody(unload);
		unload.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		MethodDefinition val = new MethodDefinition(Utf8String.op_Implicit("serverInit"), (MethodAttributes)0, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { importer.ImportTypeSignature(typeof(EventArgs)) }));
		val.CilMethodBody = new CilMethodBody(val);
		FieldDefinition val2 = new FieldDefinition(Utf8String.op_Implicit("loaded"), (FieldAttributes)0, new FieldSignature((TypeSignature)(object)asm.CorLibTypeFactory.Boolean));
		int index = 0;
		CodeGenHelpers.GenerateCarbonEventCall(load.CilMethodBody, importer, ref index, CarbonEvent.HookValidatorRefreshed, val, new CilInstruction(CilOpCodes.Ldarg_0));
		load.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
		CilInstruction val3 = new CilInstruction(CilOpCodes.Ret);
		val.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)(object)new CilInstruction[6]
		{
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldfld, (object)val2),
			new CilInstruction(CilOpCodes.Brtrue_S, (object)val3.CreateLabel()),
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Ldc_I4_1),
			new CilInstruction(CilOpCodes.Stfld, (object)val2)
		});
		foreach (TypeDefinition item2 in enumerable)
		{
			MethodDefinition val4 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == "Load" && x.IsVirtual);
			MethodDefinition val5 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == "OnModLoad" && x.IsVirtual && x.Parameters.Count == 0);
			MethodDefinition val6 = item2.Methods.FirstOrDefault((MethodDefinition x) => x.Name == ".ctor" && x.Parameters.Count == 1);
			if (val4 != null || val5 != null)
			{
				CilLocalVariable item = new CilLocalVariable(item2.ToTypeSignature());
				((Collection<CilLocalVariable>)(object)val.CilMethodBody.LocalVariables).Add(item);
				short num = (short)(((Collection<CilLocalVariable>)(object)val.CilMethodBody.LocalVariables).Count - 1);
				val.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)(object)new CilInstruction[3]
				{
					new CilInstruction(CilOpCodes.Ldnull),
					new CilInstruction(CilOpCodes.Newobj, (object)val6),
					new CilInstruction(CilOpCodes.Stloc, (object)num)
				});
				if (val4 != null)
				{
					val.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldloc, (object)num));
					val.CilMethodBody.Instructions.Add(CilOpCodes.Callvirt, (IMethodDescriptor)(object)val4);
				}
				if (val5 != null)
				{
					val.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ldloc, (object)num));
					val.CilMethodBody.Instructions.Add(CilOpCodes.Callvirt, (IMethodDescriptor)(object)val5);
				}
			}
		}
		val.CilMethodBody.Instructions.Add(val3);
		typeDef.Fields.Add(val2);
		typeDef.Methods.Add(val);
	}
}
