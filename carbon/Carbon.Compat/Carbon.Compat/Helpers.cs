using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using HarmonyLib;

namespace Carbon.Compat;

public static class Helpers
{
	public static bool IsOxideASM(AssemblyReference aref)
	{
		if (((AssemblyDescriptor)aref).Name.StartsWith("Oxide."))
		{
			return !((AssemblyDescriptor)aref).Name.ToLower().StartsWith("oxide.ext.");
		}
		return false;
	}

	public static bool StartsWith(this Utf8String str, string value)
	{
		return str.Value.StartsWith(value);
	}

	public static bool EndsWith(this Utf8String str, string value)
	{
		return str.Value.EndsWith(value);
	}

	public static Utf8String ToLower(this Utf8String str)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		return new Utf8String(str.Value.ToLower());
	}

	public static void AddDefaultCtor(this TypeDefinition type, ModuleDefinition asm, ReferenceImporter importer)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		MethodDefinition val = new MethodDefinition(Utf8String.op_Implicit(".ctor"), (MethodAttributes)6278, MethodSignature.CreateInstance((TypeSignature)(object)asm.CorLibTypeFactory.Void));
		val.MethodBody = (MethodBody)new CilMethodBody(val);
		val.CilMethodBody.Instructions.AddRange((IEnumerable<CilInstruction>)(object)new CilInstruction[4]
		{
			new CilInstruction(CilOpCodes.Ldarg_0),
			new CilInstruction(CilOpCodes.Call, (object)importer.ImportMethod((MethodBase)AccessTools.Constructor(typeof(object), (Type[])null, false))),
			new CilInstruction(CilOpCodes.Nop),
			new CilInstruction(CilOpCodes.Ret)
		});
		type.Methods.Add(val);
	}

	public static bool IsBaseType(this TypeDefinition type, Func<ITypeDefOrRef, bool> call)
	{
		if (type.BaseType == null)
		{
			return false;
		}
		while (type != null && type.BaseType != null)
		{
			if (call(type.BaseType))
			{
				return true;
			}
			ITypeDefOrRef baseType = type.BaseType;
			type = (TypeDefinition)(object)((baseType is TypeDefinition) ? baseType : null);
		}
		return false;
	}

	public static AssemblyReference DefinitionAssembly(this ITypeDescriptor type)
	{
		AssemblyReference output = null;
		while (type != null)
		{
			type = rec(type, out output);
		}
		return output;
		static ITypeDescriptor rec(ITypeDescriptor ftype, out AssemblyReference reference)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			IResolutionScope scope = ftype.Scope;
			AssemblyReference val = (AssemblyReference)(object)((scope is AssemblyReference) ? scope : null);
			if (val != null)
			{
				reference = val;
				return null;
			}
			ModuleDefinition val2 = (ModuleDefinition)(object)((scope is ModuleDefinition) ? scope : null);
			if (val2 != null)
			{
				reference = new AssemblyReference((AssemblyDescriptor)(object)val2.Assembly);
				return null;
			}
			reference = null;
			return ((IMemberDescriptor)ftype).DeclaringType;
		}
	}
}
