using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using Carbon.Compat.Converters;
using Facepunch.Crypt;

namespace Carbon.Compat.Patches.Oxide;

public class OxidePluginAttribute : BaseOxidePatch
{
	public override void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		string text = context.Author ?? "CCL";
		foreach (TypeDefinition allType in assembly.GetAllTypes())
		{
			if (allType.IsBaseType((ITypeDefOrRef x) => x.Name == "RustPlugin" && ((AssemblyDescriptor)((ITypeDescriptor)(object)x).DefinitionAssembly()).Name == "Carbon.Common"))
			{
				if (((object)allType.Name).ToString().IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				{
					string text2 = "plugin_" + Md5.Calculate(Utf8String.op_Implicit(allType.Name));
					Logger.Warn($"Plugin \"{allType.Name}\" has an invalid name, renaming to {text2}");
					allType.Name = Utf8String.op_Implicit(text2);
				}
				CustomAttribute val = allType.CustomAttributes.FirstOrDefault((CustomAttribute x) => ((IFullNameProvider)((IMethodDefOrRef)x.Constructor).DeclaringType).FullName == "InfoAttribute" && ((AssemblyDescriptor)((ITypeDescriptor)(object)((IMethodDefOrRef)x.Constructor).DeclaringType).DefinitionAssembly()).Name == "Carbon.Common");
				if (val == null)
				{
					IList<CustomAttribute> customAttributes = allType.CustomAttributes;
					CustomAttribute val2 = new CustomAttribute((ICustomAttributeType)(object)TypeDescriptorExtensions.CreateMemberReference((IMemberRefParent)(object)importer.ImportType(typeof(InfoAttribute)), ".ctor", (MemberSignature)(object)MethodSignature.CreateInstance((TypeSignature)(object)assembly.CorLibTypeFactory.Void, (TypeSignature[])(object)new TypeSignature[3]
					{
						(TypeSignature)assembly.CorLibTypeFactory.String,
						(TypeSignature)assembly.CorLibTypeFactory.String,
						(TypeSignature)assembly.CorLibTypeFactory.Double
					})).ImportWith(importer));
					val2.Signature = new CustomAttributeSignature((CustomAttributeArgument[])(object)new CustomAttributeArgument[3]
					{
						new CustomAttributeArgument((TypeSignature)(object)assembly.CorLibTypeFactory.String, (object)$"{((AssemblyDescriptor)assembly.Assembly).Name}-{allType.Name}"),
						new CustomAttributeArgument((TypeSignature)(object)assembly.CorLibTypeFactory.String, (object)text),
						new CustomAttributeArgument((TypeSignature)(object)assembly.CorLibTypeFactory.Double, (object)0.0)
					});
					customAttributes.Add(val2);
				}
			}
		}
	}
}
