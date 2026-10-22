using System;
using ConVar;
using Network;
using UnityEngine;

public static class NetworkProfiler
{
	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void InstallResolvers()
	{
		NetProfileCapture.NameResolver = StringPool.Get;
		NetProfileCapture.PrefabResolver = ResolvePrefabId;
	}

	private static uint ResolvePrefabId(ulong entityId, bool serverRealm)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = new NetworkableId(entityId);
		if (serverRealm)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
			if (!((Object)(object)baseNetworkable != (Object)null))
			{
				return 0u;
			}
			return baseNetworkable.prefabID;
		}
		return 0u;
	}

	private static void ExportProfile()
	{
		NetProfileSnapshot netProfileSnapshot = NetProfileCapture.CreateSnapshot();
		NetProfileCapture.Stop();
		if (netProfileSnapshot != null)
		{
			string text = NetProfileSnapshot.DefaultPath();
			netProfileSnapshot.Save(text);
			Debug.Log((object)("[NetworkProfiler] Exported profile to: " + text));
		}
	}

	[ServerVar(Help = "networkprofiler.serverprofile [time to profile(in seconds), min(0.1), max(1000), float]", ServerAdmin = true)]
	public static void ServerProfile(ConsoleSystem.Arg arg)
	{
		float num = arg.GetFloat(0);
		num = Mathf.Clamp(num, 0.1f, 1000f);
		NetProfileCapture.Start(num);
		Chat.Broadcast($"Server is taking a network snapshot for {num} seconds...", "SERVER", "#eee", 0uL);
		InvokeHandler.Invoke((Behaviour)(object)SingletonComponent<InvokeHandler>.Instance, () =>
		{
			Chat.Broadcast("Done!", "SERVER", "#eee", 0uL);
			ExportProfile();
		}, num);
	}

	[ServerVar(Name = "packet_capture", Help = "networkprofiler.packet_capture [max size(in MB), min(1), max(10000), float] [max duration(in seconds), min(1), max(3600), float]", ServerAdmin = true)]
	public static void PacketCapture(ConsoleSystem.Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Usage: networkprofiler.packet_capture <max size in MB> <max duration in seconds>");
			return;
		}
		NetPacketCapture packetCapture = Net.sv.PacketCapture;
		if (packetCapture != null)
		{
			arg.ReplyWith("A packet capture is already running: " + packetCapture.FilePath);
			return;
		}
		float num = Mathf.Clamp(arg.GetFloat(0), 1f, 10000f);
		float num2 = Mathf.Clamp(arg.GetFloat(1), 1f, 3600f);
		NetPacketCapture netPacketCapture = NetPacketCapture.Start(Net.sv, NetPacketCapture.DefaultPath(), (long)(num * 1024f * 1024f), TimeSpan.FromSeconds(num2));
		arg.ReplyWith($"Capturing packets for up to {num2} seconds or {num} MB to: {netPacketCapture.FilePath}");
	}

	[ServerVar(Name = "stop_packet_capture", Help = "networkprofiler.stop_packet_capture - ends the running packet capture early", ServerAdmin = true)]
	public static void PacketCaptureStop(ConsoleSystem.Arg arg)
	{
		NetPacketCapture packetCapture = Net.sv.PacketCapture;
		if (packetCapture == null)
		{
			arg.ReplyWith("No packet capture is running");
			return;
		}
		packetCapture.RequestStop();
		arg.ReplyWith("Stopping packet capture: " + packetCapture.FilePath);
	}
}
