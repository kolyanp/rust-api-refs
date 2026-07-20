using Facepunch;
using Network.Visibility;

namespace Network;

public class DefaultSubscriberStrategy : ISubscriberStrategy
{
	public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
	{
		GatherHighPrioSubscriptions(net.group, net.secondaryGroup, net.sv.visibility, visible);
	}

	public void GatherHighPrioSubscriptions(Group primaryGroup, Group secondaryGroup, Manager visManager, ListHashSet<Group> visible)
	{
		visManager.GetVisibleFromNear(primaryGroup, visible);
		if (secondaryGroup != null)
		{
			AddSecondaryGroup(secondaryGroup, visManager, visible);
		}
	}

	public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
	{
		GatherSubscriptions(net.group, net.secondaryGroup, net.sv.visibility, visible);
	}

	public void GatherSubscriptions(Group primaryGroup, Group secondaryGroup, Manager visManager, ListHashSet<Group> visible)
	{
		visManager.GetVisibleFromFar(primaryGroup, visible);
		if (secondaryGroup != null)
		{
			AddSecondaryGroup(secondaryGroup, visManager, visible);
		}
	}

	private void AddSecondaryGroup(Group additionalGroup, Manager visManager, ListHashSet<Group> groupsVisible)
	{
		ListHashSet<Group> val = Pool.Get<ListHashSet<Group>>();
		visManager.GetVisibleFromNear(additionalGroup, val);
		for (int i = 0; i < val.Count; i++)
		{
			groupsVisible.TryAdd(val[i]);
		}
		Pool.FreeUnmanaged<Group>(ref val);
	}
}
