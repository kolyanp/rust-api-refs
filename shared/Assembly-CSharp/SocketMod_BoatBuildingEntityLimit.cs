using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingEntityLimit : SocketMod
{
	public BaseEntity LimitedEntityType;

	public int Limit = 1;

	protected override Phrase ErrorPhrase => ConstructionErrors.BoatBuildingEntityLimit;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 point = place.position + place.rotation * worldPosition;
		return IsBelowLimit(point);
	}

	private bool IsBelowLimit(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<TriggerBoatBuildingArea> list = Pool.Get<List<TriggerBoatBuildingArea>>();
		Vis.Components<TriggerBoatBuildingArea>(point, 3f, list, 262144, (QueryTriggerInteraction)2);
		int num = 0;
		foreach (TriggerBoatBuildingArea item in list)
		{
			BoatBuildingStation boatBuildingStation = GameObjectEx.ToBaseEntity(((Component)item).gameObject) as BoatBuildingStation;
			if ((Object)(object)boatBuildingStation == (Object)null || boatBuildingStation.isServer != isServer)
			{
				continue;
			}
			List<BaseEntity> entitiesInBuildArea = BoatBuildingStation.GetEntitiesInBuildArea<BaseEntity>(boatBuildingStation.BuildArea, 256, isServer);
			foreach (BaseEntity item2 in entitiesInBuildArea)
			{
				if (!((Object)(object)item2 == (Object)null) && item2.isServer == isServer && ((object)item2).GetType() == ((object)LimitedEntityType).GetType())
				{
					num++;
					if (num >= Limit)
					{
						Pool.FreeUnmanaged<BaseEntity>(ref entitiesInBuildArea);
						Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
						return false;
					}
				}
			}
			Pool.FreeUnmanaged<BaseEntity>(ref entitiesInBuildArea);
		}
		Pool.FreeUnmanaged<TriggerBoatBuildingArea>(ref list);
		return true;
	}
}
