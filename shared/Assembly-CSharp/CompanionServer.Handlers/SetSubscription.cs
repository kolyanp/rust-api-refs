using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class SetSubscription : BaseEntityHandler<AppFlag>
{
	public override ValueTask Execute()
	{
		if (Entity is ISubscribable subscribable)
		{
			if (Proto.value)
			{
				if (!subscribable.AddSubscription(UserId))
				{
					SendError("too_many_subscribers");
					return default;
				}
			}
			else
			{
				subscribable.RemoveSubscription(UserId);
			}
			SendSuccess();
		}
		else
		{
			SendError("wrong_type");
		}
		return default;
	}
}
