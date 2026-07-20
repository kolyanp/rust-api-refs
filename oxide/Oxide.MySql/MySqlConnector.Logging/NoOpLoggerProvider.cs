using System.Runtime.CompilerServices;

namespace MySqlConnector.Logging;

public sealed class NoOpLoggerProvider : IMySqlConnectorLoggerProvider
{
	[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
	public IMySqlConnectorLogger CreateLogger(string name)
	{
		return NoOpLogger.Instance;
	}
}
