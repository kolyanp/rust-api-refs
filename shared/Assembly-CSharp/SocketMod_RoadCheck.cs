using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_RoadCheck : SocketMod
{
	public float sphereRadius;

	public bool wantsCollide;

	public LayerMask layerMask;

	protected override Phrase ErrorPhrase
	{
		get
		{
			if (!wantsCollide)
			{
				return ConstructionErrors.CantPlaceOnRoad;
			}
			return ConstructionErrors.MustPlaceOnRoad;
		}
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = place.position + place.rotation * worldPosition;
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(position, sphereRadius, list, ((LayerMask)(ref layerMask)).value, (QueryTriggerInteraction)2);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			Collider val = list[i];
			if (!place.ShouldIgnoreCollider(val) && (Object)(object)val != (Object)null && ((Component)val).gameObject.HasCustomTag(GameObjectTag.Road))
			{
				flag = true;
				break;
			}
		}
		bool num = wantsCollide == flag;
		Pool.FreeUnmanaged<Collider>(ref list);
		if (num)
		{
			return true;
		}
		return false;
	}

	public SocketMod_RoadCheck()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		sphereRadius = 1f;
		layerMask = LayerMask.op_Implicit(65536);
		base._002Ector();
	}
}
