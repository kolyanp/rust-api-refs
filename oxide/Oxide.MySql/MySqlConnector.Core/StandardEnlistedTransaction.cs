using System;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class StandardEnlistedTransaction : EnlistedTransactionBase
{
	public StandardEnlistedTransaction(Transaction transaction, MySqlConnection connection)
		: base(transaction, connection)
	{
	}

	protected override void OnStart()
	{
		using MySqlCommand mySqlCommand = new MySqlCommand("set session transaction isolation level " + base.Transaction.IsolationLevel switch
		{
			IsolationLevel.Serializable => "serializable", 
			IsolationLevel.ReadCommitted => "read committed", 
			IsolationLevel.ReadUncommitted => "read uncommitted", 
			IsolationLevel.RepeatableRead => "repeatable read", 
			IsolationLevel.Snapshot => "repeatable read", 
			IsolationLevel.Chaos => throw new NotSupportedException($"IsolationLevel.{base.Transaction.IsolationLevel} is not supported."), 
			IsolationLevel.Unspecified => "repeatable read", 
			_ => "repeatable read", 
		} + ";", base.Connection);
		mySqlCommand.ExecuteNonQuery();
		string text = ((base.Transaction.IsolationLevel == IsolationLevel.Snapshot) ? " with consistent snapshot" : "");
		mySqlCommand.CommandText = "start transaction" + text + ";";
		mySqlCommand.ExecuteNonQuery();
	}

	protected override void OnPrepare(PreparingEnlistment enlistment)
	{
	}

	protected override void OnCommit(Enlistment enlistment)
	{
		using MySqlCommand mySqlCommand = new MySqlCommand("commit;", base.Connection);
		mySqlCommand.ExecuteNonQuery();
	}

	protected override void OnRollback(Enlistment enlistment)
	{
		using MySqlCommand mySqlCommand = new MySqlCommand("rollback;", base.Connection);
		mySqlCommand.ExecuteNonQuery();
	}
}
