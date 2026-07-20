namespace MySqlConnector.Protocol;

internal enum SessionTrackKind : byte
{
	SystemVariables,
	Schema,
	StateChange,
	Gtids
}
