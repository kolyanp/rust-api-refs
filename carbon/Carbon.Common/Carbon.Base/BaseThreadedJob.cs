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
		if (Community.IsServerInitialized)
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
		ThreadFunction();
		IsDone = true;
	}

	public virtual void Dispose()
	{
	}
}
