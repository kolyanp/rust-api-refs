using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using Spatial;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcCoverManager : SingletonComponent<NpcCoverManager>, IServerComponent
{
	private const float worldSize = 8096f;

	private const int cellSize = 32;

	private Grid<CoverComponent> coverGrid = new Grid<CoverComponent>(32, 8096f);

	private Dictionary<Cover, BaseEntity> coverToEntity = new Dictionary<Cover, BaseEntity>();

	private Dictionary<BaseEntity, Cover> entityToCover = new Dictionary<BaseEntity, Cover>();

	public void Add(CoverComponent cover)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		coverGrid.Add(cover, ((Component)cover).transform.position.x, ((Component)cover).transform.position.z);
	}

	public void Remove(CoverComponent cover)
	{
		coverGrid.Remove(cover);
	}

	public void GetCoversAround(BaseEntity entity, Vector3 origin, Vector3 threatPosition, float range, List<Cover> covers)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcCoverManager.GetCoverAround"))
		{
			if (coverGrid == null)
			{
				return;
			}
			PooledList<CoverComponent> val = Pool.Get<PooledList<CoverComponent>>();
			try
			{
				coverGrid.Query(origin.x, origin.z, range, (List<CoverComponent>)(object)val);
				foreach (CoverComponent item in (List<CoverComponent>)(object)val)
				{
					item.GetCovers(covers, threatPosition);
				}
				PooledList<BaseEntity> val2 = Pool.Get<PooledList<BaseEntity>>();
				try
				{
					BaseEntity.Query.Server.GetInSphere(origin, range, (List<BaseEntity>)(object)val2);
					foreach (BaseEntity item2 in (List<BaseEntity>)(object)val2)
					{
						if (!(item2 is TreeEntity) && !(item2 is OreResourceEntity) && !(item2 is LootContainer))
						{
							continue;
						}
						Bounds bounds = ((Component)item2).GetComponentInChildren<Collider>().bounds;
						float num = Mathf.Max(((Bounds)(ref bounds)).extents.x, ((Bounds)(ref bounds)).extents.z);
						if (!(num > 1.5f) && !(num < 0.5f))
						{
							using PillarCoverGroup pillarCoverGroup = Pool.Get<PillarCoverGroup>();
							pillarCoverGroup.GenerateCovers(((Component)item2).gameObject);
							pillarCoverGroup.GetCovers(((Component)item2).transform, covers, threatPosition);
						}
					}
					for (int num2 = covers.Count - 1; num2 >= 0; num2--)
					{
						if (coverToEntity.TryGetValue(covers[num2], out var value) && (Object)(object)value != (Object)(object)entity)
						{
							covers.RemoveAt(num2);
						}
						else if (Vector3.Distance(covers[num2].position, origin) > range)
						{
							covers.RemoveAt(num2);
						}
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public Cover? FindBestCover(RustNavMeshAgent agent, Vector3 threatPosition, float radius, float preferedEngagementDistance, ref RustNavMeshPath path, bool requireLoS, float? targetRadius = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("FindBestCover"))
		{
			PooledList<Cover> val = Pool.Get<PooledList<Cover>>();
			try
			{
				BaseEntity baseEntity = agent.GetBaseEntity();
				GetCoversAround(baseEntity, ((Component)agent).transform.position, threatPosition, radius, (List<Cover>)(object)val);
				PooledList<(Cover, float)> val2 = Pool.Get<PooledList<(Cover, float)>>();
				try
				{
					foreach (Cover item2 in (List<Cover>)(object)val)
					{
						if (item2.ProtectsFrom(threatPosition))
						{
							float num = 0f - Mathf.Abs(Vector3.Distance(item2.position, threatPosition) - preferedEngagementDistance);
							float item = (0f - Vector3.Distance(((Component)agent).transform.position, item2.position)) * 4f + num;
							((List<(Cover, float)>)(object)val2).Add((item2, item));
						}
					}
					((List<(Cover, float)>)(object)val2).Sort((Comparison<(Cover, float)>)(((Cover cover, float score) a, (Cover cover, float score) b) => b.score.CompareTo(a.score)));
					((List<Cover>)(object)val).Clear();
					foreach (var item3 in (List<(Cover, float)>)(object)val2)
					{
						((List<Cover>)(object)val).Add(item3.Item1);
					}
					return GetFirstUsableCover(agent, threatPosition, (List<Cover>)(object)val, radius * 4f, ref path, requireLoS, targetRadius);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private static Cover? GetFirstUsableCover(RustNavMeshAgent agent, Vector3 threatPosition, List<Cover> covers, float maxPathLength, ref RustNavMeshPath navPath, bool requireLoS, float? targetRadius)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("GetFirstUsableCover"))
		{
			BaseEntity baseEntity = agent.GetBaseEntity();
			foreach (Cover cover in covers)
			{
				if ((!requireLoS || cover.GetFirstUnoccludedPeek(threatPosition, baseEntity) != Cover.Peeks.None) && agent.CalculatePath(cover.position, navPath) && (int)navPath.status == 0 && !(navPath.GetPathLength() > maxPathLength) && (!targetRadius.HasValue || !DoesPathIntersectTarget(navPath, agent.WorldToNavSpace(threatPosition), targetRadius.Value)))
				{
					return cover;
				}
			}
			return null;
		}
	}

	private static bool DoesPathIntersectTarget(RustNavMeshPath path, NavVector3 targetPosition, float targetRadius)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < path.corners.Count - 1; i++)
		{
			NavVector3 navVector = path.corners[i];
			NavVector3 navVector2 = path.corners[i + 1];
			if (SegmentSphereIntersection(navVector.Value, navVector2.Value, targetPosition.Value, targetRadius))
			{
				return true;
			}
		}
		return false;
	}

	public static bool SegmentSphereIntersection(Vector3 start, Vector3 end, Vector3 center, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		if (((Vector3)(ref val)).sqrMagnitude == 0f)
		{
			return false;
		}
		float num = radius * radius;
		Vector3 val2 = start - center;
		bool flag = ((Vector3)(ref val2)).sqrMagnitude <= num;
		val2 = end - center;
		bool flag2 = ((Vector3)(ref val2)).sqrMagnitude <= num;
		if (flag & flag2)
		{
			return true;
		}
		if (flag ^ flag2)
		{
			return true;
		}
		Vector3 val3 = start - center;
		float num2 = Vector3.Dot(val, val);
		float num3 = 2f * Vector3.Dot(val3, val);
		float num4 = Vector3.Dot(val3, val3) - num;
		float num5 = num3 * num3 - 4f * num2 * num4;
		if (Mathf.Abs(num5) < Mathf.Epsilon)
		{
			num5 = 0f;
		}
		if (num5 < 0f)
		{
			return false;
		}
		float num6 = Mathf.Sqrt(num5);
		float num7 = (0f - num3 - num6) / (2f * num2);
		float num8 = (0f - num3 + num6) / (2f * num2);
		if (!(num7 >= 0f) || !(num7 <= 1f))
		{
			if (num8 >= 0f)
			{
				return num8 <= 1f;
			}
			return false;
		}
		return true;
	}

	public void Reserve(Cover cover, BaseEntity entity)
	{
		if (TryGetCover(entity, out var cover2))
		{
			Release(cover2);
		}
		coverToEntity[cover] = entity;
		entityToCover[entity] = cover;
	}

	public void Release(Cover cover)
	{
		entityToCover.Remove(coverToEntity[cover]);
		coverToEntity.Remove(cover);
	}

	public bool TryGetCover(BaseEntity entity, out Cover cover)
	{
		if (entityToCover.TryGetValue(entity, out cover))
		{
			return true;
		}
		return false;
	}

	public void Tick()
	{
		using (TimeWarning.New("NpcCoverManager.Tick"))
		{
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				foreach (KeyValuePair<BaseEntity, Cover> item in entityToCover)
				{
					BaseEntity key = item.Key;
					if (!key.IsValid() || (key is BaseCombatEntity baseCombatEntity && baseCombatEntity.IsDead()))
					{
						((List<BaseEntity>)(object)val).Add(item.Key);
					}
				}
				foreach (BaseEntity item2 in (List<BaseEntity>)(object)val)
				{
					Release(entityToCover[item2]);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}
