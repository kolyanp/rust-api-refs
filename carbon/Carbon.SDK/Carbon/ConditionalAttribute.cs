using System;
using JetBrains.Annotations;

namespace Carbon;

[AttributeUsage(AttributeTargets.All)]
[MeansImplicitUse]
public class ConditionalAttribute : Attribute
{
	public string Symbol { get; set; }

	public ConditionalAttribute()
	{
	}

	public ConditionalAttribute(string symbol)
	{
		Symbol = symbol;
	}
}
