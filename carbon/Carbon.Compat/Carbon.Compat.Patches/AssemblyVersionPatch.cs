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
			Assembly[] array = assemblies;
			foreach (Assembly assembly2 in array)
			{
				AssemblyName name = assembly2.GetName();
				if (Utf8String.op_Implicit(name.Name) == ((AssemblyDescriptor)assemblyReference).Name)
				{
					((AssemblyDescriptor)assemblyReference).Version = name.Version;
				}
			}
		}
	}
}
