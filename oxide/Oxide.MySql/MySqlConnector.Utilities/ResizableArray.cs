using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Utilities;

internal sealed class ResizableArray<[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] T>
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	private T[] m_array;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	public T[] Array
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return m_array;
		}
	}

	public int Count
	{
		get
		{
			T[] array = m_array;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public Span<T> AsSpan(int start)
	{
		return MemoryExtensions.AsSpan(m_array, start);
	}

	internal void DoResize(int length)
	{
		if (m_array == null || length > m_array.Length)
		{
			System.Array.Resize(ref m_array, Math.Max(length, Count * 2));
		}
	}
}
