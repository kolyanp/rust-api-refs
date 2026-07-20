using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector.Core;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
public sealed class MySqlBatch : ICancellableCommand, IDisposable
{
	private readonly int m_commandId;

	private bool m_isDisposed;

	private int m_timeout;

	private Action m_cancelAction;

	private Action m_cancelForCommandTimeoutAction;

	private uint m_cancelTimerId;

	private bool m_commandTimedOut;

	public MySqlConnection Connection { get; set; }

	public MySqlTransaction Transaction { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	public MySqlBatchCommandCollection BatchCommands
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	}

	public int Timeout
	{
		get
		{
			return m_timeout;
		}
		set
		{
			m_timeout = value;
			((ICancellableCommand)this).EffectiveCommandTimeout = null;
		}
	}

	internal CommandBehavior CurrentCommandBehavior { get; set; }

	int ICancellableCommand.CommandId => m_commandId;

	int ICancellableCommand.CommandTimeout => Timeout;

	int? ICancellableCommand.EffectiveCommandTimeout { get; set; }

	int ICancellableCommand.CancelAttemptCount { get; set; }

	bool ICancellableCommand.IsTimedOut => Volatile.Read(in m_commandTimedOut);

	private bool IsPrepared
	{
		get
		{
			foreach (MySqlBatchCommand batchCommand in BatchCommands)
			{
				if (Connection.Session.TryGetPreparedStatement(batchCommand.CommandText) == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	private IOBehavior AsyncIOBehavior => Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous;

	public MySqlBatch()
		: this(null, null)
	{
	}

	public MySqlBatch(MySqlConnection connection = null, MySqlTransaction transaction = null)
	{
		Connection = connection;
		Transaction = transaction;
		BatchCommands = new MySqlBatchCommandCollection();
		m_commandId = ICancellableCommandExtensions.GetNextId();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public MySqlDataReader ExecuteReader(CommandBehavior commandBehavior = CommandBehavior.Default)
	{
		return (MySqlDataReader)ExecuteDbDataReader(commandBehavior);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public async Task<MySqlDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (MySqlDataReader)(await ExecuteDbDataReaderAsync(CommandBehavior.Default, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		this.ResetCommandTimeout();
		return ExecuteReaderAsync(behavior, IOBehavior.Synchronous, CancellationToken.None).Result;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
	{
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			return await ExecuteReaderAsync(behavior, AsyncIOBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private ValueTask<MySqlDataReader> ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (!IsValid(out var exception))
		{
			return ValueTaskExtensions.FromException<MySqlDataReader>(exception);
		}
		CurrentCommandBehavior = behavior;
		foreach (MySqlBatchCommand batchCommand in BatchCommands)
		{
			batchCommand.Batch = this;
		}
		ICommandPayloadCreator payloadCreator = (IsPrepared ? SingleCommandPayloadCreator.Instance : ConcatenatedCommandPayloadCreator.Instance);
		return CommandExecutor.ExecuteReaderAsync(new CommandListPosition(BatchCommands.Commands), payloadCreator, behavior, null, ioBehavior, cancellationToken);
	}

	public int ExecuteNonQuery()
	{
		return ExecuteNonQueryAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public object ExecuteScalar()
	{
		return ExecuteScalarAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteNonQueryAsync(AsyncIOBehavior, cancellationToken);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	public Task<object> ExecuteScalarAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteScalarAsync(AsyncIOBehavior, cancellationToken);
	}

	public void Prepare()
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
			DoPrepareAsync(IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
		}
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return PrepareAsync(AsyncIOBehavior, cancellationToken);
	}

	public void Cancel()
	{
		Connection?.Cancel(this, m_commandId, isCancel: true);
	}

	public void Dispose()
	{
		m_isDisposed = true;
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
		Volatile.Write(ref m_commandTimedOut, value: false);
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
		Cancel();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private async Task<int> ExecuteNonQueryAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			using MySqlDataReader reader = await ExecuteReaderAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			while (await reader.ReadAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) || await reader.NextResultAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
			}
			return reader.RecordsAffected;
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	private async Task<object> ExecuteScalarAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		this.ResetCommandTimeout();
		using (((ICancellableCommand)this).RegisterCancel(cancellationToken))
		{
			bool hasSetResult = false;
			object result = null;
			using MySqlDataReader reader = await ExecuteReaderAsync(CommandBehavior.Default, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
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

	private bool IsValid([_003Ce940fe46_002D60b5_002D4fb7_002D817f_002D6effabbc4d82_003ENotNullWhen(false)] out Exception exception)
	{
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
				exception = new InvalidOperationException("The transaction associated with this batch is not the connection's active transaction; see https://fl.vu/mysql-trans");
			}
			else if (BatchCommands.Count == 0)
			{
				exception = new InvalidOperationException("BatchCommands must contain a command");
			}
			else
			{
				exception = GetExceptionForInvalidCommands();
			}
		}
		return exception == null;
	}

	private bool NeedsPrepare(out Exception exception)
	{
		if (m_isDisposed)
		{
			exception = new ObjectDisposedException(GetType().Name);
		}
		else if (Connection == null)
		{
			exception = new InvalidOperationException("Connection property must be non-null.");
		}
		else if (Connection.State != ConnectionState.Open)
		{
			exception = new InvalidOperationException($"Connection must be Open; current state is {Connection.State}");
		}
		else if (BatchCommands.Count == 0)
		{
			exception = new InvalidOperationException("BatchCommands must contain a command");
		}
		else if (Connection.HasActiveReader)
		{
			exception = new InvalidOperationException("Cannot call Prepare when there is an open DataReader for this command; it must be closed first.");
		}
		else
		{
			exception = GetExceptionForInvalidCommands();
		}
		if (exception == null)
		{
			return !Connection.IgnorePrepare;
		}
		return false;
	}

	private InvalidOperationException GetExceptionForInvalidCommands()
	{
		foreach (MySqlBatchCommand batchCommand in BatchCommands)
		{
			if (batchCommand == null)
			{
				return new InvalidOperationException("BatchCommands must not contain null");
			}
			if (string.IsNullOrWhiteSpace(batchCommand.CommandText))
			{
				return new InvalidOperationException("CommandText must be specified on each batch command");
			}
		}
		return null;
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
		return DoPrepareAsync(ioBehavior, cancellationToken);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	private async Task DoPrepareAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		foreach (MySqlBatchCommand batchCommand in BatchCommands)
		{
			if (((IMySqlCommand)batchCommand).CommandType != CommandType.Text)
			{
				throw new NotSupportedException("Only CommandType.Text is currently supported by MySqlBatch.Prepare");
			}
			batchCommand.Batch = this;
			if (Connection.Session.TryGetPreparedStatement(((IMySqlCommand)batchCommand).CommandText) == null)
			{
				await Connection.Session.PrepareAsync(batchCommand, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}
}
