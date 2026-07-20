using System;
using System.Data.Common;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Core;
using MySqlConnector.Logging;
using MySqlConnector.Protocol.Serialization;

namespace MySqlConnector;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class MySqlDataSource : DbDataSource
{
	private static int s_lastId;

	private readonly ILogger m_logger;

	private readonly int m_id;

	private readonly string m_connectionString;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private readonly Func<X509CertificateCollection, ValueTask> m_clientCertificatesCallback;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly RemoteCertificateValidationCallback m_remoteCertificateValidationCallback;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 0, 1 })]
	private readonly Func<MySqlProvidePasswordContext, CancellationToken, ValueTask<string>> m_periodicPasswordProvider;

	private readonly TimeSpan m_periodicPasswordProviderSuccessRefreshInterval;

	private readonly TimeSpan m_periodicPasswordProviderFailureRefreshInterval;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly MySqlProvidePasswordContext m_providePasswordContext;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly CancellationTokenSource m_passwordProviderTimerCancellationTokenSource;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly Timer m_passwordProviderTimer;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private readonly Task m_initialPasswordRefreshTask;

	private bool m_isDisposed;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_password;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 1 })]
	private Func<MySqlProvidePasswordContext, string> m_providePasswordCallback;

	public override string ConnectionString => m_connectionString;

	public string Password
	{
		set
		{
			if (m_periodicPasswordProvider != null)
			{
				throw new InvalidOperationException("Cannot set Password when this MySqlDataSource is configured with a PeriodicPasswordProvider.");
			}
			m_password = value ?? throw new ArgumentNullException("value");
			m_providePasswordCallback = ProvidePasswordFromField;
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal ConnectionPool Pool
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	internal MySqlConnectorLoggingConfiguration LoggingConfiguration { get; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	[field: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	internal string Name
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		get;
	}

	public MySqlDataSource(string connectionString)
		: this(connectionString ?? throw new ArgumentNullException("connectionString"), MySqlConnectorLoggingConfiguration.NullConfiguration, null, null, null, null, default(TimeSpan), default(TimeSpan))
	{
	}

	internal MySqlDataSource(string connectionString, MySqlConnectorLoggingConfiguration loggingConfiguration, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string name, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })] Func<X509CertificateCollection, ValueTask> clientCertificatesCallback, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] RemoteCertificateValidationCallback remoteCertificateValidationCallback, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 0, 1 })] Func<MySqlProvidePasswordContext, CancellationToken, ValueTask<string>> periodicPasswordProvider, TimeSpan periodicPasswordProviderSuccessRefreshInterval, TimeSpan periodicPasswordProviderFailureRefreshInterval)
	{
		m_connectionString = connectionString;
		LoggingConfiguration = loggingConfiguration;
		Name = name;
		m_clientCertificatesCallback = clientCertificatesCallback;
		m_remoteCertificateValidationCallback = remoteCertificateValidationCallback;
		m_logger = loggingConfiguration.DataSourceLogger;
		Pool = ConnectionPool.CreatePool(m_connectionString, LoggingConfiguration, name);
		m_id = Interlocked.Increment(ref s_lastId);
		if (Pool != null && Name != null)
		{
			Log.DataSourceCreatedWithPoolWithName(m_logger, m_id, Pool.Id, Name);
		}
		else if (Pool != null)
		{
			Log.DataSourceCreatedWithPoolWithoutName(m_logger, m_id, Pool.Id);
		}
		else if (Name != null)
		{
			Log.DataSourceCreatedWithoutPoolWithName(m_logger, m_id, Name);
		}
		else
		{
			Log.DataSourceCreatedWithoutPoolWithoutName(m_logger, m_id);
		}
		if (periodicPasswordProvider != null)
		{
			m_periodicPasswordProvider = periodicPasswordProvider;
			m_periodicPasswordProviderSuccessRefreshInterval = periodicPasswordProviderSuccessRefreshInterval;
			m_periodicPasswordProviderFailureRefreshInterval = periodicPasswordProviderFailureRefreshInterval;
			m_passwordProviderTimerCancellationTokenSource = new CancellationTokenSource();
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(m_connectionString);
			m_providePasswordContext = new MySqlProvidePasswordContext(mySqlConnectionStringBuilder.Server, (int)mySqlConnectionStringBuilder.Port, mySqlConnectionStringBuilder.UserID, mySqlConnectionStringBuilder.Database);
			m_passwordProviderTimer = new Timer([_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (object _) =>
			{
				_ = RefreshPassword();
			}, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
			m_initialPasswordRefreshTask = Task.Run((Func<Task?>)RefreshPassword);
			m_providePasswordCallback = ProvidePasswordFromInitialRefreshTask;
		}
	}

	public new MySqlConnection CreateConnection()
	{
		return (MySqlConnection)base.CreateConnection();
	}

	public new MySqlConnection OpenConnection()
	{
		return (MySqlConnection)base.OpenConnection();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public new async ValueTask<MySqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (MySqlConnection)(await base.OpenConnectionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	protected override DbConnection CreateDbConnection()
	{
		if (m_isDisposed)
		{
			throw new ObjectDisposedException("MySqlDataSource");
		}
		return new MySqlConnection(this)
		{
			ProvideClientCertificatesCallback = m_clientCertificatesCallback,
			ProvidePasswordCallback = m_providePasswordCallback,
			RemoteCertificateValidationCallback = m_remoteCertificateValidationCallback
		};
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				DisposeAsync(IOBehavior.Synchronous).GetAwaiter().GetResult();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	protected override ValueTask DisposeAsyncCore()
	{
		return DisposeAsync(IOBehavior.Asynchronous);
	}

	private async ValueTask DisposeAsync(IOBehavior ioBehavior)
	{
		CancellationTokenSource passwordProviderTimerCancellationTokenSource = m_passwordProviderTimerCancellationTokenSource;
		if (passwordProviderTimerCancellationTokenSource != null)
		{
			passwordProviderTimerCancellationTokenSource.Cancel();
			passwordProviderTimerCancellationTokenSource.Dispose();
		}
		if (Pool != null)
		{
			await Pool.ClearAsync(ioBehavior, default(CancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
			Pool.Dispose();
		}
		m_isDisposed = true;
	}

	private async Task RefreshPassword()
	{
		try
		{
			m_password = await m_periodicPasswordProvider(m_providePasswordContext, m_passwordProviderTimerCancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
			m_providePasswordCallback = ProvidePasswordFromField;
			m_passwordProviderTimer.Change(m_periodicPasswordProviderSuccessRefreshInterval, Timeout.InfiniteTimeSpan);
		}
		catch (Exception ex)
		{
			Log.PeriodicPasswordProviderFailed(m_logger, ex, m_id, ex.Message);
			m_passwordProviderTimer.Change(m_periodicPasswordProviderFailureRefreshInterval, Timeout.InfiniteTimeSpan);
			throw new MySqlException("The periodic password provider failed", ex);
		}
	}

	private string ProvidePasswordFromField(MySqlProvidePasswordContext context)
	{
		return m_password;
	}

	private string ProvidePasswordFromInitialRefreshTask(MySqlProvidePasswordContext context)
	{
		if (m_password == null)
		{
			m_initialPasswordRefreshTask.GetAwaiter().GetResult();
			m_providePasswordCallback = ProvidePasswordFromField;
		}
		return m_password;
	}
}
