using System;
using System.Threading;

namespace Microsoft.Extensions.Logging;

internal class LoggerExternalScopeProvider : IExternalScopeProvider
{
	private sealed class Scope : IDisposable
	{
		private readonly LoggerExternalScopeProvider _provider;

		private bool _isDisposed;

		public Scope Parent { get; }

		public object State { get; }

		internal Scope(LoggerExternalScopeProvider provider, object state, Scope parent)
		{
			_provider = provider;
			State = state;
			Parent = parent;
		}

		public override string ToString()
		{
			return State?.ToString();
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_provider._currentScope.Value = Parent;
				_isDisposed = true;
			}
		}
	}

	private readonly AsyncLocal<Scope> _currentScope = new AsyncLocal<Scope>();

	public void ForEachScope<TState>(Action<object?, TState> callback, TState state)
	{
		Report(_currentScope.Value);
		void Report(Scope? current)
		{
			if (current != null)
			{
				Report(current.Parent);
				callback(current.State, state);
			}
		}
	}

	public IDisposable Push(object? state)
	{
		Scope value = _currentScope.Value;
		Scope scope = new Scope(this, state, value);
		_currentScope.Value = scope;
		return scope;
	}
}
