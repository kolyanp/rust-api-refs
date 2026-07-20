using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingNetting : SocketMod_BuildingBlock
{
	protected override Phrase ErrorPhrase
	{
		get
		{
			if (!wantsCollide)
			{
				return ConstructionErrors.CantPlaceOnNetting;
			}
			return ConstructionErrors.MustPlaceOnNetting;
		}
	}

	protected override bool GetContained(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		List<BoatBuildingNetting> list = Pool.Get<List<BoatBuildingNetting>>();
		Vis.Components<BoatBuildingNetting>(pos, sphereRadius, list, ((LayerMask)(ref layerMask)).value, queryTriggers);
		bool result = list.Count > 0;
		Pool.FreeUnmanaged<BoatBuildingNetting>(ref list);
		return result;
	}
}
