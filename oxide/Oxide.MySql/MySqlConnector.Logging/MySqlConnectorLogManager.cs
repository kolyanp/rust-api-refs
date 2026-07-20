using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MySqlConnector.Logging;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public static class MySqlConnectorLogManager
{
	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class MySqlConnectorLoggerFactory(IMySqlConnectorLoggerProvider loggerProvider) : ILoggerFactory, IDisposable
	{
		public void AddProvider(ILoggerProvider provider)
		{
			throw new NotSupportedException();
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new MySqlConnectorLogger(loggerProvider.CreateLogger(categoryName.Substring(15, categoryName.Length - 15)));
		}

		public void Dispose()
		{
		}
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	private sealed class MySqlConnectorLogger(IMySqlConnectorLogger logger) : ILogger
	{
		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotSupportedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logger.IsEnabled(ConvertLogLevel(logLevel));
		}

		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
		public void Log<TState>(LogLevel logLevel, EventId eventId, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] TState state, Exception exception, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(new byte[] { 1, 1, 2, 1 })] Func<TState, Exception, string> formatter)
		{
			logger.Log(ConvertLogLevel(logLevel), formatter(state, exception), null, exception);
		}

		private static MySqlConnectorLogLevel ConvertLogLevel(LogLevel logLevel)
		{
			return logLevel switch
			{
				LogLevel.Trace => MySqlConnectorLogLevel.Trace, 
				LogLevel.Debug => MySqlConnectorLogLevel.Debug, 
				LogLevel.Information => MySqlConnectorLogLevel.Info, 
				LogLevel.Warning => MySqlConnectorLogLevel.Warn, 
				LogLevel.Error => MySqlConnectorLogLevel.Error, 
				LogLevel.Critical => MySqlConnectorLogLevel.Fatal, 
				_ => MySqlConnectorLogLevel.Info, 
			};
		}
	}

	public static IMySqlConnectorLoggerProvider Provider
	{
		set
		{
			MySqlConnectorLoggingConfiguration.GlobalConfiguration = new MySqlConnectorLoggingConfiguration(new MySqlConnectorLoggerFactory(value));
		}
	}
}
