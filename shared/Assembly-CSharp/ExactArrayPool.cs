using System.Collections.Concurrent;
using System.Collections.Generic;

public class ExactArrayPool<T>
{
	private readonly Dictionary<int, ConcurrentQueue<T[]>> _buffers = new Dictionary<int, ConcurrentQueue<T[]>>();

	public T[] Rent(int size)
	{
		if (!_buffers.TryGetValue(size, out var value))
		{
			value = new ConcurrentQueue<T[]>();
			_buffers[size] = value;
		}
		if (!value.TryDequeue(out var result))
		{
			return new T[size];
		}
		return result;
	}

	public void Return(T[] array)
	{
		if (!_buffers.TryGetValue(array.Length, out var value))
		{
			value = new ConcurrentQueue<T[]>();
			_buffers[array.Length] = value;
		}
		value.Enqueue(array);
	}
}
