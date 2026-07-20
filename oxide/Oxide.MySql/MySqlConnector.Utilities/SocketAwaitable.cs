using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MySqlConnector.Utilities;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class SocketAwaitable : INotifyCompletion
{
	private static readonly Action s_sentinel = delegate
	{
	};

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	private Action m_continuation;

	public bool IsCompleted => WasCompleted;

	internal bool WasCompleted { get; set; }

	internal SocketAsyncEventArgs EventArgs { get; }

	public SocketAwaitable(SocketAsyncEventArgs eventArgs)
	{
		EventArgs = eventArgs ?? throw new ArgumentNullException("eventArgs");
		eventArgs.Completed += [_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(0)] (object s, SocketAsyncEventArgs e) =>
		{
			(m_continuation ?? Interlocked.CompareExchange(ref m_continuation, s_sentinel, null))?.Invoke();
		};
	}

	public SocketAwaitable GetAwaiter()
	{
		return this;
	}

	public void OnCompleted(Action continuation)
	{
		if (m_continuation == s_sentinel || Interlocked.CompareExchange(ref m_continuation, continuation, null) == s_sentinel)
		{
			Task.Run(continuation);
		}
	}

	public void GetResult()
	{
		if (EventArgs.SocketError != SocketError.Success)
		{
			throw new SocketException((int)EventArgs.SocketError);
		}
	}

	internal void Reset()
	{
		WasCompleted = false;
		m_continuation = null;
	}
}
