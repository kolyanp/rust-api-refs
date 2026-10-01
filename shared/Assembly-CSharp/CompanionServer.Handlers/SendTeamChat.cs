using System.Threading.Tasks;
using ConVar;
using Facepunch.Extend;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SendTeamChat : BasePlayerHandler<AppSendMessage>
{
	protected override double TokenCost => 2.0;

	public override async ValueTask Execute()
	{
		string text = Proto.message?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			SendSuccess();
			return;
		}
		text = StringExtensions.Truncate(text, 256, "…");
		string username = Player?.displayName ?? SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(UserId) ?? "[unknown]";
		if (await Chat.sayAs(Chat.ChatChannel.Team, UserId, username, text, Player))
		{
			SendSuccess();
		}
		else
		{
			SendError("message_not_sent");
		}
	}
}
