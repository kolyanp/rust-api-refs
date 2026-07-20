using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Protocol.Serialization;

internal sealed class ArraySegmentHolder<[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] T>
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public ArraySegment<T> ArraySegment
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
		get;
		[param: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
		set;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
	public T[] Array
	{
		[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return ArraySegment.Array;
		}
	}

	public int Offset => ArraySegment.Offset;

	public int Count => ArraySegment.Count;

	public void Clear()
	{
		if (ArraySegment.Count > 0)
		{
			ArraySegment = new ArraySegment<T>(ArraySegment.Array, 0, 0);
		}
	}
}
