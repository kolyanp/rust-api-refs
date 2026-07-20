using System.Runtime.CompilerServices;

namespace System;

internal static class _003Cf9b47b20_002Df8fc_002D4539_002Db746_002D9c209e9728ac_003EThrowHelper
{
	internal static void ThrowArgumentNullException(System.ExceptionArgument argument)
	{
		throw GetArgumentNullException(argument);
	}

	internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument)
	{
		throw GetArgumentOutOfRangeException(argument);
	}

	private static ArgumentNullException GetArgumentNullException(System.ExceptionArgument argument)
	{
		return new ArgumentNullException(GetArgumentName(argument));
	}

	private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(System.ExceptionArgument argument)
	{
		return new ArgumentOutOfRangeException(GetArgumentName(argument));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static string GetArgumentName(System.ExceptionArgument argument)
	{
		return argument.ToString();
	}
}
