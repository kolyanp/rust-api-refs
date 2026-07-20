using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using MySqlConnector.Core;
using MySqlConnector.Logging;
using MySqlConnector.Protocol;
using MySqlConnector.Protocol.Payloads;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlConnection : DbConnection, ICloneable
{
	private static readonly StateChangeEventArgs s_stateChangeClosedConnecting = new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Connecting);

	private static readonly StateChangeEventArgs s_stateChangeConnectingOpen = new StateChangeEventArgs(ConnectionState.Connecting, ConnectionState.Open);

	private static readonly StateChangeEventArgs s_stateChangeOpenClosed = new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed);

	private static readonly object s_lock = new object();

	private static readonly Dictionary<Transaction, List<EnlistedTransactionBase>> s_transactionConnections = new Dictionary<Transaction, List<EnlistedTransactionBase>>();

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 0 })]
	private static readonly ReadOnlyMemory<byte>[] s_startTransactionPayloads = new ReadOnlyMemory<byte>[30];

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly MySqlDataSource m_dataSource;

	private readonly ILogger m_logger;

	private readonly ILogger m_transactionLogger;

	private string m_connectionString;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ConnectionSettings m_connectionSettings;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ServerSession m_session;

	private ConnectionState m_connectionState;

	private bool m_hasBeenOpened;

	private bool m_isDisposed;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 2 })]
	private Dictionary<string, CachedProcedure> m_cachedProcedures;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private SchemaProvider m_schemaProvider;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private MySqlDataReader m_activeReader;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private EnlistedTransactionBase m_enlistedTransaction;

	public override string ConnectionString
	{
		get
		{
			if (!m_hasBeenOpened)
			{
				return m_connectionString;
			}
			MySqlConnectionStringBuilder connectionStringBuilder = GetConnectionSettings().ConnectionStringBuilder;
			return connectionStringBuilder.GetConnectionString(connectionStringBuilder.PersistSecurityInfo);
		}
		[param: _003Cf1ae102e_002De29f_002D42d0_002Db906_002Dc0ed4b29cd99_003EAllowNull]
		set
		{
			if (m_connectionState == ConnectionState.Open)
			{
				throw new InvalidOperationException("Cannot change the connection string on an open connection.");
			}
			m_hasBeenOpened = false;
			m_connectionString = value ?? "";
			m_connectionSettings = null;
		}
	}

	public override string Database => m_session?.DatabaseOverride ?? GetConnectionSettings().Database;

	public override ConnectionState State => m_connectionState;

	public override string DataSource => GetConnectionSettings().ConnectionStringBuilder.Server;

	public override string ServerVersion => Session.ServerVersion.OriginalString;

	public int ServerThread => Session.ConnectionId;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	public Func<X509CertificateCollection, ValueTask> ProvideClientCertificatesCallback
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		get;
		[param: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
	public Func<MySqlProvidePasswordContext, string> ProvidePasswordCallback
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
		get;
		[param: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
		set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public RemoteCertificateValidationCallback RemoteCertificateValidationCallback
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	protected override DbProviderFactory DbProviderFactory => MySqlConnectorFactory.Instance;

	public override int ConnectionTimeout => GetConnectionSettings().ConnectionTimeout;

	public bool CanCreateBatch => true;

	internal ServerSession Session
	{
		get
		{
			VerifyNotDisposed();
			if (m_session == null || State != ConnectionState.Open)
			{
				throw new InvalidOperationException($"Connection must be Open; current state is {State}");
			}
			return m_session;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal MySqlTransaction CurrentTransaction
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		set;
	}

	internal MySqlConnectorLoggingConfiguration LoggingConfiguration { get; }

	internal bool AllowLoadLocalInfile => GetInitializedConnectionSettings().AllowLoadLocalInfile;

	internal bool AllowUserVariables => GetInitializedConnectionSettings().AllowUserVariables;

	internal bool AllowZeroDateTime => GetInitializedConnectionSettings().AllowZeroDateTime;

	internal bool ConvertZeroDateTime => GetInitializedConnectionSettings().ConvertZeroDateTime;

	internal DateTimeKind DateTimeKind => GetInitializedConnectionSettings().DateTimeKind;

	internal int DefaultCommandTimeout => GetConnectionSettings().DefaultCommandTimeout;

	internal MySqlGuidFormat GuidFormat => GetInitializedConnectionSettings().GuidFormat;

	internal bool IgnoreCommandTransaction
	{
		get
		{
			if (!GetInitializedConnectionSettings().IgnoreCommandTransaction)
			{
				return m_enlistedTransaction is StandardEnlistedTransaction;
			}
			return true;
		}
	}

	internal bool IgnorePrepare => GetInitializedConnectionSettings().IgnorePrepare;

	internal bool NoBackslashEscapes => GetInitializedConnectionSettings().NoBackslashEscapes;

	internal bool TreatTinyAsBoolean => GetInitializedConnectionSettings().TreatTinyAsBoolean;

	internal IOBehavior AsyncIOBehavior
	{
		get
		{
			if (!GetConnectionSettings().ForceSynchronous)
			{
				return IOBehavior.Asynchronous;
			}
			return IOBehavior.Synchronous;
		}
	}

	internal IOBehavior SimpleAsyncIOBehavior
	{
		get
		{
			if ((!(m_connectionSettings?.ForceSynchronous)) ?? true)
			{
				return IOBehavior.Asynchronous;
			}
			return IOBehavior.Synchronous;
		}
	}

	internal MySqlSslMode SslMode => GetInitializedConnectionSettings().SslMode;

	internal int? ActiveCommandId => m_session?.ActiveCommandId;

	internal bool SupportsPerQueryVariables => m_session?.SupportsPerQueryVariables ?? false;

	internal bool HasActiveReader => m_activeReader != null;

	internal bool SslIsEncrypted => m_session.SslIsEncrypted;

	internal bool SslIsSigned => m_session.SslIsSigned;

	internal bool SslIsAuthenticated => m_session.SslIsAuthenticated;

	internal bool SslIsMutuallyAuthenticated => m_session.SslIsMutuallyAuthenticated;

	internal SslProtocols SslProtocol => m_session.SslProtocol;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[method: _003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public event MySqlInfoMessageEventHandler InfoMessage;

	public MySqlConnection()
		: this("")
	{
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public MySqlConnection(string connectionString)
		: this(connectionString ?? "", MySqlConnectorLoggingConfiguration.GlobalConfiguration)
	{
	}

	internal MySqlConnection(MySqlDataSource dataSource)
		: this(dataSource.ConnectionString, dataSource.LoggingConfiguration)
	{
		m_dataSource = dataSource;
	}

	private MySqlConnection(string connectionString, MySqlConnectorLoggingConfiguration loggingConfiguration)
	{
		GC.SuppressFinalize(this);
		m_connectionString = connectionString;
		LoggingConfiguration = loggingConfiguration;
		m_logger = loggingConfiguration.ConnectionLogger;
		m_transactionLogger = loggingConfiguration.TransactionLogger;
	}

	public new MySqlTransaction BeginTransaction()
	{
		return BeginTransactionAsync(System.Data.IsolationLevel.Unspecified, null, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public new MySqlTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
	{
		return BeginTransactionAsync(isolationLevel, null, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public MySqlTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel, bool isReadOnly)
	{
		return BeginTransactionAsync(isolationLevel, isReadOnly, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
	{
		return BeginTransactionAsync(isolationLevel, null, IOBehavior.Synchronous, default(CancellationToken)).GetAwaiter().GetResult();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ValueTask<MySqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return BeginTransactionAsync(System.Data.IsolationLevel.Unspecified, null, AsyncIOBehavior, cancellationToken);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ValueTask<MySqlTransaction> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken))
	{
		return BeginTransactionAsync(isolationLevel, null, AsyncIOBehavior, cancellationToken);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ValueTask<MySqlTransaction> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, bool isReadOnly, CancellationToken cancellationToken = default(CancellationToken))
	{
		return BeginTransactionAsync(isolationLevel, isReadOnly, AsyncIOBehavior, cancellationToken);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private async ValueTask<MySqlTransaction> BeginTransactionAsync(System.Data.IsolationLevel isolationLevel, bool? isReadOnly, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (State != ConnectionState.Open)
		{
			throw new InvalidOperationException("Connection is not open.");
		}
		if (CurrentTransaction != null)
		{
			throw new InvalidOperationException("Transactions may not be nested.");
		}
		if (m_enlistedTransaction != null)
		{
			throw new InvalidOperationException("Cannot begin a transaction when already enlisted in a transaction.");
		}
		Log.StartingTransaction(m_transactionLogger, m_session.Id);
		ReadOnlyMemory<byte> startTransactionPayload = GetStartTransactionPayload(isolationLevel, isReadOnly, m_session.SupportsQueryAttributes);
		ConnectionSettings initializedConnectionSettings = GetInitializedConnectionSettings();
		if ((initializedConnectionSettings != null && !initializedConnectionSettings.UseCompression && !((!initializedConnectionSettings.Pipelining) ?? false)) ? true : false)
		{
			await m_session.SendRawAsync(startTransactionPayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			OkPayload.Verify((await m_session.ReceiveReplyAsync(1, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, m_session.SupportsDeprecateEof, m_session.SupportsSessionTrack);
			OkPayload.Verify((await m_session.ReceiveReplyAsync(1, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, m_session.SupportsDeprecateEof, m_session.SupportsSessionTrack);
		}
		else
		{
			await m_session.SendAsync(new PayloadData(startTransactionPayload.Slice(4, startTransactionPayload.Span[0])), ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			OkPayload.Verify((await m_session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, m_session.SupportsDeprecateEof, m_session.SupportsSessionTrack);
			await m_session.SendAsync(new PayloadData(startTransactionPayload.Slice(8 + startTransactionPayload.Span[0], startTransactionPayload.Span[startTransactionPayload.Span[0] + 4])), ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			OkPayload.Verify((await m_session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, m_session.SupportsDeprecateEof, m_session.SupportsSessionTrack);
		}
		return CurrentTransaction = new MySqlTransaction(this, isolationLevel, m_transactionLogger);
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	internal static ReadOnlyMemory<byte> GetStartTransactionPayload(System.Data.IsolationLevel isolationLevel, bool? isReadOnly, bool supportsQueryAttributes)
	{
		int num = isolationLevel switch
		{
			System.Data.IsolationLevel.ReadUncommitted => 0, 
			System.Data.IsolationLevel.ReadCommitted => 1, 
			System.Data.IsolationLevel.Serializable => 2, 
			System.Data.IsolationLevel.RepeatableRead => 3, 
			System.Data.IsolationLevel.Snapshot => 3, 
			System.Data.IsolationLevel.Unspecified => 3, 
			_ => throw new NotSupportedException($"IsolationLevel.{isolationLevel} is not supported."), 
		};
		int num2 = ((isolationLevel == System.Data.IsolationLevel.Snapshot) ? 1 : 0);
		int num3 = (isReadOnly.HasValue ? ((isReadOnly != true) ? 1 : 2) : 0);
		int num4 = num3;
		int num5 = ((supportsQueryAttributes ? 1 : 0) * 5 + num + num2) * 3 + num4;
		if (s_startTransactionPayloads[num5].IsEmpty)
		{
			byte[] array = new byte[125];
			int num6 = 4;
			int num7 = 0;
			array[num6] = 3;
			num6++;
			num7++;
			if (supportsQueryAttributes)
			{
				array[num6 + 1] = 1;
				num6 += 2;
				num7 += 2;
			}
			ReadOnlySpan<byte> readOnlySpan = "set session transaction isolation level "u8;
			readOnlySpan.CopyTo(MemoryExtensions.AsSpan(array, num6));
			num7 += readOnlySpan.Length;
			num6 += readOnlySpan.Length;
			ReadOnlySpan<byte> readOnlySpan2 = num switch
			{
				0 => "read uncommitted"u8, 
				1 => "read committed"u8, 
				2 => "serializable"u8, 
				_ => "repeatable read"u8, 
			};
			readOnlySpan2.CopyTo(MemoryExtensions.AsSpan(array, num6));
			num6 += readOnlySpan2.Length;
			num7 += readOnlySpan2.Length;
			array[num6] = 59;
			num6++;
			num7++;
			array[0] = (byte)num7;
			num6 += 4;
			array[num6] = 3;
			num6++;
			num7 = 1;
			if (supportsQueryAttributes)
			{
				array[num6 + 1] = 1;
				num6 += 2;
				num7 += 2;
			}
			ReadOnlySpan<byte> readOnlySpan3 = "start transaction"u8;
			readOnlySpan3.CopyTo(MemoryExtensions.AsSpan(array, num6));
			num7 += readOnlySpan3.Length;
			num6 += readOnlySpan3.Length;
			if (num2 == 1)
			{
				ReadOnlySpan<byte> readOnlySpan4 = " with consistent snapshot"u8;
				readOnlySpan4.CopyTo(MemoryExtensions.AsSpan(array, num6));
				num7 += readOnlySpan4.Length;
				num6 += readOnlySpan4.Length;
			}
			if (num2 > 0 && num4 > 0)
			{
				array[num6] = 44;
				num6++;
				num7++;
			}
			ReadOnlySpan<byte> readOnlySpan5 = num4 switch
			{
				1 => " read write"u8, 
				2 => " read only"u8, 
				_ => ""u8, 
			};
			readOnlySpan5.CopyTo(MemoryExtensions.AsSpan(array, num6));
			num6 += readOnlySpan5.Length;
			num7 += readOnlySpan5.Length;
			array[num6] = 59;
			num6++;
			num7++;
			array[array[0] + 4] = (byte)num7;
			s_startTransactionPayloads[num5] = new ReadOnlyMemory<byte>(array, 0, array[0] + array[array[0] + 4] + 8);
		}
		return s_startTransactionPayloads[num5];
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public override void EnlistTransaction(Transaction transaction)
	{
		if (State != ConnectionState.Open)
		{
			throw new InvalidOperationException("Connection is not open.");
		}
		if (!((!(m_enlistedTransaction?.Transaction.Equals(transaction))) ?? true))
		{
			return;
		}
		if (m_enlistedTransaction != null)
		{
			throw new MySqlException("Already enlisted in a Transaction.");
		}
		if (CurrentTransaction != null)
		{
			throw new InvalidOperationException("Can't enlist in a Transaction when there is an active MySqlTransaction.");
		}
		if ((object)transaction == null)
		{
			return;
		}
		MySqlConnection mySqlConnection = FindExistingEnlistedSession(transaction);
		if (mySqlConnection != null)
		{
			CloseAsync(changeState: false, IOBehavior.Synchronous).GetAwaiter().GetResult();
			TakeSessionFrom(mySqlConnection);
			return;
		}
		m_enlistedTransaction = (GetInitializedConnectionSettings().UseXaTransactions ? ((EnlistedTransactionBase)new XaEnlistedTransaction(transaction, this)) : ((EnlistedTransactionBase)new StandardEnlistedTransaction(transaction, this)));
		m_enlistedTransaction.Start();
		lock (s_lock)
		{
			if (!s_transactionConnections.TryGetValue(transaction, out var value))
			{
				value = (s_transactionConnections[transaction] = new List<EnlistedTransactionBase>());
			}
			value.Add(m_enlistedTransaction);
		}
	}

	internal void UnenlistTransaction()
	{
		Transaction transaction = m_enlistedTransaction.Transaction;
		m_enlistedTransaction = null;
		bool? flag = null;
		lock (s_lock)
		{
			List<EnlistedTransactionBase> list = s_transactionConnections[transaction];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Connection == this)
				{
					flag = list[i].IsIdle;
					list.RemoveAt(i);
					break;
				}
			}
			if (list.Count == 0)
			{
				s_transactionConnections.Remove(transaction);
			}
		}
		if (!flag.HasValue)
		{
			throw new InvalidOperationException("Didn't find transaction");
		}
		if (flag.Value)
		{
			Close();
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private MySqlConnection FindExistingEnlistedSession(Transaction transaction)
	{
		bool flag = false;
		bool flag2 = false;
		lock (s_lock)
		{
			if (s_transactionConnections.TryGetValue(transaction, out var value))
			{
				flag = true;
				foreach (EnlistedTransactionBase item in value)
				{
					flag2 = item.Connection.GetInitializedConnectionSettings().UseXaTransactions;
					if (item.IsIdle && item.Connection.m_connectionString == m_connectionString)
					{
						MySqlConnection connection = item.Connection;
						item.Connection = this;
						item.IsIdle = false;
						return connection;
					}
				}
			}
		}
		if (GetInitializedConnectionSettings().UseXaTransactions)
		{
			if (flag && !flag2)
			{
				throw new NotSupportedException("Cannot start an XA transaction when there is an existing non-XA transaction.");
			}
		}
		else if (flag)
		{
			throw new NotSupportedException("Multiple simultaneous connections or connections with different connection strings inside the same transaction are not supported when UseXaTransactions=False.");
		}
		return null;
	}

	private void TakeSessionFrom(MySqlConnection other)
	{
		m_session = other.m_session;
		m_session.OwningConnection = new WeakReference<MySqlConnection>(this);
		other.m_session = null;
		m_cachedProcedures = other.m_cachedProcedures;
		other.m_cachedProcedures = null;
		m_enlistedTransaction = other.m_enlistedTransaction;
		other.m_enlistedTransaction = null;
	}

	public override void Close()
	{
		CloseAsync(changeState: true, IOBehavior.Synchronous).GetAwaiter().GetResult();
	}

	public Task CloseAsync()
	{
		return CloseAsync(changeState: true, SimpleAsyncIOBehavior);
	}

	internal Task CloseAsync(IOBehavior ioBehavior)
	{
		return CloseAsync(changeState: true, ioBehavior);
	}

	public override void ChangeDatabase(string databaseName)
	{
		ChangeDatabaseAsync(IOBehavior.Synchronous, databaseName, CancellationToken.None).GetAwaiter().GetResult();
	}

	public Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ChangeDatabaseAsync(AsyncIOBehavior, databaseName, cancellationToken);
	}

	private async Task ChangeDatabaseAsync(IOBehavior ioBehavior, string databaseName, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(databaseName))
		{
			throw new ArgumentException("Database name is not valid.", "databaseName");
		}
		if (State != ConnectionState.Open)
		{
			throw new InvalidOperationException("Connection is not open.");
		}
		using (PayloadData initDatabasePayload = InitDatabasePayload.Create(databaseName))
		{
			await m_session.SendAsync(initDatabasePayload, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		OkPayload.Verify((await m_session.ReceiveReplyAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, m_session.SupportsDeprecateEof, m_session.SupportsSessionTrack);
		m_session.DatabaseOverride = databaseName;
	}

	public new MySqlCommand CreateCommand()
	{
		return (MySqlCommand)base.CreateCommand();
	}

	public bool Ping()
	{
		return PingAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public Task<bool> PingAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return PingAsync(SimpleAsyncIOBehavior, cancellationToken).AsTask();
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)]
	private async ValueTask<bool> PingAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (m_session == null)
		{
			return false;
		}
		try
		{
			if (await m_session.TryPingAsync(logInfo: true, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return true;
			}
		}
		catch (InvalidOperationException)
		{
		}
		SetState(ConnectionState.Closed);
		return false;
	}

	public override void Open()
	{
		OpenAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public override Task OpenAsync(CancellationToken cancellationToken)
	{
		return OpenAsync(null, cancellationToken);
	}

	internal async Task OpenAsync(IOBehavior? ioBehavior, CancellationToken cancellationToken)
	{
		long timestamp = Stopwatch.GetTimestamp();
		VerifyNotDisposed();
		cancellationToken.ThrowIfCancellationRequested();
		if (State != ConnectionState.Closed)
		{
			throw new InvalidOperationException($"Cannot Open when State is {State}.");
		}
		using Activity activity = ActivitySourceHelper.StartActivity("Open");
		try
		{
			SetState(ConnectionState.Connecting);
			ConnectionPool connectionPool = m_dataSource?.Pool ?? ConnectionPool.GetPool(m_connectionString, LoggingConfiguration);
			if (m_connectionSettings == null)
			{
				m_connectionSettings = connectionPool?.ConnectionSettings ?? new ConnectionSettings(new MySqlConnectionStringBuilder(m_connectionString));
			}
			if (m_connectionSettings.AutoEnlist && (object)Transaction.Current != null)
			{
				MySqlConnection mySqlConnection = FindExistingEnlistedSession(Transaction.Current);
				if (mySqlConnection != null)
				{
					TakeSessionFrom(mySqlConnection);
					ActivitySourceHelper.CopyTags(m_session.ActivityTags, activity);
					m_hasBeenOpened = true;
					SetState(ConnectionState.Open);
					return;
				}
			}
			try
			{
				m_session = await CreateSessionAsync(connectionPool, timestamp, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				m_hasBeenOpened = true;
				SetState(ConnectionState.Open);
			}
			catch (OperationCanceledException ex)
			{
				SetState(ConnectionState.Closed);
				if (!cancellationToken.Equals(ex.CancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();
				}
				throw;
			}
			catch (MySqlException)
			{
				SetState(ConnectionState.Closed);
				cancellationToken.ThrowIfCancellationRequested();
				throw;
			}
			catch (SocketException)
			{
				SetState(ConnectionState.Closed);
				throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Unable to connect to any of the specified MySQL hosts.");
			}
			if (m_connectionSettings.AutoEnlist && (object)Transaction.Current != null)
			{
				EnlistTransaction(Transaction.Current);
			}
		}
		catch (Exception exception) when (activity?.IsAllDataRequested ?? false)
		{
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder = m_connectionSettings?.ConnectionStringBuilder;
			if (mySqlConnectionStringBuilder != null)
			{
				activity.SetTag("db.connection_string", mySqlConnectionStringBuilder.GetConnectionString(mySqlConnectionStringBuilder.PersistSecurityInfo));
			}
			activity.SetException(exception);
			throw;
		}
	}

	public async ValueTask ResetConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		ServerSession session = Session;
		Log.ResettingConnection(m_logger, session.Id);
		await session.SendAsync(ResetConnectionPayload.Instance, AsyncIOBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		OkPayload.Verify((await session.ReceiveReplyAsync(AsyncIOBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span, session.SupportsDeprecateEof, session.SupportsSessionTrack);
	}

	public static void ClearPool(MySqlConnection connection)
	{
		ClearPoolAsync(connection, IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static Task ClearPoolAsync(MySqlConnection connection, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ClearPoolAsync(connection, connection.AsyncIOBehavior, cancellationToken);
	}

	public static void ClearAllPools()
	{
		ConnectionPool.ClearPoolsAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}

	public static Task ClearAllPoolsAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return ConnectionPool.ClearPoolsAsync(IOBehavior.Asynchronous, cancellationToken);
	}

	private static async Task ClearPoolAsync(MySqlConnection connection, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		ConnectionPool pool = ConnectionPool.GetPool(connection.m_connectionString, null, createIfNotFound: false);
		if (pool != null)
		{
			await pool.ClearAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	protected override DbCommand CreateDbCommand()
	{
		return new MySqlCommand(this, null);
	}

	public override DataTable GetSchema()
	{
		return GetSchemaProvider().GetSchemaAsync(IOBehavior.Synchronous, "MetaDataCollections", null, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public override DataTable GetSchema(string collectionName)
	{
		return GetSchemaProvider().GetSchemaAsync(IOBehavior.Synchronous, collectionName, null, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public override DataTable GetSchema(string collectionName, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })] string[] restrictionValues)
	{
		return GetSchemaProvider().GetSchemaAsync(IOBehavior.Synchronous, collectionName, restrictionValues, default(CancellationToken)).GetAwaiter().GetResult();
	}

	public Task<DataTable> GetSchemaAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSchemaProvider().GetSchemaAsync(AsyncIOBehavior, "MetaDataCollections", null, cancellationToken).AsTask();
	}

	public Task<DataTable> GetSchemaAsync(string collectionName, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSchemaProvider().GetSchemaAsync(AsyncIOBehavior, collectionName, null, cancellationToken).AsTask();
	}

	public Task<DataTable> GetSchemaAsync(string collectionName, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })] string[] restrictionValues, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSchemaProvider().GetSchemaAsync(AsyncIOBehavior, collectionName, restrictionValues, cancellationToken).AsTask();
	}

	private SchemaProvider GetSchemaProvider()
	{
		return m_schemaProvider ?? (m_schemaProvider = new SchemaProvider(this));
	}

	public MySqlBatch CreateBatch()
	{
		return new MySqlBatch(this);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				CloseAsync(changeState: true, IOBehavior.Synchronous).GetAwaiter().GetResult();
			}
		}
		finally
		{
			m_isDisposed = true;
			base.Dispose(disposing);
		}
	}

	public async Task DisposeAsync()
	{
		try
		{
			await CloseAsync(changeState: true, SimpleAsyncIOBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			m_isDisposed = true;
			base.Dispose(disposing: true);
		}
	}

	public MySqlConnection Clone()
	{
		return new MySqlConnection(this, m_dataSource, m_connectionString, m_hasBeenOpened);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public MySqlConnection CloneWith(string connectionString)
	{
		MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString ?? throw new ArgumentNullException("connectionString"));
		MySqlConnectionStringBuilder connectionStringBuilder = GetConnectionSettings().ConnectionStringBuilder;
		bool flag = mySqlConnectionStringBuilder.Password.Length == 0 && (!mySqlConnectionStringBuilder.PersistSecurityInfo || connectionStringBuilder.PersistSecurityInfo);
		if (flag)
		{
			mySqlConnectionStringBuilder.Password = connectionStringBuilder.Password;
		}
		string connectionString2 = mySqlConnectionStringBuilder.ConnectionString;
		MySqlDataSource dataSource = ((connectionString2 == connectionStringBuilder.ConnectionString) ? m_dataSource : null);
		return new MySqlConnection(this, dataSource, connectionString2, m_hasBeenOpened && flag && !connectionStringBuilder.PersistSecurityInfo);
	}

	internal void SetSessionFailed(Exception exception)
	{
		m_session.SetFailed(exception);
	}

	internal void Cancel(ICancellableCommand command, int commandId, bool isCancel)
	{
		string text = m_session?.Id;
		if (text == null || State != ConnectionState.Open || !(m_session?.TryStartCancel(command) ?? false))
		{
			Log.IgnoringCancellationForCommand(m_logger, commandId);
			return;
		}
		Log.CommandHasBeenCanceled(m_logger, commandId, text, isCancel ? "Cancel()" : "command timeout");
		try
		{
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(m_connectionString)
			{
				AutoEnlist = false,
				Pooling = false
			};
			IPEndPoint iPEndPoint = m_session.IPEndPoint;
			if (iPEndPoint != null)
			{
				IPAddress address = iPEndPoint.Address;
				if (address != null)
				{
					int port = iPEndPoint.Port;
					mySqlConnectionStringBuilder.Server = address.ToString();
					mySqlConnectionStringBuilder.Port = (uint)port;
				}
			}
			mySqlConnectionStringBuilder.UserID = m_session.UserID;
			int cancellationTimeout = GetConnectionSettings().CancellationTimeout;
			mySqlConnectionStringBuilder.ConnectionTimeout = ((cancellationTimeout < 1) ? 3u : ((uint)cancellationTimeout));
			using MySqlConnection mySqlConnection = CloneWith(mySqlConnectionStringBuilder.ConnectionString);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(FormattableString.Invariant($"KILL QUERY {command.Connection.ServerThread}"), mySqlConnection);
			mySqlCommand.CommandTimeout = ((cancellationTimeout < 1) ? 3 : cancellationTimeout);
			m_session?.DoCancel(command, mySqlCommand);
		}
		catch (InvalidOperationException exception)
		{
			Log.IgnoringCancellationForClosedConnection(m_logger, exception, text);
			m_session?.AbortCancel(command);
		}
		catch (MySqlException exception2)
		{
			Log.CancelingCommandFailed(m_logger, exception2, text, command.CommandId);
			m_session?.AbortCancel(command);
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 2 })]
	internal async Task<CachedProcedure> GetCachedProcedure(string name, bool revalidateMissing, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.GettingCachedProcedure(m_logger, m_session.Id, name);
		if (State != ConnectionState.Open)
		{
			throw new InvalidOperationException("Connection is not open.");
		}
		Dictionary<string, CachedProcedure> cachedProcedures = m_session.Pool?.GetProcedureCache() ?? m_cachedProcedures;
		if (cachedProcedures == null)
		{
			Log.PoolDoesNotHaveSharedProcedureCache(m_logger, m_session.Id, m_session.Pool?.Id);
			cachedProcedures = (m_cachedProcedures = new Dictionary<string, CachedProcedure>());
		}
		NormalizedSchema normalized = NormalizedSchema.MustNormalize(name, Database);
		if (string.IsNullOrEmpty(normalized.Schema))
		{
			Log.CouldNotNormalizeDatabaseAndName(m_logger, m_session.Id, name, Database);
			return null;
		}
		bool flag;
		CachedProcedure value;
		lock (cachedProcedures)
		{
			flag = cachedProcedures.TryGetValue(normalized.FullyQualified, out value);
		}
		if (!flag || (value == null && revalidateMissing))
		{
			value = await CachedProcedure.FillAsync(ioBehavior, this, normalized.Schema, normalized.Component, m_logger, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (value == null)
			{
				Log.FailedToCacheProcedure(m_logger, m_session.Id, normalized.Schema, normalized.Component);
			}
			else
			{
				Log.CachingProcedure(m_logger, m_session.Id, normalized.Schema, normalized.Component);
			}
			int count;
			lock (cachedProcedures)
			{
				cachedProcedures[normalized.FullyQualified] = value;
				count = cachedProcedures.Count;
			}
			Log.ProcedureCacheCount(m_logger, m_session.Id, count);
		}
		if (value == null)
		{
			Log.DidNotFindCachedProcedure(m_logger, m_session.Id, normalized.Schema, normalized.Component);
		}
		else
		{
			Log.ReturningCachedProcedure(m_logger, m_session.Id, normalized.Schema, normalized.Component);
		}
		return value;
	}

	internal void SetActiveReader(MySqlDataReader dataReader)
	{
		if (dataReader == null)
		{
			throw new ArgumentNullException("dataReader");
		}
		if (m_activeReader != null)
		{
			throw new InvalidOperationException("Can't replace active reader.");
		}
		m_activeReader = dataReader;
	}

	internal void FinishQuerying(bool hasWarnings)
	{
		m_session.FinishQuerying();
		m_activeReader = null;
		if (!hasWarnings || this.InfoMessage == null)
		{
			return;
		}
		List<MySqlError> list = new List<MySqlError>();
		using (MySqlCommand mySqlCommand = new MySqlCommand("SHOW WARNINGS;", this))
		{
			mySqlCommand.Transaction = CurrentTransaction;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				list.Add(new MySqlError(mySqlDataReader.GetString(0), mySqlDataReader.GetInt32(1), mySqlDataReader.GetString(2)));
			}
		}
		this.InfoMessage(this, new MySqlInfoMessageEventArgs(list));
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private async ValueTask<ServerSession> CreateSessionAsync(ConnectionPool pool, long startingTimestamp, Activity activity, IOBehavior? ioBehavior, CancellationToken cancellationToken)
	{
		MetricsReporter.AddPendingRequest(pool);
		ConnectionSettings connectionSettings = GetInitializedConnectionSettings();
		IOBehavior actualIOBehavior = (IOBehavior)(((int?)ioBehavior) ?? ((!connectionSettings.ForceSynchronous) ? 1 : 0));
		CancellationTokenSource timeoutSource = null;
		CancellationTokenSource linkedSource = null;
		try
		{
			if (connectionSettings.ConnectionTimeout != 0)
			{
				timeoutSource = new CancellationTokenSource(TimeSpan.FromMilliseconds((double)Math.Max(1, connectionSettings.ConnectionTimeoutMilliseconds - Utility.GetElapsedMilliseconds(startingTimestamp))));
			}
			if (cancellationToken.CanBeCanceled && timeoutSource != null)
			{
				linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
			}
			CancellationToken cancellationToken2 = linkedSource?.Token ?? timeoutSource?.Token ?? cancellationToken;
			if (pool != null)
			{
				return await pool.GetSessionAsync(this, startingTimestamp, connectionSettings.ConnectionTimeoutMilliseconds, activity, actualIOBehavior, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
			}
			ILoadBalancer loadBalancer = ((connectionSettings.LoadBalance == MySqlLoadBalance.Random && connectionSettings.HostNames.Count > 1) ? RandomLoadBalancer.Instance : FailOverLoadBalancer.Instance);
			ServerSession session = new ServerSession(m_logger)
			{
				OwningConnection = new WeakReference<MySqlConnection>(this)
			};
			Log.CreatedNonPooledSession(m_logger, session.Id);
			try
			{
				await session.ConnectAsync(connectionSettings, this, startingTimestamp, loadBalancer, activity, actualIOBehavior, cancellationToken2).ConfigureAwait(continueOnCapturedContext: false);
				return session;
			}
			catch (Exception)
			{
				await session.DisposeAsync(actualIOBehavior, default(CancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
				throw;
			}
		}
		catch (OperationCanceledException) when (timeoutSource?.IsCancellationRequested ?? false)
		{
			MetricsReporter.AddTimeout(pool, connectionSettings);
			string text = ((pool?.IsEmpty ?? false) ? " All pooled connections are in use." : "");
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired." + text);
		}
		catch (MySqlException ex3) when ((timeoutSource?.IsCancellationRequested ?? false) || ex3.ErrorCode == MySqlErrorCode.CommandTimeoutExpired)
		{
			MetricsReporter.AddTimeout(pool, connectionSettings);
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Connect Timeout expired.", ex3);
		}
		catch (MySqlException ex4) when (ex4.ErrorCode == MySqlErrorCode.UnableToConnectToHost && ex4.Message == "Connect Timeout expired.")
		{
			MetricsReporter.AddTimeout(pool, connectionSettings);
			throw;
		}
		finally
		{
			MetricsReporter.RemovePendingRequest(pool);
			linkedSource?.Dispose();
			timeoutSource?.Dispose();
		}
	}

	internal void SetState(ConnectionState newState)
	{
		if (m_connectionState != newState)
		{
			ConnectionState connectionState = m_connectionState;
			m_connectionState = newState;
			StateChangeEventArgs stateChange = ((connectionState == ConnectionState.Closed && newState == ConnectionState.Connecting) ? s_stateChangeClosedConnecting : ((connectionState == ConnectionState.Connecting && newState == ConnectionState.Open) ? s_stateChangeConnectingOpen : ((connectionState == ConnectionState.Open && newState == ConnectionState.Closed) ? s_stateChangeOpenClosed : new StateChangeEventArgs(connectionState, newState))));
			OnStateChange(stateChange);
		}
	}

	private MySqlConnection(MySqlConnection other, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] MySqlDataSource dataSource, string connectionString, bool hasBeenOpened)
		: this(connectionString, other.LoggingConfiguration)
	{
		m_dataSource = dataSource;
		m_hasBeenOpened = hasBeenOpened;
		ProvideClientCertificatesCallback = other.ProvideClientCertificatesCallback;
		ProvidePasswordCallback = other.ProvidePasswordCallback;
		RemoteCertificateValidationCallback = other.RemoteCertificateValidationCallback;
	}

	private void VerifyNotDisposed()
	{
		if (m_isDisposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	private async Task CloseAsync(bool changeState, IOBehavior ioBehavior)
	{
		if (m_activeReader != null || CurrentTransaction != null || m_enlistedTransaction != null || !(m_connectionSettings?.Pooling ?? false))
		{
			await DoCloseAsync(changeState, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		m_cachedProcedures = null;
		if (m_session != null)
		{
			await m_session.ReturnToPoolAsync(ioBehavior, this).ConfigureAwait(continueOnCapturedContext: false);
			m_session = null;
		}
		if (changeState)
		{
			SetState(ConnectionState.Closed);
		}
	}

	private async Task DoCloseAsync(bool changeState, IOBehavior ioBehavior)
	{
		if (m_enlistedTransaction != null)
		{
			if (m_activeReader != null)
			{
				await m_activeReader.DisposeAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			}
			m_activeReader = null;
			MySqlConnection mySqlConnection = new MySqlConnection
			{
				m_connectionString = m_connectionString,
				m_connectionSettings = m_connectionSettings,
				m_connectionState = m_connectionState,
				m_hasBeenOpened = true
			};
			mySqlConnection.TakeSessionFrom(this);
			lock (s_lock)
			{
				foreach (EnlistedTransactionBase item in s_transactionConnections[mySqlConnection.m_enlistedTransaction.Transaction])
				{
					if (item.Connection == this)
					{
						item.Connection = mySqlConnection;
						item.IsIdle = true;
						break;
					}
				}
			}
			if (changeState)
			{
				SetState(ConnectionState.Closed);
			}
			return;
		}
		m_cachedProcedures = null;
		try
		{
			if (m_activeReader != null || CurrentTransaction != null)
			{
				await CloseDatabaseAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			if (m_session != null)
			{
				if (GetInitializedConnectionSettings().Pooling)
				{
					await m_session.ReturnToPoolAsync(ioBehavior, this).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await m_session.DisposeAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
					m_session.OwningConnection = null;
				}
				m_session = null;
			}
			if (changeState)
			{
				SetState(ConnectionState.Closed);
			}
		}
	}

	private async ValueTask CloseDatabaseAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		if (m_activeReader != null)
		{
			await m_activeReader.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (CurrentTransaction != null && m_session.IsConnected)
		{
			await CurrentTransaction.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			CurrentTransaction = null;
		}
	}

	private ConnectionSettings GetConnectionSettings()
	{
		return m_connectionSettings ?? (m_connectionSettings = new ConnectionSettings(new MySqlConnectionStringBuilder(m_connectionString)));
	}

	private ConnectionSettings GetInitializedConnectionSettings()
	{
		return m_connectionSettings;
	}
}
