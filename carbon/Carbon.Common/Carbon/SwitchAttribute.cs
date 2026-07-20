using System;

namespace Carbon;

[AttributeUsage(AttributeTargets.Method)]
public class SwitchAttribute : Attribute
{
	public string Name { get; set; }

	public string Help { get; set; }
}
