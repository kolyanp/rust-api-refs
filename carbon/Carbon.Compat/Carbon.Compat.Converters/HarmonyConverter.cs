using System.Collections.Generic;
using System.Collections.Immutable;
using Carbon.Compat.Patches;
using Carbon.Compat.Patches.Harmony;
using Carbon.Compat.Patches.Oxide;
using JetBrains.Annotations;

namespace Carbon.Compat.Converters;

[UsedImplicitly]
public class HarmonyConverter : BaseConverter
{
	private readonly ImmutableList<IAssemblyPatch> _patches = ImmutableList.ToImmutableList<IAssemblyPatch>((IEnumerable<IAssemblyPatch>)new List<IAssemblyPatch>
	{
		new HarmonyTypeRef(),
		new OxideTypeRef(),
		new OxideILSwitch(),
		new HarmonyPatchProcessor(),
		new ReflectionFlagsPatch(),
		new AssemblyVersionPatch(),
		new AssemblyDebugPatch()
	});

	public override ImmutableList<IAssemblyPatch> Patches => _patches;

	public override string Name => "HarmonyMod";
}
