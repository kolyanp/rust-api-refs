using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class DeployVolumeRequireBoatBuildingVolume : DeployVolume
{
	public List<Transform> Points = new List<Transform>();

	protected override bool Check(Vector3 position, Quaternion rotation, int mask = -1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		List<TriggerBoatBuildingArea> list = Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components<TriggerBoatBuildingArea>(position, 3f, list, 262144, (QueryTriggerInteraction)2);
		using (List<TriggerBoatBuildingArea>.Enumerator enumerator = list.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				TriggerBoatBuildingArea current = enumerator.Current;
				Vector3 position2 = ((Component)current).transform.position;
				Vector3 lossyScale = ((Component)current).transform.lossyScale;
				Quaternion rotation2 = ((Component)current).transform.rotation;
				OBB val = default(OBB);
				((OBB)(ref val))._002Ector(position2, lossyScale, rotation2);
				foreach (Transform point in Points)
				{
					Vector3 val2 = position + rotation * point.position;
					if (!((OBB)(ref val)).Contains(val2))
					{
						Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
						return true;
					}
				}
				Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
				return false;
			}
		}
		Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
		return true;
	}

	protected override bool Check(Vector3 position, Quaternion rotation, List<Type> types, TypeFilterMode filterMode, BaseEntity ignoredEntity = null, int mask = -1, bool ignoreChildrenOfEntity = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return Check(position, rotation, mask);
	}

	protected override bool Check(Vector3 position, Quaternion rotation, OBB obb, int mask = -1)
	{
		return false;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
