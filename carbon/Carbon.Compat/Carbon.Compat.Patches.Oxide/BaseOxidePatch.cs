using AsmResolver.DotNet;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches.Oxide;

public abstract class BaseOxidePatch : IAssemblyPatch
{
	public const string OxideStr = "Oxide";

	public abstract void Apply(ModuleDefinition assembly, ReferenceImporter importer, ref BaseConverter.Context context);
}
