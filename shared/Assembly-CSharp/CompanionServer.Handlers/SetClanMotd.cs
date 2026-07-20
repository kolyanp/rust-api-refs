using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetClanMotd : BaseClanHandler<AppSendMessage>
{
	public override async void Execute()
	{
		ClanValidatorResult validatedMotd = ClanValidator.ValidateMotd(base.Proto.message);
		if (!((ClanValidatorResult)(ref validatedMotd)).Success)
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
		ClanResult val = await clan.SetMotd(((ClanValidatorResult)(ref validatedMotd)).Value, base.UserId);
		if ((int)val == 1)
		{
			SendSuccess();
			ClanPushNotifications.SendClanAnnouncement(clan, previousTimestamp, base.UserId);
		}
		else
		{
			SendError(val);
		}
	}
}
