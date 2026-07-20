using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class DeployVolumeEntityBoundsReverse : DeployVolume
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	public int layer;

	protected override bool Check(Vector3 position, Quaternion rotation, int mask = -1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		position += rotation * ((Bounds)(ref bounds)).center;
		OBB test = default(OBB);
		((OBB)(ref test))._002Ector(position, ((Bounds)(ref bounds)).size, rotation);
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, ((Vector3)(ref test.extents)).magnitude, list, LayerMask.op_Implicit(layers) & mask, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			DeployVolume[] array = PrefabAttribute.server.FindAll<DeployVolume>(item.prefabID);
			List<DeployVolume> list2 = Pool.Get<List<DeployVolume>>();
			DeployVolume[] array2 = array;
			foreach (DeployVolume deployVolume in array2)
			{
				if (DeployVolume.ShouldApplyVolumeForEntity(deployVolume, item))
				{
					list2.Add(deployVolume);
				}
			}
			if (DeployVolume.Check(((Component)item).transform.position, ((Component)item).transform.rotation, list2, test, 1 << layer))
			{
				Pool.FreeUnmanaged<DeployVolume>(ref list2);
				Pool.FreeUnmanaged<BaseEntity>(ref list);
				return true;
			}
			Pool.FreeUnmanaged<DeployVolume>(ref list2);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		return false;
	}

	protected override bool Check(Vector3 position, Quaternion rotation, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Check(position, rotation, mask);
	}

	protected override bool Check(Vector3 position, Quaternion rotation, OBB test, int mask = -1)
	{
		return false;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
		layer = rootObj.layer;
	}
}
