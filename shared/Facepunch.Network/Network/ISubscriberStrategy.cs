using Network.Visibility;

namespace Network;

public interface ISubscriberStrategy
{
	void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible);

	void GatherSubscriptions(Networkable net, ListHashSet<Group> visible);
}
