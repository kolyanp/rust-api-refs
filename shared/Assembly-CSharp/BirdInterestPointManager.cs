using System.Collections.Generic;
using Development.Attributes;
using UnityEngine;

[ResetStaticFields]
public static class BirdInterestPointManager
{
	public struct InterestPoint
	{
		public int id;

		public BirdSpecies species;

		public Vector3 position;

		public float priority;

		public float circleRadius;

		public float circleHeight;

		public float expireTime;

		public BirdFlock claimant;
	}

	private const float zoneRefreshInterval = 10f;

	private const float zonePointDuration = 30f;

	private static readonly List<InterestPoint> points = new List<InterestPoint>();

	private static int nextId;

	private static readonly List<BirdInterestZone> zones = new List<BirdInterestZone>();

	private static float nextZoneRefresh;

	public static void AddZone(BirdInterestZone zone)
	{
		zones.Add(zone);
	}

	public static void RemoveZone(BirdInterestZone zone)
	{
		zones.Remove(zone);
		Remove(zone.pointId);
	}

	public static int Add(BirdSpecies species, Vector3 position, float priority = 0f, float duration = 300f, float circleRadius = 25f, float circleHeight = 20f)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RemoveExpired();
		points.Add(new InterestPoint
		{
			id = ++nextId,
			species = species,
			position = position,
			priority = priority,
			circleRadius = circleRadius,
			circleHeight = circleHeight,
			expireTime = Time.time + duration
		});
		return nextId;
	}

	public static void Remove(int id)
	{
		int num = IndexOf(id);
		if (num >= 0)
		{
			points.RemoveAt(num);
		}
	}

	public static bool Renew(int id, float duration)
	{
		int num = IndexOf(id);
		if (num < 0)
		{
			return false;
		}
		InterestPoint value = points[num];
		value.expireTime = Time.time + duration;
		points[num] = value;
		return true;
	}

	public static bool TryGet(int id, out InterestPoint point)
	{
		RefreshZones();
		int num = IndexOf(id);
		point = ((num >= 0) ? points[num] : default(InterestPoint));
		if (num >= 0)
		{
			return Time.time < point.expireTime;
		}
		return false;
	}

	public static int Claim(BirdFlock flock)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		RefreshZones();
		RemoveExpired();
		Vector3 position = ((Component)flock).transform.position;
		bool flag = DeepSeaManager.IsInsideDeepSea(position);
		int num = -1;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		for (int i = 0; i < points.Count; i++)
		{
			InterestPoint point = points[i];
			if (point.species == flock.species && !IsClaimed(point) && DeepSeaManager.IsInsideDeepSea(point.position) == flag)
			{
				Vector3 val = point.position - position;
				float sqrMagnitude = val.sqrMagnitude;
				if (!(point.priority < num2) && (point.priority != num2 || !(sqrMagnitude >= num3)))
				{
					num = i;
					num2 = point.priority;
					num3 = sqrMagnitude;
				}
			}
		}
		if (num < 0)
		{
			return 0;
		}
		InterestPoint value = points[num];
		value.claimant = flock;
		points[num] = value;
		return value.id;
	}

	public static void Release(BirdFlock flock, int id)
	{
		int num = IndexOf(id);
		if (num >= 0 && !((Object)(object)points[num].claimant != (Object)(object)flock))
		{
			InterestPoint value = points[num];
			value.claimant = null;
			points[num] = value;
		}
	}

	private static void RefreshZones()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (Time.time < nextZoneRefresh)
		{
			return;
		}
		nextZoneRefresh = Time.time + 10f;
		foreach (BirdInterestZone zone in zones)
		{
			if (!(Time.time < zone.cooldownEnd) && zone.HasAwakePlayerNear())
			{
				if (!Renew(zone.pointId, 30f))
				{
					zone.pointId = Add(zone.species, ((Component)zone).transform.position, zone.priority, 30f, zone.circleRadius, zone.circleHeight);
					zone.activeEnd = Time.time + zone.activeDuration;
				}
				else if (Time.time >= zone.activeEnd)
				{
					Remove(zone.pointId);
					zone.cooldownEnd = Time.time + zone.cooldown;
				}
			}
		}
	}

	private static int IndexOf(int id)
	{
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].id == id)
			{
				return i;
			}
		}
		return -1;
	}

	private static void RemoveExpired()
	{
		for (int num = points.Count - 1; num >= 0; num--)
		{
			if (Time.time >= points[num].expireTime)
			{
				points.RemoveAt(num);
			}
		}
	}

	private static bool IsClaimed(InterestPoint point)
	{
		if ((Object)(object)point.claimant != (Object)null)
		{
			return !point.claimant.IsDestroyed;
		}
		return false;
	}
}
