using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Core;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlCommandBuilder : DbCommandBuilder
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public new MySqlDataAdapter DataAdapter
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return (MySqlDataAdapter)base.DataAdapter;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set
		{
			base.DataAdapter = value;
		}
	}

	public static void DeriveParameters(MySqlCommand command)
	{
		DeriveParametersAsync(IOBehavior.Synchronous, command, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static Task DeriveParametersAsync(MySqlCommand command)
	{
		return DeriveParametersAsync(command?.Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, command, CancellationToken.None);
	}

	public static Task DeriveParametersAsync(MySqlCommand command, CancellationToken cancellationToken)
	{
		return DeriveParametersAsync(command?.Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, command, cancellationToken);
	}

	private static async Task DeriveParametersAsync(IOBehavior ioBehavior, MySqlCommand command, CancellationToken cancellationToken)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		if (command.CommandType != CommandType.StoredProcedure)
		{
			throw new ArgumentException($"MySqlCommand.CommandType must be StoredProcedure not {command.CommandType}", "command");
		}
		if (string.IsNullOrWhiteSpace(command.CommandText))
		{
			throw new ArgumentException("MySqlCommand.CommandText must be set to a stored procedure name", "command");
		}
		MySqlConnection connection = command.Connection;
		if (connection == null || connection.State != ConnectionState.Open)
		{
			throw new ArgumentException("MySqlCommand.Connection must be an open connection.", "command");
		}
		if (command.Connection.Session.ServerVersion.Version < ServerVersions.SupportsProcedureCache)
		{
			throw new NotSupportedException("MySQL Server " + command.Connection.Session.ServerVersion.OriginalString + " doesn't support INFORMATION_SCHEMA");
		}
		CachedProcedure cachedProcedure = await command.Connection.GetCachedProcedure(command.CommandText, revalidateMissing: true, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (cachedProcedure == null)
		{
			NormalizedSchema normalizedSchema = NormalizedSchema.MustNormalize(command.CommandText, command.Connection.Database);
			throw new MySqlException("Procedure or function '" + normalizedSchema.Component + "' cannot be found in database '" + normalizedSchema.Schema + "'.");
		}
		command.Parameters.Clear();
		foreach (CachedParameter parameter in cachedProcedure.Parameters)
		{
			MySqlParameter mySqlParameter = command.Parameters.Add("@" + parameter.Name, parameter.MySqlDbType);
			mySqlParameter.Direction = parameter.Direction;
			mySqlParameter.Size = parameter.Length;
		}
	}

	public MySqlCommandBuilder()
	{
		GC.SuppressFinalize(this);
		QuotePrefix = "`";
		QuoteSuffix = "`";
	}

	public MySqlCommandBuilder(MySqlDataAdapter dataAdapter)
		: this()
	{
		DataAdapter = dataAdapter;
	}

	public new MySqlCommand GetDeleteCommand()
	{
		return (MySqlCommand)base.GetDeleteCommand();
	}

	public new MySqlCommand GetInsertCommand()
	{
		return (MySqlCommand)base.GetInsertCommand();
	}

	public new MySqlCommand GetUpdateCommand()
	{
		return (MySqlCommand)base.GetUpdateCommand();
	}

	protected override void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
	{
		((MySqlParameter)parameter).MySqlDbType = (MySqlDbType)row[SchemaTableColumn.ProviderType];
	}

	protected override string GetParameterName(int parameterOrdinal)
	{
		return FormattableString.Invariant($"@p{parameterOrdinal}");
	}

	protected override string GetParameterName(string parameterName)
	{
		return "@" + parameterName;
	}

	protected override string GetParameterPlaceholder(int parameterOrdinal)
	{
		return GetParameterName(parameterOrdinal);
	}

	protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
	{
		if (!(adapter is MySqlDataAdapter mySqlDataAdapter))
		{
			throw new ArgumentException("adapter needs to be a MySqlDataAdapter", "adapter");
		}
		if (adapter == DataAdapter)
		{
			mySqlDataAdapter.RowUpdating -= RowUpdatingHandler;
		}
		else
		{
			mySqlDataAdapter.RowUpdating += RowUpdatingHandler;
		}
	}

	public override string QuoteIdentifier(string unquotedIdentifier)
	{
		return QuotePrefix + unquotedIdentifier.Replace("`", "``") + QuoteSuffix;
	}

	public override string UnquoteIdentifier(string quotedIdentifier)
	{
		if (quotedIdentifier != null)
		{
			int length = quotedIdentifier.Length;
			if (length >= 2 && quotedIdentifier[0] == '`' && quotedIdentifier[length - 1] == '`')
			{
				string text = quotedIdentifier;
				quotedIdentifier = text.Substring(1, text.Length - 1 - 1);
			}
		}
		return quotedIdentifier.Replace("``", "`");
	}

	private void RowUpdatingHandler(object sender, MySqlRowUpdatingEventArgs e)
	{
		RowUpdatingHandler(e);
	}
}
