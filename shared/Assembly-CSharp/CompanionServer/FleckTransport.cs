using System.Net;
using Fleck;

namespace CompanionServer;

public sealed class FleckTransport : IConnectionTransport
{
	private readonly IWebSocketConnection _connection;

	public IPAddress Address => _connection.ConnectionInfo.ClientIpAddress;

	public bool IsAvailable
	{
		get
		{
			if (_connection != null)
			{
				return _connection.IsAvailable;
			}
			return false;
		}
	}

	public FleckTransport(IWebSocketConnection connection)
	{
		_connection = connection;
	}

	public void Send(MemoryBuffer data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_connection.Send(data);
	}

	public void Close()
	{
		IWebSocketConnection connection = _connection;
		if (connection != null)
		{
			connection.Close();
		}
	}
}
