using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_AreaCheck : SocketMod
{
	public Bounds bounds;

	public LayerMask layerMask;

	public bool wantsInside;

	public bool ignoreAntiLargeVehicleCheck;

	private Phrase lastError;

	protected override Phrase ErrorPhrase => lastError;

	public static bool IsInArea(Vector3 position, OBB obb, LayerMask layerMask, out bool foundParent, bool wantsInside = true, bool shouldParent = false, BaseEntity parentEntity = null, BaseEntity ignoredEntity = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(obb, list, ((LayerMask)(ref layerMask)).value, (QueryTriggerInteraction)0);
		foundParent = false;
		if ((Object)(object)ignoredEntity != (Object)null)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(list[num]);
				if (!((Object)(object)baseEntity == (Object)null))
				{
					if (baseEntity.isServer != ignoredEntity.isServer)
					{
						list.RemoveAt(num);
					}
					else if ((Object)(object)baseEntity == (Object)(object)ignoredEntity)
					{
						list.RemoveAt(num);
					}
				}
			}
		}
		if (shouldParent & wantsInside)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if ((Object)(object)GameObjectEx.ToBaseEntity(list[i]) == (Object)(object)parentEntity)
				{
					foundParent = true;
					break;
				}
				if (parentEntity is PlayerBoat playerBoat)
				{
					OBB val = playerBoat.WorldSpaceBounds();
					if (((OBB)(ref val)).Contains(position))
					{
						foundParent = true;
					}
					break;
				}
			}
		}
		bool result = list.Count > 0;
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	private bool SocketCanTargetBoats()
	{
		if (baseSocket == null || baseSocket.socketMods == null)
		{
			if (wantsInside)
			{
				return AcceptsLargeVehicles(this);
			}
			return false;
		}
		SocketMod[] socketMods = baseSocket.socketMods;
		foreach (SocketMod socketMod in socketMods)
		{
			if (socketMod is SocketMod_BoatBuildingBlock { wantsCollide: not false })
			{
				return true;
			}
			if (socketMod is SocketMod_AreaCheck { wantsInside: not false } socketMod_AreaCheck && AcceptsLargeVehicles(socketMod_AreaCheck))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AcceptsLargeVehicles(SocketMod_AreaCheck check)
	{
		if (!check.ignoreAntiLargeVehicleCheck)
		{
			return (((LayerMask)(ref check.layerMask)).value & 0x8000000) != 0;
		}
		return true;
	}

	public bool DoCheck(Vector3 position, Quaternion rotation, BaseEntity entity = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position + rotation * worldPosition;
		Quaternion val2 = rotation * worldRotation;
		bool foundParent;
		return IsInArea(position, new OBB(val, val2, bounds), layerMask, out foundParent, wantsInside, shouldParent: false, null, entity) == wantsInside;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = place.position + place.rotation * worldPosition;
		Quaternion val2 = place.rotation * worldRotation;
		bool flag = IsInArea(val, new OBB(val, val2, bounds), layerMask, out var foundParent, wantsInside, !place.parentPassed && place.shouldParent, ((Object)(object)place.transform != (Object)null) ? GameObjectEx.ToBaseEntity(place.transform) : null, place.ignoredEntity) == wantsInside;
		place.parentPassed |= foundParent;
		if (!flag)
		{
			lastError = ConstructionErrors.NotStableEnough;
			if (LayerMask.op_Implicit(layerMask) == 2097152 || LayerMask.op_Implicit(layerMask) == 136314880)
			{
				lastError = (wantsInside ? ConstructionErrors.MustPlaceOnConstruction : ConstructionErrors.CantPlaceOnConstruction);
			}
		}
		else if (!ignoreAntiLargeVehicleCheck && wantsInside && (LayerMask.op_Implicit(layerMask) & 0x8000000) == 0)
		{
			flag = !GamePhysics.CheckSphere(place.position, 5f, 134217728, (QueryTriggerInteraction)0);
			if (!flag)
			{
				lastError = ConstructionErrors.InvalidAreaVehicleLarge;
			}
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	public SocketMod_AreaCheck()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		bounds = new Bounds(Vector3.zero, Vector3.one * 0.1f);
		wantsInside = true;
		lastError = new Phrase("", "");
		base._002Ector();
	}
}
