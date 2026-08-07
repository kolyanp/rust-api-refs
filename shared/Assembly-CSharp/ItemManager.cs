using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class ItemManager
{
	public class ItemLookup
	{
		public ItemDefinition Scrap = FindItemDefinition("scrap");

		public ItemDefinition Hoodie = FindItemDefinition("hoodie");

		public ItemDefinition Pants = FindItemDefinition("pants");

		public ItemDefinition HazmatSuit = FindItemDefinition("hazmatsuit");

		public ItemDefinition Rock = FindItemDefinition("rock");

		public ItemDefinition MasterKey = FindItemDefinition("apartment.master_key");
	}

	private struct ItemRemove
	{
		public Item item;

		public float time;
	}

	[ServerVar(Help = "(Generated) When enabled, ItemManager uses object pooling for item instances to reduce GC allocations from frequent item creation and destruction")]
	public static bool EnablePooling = true;

	public static List<ItemDefinition> itemList;

	public static Dictionary<int, ItemDefinition> itemDictionary;

	public static Dictionary<string, ItemDefinition> itemDictionaryByName;

	public static List<ItemBlueprint> bpList;

	public static int[] defaultBlueprints;

	public static ItemDefinition blueprintBaseDef;

	public static Dictionary<ItemDefinition, ItemBlueprint> itemToBlueprint;

	public static Dictionary<ItemDefinition, List<ItemDefinition>> redirectPerItem;

	public static Dictionary<ItemDefinition, List<ItemBlueprint>> ingredientToBlueprints;

	private static List<ItemRemove> ItemRemoves = new List<ItemRemove>();

	public static ItemLookup Items { get; private set; }

	public static void InvalidateWorkshopSkinCache()
	{
		if (itemList == null)
		{
			return;
		}
		foreach (ItemDefinition item in itemList)
		{
			item.InvalidateWorkshopSkinCache();
		}
	}

	public static void Initialize()
	{
		if (itemList != null)
		{
			return;
		}
		itemToBlueprint = new Dictionary<ItemDefinition, ItemBlueprint>();
		redirectPerItem = new Dictionary<ItemDefinition, List<ItemDefinition>>();
		ingredientToBlueprints = new Dictionary<ItemDefinition, List<ItemBlueprint>>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		GameObject[] array = FileSystem.LoadAllFromBundle<GameObject>("items.preload.bundle", "l:ItemDefinition");
		if (array.Length == 0)
		{
			throw new Exception("items.preload.bundle has no items!");
		}
		if (stopwatch.Elapsed.TotalSeconds > 1.0)
		{
			Debug.Log((object)("Loading Items Took: " + stopwatch.Elapsed.TotalMilliseconds / 1000.0 + " seconds"));
		}
		List<ItemDefinition> list = (from x in array
			select x.GetComponent<ItemDefinition>() into x
			where (Object)(object)x != (Object)null
			select x).ToList();
		List<ItemBlueprint> list2 = (from x in array
			select x.GetComponent<ItemBlueprint>() into x
			where (Object)(object)x != (Object)null && x.userCraftable
			select x).ToList();
		Dictionary<int, ItemDefinition> dictionary = new Dictionary<int, ItemDefinition>();
		Dictionary<string, ItemDefinition> dictionary2 = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
		foreach (ItemDefinition item in list)
		{
			item.Initialize(list);
			if (dictionary.ContainsKey(item.itemid))
			{
				ItemDefinition itemDefinition = dictionary[item.itemid];
				Debug.LogWarning((object)("Item ID duplicate " + item.itemid + " (" + ((Object)item).name + ") - have you given your items unique shortnames?"), (Object)(object)((Component)item).gameObject);
				Debug.LogWarning((object)("Other item is " + ((Object)itemDefinition).name), (Object)(object)itemDefinition);
			}
			else if (string.IsNullOrEmpty(item.shortname))
			{
				Debug.LogWarning((object)$"{item} has a null short name! id: {item.itemid} {item.displayName.english}");
			}
			else
			{
				dictionary.Add(item.itemid, item);
				dictionary2.Add(item.shortname, item);
				ItemBlueprint component = ((Component)item).GetComponent<ItemBlueprint>();
				if ((Object)(object)component != (Object)null)
				{
					itemToBlueprint.Add(item, component);
				}
			}
		}
		stopwatch.Stop();
		if (stopwatch.Elapsed.TotalSeconds > 1.0)
		{
			Debug.Log((object)("Building Items Took: " + stopwatch.Elapsed.TotalMilliseconds / 1000.0 + " seconds / Items: " + list.Count + " / Blueprints: " + list2.Count));
		}
		defaultBlueprints = (from x in list2
			where !x.NeedsSteamItem && !x.NeedsSteamDLC && x.defaultBlueprint
			select x.targetItem.itemid).ToArray();
		itemList = list;
		bpList = list2;
		itemDictionary = dictionary;
		itemDictionaryByName = dictionary2;
		blueprintBaseDef = FindItemDefinition("blueprintbase");
		foreach (ItemDefinition item2 in itemList)
		{
			if ((Object)(object)item2 != (Object)null && (Object)(object)item2.isRedirectOf != (Object)null)
			{
				if (!redirectPerItem.TryGetValue(item2.isRedirectOf, out var value))
				{
					value = new List<ItemDefinition>();
					redirectPerItem[item2.isRedirectOf] = value;
				}
				value.Add(item2);
			}
		}
		foreach (ItemBlueprint bp in bpList)
		{
			foreach (ItemAmount ingredient in bp.ingredients)
			{
				if (!ingredientToBlueprints.TryGetValue(ingredient.itemDef, out var value2))
				{
					value2 = new List<ItemBlueprint>();
					ingredientToBlueprints[ingredient.itemDef] = value2;
				}
				value2.Add(bp);
			}
		}
		CalculateApartmentItemTaxes();
		Items = new ItemLookup();
	}

	private static void CalculateApartmentItemTaxes()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<ItemDefinition, int> dictionary = new Dictionary<ItemDefinition, int>();
		ListHashSet<ItemDefinition> val = new ListHashSet<ItemDefinition>();
		ListHashSet<ItemDefinition> val2 = new ListHashSet<ItemDefinition>();
		foreach (ItemDefinition item in itemList)
		{
			dictionary[item] = 0;
			val.Add(item);
		}
		for (int i = 0; i < 100; i++)
		{
			if (val.Count == 0)
			{
				break;
			}
			Enumerator<ItemDefinition> enumerator2 = val.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					ItemDefinition current2 = enumerator2.Current;
					if (!ingredientToBlueprints.TryGetValue(current2, out var value))
					{
						continue;
					}
					foreach (ItemBlueprint item2 in value)
					{
						dictionary[item2.targetItem] = i + 1;
						val2.TryAdd(item2.targetItem);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			ListHashSet<ItemDefinition> obj = val;
			val = val2;
			val2 = obj;
			val2.Clear();
		}
		IGrouping<int, KeyValuePair<ItemDefinition, int>>[] array = (from x in dictionary
			group x by x.Value into x
			orderby x.Key
			select x).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			foreach (KeyValuePair<ItemDefinition, int> item3 in array[num])
			{
				ItemDefinition key = item3.Key;
				ItemBlueprint blueprint = key.Blueprint;
				ItemModApartmentTax component = ((Component)key).GetComponent<ItemModApartmentTax>();
				if ((Object)(object)component != (Object)null && component.ScrapPerStack != 0f)
				{
					key.ApartmentTaxPerStack = component.ScrapPerStack;
				}
				else
				{
					if ((Object)(object)blueprint == (Object)null)
					{
						continue;
					}
					float num2 = 0f;
					foreach (ItemAmount ingredient in blueprint.ingredients)
					{
						float num3 = (float)(blueprint.targetItem.stackable / blueprint.amountToCreate) * ingredient.amount / (float)ingredient.itemDef.stackable;
						num2 += ingredient.itemDef.ApartmentTaxPerStack * num3;
					}
					blueprint.targetItem.ApartmentTaxPerStack = num2;
				}
			}
		}
		foreach (ItemDefinition item4 in itemList)
		{
			if (!(item4.ApartmentTaxPerStack > 0f))
			{
				Rarity rarity = ((item4.despawnRarity > item4.rarity) ? item4.despawnRarity : item4.rarity);
				item4.ApartmentTaxPerStack = GetRarityTaxPerStack(rarity);
			}
		}
		static float GetRarityTaxPerStack(Rarity val3)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected I4, but got Unknown
			return (val3 - 2) switch
			{
				0 => 3f, 
				1 => 8f, 
				2 => 15f, 
				_ => 0f, 
			};
		}
	}

	public static Item CreateByName(string strName, int iAmount = 1, ulong skin = 0uL)
	{
		ItemDefinition itemDefinition = itemList.Find((ItemDefinition x) => x.shortname == strName);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return null;
		}
		return CreateByItemID(itemDefinition.itemid, iAmount, skin, 0uL);
	}

	public static Item CreateByPartialName(string strName, int iAmount = 1, ulong skin = 0uL)
	{
		ItemDefinition itemDefinition = FindDefinitionByPartialName(strName);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return null;
		}
		return CreateByItemID(itemDefinition.itemid, iAmount, skin, 0uL);
	}

	public static ItemDefinition FindDefinitionByPartialName(string strName)
	{
		ItemDefinition itemDefinition = itemList.Find((ItemDefinition x) => x.shortname == strName);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			itemDefinition = itemList.Find((ItemDefinition x) => StringEx.Contains(x.shortname, strName, CompareOptions.IgnoreCase));
		}
		return itemDefinition;
	}

	public static Item CreateByItemID(int itemID, int iAmount = 1, ulong skin = 0uL, ulong attachment = 0uL)
	{
		ItemDefinition itemDefinition = FindItemDefinition(itemID);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return null;
		}
		return Create(itemDefinition, iAmount, skin, isServerSide: true, attachment);
	}

	public static Item Create(ItemDefinition template, int iAmount = 1, ulong skin = 0uL, bool isServerSide = true, ulong attachment = 0uL)
	{
		Debug.Assert(isServerSide, "Tried to create client item on server!");
		TrySkinChangeItem(ref template, ref skin);
		if ((Object)(object)template == (Object)null)
		{
			Debug.LogWarning((object)"Creating invalid/missing item!");
			return null;
		}
		if (iAmount <= 0)
		{
			Debug.LogError((object)("Creating item with less than 1 amount! (" + template.displayName.english + ")"));
			return null;
		}
		Item item = ((EnablePooling && isServerSide) ? Pool.Get<Item>() : new Item());
		item.isServer = isServerSide;
		item.info = template;
		item.amount = iAmount;
		item.skin = skin;
		item.attachment = attachment;
		item.Initialize(template);
		RustLog.Log(RustLog.EntryType.Item, 1, null, "Created <color={0}>{1}</color>", item.isServer ? "yellow" : "cyan", item);
		return item;
	}

	private static void TrySkinChangeItem(ref ItemDefinition template, ref ulong skinId)
	{
		if (skinId == 0L)
		{
			return;
		}
		ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId((int)skinId);
		if (skin.id != 0)
		{
			ItemSkin itemSkin = skin.invItem as ItemSkin;
			if (!((Object)(object)itemSkin == (Object)null) && !((Object)(object)itemSkin.Redirect == (Object)null))
			{
				template = itemSkin.Redirect;
				skinId = 0uL;
			}
		}
	}

	public static Item Load(Item load, Item created, bool isServer)
	{
		if (created == null)
		{
			created = ((EnablePooling && isServer) ? Pool.Get<Item>() : new Item());
		}
		created.isServer = isServer;
		created.Load(load);
		if ((Object)(object)created.info == (Object)null)
		{
			Debug.LogWarning((object)"Item loading failed - item is invalid");
			return null;
		}
		if ((Object)(object)created.info == (Object)(object)blueprintBaseDef && (Object)(object)created.blueprintTargetDef == (Object)null)
		{
			Debug.LogWarning((object)"Blueprint item loading failed - invalid item target");
			return null;
		}
		RustLog.Log(RustLog.EntryType.Item, 1, null, "Loaded <color={0}>{1}</color>", created.isServer ? "yellow" : "cyan", created);
		return created;
	}

	public static ItemDefinition FindItemDefinition(int itemID)
	{
		Initialize();
		return itemDictionary.GetValueOrDefault(itemID, null);
	}

	public static ItemDefinition FindItemDefinition(string shortName)
	{
		Initialize();
		return itemDictionaryByName.GetValueOrDefault(shortName, null);
	}

	public static ItemBlueprint FindBlueprint(ItemDefinition item)
	{
		Initialize();
		return itemToBlueprint.GetValueOrDefault(item, null);
	}

	public static List<ItemDefinition> GetItemDefinitions()
	{
		Initialize();
		return itemList;
	}

	public static List<ItemBlueprint> GetBlueprints()
	{
		Initialize();
		return bpList;
	}

	public static void DoRemoves(bool force = false)
	{
		using (TimeWarning.New("DoRemoves"))
		{
			float time = Time.time;
			for (int i = 0; i < ItemRemoves.Count; i++)
			{
				if (force || !(ItemRemoves[i].time > time))
				{
					Item item = ItemRemoves[i].item;
					ItemRemoves.RemoveAt(i--);
					RustLog.Log(RustLog.EntryType.Item, 1, null, "Removing <color={0}>{1}</color>", item.isServer ? "yellow" : "cyan", item);
					item.DoRemove();
					if (EnablePooling)
					{
						Pool.Free<Item>(ref item);
					}
				}
			}
		}
	}

	public static void Heartbeat()
	{
		DoRemoves();
	}

	public static void RemoveItem(Item item, float fTime = 0f)
	{
		RustLog.Log(RustLog.EntryType.Item, 2, null, "Scheduled removal of <color={0}>{1}</color>", item.isServer ? "yellow" : "cyan", item);
		ItemRemove item2 = new ItemRemove
		{
			item = item,
			time = Time.time + fTime
		};
		ItemRemoves.Add(item2);
	}

	public static IEnumerable<Item> GetAllItems()
	{
		Queue<Item> buffer = new Queue<Item>();
		HashSet<Item> bufferHash = new HashSet<Item>();
		foreach (Item item in GetAllItemsInternal())
		{
			if (item == null)
			{
				continue;
			}
			yield return item;
			if (item.contents == null)
			{
				continue;
			}
			bufferHash.Clear();
			buffer.Enqueue(item);
			Item result;
			while (buffer.TryDequeue(out result))
			{
				if (result.contents?.itemList == null)
				{
					continue;
				}
				foreach (Item child in result.contents.itemList)
				{
					yield return child;
					if (bufferHash.Add(child))
					{
						buffer.Enqueue(child);
					}
				}
			}
		}
	}

	private static IEnumerable<Item> GetAllItemsInternal()
	{
		List<ItemContainer> buffer = new List<ItemContainer>();
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseNetworkable current = enumerator.Current;
				if (current is IInventoryProvider inventoryProvider)
				{
					buffer.Clear();
					inventoryProvider.GetAllInventories(buffer);
					foreach (ItemContainer item in buffer)
					{
						foreach (Item item2 in item.itemList)
						{
							yield return item2;
						}
					}
				}
				else if (current is DroppedItem droppedItem)
				{
					yield return droppedItem.item;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
