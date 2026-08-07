using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using Carbon.Base;
using Carbon.Compat.Converters;
using Carbon.Compat.Legacy.EventCompat;
using Carbon.Compat.Lib;
using HarmonyLib;

namespace Carbon.Compat.Patches.Oxide;

public class OxideTypeRef : BaseOxidePatch
{
	public static List<string> PluginToBaseHookable = new List<string> { "System.Void Oxide.Core.Libraries.Permission::RegisterPermission(System.String, Oxide.Core.Plugins.Plugin)", "System.Void Oxide.Core.Libraries.Lang::RegisterMessages(System.Collections.Generic.Dictionary`2<System.String, System.String>, Oxide.Core.Plugins.Plugin, System.String)", "System.Void Oxide.Game.Rust.Libraries.Command::RemoveConsoleCommand(System.String, Oxide.Core.Plugins.Plugin)", "System.Collections.Generic.Dictionary`2<System.String, System.String> Oxide.Core.Libraries.Lang::GetMessages(System.String, Oxide.Core.Plugins.Plugin)" };

	public override void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		foreach (MemberReference importedMemberReference in assembly.GetImportedMemberReferences())
		{
			AssemblyReference aref = ((ITypeDescriptor)(object)importedMemberReference.DeclaringType).DefinitionAssembly();
			CallingConventionSignature signature = importedMemberReference.Signature;
			MethodSignature val = (MethodSignature)(object)((signature is MethodSignature) ? signature : null);
			if (val == null || !Helpers.IsOxideASM(aref))
			{
				continue;
			}
			string fullName = importedMemberReference.FullName;
			if (PluginToBaseHookable.Contains(importedMemberReference.FullName))
			{
				for (int i = 0; i < ((MethodSignatureBase)val).ParameterTypes.Count; i++)
				{
					TypeSignature val2 = ((MethodSignatureBase)val).ParameterTypes[i];
					if (val2.FullName == "Oxide.Core.Plugins.Plugin" && Helpers.IsOxideASM(((ITypeDescriptor)(object)val2).DefinitionAssembly()))
					{
						((MethodSignatureBase)val).ParameterTypes[i] = importer.ImportTypeSignature(typeof(BaseHookable));
					}
				}
			}
			else if (val.GenericParameterCount == 1 && fullName == "!!0 Oxide.Core.Interface::Call<?>(System.String, System.Object[])")
			{
				importedMemberReference.Parent = (IMemberRefParent)(object)importer.ImportType(typeof(OxideCompat));
				importedMemberReference.Name = Utf8String.op_Implicit("OxideCallHookGeneric");
			}
			else if (fullName == "System.String Oxide.Core.Libraries.Lang::GetMessage(System.String, Oxide.Core.Plugins.Plugin, System.String)")
			{
				importedMemberReference.Signature = (CallingConventionSignature)(object)importer.ImportMethod((MethodBase)AccessTools.Method(typeof(OxideCompat), "GetMessage1", (Type[])null, (Type[])null)).Signature;
				importedMemberReference.Parent = (IMemberRefParent)(object)importer.ImportType(typeof(OxideCompat));
				importedMemberReference.Name = Utf8String.op_Implicit("GetMessage1");
			}
		}
		foreach (TypeReference importedTypeReference in assembly.GetImportedTypeReferences())
		{
			ProcessTypeRef(importedTypeReference, importer);
		}
		ProcessAttrList(assembly.CustomAttributes);
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			ProcessAttrList(allType.CustomAttributes);
			foreach (FieldDefinition field in allType.Fields)
			{
				ProcessAttrList(field.CustomAttributes);
			}
			foreach (MethodDefinition method in allType.Methods)
			{
				ProcessAttrList(method.CustomAttributes);
			}
			foreach (PropertyDefinition property in allType.Properties)
			{
				ProcessAttrList(property.CustomAttributes);
			}
		}
		void ProcessAttrList(IList<CustomAttribute> list)
		{
			for (int j = 0; j < list.Count; j++)
			{
				CustomAttribute val3 = list[j];
				try
				{
					int num = 0;
					while (true)
					{
						int num2 = num;
						CustomAttributeSignature signature2 = val3.Signature;
						if (!(num2 < ((signature2 != null) ? new int?(signature2.FixedArguments.Count) : ((int?)null))))
						{
							break;
						}
						CustomAttributeArgument val4 = val3.Signature.FixedArguments[num];
						object element = val4.Element;
						TypeDefOrRefSignature val5 = (TypeDefOrRefSignature)((element is TypeDefOrRefSignature) ? element : null);
						if (val5 != null)
						{
							ITypeDefOrRef type = val5.Type;
							ProcessTypeRef((TypeReference)(object)((type is TypeReference) ? type : null), importer);
						}
						num++;
					}
				}
				catch
				{
				}
			}
		}
	}

	public static void ProcessTypeRef(TypeReference type, ReferenceImporter importer)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		if (type == null)
		{
			return;
		}
		IResolutionScope scope = type.Scope;
		TypeReference val = (TypeReference)(object)((scope is TypeReference) ? scope : null);
		if (val != null && val.FullName == "Oxide.Plugins.Timer" && type.Name == "Timer")
		{
			type.Name = Utf8String.op_Implicit("Timer");
			type.Namespace = Utf8String.op_Implicit("Oxide.Plugins");
			type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.Common).ImportWith(importer);
			return;
		}
		IResolutionScope scope2 = type.Scope;
		AssemblyReference val2 = (AssemblyReference)(object)((scope2 is AssemblyReference) ? scope2 : null);
		if (val2 == null || !Helpers.IsOxideASM(val2))
		{
			return;
		}
		if (type.FullName == "Oxide.Core.Event" || type.FullName.StartsWith("Oxide.Core.Event`"))
		{
			type.Scope = (IResolutionScope)importer.ImportType(typeof(OxideEvents));
			return;
		}
		if (type.FullName == "Oxide.Core.Plugins.PluginEvent")
		{
			type.Namespace = Utf8String.op_Implicit(string.Empty);
		}
		if (type.Namespace.StartsWith("Newtonsoft.Json"))
		{
			type.Scope = Helpers.GetNewtonsoftScope(type, importer);
			return;
		}
		if (type.Namespace.StartsWith("ProtoBuf"))
		{
			if (type.Namespace == "ProtoBuf" && type.Name == "Serializer")
			{
				type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.protobuf).ImportWith(importer);
			}
			else
			{
				type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.protobufCore).ImportWith(importer);
			}
			return;
		}
		if (type.Namespace.StartsWith("WebSocketSharp"))
		{
			if (type.Namespace == "WebSocketSharp.Net" && type.Name == "SslConfiguration")
			{
				type.Name = Utf8String.op_Implicit("ClientSslConfiguration");
			}
			type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.wsSharp).ImportWith(importer);
			return;
		}
		if (!(type.Name == "VersionNumber"))
		{
			if (!(type.Namespace == "Oxide.Plugins") || !type.Name.EndsWith("Attribute"))
			{
				if (type.FullName == "Oxide.Plugins.Hash`2")
				{
					type.Namespace = Utf8String.op_Implicit(string.Empty);
				}
				else
				{
					string fullName = type.FullName;
					if ((!(fullName == "Oxide.Core.Libraries.Timer") && !(fullName == "Oxide.Plugins.PluginTimers")) || 1 == 0)
					{
						if (type.FullName == "Oxide.Core.Plugins.HookMethodAttribute")
						{
							type.Namespace = Utf8String.op_Implicit(string.Empty);
							goto IL_02fe;
						}
						fullName = type.FullName;
						if ((fullName == "Oxide.Plugins.CSharpPlugin" || fullName == "Oxide.Core.Plugins.CSPlugin") ? true : false)
						{
							type.Name = Utf8String.op_Implicit("RustPlugin");
							type.Namespace = Utf8String.op_Implicit("Oxide.Plugins");
						}
						else if (type.FullName == "Oxide.Core.Plugins.PluginManager")
						{
							type.Namespace = Utf8String.op_Implicit(string.Empty);
						}
					}
				}
				type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.Common).ImportWith(importer);
				return;
			}
			type.Namespace = Utf8String.op_Implicit(string.Empty);
		}
		goto IL_02fe;
		IL_02fe:
		type.Scope = (IResolutionScope)(object)((AssemblyDescriptor)CompatManager.SDK).ImportWith(importer);
	}
}
