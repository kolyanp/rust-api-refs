using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class ColumnTypeMetadata(string dataTypeName, DbTypeMapping dbTypeMapping, MySqlDbType mySqlDbType, bool isUnsigned = false, bool binary = false, int length = 0, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string simpleDataTypeName = null, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string createFormat = null, long columnSize = 0L)
{
	public string DataTypeName { get; } = dataTypeName;

	public string SimpleDataTypeName { get; } = simpleDataTypeName ?? dataTypeName;

	public string CreateFormat { get; } = createFormat ?? (dataTypeName + (isUnsigned ? " UNSIGNED" : ""));

	public DbTypeMapping DbTypeMapping { get; } = dbTypeMapping;

	public MySqlDbType MySqlDbType { get; } = mySqlDbType;

	public bool Binary { get; } = binary;

	public long ColumnSize { get; } = columnSize;

	public bool IsUnsigned { get; } = isUnsigned;

	public int Length { get; } = length;

	public static string CreateLookupKey(string columnTypeName, bool isUnsigned, int length)
	{
		return string.Format("{0}|{1}|{2}", columnTypeName, isUnsigned ? "u" : "s", length);
	}

	public string CreateLookupKey()
	{
		return CreateLookupKey(DataTypeName, IsUnsigned, Length);
	}
}
