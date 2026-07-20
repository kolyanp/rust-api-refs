using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace System.Runtime.CompilerServices;

[_003Cd1319d0b_002D8e9a_002D4832_002D93a5_002D07396d9c0bfd_003EIsReadOnly]
internal struct ValueTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
{
	internal static readonly Action<object> s_invokeActionDelegate = delegate(object state)
	{
		if (!(state is Action action))
		{
			_003Cf9b47b20_002Df8fc_002D4539_002Db746_002D9c209e9728ac_003EThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.state);
		}
		else
		{
			action();
		}
	};

	private readonly ValueTask _value;

	public bool IsCompleted
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _value.IsCompleted;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ValueTaskAwaiter(ValueTask value)
	{
		_value = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public void GetResult()
	{
		_value.ThrowIfCompletedUnsuccessfully();
	}

	public void OnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task task)
		{
			task.GetAwaiter().OnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
		}
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task task)
		{
			task.GetAwaiter().UnsafeOnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
		}
	}
}
[_003Cd1319d0b_002D8e9a_002D4832_002D93a5_002D07396d9c0bfd_003EIsReadOnly]
internal struct ValueTaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
{
	private readonly ValueTask<TResult> _value;

	public bool IsCompleted
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _value.IsCompleted;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal ValueTaskAwaiter(ValueTask<TResult> value)
	{
		_value = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[StackTraceHidden]
	public TResult GetResult()
	{
		return _value.Result;
	}

	public void OnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task<TResult> task)
		{
			task.GetAwaiter().OnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().OnCompleted(continuation);
		}
	}

	public void UnsafeOnCompleted(Action continuation)
	{
		object obj = _value._obj;
		if (obj is Task<TResult> task)
		{
			task.GetAwaiter().UnsafeOnCompleted(continuation);
		}
		else if (obj != null)
		{
			Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
		}
		else
		{
			ValueTask.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
		}
	}
}
