using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Core;

internal sealed class DbDataReaderValuesEnumerator([field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] DbDataReader dataReader) : IValuesEnumerator
{
	public int FieldCount => dataReader.FieldCount;

	public ValueTask<bool> MoveNextAsync()
	{
		return new ValueTask<bool>(dataReader.ReadAsync());
	}

	public bool MoveNext()
	{
		return dataReader.Read();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public void GetValues(object[] values)
	{
		dataReader.GetValues(values);
	}
}
