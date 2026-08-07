using System;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches;

public class AssemblyVersionPatch : IAssemblyPatch
{
	public void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (AssemblyReference assemblyReference in assembly.AssemblyReferences)
		{
			Utf8String culture = ((AssemblyDescriptor)assemblyReference).Culture;
			if (string.IsNullOrEmpty((culture != null) ? culture.Value : null) && Helpers.TryGetLoadedIdentity(((AssemblyDescriptor)assemblyReference).Name, assemblies, out var identity))
			{
				assemblyReference.AlignIdentityWith(identity);
			}
		}
	}
}
