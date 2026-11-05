using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestClassAttribute : Attribute
{
	public string Category;

	public TestRealm Realm { get; }

	public TestClassAttribute(TestRealm realm)
	{
		Realm = realm;
	}

	public string GetCategory()
	{
		return Category ?? "Uncategorized";
	}
}
