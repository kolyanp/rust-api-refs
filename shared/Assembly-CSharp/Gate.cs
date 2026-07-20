using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class Gate : Door
{
	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			Vis.Entities(WorldSpaceBounds(), (List<BaseEntity>)(object)val, -2145386240, (QueryTriggerInteraction)2);
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && !item.isClient && !((Object)(object)item == (Object)(object)this) && !(item is BuildingBlock) && !(item is SimpleBuildingBlock) && !(item is Door) && !(item is BaseOven) && !(item is Barricade))
				{
					if (!string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(item)))
					{
						SprayCan.LastReskinError = ConstructionErrors.BlockedBy;
						SprayCan.LastReskinErrorEntity = item;
					}
					else
					{
						SprayCan.LastReskinError = SprayCan.BlockedBySomething;
					}
					return false;
				}
			}
			return base.CanBeRedirectSwapped(player);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
