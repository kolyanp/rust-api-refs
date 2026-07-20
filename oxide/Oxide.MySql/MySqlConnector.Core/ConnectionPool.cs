using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;
using MySqlConnector.Utilities;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class ConnectionPool : IDisposable
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class LeastConnectionsLoadBalancer(Dictionary<string, int> hostSessions) : ILoadBalancer
	{
		public IReadOnlyList<string> LoadBalance(IReadOnlyList<string> hosts)
		{
			lock (hostSessions)
			{
				return (from x in hostSessions
					orderby x.Value
					select x.Key).ToList();
			}
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class ConnectionStringPool(string connectionString, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ConnectionPool pool)
	{
		public string ConnectionString { get; } = connectionString;

		[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
		[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
		public ConnectionPool Pool
		{
			[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
			get;
		} = pool;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })]
	private static readonly ConcurrentDictionary<string, ConnectionPool> s_pools;

	private static readonly List<ConnectionPool> s_allPools;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })]
	private static readonly Action<ILogger, int, string, Exception> s_createdNewSession;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })]
	private static readonly Action<ILogger, int, string, Exception> s_createdToReachMinimumPoolSize;

	private static int s_poolId;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private static ConnectionStringPool s_mruCache;

	private readonly ILogger m_logger;

	private readonly ILogger m_connectionLogger;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 0, 1, 2 })]
	private readonly KeyValuePair<string, object>[] m_stateTagList;

	private readonly SemaphoreSlim m_cleanSemaphore;

	private readonly SemaphoreSlim m_sessionSemaphore;

	private readonly LinkedList<ServerSession> m_sessions;

	private readonly Dictionary<string, ServerSession> m_leasedSessions;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly ILoadBalancer m_loadBalancer;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private readonly Dictionary<string, int> m_hostSessions;

	private int m_generation;

	private uint m_lastRecoveryTime;

	private int m_lastSessionId;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 2 })]
	private Dictionary<string, CachedProcedure> m_procedureCache;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Timer m_dnsCheckTimer;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Timer m_reaperTimer;

	public int Id { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public string Name
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public ConnectionSettings ConnectionSettings { get; }

	internal bool IsEmpty => m_sessionSemaphore.CurrentCount == 0;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public ReadOnlySpan<KeyValuePair<string, object>> IdleStateTagList
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
		get
		{
			return MemoryExtensions.AsSpan(m_stateTagList, 0, 2);
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public ReadOnlySpan<KeyValuePair<string, object>> UsedStateTagList
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
		get
		{
			return MemoryExtensions.AsSpan(m_stateTagList, 1, 2);
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
	public ReadOnlySpan<KeyValuePair<string, object>> PoolNameTagList
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 0, 1, 2 })]
		get
		{
			return MemoryExtensions.AsSpan(m_stateTagList, 1, 1);
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public async ValueTask<ServerSession> GetSessionAsync(MySqlConnection connection, long startingTimestamp, int timeoutMilliseconds, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (IsEmpty && (uint)(Environment.TickCount - (int)m_lastRecoveryTime) >= 1000u)
		{
			Log.ScanningForLeakedSessions(m_logger, Id);
			await RecoverLeakedSessionsAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (ConnectionSettings.MinimumPoolSize > 0)
		{
			await CreateMinimumPooledSessions(connection, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		Log.WaitingForAvailableSession(m_logger, Id);
		if (ioBehavior == IOBehavior.Asynchronous)
		{
			await m_sessionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			m_sessionSemaphore.Wait(cancellationToken);
		}
		ServerSession session = null;
		try
		{
			lock (m_sessions)
			{
				if (m_sessions.Count > 0)
				{
					session = m_sessions.First.Value;
					m_sessions.RemoveFirst();
				}
			}
			if (session != null)
			{
				MetricsReporter.RemoveIdle(this);
				Log.FoundExistingSession(m_logger, Id);
				bool flag;
				if (session.PoolGeneration != m_generation)
				{
					Log.DiscardingSessionDueToWrongGeneration(m_logger, Id);
					flag = false;
				}
				else if (ConnectionSettings.ConnectionReset || session.DatabaseOverride != null)
				{
					if (timeoutMilliseconds != 0)
					{
						session.SetTimeout(Math.Max(1, timeoutMilliseconds - Utility.GetElapsedMilliseconds(startingTimestamp)));
					}
					flag = await session.TryResetConnectionAsync(ConnectionSettings, connection, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					session.SetTimeout(int.MaxValue);
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					session.OwningConnection = new WeakReference<MySqlConnection>(connection);
					int count;
					lock (m_leasedSessions)
					{
						m_leasedSessions.Add(session.Id, session);
						count = m_leasedSessions.Count;
					}
					MetricsReporter.AddUsed(this);
					ActivitySourceHelper.CopyTags(session.ActivityTags, activity);
					Log.ReturningPooledSession(m_logger, Id, session.Id, count);
					session.LastLeasedTimestamp = Stopwatch.GetTimestamp();
					MetricsReporter.RecordWaitTime(this, Utility.GetElapsedSeconds(startingTimestamp, session.LastLeasedTimestamp));
					return session;
				}
				Log.SessionIsUnusable(m_logger, Id, session.Id);
				AdjustHostConnectionCount(session, -1);
				await session.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			session = await ConnectSessionAsync(connection, s_createdNewSession, startingTimestamp, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			AdjustHostConnectionCount(session, 1);
			session.OwningConnection = new WeakReference<MySqlConnection>(connection);
			int count2;
			lock (m_leasedSessions)
			{
				m_leasedSessions.Add(session.Id, session);
				count2 = m_leasedSessions.Count;
			}
			MetricsReporter.AddUsed(this);
			Log.ReturningNewSession(m_logger, Id, session.Id, count2);
			session.LastLeasedTimestamp = Stopwatch.GetTimestamp();
			MetricsReporter.RecordCreateTime(this, Utility.GetElapsedSeconds(startingTimestamp, session.LastLeasedTimestamp));
			return session;
		}
		catch (Exception ex)
		{
			if (session != null)
			{
				try
				{
					Log.DisposingCreatedSessionDueToException(m_logger, ex, Id, session.Id, ex.Message);
					AdjustHostConnectionCount(session, -1);
					await session.DisposeAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception ex2)
				{
					Log.UnexpectedErrorInGetSessionAsync(m_logger, ex2, Id, ex2.Message);
				}
			}
			m_sessionSemaphore.Release();
			throw;
		}
	}

	private int GetSessionHealth(ServerSession session)
	{
		if (!session.IsConnected)
		{
			return 1;
		}
		if (session.PoolGeneration != m_generation)
		{
			return 2;
		}
		if (ConnectionSettings.ConnectionLifeTime != 0 && Utility.GetElapsedMilliseconds(session.CreatedTimestamp) >= ConnectionSettings.ConnectionLifeTime)
		{
			return 3;
		}
		return 0;
	}

	public async ValueTask ReturnAsync(IOBehavior ioBehavior, ServerSession session)
	{
		Log.ReceivingSessionBack(m_logger, Id, session.Id);
		try
		{
			lock (m_leasedSessions)
			{
				m_leasedSessions.Remove(session.Id);
			}
			MetricsReporter.RemoveUsed(this);
			session.OwningConnection = null;
			session.DataReader = new MySqlDataReader();
			switch (GetSessionHealth(session))
			{
			case 0:
				lock (m_sessions)
				{
					m_sessions.AddFirst(session);
				}
				MetricsReporter.AddIdle(this);
				return;
			case 1:
				Log.ReceivedInvalidSession(m_logger, Id, session.Id);
				break;
			default:
				Log.ReceivedExpiredSession(m_logger, Id, session.Id);
				break;
			}
			AdjustHostConnectionCount(session, -1);
			await session.DisposeAsync(ioBehavior, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			m_sessionSemaphore.Release();
		}
	}

	public async Task ClearAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.ClearingConnectionPool(m_logger, Id);
		Interlocked.Increment(ref m_generation);
		m_procedureCache = null;
		await RecoverLeakedSessionsAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		await CleanPoolAsync(ioBehavior, (ServerSession session) => session.PoolGeneration != m_generation, respectMinPoolSize: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task ReapAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		Log.ReapingConnectionPool(m_logger, Id);
		await RecoverLeakedSessionsAsync(ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
		await CleanPoolAsync(ioBehavior, (ServerSession session) => Utility.GetElapsedMilliseconds(session.LastReturnedTimestamp) / 1000 >= ConnectionSettings.ConnectionIdleTimeout, respectMinPoolSize: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2 })]
	public Dictionary<string, CachedProcedure> GetProcedureCache()
	{
		Dictionary<string, CachedProcedure> dictionary = m_procedureCache;
		if (dictionary == null)
		{
			Dictionary<string, CachedProcedure> dictionary2 = new Dictionary<string, CachedProcedure>();
			dictionary = Interlocked.CompareExchange(ref m_procedureCache, dictionary2, null) ?? dictionary2;
		}
		return dictionary;
	}

	public void Dispose()
	{
		Log.DisposingConnectionPool(m_logger, Id);
		lock (s_allPools)
		{
			s_allPools.Remove(this);
		}
		if (m_dnsCheckTimer != null)
		{
			using ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			m_dnsCheckTimer.Dispose(manualResetEvent);
			manualResetEvent.WaitOne();
			m_dnsCheckTimer = null;
		}
		if (m_reaperTimer != null)
		{
			using (ManualResetEvent manualResetEvent2 = new ManualResetEvent(initialState: false))
			{
				m_reaperTimer.Dispose(manualResetEvent2);
				manualResetEvent2.WaitOne();
				m_reaperTimer = null;
			}
		}
	}

	private async Task RecoverLeakedSessionsAsync(IOBehavior ioBehavior)
	{
		List<(ServerSession, MySqlConnection)> list = new List<(ServerSession, MySqlConnection)>();
		lock (m_leasedSessions)
		{
			m_lastRecoveryTime = (uint)Environment.TickCount;
			foreach (ServerSession value in m_leasedSessions.Values)
			{
				if (!value.OwningConnection.TryGetTarget(out var _))
				{
					MySqlConnection mySqlConnection = new MySqlConnection();
					value.OwningConnection = new WeakReference<MySqlConnection>(mySqlConnection);
					list.Add((value, mySqlConnection));
				}
			}
		}
		if (list.Count == 0)
		{
			Log.RecoveredNoSessions(m_logger, Id);
		}
		else
		{
			Log.RecoveredSessionCount(m_logger, Id, list.Count);
		}
		foreach (var (serverSession, connection) in list)
		{
			await serverSession.ReturnToPoolAsync(ioBehavior, null).ConfigureAwait(continueOnCapturedContext: false);
			GC.KeepAlive(connection);
		}
	}

	private async Task CleanPoolAsync(IOBehavior ioBehavior, Func<ServerSession, bool> shouldCleanFn, bool respectMinPoolSize, CancellationToken cancellationToken)
	{
		if (ioBehavior == IOBehavior.Asynchronous)
		{
			await m_cleanSemaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			m_cleanSemaphore.Wait(cancellationToken);
		}
		try
		{
			TimeSpan waitTimeout = TimeSpan.FromMilliseconds(10.0);
			while (true)
			{
				if (respectMinPoolSize)
				{
					lock (m_sessions)
					{
						if (ConnectionSettings.MaximumPoolSize - m_sessionSemaphore.CurrentCount + m_sessions.Count <= ConnectionSettings.MinimumPoolSize)
						{
							break;
						}
					}
				}
				if (ioBehavior == IOBehavior.Asynchronous)
				{
					if (!(await m_sessionSemaphore.WaitAsync(waitTimeout, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						break;
					}
				}
				else if (!m_sessionSemaphore.Wait(waitTimeout, cancellationToken))
				{
					break;
				}
				try
				{
					ServerSession serverSession = null;
					lock (m_sessions)
					{
						if (m_sessions.Count > 0)
						{
							serverSession = m_sessions.Last.Value;
							m_sessions.RemoveLast();
						}
					}
					if (serverSession == null)
					{
						break;
					}
					MetricsReporter.RemoveIdle(this);
					if (shouldCleanFn(serverSession))
					{
						Log.FoundSessionToCleanUp(m_logger, Id, serverSession.Id);
						await serverSession.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						continue;
					}
					lock (m_sessions)
					{
						m_sessions.AddLast(serverSession);
					}
					MetricsReporter.AddIdle(this);
					break;
				}
				finally
				{
					m_sessionSemaphore.Release();
				}
			}
		}
		finally
		{
			m_cleanSemaphore.Release();
		}
	}

	private async Task CreateMinimumPooledSessions(MySqlConnection connection, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		while (true)
		{
			lock (m_sessions)
			{
				if (ConnectionSettings.MaximumPoolSize - m_sessionSemaphore.CurrentCount + m_sessions.Count >= ConnectionSettings.MinimumPoolSize)
				{
					break;
				}
			}
			if (ioBehavior == IOBehavior.Asynchronous)
			{
				if (!(await m_sessionSemaphore.WaitAsync(0, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					break;
				}
			}
			else if (!m_sessionSemaphore.Wait(0, cancellationToken))
			{
				break;
			}
			try
			{
				ServerSession serverSession = await ConnectSessionAsync(connection, s_createdToReachMinimumPoolSize, Stopwatch.GetTimestamp(), null, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				AdjustHostConnectionCount(serverSession, 1);
				lock (m_sessions)
				{
					m_sessions.AddFirst(serverSession);
				}
				MetricsReporter.AddIdle(this);
			}
			finally
			{
				m_sessionSemaphore.Release();
			}
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	private async ValueTask<ServerSession> ConnectSessionAsync(MySqlConnection connection, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 1, 2 })] Action<ILogger, int, string, Exception> logMessage, long startingTimestamp, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		ServerSession session = new ServerSession(m_connectionLogger, this, m_generation, Interlocked.Increment(ref m_lastSessionId));
		if (m_logger.IsEnabled(LogLevel.Debug))
		{
			logMessage(m_logger, Id, session.Id, null);
		}
		string statusInfo;
		try
		{
			statusInfo = await session.ConnectAsync(ConnectionSettings, connection, startingTimestamp, m_loadBalancer, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception)
		{
			await session.DisposeAsync(ioBehavior, default(CancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
			throw;
		}
		Exception redirectionException = null;
		if (statusInfo != null && statusInfo.StartsWith("Location: mysql://", StringComparison.Ordinal))
		{
			Log.HasServerRedirectionHeader(m_logger, session.Id, statusInfo);
			string host;
			int port;
			string user;
			if (ConnectionSettings.ServerRedirectionMode == MySqlServerRedirectionMode.Disabled)
			{
				Log.ServerRedirectionIsDisabled(m_logger, Id);
			}
			else if (Utility.TryParseRedirectionHeader(statusInfo, out host, out port, out user))
			{
				if (host != ConnectionSettings.HostNames[0] || port != ConnectionSettings.Port || user != ConnectionSettings.UserID)
				{
					ConnectionSettings cs = ConnectionSettings.CloneWith(host, port, user);
					Log.OpeningNewConnection(m_logger, Id, host, port, user);
					ServerSession redirectedSession = new ServerSession(m_connectionLogger, this, m_generation, Interlocked.Increment(ref m_lastSessionId));
					try
					{
						await redirectedSession.ConnectAsync(cs, connection, startingTimestamp, m_loadBalancer, activity, ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (Exception ex2)
					{
						Log.FailedToConnectRedirectedSession(m_logger, ex2, Id, redirectedSession.Id);
						redirectionException = ex2;
					}
					if (redirectionException == null)
					{
						Log.ClosingSessionToUseRedirectedSession(m_logger, Id, session.Id, redirectedSession.Id);
						await session.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						return redirectedSession;
					}
					try
					{
						await redirectedSession.DisposeAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (Exception)
					{
					}
				}
				else
				{
					Log.SessionAlreadyConnectedToServer(m_logger, session.Id);
				}
			}
		}
		if (ConnectionSettings.ServerRedirectionMode == MySqlServerRedirectionMode.Required)
		{
			Log.RequiresServerRedirection(m_logger, Id);
			throw new MySqlException(MySqlErrorCode.UnableToConnectToHost, "Server does not support redirection", redirectionException);
		}
		return session;
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public static ConnectionPool CreatePool(string connectionString, MySqlConnectorLoggingConfiguration loggingConfiguration, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string name)
	{
		MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
		if (!mySqlConnectionStringBuilder.Pooling)
		{
			return null;
		}
		if (name != null)
		{
			mySqlConnectionStringBuilder.ApplicationName = name;
		}
		ConnectionSettings cs = new ConnectionSettings(mySqlConnectionStringBuilder);
		ConnectionPool connectionPool = new ConnectionPool(loggingConfiguration, cs);
		connectionPool.StartReaperTask();
		connectionPool.StartDnsCheckTimer();
		return connectionPool;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public static ConnectionPool GetPool([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string connectionString, MySqlConnectorLoggingConfiguration loggingConfiguration, bool createIfNotFound = true)
	{
		ConnectionStringPool connectionStringPool = s_mruCache;
		if (connectionStringPool?.ConnectionString == connectionString)
		{
			return connectionStringPool.Pool;
		}
		if (s_pools.TryGetValue(connectionString, out var value))
		{
			s_mruCache = new ConnectionStringPool(connectionString, value);
			return value;
		}
		MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
		if (!mySqlConnectionStringBuilder.Pooling)
		{
			s_pools.GetOrAdd(connectionString, (ConnectionPool)null);
			s_mruCache = new ConnectionStringPool(connectionString, null);
			return null;
		}
		string connectionString2 = mySqlConnectionStringBuilder.ConnectionString;
		if (connectionString2 != connectionString && s_pools.TryGetValue(connectionString2, out value))
		{
			value = s_pools.GetOrAdd(connectionString, value);
			s_mruCache = new ConnectionStringPool(connectionString, value);
			return value;
		}
		if (!createIfNotFound)
		{
			return null;
		}
		ConnectionSettings cs = new ConnectionSettings(mySqlConnectionStringBuilder);
		ConnectionPool connectionPool = new ConnectionPool(loggingConfiguration, cs);
		value = s_pools.GetOrAdd(connectionString2, connectionPool);
		if (value == connectionPool)
		{
			s_mruCache = new ConnectionStringPool(connectionString, value);
			value.StartReaperTask();
			value.StartDnsCheckTimer();
			if (connectionString != connectionString2)
			{
				s_pools.GetOrAdd(connectionString, value);
			}
		}
		else if (value != connectionPool)
		{
			Log.CreatedPoolWillNotBeUsed(connectionPool.m_logger, connectionPool.Id);
			connectionPool.Dispose();
		}
		return value;
	}

	public static async Task ClearPoolsAsync(IOBehavior ioBehavior, CancellationToken cancellationToken)
	{
		foreach (ConnectionPool cachedPool in GetCachedPools())
		{
			await cachedPool.ClearAsync(ioBehavior, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		static List<ConnectionPool> GetCachedPools()
		{
			List<ConnectionPool> list = new List<ConnectionPool>(s_pools.Count);
			HashSet<ConnectionPool> hashSet = new HashSet<ConnectionPool>();
			foreach (ConnectionPool value in s_pools.Values)
			{
				if (value != null && hashSet.Add(value))
				{
					list.Add(value);
				}
			}
			return list;
		}
	}

	private ConnectionPool(MySqlConnectorLoggingConfiguration loggingConfiguration, ConnectionSettings cs)
	{
		m_logger = loggingConfiguration.PoolLogger;
		m_connectionLogger = loggingConfiguration.ConnectionLogger;
		ConnectionSettings = cs;
		Name = cs.ApplicationName;
		m_generation = 0;
		m_cleanSemaphore = new SemaphoreSlim(1);
		m_sessionSemaphore = new SemaphoreSlim(cs.MaximumPoolSize);
		m_sessions = new LinkedList<ServerSession>();
		m_leasedSessions = new Dictionary<string, ServerSession>();
		if (cs.ConnectionProtocol == MySqlConnectionProtocol.Sockets && cs.LoadBalance == MySqlLoadBalance.LeastConnections)
		{
			m_hostSessions = new Dictionary<string, int>();
			foreach (string hostName in cs.HostNames)
			{
				m_hostSessions[hostName] = 0;
			}
		}
		object loadBalancer2;
		if (cs.ConnectionProtocol == MySqlConnectionProtocol.Sockets)
		{
			if (cs.HostNames.Count != 1 && cs.LoadBalance != MySqlLoadBalance.FailOver)
			{
				if (cs.LoadBalance != MySqlLoadBalance.Random)
				{
					if (cs.LoadBalance != MySqlLoadBalance.LeastConnections)
					{
						ILoadBalancer loadBalancer = new RoundRobinLoadBalancer();
						loadBalancer2 = loadBalancer;
					}
					else
					{
						ILoadBalancer loadBalancer = new LeastConnectionsLoadBalancer(m_hostSessions);
						loadBalancer2 = loadBalancer;
					}
				}
				else
				{
					loadBalancer2 = RandomLoadBalancer.Instance;
				}
			}
			else
			{
				loadBalancer2 = FailOverLoadBalancer.Instance;
			}
		}
		else
		{
			loadBalancer2 = null;
		}
		m_loadBalancer = (ILoadBalancer)loadBalancer2;
		string connectionString = cs.ConnectionStringBuilder.GetConnectionString(includePassword: false);
		m_stateTagList = new KeyValuePair<string, object>[3]
		{
			new KeyValuePair<string, object>("state", "idle"),
			new KeyValuePair<string, object>("pool.name", Name ?? connectionString),
			new KeyValuePair<string, object>("state", "used")
		};
		Id = Interlocked.Increment(ref s_poolId);
		lock (s_allPools)
		{
			s_allPools.Add(this);
		}
		Log.CreatingNewConnectionPool(m_logger, Id, connectionString);
	}

	private void StartReaperTask()
	{
		if (ConnectionSettings.ConnectionIdleTimeout <= 0)
		{
			return;
		}
		TimeSpan reaperInterval = TimeSpan.FromSeconds((double)Math.Max(1, Math.Min(60, ConnectionSettings.ConnectionIdleTimeout / 2)));
		m_reaperTimer = new Timer([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (object t) =>
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(reaperInterval);
				ReapAsync(IOBehavior.Synchronous, cancellationTokenSource.Token).GetAwaiter().GetResult();
			}
			catch
			{
			}
			TimeSpan timeSpan = reaperInterval - stopwatch.Elapsed;
			((Timer)t).Change((timeSpan < TimeSpan.Zero) ? TimeSpan.Zero : timeSpan, TimeSpan.FromMilliseconds(-1.0));
		});
		m_reaperTimer.Change(reaperInterval, TimeSpan.FromMilliseconds(-1.0));
	}

	private void StartDnsCheckTimer()
	{
		if (ConnectionSettings.ConnectionProtocol != MySqlConnectionProtocol.Sockets || ConnectionSettings.DnsCheckInterval <= 0)
		{
			return;
		}
		IReadOnlyList<string> hostNames = ConnectionSettings.HostNames;
		IPAddress[][] hostAddresses = new IPAddress[hostNames.Count][];
		int interval = Math.Min(2147483, ConnectionSettings.DnsCheckInterval) * 1000;
		m_dnsCheckTimer = new Timer([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (object t) =>
		{
			Log.CheckingForDnsChanges(m_logger, Id);
			bool flag = false;
			for (int i = 0; i < hostNames.Count; i++)
			{
				try
				{
					IPAddress[] hostAddresses2 = Dns.GetHostAddresses(hostNames[i]);
					if (hostAddresses[i] == null)
					{
						hostAddresses[i] = hostAddresses2;
					}
					else if (hostAddresses[i].Except(hostAddresses2).Any())
					{
						Log.DetectedDnsChange(m_logger, Id, hostNames[i], string.Join(",", (IEnumerable<IPAddress>)hostAddresses[i]), string.Join(",", (IEnumerable<IPAddress>)hostAddresses2));
						hostAddresses[i] = hostAddresses2;
						flag = true;
					}
				}
				catch (Exception ex)
				{
					Log.DnsCheckFailed(m_logger, ex, Id, hostNames[i], ex.Message);
				}
			}
			if (flag)
			{
				Log.ClearingPoolDueToDnsChanges(m_logger, Id);
				ClearAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
			}
			((Timer)t).Change(interval, -1);
		});
		m_dnsCheckTimer.Change(interval, -1);
	}

	private void AdjustHostConnectionCount(ServerSession session, int delta)
	{
		if (m_hostSessions != null)
		{
			lock (m_hostSessions)
			{
				m_hostSessions[session.HostName] += delta;
			}
		}
	}

	public static List<ConnectionPool> GetAllPools()
	{
		lock (s_allPools)
		{
			return new List<ConnectionPool>(s_allPools);
		}
	}

	static ConnectionPool()
	{
		s_pools = new ConcurrentDictionary<string, ConnectionPool>();
		s_allPools = new List<ConnectionPool>();
		s_createdNewSession = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(3018, "PoolCreatedNewSession"), "Pool {PoolId} has no pooled session available; created new session {SessionId}");
		s_createdToReachMinimumPoolSize = LoggerMessage.Define<int, string>(LogLevel.Debug, new EventId(3019, "CreatedSessionToReachMinimumPoolCount"), "Pool {PoolId} created session {SessionId} to reach minimum pool size");
		AppDomain.CurrentDomain.DomainUnload += OnAppDomainShutDown;
		AppDomain.CurrentDomain.ProcessExit += OnAppDomainShutDown;
	}

	private static void OnAppDomainShutDown([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] object sender, EventArgs e)
	{
		ClearPoolsAsync(IOBehavior.Synchronous, CancellationToken.None).GetAwaiter().GetResult();
	}
}
