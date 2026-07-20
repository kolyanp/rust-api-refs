namespace MySqlConnector;

public enum MySqlSslMode
{
	None = 0,
	Disabled = 0,
	Preferred = 1,
	Required = 2,
	VerifyCA = 3,
	VerifyFull = 4
}
