using System.Runtime.CompilerServices;

namespace System;

internal static class ThrowHelper
{
	internal static void ThrowIfNull(object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
	{
		if (argument == null)
		{
			Throw(paramName);
		}
	}

	private static void Throw(string paramName)
	{
		throw new ArgumentNullException(paramName);
	}
}
