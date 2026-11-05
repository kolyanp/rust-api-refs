using System.Collections.Generic;
using ConVar;
using UnityEngine;

public class DecayTickList
{
	public readonly float TickOverride;

	private readonly List<DecayEntity> entities = new List<DecayEntity>();

	private readonly Dictionary<DecayEntity, int> entityToIndex = new Dictionary<DecayEntity, int>();

	private readonly Stack<int> gaps = new Stack<int>();

	private RoundRobinPacer pacer;

	public float TickOrder
	{
		get
		{
			if (!(TickOverride > 0f))
			{
				return float.MaxValue;
			}
			return TickOverride;
		}
	}

	public DecayTickList(float tickOverride)
	{
		TickOverride = tickOverride;
	}

	public void Add(DecayEntity ent)
	{
		if (!entityToIndex.ContainsKey(ent))
		{
			if (gaps.Count > 0)
			{
				int num = gaps.Pop();
				entities[num] = ent;
				entityToIndex.Add(ent, num);
			}
			else
			{
				entityToIndex.Add(ent, entities.Count);
				entities.Add(ent);
			}
		}
	}

	public void Remove(DecayEntity ent)
	{
		if (entityToIndex.Remove(ent, out var value))
		{
			entities[value] = null;
			gaps.Push(value);
		}
	}

	public void Tick(ref TimeBudget budget)
	{
		float num = ((TickOverride > 0f) ? TickOverride : ConVar.Decay.tick);
		pacer.Accrue(entities.Count, num, Time.deltaTime);
		int index = default;
		while (!budget.Exceeded && pacer.TryNext(entities.Count, ref index))
		{
			DecayEntity decayEntity = entities[index];
			if ((Object)(object)decayEntity != (Object)null)
			{
				decayEntity.DecayTick();
			}
		}
	}
}
