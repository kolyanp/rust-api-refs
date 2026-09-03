using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingBlockHeight : SocketMod_BuildingBlock
{
	public float MaxHeight = 5f;

	protected override bool GetContained(Vector3 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float maxHeight = Env.oceanlevel + MaxHeight;
		return !ValidPlacementHeight(pos, sphereRadius, ((LayerMask)(ref layerMask)).value, queryTriggers, maxHeight);
	}

	public static bool ValidPlacementHeight(Vector3 pos, float sphereRadius, int layerMask, QueryTriggerInteraction queryTriggers, float maxHeight)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		List<BoatBuildingBlock> list = Pool.Get<List<BoatBuildingBlock>>();
		Vis.Entities(pos, sphereRadius, list, layerMask, queryTriggers);
		bool num = list.Count > 0;
		Pool.FreeUnmanaged<BoatBuildingBlock>(ref list);
		if (!num)
		{
			return true;
		}
		return pos.y <= maxHeight;
	}
}
