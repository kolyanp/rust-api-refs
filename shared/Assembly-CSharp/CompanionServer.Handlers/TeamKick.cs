using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class TeamKick : BasePlayerHandler<AppTeamKick>
{
	public override ValueTask Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(UserId);
		if (playerTeam == null)
		{
			SendError("no_team");
			return default;
		}
		if (Proto.steamId != UserId && playerTeam.teamLeader != UserId)
		{
			SendError("access_denied");
			return default;
		}
		if (!playerTeam.members.Contains(Proto.steamId))
		{
			SendError("not_found");
			return default;
		}
		playerTeam.RemovePlayer(Proto.steamId);
		SendSuccess();
		return default;
	}
}
