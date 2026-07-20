using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class MySqlTransaction : DbTransaction
{
	private readonly ILogger m_logger;

	private bool m_isDisposed;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public new MySqlConnection Connection
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		private set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	protected override DbConnection DbConnection
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get
		{
			return Connection;
		}
	}

	public override IsolationLevel IsolationLevel { get; }

	public override void Commit()
	{
		CommitAsync(IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return CommitAsync(Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken);
	}

	private async Task CommitAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyValid();
		using Activity activity = Connection.Session.StartActivity("Commit");
		Log.CommittingTransaction(m_logger, Connection.Session.Id);
		try
		{
			using (MySqlCommand cmd = new MySqlCommand("commit", Connection, this)
			{
				NoActivity = true
			})
			{
				await cmd.ExecuteNonQueryAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			Connection.CurrentTransaction = null;
			Log.CommittedTransaction(m_logger, Connection.Session.Id);
			Connection = null;
		}
		catch (Exception exception) when (activity?.IsAllDataRequested ?? false)
		{
			activity.SetException(exception);
			throw;
		}
	}

	public override void Rollback()
	{
		RollbackAsync(IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return RollbackAsync(Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken);
	}

	private async Task RollbackAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyValid();
		await DoRollback(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Connection.CurrentTransaction = null;
		Connection = null;
	}

	public void Release(string savepointName)
	{
		ExecuteSavepointAsync("release ", savepointName, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteSavepointAsync("release ", savepointName, Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken);
	}

	public void Rollback(string savepointName)
	{
		ExecuteSavepointAsync("rollback to ", savepointName, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteSavepointAsync("rollback to ", savepointName, Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken);
	}

	public void Save(string savepointName)
	{
		ExecuteSavepointAsync("", savepointName, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task SaveAsync(string savepointName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ExecuteSavepointAsync("", savepointName, Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, cancellationToken);
	}

	private async Task ExecuteSavepointAsync(string command, string savepointName, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		VerifyValid();
		if (savepointName == null)
		{
			throw new ArgumentNullException("savepointName");
		}
		if (savepointName.Length == 0)
		{
			throw new ArgumentException("savepointName must not be empty", "savepointName");
		}
		using MySqlCommand cmd = new MySqlCommand(command + "savepoint " + QuoteIdentifier(savepointName), Connection, this)
		{
			NoActivity = true
		};
		await cmd.ExecuteNonQueryAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				DisposeAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public Task DisposeAsync()
	{
		return DisposeAsync(Connection?.AsyncIOBehavior ?? IOBehavior.Asynchronous, CancellationToken.None);
	}

	internal Task DisposeAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		m_isDisposed = true;
		if (Connection?.CurrentTransaction == this)
		{
			return DoDisposeAsync(ioBehavior, cancellationToken);
		}
		Connection = null;
		return Task.CompletedTask;
	}

	private async Task DoDisposeAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (Connection?.CurrentTransaction == this)
		{
			if (Connection.State == ConnectionState.Open && Connection.Session.IsConnected)
			{
				try
				{
					await DoRollback(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (IOException)
				{
				}
				catch (SocketException)
				{
				}
			}
			Connection.CurrentTransaction = null;
		}
		Connection = null;
	}

	internal MySqlTransaction(MySqlConnection connection, IsolationLevel isolationLevel, ILogger logger)
	{
		Connection = connection;
		IsolationLevel = isolationLevel;
		m_logger = logger;
		Log.StartedTransaction(m_logger, Connection.Session.Id);
	}

	private async Task DoRollback(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		using Activity activity = Connection.Session.StartActivity("Rollback");
		Log.RollingBackTransaction(m_logger, Connection.Session.Id);
		try
		{
			using MySqlCommand cmd = new MySqlCommand("rollback", Connection, this)
			{
				NoActivity = true
			};
			await cmd.ExecuteNonQueryAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			Log.RolledBackTransaction(m_logger, Connection.Session.Id);
		}
		catch (Exception exception) when (activity?.IsAllDataRequested ?? false)
		{
			activity.SetException(exception);
			throw;
		}
	}

	private void VerifyValid()
	{
		if (m_isDisposed)
		{
			throw new ObjectDisposedException("MySqlTransaction");
		}
		if (Connection == null)
		{
			throw new InvalidOperationException("Already committed or rolled back.");
		}
		if (Connection.CurrentTransaction == null)
		{
			throw new InvalidOperationException("There is no active transaction.");
		}
		if (Connection.CurrentTransaction != this)
		{
			throw new InvalidOperationException("This is not the active transaction.");
		}
	}

	private static string QuoteIdentifier(string identifier)
	{
		return "`" + identifier.Replace("`", "``") + "`";
	}
}
