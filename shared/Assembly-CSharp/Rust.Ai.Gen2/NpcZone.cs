using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcZone : MonoBehaviour, IServerComponent
{
	private static List<NpcZone> zones = new List<NpcZone>();

	public Bounds bounds = new Bounds(Vector3.zero, new Vector3(10f, 3.5f, 10f));

	public bool drawBounds = true;

	private void Awake()
	{
		zones.Add(this);
	}

	private void OnDestroy()
	{
		zones.Remove(this);
	}

	public bool IsPointInside(BaseEntity querier, Vector3 point)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcZone.IsPointInside"))
		{
			OBB val = new OBB(((Component)this).transform.position, ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, bounds);
			return ((OBB)(ref val)).Contains(point);
		}
	}

	public static NpcZone GetForPoint(BaseEntity querier, Vector3 point, bool fallBackToNearest = false)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcZone.GetForPoint"))
		{
			if (zones == null || zones.Count == 0)
			{
				return null;
			}
			foreach (NpcZone zone in zones)
			{
				if (!((Object)(object)zone == (Object)null) && zone.IsPointInside(querier, point))
				{
					return zone;
				}
			}
			if (!fallBackToNearest)
			{
				return null;
			}
			float num = float.PositiveInfinity;
			NpcZone result = zones[0];
			foreach (NpcZone zone2 in zones)
			{
				if (!((Object)(object)zone2 == (Object)null) && !((Object)(object)((Component)zone2).transform == (Object)null))
				{
					float num2 = Vector3.Distance(((Component)zone2).transform.position, point);
					if (num2 < num)
					{
						num = num2;
						result = zone2;
					}
				}
			}
			return result;
		}
	}
}
