namespace MySqlConnector;

public enum MySqlLoadBalance
{
	RoundRobin,
	FailOver,
	Random,
	LeastConnections
}
