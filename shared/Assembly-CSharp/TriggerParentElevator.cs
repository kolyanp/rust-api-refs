using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class TriggerParentElevator : TriggerParentEnclosed
{
	public bool AllowHorsesToBypassClippingChecks = true;

	public bool AllowBikesToBypassClippingChecks = true;

	public bool IgnoreParentEntityColliders;

	protected override bool IsClipping(BaseEntity ent)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (AllowHorsesToBypassClippingChecks && ent is RidableHorse)
		{
			return false;
		}
		if ((AllowBikesToBypassClippingChecks && ent is Bike) || ent is Snowmobile)
		{
			return false;
		}
		if (IgnoreParentEntityColliders)
		{
			List<Collider> list = Pool.Get<List<Collider>>();
			GamePhysics.OverlapOBB(ent.WorldSpaceBounds(), list, 1218511105, (QueryTriggerInteraction)1);
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)this).gameObject);
			foreach (Collider item in list)
			{
				BaseEntity baseEntity2 = GameObjectEx.ToBaseEntity(item);
				if ((Object)(object)baseEntity2 != (Object)null)
				{
					if (!((Object)(object)baseEntity2 == (Object)(object)baseEntity) && !(baseEntity2 is Elevator))
					{
					}
					continue;
				}
				return true;
			}
			Pool.FreeUnmanaged<Collider>(ref list);
			return false;
		}
		return base.IsClipping(ent);
	}
}
