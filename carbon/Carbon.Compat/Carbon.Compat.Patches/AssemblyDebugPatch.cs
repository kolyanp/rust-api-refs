using System.Diagnostics;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches;

public class AssemblyDebugPatch : IAssemblyPatch
{
	public void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Expected O, but got Unknown
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Expected O, but got Unknown
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Invalid comparison between Unknown and I4
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		if (!Debugger.IsAttached)
		{
			return;
		}
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			foreach (MethodDefinition method in allType.Methods)
			{
				CilMethodBody cilMethodBody = method.CilMethodBody;
				if (cilMethodBody == null)
				{
					continue;
				}
				for (int i = 0; i < cilMethodBody.Instructions.Count; i++)
				{
					CilInstruction val = cilMethodBody.Instructions[i];
					if (!(val.OpCode == CilOpCodes.Call))
					{
						continue;
					}
					object operand = val.Operand;
					MemberReference val2 = (MemberReference)((operand is MemberReference) ? operand : null);
					if (val2 == null || !((AssemblyDescriptor)((ITypeDescriptor)(object)val2.DeclaringType).DefinitionAssembly()).IsCorLib)
					{
						continue;
					}
					CallingConventionSignature signature = val2.Signature;
					MethodSignature val3 = (MethodSignature)(object)((signature is MethodSignature) ? signature : null);
					if (val3 == null || ((!(val2.DeclaringType.Name == "Debugger") || (!(val2.Name == "get_IsAttached") && !(val2.Name == "IsLogging"))) && (!(val2.DeclaringType.Name == "Environment") || !(val2.Name == "FailFast"))))
					{
						continue;
					}
					for (int j = 0; j < ((MethodSignatureBase)val3).ParameterTypes.Count; j++)
					{
						cilMethodBody.Instructions.Insert(i, new CilInstruction(CilOpCodes.Pop));
						i++;
					}
					if ((int)((MethodSignatureBase)val3).ReturnType.ElementType == 2)
					{
						val.OpCode = CilOpCodes.Ldc_I4_0;
						val.Operand = null;
						continue;
					}
					if (((MethodSignatureBase)val3).ReturnsValue && !((MethodSignatureBase)val3).ReturnType.IsValueType)
					{
						cilMethodBody.Instructions.Insert(i, new CilInstruction(CilOpCodes.Ldnull));
						i++;
					}
					cilMethodBody.Instructions.RemoveAt(i);
					i--;
				}
			}
		}
		for (int k = 0; k < ((AssemblyDescriptor)assembly.Assembly).CustomAttributes.Count; k++)
		{
			CustomAttribute val4 = ((AssemblyDescriptor)assembly.Assembly).CustomAttributes[k];
			if (((IFullNameProvider)((IMethodDefOrRef)val4.Constructor).DeclaringType).FullName == "System.Diagnostics.DebuggableAttribute" && ((AssemblyDescriptor)((ITypeDescriptor)(object)((IMethodDefOrRef)val4.Constructor).DeclaringType).DefinitionAssembly()).IsCorLib)
			{
				((AssemblyDescriptor)assembly.Assembly).CustomAttributes.RemoveAt(k--);
			}
		}
		TypeSignature val5 = importer.ImportTypeSignature(typeof(DebuggableAttribute.DebuggingModes));
		CustomAttribute item = new CustomAttribute((ICustomAttributeType)(object)TypeDescriptorExtensions.CreateMemberReference((IMemberRefParent)(object)importer.ImportType(typeof(DebuggableAttribute)), ".ctor", (MemberSignature)(object)MethodSignature.CreateInstance((TypeSignature)(object)assembly.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[1] { importer.ImportTypeSignature(typeof(DebuggableAttribute.DebuggingModes)) })).ImportWith(importer), new CustomAttributeSignature((CustomAttributeArgument[])(object)new CustomAttributeArgument[1]
		{
			new CustomAttributeArgument(val5, (object)262)
		}));
		((AssemblyDescriptor)assembly.Assembly).CustomAttributes.Add(item);
	}
}
