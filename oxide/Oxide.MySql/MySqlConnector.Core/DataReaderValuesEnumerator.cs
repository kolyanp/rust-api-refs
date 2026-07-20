using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class DataReaderValuesEnumerator(IDataReader dataReader) : IValuesEnumerator
{
	public int FieldCount => dataReader.FieldCount;

	public static IValuesEnumerator Create(IDataReader dataReader)
	{
		if (!(dataReader is DbDataReader dbDataReader))
		{
			return new DataReaderValuesEnumerator(dataReader);
		}
		return new DbDataReaderValuesEnumerator(dbDataReader);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public ValueTask<bool> MoveNextAsync()
	{
		return new ValueTask<bool>(MoveNext());
	}

	public bool MoveNext()
	{
		return dataReader.Read();
	}

	public void GetValues(object[] values)
	{
		dataReader.GetValues(values);
	}
}
