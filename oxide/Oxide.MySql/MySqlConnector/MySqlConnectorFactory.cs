using System.Data.Common;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlConnectorFactory : DbProviderFactory
{
	public static readonly MySqlConnectorFactory Instance = new MySqlConnectorFactory();

	public override bool CanCreateDataSourceEnumerator => false;

	public bool CanCreateBatch => true;

	public override DbCommand CreateCommand()
	{
		return new MySqlCommand();
	}

	public override DbConnection CreateConnection()
	{
		return new MySqlConnection();
	}

	public override DbConnectionStringBuilder CreateConnectionStringBuilder()
	{
		return new MySqlConnectionStringBuilder();
	}

	public override DbParameter CreateParameter()
	{
		return new MySqlParameter();
	}

	public override DbCommandBuilder CreateCommandBuilder()
	{
		return new MySqlCommandBuilder();
	}

	public override DbDataAdapter CreateDataAdapter()
	{
		return new MySqlDataAdapter();
	}

	public MySqlBatch CreateBatch()
	{
		return new MySqlBatch();
	}

	public MySqlBatchCommand CreateBatchCommand()
	{
		return new MySqlBatchCommand();
	}

	public DbDataSource CreateDataSource(string connectionString)
	{
		return new MySqlDataSource(connectionString);
	}

	private MySqlConnectorFactory()
	{
	}
}
