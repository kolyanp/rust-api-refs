using System;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Extensions;

public static class EnumerableEx
{
	public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
	{
		if (value == null)
		{
			return 0;
		}
		int num = 0;
		foreach (T item in enumerable)
		{
			if (item.Equals(value))
			{
				return num;
			}
			num++;
		}
		return num;
	}

	public static int FindIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
	{
		int num = 0;
		if (predicate == null)
		{
			return num + enumerable.Count();
		}
		return num + enumerable.Count((T iteration) => predicate(iteration));
	}

	public static T FindAt<T>(this IEnumerable<T> enumerable, int index)
	{
		int num = 0;
		foreach (T item in enumerable)
		{
			if (num == index)
			{
				return item;
			}
			num++;
		}
		return default(T);
	}

	public static ulong SumULong<TSource>(this IEnumerable<TSource> source, Func<TSource, ulong> selector)
	{
		return source.Select(selector).Aggregate(0uL, (ulong current, ulong value) => current + value);
	}

	public static long SumLong<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
	{
		return source.Select(selector).Aggregate(0L, (long current, long value) => current + value);
	}

	public static uint SumUInt<TSource>(this IEnumerable<TSource> source, Func<TSource, uint> selector)
	{
		return source.Aggregate(0u, (uint current, TSource value) => current + selector(value));
	}
}
