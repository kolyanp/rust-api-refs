using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class DataRowsValuesEnumerator(IEnumerable<DataRow> dataRows, int columnCount) : IValuesEnumerator
{
	private readonly IEnumerator<DataRow> m_dataRows = dataRows.GetEnumerator();

	public int FieldCount { get; } = columnCount;

	public static IValuesEnumerator Create(DataTable dataTable)
	{
		return new DataRowsValuesEnumerator(from DataRow x in dataTable.Rows
			where x != null
			select (x), dataTable.Columns.Count);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	public ValueTask<bool> MoveNextAsync()
	{
		return new ValueTask<bool>(MoveNext());
	}

	public bool MoveNext()
	{
		if (m_dataRows.MoveNext())
		{
			return true;
		}
		m_dataRows.Dispose();
		return false;
	}

	public void GetValues(object[] values)
	{
		DataRow current = m_dataRows.Current;
		for (int i = 0; i < FieldCount; i++)
		{
			values[i] = current[i];
		}
	}
}
