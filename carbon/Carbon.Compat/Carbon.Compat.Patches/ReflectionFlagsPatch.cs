using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches;

public class ReflectionFlagsPatch : IAssemblyPatch
{
	public static List<string> ReflectionTypeMethods = new List<string> { "GetMethod", "GetField", "GetProperty", "GetMember" };

	public void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			foreach (MethodDefinition method in allType.Methods)
			{
				MethodBody methodBody = method.MethodBody;
				CilMethodBody val = (CilMethodBody)(object)((methodBody is CilMethodBody) ? methodBody : null);
				if (val == null)
				{
					continue;
				}
				for (int i = 0; i < val.Instructions.Count; i++)
				{
					CilInstruction val2 = val.Instructions[i];
					if (!(val2.OpCode == CilOpCodes.Callvirt))
					{
						continue;
					}
					object operand = val2.Operand;
					MemberReference val3 = (MemberReference)((operand is MemberReference) ? operand : null);
					if (val3 == null)
					{
						continue;
					}
					CallingConventionSignature signature = val3.Signature;
					MethodSignature val4 = (MethodSignature)(object)((signature is MethodSignature) ? signature : null);
					if (val4 == null)
					{
						continue;
					}
					ITypeDefOrRef declaringType = val3.DeclaringType;
					TypeReference val5 = (TypeReference)(object)((declaringType is TypeReference) ? declaringType : null);
					if (val5 == null)
					{
						continue;
					}
					IResolutionScope scope = val5.Scope;
					AssemblyReference val6 = (AssemblyReference)(object)((scope is AssemblyReference) ? scope : null);
					if (val6 == null || !((AssemblyDescriptor)val6).IsCorLib || !(val5.Name == "Type") || !ReflectionTypeMethods.Contains(Utf8String.op_Implicit(val3.Name)) || !((MethodSignatureBase)val4).ParameterTypes.Any(delegate(TypeSignature x)
					{
						IResolutionScope scope2 = x.Scope;
						AssemblyReference val8 = (AssemblyReference)(object)((scope2 is AssemblyReference) ? scope2 : null);
						return val8 != null && ((AssemblyDescriptor)val8).IsCorLib && x.Name == "BindingFlags";
					}))
					{
						continue;
					}
					int num = i - 1;
					while (true)
					{
						if (num >= Math.Max(i - 5, 0))
						{
							CilInstruction val7 = val.Instructions[num];
							if (val7.IsLdcI4())
							{
								BindingFlags bindingFlags = (BindingFlags)(val7.GetLdcI4Constant() | 0x10 | 0x20);
								val7.Operand = (int)bindingFlags;
								val7.OpCode = CilOpCodes.Ldc_I4;
								break;
							}
							num--;
							continue;
						}
						Logger.Error(string.Format("Failed to find binding flags for {0} at #IL_{1:X}:{2} in {3}", new object[4] { method.FullName, val2.Offset, i, assembly.Name }));
						break;
					}
				}
			}
		}
	}
}
