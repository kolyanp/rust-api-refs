using System;

namespace Facepunch.Sqlite;

public class SqliteException : Exception
{
	public int Result { get; }

	public SqliteException(int result, string message)
		: base(message)
	{
		Result = result;
	}
}
