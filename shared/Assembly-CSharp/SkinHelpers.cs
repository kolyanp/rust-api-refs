using System;
using System.Collections.Generic;
using System.Linq;
using Rust.Workshop;
using UnityEngine;

public static class SkinHelpers
{
	public const int RandomSkinId = -1;

	private static Dictionary<int, int> _redirectSkinIdLookup = new Dictionary<int, int>();

	public static bool IsRandom(int skinId)
	{
		return skinId == -1;
	}

	public static void SetSkin(GameObject itemModel, ItemDefinition itemDef, ulong skinID)
	{
		if ((Object)(object)itemDef == (Object)null)
		{
			return;
		}
		ItemSkinDirectory.Skin skin = itemDef.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == skinID);
		if ((ulong)skin.id == skinID)
		{
			ItemSkin itemSkin = skin.invItem as ItemSkin;
			if ((Object)(object)itemSkin != (Object)null)
			{
				itemSkin.ApplySkin(itemModel);
			}
		}
		else if (skinID != 0L)
		{
			WorkshopSkin.Apply(itemModel, skinID, (Action<Skin>)null, (Action)null);
		}
	}

	public static bool TryGetRedirectSkinId(ItemDefinition itemDef, out int skinId)
	{
		skinId = 0;
		ItemDefinition itemDefinition = itemDef?.isRedirectOf;
		if ((Object)(object)itemDefinition != (Object)null)
		{
			if (_redirectSkinIdLookup.TryGetValue(itemDef.itemid, out skinId))
			{
				return true;
			}
			ItemSkinDirectory.Skin[] skins = itemDefinition.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin = skins[i];
				if (skin.invItem is ItemSkin itemSkin && (Object)(object)itemSkin.Redirect == (Object)(object)itemDef)
				{
					skinId = skin.id;
					_redirectSkinIdLookup[itemDef.itemid] = skin.id;
					return true;
				}
			}
		}
		return false;
	}
}
