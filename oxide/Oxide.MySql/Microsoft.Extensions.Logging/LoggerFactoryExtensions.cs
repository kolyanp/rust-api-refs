using System;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.Logging;

internal static class LoggerFactoryExtensions
{
	public static ILogger<T> CreateLogger<T>(this ILoggerFactory factory)
	{
		System.ThrowHelper.ThrowIfNull(factory, "factory");
		return new Logger<T>(factory);
	}

	public static ILogger CreateLogger(this ILoggerFactory factory, Type type)
	{
		System.ThrowHelper.ThrowIfNull(factory, "factory");
		System.ThrowHelper.ThrowIfNull(type, "type");
		return factory.CreateLogger(TypeNameHelper.GetTypeDisplayName(type, fullName: true, includeGenericParameterNames: false, includeGenericParameters: false, '.'));
	}
}
