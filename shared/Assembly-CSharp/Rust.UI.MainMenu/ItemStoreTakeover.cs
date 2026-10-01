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

	[NonSerialized]
	public Sprite IconOverride;

	public DynamicResourceRef<Sprite> DynamicIconOverride;

	[NonSerialized]
	public Sprite IconPortraitOverride;

	public DynamicResourceRef<Sprite> DynamicIconPortraitOverride;

	[NonSerialized]
	public Sprite IconSquareOverride;

	public DynamicResourceRef<Sprite> DynamicIconSquareOverride;

	public string ImageURL;

	public string VideoURL;

	public string IconUrl;

	public string IconPortraitUrl;

	public string IconSquareUrl;

	public string IconBlurHash;

	public string IconPortraitBlurHash;

	public string IconSquareBlurHash;

	[NonSerialized]
	public UI_StoreItemOverlayPage PagePrefab;

	public DynamicResourceRef<UI_StoreItemOverlayPage> DynamicPagePrefab;

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
		DynamicIconOverride = null;
		DynamicIconPortraitOverride = null;
		DynamicIconSquareOverride = null;
		IconUrl = null;
		IconPortraitUrl = null;
		IconSquareUrl = null;
		IconBlurHash = null;
		IconPortraitBlurHash = null;
		IconSquareBlurHash = null;
		PagePrefab = null;
		DynamicPagePrefab = null;
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
		if (other.DynamicIconOverride.IsValid())
		{
			DynamicIconOverride = other.DynamicIconOverride;
		}
		if ((Object)(object)other.IconPortraitOverride != (Object)null)
		{
			IconPortraitOverride = other.IconPortraitOverride;
		}
		if (other.DynamicIconPortraitOverride.IsValid())
		{
			DynamicIconPortraitOverride = other.DynamicIconPortraitOverride;
		}
		if ((Object)(object)other.IconSquareOverride != (Object)null)
		{
			IconSquareOverride = other.IconSquareOverride;
		}
		if (other.DynamicIconSquareOverride.IsValid())
		{
			DynamicIconSquareOverride = other.DynamicIconSquareOverride;
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
			IconBlurHash = other.IconBlurHash;
		}
		if (!string.IsNullOrEmpty(other.IconPortraitUrl))
		{
			IconPortraitUrl = other.IconPortraitUrl;
			IconPortraitBlurHash = other.IconPortraitBlurHash;
		}
		if (!string.IsNullOrEmpty(other.IconSquareUrl))
		{
			IconSquareUrl = other.IconSquareUrl;
			IconSquareBlurHash = other.IconSquareBlurHash;
		}
		if ((Object)(object)other.PagePrefab != (Object)null)
		{
			PagePrefab = other.PagePrefab;
		}
		if (other.DynamicPagePrefab.IsValid())
		{
			DynamicPagePrefab = other.DynamicPagePrefab;
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

	public DynamicResourceRef<Sprite> GetBestIconRefForRect(float width, float height)
	{
		float num = width / height;
		bool flag = num > 1.15f;
		bool flag2 = num < 0.8f;
		if (flag)
		{
			return DynamicIconOverride;
		}
		if (flag2)
		{
			if ((Object)(object)IconPortraitOverride != (Object)null)
			{
				return DynamicIconPortraitOverride;
			}
			return DynamicIconOverride;
		}
		if ((Object)(object)IconSquareOverride != (Object)null)
		{
			return DynamicIconSquareOverride;
		}
		return DynamicIconOverride;
	}

	public string GetBestIconUrlForRect(float width, float height, out string loadingBlurHash)
	{
		float num = width / height;
		bool flag = num > 1.15f;
		bool flag2 = num < 0.8f;
		if (flag)
		{
			loadingBlurHash = IconBlurHash;
			return IconUrl;
		}
		if (flag2)
		{
			if (!string.IsNullOrEmpty(IconPortraitUrl))
			{
				loadingBlurHash = IconPortraitBlurHash;
				return IconPortraitUrl;
			}
			loadingBlurHash = IconBlurHash;
			return IconUrl;
		}
		if (!string.IsNullOrEmpty(IconSquareUrl))
		{
			loadingBlurHash = IconSquareBlurHash;
			return IconSquareUrl;
		}
		loadingBlurHash = IconBlurHash;
		return IconUrl;
	}
}
