using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class TypeMapper
{
	private readonly List<ColumnTypeMetadata> m_columnTypeMetadata;

	private readonly Dictionary<Type, DbTypeMapping> m_dbTypeMappingsByClrType;

	private readonly Dictionary<DbType, DbTypeMapping> m_dbTypeMappingsByDbType;

	private readonly Dictionary<string, ColumnTypeMetadata> m_columnTypeMetadataLookup;

	private readonly Dictionary<MySqlDbType, ColumnTypeMetadata> m_mySqlDbTypeToColumnTypeMetadata;

	public static TypeMapper Instance { get; } = new TypeMapper();

	private TypeMapper()
	{
		m_columnTypeMetadata = new List<ColumnTypeMetadata>();
		m_dbTypeMappingsByClrType = new Dictionary<Type, DbTypeMapping>();
		m_dbTypeMappingsByDbType = new Dictionary<DbType, DbTypeMapping>();
		m_columnTypeMetadataLookup = new Dictionary<string, ColumnTypeMetadata>(StringComparer.OrdinalIgnoreCase);
		m_mySqlDbTypeToColumnTypeMetadata = new Dictionary<MySqlDbType, ColumnTypeMetadata>();
		DbTypeMapping dbTypeMapping = AddDbTypeMapping(new DbTypeMapping(typeof(bool), new DbType[1] { DbType.Boolean }, (object o) => Convert.ToBoolean(o, CultureInfo.InvariantCulture)));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYINT", dbTypeMapping, MySqlDbType.Bool, isUnsigned: false, binary: false, 1, "BOOL", "BOOL", 1L));
		DbTypeMapping dbTypeMapping2 = AddDbTypeMapping(new DbTypeMapping(typeof(sbyte), new DbType[1] { DbType.SByte }, (object o) => Convert.ToSByte(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping3 = AddDbTypeMapping(new DbTypeMapping(typeof(byte), new DbType[1] { DbType.Byte }, (object o) => Convert.ToByte(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping4 = AddDbTypeMapping(new DbTypeMapping(typeof(short), new DbType[1] { DbType.Int16 }, (object o) => Convert.ToInt16(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping5 = AddDbTypeMapping(new DbTypeMapping(typeof(ushort), new DbType[1] { DbType.UInt16 }, (object o) => Convert.ToUInt16(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping6 = AddDbTypeMapping(new DbTypeMapping(typeof(int), new DbType[1] { DbType.Int32 }, (object o) => Convert.ToInt32(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping7 = AddDbTypeMapping(new DbTypeMapping(typeof(uint), new DbType[1] { DbType.UInt32 }, (object o) => Convert.ToUInt32(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping8 = AddDbTypeMapping(new DbTypeMapping(typeof(long), new DbType[1] { DbType.Int64 }, (object o) => Convert.ToInt64(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping9 = AddDbTypeMapping(new DbTypeMapping(typeof(ulong), new DbType[1] { DbType.UInt64 }, (object o) => Convert.ToUInt64(o, CultureInfo.InvariantCulture)));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYINT", dbTypeMapping2, MySqlDbType.Byte, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYINT", dbTypeMapping3, MySqlDbType.UByte, isUnsigned: true, binary: false, 1, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYINT", dbTypeMapping3, MySqlDbType.UByte, isUnsigned: true, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("SMALLINT", dbTypeMapping4, MySqlDbType.Int16, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("SMALLINT", dbTypeMapping5, MySqlDbType.UInt16, isUnsigned: true, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("INT", dbTypeMapping6, MySqlDbType.Int32, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("INT", dbTypeMapping7, MySqlDbType.UInt32, isUnsigned: true, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MEDIUMINT", dbTypeMapping6, MySqlDbType.Int24, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MEDIUMINT", dbTypeMapping7, MySqlDbType.UInt24, isUnsigned: true, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("BIGINT", dbTypeMapping8, MySqlDbType.Int64, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("BIGINT", dbTypeMapping9, MySqlDbType.UInt64, isUnsigned: true, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("BIT", dbTypeMapping9, MySqlDbType.Bit, isUnsigned: false, binary: false, 0, null, null, 0L));
		DbTypeMapping dbTypeMapping10 = AddDbTypeMapping(new DbTypeMapping(typeof(decimal), new DbType[3]
		{
			DbType.Decimal,
			DbType.Currency,
			DbType.VarNumeric
		}, (object o) => Convert.ToDecimal(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping11 = AddDbTypeMapping(new DbTypeMapping(typeof(double), new DbType[1] { DbType.Double }, (object o) => Convert.ToDouble(o, CultureInfo.InvariantCulture)));
		DbTypeMapping dbTypeMapping12 = AddDbTypeMapping(new DbTypeMapping(typeof(float), new DbType[1] { DbType.Single }, (object o) => Convert.ToSingle(o, CultureInfo.InvariantCulture)));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DECIMAL", dbTypeMapping10, MySqlDbType.NewDecimal, isUnsigned: false, binary: false, 0, null, "DECIMAL({0},{1});precision,scale", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DECIMAL", dbTypeMapping10, MySqlDbType.NewDecimal, isUnsigned: true, binary: false, 0, null, "DECIMAL({0},{1}) UNSIGNED;precision,scale", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DECIMAL", dbTypeMapping10, MySqlDbType.Decimal, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DOUBLE", dbTypeMapping11, MySqlDbType.Double, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("FLOAT", dbTypeMapping12, MySqlDbType.Float, isUnsigned: false, binary: false, 0, null, null, 0L));
		DbTypeMapping dbTypeMapping13 = AddDbTypeMapping(new DbTypeMapping(typeof(string), new DbType[2]
		{
			DbType.StringFixedLength,
			DbType.AnsiStringFixedLength
		}, Convert.ToString));
		DbTypeMapping dbTypeMapping14 = AddDbTypeMapping(new DbTypeMapping(typeof(string), new DbType[3]
		{
			DbType.String,
			DbType.AnsiString,
			DbType.Xml
		}, Convert.ToString));
		AddColumnTypeMetadata(new ColumnTypeMetadata("VARCHAR", dbTypeMapping14, MySqlDbType.VarChar, isUnsigned: false, binary: false, 0, null, "VARCHAR({0});size", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("VARCHAR", dbTypeMapping14, MySqlDbType.VarString, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("CHAR", dbTypeMapping13, MySqlDbType.String, isUnsigned: false, binary: false, 0, null, "CHAR({0});size", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYTEXT", dbTypeMapping14, MySqlDbType.TinyText, isUnsigned: false, binary: false, 0, "VARCHAR", null, 255L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TEXT", dbTypeMapping14, MySqlDbType.Text, isUnsigned: false, binary: false, 0, "VARCHAR", null, 65535L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MEDIUMTEXT", dbTypeMapping14, MySqlDbType.MediumText, isUnsigned: false, binary: false, 0, "VARCHAR", null, 16777215L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("LONGTEXT", dbTypeMapping14, MySqlDbType.LongText, isUnsigned: false, binary: false, 0, "VARCHAR", null, 4294967295L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("ENUM", dbTypeMapping14, MySqlDbType.Enum, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("SET", dbTypeMapping14, MySqlDbType.Set, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("JSON", dbTypeMapping14, MySqlDbType.JSON, isUnsigned: false, binary: false, 0, null, null, 0L));
		DbTypeMapping dbTypeMapping15 = AddDbTypeMapping(new DbTypeMapping(typeof(byte[]), new DbType[1] { DbType.Binary }));
		AddColumnTypeMetadata(new ColumnTypeMetadata("BLOB", dbTypeMapping15, MySqlDbType.Blob, isUnsigned: false, binary: true, 0, "BLOB", null, 65535L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("BINARY", dbTypeMapping15, MySqlDbType.Binary, isUnsigned: false, binary: true, 0, "BLOB", "BINARY({0});length", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("VARBINARY", dbTypeMapping15, MySqlDbType.VarBinary, isUnsigned: false, binary: true, 0, "BLOB", "VARBINARY({0});length", 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TINYBLOB", dbTypeMapping15, MySqlDbType.TinyBlob, isUnsigned: false, binary: true, 0, "BLOB", null, 255L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MEDIUMBLOB", dbTypeMapping15, MySqlDbType.MediumBlob, isUnsigned: false, binary: true, 0, "BLOB", null, 16777215L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("LONGBLOB", dbTypeMapping15, MySqlDbType.LongBlob, isUnsigned: false, binary: true, 0, "BLOB", null, 4294967295L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("GEOMETRY", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("POINT", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("LINESTRING", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("POLYGON", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MULTIPOINT", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MULTILINESTRING", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("MULTIPOLYGON", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("GEOMETRYCOLLECTION", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("GEOMCOLLECTION", dbTypeMapping15, MySqlDbType.Geometry, isUnsigned: false, binary: true, 0, null, null, 0L));
		DbTypeMapping dbTypeMapping16 = AddDbTypeMapping(new DbTypeMapping(typeof(DateTime), new DbType[1] { DbType.Date }));
		DbTypeMapping dbTypeMapping17 = AddDbTypeMapping(new DbTypeMapping(typeof(DateTime), new DbType[3]
		{
			DbType.DateTime,
			DbType.DateTime2,
			DbType.DateTimeOffset
		}));
		AddDbTypeMapping(new DbTypeMapping(typeof(DateTimeOffset), new DbType[1] { DbType.DateTimeOffset }));
		DbTypeMapping dbTypeMapping18 = AddDbTypeMapping(new DbTypeMapping(typeof(TimeSpan), new DbType[1] { DbType.Time }, (object o) => (!(o is string s)) ? Convert.ChangeType(o, typeof(TimeSpan), CultureInfo.InvariantCulture) : ((object)Utility.ParseTimeSpan(Encoding.UTF8.GetBytes(s)))));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DATETIME", dbTypeMapping17, MySqlDbType.DateTime, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DATE", dbTypeMapping16, MySqlDbType.Date, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("DATE", dbTypeMapping16, MySqlDbType.Newdate, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TIME", dbTypeMapping18, MySqlDbType.Time, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("TIMESTAMP", dbTypeMapping17, MySqlDbType.Timestamp, isUnsigned: false, binary: false, 0, null, null, 0L));
		AddColumnTypeMetadata(new ColumnTypeMetadata("YEAR", dbTypeMapping6, MySqlDbType.Year, isUnsigned: false, binary: false, 0, null, null, 0L));
		Func<object, object> convert = (object o) => Guid.Parse(Convert.ToString(o, CultureInfo.InvariantCulture));
		DbTypeMapping dbTypeMapping19 = AddDbTypeMapping(new DbTypeMapping(typeof(Guid), new DbType[1] { DbType.Guid }, convert));
		AddColumnTypeMetadata(new ColumnTypeMetadata("CHAR", dbTypeMapping19, MySqlDbType.Guid, isUnsigned: false, binary: false, 36, "CHAR(36)", "CHAR(36)", 0L));
		DbTypeMapping dbTypeMapping20 = AddDbTypeMapping(new DbTypeMapping(typeof(object), new DbType[1] { DbType.Object }));
		AddColumnTypeMetadata(new ColumnTypeMetadata("NULL", dbTypeMapping20, MySqlDbType.Null, isUnsigned: false, binary: false, 0, null, null, 0L));
	}

	public IReadOnlyList<ColumnTypeMetadata> GetColumnTypeMetadata()
	{
		return m_columnTypeMetadata.AsReadOnly();
	}

	public ColumnTypeMetadata GetColumnTypeMetadata(MySqlDbType mySqlDbType)
	{
		return m_mySqlDbTypeToColumnTypeMetadata[mySqlDbType];
	}

	public DbType GetDbTypeForMySqlDbType(MySqlDbType mySqlDbType)
	{
		return m_mySqlDbTypeToColumnTypeMetadata[mySqlDbType].DbTypeMapping.DbTypes[0];
	}

	public MySqlDbType GetMySqlDbTypeForDbType(DbType dbType)
	{
		foreach (KeyValuePair<MySqlDbType, ColumnTypeMetadata> mySqlDbTypeToColumnTypeMetadatum in m_mySqlDbTypeToColumnTypeMetadata)
		{
			if (Enumerable.Contains(mySqlDbTypeToColumnTypeMetadatum.Value.DbTypeMapping.DbTypes, dbType))
			{
				return mySqlDbTypeToColumnTypeMetadatum.Key;
			}
		}
		return MySqlDbType.VarChar;
	}

	private DbTypeMapping AddDbTypeMapping(DbTypeMapping dbTypeMapping)
	{
		m_dbTypeMappingsByClrType[dbTypeMapping.ClrType] = dbTypeMapping;
		if (dbTypeMapping.DbTypes != null)
		{
			DbType[] dbTypes = dbTypeMapping.DbTypes;
			foreach (DbType key in dbTypes)
			{
				m_dbTypeMappingsByDbType[key] = dbTypeMapping;
			}
		}
		return dbTypeMapping;
	}

	private void AddColumnTypeMetadata(ColumnTypeMetadata columnTypeMetadata)
	{
		m_columnTypeMetadata.Add(columnTypeMetadata);
		string key = columnTypeMetadata.CreateLookupKey();
		if (!m_columnTypeMetadataLookup.ContainsKey(key))
		{
			m_columnTypeMetadataLookup.Add(key, columnTypeMetadata);
		}
		if (!m_mySqlDbTypeToColumnTypeMetadata.ContainsKey(columnTypeMetadata.MySqlDbType))
		{
			m_mySqlDbTypeToColumnTypeMetadata.Add(columnTypeMetadata.MySqlDbType, columnTypeMetadata);
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal DbTypeMapping GetDbTypeMapping(Type clrType)
	{
		if (clrType.IsEnum)
		{
			clrType = Enum.GetUnderlyingType(clrType);
		}
		m_dbTypeMappingsByClrType.TryGetValue(clrType, out var value);
		return value;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	internal DbTypeMapping GetDbTypeMapping(DbType dbType)
	{
		m_dbTypeMappingsByDbType.TryGetValue(dbType, out var value);
		return value;
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public DbTypeMapping GetDbTypeMapping(string columnTypeName, bool unsigned = false, int length = 0)
	{
		return GetColumnTypeMetadata(columnTypeName, unsigned, length)?.DbTypeMapping;
	}

	public MySqlDbType GetMySqlDbType(string typeName, bool unsigned, int length)
	{
		return GetColumnTypeMetadata(typeName, unsigned, length).MySqlDbType;
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ColumnTypeMetadata GetColumnTypeMetadata(string columnTypeName, bool unsigned, int length)
	{
		if (!m_columnTypeMetadataLookup.TryGetValue(ColumnTypeMetadata.CreateLookupKey(columnTypeName, unsigned, length), out var value) && length != 0)
		{
			m_columnTypeMetadataLookup.TryGetValue(ColumnTypeMetadata.CreateLookupKey(columnTypeName, unsigned, 0), out value);
		}
		return value;
	}

	public static MySqlDbType ConvertToMySqlDbType(ColumnDefinitionPayload columnDefinition, bool treatTinyAsBoolean, MySqlGuidFormat guidFormat)
	{
		bool flag = (columnDefinition.ColumnFlags & ColumnFlags.Unsigned) != 0;
		switch (columnDefinition.ColumnType)
		{
		case ColumnType.Tiny:
			if (!treatTinyAsBoolean || columnDefinition.ColumnLength != 1 || flag)
			{
				if (!flag)
				{
					return MySqlDbType.Byte;
				}
				return MySqlDbType.UByte;
			}
			return MySqlDbType.Bool;
		case ColumnType.Int24:
			if (!flag)
			{
				return MySqlDbType.Int24;
			}
			return MySqlDbType.UInt24;
		case ColumnType.Long:
			if (!flag)
			{
				return MySqlDbType.Int32;
			}
			return MySqlDbType.UInt32;
		case ColumnType.Longlong:
			if (!flag)
			{
				return MySqlDbType.Int64;
			}
			return MySqlDbType.UInt64;
		case ColumnType.Bit:
			return MySqlDbType.Bit;
		case ColumnType.String:
			if (guidFormat == MySqlGuidFormat.Char36 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 36)
			{
				return MySqlDbType.Guid;
			}
			if (guidFormat == MySqlGuidFormat.Char32 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 32)
			{
				return MySqlDbType.Guid;
			}
			if ((columnDefinition.ColumnFlags & ColumnFlags.Enum) != 0)
			{
				return MySqlDbType.Enum;
			}
			if ((columnDefinition.ColumnFlags & ColumnFlags.Set) != 0)
			{
				return MySqlDbType.Set;
			}
			goto case ColumnType.VarChar;
		case ColumnType.VarChar:
		case ColumnType.TinyBlob:
		case ColumnType.MediumBlob:
		case ColumnType.LongBlob:
		case ColumnType.Blob:
		case ColumnType.VarString:
		{
			ColumnType columnType = columnDefinition.ColumnType;
			if (columnDefinition.CharacterSet == CharacterSet.Binary)
			{
				bool flag2 = (uint)(guidFormat - 4) <= 2u;
				if (flag2 && columnDefinition.ColumnLength == 16)
				{
					return MySqlDbType.Guid;
				}
				return columnType switch
				{
					ColumnType.String => MySqlDbType.Binary, 
					ColumnType.VarString => MySqlDbType.VarBinary, 
					ColumnType.TinyBlob => MySqlDbType.TinyBlob, 
					ColumnType.Blob => MySqlDbType.Blob, 
					ColumnType.MediumBlob => MySqlDbType.MediumBlob, 
					_ => MySqlDbType.LongBlob, 
				};
			}
			return columnType switch
			{
				ColumnType.String => MySqlDbType.String, 
				ColumnType.VarString => MySqlDbType.VarChar, 
				ColumnType.TinyBlob => MySqlDbType.TinyText, 
				ColumnType.Blob => MySqlDbType.Text, 
				ColumnType.MediumBlob => MySqlDbType.MediumText, 
				_ => MySqlDbType.LongText, 
			};
		}
		case ColumnType.Json:
			return MySqlDbType.JSON;
		case ColumnType.Short:
			if (!flag)
			{
				return MySqlDbType.Int16;
			}
			return MySqlDbType.UInt16;
		case ColumnType.Date:
		case ColumnType.NewDate:
			return MySqlDbType.Date;
		case ColumnType.DateTime:
			return MySqlDbType.DateTime;
		case ColumnType.Timestamp:
			return MySqlDbType.Timestamp;
		case ColumnType.Time:
			return MySqlDbType.Time;
		case ColumnType.Year:
			return MySqlDbType.Year;
		case ColumnType.Float:
			return MySqlDbType.Float;
		case ColumnType.Double:
			return MySqlDbType.Double;
		case ColumnType.Decimal:
			return MySqlDbType.Decimal;
		case ColumnType.NewDecimal:
			return MySqlDbType.NewDecimal;
		case ColumnType.Geometry:
			return MySqlDbType.Geometry;
		case ColumnType.Null:
			return MySqlDbType.Null;
		case ColumnType.Enum:
			return MySqlDbType.Enum;
		case ColumnType.Set:
			return MySqlDbType.Set;
		default:
			throw new NotImplementedException($"ConvertToMySqlDbType for {columnDefinition.ColumnType} is not implemented");
		}
	}

	public static ushort ConvertToColumnTypeAndFlags(MySqlDbType dbType, MySqlGuidFormat guidFormat)
	{
		bool flag = (((uint)(dbType - 501) <= 2u || (uint)(dbType - 508) <= 1u) ? true : false);
		bool flag2 = flag;
		ColumnType columnType;
		switch (dbType)
		{
		case MySqlDbType.Bool:
		case MySqlDbType.Byte:
		case MySqlDbType.UByte:
			columnType = ColumnType.Tiny;
			break;
		case MySqlDbType.Int16:
		case MySqlDbType.UInt16:
			columnType = ColumnType.Short;
			break;
		case MySqlDbType.Int24:
		case MySqlDbType.UInt24:
			columnType = ColumnType.Int24;
			break;
		case MySqlDbType.Int32:
		case MySqlDbType.UInt32:
			columnType = ColumnType.Long;
			break;
		case MySqlDbType.Int64:
		case MySqlDbType.UInt64:
			columnType = ColumnType.Longlong;
			break;
		case MySqlDbType.Bit:
			columnType = ColumnType.Bit;
			break;
		case MySqlDbType.Guid:
			flag = (uint)(guidFormat - 2) <= 1u;
			columnType = (flag ? ColumnType.String : ColumnType.Blob);
			break;
		case MySqlDbType.Enum:
		case MySqlDbType.Set:
			columnType = ColumnType.String;
			break;
		case MySqlDbType.String:
		case MySqlDbType.Binary:
			columnType = ColumnType.String;
			break;
		case MySqlDbType.VarString:
		case MySqlDbType.VarChar:
		case MySqlDbType.VarBinary:
			columnType = ColumnType.VarString;
			break;
		case MySqlDbType.TinyBlob:
		case MySqlDbType.TinyText:
			columnType = ColumnType.TinyBlob;
			break;
		case MySqlDbType.Blob:
		case MySqlDbType.Text:
			columnType = ColumnType.Blob;
			break;
		case MySqlDbType.MediumBlob:
		case MySqlDbType.MediumText:
			columnType = ColumnType.MediumBlob;
			break;
		case MySqlDbType.LongBlob:
		case MySqlDbType.LongText:
			columnType = ColumnType.LongBlob;
			break;
		case MySqlDbType.JSON:
			columnType = ColumnType.Json;
			break;
		case MySqlDbType.Date:
		case MySqlDbType.Newdate:
			columnType = ColumnType.Date;
			break;
		case MySqlDbType.DateTime:
			columnType = ColumnType.DateTime;
			break;
		case MySqlDbType.Timestamp:
			columnType = ColumnType.Timestamp;
			break;
		case MySqlDbType.Time:
			columnType = ColumnType.Time;
			break;
		case MySqlDbType.Year:
			columnType = ColumnType.Year;
			break;
		case MySqlDbType.Float:
			columnType = ColumnType.Float;
			break;
		case MySqlDbType.Double:
			columnType = ColumnType.Double;
			break;
		case MySqlDbType.Decimal:
			columnType = ColumnType.Decimal;
			break;
		case MySqlDbType.NewDecimal:
			columnType = ColumnType.NewDecimal;
			break;
		case MySqlDbType.Geometry:
			columnType = ColumnType.Geometry;
			break;
		case MySqlDbType.Null:
			columnType = ColumnType.Null;
			break;
		default:
			throw new NotImplementedException($"ConvertToColumnTypeAndFlags for {dbType} is not implemented");
		}
		return (ushort)((byte)columnType | (flag2 ? 32768 : 0));
	}

	internal IEnumerable<ColumnTypeMetadata> GetColumnMappings()
	{
		return m_columnTypeMetadataLookup.Values.AsEnumerable();
	}
}
