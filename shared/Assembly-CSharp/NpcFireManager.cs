using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Spatial;
using UnityEngine;

public class NpcFireManager : SingletonComponent<NpcFireManager>, IServerComponent
{
	private const float maxFireMeleeAge = 1f;

	private const float worldSize = 8096f;

	private const int cellSize = 32;

	private Grid<BaseEntity> fireGrid = new Grid<BaseEntity>(32, 8096f);

	private Dictionary<BaseEntity, double> recentFireMeleeEvents = new Dictionary<BaseEntity, double>();

	private double nextTickTime;

	public void Add(BaseEntity entity)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		fireGrid.Add(entity, ((Component)entity).transform.position.x, ((Component)entity).transform.position.z);
	}

	public void Move(BaseEntity entity)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcFireManager.Move"))
		{
			if (IsOnFire(entity))
			{
				Vector3 position = ((Component)entity).transform.position;
				fireGrid.Move(entity, position.x, position.z);
			}
		}
	}

	public void Remove(BaseEntity entity)
	{
		fireGrid.Remove(entity);
	}

	public void GetFiresAround(Vector3 position, float range, List<BaseEntity> results)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcFireManager.GetFiresAround"))
		{
			if (fireGrid == null)
			{
				return;
			}
			fireGrid.Query(position.x, position.z, range, results);
			for (int num = results.Count - 1; num >= 0; num--)
			{
				if (!results[num].IsValid())
				{
					if (AI.logIssues)
					{
						Debug.LogWarning((object)$"Removed null fire from results list near {position}, this is unusual as fires should be removed from the grid when destroyed.");
					}
					results.RemoveAt(num);
				}
			}
		}
	}

	public void OnReceivedSignalServer(BaseEntity entity, BaseEntity.Signal signal, string arg)
	{
		if (signal != BaseEntity.Signal.Attack)
		{
			return;
		}
		if (entity is HeldEntity heldEntity && IsOnFire(heldEntity))
		{
			BasePlayer ownerPlayer = heldEntity.GetOwnerPlayer();
			if ((Object)(object)ownerPlayer != (Object)null)
			{
				recentFireMeleeEvents[ownerPlayer] = Time.timeAsDouble;
			}
		}
		if (entity is BasePlayer basePlayer && IsOnFire(basePlayer.GetHeldEntity()) && (Object)(object)entity != (Object)null)
		{
			recentFireMeleeEvents[entity] = Time.timeAsDouble;
		}
	}

	public bool DidMeleeWithFireRecently(BaseEntity querier, BaseEntity target, out double meleeTime, float maxDistance = 10f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcFireManager.DidMeleeWithFireRecently"))
		{
			if (recentFireMeleeEvents.TryGetValue(target, out var value) && Vector3.Distance(((Component)querier).transform.position, ((Component)target).transform.position) <= maxDistance)
			{
				meleeTime = value;
				return true;
			}
			meleeTime = 0.0;
			return false;
		}
	}

	public static bool IsOnFire(BaseEntity entity)
	{
		if (!entity.IsValid())
		{
			return false;
		}
		if (entity.IsOnFire())
		{
			return true;
		}
		if (entity is BaseOven baseOven && baseOven.IsOn() && baseOven.hasOpenFlame)
		{
			return true;
		}
		return false;
	}

	public void Tick()
	{
		if (Time.timeAsDouble < nextTickTime)
		{
			return;
		}
		nextTickTime = Time.timeAsDouble + (double)Random.Range(4f, 6f);
		using (TimeWarning.New("NpcFireManager.RemoveStaleEntries"))
		{
			PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
			try
			{
				foreach (var (baseEntity2, num2) in recentFireMeleeEvents)
				{
					if (!baseEntity2.IsValid() || Time.timeAsDouble - num2 > 1.0)
					{
						((List<BaseEntity>)(object)val).Add(baseEntity2);
					}
				}
				foreach (BaseEntity item in (List<BaseEntity>)(object)val)
				{
					recentFireMeleeEvents.Remove(item);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}
}
