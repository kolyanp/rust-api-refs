using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MySqlConnector.Core;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlBatchCommand : IMySqlCommand
{
	private MySqlParameterCollection m_parameterCollection;

	private long m_lastInsertedId;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public string CommandText
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		set;
	}

	public CommandType CommandType { get; set; }

	public int RecordsAffected => 0;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public MySqlParameterCollection Parameters
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return m_parameterCollection ?? (m_parameterCollection = new MySqlParameterCollection());
		}
	}

	public bool CanCreateParameter => true;

	bool IMySqlCommand.AllowUserVariables => false;

	CommandBehavior IMySqlCommand.CommandBehavior => Batch.CurrentCommandBehavior;

	MySqlParameterCollection IMySqlCommand.RawParameters => m_parameterCollection;

	MySqlAttributeCollection IMySqlCommand.RawAttributes => null;

	MySqlConnection IMySqlCommand.Connection => Batch?.Connection;

	long IMySqlCommand.LastInsertedId => m_lastInsertedId;

	MySqlParameterCollection IMySqlCommand.OutParameters { get; set; }

	MySqlParameter IMySqlCommand.ReturnParameter { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ICancellableCommand IMySqlCommand.CancellableCommand
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return Batch;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ILogger IMySqlCommand.Logger
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return Batch.Connection.LoggingConfiguration.CommandLogger;
		}
	}

	internal MySqlBatch Batch { get; set; }

	public MySqlBatchCommand()
		: this(null)
	{
	}

	public MySqlBatchCommand(string commandText)
	{
		CommandText = commandText ?? "";
		CommandType = CommandType.Text;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public DbParameter CreateParameter()
	{
		return new MySqlParameter();
	}

	PreparedStatements IMySqlCommand.TryGetPreparedStatements()
	{
		return null;
	}

	void IMySqlCommand.SetLastInsertedId(long lastInsertedId)
	{
		m_lastInsertedId = lastInsertedId;
	}
}
