using System.Buffers;
using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public sealed class JsonArrayPool<T> : IArrayPool<T>
{
	public static readonly JsonArrayPool<T> Shared = new JsonArrayPool<T>();

	public T[] Rent(int minimumLength)
	{
		return ArrayPool<T>.Shared.Rent(minimumLength);
	}

	public void Return(T[] array)
	{
		ArrayPool<T>.Shared.Return(array);
	}
}
