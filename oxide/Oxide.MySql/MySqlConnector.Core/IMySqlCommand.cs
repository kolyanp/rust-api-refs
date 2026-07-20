using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(2)]
internal interface IMySqlCommand
{
	string CommandText { get; }

	CommandType CommandType { get; }

	bool AllowUserVariables { get; }

	CommandBehavior CommandBehavior { get; }

	MySqlParameterCollection RawParameters { get; }

	MySqlAttributeCollection RawAttributes { get; }

	MySqlConnection Connection { get; }

	long LastInsertedId { get; }

	MySqlParameterCollection OutParameters { get; set; }

	MySqlParameter ReturnParameter { get; set; }

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ICancellableCommand CancellableCommand
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	}

	[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(1)]
	ILogger Logger
	{
		[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
		get;
	}

	PreparedStatements TryGetPreparedStatements();

	void SetLastInsertedId(long lastInsertedId);
}
