using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingBlock : SocketMod_BuildingBlock
{
	public enum BoatBuildFailReason
	{
		None,
		NotOnHull,
		CannotBePlacedOnBoat
	}

	public bool RequireHull;

	public bool RequireNoParentBoat = true;

	private BoatBuildFailReason lastFailReason;

	protected override Phrase ErrorPhrase => (Phrase)(lastFailReason switch
	{
		BoatBuildFailReason.NotOnHull => ConstructionErrors.RequiresHull, 
		BoatBuildFailReason.CannotBePlacedOnBoat => ConstructionErrors.CannotPlaceOnBoat, 
		_ => ConstructionErrors.MustPlaceOnBoat, 
	});

	protected override bool GetContained(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Contained(pos, sphereRadius, ((LayerMask)(ref layerMask)).value, queryTriggers, RequireHull, RequireNoParentBoat, out lastFailReason);
		if (flag && !wantsCollide)
		{
			lastFailReason = BoatBuildFailReason.CannotBePlacedOnBoat;
			return true;
		}
		return flag;
	}

	public static bool Contained(Vector3 pos, float sphereRadius, int layerMask, QueryTriggerInteraction queryTriggers, bool requireHull, bool requireNoParentBoat, out BoatBuildFailReason failReason)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		failReason = BoatBuildFailReason.None;
		List<BoatBuildingBlock> list = Pool.Get<List<BoatBuildingBlock>>();
		Vis.Entities(pos, sphereRadius, list, layerMask, queryTriggers);
		bool flag = list.Count > 0;
		if (flag && (requireHull || requireNoParentBoat))
		{
			flag = false;
			foreach (BoatBuildingBlock item in list)
			{
				if (requireHull && !item.Hull)
				{
					failReason = BoatBuildFailReason.NotOnHull;
				}
				else if (!requireNoParentBoat || !(item.GetParentEntity() is PlayerBoat))
				{
					flag = true;
					break;
				}
			}
		}
		Pool.FreeUnmanaged<BoatBuildingBlock>(ref list);
		return flag;
	}
}
