using System;
using System.Runtime.CompilerServices;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.ColumnReaders;

internal abstract class ColumnReader
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public static ColumnReader Create(bool isBinary, ColumnDefinitionPayload columnDefinition, MySqlConnection connection)
	{
		bool flag = (columnDefinition.ColumnFlags & ColumnFlags.Unsigned) != 0;
		switch (columnDefinition.ColumnType)
		{
		case ColumnType.Tiny:
			if (connection.TreatTinyAsBoolean && columnDefinition.ColumnLength == 1 && !flag)
			{
				if (!isBinary)
				{
					return TextBooleanColumnReader.Instance;
				}
				return BinaryBooleanColumnReader.Instance;
			}
			if (!isBinary)
			{
				if (!flag)
				{
					return TextSignedInt8ColumnReader.Instance;
				}
				return TextUnsignedInt8ColumnReader.Instance;
			}
			if (!flag)
			{
				return BinarySignedInt8ColumnReader.Instance;
			}
			return BinaryUnsignedInt8ColumnReader.Instance;
		case ColumnType.Long:
		case ColumnType.Int24:
			if (!isBinary)
			{
				if (!flag)
				{
					return TextSignedInt32ColumnReader.Instance;
				}
				return TextUnsignedInt32ColumnReader.Instance;
			}
			if (!flag)
			{
				return BinarySignedInt32ColumnReader.Instance;
			}
			return BinaryUnsignedInt32ColumnReader.Instance;
		case ColumnType.Longlong:
			if (!isBinary)
			{
				if (!flag)
				{
					return TextSignedInt64ColumnReader.Instance;
				}
				return TextUnsignedInt64ColumnReader.Instance;
			}
			if (!flag)
			{
				return BinarySignedInt64ColumnReader.Instance;
			}
			return BinaryUnsignedInt64ColumnReader.Instance;
		case ColumnType.Bit:
			return BitColumnReader.Instance;
		case ColumnType.String:
			if (connection.GuidFormat == MySqlGuidFormat.Char36 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 36)
			{
				return GuidChar36ColumnReader.Instance;
			}
			if (connection.GuidFormat == MySqlGuidFormat.Char32 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 32)
			{
				return GuidChar32ColumnReader.Instance;
			}
			goto case ColumnType.VarChar;
		case ColumnType.VarChar:
		case ColumnType.Enum:
		case ColumnType.Set:
		case ColumnType.TinyBlob:
		case ColumnType.MediumBlob:
		case ColumnType.LongBlob:
		case ColumnType.Blob:
		case ColumnType.VarString:
			if (columnDefinition.CharacterSet != CharacterSet.Binary)
			{
				return StringColumnReader.Instance;
			}
			if (columnDefinition.ColumnLength == 16)
			{
				return connection.GuidFormat switch
				{
					MySqlGuidFormat.Binary16 => GuidBinary16ColumnReader.Instance, 
					MySqlGuidFormat.TimeSwapBinary16 => GuidTimeSwapBinary16ColumnReader.Instance, 
					MySqlGuidFormat.LittleEndianBinary16 => GuidLittleEndianBinary16ColumnReader.Instance, 
					_ => BytesColumnReader.Instance, 
				};
			}
			return BytesColumnReader.Instance;
		case ColumnType.Json:
			return StringColumnReader.Instance;
		case ColumnType.Short:
			if (!isBinary)
			{
				if (!flag)
				{
					return TextSignedInt16ColumnReader.Instance;
				}
				return TextUnsignedInt16ColumnReader.Instance;
			}
			if (!flag)
			{
				return BinarySignedInt16ColumnReader.Instance;
			}
			return BinaryUnsignedInt16ColumnReader.Instance;
		case ColumnType.Timestamp:
		case ColumnType.Date:
		case ColumnType.DateTime:
		case ColumnType.NewDate:
			if (!isBinary)
			{
				return new TextDateTimeColumnReader(connection);
			}
			return new BinaryDateTimeColumnReader(connection);
		case ColumnType.Time:
			if (!isBinary)
			{
				return TextTimeColumnReader.Instance;
			}
			return BinaryTimeColumnReader.Instance;
		case ColumnType.Year:
			if (!isBinary)
			{
				return TextSignedInt32ColumnReader.Instance;
			}
			return BinaryYearColumnReader.Instance;
		case ColumnType.Float:
			if (!isBinary)
			{
				return TextFloatColumnReader.Instance;
			}
			return BinaryFloatColumnReader.Instance;
		case ColumnType.Double:
			if (!isBinary)
			{
				return TextDoubleColumnReader.Instance;
			}
			return BinaryDoubleColumnReader.Instance;
		case ColumnType.Decimal:
		case ColumnType.NewDecimal:
			return DecimalColumnReader.Instance;
		case ColumnType.Geometry:
			return BytesColumnReader.Instance;
		case ColumnType.Null:
			return NullColumnReader.Instance;
		default:
			throw new NotImplementedException($"Reading {columnDefinition.ColumnType} not implemented");
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public abstract object ReadValue([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition);

	public virtual int? TryReadInt32(ReadOnlySpan<byte> data, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] ColumnDefinitionPayload columnDefinition)
	{
		return null;
	}
}
