using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
[MeansImplicitUse]
public class AuthLevelAttribute : Attribute
{
	public int AuthLevel { get; }

	public AuthLevelAttribute(int level)
	{
		AuthLevel = level;
	}
}
