using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Facepunch;
using Rust.Assertions;

public sealed class ClanAsyncResult<T> : IPooled
{
	private readonly Stopwatch _sinceStarted = new Stopwatch();

	private AutoResetUniTaskCompletionSource<T> _completionSource;

	private bool _gotTask;

	public bool IsStarted => _sinceStarted.IsRunning;

	public float Elapsed => (float)_sinceStarted.Elapsed.TotalSeconds;

	public bool IsComplete
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if (_completionSource != null)
			{
				return (int)_completionSource.UnsafeGetStatus() > 0;
			}
			return false;
		}
	}

	public UniTask<T> Task
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			if (_completionSource == null)
			{
				throw new InvalidOperationException("Task is not started yet. Call Start() first.");
			}
			_gotTask = true;
			return _completionSource.Task;
		}
	}

	public void Start()
	{
		_sinceStarted.Restart();
		_completionSource = AutoResetUniTaskCompletionSource<T>.Create();
	}

	public bool TrySetResult(T result)
	{
		if (!_completionSource.TrySetResult(result))
		{
			return false;
		}
		_sinceStarted.Stop();
		return true;
	}

	private void Reset()
	{
		Assert.That(_gotTask, "Task was not retrieved before reset. The task must be awaited to be pooled.");
		_completionSource = null;
		_gotTask = false;
		_sinceStarted.Reset();
	}

	void IPooled.EnterPool()
	{
		Reset();
	}

	void IPooled.LeavePool()
	{
	}
}
