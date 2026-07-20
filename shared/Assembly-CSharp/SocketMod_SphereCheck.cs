using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_SphereCheck : SocketMod
{
	public float sphereRadius = 1f;

	public LayerMask layerMask;

	public bool wantsCollide;

	public bool requireMonument;

	public bool ignoreAntiLargeVehicleCheck;

	[Space]
	public BaseEntity[] entityWhitelist;

	private Phrase lastError = new Phrase("", "");

	protected override Phrase ErrorPhrase => lastError;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = place.position + place.rotation * worldPosition;
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(position, sphereRadius, list, ((LayerMask)(ref layerMask)).value, (QueryTriggerInteraction)2);
		if ((Object)(object)place.ignoredEntity != (Object)null)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (place.ShouldIgnoreCollider(list[num]))
				{
					list.RemoveAt(num);
				}
			}
		}
		if (requireMonument)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Collider val = list[i];
				if (!((Component)val).gameObject.HasCustomTag(GameObjectTag.BlockBarricadePlacement) && ((Object)(object)ColliderEx.GetMonument(val) == (Object)null || ((Component)val).gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement)))
				{
					list.RemoveAt(i);
					i--;
				}
			}
		}
		bool flag = wantsCollide == list.Count > 0;
		if (entityWhitelist.Length != 0)
		{
			foreach (Collider item in list)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
				if ((Object)(object)baseEntity != (Object)null)
				{
					flag = DeployVolume.CheckEntityList(baseEntity, entityWhitelist, trueIfAnyFound: true);
				}
			}
		}
		if (!flag)
		{
			lastError = ConstructionErrors.NotStableEnough;
			if (LayerMask.op_Implicit(layerMask) == 2097152 || LayerMask.op_Implicit(layerMask) == 136314880)
			{
				lastError = (wantsCollide ? ConstructionErrors.MustPlaceOnConstruction : ConstructionErrors.CantPlaceOnConstruction);
			}
			else if (!wantsCollide && requireMonument)
			{
				lastError = ConstructionErrors.CantPlaceOnMonument;
			}
			else if (!wantsCollide && list.Count > 0)
			{
				foreach (Collider item2 in list)
				{
					string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit = item2);
					if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
					{
						Construction.lastPlacementErrorIsDetailed = true;
						lastError = Phrase.op_Implicit(blockedByErrorFromCollider);
						break;
					}
				}
			}
		}
		else if (!ignoreAntiLargeVehicleCheck && wantsCollide && (LayerMask.op_Implicit(layerMask) & 0x8000000) == 0)
		{
			flag = !GamePhysics.CheckSphere(place.position, 5f, 134217728, (QueryTriggerInteraction)0);
			if (!flag)
			{
				lastError = ConstructionErrors.InvalidAreaVehicleLarge;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		if (flag)
		{
			return true;
		}
		return false;
	}
}
