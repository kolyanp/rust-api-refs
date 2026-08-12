using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_BoatBuildingNettingPoint : SocketMod
{
	private static Phrase lastError;

	protected override Phrase ErrorPhrase => lastError;

	public static bool IsOnBoatBuildingNetting(Vector3 vPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAllUnordered(new Ray(vPoint + Vector3.up * 3f, Vector3.down), 0f, list, 3f, 8388608, (QueryTriggerInteraction)0);
		foreach (RaycastHit item in list)
		{
			RaycastHit current = item;
			if (((Component)((RaycastHit)(ref current)).collider).gameObject.CompareTag("BoatBuildingNetting"))
			{
				Pool.FreeUnmanaged<RaycastHit>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return false;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsOnBoatBuildingNetting(vPoint))
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.MustPlaceOnNetting;
		}
		return false;
	}

	static SocketMod_BoatBuildingNettingPoint()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		lastError = new Phrase("", "");
	}
}
