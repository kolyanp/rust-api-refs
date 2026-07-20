using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Core;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlCommand : DbCommand, IMySqlCommand, ICancellableCommand, ICloneable
{
	private readonly int m_commandId;

	private bool m_isDisposed;

	private MySqlConnection m_connection;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	private string m_commandText;

	private MySqlParameterCollection m_parameterCollection;

	private MySqlAttributeCollection m_attributeCollection;

	private int? m_commandTimeout;

	private CommandType m_commandType;

	private CommandBehavior m_commandBehavior;

	private Action m_cancelAction;

	private Action m_cancelForCommandTimeoutAction;

	private uint m_cancelTimerId;

	private bool m_commandTimedOut;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public new MySqlParameterCollection Parameters
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return m_parameterCollection ?? (m_parameterCollection = new MySqlParameterCollection());
		}
	}

	MySqlParameterCollection IMySqlCommand.RawParameters => m_parameterCollection;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public MySqlAttributeCollection Attributes
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return m_attributeCollection ?? (m_attributeCollection = new MySqlAttributeCollection());
		}
	}

	MySqlAttributeCollection IMySqlCommand.RawAttributes => m_attributeCollection;

	bool IMySqlCommand.AllowUserVariables => AllowUserVariables;

	internal bool AllowUserVariables { get; set; }

	internal bool NoActivity { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public override string CommandText
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return m_commandText;
		}
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			if (m_connection?.ActiveCommandId == m_commandId)
			{
				throw new InvalidOperationException("Cannot set MySqlCommand.CommandText when there is an open DataReader for this command; it must be closed first.");
			}
			m_commandText = value ?? "";
		}
	}

	public bool IsPrepared => ((IMySqlCommand)this).TryGetPreparedStatements() != null;

	public new MySqlTransaction Transaction { get; set; }

	public new MySqlConnection Connection
	{
		get
		{
			return m_connection;
		}
		set
		{
			if (m_connection?.ActiveCommandId == m_commandId)
			{
				throw new InvalidOperationException("Cannot set MySqlCommand.Connection when there is an open DataReader for this command; it must be closed first.");
			}
			m_connection = value;
		}
	}

	public override int CommandTimeout
	{
		get
		{
			return Math.Min(m_commandTimeout ?? Connection?.DefaultCommandTimeout ?? 0, 2147483);
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "CommandTimeout must be greater than or equal to zero.");
			}
			m_commandTimeout = value;
			((ICancellableCommand)this).EffectiveCommandTimeout = null;
		}
	}

	public override CommandType CommandType
	{
		get
		{
			return m_commandType;
		}
		set
		{
			if (value != CommandType.Text && value != CommandType.StoredProcedure)
			{
				throw new ArgumentException("CommandType must be Text or StoredProcedure.", "value");
			}
			m_commandType = value;
		}
	}

	public override bool DesignTimeVisible { get; set; }

	public override UpdateRowSource UpdatedRowSource { get; set; }

	public long LastInsertedId { get; private set; }

	protected override DbConnection DbConnection
	{
		get
		{
			return Connection;
		}
		set
		{
			Connection = (MySqlConnection)value;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	protected override DbParameterCollection DbParameterCollection
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return Parameters;
		}
	}

	protected override DbTransaction DbTransaction
	{
		get
		{
			return Transaction;
		}
		set
		{
			Transaction = (MySqlTransaction)value;
		}
	}

	bool ICancellableCommand.IsTimedOut => Volatile.Read(in m_commandTimedOut);

	int ICancellableCommand.CommandId => m_commandId;

	int? ICancellableCommand.EffectiveCommandTimeout { get; set; }

	int ICancellableCommand.CancelAttemptCount { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ICancellableCommand IMySqlCommand.CancellableCommand
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return this;
		}
	}

	private IOBehavior AsyncIOBehavior => Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous;

	CommandBehavior IMySqlCommand.CommandBehavior => m_commandBehavior;

	MySqlParameterCollection IMySqlCommand.OutParameters { get; set; }

	MySqlParameter IMySqlCommand.ReturnParameter { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ILogger IMySqlCommand.Logger
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get
		{
			return Connection.LoggingConfiguration.CommandLogger;
		}
	}

	public MySqlCommand()
		: this(null, null, null)
	{
	}

	public MySqlCommand(string commandText)
		: this(commandText, null, null)
	{
	}

	public MySqlCommand(MySqlConnection connection, MySqlTransaction transaction)
		: this(null, connection, transaction)
	{
	}

	public MySqlCommand(string commandText, MySqlConnection connection)
		: this(commandText, connection, null)
	{
	}

	public MySqlCommand(string commandText, MySqlConnection connection, MySqlTransaction transaction)
	{
		GC.SuppressFinalize(this);
		m_commandId = ICancellableCommandExtensions.GetNextId();
		m_commandText = commandText ?? "";
		Connection = connection;
		Transaction = transaction;
		CommandType = CommandType.Text;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private MySqlCommand(MySqlCommand other)
		: this(other.CommandText, other.Connection, other.Transaction)
	{
		GC.SuppressFinalize(this);
		m_commandTimeout = other.m_commandTimeout;
		((ICancellableCommand)this).EffectiveCommandTimeout = null;
		m_commandType = other.m_commandType;
		DesignTimeVisible = other.DesignTimeVisible;
		UpdatedRowSource = other.UpdatedRowSource;
		m_parameterCollection = other.CloneRawParameters();
		m_attributeCollection = other.CloneRawAttributes();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public new MySqlParameter CreateParameter()
	{
		return (MySqlParameter)base.CreateParameter();
	}

	public override void Cancel()
	{
		Connection?.Cancel(this, m_commandId, isCancel: true);
	}

	public override int ExecuteNonQuery()
	{
		return ExecuteNonQueryAsync(IOBehavior.Synchronous, CancellationToken.None).Result;
	}

	public override object ExecuteScalar()
	{
		return ExecuteScalarAsync(IOBehavior.Synchronous, CancellationToken.None).Result;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public new MySqlDataReader ExecuteReader()
	{
		return ExecuteReaderAsync(CommandBehavior.Default, IOBehavior.Synchronous, default(CancellationToken)).Result;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public new MySqlDataReader ExecuteReader(CommandBehavior commandBehavior)
	{
		return ExecuteReaderAsync(commandBehavior, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public override void Prepare()
	{
		if (!NeedsPrepare(out var exception))
		{
			if (exception != null)
			{
				throw exception;
			}
		}
		else
		{
			Connection.Session.PrepareAsync(this, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return PrepareAsync(AsyncIOBehavior, cancellationToken);
	}

	internal MySqlParameterCollection CloneRawParameters()
	{
		if (m_parameterCollection == null)
		{
			return null;
		}
		MySqlParameterCollection mySqlParameterCollection = new MySqlParameterCollection();
		foreach (MySqlParameter item in (IEnumerable<MySqlParameter>)m_parameterCollection)
		{
			mySqlParameterCollection.Add(item.Clone());
		}
		return mySqlParameterCollection;
	}

	private MySqlAttributeCollection CloneRawAttributes()
	{
		if (m_attributeCollection == null)
		{
			return null;
		}
		MySqlAttributeCollection mySqlAttributeCollection = new MySqlAttributeCollection();
		foreach (MySqlAttribute item in m_attributeCollection)
		{
			mySqlAttributeCollection.Add(new MySqlAttribute(item.AttributeName, item.Value));
		}
		return mySqlAttributeCollection;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private Task PrepareAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (!NeedsPrepare(out var exception))
		{
			if (exception != null)
			{
				return Task.FromException(exception);
			}
			return Task.CompletedTask;
		}
		return Connection.Session.PrepareAsync(this, ioBehavior, cancellationToken);
	}

	private bool NeedsPrepare(out Exception exception)
	{
		exception = null;
		if (Connection == null)
		{
			exception = new InvalidOperationException("Connection property must be non-null.");
		}
		else if (Connection.State != ConnectionState.Open)
		{
			exception = new InvalidOperationException($"Connection must be Open; current state is {Connection.State}");
		}
		else if (string.IsNullOrWhiteSpace(CommandText))
		{
			exception = new InvalidOperationException("CommandText must be specified");
		}
		else if (Connection?.HasActiveReader ?? false)
		{
			exception = new InvalidOperationException("Cannot call Prepare when there is an open DataReader for this command's connection; it must be closed first.");
		}
		if (exception != null || Connection.IgnorePrepare)
		{
			return false;
		}
		CommandType commandType = CommandType;
		if (commandType != CommandType.StoredProcedure && commandType != CommandType.Text)
		{
			exception = new NotSupportedException("Only CommandType.Text and CommandType.StoredProcedure are currently supported by MySqlCommand.Prepare.");
			return false;
		}
		return Connection.Session.TryGetPreparedStatement(CommandText) == null;
	}

	void IMySqlCommand.SetLastInsertedId(long lastInsertedId)
	{
		LastInsertedId = lastInsertedId;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	protected override DbParameter CreateDbParameter()
	{
		return new MySqlParameter();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		return ExecuteReaderAsync(behavior, IOBehavior.Synchronous, CancellationToken.None).Result;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
	{
		return ExecuteNonQueryAsync(AsyncIOBehavior, cancellationToken).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	internal async ValueTask<int> ExecuteNonQueryAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Volatile.Write(ref m_commandTimedOut, value: false);
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			using MySqlDataReader reader = await ExecuteReaderNoResetTimeoutAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			while (await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) || await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
			}
			return reader.RecordsAffected;
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
	{
		return ExecuteScalarAsync(AsyncIOBehavior, cancellationToken).AsTask();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 2 })]
	internal async ValueTask<object> ExecuteScalarAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Volatile.Write(ref m_commandTimedOut, value: false);
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			bool hasSetResult = false;
			object result = null;
			using MySqlDataReader reader = await ExecuteReaderNoResetTimeoutAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			do
			{
				bool flag = await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!hasSetResult)
				{
					if (flag)
					{
						result = reader.GetValue(0);
					}
					hasSetResult = true;
				}
			}
			while (await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			return result;
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public new Task<MySqlDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteReaderAsync(CommandBehavior.Default, AsyncIOBehavior, cancellationToken).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public new Task<MySqlDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteReaderAsync(behavior, AsyncIOBehavior, cancellationToken).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		return await ExecuteReaderAsync(behavior, AsyncIOBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	internal async ValueTask<MySqlDataReader> ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Volatile.Write(ref m_commandTimedOut, value: false);
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			return await ExecuteReaderNoResetTimeoutAsync(behavior, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	internal ValueTask<MySqlDataReader> ExecuteReaderNoResetTimeoutAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (!IsValid(out var exception))
		{
			return ValueTaskExtensions.FromException<MySqlDataReader>(exception);
		}
		Activity activity = (NoActivity ? null : Connection.Session.StartActivity("Execute", "db.statement", CommandText));
		m_commandBehavior = behavior;
		return CommandExecutor.ExecuteReaderAsync(new CommandListPosition(this), SingleCommandPayloadCreator.Instance, behavior, activity, ioBehavior, cancellationToken);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public MySqlCommand Clone()
	{
		return new MySqlCommand(this);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	object ICloneable.Clone()
	{
		return Clone();
	}

	protected override void Dispose(bool disposing)
	{
		m_isDisposed = true;
		base.Dispose(disposing);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task DisposeAsync()
	{
		Dispose();
		return Task.CompletedTask;
	}

	CancellationTokenRegistration ICancellableCommand.RegisterCancel(CancellationToken cancellationToken)
	{
		if (!cancellationToken.CanBeCanceled)
		{
			return default(CancellationTokenRegistration);
		}
		if (m_cancelAction == null)
		{
			m_cancelAction = Cancel;
		}
		return cancellationToken.Register(m_cancelAction);
	}

	void ICancellableCommand.SetTimeout(int milliseconds)
	{
		if (m_cancelTimerId != 0)
		{
			TimerQueue.Instance.Remove(m_cancelTimerId);
		}
		if (milliseconds != int.MaxValue)
		{
			if (m_cancelForCommandTimeoutAction == null)
			{
				m_cancelForCommandTimeoutAction = CancelCommandForTimeout;
			}
			m_cancelTimerId = TimerQueue.Instance.Add(milliseconds, m_cancelForCommandTimeoutAction);
		}
	}

	private void CancelCommandForTimeout()
	{
		Volatile.Write(ref m_commandTimedOut, value: true);
		Connection?.Cancel(this, m_commandId, isCancel: false);
	}

	private bool IsValid([_003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhen(false)] out Exception exception)
	{
		exception = null;
		if (m_isDisposed)
		{
			exception = new ObjectDisposedException(GetType().Name);
		}
		else if (Connection == null)
		{
			exception = new InvalidOperationException("Connection property must be non-null.");
		}
		else
		{
			ConnectionState state = Connection.State;
			if (state != ConnectionState.Open && state != ConnectionState.Connecting)
			{
				exception = new InvalidOperationException($"Connection must be Open; current state is {Connection.State}");
			}
			else if (!Connection.IgnoreCommandTransaction && Transaction != Connection.CurrentTransaction)
			{
				exception = new InvalidOperationException("The transaction associated with this command is not the connection's active transaction; see https://fl.vu/mysql-trans");
			}
			else if (string.IsNullOrWhiteSpace(CommandText))
			{
				exception = new InvalidOperationException("CommandText must be specified");
			}
		}
		return exception == null;
	}

	PreparedStatements IMySqlCommand.TryGetPreparedStatements()
	{
		if (CommandType != CommandType.Text || string.IsNullOrWhiteSpace(CommandText) || m_connection == null || m_connection.State != ConnectionState.Open)
		{
			return null;
		}
		return m_connection.Session.TryGetPreparedStatement(CommandText);
	}
}
