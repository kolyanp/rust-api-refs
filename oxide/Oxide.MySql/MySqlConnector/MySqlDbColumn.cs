using System;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;
using MySqlConnector.Core;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector;

public sealed class MySqlDbColumn : DbColumn
{
	public MySqlDbType ProviderType { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	internal MySqlDbColumn(int ordinal, ColumnDefinitionPayload column, bool allowZeroDateTime, MySqlDbType mySqlDbType)
	{
		ColumnTypeMetadata columnTypeMetadata = TypeMapper.Instance.GetColumnTypeMetadata(mySqlDbType);
		Type clrType = columnTypeMetadata.DbTypeMapping.ClrType;
		long num = ((clrType == typeof(string) || clrType == typeof(Guid)) ? (column.ColumnLength / ProtocolUtility.GetBytesPerCharacter(column.CharacterSet)) : column.ColumnLength);
		base.AllowDBNull = (column.ColumnFlags & ColumnFlags.NotNull) == 0;
		base.BaseCatalogName = null;
		base.BaseColumnName = column.PhysicalName;
		base.BaseSchemaName = column.SchemaName;
		base.BaseTableName = column.PhysicalTable;
		base.ColumnName = column.Name;
		base.ColumnOrdinal = ordinal;
		base.ColumnSize = (int)((num > int.MaxValue) ? int.MaxValue : num);
		base.DataType = ((allowZeroDateTime && clrType == typeof(DateTime)) ? typeof(MySqlDateTime) : clrType);
		base.DataTypeName = columnTypeMetadata.SimpleDataTypeName;
		if (mySqlDbType == MySqlDbType.String)
		{
			base.DataTypeName += string.Format(CultureInfo.InvariantCulture, "({0})", num);
		}
		base.IsAliased = column.PhysicalName != column.Name;
		base.IsAutoIncrement = (column.ColumnFlags & ColumnFlags.AutoIncrement) != 0;
		base.IsExpression = false;
		base.IsHidden = false;
		base.IsKey = (column.ColumnFlags & ColumnFlags.PrimaryKey) != 0;
		bool flag = column.ColumnLength > 255;
		ColumnType columnType;
		if (flag)
		{
			bool flag2 = (column.ColumnFlags & ColumnFlags.Blob) != 0;
			if (!flag2)
			{
				columnType = column.ColumnType;
				flag2 = (uint)(columnType - 249) <= 3u;
			}
			flag = flag2;
		}
		base.IsLong = flag;
		base.IsReadOnly = false;
		base.IsUnique = (column.ColumnFlags & ColumnFlags.UniqueKey) != 0;
		columnType = column.ColumnType;
		if ((columnType == ColumnType.Decimal || columnType == ColumnType.NewDecimal) ? true : false)
		{
			base.NumericPrecision = (int)column.ColumnLength;
			if ((column.ColumnFlags & ColumnFlags.Unsigned) == 0)
			{
				base.NumericPrecision--;
			}
			if (column.Decimals > 0)
			{
				base.NumericPrecision--;
			}
		}
		base.NumericScale = column.Decimals;
		ProviderType = mySqlDbType;
	}
}
