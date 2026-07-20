using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Utilities;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal readonly struct ResizableArraySegment<T>(ResizableArray<T> array, int offset, int count)
{
	public ResizableArray<T> Array { get; } = array;

	public int Offset { get; } = offset;

	public int Count { get; } = count;

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public static implicit operator ReadOnlySpan<T>([_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })] ResizableArraySegment<T> segment)
	{
		return new ReadOnlySpan<T>(segment.Array.Array, segment.Offset, segment.Count);
	}
}
