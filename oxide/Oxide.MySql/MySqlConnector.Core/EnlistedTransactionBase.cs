using System;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal abstract class EnlistedTransactionBase(Transaction transaction, MySqlConnection connection) : IEnlistmentNotification
{
	public MySqlConnection Connection { get; set; } = connection;

	public bool IsIdle { get; set; }

	public bool IsPrepared { get; private set; }

	public Transaction Transaction { get; private set; } = transaction;

	public void Start()
	{
		OnStart();
		Transaction.EnlistVolatile(this, EnlistmentOptions.None);
	}

	void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
	{
		try
		{
			OnPrepare(preparingEnlistment);
			IsPrepared = true;
			preparingEnlistment.Prepared();
		}
		catch (Exception e)
		{
			preparingEnlistment.ForceRollback(e);
		}
	}

	void IEnlistmentNotification.Commit(Enlistment enlistment)
	{
		OnCommit(enlistment);
		enlistment.Done();
		Connection.UnenlistTransaction();
	}

	void IEnlistmentNotification.Rollback(Enlistment enlistment)
	{
		OnRollback(enlistment);
		enlistment.Done();
		Connection.UnenlistTransaction();
	}

	public void InDoubt(Enlistment enlistment)
	{
		throw new NotImplementedException();
	}

	protected abstract void OnStart();

	protected abstract void OnPrepare(PreparingEnlistment enlistment);

	protected abstract void OnCommit(Enlistment enlistment);

	protected abstract void OnRollback(Enlistment enlistment);
}
