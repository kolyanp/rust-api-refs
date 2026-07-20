using System;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector.Logging;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlDataSourceBuilder
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private ILoggerFactory m_loggerFactory;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_name;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private Func<X509CertificateCollection, ValueTask> m_clientCertificatesCallback;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private RemoteCertificateValidationCallback m_remoteCertificateValidationCallback;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 0, 1 })]
	private Func<MySqlProvidePasswordContext, CancellationToken, ValueTask<string>> m_periodicPasswordProvider;

	private TimeSpan m_periodicPasswordProviderSuccessRefreshInterval;

	private TimeSpan m_periodicPasswordProviderFailureRefreshInterval;

	public MySqlConnectionStringBuilder ConnectionStringBuilder { get; }

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public MySqlDataSourceBuilder(string connectionString = null)
	{
		ConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString ?? "");
	}

	public MySqlDataSourceBuilder UseLoggerFactory([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] ILoggerFactory loggerFactory)
	{
		m_loggerFactory = loggerFactory;
		return this;
	}

	public MySqlDataSourceBuilder UseName([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string name)
	{
		m_name = name;
		return this;
	}

	public MySqlDataSourceBuilder UseClientCertificatesCallback([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })] Func<X509CertificateCollection, ValueTask> callback)
	{
		m_clientCertificatesCallback = callback;
		return this;
	}

	public MySqlDataSourceBuilder UsePeriodicPasswordProvider([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1, 0, 1 })] Func<MySqlProvidePasswordContext, CancellationToken, ValueTask<string>> passwordProvider, TimeSpan successRefreshInterval, TimeSpan failureRefreshInterval)
	{
		m_periodicPasswordProvider = passwordProvider;
		m_periodicPasswordProviderSuccessRefreshInterval = successRefreshInterval;
		m_periodicPasswordProviderFailureRefreshInterval = failureRefreshInterval;
		return this;
	}

	public MySqlDataSourceBuilder UseRemoteCertificateValidationCallback(RemoteCertificateValidationCallback callback)
	{
		m_remoteCertificateValidationCallback = callback;
		return this;
	}

	public MySqlDataSource Build()
	{
		MySqlConnectorLoggingConfiguration loggingConfiguration = ((m_loggerFactory == null) ? MySqlConnectorLoggingConfiguration.NullConfiguration : new MySqlConnectorLoggingConfiguration(m_loggerFactory));
		return new MySqlDataSource(ConnectionStringBuilder.ConnectionString, loggingConfiguration, m_name, m_clientCertificatesCallback, m_remoteCertificateValidationCallback, m_periodicPasswordProvider, m_periodicPasswordProviderSuccessRefreshInterval, m_periodicPasswordProviderFailureRefreshInterval);
	}
}
