using System.Threading.Tasks;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class TeamInfo : BasePlayerHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(UserId);
		AppTeamInfo teamInfo = ((playerTeam == null) ? Player.GetAppTeamInfo(UserId) : playerTeam.GetAppTeamInfo(UserId));
		AppResponse val = Pool.Get<AppResponse>();
		val.teamInfo = teamInfo;
		Send(val);
		return default;
	}
}
