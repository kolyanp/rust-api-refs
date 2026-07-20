using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Utilities;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class ValueTaskExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ValueTask FromException(Exception exception)
	{
		return new ValueTask(Task.FromException(exception));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 0, 1 })]
	public static ValueTask<T> FromException<[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] T>(Exception exception)
	{
		return new ValueTask<T>(Task.FromException<T>(exception));
	}
}
