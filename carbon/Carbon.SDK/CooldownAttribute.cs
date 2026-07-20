using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method)]
[MeansImplicitUse]
public class CooldownAttribute : Attribute
{
	public int Miliseconds { get; }

	public bool DoCooldownPenalty { get; }

	public CooldownAttribute(int miliseconds, bool doCooldownPenalty = false)
	{
		Miliseconds = miliseconds;
		DoCooldownPenalty = doCooldownPenalty;
	}
}
