namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class _003C02e6fba7_002D1bc8_002D4ad5_002Db577_002D9e035590065f_003ELibraryImportAttribute : Attribute
{
	public string LibraryName { get; }

	public string EntryPoint { get; set; }

	public _003Caecd3c41_002D052f_002D4884_002D891d_002D5d7dcca54456_003EStringMarshalling StringMarshalling { get; set; }

	public Type StringMarshallingCustomType { get; set; }

	public bool SetLastError { get; set; }

	public _003C02e6fba7_002D1bc8_002D4ad5_002Db577_002D9e035590065f_003ELibraryImportAttribute(string libraryName)
	{
		LibraryName = libraryName;
	}
}
