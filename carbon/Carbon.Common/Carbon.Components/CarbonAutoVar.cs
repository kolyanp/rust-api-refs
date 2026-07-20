using System;

namespace Carbon.Components;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CarbonAutoVar : CommandVarAttribute
{
	public string DisplayName;

	public bool ForceModded;

	public CarbonAutoVar(string name, string displayName, string help = null, bool @protected = false, bool forceModded = false)
		: base(name, @protected, help)
	{
		DisplayName = displayName;
		ForceModded = forceModded;
	}
}
