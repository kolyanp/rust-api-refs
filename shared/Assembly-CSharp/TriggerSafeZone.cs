using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSafeZone : TriggerBase
{
	public bool IncludePlayerBoats;

	public static List<TriggerSafeZone> allSafeZones = new List<TriggerSafeZone>();

	public float maxDepth = 20f;

	public float maxAltitude = -1f;

	[NonSerialized]
	public ApartmentBuilding Apartment;

	public Collider triggerCollider { get; private set; }

	protected override void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		triggerCollider = ((Component)this).GetComponent<Collider>();
		base.InterestLayers = LayerMask.op_Implicit(LayerMask.op_Implicit(base.InterestLayers) | 0x200);
	}

	protected void OnEnable()
	{
		allSafeZones.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		allSafeZones.Remove(this);
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		if (IncludePlayerBoats && baseEntity is BoatBuildingBlock)
		{
			PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(baseEntity);
			if (parentPlayerBoat == null)
			{
				return null;
			}
			return ((Component)parentPlayerBoat).gameObject;
		}
		return ((Component)baseEntity).gameObject;
	}

	public bool PassesHeightChecks(Vector3 entPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float num = Mathf.Abs(position.y - entPos.y);
		if (maxDepth != -1f && entPos.y < position.y && num > maxDepth)
		{
			return false;
		}
		if (maxAltitude != -1f && entPos.y > position.y && num > maxAltitude)
		{
			return false;
		}
		return true;
	}

	public float GetSafeLevel(Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (!PassesHeightChecks(pos))
		{
			return 0f;
		}
		return 1f;
	}

	private static bool CheckIntersects(in OBB bounds, Collider trigger)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		SphereCollider val = (SphereCollider)(object)((trigger is SphereCollider) ? trigger : null);
		OBB val2;
		if (val != null)
		{
			val2 = bounds;
			return Vector3.Distance(((OBB)(ref val2)).ClosestPoint(((Component)val).transform.position), ((Component)val).transform.position) < val.radius;
		}
		BoxCollider val3 = (BoxCollider)(object)((trigger is BoxCollider) ? trigger : null);
		if (val3 != null)
		{
			val2 = bounds;
			return ((OBB)(ref val2)).Intersects(new OBB(((Component)trigger).transform, new Bounds(val3.center, val3.size)));
		}
		throw new NotSupportedException("Unsupported safezone collider type: " + ((object)trigger).GetType().Name);
	}

	public static bool IsBoundsInsideSafeZone(OBB worldSpaceBound, bool checkCombatZones = true)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.safeZone)
		{
			return false;
		}
		bool flag = false;
		foreach (TriggerSafeZone allSafeZone in allSafeZones)
		{
			if (CheckIntersects(in worldSpaceBound, allSafeZone.triggerCollider))
			{
				flag = true;
				break;
			}
		}
		if (flag && checkCombatZones)
		{
			foreach (TriggerSafeZoneOverride allHostileZone in TriggerSafeZoneOverride.allHostileZones)
			{
				if (allHostileZone.IsCombatActive && CheckIntersects(in worldSpaceBound, allHostileZone.triggerCollider))
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}
}
