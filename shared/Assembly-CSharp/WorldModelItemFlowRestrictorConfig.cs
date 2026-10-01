using System;
using UnityEngine;

public class WorldModelItemFlowRestrictorConfig : PrefabAttribute, IClientComponent
{
	public Vector3 displayLocalPosition;

	public Vector3 displayLocalEulerAngles;

	public Vector3 displayLocalScale = Vector3.one;

	protected override Type GetIndexedType()
	{
		return typeof(WorldModelItemFlowRestrictorConfig);
	}

	public WorldModelItemFlowRestrictorConfig()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
