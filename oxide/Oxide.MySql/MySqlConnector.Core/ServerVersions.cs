using System;
using System.Runtime.CompilerServices;

namespace MySqlConnector.Core;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class ServerVersions
{
	public static readonly Version SupportsUtf8Mb4 = new Version(5, 5, 3);

	public static readonly Version SupportsResetConnection = new Version(5, 7, 3);

	public static readonly Version MariaDbSupportsResetConnection = new Version(10, 2, 4);

	public static readonly Version SupportsProcedureCache = new Version(5, 5, 3);

	public static readonly Version RemovesMySqlProcTable = new Version(8, 0, 0);

	public static readonly Version MariaDbSupportsPerQueryVariables = new Version(10, 1, 2);
}
