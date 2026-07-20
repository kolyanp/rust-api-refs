using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal sealed class CachedParameter
{
	public int Position { get; }

	public ParameterDirection Direction { get; }

	public string Name { get; }

	public MySqlDbType MySqlDbType { get; }

	public int Length { get; }

	public CachedParameter(int ordinalPosition, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)] string mode, string name, string dataType, bool unsigned, int length)
	{
		Position = ordinalPosition;
		if (Position == 0)
		{
			Direction = ParameterDirection.ReturnValue;
		}
		else if (string.Equals(mode, "in", StringComparison.OrdinalIgnoreCase))
		{
			Direction = ParameterDirection.Input;
		}
		else if (string.Equals(mode, "inout", StringComparison.OrdinalIgnoreCase))
		{
			Direction = ParameterDirection.InputOutput;
		}
		else if (string.Equals(mode, "out", StringComparison.OrdinalIgnoreCase))
		{
			Direction = ParameterDirection.Output;
		}
		Name = name;
		MySqlDbType = TypeMapper.Instance.GetMySqlDbType(dataType, unsigned, length);
		Length = length;
	}
}
