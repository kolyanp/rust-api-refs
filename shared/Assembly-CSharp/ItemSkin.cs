using System;
using Rust.Workshop;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skins/ItemSkin")]
public class ItemSkin : SteamInventoryItem
{
	[Serializable]
	public class MaterialRandomSet
	{
		public Material[] Materials;
	}

	public Skinnable Skinnable;

	public Material[] Materials;

	[Tooltip("If set, whenever we make an item with this skin, we'll spawn this item without a skin instead")]
	public ItemDefinition Redirect;

	[Tooltip("(overriden by Redirect) If set, this is the icon that will be used in-game (spray can, crafting menu, repair bench). Allows you to have different icons for the store and in-game (see halloween wallpapers as an example)")]
	public Sprite inGameIcon;

	public bool UnlockedByDefault;

	public MaterialRandomSet[] MaterialRandomisation;

	public void ApplySkin(GameObject obj)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Skinnable == (Object)null)
		{
			return;
		}
		if (MaterialRandomisation != null && MaterialRandomisation.Length != 0)
		{
			using (TimeWarning.New("SkinRandomisation"))
			{
				MaterialRandomSet random = ArrayEx.GetRandom(MaterialRandomisation, SeedEx.Seed(obj.transform.position, World.Seed));
				if (random != null && random.Materials != null && random.Materials.Length != 0)
				{
					Skin.Apply(obj, Skinnable, random.Materials);
					return;
				}
			}
		}
		Skin.Apply(obj, Skinnable, Materials);
	}

	public override bool HasUnlocked(BasePlayer player)
	{
		if (UnlockedByDefault)
		{
			return true;
		}
		if (!player.DefaultSkinAccess)
		{
			return player.AllSkinsUnlocked;
		}
		if ((Object)(object)Redirect != (Object)null && (Object)(object)Redirect.isRedirectOf != (Object)null && (Object)(object)Redirect.isRedirectOf.steamItem != (Object)null && (Object)(object)player != (Object)null && player.blueprints.CheckSkinOwnership(Redirect.isRedirectOf.steamItem.id, player))
		{
			return true;
		}
		return base.HasUnlocked(player);
	}
}
