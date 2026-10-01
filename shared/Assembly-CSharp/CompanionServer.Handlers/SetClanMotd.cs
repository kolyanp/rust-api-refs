using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetClanMotd : BaseClanHandler<AppSendMessage>
{
	public override async ValueTask Execute()
	{
		ClanValidatorResult validatedMotd = ClanValidator.ValidateMotd(Proto.message);
		if (!validatedMotd.Success)
		{
			((BaseHandler<AppSendMessage>)this).SendError("invalid_motd");
			return;
		}
		IClan clan = await GetClan();
		if (clan == null)
		{
			((BaseHandler<AppSendMessage>)this).SendError("no_clan");
			return;
		}
		long previousTimestamp = clan.MotdTimestamp;
		ClanResult val = await clan.SetMotd(validatedMotd.Value, UserId);
		if ((int)val == 1)
		{
			SendSuccess();
			ClanPushNotifications.SendClanAnnouncement(clan, previousTimestamp, UserId);
		}
		else
		{
			SendError(val);
		}
	}
}
