using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_PlantCheck : SocketMod
{
	public bool CanBePotted = true;

	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public QueryTriggerInteraction queryTriggers;

	public bool wantsCollide;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!CanBePotted && (Object)(object)place.transform != (Object)null)
		{
			PlanterBox planterBox = GameObjectEx.ToBaseEntity(place.transform) as PlanterBox;
			if ((Object)(object)planterBox != (Object)null && planterBox.PlantPot)
			{
				return false;
			}
		}
		Vector3 position = place.position + place.rotation * worldPosition;
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, sphereRadius, list, ((LayerMask)(ref layerMask)).value, queryTriggers);
		if (isServer)
		{
			List<BaseEntity> list2 = Pool.Get<List<BaseEntity>>();
			BaseEntity.Query.Server.GetInSphere(position, sphereRadius, list2);
			list.AddRange(list2);
			Pool.FreeUnmanaged<BaseEntity>(ref list2);
		}
		bool result = !wantsCollide;
		foreach (BaseEntity item in list)
		{
			if (!place.ShouldIgnoreEntity(item))
			{
				GrowableEntity component = ((Component)item).GetComponent<GrowableEntity>();
				if (Object.op_Implicit((Object)(object)component) && wantsCollide)
				{
					result = true;
					break;
				}
				if (Object.op_Implicit((Object)(object)component) && !wantsCollide)
				{
					result = false;
					break;
				}
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		return result;
	}
}
