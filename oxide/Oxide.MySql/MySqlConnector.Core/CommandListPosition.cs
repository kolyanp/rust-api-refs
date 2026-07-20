using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
internal struct CommandListPosition
{
	private readonly object m_commands;

	public readonly int CommandCount;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public PreparedStatements PreparedStatements;

	public int CommandIndex;

	public int PreparedStatementIndex;

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(2)]
	public PreparedStatement LastUsedPreparedStatement;

	public CommandListPosition(object commands)
	{
		LastUsedPreparedStatement = null;
		m_commands = commands;
		int commandCount = ((commands is MySqlCommand) ? 1 : ((commands is IReadOnlyList<MySqlBatchCommand> readOnlyList) ? readOnlyList.Count : 0));
		CommandCount = commandCount;
		PreparedStatements = null;
		CommandIndex = 0;
		PreparedStatementIndex = 0;
	}

	public readonly IMySqlCommand CommandAt(int index)
	{
		object commands = m_commands;
		if (!(commands is MySqlCommand result))
		{
			if (commands is IReadOnlyList<MySqlBatchCommand> readOnlyList)
			{
				return readOnlyList[index];
			}
		}
		else if (index == 0)
		{
			return result;
		}
		throw new ArgumentOutOfRangeException("index");
	}
}
