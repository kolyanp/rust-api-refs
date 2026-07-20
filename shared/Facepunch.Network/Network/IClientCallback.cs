namespace Network;

public interface IClientCallback
{
	void OnNetworkMessage(Message message);

	void OnClientDisconnected(string reason);
}
