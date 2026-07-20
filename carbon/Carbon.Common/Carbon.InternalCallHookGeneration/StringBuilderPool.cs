using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Carbon.InternalCallHookGeneration;

internal static class StringBuilderPool
{
	private const int InitialCapacity = 1024;

	private const int MaxRetainedCapacity = 32768;

	private const int MaxPoolSize = 64;

	private static readonly ConcurrentQueue<StringBuilder> Pool = new ConcurrentQueue<StringBuilder>();

	private static int _count;

	public static StringBuilder Rent()
	{
		if (Pool.TryDequeue(out StringBuilder result))
		{
			Interlocked.Decrement(ref _count);
			return result;
		}
		return new StringBuilder(1024);
	}

	public static string ToStringAndReturn(ref StringBuilder? builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		string result = builder.ToString();
		Return(ref builder);
		return result;
	}

	public static void Return(ref StringBuilder? builder)
	{
		if (builder != null && builder.Capacity <= 32768)
		{
			builder.Clear();
			if (Interlocked.Increment(ref _count) > 64)
			{
				Interlocked.Decrement(ref _count);
				return;
			}
			Pool.Enqueue(builder);
			builder = null;
		}
	}
}
