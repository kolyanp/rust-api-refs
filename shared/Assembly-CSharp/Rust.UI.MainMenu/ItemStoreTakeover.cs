using System;
using Facepunch.Models;
using UnityEngine;

namespace Rust.UI.MainMenu;

[Serializable]
public struct ItemStoreTakeover
{
	public Phrase NameOverride;

	public Phrase SubtitleOverride;

	public Phrase HeaderPhrase;

	public Sprite IconOverride;

	public Sprite IconPortraitOverride;

	public Sprite IconSquareOverride;

	public string ImageURL;

	public string VideoURL;

	public string IconUrl;

	public string IconPortraitUrl;

	public string IconSquareUrl;

	public UI_StoreItemOverlayPage PagePrefab;

	public UI_StoreItemTile TilePrefabOverride;

	public SteamInventoryItem Item;

	[Tooltip("Will be used if you don't have an Item definition (DLCs)")]
	public int ItemId;

	public readonly int GetItemID()
	{
		if (!((Object)(object)Item != (Object)null))
		{
			return ItemId;
		}
		return Item.id;
	}

	public ItemStoreTakeover(StoreFeaturing storeFeaturing)
	{
		NameOverride = Phrase.op_Implicit(storeFeaturing.TitleText);
		SubtitleOverride = Phrase.op_Implicit(storeFeaturing.SubtitleText);
		HeaderPhrase = Phrase.op_Implicit(storeFeaturing.HeaderText);
		ImageURL = storeFeaturing.ImageUrl;
		VideoURL = storeFeaturing.VideoUrl;
		ItemId = storeFeaturing.ItemID;
		IconOverride = null;
		IconPortraitOverride = null;
		IconSquareOverride = null;
		IconUrl = null;
		IconPortraitUrl = null;
		IconSquareUrl = null;
		PagePrefab = null;
		TilePrefabOverride = null;
		Item = null;
	}

	public readonly bool IsValid()
	{
		return GetItemID() != 0;
	}

	public void OverridesWith(ItemStoreTakeover other)
	{
		if (other.NameOverride != null && !string.IsNullOrEmpty(other.NameOverride.translated))
		{
			NameOverride = other.NameOverride;
		}
		if (other.SubtitleOverride != null && !string.IsNullOrEmpty(other.SubtitleOverride.translated))
		{
			SubtitleOverride = other.SubtitleOverride;
		}
		if (other.HeaderPhrase != null && !string.IsNullOrEmpty(other.HeaderPhrase.translated))
		{
			HeaderPhrase = other.HeaderPhrase;
		}
		if ((Object)(object)other.IconOverride != (Object)null)
		{
			IconOverride = other.IconOverride;
		}
		if ((Object)(object)other.IconPortraitOverride != (Object)null)
		{
			IconPortraitOverride = other.IconPortraitOverride;
		}
		if ((Object)(object)other.IconSquareOverride != (Object)null)
		{
			IconSquareOverride = other.IconSquareOverride;
		}
		if (!string.IsNullOrEmpty(other.ImageURL))
		{
			ImageURL = other.ImageURL;
		}
		if (!string.IsNullOrEmpty(other.VideoURL))
		{
			VideoURL = other.VideoURL;
		}
		if (!string.IsNullOrEmpty(other.IconUrl))
		{
			IconUrl = other.IconUrl;
		}
		if (!string.IsNullOrEmpty(other.IconPortraitUrl))
		{
			IconPortraitUrl = other.IconPortraitUrl;
		}
		if (!string.IsNullOrEmpty(other.IconSquareUrl))
		{
			IconSquareUrl = other.IconSquareUrl;
		}
		if ((Object)(object)other.PagePrefab != (Object)null)
		{
			PagePrefab = other.PagePrefab;
		}
		if ((Object)(object)other.TilePrefabOverride != (Object)null)
		{
			TilePrefabOverride = other.TilePrefabOverride;
		}
		if ((Object)(object)other.Item != (Object)null)
		{
			Item = other.Item;
		}
		if (other.ItemId != 0)
		{
			ItemId = other.ItemId;
		}
	}

	public Sprite GetBestIconForRect(float width, float height)
	{
		float num = width / height;
		bool flag = num > 1.15f;
		bool flag2 = num < 0.8f;
		if (flag)
		{
			return IconOverride;
		}
		if (flag2)
		{
			if ((Object)(object)IconPortraitOverride != (Object)null)
			{
				return IconPortraitOverride;
			}
			return IconOverride;
		}
		if ((Object)(object)IconSquareOverride != (Object)null)
		{
			return IconSquareOverride;
		}
		return IconOverride;
	}

	public string GetBestIconUrlForRect(float width, float height)
	{
		float num = width / height;
		bool flag = num > 1.15f;
		bool flag2 = num < 0.8f;
		if (flag)
		{
			return IconUrl;
		}
		if (flag2)
		{
			if (!string.IsNullOrEmpty(IconPortraitUrl))
			{
				return IconPortraitUrl;
			}
			return IconUrl;
		}
		if (!string.IsNullOrEmpty(IconSquareUrl))
		{
			return IconSquareUrl;
		}
		return IconUrl;
	}
}
