using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Network.Relay;
using UnityEngine;

namespace Network;

public class NetPacketCapture
{
	private const int FileBufferSize = 8388608;

	public readonly string FilePath;

	private readonly BaseNetwork peer;

	private readonly long maxBytes;

	private readonly long endTimestamp;

	private readonly bool captureRelay;

	private readonly FileStream file;

	private byte[] header = new byte[1024];

	private long bytesCaptured;

	private volatile bool stopRequested;

	public static string DefaultPath()
	{
		string text = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		return Path.Combine(NetProfileSnapshot.DefaultDirectory, "packetcapture_" + text + ".bin");
	}

	public static NetPacketCapture Start(BaseNetwork peer, string path, long maxBytes, TimeSpan maxDuration, bool captureRelay)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		return peer.PacketCapture = new NetPacketCapture(peer, path, maxBytes, maxDuration, captureRelay);
	}

	private NetPacketCapture(BaseNetwork peer, string path, long maxBytes, TimeSpan maxDuration, bool captureRelay)
	{
		this.peer = peer;
		this.maxBytes = maxBytes;
		this.captureRelay = captureRelay;
		FilePath = path;
		endTimestamp = Stopwatch.GetTimestamp() + (long)(maxDuration.TotalSeconds * (double)Stopwatch.Frequency);
		file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 8388608);
	}

	public void RequestStop()
	{
		stopRequested = true;
	}

	public void Tick()
	{
		if (stopRequested || Stopwatch.GetTimestamp() >= endTimestamp)
		{
			Finish();
		}
	}

	public void Capture(NetWrite write)
	{
		List<Connection> connections = write.connections;
		int num = 0;
		for (int i = 0; i < connections.Count; i++)
		{
			if (ShouldRecord(connections[i]))
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		(byte[] Buffer, int Length) buffer = write.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		int num2 = 4 + num * 8 + 4;
		int num3 = num2 + item2;
		if (bytesCaptured + num3 > maxBytes)
		{
			Finish();
			return;
		}
		if (header.Length < num2)
		{
			header = new byte[num2];
		}
		BinaryPrimitives.WriteInt32LittleEndian(header, num);
		int num4 = 4;
		for (int j = 0; j < connections.Count; j++)
		{
			if (ShouldRecord(connections[j]))
			{
				BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(num4), connections[j].userid);
				num4 += 8;
			}
		}
		BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(num4), item2);
		try
		{
			file.Write(header, 0, num2);
			file.Write(item, 0, item2);
			bytesCaptured += num3;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Finish();
		}
	}

	private bool ShouldRecord(Connection connection)
	{
		if (!captureRelay)
		{
			return !RustRelay.IsFakeConnection(connection);
		}
		return true;
	}

	private void Finish()
	{
		peer.PacketCapture = null;
		try
		{
			file.Dispose();
			Debug.Log((object)$"[NetPacketCapture] Saved {(float)bytesCaptured / 1048576f:0.00} MB of packets to: {FilePath}");
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}
}
