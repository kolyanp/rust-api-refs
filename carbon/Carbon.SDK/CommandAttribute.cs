using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class CommandAttribute : Attribute
{
	public string[] Names { get; } = new string[1];

	public CommandAttribute(string name)
	{
		Names[0] = name;
	}

	public CommandAttribute(params string[] commands)
	{
		Names = commands;
	}
}
