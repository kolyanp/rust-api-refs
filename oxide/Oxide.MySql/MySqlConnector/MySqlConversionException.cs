using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MySqlConnector;

[Serializable]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
public sealed class MySqlConversionException : Exception
{
	internal MySqlConversionException(string message)
		: base(message)
	{
	}

	private MySqlConversionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
