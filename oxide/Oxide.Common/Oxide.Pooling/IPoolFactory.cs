using System;

namespace Oxide.Pooling;

public interface IPoolFactory
{
	IPoolProvider<T> GetProvider<T>();

	bool IsHandledType<T>();

	IDisposable RegisterProvider<TProvider>(out TProvider provider, params object[] args) where TProvider : IPoolProvider;
}
