using System;
using System.Reflection;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
[MeansImplicitUse]
public class PluginReferenceAttribute : Attribute
{
	public string Name { get; set; }

	public string MinVersion { get; set; }

	public bool IsRequired { get; set; }

	public FieldInfo Field { get; set; }

	public PluginReferenceAttribute()
	{
	}

	public PluginReferenceAttribute(string name = null, string minVersion = null, bool isRequired = false)
	{
		Name = name;
		MinVersion = minVersion;
		IsRequired = isRequired;
	}
}
