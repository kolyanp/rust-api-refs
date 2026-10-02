using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

[CreateAssetMenu(menuName = "Rust/AI/Livestock Sale Table")]
public class LivestockSaleTable : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public ItemDefinition Item;

		[Min(1f)]
		[Tooltip("How much of the item the full listing is. More than one makes it a stack the vendor can pay part of.")]
		public int Amount = 1;

		[Range(0f, 1f)]
		[Tooltip("What the full listing costs out of an offer's budget, where 1 is a perfect animal or a full lot of wool.")]
		public float Cost = 0.1f;

		[Tooltip("The least budget left for a stack to be considered. A single item needs its whole Cost instead.")]
		[Range(0f, 1f)]
		public float Threshold;

		[Range(0f, 1f)]
		[Tooltip("The largest offer this is considered for, so a big sale is not paid in cheap items. 1 never rules it out.")]
		public float MaxBudget = 1f;

		[Range(0f, 1f)]
		[Tooltip("The chance this is picked when it is affordable. Ignored when topping up a short offer.")]
		public float Chance = 1f;

		[Min(1f)]
		[Tooltip("Part stacks are rounded down to a multiple of this, so an offer reads 100 rather than 97.")]
		public int RoundTo = 1;

		public bool IsStack => Amount > 1;

		public float MinSpend
		{
			get
			{
				if (!IsStack)
				{
					return Cost;
				}
				return Threshold;
			}
		}

		public int AmountFor(float spend)
		{
			if (Cost <= 0f)
			{
				return 0;
			}
			int num = Mathf.Max(1, RoundTo);
			return Mathf.FloorToInt((float)Amount * spend / Cost / (float)num) * num;
		}

		public float CostOf(int amount)
		{
			return Cost * (float)amount / (float)Amount;
		}
	}

	[Tooltip("Every listing this vendor pays with. An item listed twice is only ever offered once per offer.")]
	public Entry[] Entries = Array.Empty<Entry>();

	[Tooltip("Whatever an offer's picks leave of its budget is paid in this. Only Item, Amount, Cost and RoundTo are read.")]
	public Entry Fallback = new Entry();

	[Tooltip("The most of an offer's budget one stack can take, so a single stack never crowds out the rest of the bundle.")]
	[Range(0f, 1f)]
	public float MaxStackShare = 0.75f;

	private readonly List<Entry> candidates = new List<Entry>();

	public void PickOffer(float budget, int picks, List<LivestockVendor.OfferItem> results)
	{
		results.Clear();
		candidates.Clear();
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			if ((Object)(object)entry.Item != (Object)null && entry.Cost > 0f && entry.Item.IsAllowed((EraRestriction)1))
			{
				candidates.Add(entry);
			}
		}
		for (int num = candidates.Count - 1; num > 0; num--)
		{
			int num2 = Random.Range(0, num + 1);
			List<Entry> list = candidates;
			int i = num;
			List<Entry> list2 = candidates;
			int index = num2;
			Entry entry2 = candidates[num2];
			Entry entry3 = candidates[num];
			Entry entry4 = (list[i] = entry2);
			entry4 = (list2[index] = entry3);
		}
		float remaining = budget;
		PickPass(budget, picks, rollChance: true, ref remaining, results);
		PickPass(budget, picks, rollChance: false, ref remaining, results);
		PayFallback(remaining, results);
	}

	private void PickPass(float budget, int picks, bool rollChance, ref float remaining, List<LivestockVendor.OfferItem> results)
	{
		foreach (Entry candidate in candidates)
		{
			if (results.Count >= picks)
			{
				break;
			}
			if (IsOffered(candidate.Item, results) || budget > candidate.MaxBudget)
			{
				continue;
			}
			float num = (candidate.IsStack ? Mathf.Min(new float[3]
			{
				candidate.Cost,
				remaining,
				budget * MaxStackShare
			}) : candidate.Cost);
			if (!(remaining < candidate.MinSpend) && !(num < candidate.MinSpend) && (!rollChance || !(Random.value > candidate.Chance)))
			{
				int num2 = (candidate.IsStack ? candidate.AmountFor(Random.Range(candidate.MinSpend, num)) : candidate.Amount);
				if (num2 > 0)
				{
					results.Add(new LivestockVendor.OfferItem
					{
						item = candidate.Item,
						amount = num2
					});
					remaining -= candidate.CostOf(num2);
				}
			}
		}
	}

	private void PayFallback(float remaining, List<LivestockVendor.OfferItem> results)
	{
		if (Fallback == null || (Object)(object)Fallback.Item == (Object)null || !Fallback.Item.IsAllowed((EraRestriction)1))
		{
			return;
		}
		int num = Fallback.AmountFor(remaining);
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if ((Object)(object)results[i].item == (Object)(object)Fallback.Item)
			{
				LivestockVendor.OfferItem value = results[i];
				value.amount += num;
				results[i] = value;
				return;
			}
		}
		results.Add(new LivestockVendor.OfferItem
		{
			item = Fallback.Item,
			amount = num
		});
	}

	private static bool IsOffered(ItemDefinition item, List<LivestockVendor.OfferItem> results)
	{
		foreach (LivestockVendor.OfferItem result in results)
		{
			if ((Object)(object)result.item == (Object)(object)item)
			{
				return true;
			}
		}
		return false;
	}

	public int ReferenceValueFor(float budget)
	{
		if (Fallback == null || (Object)(object)Fallback.Item == (Object)null || Fallback.Cost <= 0f)
		{
			return 0;
		}
		return Mathf.FloorToInt((float)Fallback.Amount * budget / Fallback.Cost);
	}
}
