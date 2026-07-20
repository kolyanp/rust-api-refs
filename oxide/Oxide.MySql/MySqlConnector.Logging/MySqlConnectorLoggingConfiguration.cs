using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MySqlConnector.Logging;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class MySqlConnectorLoggingConfiguration(ILoggerFactory loggerFactory)
{
	public ILogger DataSourceLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.MySqlDataSource");

	public ILogger ConnectionLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.MySqlConnection");

	public ILogger CommandLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.MySqlCommand");

	public ILogger PoolLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.ConnectionPool");

	public ILogger BulkCopyLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.MySqlBulkCopy");

	public ILogger TransactionLogger { get; } = loggerFactory.CreateLogger("MySqlConnector.Transaction");

	public static MySqlConnectorLoggingConfiguration NullConfiguration { get; } = new MySqlConnectorLoggingConfiguration(NullLoggerFactory.Instance);

	public static MySqlConnectorLoggingConfiguration GlobalConfiguration { get; set; } = NullConfiguration;
}
