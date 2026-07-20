using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Logging;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
public sealed class NoOpLogger : IMySqlConnectorLogger
{
	public static IMySqlConnectorLogger Instance { get; } = new NoOpLogger();

	public bool IsEnabled(MySqlConnectorLogLevel level)
	{
		return false;
	}

	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
	public void Log(MySqlConnectorLogLevel level, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)] string message, object[] args = null, Exception exception = null)
	{
	}
}
