namespace CompanionServer.Handlers;

public abstract class BasePlayerHandler<T> : BaseHandler<T> where T : class
{
	protected ulong UserId { get; private set; }

	protected BasePlayer Player { get; private set; }

	public override void EnterPool()
	{
		base.EnterPool();
		UserId = 0uL;
		Player = null;
	}

	public override ValidationResult Validate()
	{
		ValidationResult validationResult = base.Validate();
		if (validationResult != ValidationResult.Success)
		{
			return validationResult;
		}
		if (Client.ChannelSteamId != 0L && Client.ChannelSteamId != Request.playerId)
		{
			return ValidationResult.NotFound;
		}
		int orGenerateAppToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(Request.playerId, out var locked);
		if (Request.playerId == 0L || Request.playerToken != orGenerateAppToken)
		{
			return ValidationResult.NotFound;
		}
		if (locked)
		{
			return ValidationResult.Banned;
		}
		if ((ServerUsers.Get(Request.playerId)?.group ?? ServerUsers.UserGroup.None) == ServerUsers.UserGroup.Banned)
		{
			return ValidationResult.Banned;
		}
		TokenBucket tokenBucket = PlayerBuckets?.Get(Request.playerId);
		if (tokenBucket == null || !tokenBucket.TryTake(TokenCost))
		{
			if (tokenBucket == null || !tokenBucket.IsNaughty)
			{
				return ValidationResult.RateLimit;
			}
			return ValidationResult.Rejected;
		}
		UserId = Request.playerId;
		Player = BasePlayer.FindByID(UserId) ?? BasePlayer.FindSleeping(UserId);
		Client.Subscribe(new PlayerTarget(UserId));
		return ValidationResult.Success;
	}
}
