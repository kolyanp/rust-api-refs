using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BuildingBlock : SocketMod
{
	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public QueryTriggerInteraction queryTriggers;

	public bool wantsCollide;

	protected override Phrase ErrorPhrase => ConstructionErrors.MustPlaceOnConstruction;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = place.position + place.rotation * worldPosition;
		bool contained = GetContained(pos);
		if (!contained || !wantsCollide)
		{
			if (!contained)
			{
				return !wantsCollide;
			}
			return false;
		}
		return true;
	}

	protected virtual bool GetContained(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		List<BuildingBlock> list = Pool.Get<List<BuildingBlock>>();
		Vis.Entities(pos, sphereRadius, list, ((LayerMask)(ref layerMask)).value, queryTriggers);
		bool result = list.Count > 0;
		Pool.FreeUnmanaged<BuildingBlock>(ref list);
		return result;
	}
}
