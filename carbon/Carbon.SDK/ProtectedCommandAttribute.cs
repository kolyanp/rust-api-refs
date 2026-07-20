using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class ProtectedCommandAttribute : Attribute
{
	public string Name { get; }

	public string Help { get; }

	public ProtectedCommandAttribute(string name)
	{
		Name = name;
	}

	public ProtectedCommandAttribute(string name, string help)
	{
		Name = name;
		Help = help;
	}
}
