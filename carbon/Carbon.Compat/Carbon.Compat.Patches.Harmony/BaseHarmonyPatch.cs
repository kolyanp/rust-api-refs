using AsmResolver.DotNet;
using Carbon.Compat.Converters;

namespace Carbon.Compat.Patches.Harmony;

public abstract class BaseHarmonyPatch : IAssemblyPatch
{
	public const string HarmonyASM = "0Harmony";

	public const string Harmony1NS = "Harmony";

	public const string Harmony2NS = "HarmonyLib";

	public const string HarmonyStr = "Harmony";

	public abstract void Apply(ModuleDefinition asm, ReferenceImporter importer, ref BaseConverter.Context context);
}
