using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Fishing Lookup")]
public class FishLookup : BaseScriptableObject
{
	public ItemModFishable FallbackFish;

	private static FishLookup _instance;

	private static ItemModFishable[] AvailableFish;

	private static ItemModFishable[] JunkItems;

	public static ItemDefinition[] BaitItems;

	private static TimeSince lastShuffle;

	public const int ALL_FISH_COUNT = 9;

	public const string ALL_FISH_ACHIEVEMENT_NAME = "PRO_ANGLER";

	public static FishLookup Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<FishLookup>("assets/prefabs/tools/fishing rod/fishlookup.asset", true);
			}
			return _instance;
		}
	}

	public static void LoadFish()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (AvailableFish != null)
		{
			if (TimeSince.op_Implicit(lastShuffle) > 5f)
			{
				ArrayEx.Shuffle(AvailableFish, (uint)Random.Range(0, 10000));
			}
			return;
		}
		List<ItemModFishable> list = Pool.Get<List<ItemModFishable>>();
		List<ItemDefinition> list2 = Pool.Get<List<ItemDefinition>>();
		List<ItemModFishable> list3 = Pool.Get<List<ItemModFishable>>();
		ItemModFishable itemModFishable = default(ItemModFishable);
		ItemModCompostable itemModCompostable = default(ItemModCompostable);
		foreach (ItemDefinition item in ItemManager.itemList)
		{
			if (((Component)item).TryGetComponent<ItemModFishable>(ref itemModFishable))
			{
				list.Add(itemModFishable);
				if (itemModFishable.IsJunk && itemModFishable.CanBeFished)
				{
					list3.Add(itemModFishable);
				}
			}
			if (((Component)item).TryGetComponent<ItemModCompostable>(ref itemModCompostable) && itemModCompostable.BaitValue > 0f)
			{
				list2.Add(item);
			}
		}
		AvailableFish = list.ToArray();
		JunkItems = list3.ToArray();
		BaitItems = list2.ToArray();
		Pool.FreeUnmanaged<ItemModFishable>(ref list);
		Pool.FreeUnmanaged<ItemModFishable>(ref list3);
		Pool.FreeUnmanaged<ItemDefinition>(ref list2);
	}

	public ItemDefinition GetFish(Vector3 worldPos, WaterBody bodyType, Item lure, out ItemModFishable fishable, ItemModFishable ignoreFish, out int usedLureAmount, out bool isOverfished, float overrideDepth = 0f)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		LoadFish();
		usedLureAmount = 1;
		isOverfished = false;
		if (!Fishing.disableOverfishing)
		{
			if (Fishing.debugOverfishing)
			{
				Debug.Log((object)$"FISH LOOKUP | Checking position {worldPos} for overfishing area...");
			}
			if ((Object)(object)OverfishedArea.GetOverfishedAreaAtPosition(worldPos) != (Object)null)
			{
				isOverfished = true;
				if (JunkItems.Length == 0)
				{
					fishable = FallbackFish;
					return ((Component)FallbackFish).GetComponent<ItemDefinition>();
				}
				int num = Random.Range(0, JunkItems.Length);
				fishable = JunkItems[num];
				if (Fishing.debugOverfishing)
				{
					Debug.Log((object)("FISH LOOKUP | Area is overfished, returning junk item " + ((Object)fishable).name));
				}
				return ((Component)JunkItems[num]).GetComponent<ItemDefinition>();
			}
		}
		ItemModCompostable itemModCompostable = default(ItemModCompostable);
		float num2 = (((Component)lure.info).TryGetComponent<ItemModCompostable>(ref itemModCompostable) ? itemModCompostable.BaitValue : 0f);
		if ((Object)(object)itemModCompostable != (Object)null && itemModCompostable.MaxBaitStack > 0)
		{
			usedLureAmount = Mathf.Min(lure.amount, itemModCompostable.MaxBaitStack);
			num2 *= (float)usedLureAmount;
		}
		WaterBody.FishingTag fishingTag = (((Object)(object)bodyType != (Object)null) ? bodyType.FishingType : WaterBody.FishingTag.Ocean);
		if (DeepSeaManager.IsInsideDeepSea(worldPos))
		{
			fishingTag &= ~WaterBody.FishingTag.Ocean;
			fishingTag |= WaterBody.FishingTag.DeepSea;
		}
		if (WaterResource.IsFreshWater(worldPos))
		{
			fishingTag |= WaterBody.FishingTag.River;
		}
		float num3 = WaterLevel.GetOverallWaterDepth(worldPos, waves: true, volumes: false);
		if (worldPos.y < -10f)
		{
			num3 = 10f;
		}
		if (overrideDepth != 0f)
		{
			num3 = overrideDepth;
		}
		int num4 = Random.Range(0, AvailableFish.Length);
		for (int i = 0; i < AvailableFish.Length; i++)
		{
			num4++;
			if (num4 >= AvailableFish.Length)
			{
				num4 = 0;
			}
			ItemModFishable itemModFishable = AvailableFish[num4];
			if (itemModFishable.CanBeFished && !(itemModFishable.MinimumBaitLevel > num2) && (!(itemModFishable.MaximumBaitLevel > 0f) || !(num2 > itemModFishable.MaximumBaitLevel)) && !((Object)(object)itemModFishable == (Object)(object)ignoreFish) && (itemModFishable.RequiredTag == (WaterBody.FishingTag)(-1) || (itemModFishable.RequiredTag & fishingTag) != 0) && ((fishingTag & WaterBody.FishingTag.Ocean) != WaterBody.FishingTag.Ocean || ((!(itemModFishable.MinimumWaterDepth > 0f) || !(num3 < itemModFishable.MinimumWaterDepth)) && (!(itemModFishable.MaximumWaterDepth > 0f) || !(num3 > itemModFishable.MaximumWaterDepth)))) && !(Random.Range(0f, 1f) - num2 * 3f * 0.01f > itemModFishable.Chance))
			{
				fishable = itemModFishable;
				return ((Component)itemModFishable).GetComponent<ItemDefinition>();
			}
		}
		fishable = FallbackFish;
		return ((Component)FallbackFish).GetComponent<ItemDefinition>();
	}

	public void CheckCatchAllAchievement(BasePlayer player)
	{
		LoadFish();
		int num = 0;
		ItemModFishable[] availableFish = AvailableFish;
		foreach (ItemModFishable itemModFishable in availableFish)
		{
			if (!string.IsNullOrEmpty(itemModFishable.SteamStatName) && player.stats.steam.Get(itemModFishable.SteamStatName) > 0)
			{
				num++;
			}
		}
		if (num == 9)
		{
			player.GiveAchievement("PRO_ANGLER");
		}
	}
}
