using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Facepunch;
using Facepunch.Rcon;
using Fleck;
using UnityEngine;

namespace Carbon.Components;

public abstract class BridgeServer
{
	public Listener Listener;

	public Action<BridgeConnection> OnNewConnection;

	public Action<BridgeConnection> OnClosedConnection;

	public BridgeMessages Messages;

	public readonly Dictionary<int, BridgeConnection> Connections = new Dictionary<int, BridgeConnection>();

	public readonly ListHashSet<BridgeConnection> ConnectionsList = new ListHashSet<BridgeConnection>();

	private const int MaxEventsPerFrame = 100;

	private readonly ConcurrentQueue<Action> _bridgeEvents = new ConcurrentQueue<Action>();

	private Coroutine _routine;

	private string _context;

	private bool _isConnected;

	public void Start(BridgeServerInfo serverInfo)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0042: Expected O, but got Unknown
		_context = serverInfo.context;
		if (!OnPasswordValidate(serverInfo.password))
		{
			return;
		}
		SetMessages(serverInfo.messages);
		Listener val = new Listener();
		Listener val2 = val;
		Listener = val;
		Listener listener = val2;
		if (!string.IsNullOrEmpty(serverInfo.ip))
		{
			listener.Address = serverInfo.ip;
		}
		listener.Password = Vault.ApplyReplacement(serverInfo.password) ?? serverInfo.password;
		listener.Port = serverInfo.port;
		try
		{
			listener.Start(serverInfo.maxConnections, serverInfo.maxConnectionsPerIp);
			listener.server._config = delegate(IWebSocketConnection socket)
			{
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cb: Expected O, but got Unknown
				//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d5: Expected O, but got Unknown
				lock (listener.clients)
				{
					if (!OnSocketValidate(socket))
					{
						socket.Close();
					}
					else
					{
						int id = Interlocked.Increment(ref listener.nextClientId);
						BridgeConnection bridgeConnection = Pool.Get<BridgeConnection>().Init(id, socket, Messages);
						socket.OnOpen = delegate
						{
							_bridgeEvents.Enqueue(delegate
							{
								OnOpenSocket(socket, bridgeConnection);
							});
						};
						socket.OnClose = delegate
						{
							_bridgeEvents.Enqueue(delegate
							{
								OnCloseSocket(socket, bridgeConnection);
							});
						};
						IWebSocketConnection obj = socket;
						obj.OnBinary = (BinaryDataHandler)Delegate.Combine((Delegate?)(object)obj.OnBinary, (Delegate?)(BinaryDataHandler)delegate(Span<byte> data)
						{
							BufferStream stream = Pool.Get<BufferStream>().Initialize();
							stream._buffer = BufferStream.RentBuffer(data.Length);
							stream._length = stream._buffer.Length;
							stream._isBufferOwned = true;
							for (int i = 0; i < data.Length; i++)
							{
								stream._buffer[i] = data[i];
							}
							_bridgeEvents.Enqueue(delegate
							{
								OnBinarySocket(socket, bridgeConnection, stream);
							});
						});
						socket.OnError = delegate(Exception e)
						{
							Logger.Error("Socket failure", e);
						};
					}
				}
			};
			Logger.Log($"Started Carbon.Bridge on port {serverInfo.port} ({_context})");
			_isConnected = true;
			OnServerConnected();
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to start Carbon.Bridge on port {serverInfo.port} ({_context})", ex);
			Shutdown();
		}
	}

	public void Shutdown()
	{
		Action result;
		while (_bridgeEvents.TryDequeue(out result))
		{
			try
			{
				result();
			}
			catch (Exception ex)
			{
				Logger.Error("Bridge shutdown drain failure", ex);
			}
		}
		PooledList<BridgeConnection> val = Pool.Get<PooledList<BridgeConnection>>();
		try
		{
			for (int i = 0; i < ConnectionsList.Count; i++)
			{
				((List<BridgeConnection>)(object)val).Add(ConnectionsList[i]);
			}
			for (int j = 0; j < ((List<BridgeConnection>)(object)val).Count; j++)
			{
				BridgeConnection bridgeConnection = ((List<BridgeConnection>)(object)val)[j];
				if (bridgeConnection != null)
				{
					IWebSocketConnection socket = bridgeConnection.Socket;
					if (socket != null)
					{
						socket.Close();
					}
				}
			}
			Connections.Clear();
			ConnectionsList.Clear();
			if (Listener != null)
			{
				Logger.Log($"Stopped Carbon.Bridge on port {Listener.Port} ({_context})");
			}
			Listener listener = Listener;
			if (listener != null)
			{
				listener.Shutdown();
			}
			OnNewConnection = null;
			OnClosedConnection = null;
			Messages = null;
			_isConnected = false;
			OnServerDisconnected();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool IsConnected()
	{
		if (Listener != null)
		{
			return _isConnected;
		}
		return false;
	}

	public virtual void OnServerConnected()
	{
		_routine = Application.Controller.StartCoroutine(RunEventRoutine());
	}

	public virtual void OnServerDisconnected()
	{
		if (_routine != null)
		{
			Application.Controller.StopCoroutine(_routine);
			_routine = null;
		}
	}

	public virtual bool OnPasswordValidate(string password)
	{
		bool flag;
		switch (password)
		{
		case null:
		case "unset":
		case "password":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		return !flag;
	}

	public virtual bool OnSocketValidate(IWebSocketConnection socket)
	{
		return socket.ConnectionInfo.Path == "/" + Listener.Password;
	}

	public abstract void OnBridgeConnection(BridgeConnection connection);

	public abstract void OnBridgeDisconnection(BridgeConnection connection);

	private IEnumerator RunEventRoutine()
	{
		while (true)
		{
			for (int i = 0; i < 100; i++)
			{
				if (!_bridgeEvents.TryDequeue(out var result))
				{
					break;
				}
				try
				{
					result();
				}
				catch (Exception ex)
				{
					Logger.Error("Bridge event failure", ex);
				}
			}
			yield return null;
		}
	}

	private void OnOpenSocket(IWebSocketConnection socket, BridgeConnection bridgeConnection)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Listener.clients.Add(bridgeConnection.Id, new RconConnection(socket, bridgeConnection.Id));
		Connections[bridgeConnection.Id] = bridgeConnection;
		ConnectionsList.Add(bridgeConnection);
		OnNewConnection?.Invoke(bridgeConnection);
		OnBridgeConnection(bridgeConnection);
	}

	private void OnCloseSocket(IWebSocketConnection socket, BridgeConnection bridgeConnection)
	{
		Listener.clients.Remove(bridgeConnection.Id);
		if (Connections.ContainsKey(bridgeConnection.Id))
		{
			OnBridgeDisconnection(bridgeConnection);
			OnClosedConnection?.Invoke(bridgeConnection);
			Connections.Remove(bridgeConnection.Id);
			ConnectionsList.Remove(bridgeConnection);
			Pool.Free<BridgeConnection>(ref bridgeConnection);
		}
	}

	private void OnBinarySocket(IWebSocketConnection socket, BridgeConnection bridgeConnection, BufferStream buffer)
	{
		BridgeRead read = BridgeRead.Rent(buffer, bridgeConnection);
		try
		{
			Messages.HandleChannelRead(read);
		}
		catch (Exception ex)
		{
			Logger.Error("Carbon.Bridge.OnBinarySocket failure", ex);
		}
		if (Messages.ShouldPool)
		{
			BridgeRead.Return(ref read);
		}
	}

	public void SetMessages(BridgeMessages messages)
	{
		Messages = messages ?? new DefaultBridgeMessages();
		for (int i = 0; i < ConnectionsList.Count; i++)
		{
			ConnectionsList[i].Messages = Messages;
		}
	}
}
