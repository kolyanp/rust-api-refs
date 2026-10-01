using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Facepunch.Nexus;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class NexusAuth : BaseHandler<AppGetNexusAuth>
{
	public override ValidationResult Validate()
	{
		if (!NexusServer.Started)
		{
			return ValidationResult.NotFound;
		}
		return base.Validate();
	}

	public override async ValueTask Execute()
	{
		if (Request.playerId == 0L)
		{
			SendError("invalid_playerid");
			return;
		}
		NexusPlayer val = await NexusServer.ZoneClient.GetPlayer(Request.playerId);
		Variable val2 = default;
		if (val == null || !val.TryGetVariable("appKey", ref val2) || (int)val2.Type != 1 || Proto.appKey != val2.GetAsString())
		{
			SendError("access_denied");
			return;
		}
		AppResponse val3 = Pool.Get<AppResponse>();
		val3.nexusAuth = Pool.Get<AppNexusAuth>();
		val3.nexusAuth.serverId = App.serverid;
		val3.nexusAuth.playerToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(Request.playerId, out var _);
		Send(val3);
	}
}
