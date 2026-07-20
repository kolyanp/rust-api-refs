namespace Carbon.Components;

public struct BridgeServerInfo
{
	public int port;

	public string ip;

	public string password;

	public string context;

	public BridgeMessages messages;

	public int maxConnections;

	public int maxConnectionsPerIp;
}
