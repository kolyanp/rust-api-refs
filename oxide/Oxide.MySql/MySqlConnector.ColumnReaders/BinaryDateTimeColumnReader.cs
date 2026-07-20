using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Utilities;

namespace MySqlConnector.ColumnReaders;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class BinaryDateTimeColumnReader : ColumnReader
{
	private readonly bool m_allowZeroDateTime;

	private readonly bool m_convertZeroDateTime;

	private readonly DateTimeKind m_dateTimeKind;

	public BinaryDateTimeColumnReader(MySqlConnection connection)
	{
		m_allowZeroDateTime = connection.AllowZeroDateTime;
		m_convertZeroDateTime = connection.ConvertZeroDateTime;
		m_dateTimeKind = connection.DateTimeKind;
	}

	public override object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		if (data.Length == 0)
		{
			if (m_convertZeroDateTime)
			{
				return DateTime.MinValue;
			}
			if (m_allowZeroDateTime)
			{
				return default(MySqlDateTime);
			}
			throw new InvalidCastException("Unable to convert MySQL date/time to System.DateTime.");
		}
		int year = data[0] + data[1] * 256;
		int month = data[2];
		int day = data[3];
		int hour;
		int minute;
		int second;
		if (data.Length <= 4)
		{
			hour = 0;
			minute = 0;
			second = 0;
		}
		else
		{
			hour = data[4];
			minute = data[5];
			second = data[6];
		}
		int num;
		if (data.Length > 7)
		{
			num = MemoryMarshal.Read<int>(data.Slice(7, data.Length - 7));
		}
		else
		{
			num = 0;
		}
		int num2 = num;
		try
		{
			return m_allowZeroDateTime ? ((object)new MySqlDateTime(year, month, day, hour, minute, second, num2)) : ((object)new DateTime(year, month, day, hour, minute, second, num2 / 1000, m_dateTimeKind).AddTicks(num2 % 1000 * 10));
		}
		catch (Exception innerException)
		{
			throw new FormatException("Couldn't interpret value as a valid DateTime: " + Utility.GetString(Encoding.UTF8, data), innerException);
		}
	}
}
