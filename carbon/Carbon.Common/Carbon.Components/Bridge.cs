using System.Threading.Tasks;

namespace Carbon.Components;

public static class Bridge
{
	public static async ValueTask<BridgeClient> StartClient(string ip, int port, string password, BridgeMessages messages, int maxBufferSize = 8192)
	{
		return await new BridgeClient().Connect(ip, port, password, messages, maxBufferSize);
	}
}
