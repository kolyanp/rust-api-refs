using System.IO;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Carbon.Compat.Patches;

internal static class PrefabIdPatch
{
	private static readonly PropertyInfo PrefabId = typeof(BaseNetworkable).GetProperty("prefabID", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

	internal static byte[] Rewrite(byte[] bytes, ModuleDefinition module = null)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		if (PrefabId == null)
		{
			return bytes;
		}
		if (module == null)
		{
			module = ModuleDefinition.FromBytes(bytes);
		}
		if (!Apply(module))
		{
			return bytes;
		}
		using MemoryStream memoryStream = new MemoryStream();
		module.Write((Stream)memoryStream, (IPEImageBuilder)new ManagedPEImageBuilder((MetadataBuilderFlags)524287));
		return memoryStream.ToArray();
	}

	internal static bool Apply(ModuleDefinition module)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Invalid comparison between Unknown and I4
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		if (PrefabId?.PropertyType != typeof(uint) || !module.GetImportedMemberReferences().Any(IsPrefabIdField))
		{
			return false;
		}
		IMethodDescriptor val = null;
		IMethodDescriptor val2 = null;
		foreach (TypeDefinition allType in module.GetAllTypes())
		{
			for (int i = 0; i < allType.Methods.Count; i++)
			{
				CilMethodBody cilMethodBody = allType.Methods[i].CilMethodBody;
				CilInstructionCollection val3 = ((cilMethodBody != null) ? cilMethodBody.Instructions : null);
				if (val3 == null)
				{
					continue;
				}
				for (int j = 0; j < val3.Count; j++)
				{
					CilInstruction val4 = val3[j];
					if (val4.OpCode != CilOpCodes.Ldfld && val4.OpCode != CilOpCodes.Stfld)
					{
						continue;
					}
					object operand = val4.Operand;
					MemberReference val5 = (MemberReference)((operand is MemberReference) ? operand : null);
					if (val5 == null || !IsPrefabIdField(val5))
					{
						continue;
					}
					if (j > 0)
					{
						CilOpCode opCode = val3[j - 1].OpCode;
						if ((int)opCode.OpCodeType == 4)
						{
							continue;
						}
					}
					val4.Operand = ((val4.OpCode == CilOpCodes.Ldfld) ? (val ?? (val = module.DefaultImporter.ImportMethod((MethodBase)PrefabId.GetMethod))) : (val2 ?? (val2 = module.DefaultImporter.ImportMethod((MethodBase)PrefabId.SetMethod))));
					val4.OpCode = CilOpCodes.Callvirt;
				}
			}
		}
		if (val == null)
		{
			return val2 != null;
		}
		return true;
	}

	private static bool IsPrefabIdField(MemberReference member)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		if (member.Name == "prefabID")
		{
			CallingConventionSignature signature = member.Signature;
			FieldSignature val = (FieldSignature)(object)((signature is FieldSignature) ? signature : null);
			if (val != null && (int)val.FieldType.ElementType == 9)
			{
				IMemberRefParent parent = member.Parent;
				TypeReference val2 = (TypeReference)(object)((parent is TypeReference) ? parent : null);
				if (val2 != null)
				{
					IResolutionScope scope = val2.Scope;
					AssemblyReference val3 = (AssemblyReference)(object)((scope is AssemblyReference) ? scope : null);
					if (val3 != null && val2.FullName == "BaseNetworkable")
					{
						return ((AssemblyDescriptor)val3).Name == "Assembly-CSharp";
					}
				}
			}
		}
		return false;
	}
}
