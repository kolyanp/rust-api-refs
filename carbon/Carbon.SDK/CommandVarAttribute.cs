using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
[MeansImplicitUse]
public class CommandVarAttribute : Attribute
{
	public string Name { get; }

	public string Help { get; }

	public bool Protected { get; set; }

	public CommandVarAttribute(string name, string help = null)
	{
		Name = name;
		Help = help;
	}

	public CommandVarAttribute(string name, bool @protected, string help = null)
	{
		Name = name;
		Help = help;
		Protected = @protected;
	}
}
