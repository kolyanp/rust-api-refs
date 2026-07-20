using AsmResolver.DotNet;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches;

public interface IAssemblyPatch
{
	void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context);
}
