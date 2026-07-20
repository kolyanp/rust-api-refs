using System;
using Facepunch;
using UnityEngine;

namespace Network.Visibility;

public class Manager : IDisposable
{
	public Provider provider;

	public void Dispose()
	{
		provider = null;
	}

	public Manager(Provider p)
	{
		provider = p;
	}

	public Group TryGet(uint ID)
	{
		provider.TryGetGroup(ID, out var group);
		return group;
	}

	public Group Get(uint ID)
	{
		return provider.GetGroup(ID);
	}

	public Subscriber CreateSubscriber(Connection connection)
	{
		Subscriber subscriber = Pool.Get<Subscriber>();
		subscriber.manager = this;
		subscriber.connection = connection;
		return subscriber;
	}

	public void DestroySubscriber(ref Subscriber subscriber)
	{
		subscriber.Destroy();
		Pool.Free<Subscriber>(ref subscriber);
	}

	public bool IsInside(Group group, Vector3 vPos, EntityNetworkRange range)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (group == null)
		{
			return false;
		}
		return provider.IsInside(group, vPos, range);
	}

	public bool IsVisibleFromFar(Group from, Group to)
	{
		if (from == null || to == null)
		{
			return false;
		}
		return provider.IsVisibleFromFar(from, to);
	}

	public bool IsVisibleFromNear(Group from, Group to)
	{
		if (from == null || to == null)
		{
			return false;
		}
		return provider.IsVisibleFromNear(from, to);
	}

	public Group GetGroup(Vector3 vPos, EntityNetworkRange range)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return provider.GetGroup(vPos, range);
	}

	public void GetVisibleFromDistance(Group center, ListHashSet<Group> groups, float radiusInWorldUnits)
	{
		if (center != null)
		{
			provider.GetVisibleFromDistance(center, groups, radiusInWorldUnits);
		}
	}

	public void GetVisibleFromFar(Group center, ListHashSet<Group> groups)
	{
		if (center != null)
		{
			provider.GetVisibleFromFar(center, groups);
		}
	}

	public void GetVisibleFromNear(Group center, ListHashSet<Group> groups)
	{
		if (center != null)
		{
			provider.GetVisibleFromNear(center, groups);
		}
	}

	public int PositionToLayer(float x, float y, float z, EntityNetworkRange range)
	{
		return provider.PositionToLayer(x, y, z, range);
	}

	public (int x, int y, int layer) DeconstructGroupId(int groupId)
	{
		return provider.DeconstructGroupId(groupId);
	}

	public bool IsGroupIdSpecial(uint groupId)
	{
		return provider.IsGroupIdSpecial(groupId);
	}

	public float GetFarDistanceForRange(EntityNetworkRange range)
	{
		return provider.GetFarDistanceForRange(range);
	}

	public void ForEachGroup(int layer, Action<Group> callback)
	{
		provider.ForEach(layer, callback);
	}

	public void AddGroups(int layer, ListHashSet<Group> groups, bool create)
	{
		provider.AddGroups(layer, groups, create);
	}
}
