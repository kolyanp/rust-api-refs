using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Transactions;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal sealed class XaEnlistedTransaction : EnlistedTransactionBase
{
	private static int s_currentId;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private string m_xid;

	public XaEnlistedTransaction(Transaction transaction, MySqlConnection connection)
		: base(transaction, connection)
	{
	}

	protected override void OnStart()
	{
		int num = Interlocked.Increment(ref s_currentId);
		m_xid = "'" + base.Transaction.TransactionInformation.LocalIdentifier + "', '" + num.ToString(CultureInfo.InvariantCulture) + "'";
		ExecuteXaCommand("START");
	}

	protected override void OnPrepare(PreparingEnlistment enlistment)
	{
		ExecuteXaCommand("END");
		ExecuteXaCommand("PREPARE");
	}

	protected override void OnCommit(Enlistment enlistment)
	{
		ExecuteXaCommand("COMMIT");
	}

	protected override void OnRollback(Enlistment enlistment)
	{
		try
		{
			if (!base.IsPrepared)
			{
				ExecuteXaCommand("END");
			}
			ExecuteXaCommand("ROLLBACK");
		}
		catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.XARBDeadlock)
		{
		}
	}

	private void ExecuteXaCommand(string statement)
	{
		using MySqlCommand mySqlCommand = base.Connection.CreateCommand();
		mySqlCommand.CommandText = "XA " + statement + " " + m_xid;
		mySqlCommand.ExecuteNonQuery();
	}
}
