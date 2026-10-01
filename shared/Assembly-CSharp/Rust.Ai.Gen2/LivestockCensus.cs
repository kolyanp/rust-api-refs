using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class LivestockCensus
{
	public enum DeathCause
	{
		OldAge,
		Neglect,
		Killed,
		Other,
		Count
	}

	private class AreaTally
	{
		public Vector3 Centre;

		public float Radius;

		public int Births;

		public int DungDropped;

		public int TroughItemsEaten;

		public readonly int[] Deaths = new int[4];

		public void Clear()
		{
			Births = 0;
			DungDropped = 0;
			TroughItemsEaten = 0;
			for (int i = 0; i < Deaths.Length; i++)
			{
				Deaths[i] = 0;
			}
		}
	}

	public struct Snapshot
	{
		public int Count;

		public int Cattle;

		public int Sheep;

		public int OtherSpecies;

		public int Adults;

		public int Infants;

		public int Males;

		public int Females;

		public int Pregnant;

		public int Leading;

		public float MeanFullness;

		public float MinFullness;

		public float MeanHydration;

		public float MinHydration;

		public float MeanPersonalSpace;

		public float MinPersonalSpace;

		public float MeanHerdLoadTarget;

		public float MeanPackingTarget;

		public float MeanContentment;

		public float MinContentment;

		public float MeanCondition;

		public float MinCondition;

		public int BreedingPaused;

		public int Sleeping;

		public float MeanGrazeShare;

		public float MeanRestShare;

		public float HerdRadius;

		public Vector3 Centroid;
	}

	private static readonly int[] deaths = new int[4];

	private static readonly List<AreaTally> areas = new List<AreaTally>();

	public static int Births { get; private set; }

	public static int DungDropped { get; private set; }

	public static int TroughItemsEaten { get; private set; }

	public static int AreaCount => areas.Count;

	public static int Deaths
	{
		get
		{
			int num = 0;
			for (int i = 0; i < deaths.Length; i++)
			{
				num += deaths[i];
			}
			return num;
		}
	}

	public static int AddArea(Vector3 centre, float radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		areas.Add(new AreaTally
		{
			Centre = centre,
			Radius = radius
		});
		return areas.Count - 1;
	}

	public static void ClearAreas()
	{
		areas.Clear();
	}

	public static float ClosestAreaGap()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		for (int i = 0; i < areas.Count; i++)
		{
			for (int j = i + 1; j < areas.Count; j++)
			{
				float num2 = Vector3.Distance(areas[i].Centre, areas[j].Centre) - areas[i].Radius - areas[j].Radius;
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (areas.Count >= 2)
		{
			return num;
		}
		return float.MaxValue;
	}

	public static int BirthsIn(int area)
	{
		return areas[area].Births;
	}

	public static int DungDroppedIn(int area)
	{
		return areas[area].DungDropped;
	}

	public static int TroughItemsEatenIn(int area)
	{
		return areas[area].TroughItemsEaten;
	}

	public static int DeathsIn(int area, DeathCause cause)
	{
		return areas[area].Deaths[(int)cause];
	}

	private static AreaTally TallyAt(Vector3 position)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < areas.Count; i++)
		{
			AreaTally areaTally = areas[i];
			if (Vector3.Distance(position, areaTally.Centre) <= areaTally.Radius)
			{
				return areaTally;
			}
		}
		return null;
	}

	public static int DeathsBy(DeathCause cause)
	{
		return deaths[(int)cause];
	}

	public static void Reset()
	{
		Births = 0;
		DungDropped = 0;
		TroughItemsEaten = 0;
		for (int i = 0; i < deaths.Length; i++)
		{
			deaths[i] = 0;
		}
		for (int j = 0; j < areas.Count; j++)
		{
			areas[j].Clear();
		}
	}

	public static void RecordBirth(Vector3 at)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Births++;
		AreaTally areaTally = TallyAt(at);
		if (areaTally != null)
		{
			areaTally.Births++;
		}
	}

	public static void RecordDung(Vector3 at)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		DungDropped++;
		AreaTally areaTally = TallyAt(at);
		if (areaTally != null)
		{
			areaTally.DungDropped++;
		}
	}

	public static void RecordTroughItemEaten(Vector3 at)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TroughItemsEaten++;
		AreaTally areaTally = TallyAt(at);
		if (areaTally != null)
		{
			areaTally.TroughItemsEaten++;
		}
	}

	public static void RecordDeath(HitInfo info, Vector3 at)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		DeathCause deathCause = CauseOf(info);
		deaths[(int)deathCause]++;
		AreaTally areaTally = TallyAt(at);
		if (areaTally != null)
		{
			areaTally.Deaths[(int)deathCause]++;
		}
	}

	public static DeathCause CauseOf(HitInfo info)
	{
		if (info == null)
		{
			return DeathCause.OldAge;
		}
		if (info.damageTypes != null && info.damageTypes.Has(DamageType.Hunger) && (Object)(object)info.Initiator == (Object)null)
		{
			return DeathCause.Neglect;
		}
		if (!((Object)(object)info.InitiatorPlayer != (Object)null))
		{
			return DeathCause.Other;
		}
		return DeathCause.Killed;
	}

	public static Snapshot Sample(Vector3 origin, float radius)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			LivestockAnimal[] array = BaseEntity.Util.FindAll<LivestockAnimal>();
			foreach (LivestockAnimal livestockAnimal in array)
			{
				if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && livestockAnimal.isServer && (!(radius > 0f) || !(Vector3.Distance(((Component)livestockAnimal).transform.position, origin) > radius)))
				{
					((List<LivestockAnimal>)(object)val).Add(livestockAnimal);
				}
			}
			return SampleLiving((List<LivestockAnimal>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static Snapshot Sample(List<LivestockAnimal> herd)
	{
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			for (int i = 0; i < herd.Count; i++)
			{
				LivestockAnimal livestockAnimal = herd[i];
				if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && livestockAnimal.isServer)
				{
					((List<LivestockAnimal>)(object)val).Add(livestockAnimal);
				}
			}
			return SampleLiving((List<LivestockAnimal>)(object)val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static Snapshot SampleLiving(List<LivestockAnimal> sampled)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		Snapshot result = new Snapshot
		{
			MinFullness = 1f,
			MinHydration = 1f,
			MinPersonalSpace = 1f,
			MinContentment = 1f,
			MinCondition = 1f
		};
		if (sampled.Count == 0)
		{
			result.MinFullness = 0f;
			result.MinHydration = 0f;
			result.MinPersonalSpace = 0f;
			result.MinContentment = 0f;
			result.MinCondition = 0f;
			return result;
		}
		Vector3 val = Vector3.zero;
		foreach (LivestockAnimal item in sampled)
		{
			result.Count++;
			val += ((Component)item).transform.position;
			if (item is Cow)
			{
				result.Cattle++;
			}
			else if (item is Sheep)
			{
				result.Sheep++;
			}
			else
			{
				result.OtherSpecies++;
			}
			if (item.IsAdult())
			{
				result.Adults++;
			}
			else
			{
				result.Infants++;
			}
			if (item.IsMale)
			{
				result.Males++;
			}
			else
			{
				result.Females++;
			}
			if (item.IsPregnant())
			{
				result.Pregnant++;
			}
			if (item.IsLeading())
			{
				result.Leading++;
			}
			float needValue = item.Fullness.NeedValue;
			float needValue2 = item.Hydration.NeedValue;
			float needValue3 = item.PersonalSpace.NeedValue;
			float contentment = item.Contentment;
			result.MeanFullness += needValue;
			result.MeanHydration += needValue2;
			result.MeanPersonalSpace += needValue3;
			result.MeanContentment += contentment;
			result.MeanHerdLoadTarget += item.LastHerdLoadTarget;
			result.MeanPackingTarget += item.LastPackingTarget;
			result.MinFullness = Mathf.Min(result.MinFullness, needValue);
			result.MinHydration = Mathf.Min(result.MinHydration, needValue2);
			result.MinPersonalSpace = Mathf.Min(result.MinPersonalSpace, needValue3);
			result.MinContentment = Mathf.Min(result.MinContentment, contentment);
			result.MeanCondition += item.Condition;
			result.MinCondition = Mathf.Min(result.MinCondition, item.Condition);
			if (item.BreedingPaused)
			{
				result.BreedingPaused++;
			}
			if (item.IsSleeping())
			{
				result.Sleeping++;
			}
			result.MeanGrazeShare += item.ShareOfDay(LivestockAnimal.DayActivity.Grazing);
			result.MeanRestShare += item.ShareOfDay(LivestockAnimal.DayActivity.Resting);
		}
		int count = result.Count;
		result.MeanFullness /= count;
		result.MeanHydration /= count;
		result.MeanPersonalSpace /= count;
		result.MeanContentment /= count;
		result.MeanCondition /= count;
		result.MeanHerdLoadTarget /= count;
		result.MeanPackingTarget /= count;
		result.MeanGrazeShare /= count;
		result.MeanRestShare /= count;
		result.Centroid = val / (float)count;
		foreach (LivestockAnimal item2 in sampled)
		{
			float num = Vector3.Distance(((Component)item2).transform.position, result.Centroid);
			if (num > result.HerdRadius)
			{
				result.HerdRadius = num;
			}
		}
		return result;
	}

	public static int CountDroppedItems(ItemDefinition definition, Vector3 origin, float radius)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)definition == (Object)null)
		{
			return 0;
		}
		int num = 0;
		DroppedItem[] array = BaseEntity.Util.FindAll<DroppedItem>();
		foreach (DroppedItem droppedItem in array)
		{
			if (!((Object)(object)droppedItem == (Object)null) && !droppedItem.IsDestroyed && droppedItem.item != null && droppedItem.isServer && !((Object)(object)droppedItem.item.info != (Object)(object)definition) && (!(radius > 0f) || !(Vector3.Distance(((Component)droppedItem).transform.position, origin) > radius)))
			{
				num++;
			}
		}
		return num;
	}
}
