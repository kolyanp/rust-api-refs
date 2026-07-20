using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class RConCommandAttribute : Attribute
{
	public string Name { get; }

	public string Help { get; }

	public RConCommandAttribute(string name, string help = null)
	{
		Name = name;
		Help = help;
	}
}
