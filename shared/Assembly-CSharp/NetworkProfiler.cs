using ConVar;
using Network;
using UnityEngine;

public static class NetworkProfiler
{
	private static void SetupProfile()
	{
		PacketProfiler.enabled = true;
		PacketProfiler.detailedProfiling = true;
		PacketProfiler.Reset();
	}

	private static void ExportProfile()
	{
		PacketProfiler.Export.ToFile(PacketProfiler.Export.CreateSnapshot());
		PacketProfiler.enabled = false;
		PacketProfiler.detailedProfiling = false;
		PacketProfiler.Reset();
	}

	[ServerVar(Help = "networkprofiler.serverprofile [time to profile(in seconds), min(0.1), max(1000), float]", ServerAdmin = true)]
	public static void ServerProfile(ConsoleSystem.Arg arg)
	{
		float num = arg.GetFloat(0);
		num = Mathf.Clamp(num, 0.1f, 1000f);
		SetupProfile();
		Chat.Broadcast($"Server is taking a network snapshot for {num} seconds...", "SERVER", "#eee", 0uL);
		InvokeHandler.Invoke((Behaviour)(object)SingletonComponent<InvokeHandler>.Instance, delegate
		{
			Chat.Broadcast("Done!", "SERVER", "#eee", 0uL);
			ExportProfile();
		}, num);
	}
}
