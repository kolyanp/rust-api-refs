using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Carbon.Base;

public class BaseThreadedJob : IDisposable
{
	internal bool _isDone;

	internal object _handle = new object();

	internal Task _task;

	private CancellationTokenSource cancellationToken;

	private volatile bool _isAborted;

	internal readonly object _abortHandle = new object();

	public CancellationToken CancellationToken => cancellationToken?.Token ?? CancellationToken.None;

	public bool IsAborted => _isAborted;

	public bool IsDone
	{
		get
		{
			lock (_handle)
			{
				return _isDone;
			}
		}
		set
		{
			lock (_handle)
			{
				_isDone = value;
			}
		}
	}

	public virtual void Start()
	{
		if (IsAborted)
		{
			IsDone = true;
		}
		else if (Community.IsServerInitialized)
		{
			cancellationToken = new CancellationTokenSource();
			_task = Task.Factory.StartNew(Run, cancellationToken.Token);
		}
		else
		{
			Run();
		}
	}

	public virtual void Abort()
	{
		lock (_abortHandle)
		{
			_isAborted = true;
		}
		cancellationToken?.Cancel();
	}

	public virtual void ThreadFunction()
	{
	}

	public virtual void OnFinished()
	{
	}

	public virtual bool Update()
	{
		if (IsDone)
		{
			OnFinished();
			return true;
		}
		return false;
	}

	public IEnumerator WaitFor()
	{
		while (!Update())
		{
			yield return null;
		}
	}

	private void Run()
	{
		try
		{
			ThreadFunction();
		}
		finally
		{
			IsDone = true;
		}
	}

	public virtual void Dispose()
	{
	}
}
