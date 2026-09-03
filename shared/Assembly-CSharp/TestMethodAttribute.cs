using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[MeansImplicitUse]
public class TestMethodAttribute : Attribute
{
	public object[] TestParameters { get; private set; }

	public TestMethodAttribute(params object[] testParameters)
	{
		TestParameters = testParameters;
	}
}
