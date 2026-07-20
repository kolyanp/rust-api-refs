using CompanionServer.Cameras;
using ConVar;
using Facepunch;
using Facepunch.Math;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class Info : BasePlayerHandler<AppEmpty>
{
	public override void Execute()
	{
		AppInfo val = Pool.Get<AppInfo>();
		val.name = ConVar.Server.hostname;
		val.headerImage = ConVar.Server.headerimage;
		val.logoImage = ConVar.Server.logoimage;
		val.url = ConVar.Server.url;
		val.map = World.Name;
		val.mapSize = World.Size;
		val.wipeTime = (uint)Epoch.FromDateTime(SaveRestore.SaveCreatedTime.ToUniversalTime());
		val.players = (uint)BasePlayer.activePlayerList.Count;
		val.maxPlayers = (uint)ConVar.Server.maxplayers;
		val.queuedPlayers = (uint)SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued;
		val.seed = World.Seed;
		val.camerasEnabled = CameraRenderer.enabled;
		if (NexusServer.Started)
		{
			int? nexusId = NexusServer.NexusId;
			string zoneKey = NexusServer.ZoneKey;
			if (nexusId.HasValue && zoneKey != null)
			{
				val.nexus = Nexus.endpoint;
				val.nexusId = nexusId.Value;
				val.nexusZone = zoneKey;
			}
		}
		AppResponse val2 = Pool.Get<AppResponse>();
		val2.info = val;
		Send(val2);
	}
}
