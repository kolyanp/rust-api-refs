using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PickupVolume : PrefabAttribute
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	protected override Type GetIndexedType()
	{
		return typeof(PickupVolume);
	}

	public static bool Check(Vector3 position, Quaternion rotation, PickupVolume[] volumes, BaseEntity ignoreEntity = null)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < volumes.Length; i++)
		{
			if (volumes[i].CheckInternal(position, rotation, 256, ignoreEntity))
			{
				return true;
			}
		}
		return false;
	}

	protected bool CheckInternal(Vector3 position, Quaternion rotation, int mask = -1, BaseEntity ignoreEntity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		position += rotation * (worldRotation * ((Bounds)(ref bounds)).center + worldPosition);
		if (CheckOBB(new OBB(position, ((Bounds)(ref bounds)).size, rotation * worldRotation), mask, this, ignoreEntity))
		{
			return true;
		}
		return false;
	}

	private static bool CheckOBB(OBB obb, int layerMask, PickupVolume volume)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return CheckOBB(obb, layerMask, volume, null);
	}

	private static bool CheckOBB(OBB obb, int layerMask, PickupVolume volume, BaseEntity ignoredEntity = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(obb, list, layerMask, (QueryTriggerInteraction)2);
		bool result = CheckFlags(list, volume, ignoredEntity);
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	public static bool CheckFlags(List<Collider> colliders, PickupVolume volume, BaseEntity ignoredEntity = null)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Collider collider in colliders)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collider);
			if ((!((Object)(object)baseEntity != (Object)null) || !((Object)(object)ignoredEntity != (Object)null) || !(baseEntity.net.ID == ignoredEntity.net.ID)) && (Object)(object)baseEntity != (Object)null && (Object)(object)baseEntity != (Object)(object)ignoredEntity)
			{
				return true;
			}
		}
		return false;
	}
}
