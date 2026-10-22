using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Network;

public class NetPacketCapture
{
	private const int FileBufferSize = 8388608;

	public readonly string FilePath;

	private readonly BaseNetwork peer;

	private readonly long maxBytes;

	private readonly long endTimestamp;

	private readonly FileStream file;

	private byte[] header = new byte[1024];

	private long bytesCaptured;

	private volatile bool stopRequested;

	public static string DefaultPath()
	{
		string text = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		return Path.Combine(NetProfileSnapshot.DefaultDirectory, "packetcapture_" + text + ".bin");
	}

	public static NetPacketCapture Start(BaseNetwork peer, string path, long maxBytes, TimeSpan maxDuration)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		return peer.PacketCapture = new NetPacketCapture(peer, path, maxBytes, maxDuration);
	}

	private NetPacketCapture(BaseNetwork peer, string path, long maxBytes, TimeSpan maxDuration)
	{
		this.peer = peer;
		this.maxBytes = maxBytes;
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
		(byte[] Buffer, int Length) buffer = write.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		int num = 4 + connections.Count * 8 + 4;
		int num2 = num + item2;
		if (bytesCaptured + num2 > maxBytes)
		{
			Finish();
			return;
		}
		if (header.Length < num)
		{
			header = new byte[num];
		}
		BinaryPrimitives.WriteInt32LittleEndian(header, connections.Count);
		int num3 = 4;
		for (int i = 0; i < connections.Count; i++)
		{
			BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(num3), connections[i].userid);
			num3 += 8;
		}
		BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(num3), item2);
		try
		{
			file.Write(header, 0, num);
			file.Write(item, 0, item2);
			bytesCaptured += num2;
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			Finish();
		}
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
