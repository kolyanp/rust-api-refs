using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Common;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal abstract class DbDataSource : IDisposable
{
	public abstract string ConnectionString { get; }

	public DbConnection CreateConnection()
	{
		return CreateDbConnection();
	}

	public DbConnection OpenConnection()
	{
		return OpenDbConnection();
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return OpenDbConnectionAsync(cancellationToken);
	}

	public DbCommand CreateCommand([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string commandText = null)
	{
		return CreateDbCommand(commandText);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(continueOnCapturedContext: false);
		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected abstract DbConnection CreateDbConnection();

	protected virtual DbConnection OpenDbConnection()
	{
		DbConnection dbConnection = CreateDbConnection();
		try
		{
			dbConnection.Open();
			return dbConnection;
		}
		catch
		{
			dbConnection.Dispose();
			throw;
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	protected virtual async ValueTask<DbConnection> OpenDbConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		DbConnection connection = CreateDbConnection();
		try
		{
			await connection.OpenAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return connection;
		}
		catch
		{
			connection.Dispose();
			throw;
		}
	}

	protected virtual DbCommand CreateDbCommand([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string commandText = null)
	{
		throw new NotSupportedException();
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	protected virtual ValueTask DisposeAsyncCore()
	{
		return default(ValueTask);
	}
}
