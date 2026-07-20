using UnityEngine;

public class SocketMod_Anchor : SocketMod
{
	public float Radius = 0.38f;

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
		return CanSeeNetting(pos);
	}

	private bool CanSeeNetting(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		pos += Vector3.up;
		if (GamePhysics.Trace(new Ray(pos, Vector3.down), Radius, out var hitInfo, 10f, 144769024, (QueryTriggerInteraction)0))
		{
			return (Object)(object)((Component)((RaycastHit)(ref hitInfo)).collider).GetComponent<BoatBuildingNetting>() != (Object)null;
		}
		return false;
	}
}
