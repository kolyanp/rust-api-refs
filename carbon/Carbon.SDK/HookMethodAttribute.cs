using System;
using System.Reflection;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class HookMethodAttribute : Attribute
{
	public string Name { get; set; }

	public MethodInfo Method { get; set; }

	public HookMethodAttribute()
	{
	}

	public HookMethodAttribute(string name)
	{
		Name = name;
	}
}
