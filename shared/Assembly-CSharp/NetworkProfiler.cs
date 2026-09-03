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
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId uid = default(NetworkableId);
		((NetworkableId)(ref uid))._002Ector(entityId);
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
		InvokeHandler.Invoke((Behaviour)(object)SingletonComponent<InvokeHandler>.Instance, delegate
		{
			Chat.Broadcast("Done!", "SERVER", "#eee", 0uL);
			ExportProfile();
		}, num);
	}
}
