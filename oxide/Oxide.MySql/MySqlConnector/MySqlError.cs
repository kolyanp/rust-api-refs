using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlError
{
	public string Level { get; }

	[Obsolete("Use ErrorCode")]
	public int Code { get; }

	public MySqlErrorCode ErrorCode { get; }

	public string Message { get; }

	internal MySqlError(string level, int code, string message)
	{
		Level = level;
		Code = code;
		ErrorCode = (MySqlErrorCode)code;
		Message = message;
	}
}
