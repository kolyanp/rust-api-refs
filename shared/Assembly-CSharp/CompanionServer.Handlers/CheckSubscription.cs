using System.Threading.Tasks;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class CheckSubscription : BaseEntityHandler<AppEmpty>
{
	public override ValueTask Execute()
	{
		if (Entity is ISubscribable subscribable)
		{
			bool value = subscribable.HasSubscription(UserId);
			SendFlag(value);
		}
		else
		{
			SendError("wrong_type");
		}
		return default;
	}
}
