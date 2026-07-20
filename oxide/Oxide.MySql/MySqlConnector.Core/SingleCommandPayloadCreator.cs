using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class SingleCommandPayloadCreator : ICommandPayloadCreator
{
	public static ICommandPayloadCreator Instance { get; } = new SingleCommandPayloadCreator();

	public static string OutParameterSentinelColumnName => "\ue001\b\v";

	public bool WriteQueryCommand(ref CommandListPosition commandListPosition, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })] IDictionary<string, CachedProcedure> cachedProcedures, ByteBufferWriter writer, bool appendSemicolon)
	{
		if (commandListPosition.CommandIndex == commandListPosition.CommandCount)
		{
			return false;
		}
		IMySqlCommand mySqlCommand = commandListPosition.CommandAt(commandListPosition.CommandIndex);
		commandListPosition.PreparedStatements = mySqlCommand.TryGetPreparedStatements();
		if (commandListPosition.PreparedStatements == null)
		{
			Log.PreparingCommandPayload(mySqlCommand.Logger, mySqlCommand.Connection.Session.Id, mySqlCommand.CommandText);
			writer.Write((byte)3);
			if (mySqlCommand.Connection.Session.SupportsQueryAttributes)
			{
				MySqlAttributeCollection rawAttributes = mySqlCommand.RawAttributes;
				writer.WriteLengthEncodedInteger((uint)(rawAttributes?.Count ?? 0));
				writer.Write((byte)1);
				if (rawAttributes != null && rawAttributes.Count > 0)
				{
					WriteBinaryParameters(writer, rawAttributes.Select([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (MySqlAttribute x) => x.ToParameter()).ToArray(), mySqlCommand, supportsQueryAttributes: true, 0);
				}
			}
			else
			{
				MySqlAttributeCollection rawAttributes2 = mySqlCommand.RawAttributes;
				if (rawAttributes2 != null && rawAttributes2.Count > 0)
				{
					Log.QueryAttributesNotSupported(mySqlCommand.Logger, mySqlCommand.Connection.Session.Id, mySqlCommand.CommandText);
				}
			}
			WriteQueryPayload(mySqlCommand, cachedProcedures, writer, appendSemicolon, isFirstCommand: true, isLastCommand: true);
			commandListPosition.LastUsedPreparedStatement = null;
			commandListPosition.CommandIndex++;
		}
		else
		{
			writer.Write((byte)23);
			commandListPosition.LastUsedPreparedStatement = commandListPosition.PreparedStatements.Statements[commandListPosition.PreparedStatementIndex];
			WritePreparedStatement(mySqlCommand, commandListPosition.LastUsedPreparedStatement, writer);
			if (++commandListPosition.PreparedStatementIndex == commandListPosition.PreparedStatements.Statements.Count)
			{
				commandListPosition.CommandIndex++;
				commandListPosition.PreparedStatementIndex = 0;
			}
		}
		return true;
	}

	public static bool WriteQueryPayload(IMySqlCommand command, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })] IDictionary<string, CachedProcedure> cachedProcedures, ByteBufferWriter writer, bool appendSemicolon, bool isFirstCommand, bool isLastCommand)
	{
		if (command.CommandType != CommandType.StoredProcedure)
		{
			return WriteCommand(command, writer, appendSemicolon, isFirstCommand, isLastCommand);
		}
		return WriteStoredProcedure(command, cachedProcedures, writer);
	}

	private static void WritePreparedStatement(IMySqlCommand command, PreparedStatement preparedStatement, ByteBufferWriter writer)
	{
		MySqlParameterCollection rawParameters = command.RawParameters;
		Log.PreparingCommandPayloadWithId(command.Logger, command.Connection.Session.Id, preparedStatement.StatementId, command.CommandText);
		MySqlAttributeCollection rawAttributes = command.RawAttributes;
		bool supportsQueryAttributes = command.Connection.Session.SupportsQueryAttributes;
		writer.Write(preparedStatement.StatementId);
		int num;
		if (supportsQueryAttributes)
		{
			Version version = command.Connection.Session.ServerVersion.Version;
			if ((object)version != null && version.Major == 8 && version.Minor == 0)
			{
				int build = version.Build;
				if (build >= 23)
				{
					num = ((build <= 25) ? 1 : 0);
					goto IL_009e;
				}
			}
			num = 0;
			goto IL_009e;
		}
		int num2 = 0;
		goto IL_00a4;
		IL_00a4:
		bool flag = (byte)num2 != 0;
		writer.Write((byte)(flag ? 8u : 0u));
		writer.Write(1);
		int num3 = preparedStatement.Statement.ParameterNames?.Count ?? 0;
		int num4 = rawAttributes?.Count ?? 0;
		if (flag)
		{
			writer.WriteLengthEncodedInteger((uint)(num3 + num4));
		}
		else
		{
			if (supportsQueryAttributes && num3 > 0)
			{
				writer.WriteLengthEncodedInteger((uint)num3);
			}
			if (num4 > 0)
			{
				Log.QueryAttributesNotSupportedWithId(command.Logger, command.Connection.Session.Id, preparedStatement.StatementId);
				num4 = 0;
			}
		}
		if (num3 <= 0 && num4 <= 0)
		{
			return;
		}
		MySqlParameter[] array = new MySqlParameter[num3 + num4];
		for (int i = 0; i < num3; i++)
		{
			string text = preparedStatement.Statement.NormalizedParameterNames[i];
			int num5 = ((text == null) ? preparedStatement.Statement.ParameterIndexes[i] : (rawParameters?.UnsafeIndexOf(text) ?? (-1)));
			if (num5 == -1 && text != null)
			{
				throw new MySqlException("Parameter '" + preparedStatement.Statement.ParameterNames[i] + "' must be defined.");
			}
			if (num5 < 0 || num5 >= (rawParameters?.Count ?? 0))
			{
				throw new MySqlException(string.Format("Parameter index {0} is invalid when only {1} parameter{2} defined.", num5, rawParameters?.Count ?? 0, (rawParameters != null && rawParameters.Count == 1) ? " is" : "s are"));
			}
			array[i] = rawParameters[num5];
		}
		for (int j = 0; j < num4; j++)
		{
			array[num3 + j] = rawAttributes[j].ToParameter();
		}
		WriteBinaryParameters(writer, array, command, supportsQueryAttributes, num3);
		return;
		IL_009e:
		num2 = ((num == 0) ? 1 : 0);
		goto IL_00a4;
	}

	private static void WriteBinaryParameters(ByteBufferWriter writer, MySqlParameter[] parameters, IMySqlCommand command, bool supportsQueryAttributes, int parameterCount)
	{
		byte b = 0;
		for (int i = 0; i < parameters.Length; i++)
		{
			MySqlParameter mySqlParameter = parameters[i];
			if (mySqlParameter.Value == null || mySqlParameter.Value == DBNull.Value)
			{
				b |= (byte)(1 << i % 8);
			}
			if (i % 8 == 7)
			{
				writer.Write(b);
				b = 0;
			}
		}
		if (parameters.Length % 8 != 0)
		{
			writer.Write(b);
		}
		writer.Write((byte)1);
		for (int j = 0; j < parameters.Length; j++)
		{
			MySqlParameter mySqlParameter2 = parameters[j];
			MySqlDbType dbType = mySqlParameter2.MySqlDbType;
			DbTypeMapping dbTypeMapping = ((mySqlParameter2.Value == null || mySqlParameter2.Value == DBNull.Value) ? null : TypeMapper.Instance.GetDbTypeMapping(mySqlParameter2.Value.GetType()));
			if (dbTypeMapping != null)
			{
				DbType dbType2 = dbTypeMapping.DbTypes[0];
				dbType = TypeMapper.Instance.GetMySqlDbTypeForDbType(dbType2);
			}
			writer.Write(TypeMapper.ConvertToColumnTypeAndFlags(dbType, command.Connection.GuidFormat));
			if (supportsQueryAttributes)
			{
				if (j < parameterCount)
				{
					writer.Write((byte)0);
				}
				else
				{
					writer.WriteLengthEncodedString(mySqlParameter2.ParameterName);
				}
			}
		}
		StatementPreparerOptions options = command.CreateStatementPreparerOptions();
		for (int k = 0; k < parameters.Length; k++)
		{
			parameters[k].AppendBinary(writer, options);
		}
	}

	private static bool WriteStoredProcedure(IMySqlCommand command, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })] IDictionary<string, CachedProcedure> cachedProcedures, ByteBufferWriter writer)
	{
		MySqlParameterCollection mySqlParameterCollection = command.RawParameters;
		CachedProcedure cachedProcedure = cachedProcedures[command.CommandText];
		if (cachedProcedure != null)
		{
			mySqlParameterCollection = cachedProcedure.AlignParamsWithDb(mySqlParameterCollection);
		}
		MySqlParameter mySqlParameter = null;
		MySqlParameterCollection mySqlParameterCollection2 = new MySqlParameterCollection();
		List<string> list = new List<string>();
		MySqlParameterCollection mySqlParameterCollection3 = new MySqlParameterCollection();
		List<string> list2 = new List<string>();
		string text = "";
		for (int i = 0; i < (mySqlParameterCollection?.Count ?? 0); i++)
		{
			MySqlParameter mySqlParameter2 = mySqlParameterCollection[i];
			string text2 = "@inParam" + i;
			string text3 = "@outParam" + i;
			switch (mySqlParameter2.Direction)
			{
			case ParameterDirection.Input:
			case ParameterDirection.InputOutput:
			{
				MySqlParameter parameter = mySqlParameter2.WithParameterName(text2);
				mySqlParameterCollection3.Add(parameter);
				if (mySqlParameter2.Direction == ParameterDirection.InputOutput)
				{
					text = text + "SET " + text3 + "=" + text2 + "; ";
					goto case ParameterDirection.Output;
				}
				list2.Add(text2);
				break;
			}
			case ParameterDirection.Output:
				mySqlParameterCollection2.Add(mySqlParameter2);
				list.Add(text3);
				list2.Add(text3);
				break;
			case ParameterDirection.ReturnValue:
				mySqlParameter = mySqlParameter2;
				break;
			}
		}
		string text4 = command.CommandText + "(" + string.Join(", ", list2) + ");";
		if (mySqlParameter == null)
		{
			text4 = text + "CALL " + text4;
			if (mySqlParameterCollection2.Count > 0 && (command.CommandBehavior & CommandBehavior.SchemaOnly) == 0)
			{
				text4 = text4 + "SELECT '" + OutParameterSentinelColumnName + "' AS '" + OutParameterSentinelColumnName + "', " + string.Join(", ", list);
			}
		}
		else
		{
			text4 = "SELECT " + text4;
		}
		command.OutParameters = mySqlParameterCollection2;
		command.ReturnParameter = mySqlParameter;
		return new StatementPreparer(text4, mySqlParameterCollection3, command.CreateStatementPreparerOptions()).ParseAndBindParameters(writer);
	}

	private static bool WriteCommand(IMySqlCommand command, ByteBufferWriter writer, bool appendSemicolon, bool isFirstCommand, bool isLastCommand)
	{
		bool flag = (command.CommandBehavior & CommandBehavior.SchemaOnly) != 0;
		bool flag2 = (command.CommandBehavior & CommandBehavior.SingleRow) != 0;
		if ((flag || flag2) && isFirstCommand)
		{
			ReadOnlySpan<byte> span = ((!command.Connection.SupportsPerQueryVariables) ? (flag2 ? "SET sql_select_limit=1;\n"u8 : "SET sql_select_limit=0;\n"u8) : (flag2 ? "SET STATEMENT sql_select_limit=1 FOR "u8 : "SET STATEMENT sql_select_limit=0 FOR "u8));
			writer.Write(span);
		}
		bool flag3 = new StatementPreparer(command.CommandText, command.RawParameters, (StatementPreparerOptions)((int)command.CreateStatementPreparerOptions() | ((appendSemicolon || flag || flag2) ? 512 : 0))).ParseAndBindParameters(writer);
		if ((flag || flag2) && isLastCommand && flag3 && !command.Connection.SupportsPerQueryVariables)
		{
			writer.Write("\nSET sql_select_limit=default;"u8);
		}
		return flag3;
	}
}
