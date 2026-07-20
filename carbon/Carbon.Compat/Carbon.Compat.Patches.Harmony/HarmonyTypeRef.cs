using AsmResolver;
using AsmResolver.DotNet;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches.Harmony;

public class HarmonyTypeRef : BaseHarmonyPatch
{
	public override void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		foreach (TypeReference importedTypeReference in assembly.GetImportedTypeReferences())
		{
			IResolutionScope scope = importedTypeReference.Scope;
			AssemblyReference val = (AssemblyReference)(object)((scope is AssemblyReference) ? scope : null);
			if (val != null && ((AssemblyDescriptor)val).Name == "0Harmony")
			{
				if (importedTypeReference.Namespace.StartsWith("Harmony"))
				{
					importedTypeReference.Namespace = Utf8String.op_Implicit("HarmonyLib");
				}
				if (importedTypeReference.Name == "HarmonyInstance")
				{
					importedTypeReference.Name = Utf8String.op_Implicit("Harmony");
				}
			}
		}
	}
}
