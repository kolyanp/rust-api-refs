using System;

namespace Carbon.Components;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CarbonAutoModdedVar : CarbonAutoVar
{
	public CarbonAutoModdedVar(string name, string displayName, string help = null, bool @protected = false, bool forceModded = false)
		: base(name, displayName, help, @protected, forceModded)
	{
		ForceModded = true;
	}
}
