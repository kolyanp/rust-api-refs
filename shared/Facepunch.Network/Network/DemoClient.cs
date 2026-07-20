using System;
using Facepunch;
using Rust.Demo;
using UnityEngine;

namespace Network;

public class DemoClient : Client, IDisposable
{
	protected Reader demoFile;

	public override bool IsPlaying => true;

	public bool PlayingFinished => demoFile.IsFinished;

	public DemoClient(Reader demoFile)
	{
		this.demoFile = demoFile;
		MultithreadingInit(null);
	}

	public virtual void Dispose()
	{
		Reader obj = demoFile;
		if (obj != null)
		{
			obj.Stop();
		}
		demoFile = null;
	}

	public override bool IsConnected()
	{
		return true;
	}

	public void UpdatePlayback(long frameTime)
	{
		if (!PlayingFinished)
		{
			demoFile.Progress(frameTime);
			while (!demoFile.IsFinished && PlaybackPacket())
			{
			}
		}
	}

	private bool PlaybackPacket()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Packet val = demoFile.ReadPacket();
		if (!((Packet)(ref val)).isValid)
		{
			return false;
		}
		HandleMessage(new Span<byte>(val.Data, 0, val.Size));
		return IsPlaying;
	}

	private void HandleMessage(Span<byte> buffer)
	{
		NetRead netRead = Pool.Get<NetRead>();
		netRead.Start(0uL, string.Empty, buffer);
		Decrypt(netRead.connection, netRead);
		byte b = netRead.PacketID();
		if (b < 140)
		{
			netRead.RemoveReference();
			return;
		}
		b -= 140;
		if (b > 28)
		{
			Debug.LogWarning((object)("Invalid Packet (higher than " + Message.Type.PackedSyncVar.ToString() + ")"));
			Disconnect($"Invalid Packet ({b}) {buffer.Length}b");
			netRead.RemoveReference();
			return;
		}
		Message message = StartMessage((Message.Type)b, netRead);
		if (callbackHandler != null)
		{
			try
			{
				using (TimeWarning.New("OnMessage"))
				{
					callbackHandler.OnNetworkMessage(message);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (!IsPlaying)
				{
					Disconnect(ex.Message + "\n" + ex.StackTrace);
				}
			}
		}
		Pool.Free<Message>(ref message);
		netRead.RemoveReference();
	}
}
