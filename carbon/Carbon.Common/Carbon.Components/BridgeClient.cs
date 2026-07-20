using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Facepunch;
using Network;

namespace Carbon.Components;

public sealed class BridgeClient
{
	public ClientWebSocket Socket;

	public CancellationTokenSource CancellationToken;

	public BridgeMessages Messages;

	public int MaxBufferSize;

	public async ValueTask<BridgeClient> Connect(string ip, int port, string password, BridgeMessages messages, int maxBufferSize = 8192)
	{
		MaxBufferSize = maxBufferSize;
		Messages = messages;
		Socket = new ClientWebSocket();
		CancellationToken = new CancellationTokenSource();
		try
		{
			await Socket.ConnectAsync(new Uri($"ws://{ip}:{port}/{Vault.ApplyReplacement(password) ?? password}"), CancellationToken.Token);
			Task.Run(async delegate
			{
				await ReceiveLoop();
			});
		}
		catch (Exception ex)
		{
			Logger.Error($"Carbon.Bridge Client connection attempt to '{ip}:{port}' failed", ex);
		}
		return this;
	}

	public async ValueTask Send(BridgeWrite write)
	{
		await Socket.SendAsync(new ArraySegment<byte>(((NetWrite)write).GetBuffer().Item1), WebSocketMessageType.Binary, endOfMessage: true, CancellationToken.Token);
	}

	public async ValueTask Disconnect()
	{
		await (Socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", CancellationToken.Token));
		CancellationToken.Cancel();
		CancellationToken = null;
		Socket?.Dispose();
		Socket = null;
	}

	private async ValueTask ReceiveLoop()
	{
		while (Socket.State == WebSocketState.Open && !CancellationToken.IsCancellationRequested)
		{
			BufferStream stream = Pool.Get<BufferStream>().Initialize();
			stream._isBufferOwned = true;
			stream._buffer = BufferStream.RentBuffer(MaxBufferSize);
			stream._length = stream._buffer.Length;
			stream._position = 0;
			try
			{
				WebSocketReceiveResult webSocketReceiveResult;
				do
				{
					webSocketReceiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(stream._buffer), CancellationToken.Token);
				}
				while (!webSocketReceiveResult.EndOfMessage);
				switch (webSocketReceiveResult.MessageType)
				{
				case WebSocketMessageType.Binary:
				{
					BridgeRead read = BridgeRead.Rent(stream);
					try
					{
						Messages.HandleChannelRead(read);
					}
					catch (Exception ex)
					{
						Logger.Error("Carbon.Bridge.ReceiveLoop[OnRead] failure", ex);
					}
					if (Messages.ShouldPool)
					{
						BridgeRead.Return(ref read);
					}
					break;
				}
				case WebSocketMessageType.Close:
					Pool.Free<BufferStream>(ref stream);
					await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.Token);
					break;
				default:
					Pool.Free<BufferStream>(ref stream);
					break;
				}
			}
			catch (Exception ex2)
			{
				Pool.Free<BufferStream>(ref stream);
				Logger.Error("Carbon.Bridge.ReceiveLoop failure", ex2);
			}
		}
		await Disconnect();
	}
}
