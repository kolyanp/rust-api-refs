using System;
using System.Collections.Generic;
using Facepunch.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.UI.MainMenu;

public static class StoreOverrideLoader
{
	private static readonly Dictionary<int, ItemStoreTakeover> Overrides = new Dictionary<int, ItemStoreTakeover>();

	private static readonly Dictionary<int, ElementOverride[]> PageOverrides = new Dictionary<int, ElementOverride[]>();

	public static void Load(TextAsset json)
	{
		Load(((Object)(object)json != (Object)null) ? json.text : null);
	}

	public static void Load(string json)
	{
		Overrides.Clear();
		PageOverrides.Clear();
		if (string.IsNullOrEmpty(json))
		{
			return;
		}
		try
		{
			StoreOverrides val = JsonConvert.DeserializeObject<StoreOverrides>(json);
			if (val != null)
			{
				Load(val);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Failed to load store overrides: " + ex.Message));
		}
	}

	public static void Load(StoreOverrides content)
	{
		Overrides.Clear();
		PageOverrides.Clear();
		if (content == null)
		{
			return;
		}
		if (content.Items != null)
		{
			foreach (StoreEntryOverride item in content.Items)
			{
				if (item != null)
				{
					ItemStoreTakeover value = new ItemStoreTakeover
					{
						ItemId = item.ItemId,
						NameOverride = Phrase.op_Implicit((!string.IsNullOrEmpty(item.Name)) ? item.Name : null),
						SubtitleOverride = Phrase.op_Implicit((!string.IsNullOrEmpty(item.Subtitle)) ? item.Subtitle : null),
						HeaderPhrase = Phrase.op_Implicit((!string.IsNullOrEmpty(item.Header)) ? item.Header : null),
						ImageURL = item.ImageUrl,
						VideoURL = item.VideoUrl,
						IconUrl = item.IconUrl,
						IconPortraitUrl = item.IconPortraitUrl,
						IconSquareUrl = item.IconSquareUrl
					};
					Overrides[item.ItemId] = value;
				}
			}
		}
		if (content.Pages == null)
		{
			return;
		}
		foreach (StorePageOverride page in content.Pages)
		{
			if (page != null && page.Elements != null && page.Elements.Length != 0)
			{
				PageOverrides[page.ItemId] = page.Elements;
			}
		}
	}

	public static bool TryGetItem(int itemId, out ItemStoreTakeover takeover)
	{
		return Overrides.TryGetValue(itemId, out takeover);
	}

	public static bool TryGetPageElements(int itemId, out ElementOverride[] elements)
	{
		return PageOverrides.TryGetValue(itemId, out elements);
	}

	public static bool Validate(string json, out string error)
	{
		if (string.IsNullOrEmpty(json))
		{
			error = "JSON is null or empty";
			return false;
		}
		StoreOverrides val;
		try
		{
			val = JsonConvert.DeserializeObject<StoreOverrides>(json);
		}
		catch (Exception ex)
		{
			error = "JSON deserialization failed: " + ex.Message;
			return false;
		}
		if (val == null)
		{
			error = "JSON deserialized to null";
			return false;
		}
		if (val.Items != null)
		{
			for (int i = 0; i < val.Items.Count; i++)
			{
				StoreEntryOverride val2 = val.Items[i];
				if (val2 == null)
				{
					error = $"Items[{i}] is null";
					return false;
				}
				if (val2.ItemId == 0)
				{
					error = $"Items[{i}] has no ItemId";
					return false;
				}
			}
		}
		if (val.Pages != null)
		{
			for (int j = 0; j < val.Pages.Count; j++)
			{
				StorePageOverride val3 = val.Pages[j];
				if (val3 == null)
				{
					error = $"Pages[{j}] is null";
					return false;
				}
				if (val3.ItemId == 0)
				{
					error = $"Pages[{j}] has no ItemId";
					return false;
				}
				if (val3.Elements == null)
				{
					continue;
				}
				for (int k = 0; k < val3.Elements.Length; k++)
				{
					if (val3.Elements[k] == null)
					{
						error = $"Pages[{j}].Elements[{k}] is null";
						return false;
					}
				}
			}
		}
		error = null;
		return true;
	}
}
