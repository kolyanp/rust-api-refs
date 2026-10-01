using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class PromoteToLeader : BasePlayerHandler<AppPromoteToLeader>
{
	public override ValueTask Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(UserId);
		if (playerTeam == null)
		{
			SendError("no_team");
			return default;
		}
		if (playerTeam.teamLeader != UserId)
		{
			SendError("access_denied");
			return default;
		}
		if (playerTeam.teamLeader == Proto.steamId)
		{
			SendSuccess();
			return default;
		}
		if (!playerTeam.members.Contains(Proto.steamId))
		{
			SendError("not_found");
			return default;
		}
		playerTeam.SetTeamLeader(Proto.steamId);
		SendSuccess();
		return default;
	}
}
