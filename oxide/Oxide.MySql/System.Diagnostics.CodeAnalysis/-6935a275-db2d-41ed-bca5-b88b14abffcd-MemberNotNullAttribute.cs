namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
internal sealed class _003C6935a275_002Ddb2d_002D41ed_002Dbca5_002Db88b14abffcd_003EMemberNotNullAttribute : Attribute
{
	public string[] Members { get; }

	public _003C6935a275_002Ddb2d_002D41ed_002Dbca5_002Db88b14abffcd_003EMemberNotNullAttribute(string member)
	{
		Members = new string[1] { member };
	}

	public _003C6935a275_002Ddb2d_002D41ed_002Dbca5_002Db88b14abffcd_003EMemberNotNullAttribute(params string[] members)
	{
		Members = members;
	}
}
