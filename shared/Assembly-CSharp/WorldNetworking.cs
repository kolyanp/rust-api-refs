using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class WorldNetworking
{
	private const int prefabsPerPacket = 100;

	private const int pathsPerPacket = 10;

	public static void OnServerMessageReceived(Message message)
	{
		if (message.read.Int32() == 1)
		{
			SendWorldData(message.connection);
		}
	}

	private static void SendWorldData(Connection connection)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (connection.hasRequestedWorld)
		{
			DebugEx.LogWarning($"{connection} requested world data more than once", (StackTraceLogType)0);
			return;
		}
		connection.hasRequestedWorld = true;
		WorldSerialization serialization = World.Serialization;
		WorldMessage data = Pool.Get<WorldMessage>();
		for (int i = 0; i < serialization.world.prefabs.Count; i++)
		{
			if (data.prefabs != null && data.prefabs.Count >= 100)
			{
				data.status = (MessageType)2;
				SendWorldData(connection, ref data);
				data = Pool.Get<WorldMessage>();
			}
			if (data.prefabs == null)
			{
				data.prefabs = Pool.Get<List<PrefabData>>();
			}
			data.prefabs.Add(serialization.world.prefabs[i]);
		}
		for (int j = 0; j < serialization.world.paths.Count; j++)
		{
			if (data.paths != null && data.paths.Count >= 10)
			{
				data.status = (MessageType)2;
				SendWorldData(connection, ref data);
				data = Pool.Get<WorldMessage>();
			}
			if (data.paths == null)
			{
				data.paths = Pool.Get<List<PathData>>();
			}
			data.paths.Add(serialization.world.paths[j]);
		}
		if (data != null)
		{
			data.status = (MessageType)3;
			SendWorldData(connection, ref data);
		}
	}

	private static void SendWorldData(Connection connection, ref WorldMessage data)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.World);
		netWrite.Proto<WorldMessage>(data);
		netWrite.Send(new SendInfo(connection));
		if (data.prefabs != null)
		{
			data.prefabs.Clear();
		}
		if (data.paths != null)
		{
			data.paths.Clear();
		}
		data.Dispose();
		data = null;
	}
}
