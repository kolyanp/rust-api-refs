using System;
using UnityEngine;

namespace Network.Visibility;

public interface Provider
{
	bool IsInside(Group group, Vector3 vPos, EntityNetworkRange range);

	bool IsVisibleFromFar(Group from, Group to);

	bool IsVisibleFromNear(Group from, Group to);

	Group GetGroup(Vector3 vPos, EntityNetworkRange range);

	Group GetGroup(uint groupId);

	bool TryGetGroup(uint groupId, out Group group);

	void GetVisibleFromDistance(Group group, ListHashSet<Group> groups, float radiusInWorldUnits);

	void GetVisibleFromFar(Group group, ListHashSet<Group> groups);

	void GetVisibleFromNear(Group group, ListHashSet<Group> groups);

	int PositionToLayer(float x, float y, float z, EntityNetworkRange range);

	(int x, int y, int layer) DeconstructGroupId(int groupId);

	bool IsGroupIdSpecial(uint groupId);

	float GetFarDistanceForRange(EntityNetworkRange range);

	void ForEach(int layer, Action<Group> callback);

	void AddGroups(int layer, ListHashSet<Group> groups, bool create);
}
