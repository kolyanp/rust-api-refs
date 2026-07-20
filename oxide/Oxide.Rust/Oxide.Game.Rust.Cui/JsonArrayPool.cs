using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Pooling;

namespace Oxide.Game.Rust.Cui;

public sealed class JsonArrayPool<T> : IArrayPool<T>
{
	public static readonly JsonArrayPool<T> Shared = new JsonArrayPool<T>();

	private static readonly IArrayPoolProvider<T> Provider = GetOrCreateProvider();

	private static IArrayPoolProvider<T> GetOrCreateProvider()
	{
		if (Interface.Oxide.PoolFactory.IsHandledType<T[]>())
		{
			return Interface.Oxide.PoolFactory.GetArrayProvider<T>();
		}
		Interface.Oxide.PoolFactory.RegisterProvider<BaseArrayPoolProvider<T>>(out var provider, 1000, 16384);
		return provider;
	}

	public T[] Rent(int minimumLength)
	{
		return Provider.Take(minimumLength);
	}

	public void Return(T[] array)
	{
		Provider.Return(array);
	}
}
