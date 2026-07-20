using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class CachedProcedure
{
	private static readonly IReadOnlyDictionary<string, string> s_typeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "BOOL", "TINYINT" },
		{ "BOOLEAN", "TINYINT" },
		{ "INTEGER", "INT" },
		{ "NUMERIC", "DECIMAL" },
		{ "FIXED", "DECIMAL" },
		{ "REAL", "DOUBLE" },
		{ "DOUBLE PRECISION", "DOUBLE" },
		{ "NVARCHAR", "VARCHAR" },
		{ "CHARACTER VARYING", "VARCHAR" },
		{ "NATIONAL VARCHAR", "VARCHAR" },
		{ "NCHAR", "CHAR" },
		{ "CHARACTER", "CHAR" },
		{ "NATIONAL CHAR", "CHAR" },
		{ "CHAR BYTE", "BINARY" }
	};

	private static readonly Regex s_cStyleComments = new Regex("/\\*.*?\\*/", RegexOptions.Singleline);

	private static readonly Regex s_singleLineComments = new Regex("(^|\\s)--.*?$", RegexOptions.Multiline);

	private static readonly Regex s_multipleSpaces = new Regex("\\s+");

	private static readonly Regex s_numericTypes = new Regex("(DECIMAL|DEC|FIXED|NUMERIC|FLOAT|DOUBLE PRECISION|DOUBLE|REAL)\\s*\\([0-9]+(,\\s*[0-9]+)\\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex s_enum = new Regex("ENUM\\s*\\([^)]+\\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex s_parameterName = new Regex("^(?:`((?:[\\u0001-\\u005F\\u0061-\\uFFFF]+|``)+)`|([A-Za-z0-9$_\\u0080-\\uFFFF]+)) (.*)$");

	private static readonly Regex s_characterSet = new Regex(" (CHARSET|CHARACTER SET) [A-Za-z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex s_collate = new Regex(" (COLLATE) [A-Za-z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex s_length = new Regex("\\s*\\(\\s*([0-9]+)\\s*(?:,\\s*[0-9]+\\s*)?\\)");

	private readonly string m_schema;

	private readonly string m_component;

	public IReadOnlyList<CachedParameter> Parameters { get; }

	private string FullyQualified => "`" + m_schema + "`.`" + m_component + "`";

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public static async Task<CachedProcedure> FillAsync(IOBehavior ioBehavior, MySqlConnection connection, string schema, string component, ILogger logger, CancellationToken cancellationToken)
	{
		if (!connection.Session.ServerVersion.IsMariaDb && connection.Session.ServerVersion.Version < ServerVersions.RemovesMySqlProcTable && !connection.Session.ProcAccessDenied)
		{
			try
			{
				using MySqlCommand cmd = connection.CreateCommand();
				cmd.Transaction = connection.CurrentTransaction;
				cmd.CommandText = "SELECT param_list, returns FROM mysql.proc WHERE db = @schema AND name = @component";
				cmd.Parameters.AddWithValue("@schema", schema);
				cmd.Parameters.AddWithValue("@component", component);
				using MySqlDataReader reader = await cmd.ExecuteReaderNoResetTimeoutAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!(await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					return null;
				}
				byte[] bytes = (byte[])reader.GetValue(0);
				string parametersSql = Encoding.UTF8.GetString(bytes);
				object value = reader.GetValue(1);
				string text2 = default(string);
				if (!(value is string text))
				{
					if (value != null)
					{
						text2 = Encoding.UTF8.GetString((byte[])value);
					}
					else
					{
						_003C54935a9f_002D04ec_002D42f0_002Db2db_002Dde3406f234de_003E_003CPrivateImplementationDetails_003E.ThrowInvalidOperationException();
					}
				}
				else
				{
					text2 = text;
				}
				string text3 = text2;
				List<CachedParameter> list = ParseParameters(parametersSql);
				if (text3.Length != 0)
				{
					bool unsigned;
					int length;
					string dataType = ParseDataType(text3, out unsigned, out length);
					list.Insert(0, CreateCachedParameter(0, null, "", dataType, unsigned, length, text3));
				}
				return new CachedProcedure(schema, component, list);
			}
			catch (MySqlException ex)
			{
				Log.FailedToRetrieveProcedureMetadata(logger, ex, connection.Session.Id, schema, component, ex.Message);
				if (ex.ErrorCode == MySqlErrorCode.TableAccessDenied)
				{
					connection.Session.ProcAccessDenied = true;
				}
			}
		}
		if (connection.Session.ServerVersion.Version < ServerVersions.SupportsProcedureCache)
		{
			Log.ServerDoesNotSupportCachedProcedures(logger, connection.Session.Id, connection.Session.ServerVersion.OriginalString);
			return null;
		}
		List<CachedParameter> parameters = new List<CachedParameter>();
		int routineCount;
		using (MySqlCommand cmd = connection.CreateCommand())
		{
			cmd.Transaction = connection.CurrentTransaction;
			cmd.CommandText = "SELECT COUNT(*)\n\t\t\t\tFROM information_schema.routines\n\t\t\t\tWHERE ROUTINE_SCHEMA = @schema AND ROUTINE_NAME = @component;\n\t\t\t\tSELECT ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME, DTD_IDENTIFIER\n\t\t\t\tFROM information_schema.parameters\n\t\t\t\tWHERE SPECIFIC_SCHEMA = @schema AND SPECIFIC_NAME = @component\n\t\t\t\tORDER BY ORDINAL_POSITION";
			cmd.Parameters.AddWithValue("@schema", schema);
			cmd.Parameters.AddWithValue("@component", component);
			using MySqlDataReader reader = await cmd.ExecuteReaderNoResetTimeoutAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			routineCount = reader.GetInt32(0);
			await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			while (await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				bool unsigned2;
				int length2;
				string dataType2 = ParseDataType(reader.GetString(3), out unsigned2, out length2);
				parameters.Add(new CachedParameter(reader.GetInt32(0), (!reader.IsDBNull(1)) ? reader.GetString(1) : null, (!reader.IsDBNull(2)) ? reader.GetString(2) : "", dataType2, unsigned2, length2));
			}
		}
		Log.ProcedureHasRoutineCount(logger, schema, component, routineCount, parameters.Count);
		return (routineCount == 0) ? null : new CachedProcedure(schema, component, parameters);
	}

	private CachedProcedure(string schema, string component, IReadOnlyList<CachedParameter> parameters)
	{
		m_schema = schema;
		m_component = component;
		Parameters = parameters;
	}

	internal MySqlParameterCollection AlignParamsWithDb([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] MySqlParameterCollection parameterCollection)
	{
		MySqlParameterCollection mySqlParameterCollection = new MySqlParameterCollection();
		MySqlParameter mySqlParameter = parameterCollection?.FirstOrDefault([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (MySqlParameter x) => x.Direction == ParameterDirection.ReturnValue);
		foreach (CachedParameter parameter in Parameters)
		{
			MySqlParameter mySqlParameter2;
			if (parameter.Direction == ParameterDirection.ReturnValue)
			{
				mySqlParameter2 = mySqlParameter ?? throw new InvalidOperationException("Attempt to call stored function " + FullyQualified + " without specifying a return parameter");
			}
			else
			{
				int num = parameterCollection?.NormalizedIndexOf(parameter.Name) ?? (-1);
				if (num < 0)
				{
					throw new ArgumentException("Parameter '" + parameter.Name + "' not found in the collection.");
				}
				mySqlParameter2 = parameterCollection[num];
			}
			if (!mySqlParameter2.HasSetDirection)
			{
				mySqlParameter2.Direction = parameter.Direction;
			}
			if (!mySqlParameter2.HasSetDbType)
			{
				mySqlParameter2.MySqlDbType = parameter.MySqlDbType;
			}
			mySqlParameterCollection.Add(mySqlParameter2);
		}
		return mySqlParameterCollection;
	}

	internal static List<CachedParameter> ParseParameters(string parametersSql)
	{
		parametersSql = s_cStyleComments.Replace(parametersSql, "");
		parametersSql = s_singleLineComments.Replace(parametersSql, "");
		parametersSql = s_multipleSpaces.Replace(parametersSql, " ");
		if (string.IsNullOrWhiteSpace(parametersSql))
		{
			return new List<CachedParameter>();
		}
		parametersSql = s_numericTypes.Replace(parametersSql, "$1");
		parametersSql = s_enum.Replace(parametersSql, "ENUM");
		string[] array = parametersSql.Split(new char[1] { ',' });
		List<CachedParameter> list = new List<CachedParameter>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			string originalSql = text;
			string direction = "IN";
			if (text.StartsWith("INOUT ", StringComparison.OrdinalIgnoreCase))
			{
				direction = "INOUT";
				string text2 = text;
				text = text2.Substring(6, text2.Length - 6);
			}
			else if (text.StartsWith("OUT ", StringComparison.OrdinalIgnoreCase))
			{
				direction = "OUT";
				string text2 = text;
				text = text2.Substring(4, text2.Length - 4);
			}
			else if (text.StartsWith("IN ", StringComparison.OrdinalIgnoreCase))
			{
				direction = "IN";
				string text2 = text;
				text = text2.Substring(3, text2.Length - 3);
			}
			Match match = s_parameterName.Match(text);
			string name = (match.Groups[1].Success ? match.Groups[1].Value.Replace("``", "`") : match.Groups[2].Value);
			bool unsigned;
			int length;
			string dataType = ParseDataType(match.Groups[3].Value, out unsigned, out length);
			list.Add(CreateCachedParameter(i + 1, direction, name, dataType, unsigned, length, originalSql));
		}
		return list;
	}

	internal static string ParseDataType(string sql, out bool unsigned, out int length)
	{
		sql = s_characterSet.Replace(sql, "");
		sql = s_collate.Replace(sql, "");
		sql = s_enum.Replace(sql, "ENUM");
		length = 0;
		Match match = s_length.Match(sql);
		if (match.Success)
		{
			length = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
			sql = s_length.Replace(sql, "");
		}
		string[] array = sql.Trim().Split(new char[1] { ' ' });
		if ((array.Length < 2 || !s_typeMapping.TryGetValue(array[0] + " " + array[1], out var value)) && s_typeMapping.TryGetValue(array[0], out value) && array[0].StartsWith("BOOL", StringComparison.OrdinalIgnoreCase))
		{
			length = 1;
		}
		unsigned = Enumerable.Contains<string>(array, "UNSIGNED", StringComparer.OrdinalIgnoreCase);
		return value ?? array[0];
	}

	private static CachedParameter CreateCachedParameter(int ordinal, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string direction, string name, string dataType, bool unsigned, int length, string originalSql)
	{
		try
		{
			return new CachedParameter(ordinal, direction, name, dataType, unsigned, length);
		}
		catch (NullReferenceException innerException)
		{
			throw new MySqlException("Failed to parse stored procedure parameter '" + originalSql + "'; extracted data type was " + dataType, innerException);
		}
	}
}
