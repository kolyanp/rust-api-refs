using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class DescriptionAttribute : Attribute
{
	public string Description { get; }

	public DescriptionAttribute(string description)
	{
		Description = description;
	}
}
