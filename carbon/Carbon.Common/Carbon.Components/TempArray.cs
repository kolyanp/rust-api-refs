using System;

namespace Carbon.Components;

[Obsolete("TempArray is obsolete and is going to be removed entirely from Carbon on July 3rd, 2025.")]
public class TempArray<T> : IDisposable
{
	public T[] array;

	public bool IsEmpty
	{
		get
		{
			if (array != null)
			{
				return array.Length == 0;
			}
			return true;
		}
	}

	public int Length
	{
		get
		{
			if (!IsEmpty)
			{
				return array.Length;
			}
			return 0;
		}
	}

	public T Get(int index, T @default = default(T))
	{
		if (index <= array.Length - 1)
		{
			return array[index];
		}
		return @default;
	}

	public static TempArray<T> New(T[] array)
	{
		return new TempArray<T>
		{
			array = array
		};
	}

	public void Dispose()
	{
		if (array != null)
		{
			Array.Clear(array, 0, array.Length);
			array = null;
		}
	}
}
